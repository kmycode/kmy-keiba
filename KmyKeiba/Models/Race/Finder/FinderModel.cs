using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Data.Wrappers;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race.Memo;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.Finder
{
  public class FinderModel
  {
    private readonly RaceFinder _finder;

    public ReactiveProperty<IEnumerable<IFinderColumnDefinition>> Columns { get; } = new();

    public ReactiveCollection<FinderColumnDefinition<FinderRaceItem>> RaceColumns { get; } = new();

    public ReactiveCollection<FinderColumnDefinition<FinderRaceHorseItem>> RaceHorseColumns { get; } = new();

    public ReactiveCollection<FinderColumnDefinition<FinderRaceHorseGroupItem>> RaceHorseGroupColumns { get; } = new();

    public ReactiveCollection<FinderRaceHorseGroupItem> HorseGroups { get; } = new();

    public ReactiveProperty<FinderRaceHorseGroupItem?> CurrentGroup { get; } = new();

    public ReactiveProperty<string> Keys { get; } = new("place>0");

    public ReactiveProperty<bool> IsSearchRaces { get; } = new();

    public ReactiveProperty<bool> IsSearchRaceHorses { get; } = new(true);

    public ReactiveProperty<bool> IsLoading { get; } = new();

    public RaceData? Race => this._finder.Race;

    public RaceHorseAnalyzer? RaceHorse { get; }

    public FinderModel(RaceData? race, RaceHorseAnalyzer? horse)
    {
      this._finder = new RaceFinder(race, horse?.Data);
      this.RaceHorse = horse;

      // TODO いずれカスタマイズできるように
      foreach (var preset in DatabasePresetModel.GetFinderRaceHorseColumns())
      {
        this.RaceHorseColumns.Add(preset.Clone());
      }

      this.CurrentGroup.Subscribe(g =>
      {
        if (g != null && !g.Rows.Any() && g.Items.Any())
        {
          this.UpdateRows(g.Items.Select(i => i.Analyzer).ToFinderRows(this.RaceHorseColumns));
        }
      });
    }

    public void BeginLoad()
    {
      Task.Run(async () =>
      {
        this.IsLoading.Value = true;

        try
        {
          if (this.IsSearchRaces.Value)
          {
            this.Columns.Value = this.RaceColumns;
            var data = await this._finder.FindRacesAsync(this.Keys.Value, 3000, withoutFutureRaces: false);
            this.UpdateRows(data.Items.ToFinderRows(this.RaceColumns));
          }
          else if (this.IsSearchRaceHorses.Value)
          {
            this.Columns.Value = this.RaceHorseColumns;
            var data = await this._finder.FindRaceHorsesAsync(this.Keys.Value, 3000, withoutFutureRaces: false);

            IEnumerable<IGrouping<object, RaceHorseAnalyzer>>? groups = data.GroupKey switch
            {
              QueryKey.RiderName => data.Items.GroupBy(d => d.Data.RiderName),
              QueryKey.RiderCode => data.Items.GroupBy(d => d.Data.RiderCode),
              QueryKey.TrainerName => data.Items.GroupBy(d => d.Data.TrainerName),
              QueryKey.TrainerCode => data.Items.GroupBy(d => d.Data.TrainerCode),
              QueryKey.OwnerName => data.Items.GroupBy(d => d.Data.OwnerName),
              QueryKey.OwnerCode => data.Items.GroupBy(d => d.Data.OwnerCode),
              QueryKey.Course => data.Items.GroupBy(d => (object)d.Race.Course),
              QueryKey.Weather => data.Items.GroupBy(d => (object)d.Race.TrackWeather),
              QueryKey.Condition => data.Items.GroupBy(d => (object)d.Race.TrackCondition),
              QueryKey.HorseKey => data.Items.GroupBy(d => d.Data.Key),
              QueryKey.HorseName => data.Items.GroupBy(d => d.Data.Name),
              QueryKey.FrameNumber => data.Items.GroupBy(d => d.Data.FrameNumber.ToString()),
              QueryKey.HorseNumber => data.Items.GroupBy(d => d.Data.Number.ToString()),
              _ => null,
            };
            groups?.OrderBy(g => g.Key);

            IEnumerable<IEnumerable<RaceHorseAnalyzer>> SplitData(int size)
            {
              if (data == null)
              {
                return Enumerable.Empty<IEnumerable<RaceHorseAnalyzer>>();
              }

              var lists = new List<List<RaceHorseAnalyzer>>();
              for (var i = 0; i < size; i++)
              {
                lists.Add(new());
              }
              for (var i = 0; i < data.Items.Count; i++)
              {
                lists[i % size].Add(data.Items[i]);
              }

              return lists;
            }

            // ラベルでグループ化
            if (data.GroupInfo != null)
            {
              // メモのポイントでグループ化する処理
              var list = new List<(RaceHorseAnalyzer, short)>();

              ExpansionMemoConfig? memoConfig = null;
              PointLabelData? labelConfig = null;
              IReadOnlyList<PointLabelItem>? labelItems = null;

              var tasks = SplitData(3).Select(items =>
              {
                return Task.Run(async () =>
                {
                  using var db = new MyContext();

                  foreach (var item in items)
                  {
                    if (memoConfig == null)
                    {
                      memoConfig = MemoUtil.GetMemoConfig(data.GroupInfo.Target1, data.GroupInfo.Target2, data.GroupInfo.Target3, data.GroupInfo.MemoNumber);
                      if (memoConfig == null)
                      {
                        break;
                      }
                      labelConfig = MemoUtil.GetPointLabelConfig(memoConfig);
                      labelItems = labelConfig?.GetItems();
                    }

                    var memo = await MemoUtil.GetMemoAsync(db, item.Race, memoConfig, item, false);
                    if (memo != null)
                    {
                      list.Add((item, memo.Point));
                    }
                  }
                });
              }).ToArray();
              await Task.WhenAll(tasks);

              if (labelItems == null)
              {
                groups = list.OrderBy(i => i.Item2).GroupBy(i => (object)i.Item2, i => i.Item1).ToArray();
              }
              else
              {
                groups = list.OrderBy(i => i.Item2).GroupBy(i => (object)(labelItems.FirstOrDefault(l => l.Point == i.Item2) ?? (object)string.Empty), i => i.Item1).ToArray();
              }
            }

            // リスト全体をグループ化
            if (groups != null)
            {
              groups = groups.Take(100);

              IReadOnlyList<FinderRaceHorseGroupItem> g;
              if (data.GroupKey == QueryKey.RiderCode)
              {
                g = groups.Select(g => new FinderRaceHorseGroupItem(g.First().Data.RiderName, g)).ToArray();
              }
              else if (data.GroupKey == QueryKey.TrainerCode)
              {
                g = groups.Select(g => new FinderRaceHorseGroupItem(g.First().Data.TrainerName, g)).ToArray();
              }
              else if (data.GroupKey == QueryKey.OwnerCode)
              {
                g = groups.Select(g => new FinderRaceHorseGroupItem(g.First().Data.OwnerName, g)).ToArray();
              }
              else if (data.GroupKey == QueryKey.Course)
              {
                g = groups.Select(g => new FinderRaceHorseGroupItem(g.First().Race.Course.GetName() ?? string.Empty, g)).ToArray();
              }
              else if (data.GroupKey == QueryKey.Weather)
              {
                g = groups.Select(g => new FinderRaceHorseGroupItem(g.First().Race.TrackWeather.ToString() ?? string.Empty, g)).ToArray();
              }
              else if (data.GroupKey == QueryKey.Condition)
              {
                g = groups.Select(g => new FinderRaceHorseGroupItem(g.First().Race.TrackCondition.ToString() ?? string.Empty, g)).ToArray();
              }
              else if (data.GroupKey == QueryKey.HorseKey)
              {
                g = groups.Select(g => new FinderRaceHorseGroupItem(g.First().Data.Name, g)).ToArray();
              }
              else
              {
                g = groups.Select(g => new FinderRaceHorseGroupItem(g)).ToArray();
              }
              this.UpdateGroups(g);
            }
            else
            {
              var group = new FinderRaceHorseGroupItem(data.Items);
              this.UpdateGroups(new[] { group, });
            }
          }
        }
        catch (Exception ex)
        {

        }
        finally
        {
          this.IsLoading.Value = false;
        }
      });
    }

    private void UpdateGroups(IEnumerable<FinderRaceHorseGroupItem> groups)
    {
      ThreadUtil.InvokeOnUiThread(() =>
      {
        this.HorseGroups.Clear();
        foreach (var group in groups)
        {
          this.HorseGroups.Add(group);
        }
        if (groups.Any())
        {
          this.CurrentGroup.Value = groups.First();
        }
      });
    }

    private void UpdateRows(IEnumerable<FinderRow> rows)
    {
      var group = this.CurrentGroup.Value;
      if (group == null)
      {
        return;
      }

      ThreadUtil.InvokeOnUiThread(() =>
      {
        group.Rows.Clear();
        foreach (var row in rows)
        {
          group.Rows.Add(row);
        }
      });
    }
  }

  public class FinderRow
  {
    public IReadOnlyList<FinderCell> Cells { get; }

    public RaceAnalyzer? Race { get; }

    public RaceHorseAnalyzer? RaceHorse { get; }

    public FinderRow(IReadOnlyList<FinderCell> cells, RaceAnalyzer? race, RaceHorseAnalyzer? horse)
    {
      this.Cells = cells;
      this.Race = race;
      this.RaceHorse = horse;
    }
  }
}
