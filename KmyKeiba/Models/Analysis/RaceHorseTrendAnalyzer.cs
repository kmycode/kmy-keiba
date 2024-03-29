﻿using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis.Generic;
using KmyKeiba.Models.Analysis.Math;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis
{
  public abstract class RaceHorseTrendAnalyzerBase : TrendAnalyzer
  {
    public RaceData Race { get; }

    public RaceHorseData RaceHorse { get; }

    public IReadOnlyList<RaceHorseAnalyzer> Source => this._source;
    private readonly ReactiveCollection<RaceHorseAnalyzer> _source = new();

    public ReactiveProperty<StatisticSingleArray> SpeedPoints { get; } = new(new StatisticSingleArray());

    public ReactiveProperty<TimeSpan> SpeedAverage { get; } = new();

    public ReactiveProperty<double> DisturbanceRate { get; } = new();

    public ReactiveProperty<double> TimeDeviationValue { get; } = new();

    public ReactiveProperty<double> A3HTimeDeviationValue { get; } = new();

    public ReactiveProperty<double> UntilA3HTimeDeviationValue { get; } = new();

    public ReactiveProperty<double> RecoveryRate { get; } = new();

    public ReactiveProperty<ResultOrderGradeMap> FrontRunnersGrade { get; } = new();

    public ReactiveProperty<ResultOrderGradeMap> StalkersGrade { get; } = new();

    public ReactiveProperty<ResultOrderGradeMap> SotpsGrade { get; } = new();

    public ReactiveProperty<ResultOrderGradeMap> SaveRunnersGrade { get; } = new();

    public ReactiveProperty<ResultOrderGradeMap> AllGrade { get; } = new();

    public ReactiveProperty<ResultOrderGradeMap> InsideFrameGrade { get; } = new();

    public ReactiveProperty<ResultOrderGradeMap> OutsideFrameGrade { get; } = new();

    public RaceHorseTrendAnalyzerBase(int sizeMax, RaceData race, RaceHorseData horse) : base(sizeMax)
    {
      this.Race = race;
      this.RaceHorse = horse;
    }

    public void SetSource(IEnumerable<RaceHorseAnalyzer> source)
    {
      if (this.IsLoaded.Value)
      {
        return;
      }
      this.IsLoaded.Value = true;
      this.IsLoading.Value = false;

      ThreadUtil.InvokeOnUiThread(() =>
      {
        foreach (var s in source)
        {
          this._source.Add(s);
        }
      });
      //this._source.AddRangeOnScheduler(source);

      this.Analyze(source.ToArray());
    }

    protected virtual void Analyze(IReadOnlyList<RaceHorseAnalyzer> source)
    {
      this.SpeedPoints.Value.Values = source.Select(s => s.ResultTimePerMeter).ToArray();
      var count = source.Count;

      if (count > 0)
      {
        // 分析
        this.SpeedAverage.Value = TimeSpan.FromSeconds(this.SpeedPoints.Value.Average * this.Race.Distance);
        this.DisturbanceRate.Value = AnalysisUtil.CalcDisturbanceRate(source);

        this.FrontRunnersGrade.Value = new ResultOrderGradeMap(source.Where(h => h.Data.RunningStyle == RunningStyle.FrontRunner).ToArray());
        this.StalkersGrade.Value = new ResultOrderGradeMap(source.Where(h => h.Data.RunningStyle == RunningStyle.Stalker).ToArray());
        this.SotpsGrade.Value = new ResultOrderGradeMap(source.Where(h => h.Data.RunningStyle == RunningStyle.Sotp).ToArray());
        this.SaveRunnersGrade.Value = new ResultOrderGradeMap(source.Where(h => h.Data.RunningStyle == RunningStyle.SaveRunner).ToArray());

        var timePoint = new StatisticSingleArray(source.Select(h => h.ResultTimeDeviationValue).Where(v => v != default).ToArray());
        var a3htimePoint = new StatisticSingleArray(source.Select(h => h.A3HResultTimeDeviationValue).Where(v => v != default).ToArray());
        var ua3htimePoint = new StatisticSingleArray(source.Select(h => h.UntilA3HResultTimeDeviationValue).Where(v => v != default).ToArray());
        this.TimeDeviationValue.Value = timePoint.Median;
        this.A3HTimeDeviationValue.Value = a3htimePoint.Median;
        this.UntilA3HTimeDeviationValue.Value = ua3htimePoint.Median;

        var validRaces = source.Where(r => r.Data.ResultOrder != 0);
        var sourceArr = source.Select(s => s.Data).ToArray();
        this.AllGrade.Value = new ResultOrderGradeMap(source);
        this.InsideFrameGrade.Value = new ResultOrderGradeMap(source
          .Where(s => s.Data.Number / (float)s.Race.HorsesCount <= 1 / 3f).ToArray());
        this.OutsideFrameGrade.Value = new ResultOrderGradeMap(source
          .Where(s => s.Data.Number / (float)s.Race.HorsesCount >= 2 / 3f).ToArray());

        // 回収率
        if (source.Any(s => s.Data.ResultOrder == 1))
        {
          this.RecoveryRate.Value = source.Where(s => s.Data.ResultOrder == 1).Sum(s => s.Data.Odds * 10) / (float)(count * 100);
        }
      }

      this.IsAnalyzed.Value = true;
    }
  }

  public class SimpleRaceHorseTrendAnalyzer : RaceHorseTrendAnalyzerBase
  {
    public SimpleRaceHorseTrendAnalyzer(int sizeMax, RaceData race, RaceHorseData horse) : base(sizeMax, race, horse)
    {
    }
  }

  public class RaceHorseTrendAnalyzer : RaceHorseTrendAnalyzerBase
  {
    public RaceHorseTrendAnalyzer(int sizeMax, RaceData race, RaceHorseData horse) : base(sizeMax, race, horse)
    {
    }

    protected void SetTimeDeviationValueComparations(IReadOnlyList<RaceHorseAnalyzer> source, int level)
    {
      if (source.Any(s => s.ResultTimeDVComparation != ValueComparation.Unknown))
      {
        return;
      }

      // タイム偏差値に色を付ける
      var timeMax = source.Select(s => s.ResultTimeDeviationValue).OrderByDescending(s => s).ElementAtOrDefault(level) - 1;
      var timeMin = source.Select(s => s.ResultTimeDeviationValue).OrderBy(s => s).ElementAtOrDefault(level) + 1;
      var a3hTimeMax = source.Select(s => s.A3HResultTimeDeviationValue).OrderByDescending(s => s).ElementAtOrDefault(level) - 1;
      var a3hTimeMin = source.Select(s => s.A3HResultTimeDeviationValue).OrderBy(s => s).ElementAtOrDefault(level) + 1;
      var pciMax = source.Select(s => s.PciDeviationValue).OrderByDescending(s => s).ElementAtOrDefault(level) - 1;
      var pciMin = source.Select(s => s.PciDeviationValue).OrderBy(s => s).ElementAtOrDefault(level) + 1;
      foreach (var horse in source)
      {
        horse.ResultTimeDVComparation = AnalysisUtil.CompareValue(horse.ResultTimeDeviationValue, timeMax, timeMin);
        horse.ResultA3HTimeDVComparation = AnalysisUtil.CompareValue(horse.A3HResultTimeDeviationValue, a3hTimeMax, a3hTimeMin);
        horse.PciDVComparation = AnalysisUtil.CompareValue(horse.PciDeviationValue, pciMin, pciMax, true);
      }
    }

    protected override void Analyze(IReadOnlyList<RaceHorseAnalyzer> source)
    {
      base.Analyze(source);
      this.SetTimeDeviationValueComparations(source, 2);
    }
  }

  public class RaceHorseBloodTrendAnalyzer : RaceHorseTrendAnalyzer
  {
    public RaceHorseBloodTrendAnalyzer(int sizeMax, RaceData race, RaceHorseData horse) : base(sizeMax, race, horse)
    {
    }

    protected override void Analyze(IReadOnlyList<RaceHorseAnalyzer> source)
    {
      base.Analyze(source);
      this.SetTimeDeviationValueComparations(source, 4);
    }
  }
}
