using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Data.Wrappers;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Analysis.Generic;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using Reactive.Bindings.Disposables;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace KmyKeiba.Models.Race.Finder
{
  public static class FinderColumnConfigUtil
  {
    private static bool _isInitialized;

    public static CheckableCollection<FinderColumnTabItem> Tabs { get; } = new();

    public static async Task InitializeAsync(MyContext db)
    {
      if (_isInitialized) return;

      var tabs = new List<FinderColumnTabItem>();
      var columns = await db.FinderColumns!.ToArrayAsync();

      foreach (var tabColumns in columns.OrderBy(c => c.Order).GroupBy(c => c.TabGroup).OrderBy(t => t.Key))
      {
        var items = tabColumns.Select(c => new FinderColumnItem(c));
        tabs.Add(new FinderColumnTabItem((int)tabColumns.Key, items));
      }

      NormalizeTabGroupIds(tabs);
      await db.SaveChangesAsync();

      if (tabs.Count > 0)
      {
        tabs[0].IsChecked.Value = true;
      }

      ThreadUtil.InvokeOnUiThread(() =>
      {
        foreach (var tab in tabs)
        {
          Tabs.Add(tab);
        }
        FinderColumnConfigModel.Instance.Initialize();
      });

      _isInitialized = true;
    }

    private static void NormalizeTabGroupIds(IList<FinderColumnTabItem>? tabs = null)
    {
      tabs ??= Tabs;

      if (tabs.Count == 0) return;

      var isResetList = tabs.Select((t, i) => t.TabId.Value == i + 1).Any(b => !b);

      var oldTabs = (IEnumerable<FinderColumnTabItem>)tabs;
      if (isResetList)
      {
        oldTabs = tabs.ToArray();
        tabs.Clear();
      }

      var tabId = 1;
      foreach (var tab in oldTabs.OrderBy(t => t.TabId.Value))
      {
        tab.TabId.Value = tabId++;

        if (isResetList)
        {
          tabs.Add(tab);
        }
      }
    }

    public static FinderColumnTabItem AddTab()
    {
      var tabId = Tabs.Count == 0 ? 1 : Tabs.Max(t => t.TabId.Value) + 1;
      var tab = new FinderColumnTabItem(tabId);
      Tabs.Add(tab);

      return tab;
    }

    public static async Task RemoveTabAsync(MyContext db, int tabId)
    {
      var tab = Tabs.FirstOrDefault(t => t.TabId.Value == tabId);
      if (tab == null) return;

      db.FinderColumns!.AttachRange(Tabs.SelectMany(t => t.Columns).Select(c => c.Data));

      Tabs.Remove(tab);
      NormalizeTabGroupIds();

      db.FinderColumns!.RemoveRange(tab.Columns.Select(t => t.Data));

      await db.SaveChangesAsync();

      tab.Dispose();
    }

    public static async Task UpTabAsync(MyContext db, int tabId)
    {
      var tab = Tabs.FirstOrDefault(t => t.TabId.Value == tabId);
      if (tab == null) return;

      var tabIndex = Tabs.IndexOf(tab);
      if (tabIndex <= 0) return;

      var targetTab = Tabs[tabIndex - 1];

      Tabs.Move(tabIndex, tabIndex - 1);

      db.FinderColumns!.AttachRange(tab.Columns.Concat(targetTab.Columns).Select(c => c.Data));
      tab.TabId.Value = targetTab.TabId.Value;
      targetTab.TabId.Value = tabId;

      await db.SaveChangesAsync();
    }

    public static async Task DownTabAsync(MyContext db, int tabId)
    {
      var tab = Tabs.FirstOrDefault(t => t.TabId.Value == tabId);
      if (tab == null) return;

      var tabIndex = Tabs.IndexOf(tab);
      if (tabIndex >= Tabs.Count - 1) return;

      var targetTab = Tabs[tabIndex + 1];

      Tabs.Move(tabIndex, tabIndex + 1);

      db.FinderColumns!.AttachRange(tab.Columns.Concat(targetTab.Columns).Select(c => c.Data));
      tab.TabId.Value = targetTab.TabId.Value;
      targetTab.TabId.Value = tabId;

      await db.SaveChangesAsync();
    }

    public static async Task AddColumnAsync(MyContext db, FinderColumnData data)
    {
      var tab = Tabs.FirstOrDefault(t => t.TabId.Value == data.TabGroup);
      if (tab == null)
      {
        tab = AddTab();
      }

      var order = tab.Columns.Count == 0 ? 1 : tab.Columns.Max(c => c.Data.Order) + 1;
      data.Order = order;

      await db.FinderColumns!.AddAsync(data);
      await db.SaveChangesAsync();

      tab.Columns.Add(new FinderColumnItem(data));
    }

    public static async Task RemoveColumnAsync(MyContext db, FinderColumnData data)
    {
      var tab = Tabs.FirstOrDefault(t => t.TabId.Value == data.TabGroup);
      if (tab == null) return;

      var column = tab.Columns.FirstOrDefault(c => c.Data.Id == data.Id);
      if (column == null) return;

      db.FinderColumns!.Remove(column.Data);
      await db.SaveChangesAsync();

      tab.Columns.Remove(column);

      column.Dispose();
    }

    public static async Task UpColumnAsync(MyContext db, FinderColumnData data)
    {
      var tab = Tabs.FirstOrDefault(t => t.TabId.Value == data.TabGroup);
      if (tab == null) return;

      var column = tab.Columns.FirstOrDefault(c => c.Data.Id == data.Id);
      if (column == null) return;

      var columnIndex = tab.Columns.IndexOf(column);
      if (columnIndex <= 0) return;

      var targetColumn = tab.Columns[columnIndex - 1];

      tab.Columns.Move(columnIndex, columnIndex - 1);

      db.FinderColumns!.Attach(column.Data);
      db.FinderColumns!.Attach(targetColumn.Data);

      (column.Data.Order, targetColumn.Data.Order) = (targetColumn.Data.Order, column.Data.Order);
      await db.SaveChangesAsync();
    }

    public static async Task DownColumnAsync(MyContext db, FinderColumnData data)
    {
      var tab = Tabs.FirstOrDefault(t => t.TabId.Value == data.TabGroup);
      if (tab == null) return;

      var column = tab.Columns.FirstOrDefault(c => c.Data.Id == data.Id);
      if (column == null) return;

      var columnIndex = tab.Columns.IndexOf(column);
      if (columnIndex >= tab.Columns.Count - 1) return;

      var targetColumn = tab.Columns[columnIndex + 1];

      tab.Columns.Move(columnIndex, columnIndex + 1);

      db.FinderColumns!.Attach(column.Data);
      db.FinderColumns!.Attach(targetColumn.Data);

      (column.Data.Order, targetColumn.Data.Order) = (targetColumn.Data.Order, column.Data.Order);
      await db.SaveChangesAsync();
    }

    public static IReadOnlyList<FinderColumnDefinition<FinderRaceHorseItem>> GenerateRaceHorseColumnList()
    {
      var definitions = new List<FinderColumnDefinition<FinderRaceHorseItem>>();

      foreach (var tab in Tabs)
      {
        foreach (var definition in tab.Columns.OrderBy(c => c.Data.Order)
                                              .Join(RaceHorseColumnBuilders, c => c.Property.Value, b => b.Property, (c, b) => b)
                                              .Select(b => b.Build(tab.TabId.Value)))
        {
          definitions.Add(definition);
        }
      }

      return definitions;
    }

    public static readonly ReadOnlyCollection<FinderColumnDefinitionBuilder<FinderRaceHorseItem>> RaceHorseColumnBuilders = new(new List<FinderColumnDefinitionBuilder<FinderRaceHorseItem>>
    {
      new(FinderColumnProperty.Empty, FinderColumnType.Text, 40, "未設", "(設定されていません)", h => string.Empty),

      new(FinderColumnProperty.RaceSubject, FinderColumnType.RaceSubject, 50, string.Empty,"レースのクラス", h => h.Analyzer.Subject.Subject),
      new(FinderColumnProperty.RaceName, FinderColumnType.BoldText, 200, "レース名", h => h.Analyzer.Subject.DisplayName),
      new(FinderColumnProperty.StartTime, FinderColumnType.Text, 70, "日付", h => h.Analyzer.Race.StartTime.ToString("yy/MM/dd")),
      new(FinderColumnProperty.Course, FinderColumnType.Text, 50, "場所", "競馬場の名前", h => h.Analyzer.Race.Course.GetName()),
      new(FinderColumnProperty.CourseInfo, FinderColumnType.CourseInfo, 70, "コース", "レースのコース (芝ダ/向き/距離)", h => h.Analyzer.Race),
      new(FinderColumnProperty.HorsesCount, FinderColumnType.NumericText, 40, "数", "登録された馬の数", h => h.Analyzer.Race.HorsesCount),
      new(FinderColumnProperty.Weather, FinderColumnType.RaceCourseWeather, 36, "天気", h => h.Analyzer.Race.TrackWeather),
      new(FinderColumnProperty.Condition, FinderColumnType.RaceCourseCondition, 36, "馬場", "馬場 (良/稍重/重/不良)", h => h.Analyzer.Race.TrackCondition),

      new(FinderColumnProperty.HorseName, FinderColumnType.HorseName, 120, "馬名", h => new { h.Analyzer.Data.Name, h.Analyzer.Memo }),
      new(FinderColumnProperty.Popular, FinderColumnType.NumericTextWithoutZero, 30, "人", "人気", h => h.Analyzer.Data.Popular, (h, v) => AnalysisUtil.CompareValue((short)v, h.Analyzer.Race.HorsesCount < 7 ? 2 : 3, 6, true)),
      new(FinderColumnProperty.Weight, FinderColumnType.NumericTextWithoutZero, 40, "体重", h => h.Analyzer.Data.Weight),
      new(FinderColumnProperty.WeightDiff, FinderColumnType.NumericText, 30, "重変", "体重変化", h => h.Analyzer.Data.Weight == 0 ? (short)-999 : h.Analyzer.Data.WeightDiff, v => (v is short iv && iv == -999) ? string.Empty : v, (h, v) => AnalysisUtil.CompareValue((short)v, 3, -3, true)),
      new(FinderColumnProperty.RiderWeight, FinderColumnType.NumericText, 30, "斤量", h => h.Analyzer.Data.RiderWeight / 10.0),
      new(FinderColumnProperty.ResultOrder, FinderColumnType.NumericTextWithoutZero, 30, "着", "着順", h => h.Analyzer.Data.ResultOrder, (h, v) => AnalysisUtil.CompareValue((short)v, h.Analyzer.Race.HorsesCount < 7 ? 2 : 3, 6, true)),
      new(FinderColumnProperty.GoalOrder, FinderColumnType.NumericTextWithoutZero, 30, "入", "入線順位", h => h.Analyzer.Data.GoalOrder, (h, v) => AnalysisUtil.CompareValue((short)v, h.Analyzer.Race.HorsesCount < 7 ? 2 : 3, 6, true)),
      new(FinderColumnProperty.ResultTime, FinderColumnType.NumericText, 50, "タイム", h => h.Analyzer.Data.ResultTime.ToString("mm\\:ss"), (h, v) => AnalysisUtil.CompareValue(h.Analyzer.ResultTimeDeviationValue, 65, 35)),
      new(FinderColumnProperty.After3HalongTime, FinderColumnType.NumericText, 40, "A3HT", "後3ハロンタイム", h => h.Analyzer.Data.AfterThirdHalongTime.ToString("ss\\.f"), (h, v) => AnalysisUtil.CompareValue(h.Analyzer.A3HResultTimeDeviationValue, 65, 35)),
      new(FinderColumnProperty.RunningStyle, FinderColumnType.RunningStyle, 50, "脚質", h => h.Analyzer.Data.RunningStyle),
      new(FinderColumnProperty.CornerOrders, FinderColumnType.CornerPlaces, 150, "コーナー順位", h => h.Analyzer.CornerGrades),
      new(FinderColumnProperty.RiderName, FinderColumnType.Text, 70, "騎手", h => h.Analyzer.Data.RiderName),
      new(FinderColumnProperty.HorseMark, FinderColumnType.HorseMark, 32, "印", h => h.Analyzer.Data.Mark),
      new(FinderColumnProperty.Age, FinderColumnType.NumericTextWithoutZero, 30, "齢", "年齢", h => h.Analyzer.Data.Age),
      new(FinderColumnProperty.Sex, FinderColumnType.HorseSex, 30, "性", "性別", h => h.Analyzer.Data.Sex),

      new(FinderColumnProperty.Before3HalongTime, FinderColumnType.NumericText, 40, "前3H", "前3ハロンタイム", h => h.Analyzer.Race.BeforeHaronTime3 / 10.0, v => ((double)v) != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 35, 45, true)),
      new(FinderColumnProperty.Before3HalongTimeNormalized, FinderColumnType.NumericText, 40, "前3H換", "前3ハロンタイム (距離に応じて置換)", h => h.RaceAnalyzer.NormalizedBefore3HaronTime / 10.0, v => ((double)v) != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 35, 45, true)),
      new(FinderColumnProperty.Before4HalongTime, FinderColumnType.NumericText, 40, "前4H", "前4ハロンタイム", h => h.Analyzer.Race.BeforeHaronTime4 / 10.0, v => ((double)v) != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 45, 55, true)),
      new(FinderColumnProperty.After4HalongTime, FinderColumnType.NumericText, 40, "後4H", "後4ハロンタイム", h => h.Analyzer.Race.AfterHaronTime4 / 10.0, v => ((double)v) != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 45, 55, true)),
      new(FinderColumnProperty.Pci, FinderColumnType.NumericText, 40, "PCI", h => h.Pci, v => ((double)v) != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 45, 55, true)),
      new(FinderColumnProperty.Pci3, FinderColumnType.NumericText, 40, "PCI3", h => h.Pci3, v => ((double)v) != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 45, 55, true)),
      new(FinderColumnProperty.Rpci, FinderColumnType.NumericText, 40, "RPCI", h => h.Rpci, v => ((double)v) != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 45, 55, true)),
      new(FinderColumnProperty.SinglePayoff, FinderColumnType.NumericText, 70, "単払戻", "単勝払い戻し", h => h.Payoff?.SingleNumber1Money ?? 0, v => ((int)v) != default ? v : string.Empty, (h, v) => AnalysisUtil.CompareValue((int)v, 700, 150)),
      new(FinderColumnProperty.FramePayoff, FinderColumnType.NumericText, 70, "枠払戻", "枠連払い戻し", h => h.Payoff?.FrameNumber1Money ?? 0, v => ((int)v) != default ? v : string.Empty, (h, v) => AnalysisUtil.CompareValue((int)v, 700, 150)),
      new(FinderColumnProperty.QuinellaPayoff, FinderColumnType.NumericText, 70, "連払戻", "馬複払い戻し", h => h.Payoff?.QuinellaNumber1Money ?? 0, v => ((int)v) != default ? v : string.Empty, (h, v) => AnalysisUtil.CompareValue((int)v, 3000, 1500)),
      new(FinderColumnProperty.ExactaPayoff, FinderColumnType.NumericText, 70, "単払戻", "馬単払い戻し", h => h.Payoff?.ExactaNumber1Money ?? 0, v => ((int)v) != default ? v : string.Empty, (h, v) => AnalysisUtil.CompareValue((int)v, 5000, 2000)),
      new(FinderColumnProperty.TrioPayoff, FinderColumnType.NumericText, 70, "三複払", "三連複払い戻し", h => h.Payoff?.TrioNumber1Money ?? 0, v => ((int)v) != default ? v : string.Empty, (h, v) => AnalysisUtil.CompareValue((int)v, 10000, 2000)),
      new(FinderColumnProperty.TrifectaPayoff, FinderColumnType.NumericText, 70, "三単払", "三連単払い戻し", h => h.Payoff?.TrifectaNumber1Money ?? 0, v => ((int)v) != default ? v : string.Empty, (h, v) => AnalysisUtil.CompareValue((int)v, 20000, 5000)),

      new(FinderColumnProperty.LapTime1, FinderColumnType.NumericText, 40, "L1", "ラップタイム 1", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(0) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
      new(FinderColumnProperty.LapTime2, FinderColumnType.NumericText, 40, "L2", "ラップタイム 2", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(1) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
      new(FinderColumnProperty.LapTime3, FinderColumnType.NumericText, 40, "L3", "ラップタイム 3", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(2) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
      new(FinderColumnProperty.LapTime4, FinderColumnType.NumericText, 40, "L4", "ラップタイム 4", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(3) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
      new(FinderColumnProperty.LapTime5, FinderColumnType.NumericText, 40, "L5", "ラップタイム 5", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(4) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
      new(FinderColumnProperty.LapTime6, FinderColumnType.NumericText, 40, "L6", "ラップタイム 6", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(5) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
      new(FinderColumnProperty.LapTime7, FinderColumnType.NumericText, 40, "L7", "ラップタイム 7", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(6) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
      new(FinderColumnProperty.LapTime8, FinderColumnType.NumericText, 40, "L8", "ラップタイム 8", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(7) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
      new(FinderColumnProperty.LapTime9, FinderColumnType.NumericText, 40, "L9", "ラップタイム 9", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(8) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
      new(FinderColumnProperty.LapTime10, FinderColumnType.NumericText, 40, "L10", "ラップタイム 10", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(9) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
      new(FinderColumnProperty.LapTime11, FinderColumnType.NumericText, 40, "L11", "ラップタイム 11", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(10) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
      new(FinderColumnProperty.LapTime12, FinderColumnType.NumericText, 40, "L12", "ラップタイム 12", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(11) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
      new(FinderColumnProperty.LapTime13, FinderColumnType.NumericText, 40, "L13", "ラップタイム 13", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(12) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
      new(FinderColumnProperty.LapTime14, FinderColumnType.NumericText, 40, "L14", "ラップタイム 14", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(13) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
      new(FinderColumnProperty.LapTime15, FinderColumnType.NumericText, 40, "L15", "ラップタイム 15", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(14) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
      new(FinderColumnProperty.LapTime16, FinderColumnType.NumericText, 40, "L16", "ラップタイム 16", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(15) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
      new(FinderColumnProperty.LapTime17, FinderColumnType.NumericText, 40, "L17", "ラップタイム 17", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(16) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
      new(FinderColumnProperty.LapTime18, FinderColumnType.NumericText, 40, "L18", "ラップタイム 18", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(17) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
    });
  }

  public class FinderColumnItem : IDisposable
  {
    private readonly CompositeDisposable _disposables = [];

    public ReactiveProperty<FinderColumnProperty> Property { get; } = new();

    public FinderColumnData Data { get; }

    public ReactiveProperty<FinderColumnPropertyGroup> PropertyGroup { get; } = new();

    public ReactiveProperty<IList<FinderColumnPropertyGroupRow>> PropertySelection { get; } = new();

    public ReactiveProperty<FinderColumnPropertyGroupRow?> SelectedProperty { get; } = new();

    public FinderColumnItem(FinderColumnData data)
    {
      this.Data = data;
      this.Property.Value = data.Property;

      this.PropertyGroup.Subscribe(p =>
      {
        if (FinderColumnConfigModel.Instance.PropertyGroupsMap.TryGetValue(p, out var group))
        {
          this.PropertySelection.Value = group;

          if (this.SelectedProperty.Value == null)
          {
            this.SelectedProperty.Value = group.FirstOrDefault(g => g.Property == this.Property.Value);
          }
        }
      }).AddTo(this._disposables);

      this.SelectedProperty
        .Subscribe(p => this.Property.Value = p?.Property ?? default)
        .AddTo(this._disposables);

      this.Property.Skip(1).Subscribe(async p =>
      {
        using var db = new MyContext();
        db.FinderColumns!.Attach(this.Data);

        this.Data.Property = p;

        await db.SaveChangesAsync();
      }).AddTo(this._disposables);
    }

    public void Dispose() => this._disposables.Dispose();
  }

  public class FinderColumnTabItem : IDisposable, ICheckableItem
  {
    private readonly CompositeDisposable _disposables = [];

    public ReactiveProperty<int> TabId { get; } = new();

    public ReactiveProperty<bool> IsChecked { get; } = new();

    public ReactiveCollection<FinderColumnItem> Columns { get; } = new();

    public FinderColumnTabItem(int tabId)
    {
      this.TabId.Value = tabId;

      this.TabId.Subscribe(t =>
      {
        foreach (var column in this.Columns)
        {
          column.Data.TabGroup = (uint)t;
        }
      }).AddTo(this._disposables);
    }

    public FinderColumnTabItem(int tabId, IEnumerable<FinderColumnItem> items) : this(tabId)
    {
      foreach (var item in items)
      {
        this.Columns.Add(item);
      }
    }

    public void Dispose()
    {
      this._disposables.Dispose();

      foreach (var column in this.Columns)
      {
        column.Dispose();
      }
    }
  }
}
