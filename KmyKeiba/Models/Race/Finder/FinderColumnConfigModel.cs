using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis.Generic;
using KmyKeiba.Models.Data;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.PointOfService;

namespace KmyKeiba.Models.Race.Finder
{
  public class FinderColumnConfigModel
  {
    public static FinderColumnConfigModel Instance => _instance ??= new();
    private static FinderColumnConfigModel? _instance;

    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public ReactiveProperty<string> ErrorMessage { get; } = new ReactiveProperty<string>();

    public CheckableCollection<FinderColumnTabItem> Tabs => FinderColumnConfigUtil.Tabs;

    public ReactiveProperty<FinderColumnTabItem?> ActiveTab => this.Tabs.ActiveItem;

    public List<FinderColumnPropertyGroup> PropertyGroups { get; } =
    [
      FinderColumnPropertyGroup.Race,
      FinderColumnPropertyGroup.RaceResult,
      FinderColumnPropertyGroup.RaceLapTime,
      FinderColumnPropertyGroup.RacePayoff,
      FinderColumnPropertyGroup.RaceHorse,
      FinderColumnPropertyGroup.RaceHorseResult,
    ];

    public Dictionary<FinderColumnPropertyGroup, List<FinderColumnPropertyGroupRow>> PropertyGroupsMap { get; }

    private readonly Dictionary<FinderColumnPropertyGroup, List<FinderColumnProperty>> _rawPropertyGroupsMap = new()
    {
      { FinderColumnPropertyGroup.Race,
        [
          FinderColumnProperty.RaceName,
          FinderColumnProperty.RaceSubject,
          FinderColumnProperty.StartTime,
          FinderColumnProperty.Course,
          FinderColumnProperty.CourseInfo,
          FinderColumnProperty.HorsesCount,
          FinderColumnProperty.Weather,
          FinderColumnProperty.Condition,
        ]
      },
      { FinderColumnPropertyGroup.RaceResult,
        [
          FinderColumnProperty.Pci3,
          FinderColumnProperty.Rpci,
        ]
      },
      { FinderColumnPropertyGroup.RaceHorse,
        [
          FinderColumnProperty.HorseName,
          FinderColumnProperty.RiderName,
          FinderColumnProperty.Popular,
          FinderColumnProperty.RunningStyle,
          FinderColumnProperty.CornerOrders,
          FinderColumnProperty.HorseMark,
          FinderColumnProperty.Weight,
          FinderColumnProperty.WeightDiff,
          FinderColumnProperty.RiderWeight,
          FinderColumnProperty.Age,
          FinderColumnProperty.Sex,
          FinderColumnProperty.SingleOdds,
          FinderColumnProperty.PlaceOddsMin,
          FinderColumnProperty.PlaceOddsMax,
        ]
      },
      { FinderColumnPropertyGroup.RaceHorseResult,
        [
          FinderColumnProperty.ResultOrder,
          FinderColumnProperty.GoalOrder,
          FinderColumnProperty.ResultTime,
          FinderColumnProperty.After3HalongTime,
          FinderColumnProperty.After4HalongTime,
          FinderColumnProperty.Before3HalongTime,
          FinderColumnProperty.Before3HalongTimeNormalized,
          FinderColumnProperty.Before4HalongTime,
          FinderColumnProperty.Pci,
        ]
      },
      { FinderColumnPropertyGroup.RacePayoff,
        [
          FinderColumnProperty.SinglePayoff,
          FinderColumnProperty.FramePayoff,
          FinderColumnProperty.QuinellaPayoff,
          FinderColumnProperty.ExactaPayoff,
          FinderColumnProperty.TrioPayoff,
          FinderColumnProperty.TrifectaPayoff,
        ]
      },
      { FinderColumnPropertyGroup.RaceLapTime,
        [
          FinderColumnProperty.LapTime1,
          FinderColumnProperty.LapTime2,
          FinderColumnProperty.LapTime3,
          FinderColumnProperty.LapTime4,
          FinderColumnProperty.LapTime5,
          FinderColumnProperty.LapTime6,
          FinderColumnProperty.LapTime7,
          FinderColumnProperty.LapTime8,
          FinderColumnProperty.LapTime9,
          FinderColumnProperty.LapTime10,
          FinderColumnProperty.LapTime11,
          FinderColumnProperty.LapTime12,
          FinderColumnProperty.LapTime13,
          FinderColumnProperty.LapTime14,
          FinderColumnProperty.LapTime15,
          FinderColumnProperty.LapTime16,
          FinderColumnProperty.LapTime17,
          FinderColumnProperty.LapTime18,
        ]
      },
    };

    private FinderColumnConfigModel()
    {
      this.PropertyGroupsMap = this._rawPropertyGroupsMap
        .Select(m => new KeyValuePair<FinderColumnPropertyGroup, List<FinderColumnPropertyGroupRow>>
          (m.Key, m.Value.Select(v => new FinderColumnPropertyGroupRow(v, FinderColumnConfigUtil.RaceHorseColumnBuilders.FirstOrDefault(b => b.Property == v)?.Name ?? string.Empty)).ToList()))
        .ToDictionary();
    }

