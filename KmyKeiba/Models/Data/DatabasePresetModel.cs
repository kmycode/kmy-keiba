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
      using var db = new MyContext();
      await SetExpansionMemoPresets(db);
      await SetAnalysisTablePresets(db);
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

    private static async Task SetAnalysisTablePresets(MyContext db)
    {
      if (await db.AnalysisTables!.AnyAsync() || await db.AnalysisTableWeights!.AnyAsync() || await db.Delimiters!.AnyAsync())
      {
        return;
      }

      await AnalysisTableUtil.InitializeAsync(db);
      var config = AnalysisTableConfigModel.Instance;

      AnalysisTableSurface? table;
      AnalysisTableRow? row;

      async Task AddTableRowAsync(Action<AnalysisTableRow> data)
      {
        row = await config!.AddTableRowAsync(table!);
        if (row == null) return;

        data(row);
      }

      table = await config.AddTableAsync();
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

      table = await config.AddTableAsync();
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

      table = await config.AddTableAsync();
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

        FinderColumnDefinition.Create<FinderRaceHorseItem>(2, FinderColumnType.RaceSubject, 50, "", h => h.Analyzer.Subject.Subject),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(2, FinderColumnType.BoldText, 200, "レース名", h => h.Analyzer.Subject.DisplayName),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(2, FinderColumnType.NumericText, 40, "前3H", h => h.Analyzer.Race.BeforeHaronTime3 / 10.0, v => ((double)v) != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 35, 45, true)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(2, FinderColumnType.NumericText, 40, "置換", h => h.RaceAnalyzer.NormalizedBefore3HaronTime / 10.0, v => ((double)v) != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 35, 45, true)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(2, FinderColumnType.NumericText, 40, "前4H", h => h.Analyzer.Race.BeforeHaronTime4 / 10.0, v => ((double)v) != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 45, 55, true)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(2, FinderColumnType.NumericText, 40, "後3H", h => h.Analyzer.Race.AfterHaronTime3 / 10.0, v => ((double)v) != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 35, 45, true)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(2, FinderColumnType.NumericText, 40, "後4H", h => h.Analyzer.Race.AfterHaronTime4 / 10.0, v => ((double)v) != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 45, 55, true)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(2, FinderColumnType.NumericText, 40, "PCI", h => h.Pci, v => ((double)v) != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 45, 55, true)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(2, FinderColumnType.NumericText, 40, "PCI3", h => h.Pci3, v => ((double)v) != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 45, 55, true)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(2, FinderColumnType.NumericText, 40, "RPCI", h => h.Rpci, v => ((double)v) != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 45, 55, true)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(2, FinderColumnType.NumericText, 70, "単払戻", h => h.Payoff?.SingleNumber1Money ?? 0, v => ((int)v) != default ? v : string.Empty, (h, v) => AnalysisUtil.CompareValue((int)v, 700, 150)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(2, FinderColumnType.NumericText, 70, "枠払戻", h => h.Payoff?.FrameNumber1Money ?? 0, v => ((int)v) != default ? v : string.Empty, (h, v) => AnalysisUtil.CompareValue((int)v, 700, 150)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(2, FinderColumnType.NumericText, 70, "連払戻", h => h.Payoff?.QuinellaNumber1Money ?? 0, v => ((int)v) != default ? v : string.Empty, (h, v) => AnalysisUtil.CompareValue((int)v, 3000, 1500)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(2, FinderColumnType.NumericText, 70, "単払戻", h => h.Payoff?.ExactaNumber1Money ?? 0, v => ((int)v) != default ? v : string.Empty, (h, v) => AnalysisUtil.CompareValue((int)v, 5000, 2000)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(2, FinderColumnType.NumericText, 70, "三複払", h => h.Payoff?.TrioNumber1Money ?? 0, v => ((int)v) != default ? v : string.Empty, (h, v) => AnalysisUtil.CompareValue((int)v, 10000, 2000)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(2, FinderColumnType.NumericText, 70, "三単払", h => h.Payoff?.TrifectaNumber1Money ?? 0, v => ((int)v) != default ? v : string.Empty, (h, v) => AnalysisUtil.CompareValue((int)v, 20000, 5000)),

        FinderColumnDefinition.Create<FinderRaceHorseItem>(3, FinderColumnType.RaceSubject, 50, "", h => h.Analyzer.Subject.Subject),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(3, FinderColumnType.BoldText, 200, "レース名", h => h.Analyzer.Subject.DisplayName),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(3, FinderColumnType.NumericText, 40, "L1", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(0) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(3, FinderColumnType.NumericText, 40, "L2", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(1) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(3, FinderColumnType.NumericText, 40, "L3", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(2) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(3, FinderColumnType.NumericText, 40, "L4", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(3) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(3, FinderColumnType.NumericText, 40, "L5", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(4) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(3, FinderColumnType.NumericText, 40, "L6", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(5) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(3, FinderColumnType.NumericText, 40, "L7", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(6) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(3, FinderColumnType.NumericText, 40, "L8", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(7) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(3, FinderColumnType.NumericText, 40, "L9", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(8) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(3, FinderColumnType.NumericText, 40, "L10", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(9) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(3, FinderColumnType.NumericText, 40, "L11", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(10) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(3, FinderColumnType.NumericText, 40, "L12", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(11) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(3, FinderColumnType.NumericText, 40, "L13", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(12) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(3, FinderColumnType.NumericText, 40, "L14", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(13) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(3, FinderColumnType.NumericText, 40, "L15", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(14) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(3, FinderColumnType.NumericText, 40, "L16", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(15) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(3, FinderColumnType.NumericText, 40, "L17", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(16) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
        FinderColumnDefinition.Create<FinderRaceHorseItem>(3, FinderColumnType.NumericText, 40, "L18", h => h.Analyzer.Race.GetLapTimes().ElementAtOrDefault(17) / 10.0, v => (double)v != default ? ((double)v).ToString("N1") : string.Empty, (h, v) => AnalysisUtil.CompareValue((double)v, 12, 15, true)),
      };
    }
  }
}
