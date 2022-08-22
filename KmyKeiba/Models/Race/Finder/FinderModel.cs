using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Data.Wrappers;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race.Memo;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KmyKeiba.Models.Race.Finder
{
  public class FinderModel : IDisposable
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    private readonly RaceFinder _finder;
    private readonly CompositeDisposable _disposables = new();

    public FinderQueryInput Input { get; }

    public ReactiveProperty<IEnumerable<IFinderColumnDefinition>> Columns { get; } = new();

    public ReactiveCollection<FinderColumnDefinition<FinderRaceHorseItem>> RaceHorseColumns { get; } = new();

    public ReactiveCollection<FinderColumnDefinition<FinderRaceHorseGroupItem>> RaceHorseGroupColumns { get; } = new();

    public ReactiveCollection<FinderRaceHorseGroupItem> HorseGroups { get; } = new();

    public ReactiveProperty<FinderRaceHorseGroupItem?> CurrentGroup { get; } = new();

    public ReactiveProperty<string> Keys => this.Input.Query;

    public ReactiveProperty<bool> IsLoading { get; } = new();

    public ReactiveProperty<bool> IsError { get; } = new();

    public RaceData? Race => this._finder.Race;

    public RaceHorseAnalyzer? RaceHorse { get; }

    public bool HasRace => this.Race != null;

    public bool HasRaceHorse => this.RaceHorse != null;

    public FinderModel(RaceData? race, RaceHorseAnalyzer? horse, IEnumerable<RaceHorseAnalyzer>? horses)
    {
      this._finder = new RaceFinder(race, horse?.Data);
      this.RaceHorse = horse;
      this.Input = new FinderQueryInput(race, horse?.Data, horses?.Select(h => h.Data).ToArray());

      // TODO いずれカスタマイズできるように
      foreach (var preset in DatabasePresetModel.GetFinderRaceHorseColumns())
      {
        this.RaceHorseColumns.Add(preset.Clone());
      }

      this.CurrentGroup.Subscribe(g =>
      {
        if (g != null && !g.Rows.Any() && g.Items.Any())
        {
          this.UpdateRows(g.Items.ToFinderRows(this.RaceHorseColumns));
        }
      });
    }

    public void BeginLoad()
    {
      Task.Run(async () =>
      {
        this.IsError.Value = false;
        this.IsLoading.Value = true;

        try
        {
          this.Columns.Value = this.RaceHorseColumns;
          var data = await this._finder.FindRaceHorsesAsync(this.Keys.Value, 3000);
          var allItems = data.AsItems();

          IEnumerable<IEnumerable<FinderRaceHorseItem>> SplitData(int size)
          {
            if (data == null)
            {
              return Enumerable.Empty<IEnumerable<FinderRaceHorseItem>>();
            }

            var lists = new List<List<FinderRaceHorseItem>>();
            for (var i = 0; i < size; i++)
            {
              lists.Add(new());
            }
            for (var i = 0; i < allItems.Count; i++)
            {
              lists[i % size].Add(allItems[i]);
            }

            return lists;
          }

          // TODO: グループのネスト
          {
            IEnumerable<IGrouping<object, FinderRaceHorseItem>>? groups = data.GroupKey switch
            {
              QueryKey.RiderName => allItems.GroupBy(d => d.Analyzer.Data.RiderName),
              QueryKey.RiderCode => allItems.GroupBy(d => d.Analyzer.Data.RiderCode),
              QueryKey.TrainerName => allItems.GroupBy(d => d.Analyzer.Data.TrainerName),
              QueryKey.TrainerCode => allItems.GroupBy(d => d.Analyzer.Data.TrainerCode),
              QueryKey.OwnerName => allItems.GroupBy(d => d.Analyzer.Data.OwnerName),
              QueryKey.OwnerCode => allItems.GroupBy(d => d.Analyzer.Data.OwnerCode),
              QueryKey.Course => allItems.GroupBy(d => (object)d.Analyzer.Data.Course),
              QueryKey.Weather => allItems.GroupBy(d => (object)d.Analyzer.Race.TrackWeather),
              QueryKey.Condition => allItems.GroupBy(d => (object)d.Analyzer.Race.TrackCondition),
              QueryKey.Grade => allItems.GroupBy(d => (object)d.Analyzer.Race.Grade),
              QueryKey.HorseKey => allItems.GroupBy(d => d.Analyzer.Data.Key),
              QueryKey.HorseName => allItems.GroupBy(d => d.Analyzer.Data.Name),
              QueryKey.FrameNumber => allItems.GroupBy(d => d.Analyzer.Data.FrameNumber.ToString()),
              QueryKey.HorseNumber => allItems.GroupBy(d => d.Analyzer.Data.Number.ToString()),
              QueryKey.Sex => allItems.GroupBy(d => (object)d.Analyzer.Data.Sex),
              _ => null,
            };

            if (data.GroupKey == QueryKey.HorseBelongs)
            {
              using var db = new MyContext();

              var keys = allItems.Select(i => i.Analyzer.Data.Key);
              var horses = await db.Horses!.Where(h => keys.Contains(h.Code)).ToArrayAsync();
              var items = allItems.GroupJoin(horses, i => i.Analyzer.Data.Key, h => h.Code, (i, h) =>
              {
                if (h.Count() == 1)
                {
                  return new { Item = i, Belongs = h.First().Belongs, };
                }
                if (h.Count() == 0)
                {
                  return new { Item = i, Belongs = HorseBelongs.Unknown, };
                }
                var central = h.FirstOrDefault(d => d.Belongs != HorseBelongs.Local);
                var local = h.FirstOrDefault(d => d.Belongs == HorseBelongs.Local);
                if (central == null && local == null)
                {
                  return new { Item = i, Belongs = HorseBelongs.Unknown, };
                }
                if ((central != null && local == null) || (local != null && central == null))
                {
                  return new { Item = i, Belongs = (central ?? local)!.Belongs, };
                }

                if (central!.Retired != default)
                {
                  return new { Item = i, Belongs = central.Retired >= i.Analyzer.Race.StartTime ? central.Belongs : local!.Belongs, };
                }
                if (local!.Retired != default)
                {
                  return new { Item = i, Belongs = local.Retired >= i.Analyzer.Race.StartTime ? local.Belongs : central!.Belongs, };
                }
                return new { Item = i, Belongs = h.First().Belongs, };
              });
              groups = items.GroupBy(i => { return (object)(i.Belongs switch { HorseBelongs.Ritto => "栗東", HorseBelongs.Miho => "美浦", HorseBelongs.Local => "地方", HorseBelongs.Foreign => "海外", _ => "データなし", }); }, i => i.Item);
            }

            // ラベルでグループ化
            if (data.GroupInfo != null)
            {
              // メモのポイントでグループ化する処理
              var list = new List<(FinderRaceHorseItem, short)>();

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

                    var memo = await MemoUtil.GetMemoAsync(db, item.Analyzer.Race, memoConfig, item.Analyzer, false);
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
              groups = groups.OrderByDescending(g => g.Count()).Take(100);

              IReadOnlyList<FinderRaceHorseGroupItem> g;
              if (data.GroupKey == QueryKey.RiderCode)
              {
                g = groups.Select(g => new FinderRaceHorseGroupItem(g.Key.ToString()?.All(c => c == '0') == true ? "分類不可" : g.First().Analyzer.Data.RiderName, g)).ToArray();
              }
              else if (data.GroupKey == QueryKey.TrainerCode)
              {
                g = groups.Select(g => new FinderRaceHorseGroupItem(g.Key.ToString()?.All(c => c == '0') == true ? "分類不可" : g.First().Analyzer.Data.TrainerName, g)).ToArray();
              }
              else if (data.GroupKey == QueryKey.OwnerCode)
              {
                g = groups.Select(g => new FinderRaceHorseGroupItem(g.Key.ToString()?.All(c => c == '0') == true ? "分類不可" : g.First().Analyzer.Data.OwnerName, g)).ToArray();
              }
              else if (data.GroupKey == QueryKey.Course)
              {
                g = groups.Select(g => new FinderRaceHorseGroupItem(g.First().Analyzer.Race.Course.GetName() ?? string.Empty, g)).ToArray();
              }
              else if (data.GroupKey == QueryKey.Weather)
              {
                g = groups.Select(g => new FinderRaceHorseGroupItem(g.First().Analyzer.Race.TrackWeather.ToString() ?? string.Empty, g)).ToArray();
              }
              else if (data.GroupKey == QueryKey.Condition)
              {
                g = groups.Select(g => new FinderRaceHorseGroupItem(g.First().Analyzer.Race.TrackCondition.ToString() ?? string.Empty, g)).ToArray();
              }
              else if (data.GroupKey == QueryKey.Grade)
              {
                g = groups.Select(g => new FinderRaceHorseGroupItem(g.First().Analyzer.Race.Grade.GetLabel() ?? string.Empty, g)).ToArray();
              }
              else if (data.GroupKey == QueryKey.HorseKey)
              {
                g = groups.Select(g => new FinderRaceHorseGroupItem(g.Key.ToString()?.All(c => c == '0') == true ? "分類不可" : g.First().Analyzer.Data.Name, g)).ToArray();
              }
              else
              {
                g = groups.Select(g => new FinderRaceHorseGroupItem(g)).ToArray();
              }
              this.UpdateGroups(g);
            }
            else
            {
              var group = new FinderRaceHorseGroupItem(allItems);
              this.UpdateGroups(new[] { group, });
            }
          }
        }
        catch (Exception ex)
        {
          logger.Error("検索でエラーが発生しました", ex);
          this.IsError.Value = true;
        }
        finally
        {
          this.IsLoading.Value = false;
        }
      });
    }

    internal Task<FinderQueryResult<RaceAnalyzer>> FindRacesAsync(string keys, int count, int offset)
    {
      return this._finder.FindRacesAsync(keys, count, offset);
    }

    internal Task<RaceHorseFinderQueryResult> FindRaceHorsesAsync(string keys, int count, int offset)
    {
      return this._finder.FindRaceHorsesAsync(keys, count, offset);
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

    public void ReplaceFrom(FinderModel model)
    {
      this._finder.ReplaceFrom(model._finder);
    }

    public void ReplaceFrom(RaceFinder finder)
    {
      this._finder.ReplaceFrom(finder);
    }

    public RaceHorseTrendAnalysisSelectorWrapper AsTrendAnalysisSelector()
    {
      return this._finder.AsTrendAnalysisSelector();
    }

    public void Dispose()
    {
      // なぜかUIスレッドでないと、前走／同レース他馬を検索条件に追加してからレースを切り替えるとエラーになる
      // FinderModel.Disposeメソッドが悪さをしている様子だがReactiveCollectionの変更ロジックはもちろん
      // Disposeの中に含まれていないため、原因不明
      ThreadUtil.InvokeOnUiThread(() =>
      {
        //this.Input.Dispose();
      });
      this.Input.Dispose();
      this._disposables.Dispose();
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
