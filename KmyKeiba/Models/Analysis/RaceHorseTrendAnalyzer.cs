using KmyKeiba.Common;
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

    public ReactiveProperty<ResultOrderGradeMap> FrontRunnersGrade { get; } = new();

    public ReactiveProperty<ResultOrderGradeMap> StalkersGrade { get; } = new();

    public ReactiveProperty<ResultOrderGradeMap> SotpsGrade { get; } = new();

    public ReactiveProperty<ResultOrderGradeMap> SaveRunnersGrade { get; } = new();

    public ReactiveProperty<ResultOrderGradeMap> AllGrade { get; } = new();

    public ReactiveProperty<ResultOrderGradeMap> InsideFrameGrade { get; } = new();

    public ReactiveProperty<ResultOrderGradeMap> OutsideFrameGrade { get; } = new();

    public RaceHorseTrendAnalyzerBase(RaceData race, RaceHorseData horse)
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

        var horses = source.Select(s => s.Data);
        this.FrontRunnersGrade.Value = new ResultOrderGradeMap(horses.Where(h => h.RunningStyle == RunningStyle.FrontRunner).ToArray());
        this.StalkersGrade.Value = new ResultOrderGradeMap(horses.Where(h => h.RunningStyle == RunningStyle.Stalker).ToArray());
        this.SotpsGrade.Value = new ResultOrderGradeMap(horses.Where(h => h.RunningStyle == RunningStyle.Sotp).ToArray());
        this.SaveRunnersGrade.Value = new ResultOrderGradeMap(horses.Where(h => h.RunningStyle == RunningStyle.SaveRunner).ToArray());

        var validRaces = source.Where(r => r.Data.ResultOrder != 0);
        var sourceArr = source.Select(s => s.Data).ToArray();
        this.AllGrade.Value = new ResultOrderGradeMap(sourceArr);
        this.InsideFrameGrade.Value = new ResultOrderGradeMap(source
          .Where(s => s.Data.Number / (float)s.Race.HorsesCount <= 1 / 3f).Select(s => s.Data).ToArray());
        this.OutsideFrameGrade.Value = new ResultOrderGradeMap(source
          .Where(s => s.Data.Number / (float)s.Race.HorsesCount >= 2 / 3f).Select(s => s.Data).ToArray());
      }

      this.IsAnalyzed.Value = true;
    }
  }

  public class RaceHorseTrendAnalyzer : RaceHorseTrendAnalyzerBase
  {
    public RaceHorseTrendAnalyzer(RaceData race, RaceHorseData horse) : base(race, horse)
    {
    }
  }

  public class RaceHorseBloodTrendAnalyzer : RaceHorseTrendAnalyzerBase
  {
    public RaceHorseBloodTrendAnalyzer(RaceData race, RaceHorseData horse) : base(race, horse)
    {
    }
  }
}