    public void Initialize()
    {
      foreach (var column in this.Tabs.SelectMany(t => t.Columns))
      {
        var group = this.PropertyGroupsMap.FirstOrDefault(g => g.Value.Any(v => v.Property == column.Data.Property));
        column.SelectedProperty.Value = group.Value?.FirstOrDefault(v => v.Property == column.Data.Property);
        column.PropertyGroup.Value = group.Key;
      }

      this.ActiveTab.Value = this.Tabs.FirstOrDefault();
    }

    public void AddTab()
    {
      var tab = FinderColumnConfigUtil.AddTab();
      tab.IsChecked.Value = true;
    }

    public async Task UpTabAsync()
    {
      var activeTab = this.ActiveTab.Value;
      if (activeTab == null) return;

      try
      {
        using var db = new MyContext();

        await FinderColumnConfigUtil.UpTabAsync(db, activeTab.TabId.Value);
      }
      catch (Exception ex)
      {
        logger.Error("検索結果カラム上に移動でエラー発生", ex);
        this.ErrorMessage.Value = "検索結果カラムの保存でエラーが発生しました";
      }
    }

    public async Task DownTabAsync()
    {
      var activeTab = this.ActiveTab.Value;
      if (activeTab == null) return;

      try
      {
        using var db = new MyContext();

        await FinderColumnConfigUtil.DownTabAsync(db, activeTab.TabId.Value);
      }
      catch (Exception ex)
      {
        logger.Error("検索結果カラム下に移動でエラー発生", ex);
        this.ErrorMessage.Value = "検索結果カラムの保存でエラーが発生しました";
      }
    }

    public async Task RemoveTabAsync()
    {
      var activeTab = this.ActiveTab.Value;
      if (activeTab == null) return;

      var firstTab = this.Tabs.FirstOrDefault();
      if (firstTab == null) return;

      try
      {
        using var db = new MyContext();

        await FinderColumnConfigUtil.RemoveTabAsync(db, activeTab.TabId.Value);

        firstTab.IsChecked.Value = true;
      }
      catch (Exception ex)
      {
        logger.Error("検索結果カラム下に移動でエラー発生", ex);
        this.ErrorMessage.Value = "検索結果カラムの保存でエラーが発生しました";
      }
    }

    public async Task AddColumnAsync()
    {
      var activeTab = this.ActiveTab.Value;
      if (activeTab == null) return;

      this.ErrorMessage.Value = string.Empty;

      var column = new FinderColumnData
      {
        TabGroup = (uint)activeTab.TabId.Value,
        Property = FinderColumnProperty.Empty,
      };

      try
      {
        using var db = new MyContext();

        await FinderColumnConfigUtil.AddColumnAsync(db, column);
      }
      catch (Exception ex)
      {
        logger.Error("検索結果カラム追加でエラー発生", ex);
        this.ErrorMessage.Value = "検索結果カラムの保存でエラーが発生しました";
      }
    }

    public async Task RemoveColumnAsync(FinderColumnItem column)
    {
      var activeTab = this.ActiveTab.Value;
      if (activeTab == null) return;

      this.ErrorMessage.Value = string.Empty;

      try
      {
        using var db = new MyContext();

        await FinderColumnConfigUtil.RemoveColumnAsync(db, column.Data);
      }
      catch (Exception ex)
      {
        logger.Error("検索結果カラム削除でエラー発生", ex);
        this.ErrorMessage.Value = "検索結果カラムの保存でエラーが発生しました";
      }
    }

    public async Task UpColumnAsync(FinderColumnItem column)
    {
      var activeTab = this.ActiveTab.Value;
      if (activeTab == null) return;

      this.ErrorMessage.Value = string.Empty;

      try
      {
        using var db = new MyContext();

        await FinderColumnConfigUtil.UpColumnAsync(db, column.Data);
      }
      catch (Exception ex)
      {
        logger.Error("検索結果カラム上へ移動でエラー発生", ex);
        this.ErrorMessage.Value = "検索結果カラムの保存でエラーが発生しました";
      }
    }

    public async Task DownColumnAsync(FinderColumnItem column)
    {
      var activeTab = this.ActiveTab.Value;
      if (activeTab == null) return;

      this.ErrorMessage.Value = string.Empty;

      try
      {
        using var db = new MyContext();

        await FinderColumnConfigUtil.DownColumnAsync(db, column.Data);
      }
      catch (Exception ex)
      {
        logger.Error("検索結果カラム下へ移動でエラー発生", ex);
        this.ErrorMessage.Value = "検索結果カラムの保存でエラーが発生しました";
      }
    }
  }

  public enum FinderColumnPropertyGroup
  {
    Unknown,

    [Label("レース基本情報")]
    Race,

    [Label("レースの払い戻し")]
    RacePayoff,

    [Label("レース結果")]
    RaceResult,

    [Label("レースのラップタイム")]
    RaceLapTime,

    [Label("馬")]
    RaceHorse,

    [Label("馬のレース結果")]
    RaceHorseResult,
  }

  public class FinderColumnPropertyGroupRow(FinderColumnProperty property, string label)
  {
    public FinderColumnProperty Property { get; } = property;

    public string Label { get; } = label;
  }
}
