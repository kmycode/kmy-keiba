using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Data.Wrappers;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Analysis.Generic;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KmyKeiba.Models.Race.Finder
{
  public class FinderModel : IDisposable
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    private readonly RaceFinder _finder;
    private readonly CompositeDisposable _disposables = new();

    private static ReactiveProperty<int> ActiveTabId { get; } = new();

    public FinderQueryInput Input { get; }

    public ReactiveProperty<IEnumerable<IFinderColumnDefinition>> Columns { get; } = new();

    public ReactiveCollection<FinderColumnDefinition<FinderRaceHorseItem>> RaceHorseColumns { get; } = new();

    public ReactiveCollection<FinderColumnDefinition<FinderRaceHorseGroupItem>> RaceHorseGroupColumns { get; } = new();

    public ReactiveCollection<FinderRaceHorseGroupItem> HorseGroups { get; } = new();

    public ReactiveProperty<FinderRaceHorseGroupItem?> CurrentGroup { get; } = new();

    public CheckableCollection<FinderTab> Tabs { get; } = new();

    public ReactiveProperty<string> Keys => this.Input.Query;

    public ReactiveProperty<bool> IsLoading { get; } = new();

    public ReactiveProperty<bool> IsError { get; } = new();

    public ReactiveProperty<bool> IsCanceled { get; } = new();

    public ReactiveProperty<bool> CanCancel { get; } = new();

    public RaceData? Race => this._finder.Race;

    public RaceHorseAnalyzer? RaceHorse { get; }

    public bool HasRace => this.Race != null;

    public bool HasRaceHorse => this.RaceHorse != null;

    private CancellationTokenSource? _cancellationTokenSource;

    public int DefaultSize
    {
      get => this.Input.OtherSetting.DefaultSize;
      init => this.Input.OtherSetting.DefaultSize = value;
    }

    public FinderModel(RaceData? race, RaceHorseAnalyzer? horse, IEnumerable<RaceHorseAnalyzer>? horses)
    {
      this._finder = new RaceFinder(race, horse?.Data);
      this.RaceHorse = horse;
      this.Input = new FinderQueryInput(race, horse?.Data, horse, horses?.Select(h => h.Data).ToArray());

      this.CurrentGroup.Subscribe(g =>
      {
        if (g != null && !g.Rows.Any() && g.Items.Any())
        {
          this.UpdateRows(g.Items.ToFinderRows(this.RaceHorseColumns));
        }
      }).AddTo(this._disposables);
    }

    private void UpdateColumnConfigs()
    {
      this.RaceHorseColumns.Clear();
      this.Tabs.Clear();

      foreach (var preset in FinderColumnConfigUtil.GenerateRaceHorseColumnList())
      {
        this.RaceHorseColumns.Add(preset);
      }

      if (this.RaceHorseColumns.Any())
      {
        var maxTab = this.RaceHorseColumns.Max(c => c.Tab);
        if (maxTab >= 1)
        {
          for (var i = 1; i <= maxTab; i++)
          {
            this.Tabs.Add(new FinderTab
            {
              TabId = i,
            });
          }
          this.SwitchActiveTab();
        }

        this.Tabs.ActiveItem.Subscribe(_ => this.OnTabChanged()).AddTo(this._disposables);
      }
    }

    public void BeginLoad()
    {
      this.UpdateColumnConfigs();

      Task.Run(async () =>
      {
        this.IsError.Value = false;
        this.IsCanceled.Value = false;
        this.CanCancel.Value = true;
        this.IsLoading.Value = true;

        this._cancellationTokenSource = new();

        try
        {
          this.Columns.Value = this.RaceHorseColumns;
          var data = await ((IRaceFinder)this._finder).FindRaceHorsesAsync(this.Keys.Value, 3000, cancellationToken: this._cancellationTokenSource.Token);
          data.AddTo(this._disposables);

          this.CanCancel.Value = false;
          this._cancellationTokenSource = null;

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
              QueryKey.Distance => allItems.GroupBy(d => (object)d.Analyzer.Race.Distance),
              QueryKey.Direction => allItems.GroupBy(d => (object)d.Analyzer.Race.TrackCornerDirection),
              QueryKey.Weather => allItems.GroupBy(d => (object)d.Analyzer.Race.TrackWeather),
              QueryKey.Condition => allItems.GroupBy(d => (object)d.Analyzer.Race.TrackCondition),
              QueryKey.Grade => allItems.GroupBy(d => (object)d.Analyzer.Race.Grade),
              QueryKey.HorseKey => allItems.GroupBy(d => d.Analyzer.Data.Key),
              QueryKey.HorseName => allItems.GroupBy(d => d.Analyzer.Data.Name),
              QueryKey.FrameNumber => allItems.GroupBy(d => d.Analyzer.Data.FrameNumber.ToString()),
              QueryKey.HorseNumber => allItems.GroupBy(d => d.Analyzer.Data.Number.ToString()),
              QueryKey.Sex => allItems.GroupBy(d => (object)d.Analyzer.Data.Sex),
              QueryKey.Popular => allItems.GroupBy(d => (object)d.Analyzer.Data.Popular),
              QueryKey.Place => allItems.GroupBy(d => (object)d.Analyzer.Data.ResultOrder),
              QueryKey.RunningStyle => allItems.GroupBy(d => (object)d.Analyzer.Data.RunningStyle),
              QueryKey.Ground => allItems.GroupBy(d => (object)d.Analyzer.Race.TrackGround),
              QueryKey.TrackType => allItems.GroupBy(d => (object)d.Analyzer.Race.TrackType),
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

            // 血統でグループ化
            if (data.GroupKey == QueryKey.Father || data.GroupKey == QueryKey.Mother || data.GroupKey == QueryKey.MotherFather)
            {
              using var db = new MyContext();
              var horseKeys = allItems.Select(i => i.Analyzer.Data.Key).Distinct().ToArray();
              var parents1 = db.BornHorses!.Where(h => horseKeys.Contains(h.Code));
              var parents2 =
                    data.GroupKey == QueryKey.Father ? parents1.Select(h => new { Key = h.Code, Parent = h.FatherBreedingCode, }) :
                    data.GroupKey == QueryKey.Mother ? parents1.Select(h => new { Key = h.Code, Parent = h.MotherBreedingCode, }) :
                                                       parents1.Select(h => new { Key = h.Code, Parent = h.MFBreedingCode, });
              var parents = await parents2.ToArrayAsync();

              var list = new List<IGrouping<string, FinderRaceHorseItem>>();
              foreach (var d in parents.GroupBy(p => p.Parent))
              {
                var parentName = await db.HorseBloods!.Where(b => b.Key == d.Key).Select(b => b.Name).FirstOrDefaultAsync() ?? "不明";
                var items = allItems.Where(i => d.Select(s => s.Key).Contains(i.Analyzer.Data.Key));
                list.Add(new Grouping<string, FinderRaceHorseItem>(parentName, items));
              }

              groups = list;
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

              if (data.IsExpandedResult)
              {
                this.UpdateExpandedData(g);
              }
              this.UpdateGroups(g);
            }
            else
            {
              var group = new FinderRaceHorseGroupItem(allItems);
              var gs = new[] { group, };

              if (data.IsExpandedResult)
              {
                this.UpdateExpandedData(gs);
              }
              this.UpdateGroups(gs);
            }
          }

          this.OnTabChanged();
        }
        catch (TaskCanceledException ex)
        {
          logger.Error("検索がキャンセルされました", ex);
          this.CanCancel.Value = false;
          this.IsCanceled.Value = true;
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

    public void CancelLoad()
    {
      this._cancellationTokenSource?.Cancel();
      this.CanCancel.Value = false;
    }

    internal Task<FinderQueryResult<RaceAnalyzer>> FindRacesAsync(string keys, int count, int offset)
    {
      return this._finder.FindRacesAsync(keys, count, offset);
    }

    internal Task<RaceHorseFinderQueryResult> FindRaceHorsesAsync(string keys, int count, int offset)
    {
      return ((IRaceFinder)this._finder).FindRaceHorsesAsync(keys, count, offset);
    }

    private void UpdateExpandedData(IEnumerable<FinderRaceHorseGroupItem> groups)
    {
      foreach (var group in groups)
      {
        group.ExpandedData = new FinderRaceHorseGroupExpandedData(group.Items);
      }
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

    public void OnRendered()
    {
      this.SwitchActiveTab();
    }

    private void SwitchActiveTab()
    {
      if (this.Tabs.Count == 0) return;

      var tab = this.Tabs.FirstOrDefault(t => t.TabId == ActiveTabId.Value) ?? this.Tabs.First();

      this.SwitchTab(tab);
    }

    private void SwitchTab(int tabId)
    {
      var tab = this.Tabs.FirstOrDefault(t => t.TabId == tabId);
      if (tab != null)
      {
        this.SwitchTab(tab);
      }
    }

    private void SwitchTab(FinderTab tab)
    {
      if (!tab.IsChecked.Value)
      {
        tab.IsChecked.Value = true;
        this.OnTabChanged();
      }
    }

    private void OnTabChanged()
    {
      var id = this.Tabs.ActiveItem.Value?.TabId ?? 1;
      ActiveTabId.Value = id;

      if (this.Columns.Value == null)
      {
        return;
      }

      foreach (var column in this.Columns.Value)
      {
        column.IsVisible.Value = column.Tab == id;
      }
    }

    public void ReplaceFrom(FinderModel model)
    {
      this._finder.CopyFrom(model._finder);
    }

    public void ReplaceFrom(RaceFinder finder)
    {
      this._finder.CopyFrom(finder);
    }

    public void ClearCache()
    {
      this._finder.ClearCache();
    }

    public void Dispose()
    {
      this.Input.Dispose();
      this.Tabs.Dispose();
      this._finder.Dispose();
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
