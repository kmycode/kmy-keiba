using KmyKeiba.Data.Db;
using KmyKeiba.Data.Wrappers;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Race.AnalysisTable;
using KmyKeiba.Models.Race.Finder;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Data
{
  static class DatabasePresetModel
  {
    public static async Task SetPresetsAsync()
    {
      await SetExpansionMemoPresets();
      await SetAnalysisTablePresets();
      await SetFinderRaceHorseColumns();
    }

    private static async Task SetExpansionMemoPresets()
    {
      using var db = new MyContext();

      if (await db.MemoConfigs!.AnyAsync() || await db.Memos!.AnyAsync())
      {
        return;
      }

      var presets = new ExpansionMemoConfig[]
      {
        new ExpansionMemoConfig
        {
          Header = "レースメモ",
          Target1 = MemoTarget.Race,
          MemoNumber = 1,
          Order = 1,
          Type = MemoType.Race,
          Style = MemoStyle.Memo,
        },
        new ExpansionMemoConfig
        {
          Header = "日記",
          Target1 = MemoTarget.Day,
          MemoNumber = 1,
          Order = 2,
          Type = MemoType.Race,
          Style = MemoStyle.Memo,
        },
        new ExpansionMemoConfig
        {
          Header = "馬メモ",
          Target1 = MemoTarget.Horse,
          MemoNumber = 1,
          Order = 3,
          MemoGroup = 1,
          Type = MemoType.RaceHorse,
          Style = MemoStyle.Memo,
        },
        new ExpansionMemoConfig
        {
          Header = "騎手メモ",
          Target1 = MemoTarget.Rider,
          MemoNumber = 1,
          Order = 4,
          MemoGroup = 1,
          Type = MemoType.RaceHorse,
          Style = MemoStyle.Memo,
        },
        new ExpansionMemoConfig
        {
          Header = "調教師メモ",
          Target1 = MemoTarget.Trainer,
          MemoNumber = 1,
          Order = 5,
          MemoGroup = 1,
          Type = MemoType.RaceHorse,
          Style = MemoStyle.Memo,
        },
      };
      var presetsWithLabel = new (ExpansionMemoConfig, PointLabelData)[]
      {
        (
          new ExpansionMemoConfig
          {
            Header = "レースフラグ",
            Target1 = MemoTarget.Race,
            MemoNumber = 2,
            Order = 6,
            Type = MemoType.Race,
            Style = MemoStyle.Point,
          },
          new PointLabelData
          {
            Name = "レースフラグ",
          }
        ),
        (
          new ExpansionMemoConfig
          {
            Header = "レース結果",
            Target1 = MemoTarget.Race,
            MemoNumber = 3,
            Order = 7,
            Type = MemoType.Race,
            Style = MemoStyle.Point,
          },
          new PointLabelData
          {
            Name = "レース結果",
          }
        ),
        (
          new ExpansionMemoConfig
          {
            Header = "馬券結果",
            Target1 = MemoTarget.Race,
            MemoNumber = 4,
            Order = 8,
            Type = MemoType.Race,
            Style = MemoStyle.Point,
          },
          new PointLabelData
          {
            Name = "馬券結果",
          }
        ),
      };
      presetsWithLabel[0].Item2.SetItems(new PointLabelItem[]
      {
        new PointLabelItem
        {
          Label = string.Empty,
          Point = 0,
          Color = MemoColor.Default,
        },
        new PointLabelItem
        {
          Label = "勝負",
          Point = 1,
          Color = MemoColor.Warning,
        },
        new PointLabelItem
        {
          Label = "見送",
          Point = 2,
          Color = MemoColor.Bad,
        },
        new PointLabelItem
        {
          Label = "買い",
          Point = 3,
          Color = MemoColor.Good,
        },
        new PointLabelItem
        {
          Label = "重要",
          Point = 4,
          Color = MemoColor.Warning,
        },
        new PointLabelItem
        {
          Label = "練習",
          Point = 5,
          Color = MemoColor.Negative,
        },
      });
      presetsWithLabel[1].Item2.SetItems(new PointLabelItem[]
      {
        new PointLabelItem
        {
          Label = string.Empty,
          Point = 0,
          Color = MemoColor.Default,
        },
        new PointLabelItem
        {
          Label = "万券",
          Point = 1,
          Color = MemoColor.Good,
        },
        new PointLabelItem
        {
          Label = "堅い",
          Point = 2,
          Color = MemoColor.Bad,
        },
        new PointLabelItem
        {
          Label = "普通",
          Point = 3,
          Color = MemoColor.Default,
        },
        new PointLabelItem
        {
          Label = "波乱",
          Point = 4,
          Color = MemoColor.Warning,
        },
        new PointLabelItem
        {
          Label = "大荒",
          Point = 5,
          Color = MemoColor.Negative,
        },
      });
      presetsWithLabel[2].Item2.SetItems(new PointLabelItem[]
      {
        new PointLabelItem
        {
          Label = string.Empty,
          Point = 0,
          Color = MemoColor.Default,
        },
        new PointLabelItem
        {
          Label = "的中",
          Point = 1,
          Color = MemoColor.Good,
        },
        new PointLabelItem
        {
          Label = "ガミ",
          Point = 2,
          Color = MemoColor.Warning,
        },
        new PointLabelItem
        {
          Label = "外し",
          Point = 3,
          Color = MemoColor.Bad,
        },
      });
      await db.MemoConfigs!.AddRangeAsync(presets);
      await db.PointLabels!.AddRangeAsync(presetsWithLabel.Select(l => l.Item2));
      await db.SaveChangesAsync();

      foreach (var label in presetsWithLabel)
      {
        label.Item1.PointLabelId = (short)label.Item2.Id;
      }
      await db.MemoConfigs!.AddRangeAsync(presetsWithLabel.Select(l => l.Item1));
      await db.SaveChangesAsync();
    }

    private static async Task SetAnalysisTablePresets()
    {
      using (var db = new MyContext())
      {
        if (await db.AnalysisTables!.AnyAsync() || await db.AnalysisTableWeights!.AnyAsync() || await db.Delimiters!.AnyAsync())
        {
          return;
        }

        await AnalysisTableUtil.InitializeAsync(db);
      }

      var config = AnalysisTableConfigModel.Instance;

      AnalysisTableSurface? table;
      AnalysisTableRow? row;

      async Task AddTableRowAsync(Action<AnalysisTableRow> data)
      {
        row = await config!.AddTableRowAsync(table!);
        if (row == null) return;

        data(row);
      }

      table = await config.AddTableAsync(false);
      if (table == null) return;
      table.Name.Value = "馬";

      await AddTableRowAsync(r =>
      {
        r.Name.Value = "全レース複勝";
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsUnspecified.Value = false;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorse.Value = true;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorseSelf.Value = true;
      });
      await AddTableRowAsync(r =>
      {
        r.Name.Value = "競馬場複勝";
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsUnspecified.Value = false;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorse.Value = true;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorseSelf.Value = true;
        r.FinderModelForConfig.Input.Course.IsSetCurrentRaceValue.Value = true;
      });
      await AddTableRowAsync(r =>
      {
        r.Name.Value = "単勝回収率";
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsUnspecified.Value = false;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorse.Value = true;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorseSelf.Value = true;
        r.Output.Value = AnalysisTableRowOutputType.RecoveryRate;
      });
      await AddTableRowAsync(r =>
      {
        r.Name.Value = "天気";
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsUnspecified.Value = false;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorse.Value = true;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorseSelf.Value = true;
        r.FinderModelForConfig.Input.Weather.IsSetCurrentRaceValue.Value = true;
      });
      await AddTableRowAsync(r =>
      {
        r.Name.Value = "馬場状態";
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsUnspecified.Value = false;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorse.Value = true;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorseSelf.Value = true;
        r.FinderModelForConfig.Input.TrackCondition.IsSetCurrentRaceValue.Value = true;
      });
      await AddTableRowAsync(r =>
      {
        r.Name.Value = "距離";
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsUnspecified.Value = false;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorse.Value = true;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorseSelf.Value = true;
        r.FinderModelForConfig.Input.Distance.Input.IsUseCurrentRaceValue.Value = true;
      });
      await AddTableRowAsync(r =>
      {
        r.Name.Value = "距離条件";
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsUnspecified.Value = false;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorse.Value = true;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorseSelf.Value = true;
        r.FinderModelForConfig.Input.Distance.Input.IsUseCurrentRaceValue.Value = true;
        r.FinderModelForConfig.Input.Subject.IsSetCurrentRaceValue.Value = true;
      });
      await AddTableRowAsync(r =>
      {
        r.Name.Value = "間隔日数";
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsUnspecified.Value = false;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorse.Value = true;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorseSelf.Value = true;
        r.FinderModelForConfig.Input.PreviousRaceDays.Input.IsUseCurrentRaceHorseValue.Value = true;
      });
      await AddTableRowAsync(r =>
      {
        r.Name.Value = "騎手";
        r.BaseWeight.Value = "1.2";
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsUnspecified.Value = false;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorse.Value = true;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorseSelf.Value = true;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorseRider.Value = true;
      });

      table = await config.AddTableAsync(false);
      if (table == null) return;
      table.Name.Value = "競馬場";

      await AddTableRowAsync(r =>
      {
        r.Name.Value = "枠番";
        r.FinderModelForConfig.Input.Course.IsSetCurrentRaceValue.Value = true;
        r.FinderModelForConfig.Input.FrameNumber.IsSetCurrentRaceHorseValue.Value = true;
      });
      await AddTableRowAsync(r =>
      {
        r.Name.Value = "枠番距離";
        r.FinderModelForConfig.Input.Course.IsSetCurrentRaceValue.Value = true;
        r.FinderModelForConfig.Input.Distance.Input.IsUseCurrentRaceValue.Value = true;
        r.FinderModelForConfig.Input.FrameNumber.IsSetCurrentRaceHorseValue.Value = true;
      });
      await AddTableRowAsync(r =>
      {
        r.Name.Value = "枠番距離馬場";
        r.FinderModelForConfig.Input.Course.IsSetCurrentRaceValue.Value = true;
        r.FinderModelForConfig.Input.TrackCondition.IsSetCurrentRaceValue.Value = true;
        r.FinderModelForConfig.Input.Distance.Input.IsUseCurrentRaceValue.Value = true;
        r.FinderModelForConfig.Input.FrameNumber.IsSetCurrentRaceHorseValue.Value = true;
      });
      await AddTableRowAsync(r =>
      {
        r.Name.Value = "脚質";
        r.FinderModelForConfig.Input.Course.IsSetCurrentRaceValue.Value = true;
        r.FinderModelForConfig.Input.RunningStyle.IsSetCurrentRaceHorseValue.Value = true;
      });
      await AddTableRowAsync(r =>
      {
        r.Name.Value = "脚質距離馬場";
        r.FinderModelForConfig.Input.Course.IsSetCurrentRaceValue.Value = true;
        r.FinderModelForConfig.Input.TrackCondition.IsSetCurrentRaceValue.Value = true;
        r.FinderModelForConfig.Input.Distance.Input.IsUseCurrentRaceValue.Value = true;
        r.FinderModelForConfig.Input.RunningStyle.IsSetCurrentRaceHorseValue.Value = true;
      });
      await AddTableRowAsync(r =>
      {
        r.Name.Value = "性別年齢";
        r.FinderModelForConfig.Input.Course.IsSetCurrentRaceValue.Value = true;
        r.FinderModelForConfig.Input.Sex.IsSetCurrentRaceHorseValue.Value = true;
        r.FinderModelForConfig.Input.Age.IsSetCurrentRaceHorseValue.Value = true;
      });
      await AddTableRowAsync(r =>
      {
        r.Name.Value = "人気";
        r.BaseWeight.Value = "0.8";
        r.FinderModelForConfig.Input.Course.IsSetCurrentRaceValue.Value = true;
        r.FinderModelForConfig.Input.Popular.IsSetCurrentRaceHorseValue.Value = true;
      });
      await AddTableRowAsync(r =>
      {
        r.Name.Value = "人気条件";
        r.BaseWeight.Value = "0.8";
        r.FinderModelForConfig.Input.Course.IsSetCurrentRaceValue.Value = true;
        r.FinderModelForConfig.Input.Subject.IsSetCurrentRaceValue.Value = true;
        r.FinderModelForConfig.Input.Popular.IsSetCurrentRaceHorseValue.Value = true;
      });

      table = await config.AddTableAsync(false);
      if (table == null) return;
      table.Name.Value = "騎手";

      await AddTableRowAsync(r =>
      {
        r.Name.Value = "複勝";
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsUnspecified.Value = false;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorse.Value = true;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorseSelf.Value = false;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorseRider.Value = true;
      });
      await AddTableRowAsync(r =>
      {
        r.Name.Value = "競馬場";
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsUnspecified.Value = false;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorse.Value = true;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorseSelf.Value = false;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorseRider.Value = true;
        r.FinderModelForConfig.Input.Course.IsSetCurrentRaceValue.Value = true;
      });
      await AddTableRowAsync(r =>
      {
        r.Name.Value = "馬場状態";
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsUnspecified.Value = false;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorse.Value = true;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorseSelf.Value = false;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorseRider.Value = true;
        r.FinderModelForConfig.Input.TrackCondition.IsSetCurrentRaceValue.Value = true;
      });
      await AddTableRowAsync(r =>
      {
        r.Name.Value = "脚質";
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsUnspecified.Value = false;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorse.Value = true;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorseSelf.Value = false;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorseRider.Value = true;
        r.FinderModelForConfig.Input.RunningStyle.IsSetCurrentRaceHorseValue.Value = true;
      });
      await AddTableRowAsync(r =>
      {
        r.Name.Value = "距離";
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsUnspecified.Value = false;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorse.Value = true;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorseSelf.Value = false;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorseRider.Value = true;
        r.FinderModelForConfig.Input.Distance.Input.IsUseCurrentRaceValue.Value = true;
      });
      await AddTableRowAsync(r =>
      {
        r.Name.Value = "距離条件";
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsUnspecified.Value = false;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorse.Value = true;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorseSelf.Value = false;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorseRider.Value = true;
        r.FinderModelForConfig.Input.Distance.Input.IsUseCurrentRaceValue.Value = true;
        r.FinderModelForConfig.Input.Subject.IsSetCurrentRaceValue.Value = true;
      });
      await AddTableRowAsync(r =>
      {
        r.Name.Value = "単勝オッズ";
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsUnspecified.Value = false;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorse.Value = true;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorseSelf.Value = false;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorseRider.Value = true;
        r.FinderModelForConfig.Input.Odds.Input.IsUseCurrentRaceHorseValue.Value = true;
      });
      await AddTableRowAsync(r =>
      {
        r.Name.Value = "調教師回収率";
        r.Output.Value = AnalysisTableRowOutputType.RecoveryRate;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsUnspecified.Value = false;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorse.Value = true;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorseSelf.Value = false;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorseRider.Value = true;
        r.FinderModelForConfig.Input.HorseOfCurrentRace.IsActiveHorseTrainer.Value = true;
      });
    }

    private static async Task SetFinderRaceHorseColumns()
    {
      using var db = new MyContext();

      if (await db.FinderColumns!.AnyAsync())
      {
        return;
      }

      var presets = new[]
      {
        new FinderColumnData { TabGroup = 1, Order = 1, Property = FinderColumnProperty.RaceSubject, },
        new FinderColumnData { TabGroup = 1, Order = 2, Property = FinderColumnProperty.RaceName, },
        new FinderColumnData { TabGroup = 1, Order = 3, Property = FinderColumnProperty.StartTime, },
        new FinderColumnData { TabGroup = 1, Order = 4, Property = FinderColumnProperty.Course, },
        new FinderColumnData { TabGroup = 1, Order = 5, Property = FinderColumnProperty.CourseInfo, },
        new FinderColumnData { TabGroup = 1, Order = 6, Property = FinderColumnProperty.HorseName, },
        new FinderColumnData { TabGroup = 1, Order = 7, Property = FinderColumnProperty.Popular, },
        new FinderColumnData { TabGroup = 1, Order = 8, Property = FinderColumnProperty.ResultOrder, },
        new FinderColumnData { TabGroup = 1, Order = 9, Property = FinderColumnProperty.ResultTime, },
        new FinderColumnData { TabGroup = 1, Order = 10, Property = FinderColumnProperty.After3HalongTime, },
        new FinderColumnData { TabGroup = 1, Order = 11, Property = FinderColumnProperty.RunningStyle, },
        new FinderColumnData { TabGroup = 1, Order = 12, Property = FinderColumnProperty.CornerOrders, },
        new FinderColumnData { TabGroup = 1, Order = 13, Property = FinderColumnProperty.RiderName, },

        new FinderColumnData { TabGroup = 2, Order = 1, Property = FinderColumnProperty.RaceSubject, },
        new FinderColumnData { TabGroup = 2, Order = 2, Property = FinderColumnProperty.RaceName, },
        new FinderColumnData { TabGroup = 2, Order = 3, Property = FinderColumnProperty.Before3HalongTime, },
        new FinderColumnData { TabGroup = 2, Order = 4, Property = FinderColumnProperty.Before3HalongTimeNormalized, },
        new FinderColumnData { TabGroup = 2, Order = 5, Property = FinderColumnProperty.Before4HalongTime, },
        new FinderColumnData { TabGroup = 2, Order = 6, Property = FinderColumnProperty.After3HalongTime, },
        new FinderColumnData { TabGroup = 2, Order = 7, Property = FinderColumnProperty.After4HalongTime, },
        new FinderColumnData { TabGroup = 2, Order = 8, Property = FinderColumnProperty.Pci, },
        new FinderColumnData { TabGroup = 2, Order = 9, Property = FinderColumnProperty.Pci3, },
        new FinderColumnData { TabGroup = 2, Order = 10, Property = FinderColumnProperty.Rpci, },
        new FinderColumnData { TabGroup = 2, Order = 11, Property = FinderColumnProperty.SinglePayoff, },
        new FinderColumnData { TabGroup = 2, Order = 12, Property = FinderColumnProperty.FramePayoff, },
        new FinderColumnData { TabGroup = 2, Order = 13, Property = FinderColumnProperty.QuinellaPayoff, },
        new FinderColumnData { TabGroup = 2, Order = 14, Property = FinderColumnProperty.ExactaPayoff, },
        new FinderColumnData { TabGroup = 2, Order = 15, Property = FinderColumnProperty.TrioPayoff, },
        new FinderColumnData { TabGroup = 2, Order = 16, Property = FinderColumnProperty.TrifectaPayoff, },

        new FinderColumnData { TabGroup = 3, Order = 1, Property = FinderColumnProperty.RaceSubject, },
        new FinderColumnData { TabGroup = 3, Order = 2, Property = FinderColumnProperty.RaceName, },
        new FinderColumnData { TabGroup = 3, Order = 3, Property = FinderColumnProperty.LapTime1, },
        new FinderColumnData { TabGroup = 3, Order = 4, Property = FinderColumnProperty.LapTime2, },
        new FinderColumnData { TabGroup = 3, Order = 5, Property = FinderColumnProperty.LapTime3, },
        new FinderColumnData { TabGroup = 3, Order = 6, Property = FinderColumnProperty.LapTime4, },
        new FinderColumnData { TabGroup = 3, Order = 7, Property = FinderColumnProperty.LapTime5, },
        new FinderColumnData { TabGroup = 3, Order = 8, Property = FinderColumnProperty.LapTime6, },
        new FinderColumnData { TabGroup = 3, Order = 9, Property = FinderColumnProperty.LapTime7, },
        new FinderColumnData { TabGroup = 3, Order = 10, Property = FinderColumnProperty.LapTime8, },
        new FinderColumnData { TabGroup = 3, Order = 11, Property = FinderColumnProperty.LapTime9, },
        new FinderColumnData { TabGroup = 3, Order = 12, Property = FinderColumnProperty.LapTime10, },
        new FinderColumnData { TabGroup = 3, Order = 13, Property = FinderColumnProperty.LapTime11, },
        new FinderColumnData { TabGroup = 3, Order = 14, Property = FinderColumnProperty.LapTime12, },
        new FinderColumnData { TabGroup = 3, Order = 15, Property = FinderColumnProperty.LapTime13, },
        new FinderColumnData { TabGroup = 3, Order = 16, Property = FinderColumnProperty.LapTime14, },
        new FinderColumnData { TabGroup = 3, Order = 17, Property = FinderColumnProperty.LapTime15, },
        new FinderColumnData { TabGroup = 3, Order = 18, Property = FinderColumnProperty.LapTime16, },
        new FinderColumnData { TabGroup = 3, Order = 19, Property = FinderColumnProperty.LapTime17, },
        new FinderColumnData { TabGroup = 3, Order = 20, Property = FinderColumnProperty.LapTime18, },
      };

      await db.FinderColumns!.AddRangeAsync(presets);
      await db.SaveChangesAsync();
    }
  }
}
