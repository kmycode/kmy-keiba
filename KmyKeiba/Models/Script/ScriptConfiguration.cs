using KmyKeiba.Common;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Analysis.Table;
using KmyKeiba.Models.Race;
using Microsoft.ClearScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Script
{
  [NoDefaultScriptAccess]
  public class ScriptConfiguration
  {
    private List<ScriptAnalysisTableConfiguration> _tables = new();

    [ScriptMember("raceCacheMax")]
    public int RaceInfoCacheMax { get; set; } = 48;

    [ScriptMember("distanceDiffCentral")]
    public int NearDistanceDiffCentral { get; init; } = 50;

    [ScriptMember("distanceDiffCentralInHorseGrade")]
    public int NearDistanceDiffCentralInHorseGrade { get; init; } = 50;

    [ScriptMember("distanceDiffLocal")]
    public int NearDistanceDiffLocal { get; init; } = 50;

    [ScriptMember("distanceDiffLocalInHorseGrade")]
    public int NearDistanceDiffLocalInHorseGrade { get; init; } = 50;

    [ScriptMember("analysisTableSourceSize")]
    public int AnalysisTableSourceSize { get; set; } = 1000;

    [ScriptMember("analysisTableRaceHorseSourceSize")]
    public int AnalysisTableRaceHorseSourceSize { get; set; } = 4000;

    [ScriptMember("analysisTableSampleSize")]
    public int AnalysisTableSampleSize { get; set; } = 10;

    [ScriptMember("downloadNormalDataIntervalMinutes")]
    public int DownloadNormalDataIntervalMinutes { get; set; } = 120;

    [ScriptMember("isFirstMessageVisible")]
    public bool IsFirstMessageVisible { get; set; } = true;

    [ScriptMember("createAnalysisTable")]
    public ScriptAnalysisTableConfiguration CreateAnalysisTable(string name)
    {
      var table = new ScriptAnalysisTableConfiguration(name);
      this._tables.Add(table);
      return table;
    }

    public void SetConfiguration()
    {
      var configuration = new ApplicationConfiguration
      {
        RaceInfoCacheMax = this.RaceInfoCacheMax,
        NearDistanceDiffCentral = this.NearDistanceDiffCentral,
        NearDistanceDiffCentralInHorseGrade = this.NearDistanceDiffCentralInHorseGrade,
        NearDistanceDiffLocal = this.NearDistanceDiffLocal,
        NearDistanceDiffLocalInHorseGrade = this.NearDistanceDiffLocalInHorseGrade,
        AnalysisTableSourceSize = this.AnalysisTableSourceSize,
        AnalysisTableRaceHorseSourceSize = this.AnalysisTableRaceHorseSourceSize,
        AnalysisTableSampleSize = this.AnalysisTableSampleSize,
        DownloadNormalDataIntervalMinutes = this.DownloadNormalDataIntervalMinutes,
        IsFirstMessageVisible = this.IsFirstMessageVisible,
      };
      foreach (var table in this._tables)
      {
        configuration.AnalysisTableGenerators.Add(table);
      }
      ApplicationConfiguration.Current.Value = configuration;
    }
  }

  [NoDefaultScriptAccess]
  public class ScriptAnalysisTableConfiguration : AnalysisTableGenerator
  {
    private string _name = string.Empty;
    private readonly List<Func<RaceInfo, Task<AnalysisTableRow>>> _rows = new();

    public ScriptAnalysisTableConfiguration(string name)
    {
      this._name = name;
    }

    [ScriptMember("useHorseAnalyzer")]
    public void UseHorseAnalyzer(string name, string keys, string value)
    {
      this._rows.Add(async race =>
      {
        return await new AnalysisTableRow(name, race, (race, horse) =>
          new LambdaAnalysisTableCell<RaceHorseTrendAnalysisSelector, RaceHorseTrendAnalyzer>(
            horse, h => h.TrendAnalyzers!, keys,
            (analyzer, cell) => this.SetValueOfRaceHorseAnalyzer(value, analyzer, cell))
          ).WithLoadAsync();
      });
    }

    [ScriptMember("useRiderAnalyzer")]
    public void UseRiderAnalyzer(string name, string keys, string value)
    {
      this._rows.Add(race =>
      {
        return Task.FromResult(new AnalysisTableRow(name, race, (race, horse) =>
          new LambdaAnalysisTableCell<RaceRiderTrendAnalysisSelector, RaceRiderTrendAnalyzer>(
            horse, h => h.RiderTrendAnalyzers!, keys,
            (analyzer, cell) => this.SetValueOfRaceHorseAnalyzer(value, analyzer, cell))
          ));
      });
    }

    [ScriptMember("useTrainerAnalyzer")]
    public void UseTrainerAnalyzer(string name, string keys, string value)
    {
      this._rows.Add(race =>
      {
        return Task.FromResult(new AnalysisTableRow(name, race, (race, horse) =>
          new LambdaAnalysisTableCell<RaceTrainerTrendAnalysisSelector, RaceTrainerTrendAnalyzer>(
            horse, h => h.TrainerTrendAnalyzers!, keys,
            (analyzer, cell) => this.SetValueOfRaceHorseAnalyzer(value, analyzer, cell))
          ));
      });
    }

    [ScriptMember("useBloodAnalyzer")]
    public void UseRiderAnalyzer(string name, string type, string keys, string value)
    {
      this._rows.Add(race =>
      {
        return Task.FromResult(new AnalysisTableRow(name, race, (race, horse) =>
          new LambdaAnalysisTableCell<RaceHorseBloodTrendAnalysisSelector, RaceHorseBloodTrendAnalyzer>(
            horse, h => h.BloodSelectors!.GetSelectorForceAsync(type).Result, keys,
            (analyzer, cell) => this.SetValueOfRaceHorseAnalyzer(value, analyzer, cell))
          ));
      });
    }

    [ScriptMember("useFinder")]
    public void UseFinder(string name, string keys, string value)
    {
      this._rows.Add(race =>
      {
        return Task.FromResult(new AnalysisTableRow(name, race, (race, horse) =>
        {
          var selector = horse.Finder.AsTrendAnalysisSelector();
          return new LambdaAnalysisWithFinderTableCell(
            horse, h => selector, keys,
            (analyzer, cell) => this.SetValueOfRaceHorseAnalyzer(value, analyzer, cell));
        }));
      });
    }

    [ScriptMember("useRaceHorseAnalyzer")]
    public void UseRaceHorseAnalyzer(string name, string target, string keys, string value)
    {
      void SetCellValue(RaceWinnerHorseTrendAnalyzer analyzer, IAnalysisTableCell cell, Func< RaceHorseAnalyzer, bool> filter)
      {
        var grade = new ResultOrderGradeMap(analyzer.Source
          .Where(filter).ToArray());
        this.SetValueOfGradeMap(value, grade, cell);
        cell.SampleFilter = filter;
      }

      this._rows.Add(race =>
      {
        return Task.FromResult(new AnalysisTableRow(name, race, (race, horse) =>
        {
          return new LambdaAnalysisTableCell<RaceWinnerHorseTrendAnalysisSelector, RaceWinnerHorseTrendAnalyzer>(
            horse, h => race.WinnerTrendAnalyzers, keys,
            (analyzer, cell) =>
            {
              var targets = target.Split('|');
              if (targets.Contains("frame"))
              {
                SetCellValue(analyzer, cell,
                  s => s.Data.FrameNumber == horse.Data.FrameNumber);
              }
              if (targets.Contains("runningstyle"))
              {
                SetCellValue(analyzer, cell,
                  s => s.Data.RunningStyle == horse.History?.RunningStyle);
              }
              if (targets.Contains("sex"))
              {
                SetCellValue(analyzer, cell,
                  s => s.Data.Sex == horse.Data.Sex);
              }
              if (targets.Contains("color"))
              {
                SetCellValue(analyzer, cell,
                  s => s.Data.Color == horse.Data.Color);
              }
              if (targets.Contains("age"))
              {
                SetCellValue(analyzer, cell,
                  s => s.Data.Age == horse.Data.Age);
              }
              if (targets.Contains("popular"))
              {
                SetCellValue(analyzer, cell,
                  s => s.Data.Popular == horse.Data.Popular);
              }
              if (targets.Contains("weightdiff"))
              {
                SetCellValue(analyzer, cell,
                  s => s.Data.WeightDiff == horse.Data.WeightDiff);
              }
              if (targets.Contains("weight"))
              {
                SetCellValue(analyzer, cell,
                  s => s.Data.Weight / 20 == horse.Data.Weight / 20);
              }
              if (targets.Contains("riderweight"))
              {
                SetCellValue(analyzer, cell,
                  s => s.Data.RiderWeight == horse.Data.RiderWeight);
              }
            });
        }));
      });
    }

    private void SetValueOfRaceHorseAnalyzer(string value, RaceHorseTrendAnalyzerBase analyzer, IAnalysisTableCell cell)
    {
      if (value == "time")
      {
        cell.Value.Value = analyzer.TimeDeviationValue.Value.ToString("F1");
        cell.ComparationValue.Value = (float)analyzer.TimeDeviationValue.Value;
        cell.HasComparationValue.Value = analyzer.AllGrade.Value.AllCount > 0;
        cell.SampleSize = 0;
      }
      else if (value == "a3htime")
      {
        cell.Value.Value = analyzer.A3HTimeDeviationValue.Value.ToString("F1");
        cell.ComparationValue.Value = (float)analyzer.A3HTimeDeviationValue.Value;
        cell.HasComparationValue.Value = analyzer.AllGrade.Value.AllCount > 0;
        cell.SampleSize = 0;
      }
      else if (value == "ua3htime")
      {
        cell.Value.Value = analyzer.UntilA3HTimeDeviationValue.Value.ToString("F1");
        cell.ComparationValue.Value = (float)analyzer.UntilA3HTimeDeviationValue.Value;
        cell.HasComparationValue.Value = analyzer.AllGrade.Value.AllCount > 0;
        cell.SampleSize = 0;
      }
      else if (value == "shortesttime")
      {
        if (analyzer.Race.Distance <= 0)
        {
          return;
        }
        Func<RaceHorseAnalyzer, bool> filter = s => s.Data.ResultOrder >= 1 && s.Data.ResultTime != TimeSpan.Zero && s.Race.Distance > 0;
        var timePerMeters = analyzer.Source
          .Where(filter)
          .Select(s => s.Data.ResultTime.TotalSeconds / s.Race.Distance * analyzer.Race.Distance)
          .Select(s => TimeSpan.FromSeconds(s))
          .ToArray();
        if (timePerMeters.Any())
        {
          var shortestTime = timePerMeters.Min();
          cell.Value.Value = shortestTime.ToString("m\\:ss\\.f");
          cell.SubValue.Value = timePerMeters.Length.ToString();
          cell.ComparationValue.Value = (float)shortestTime.TotalSeconds * -1;
          cell.HasComparationValue.Value = true;
          cell.SampleSize = 1;
          cell.SampleFilter = filter;
        }
      }
      else if (value == "recovery")
      {
        cell.Value.Value = analyzer.RecoveryRate.Value.ToString("P1");
        cell.ComparationValue.Value = (float)analyzer.RecoveryRate.Value;
        cell.HasComparationValue.Value = analyzer.AllGrade.Value.AllCount > 0;
        cell.SampleSize = 0;
      }
      else
      {
        this.SetValueOfGradeMap(value, analyzer.AllGrade.Value, cell);
      }
    }

    private void SetValueOfGradeMap(string value, ResultOrderGradeMap grade, IAnalysisTableCell cell)
    {
      if (value == "place")
      {
        cell.Value.Value = grade.PlacingBetsRate.ToString("P1");
        cell.SubValue.Value = $"{grade.PlacingBetsCount} / {grade.AllCount}";
        cell.ComparationValue.Value = grade.PlacingBetsRate;
        cell.HasComparationValue.Value = grade.AllCount > 0;
      }
      if (value == "win")
      {
        cell.Value.Value = grade.WinRate.ToString("P1");
        cell.SubValue.Value = $"{grade.FirstCount} / {grade.AllCount}";
        cell.ComparationValue.Value = grade.WinRate;
        cell.HasComparationValue.Value = grade.AllCount > 0;
      }
    }

    public override async Task<AnalysisTable> GenerateAsync(RaceInfo race)
    {
      var table = new AnalysisTable(this._name, race.Horses);
      foreach (var row in this._rows)
      {
        table.Rows.Add(await row(race));
      }
      return table;
    }
  }
}
