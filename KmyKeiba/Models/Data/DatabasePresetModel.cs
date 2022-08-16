using KmyKeiba.Data.Db;
using KmyKeiba.Data.Wrappers;
using KmyKeiba.Models.Analysis;
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
      using var db = new MyContext();
      await SetExpansionMemoPresets(db);
    }

    private static async Task SetExpansionMemoPresets(MyContext db)
    {
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

    public static IReadOnlyList<FinderColumnDefinition<FinderRaceHorseItem>> GetFinderRaceHorseColumns()
    {
      return new[]
      {
        FinderColumnDefinition.Create<FinderRaceHorseItem>(1, FinderColumnType.RaceSubject, 50, "", h => h.Analyzer.Subject.Subject),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(1, FinderColumnType.BoldText, 200, "レース名", h => h.Analyzer.Subject.DisplayName),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(1, FinderColumnType.Text, 70, "日付", h => h.Analyzer.Race.StartTime.ToString("yy/MM/dd")),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(1, FinderColumnType.Text, 50, "馬場", h => h.Analyzer.Race.Course.GetName()),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(1, FinderColumnType.CourseInfo, 70, "コース", h => h.Analyzer.Race),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(1, FinderColumnType.HorseName, 120, "馬名", h => new { h.Analyzer.Data.Name, h.Analyzer.Memo }),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(1, FinderColumnType.NumericText, 30, "人", h => h.Analyzer.Data.Popular, (h, v) => AnalysisUtil.CompareValue((short)v, h.Analyzer.Race.HorsesCount < 7 ? 2 : 3, 6, true)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(1, FinderColumnType.NumericText, 30, "着", h => h.Analyzer.Data.ResultOrder, (h, v) => AnalysisUtil.CompareValue((short)v, h.Analyzer.Race.HorsesCount < 7 ? 2 : 3, 6, true)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(1, FinderColumnType.NumericText, 50, "タイム", h => h.Analyzer.Data.ResultTime.ToString("mm\\:ss"), (h, v) => AnalysisUtil.CompareValue(h.Analyzer.ResultTimeDeviationValue, 65, 35)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(1, FinderColumnType.NumericText, 40, "T偏", h => h.Analyzer.ResultTimeDeviationValue.ToString("F1"), (h, v) => AnalysisUtil.CompareValue(h.Analyzer.ResultTimeDeviationValue, 65, 35)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(1, FinderColumnType.NumericText, 40, "A3HT", h => h.Analyzer.Data.AfterThirdHalongTime.ToString("ss\\.f"), (h, v) => AnalysisUtil.CompareValue(h.Analyzer.A3HResultTimeDeviationValue, 65, 35)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(1, FinderColumnType.NumericText, 40, "A3偏", h => h.Analyzer.A3HResultTimeDeviationValue.ToString("F1"), (h, v) => AnalysisUtil.CompareValue(h.Analyzer.A3HResultTimeDeviationValue, 65, 35)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(1, FinderColumnType.RunningStyle, 50, "脚質", h => h.Analyzer.Data.RunningStyle),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(1, FinderColumnType.CornerPlaces, 150, "コーナー順位", h => h.Analyzer.CornerGrades),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(1, FinderColumnType.Text, 70, "騎手", h => h.Analyzer.Data.RiderName),
      };
    }
  }
}
