using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis.Generic;
using KmyKeiba.Models.Analysis.Math;
using KmyKeiba.Models.Race;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis
{
  /// <summary>
  /// 過去レースと比較したレースの傾向を解析
  /// </summary>
  public class RaceTrendAnalyzer : TrendAnalyzer<RaceTrendAnalyzer.Key>, IDisposable
  {
    public enum Key
    {
      Unset,

      [Label("タイム")]
      Speed,

      [Label("脚質")]
      RunningStyle,

      [Label("荒れ度")]
      RoughRate,
    }

    private readonly CompositeDisposable _disposables = new();

    public RaceData Race { get; }

    private IReadOnlyList<RaceHorseData> _raceHorses { get; set; } = Array.Empty<RaceHorseData>();

    private RaceStandardTimeMasterData _standardTime { get; set; } = new();

    public IReadOnlyList<LightRaceInfo> Source => this._source;
    private readonly ReactiveCollection<LightRaceInfo> _source = new();

    public ReactiveProperty<StatisticSingleArray> SpeedPoints { get; } = new(new StatisticSingleArray());

    public ReactiveProperty<StatisticDoubleArray> SpeedDatePoints { get; } = new(StatisticDoubleArray.Empty);

    public ReactiveProperty<StatisticSingleArray> RoughPoints { get; } = new(new StatisticSingleArray());

    public ReactiveProperty<StatisticDoubleArray> RoughDatePoints { get; } = new(StatisticDoubleArray.Empty);

    public ReactiveProperty<TimeSpan> SpeedAverage { get; } = new();

    public ReactiveProperty<TimeSpan> SpeedMedian { get; } = new();

    public ReactiveProperty<TimeSpan> SpeedDeviation { get; } = new();

    public ReactiveProperty<int> FrontRunnersCount { get; } = new();

    public ReactiveProperty<int> StalkersCount { get; } = new();

    public ReactiveProperty<int> SotpsCount { get; } = new();

    public ReactiveProperty<int> SaveRunnersCount { get; } = new();

    public ReactiveProperty<double> RoughMedian { get; } = new();

    public ReactiveProperty<double> RoughDeviation { get; } = new();

    public RaceTrendAnalyzer(RaceData race)
    {
      this.Race = race;
    }

    public void SetRaces(IEnumerable<LightRaceInfo> source, IEnumerable<RaceHorseData> raceHorses, RaceStandardTimeMasterData standardTime)
    {
      if (this.IsLoaded.Value)
      {
        return;
      }
      this.IsLoaded.Value = true;

      this._source.AddRangeOnScheduler(source);
      this._raceHorses = raceHorses.ToArray();
      this._standardTime = standardTime;

      this.Analyze(source.ToArray());
    }

    private void Analyze(IList<LightRaceInfo> source)
    {
      var topHorse = this._raceHorses.FirstOrDefault(rh => rh.ResultOrder == 1) ?? new RaceHorseData();
      var myRace = new LightRaceInfo(this.Race, this._raceHorses.Where(rh => rh.ResultOrder > 0 && rh.ResultOrder <= 3).OrderBy(rh => rh.ResultOrder).ToArray(), this._standardTime);

      var startDate = new DateTime(1980, 1, 1);
      var datePoints = new StatisticSingleArray
      {
        Values = source.Select(s => (s.Data.StartTime.Date - startDate).TotalDays).ToArray(),
      };

      this.SpeedPoints.Value.Values = source.Select(s => s.ResultTimePerMeter).ToArray();
      this.SpeedDatePoints.Value = new StatisticDoubleArray(datePoints, this.SpeedPoints.Value);
      var runningStyles = source.SelectMany(s => s.RunningStyles);
      this.RoughPoints.Value.Values = source.Select(s => s.RoughRate).ToArray();
      this.RoughDatePoints.Value = new StatisticDoubleArray(datePoints, this.RoughPoints.Value);

      // 分析
      this.SpeedAverage.Value = TimeSpan.FromSeconds(this.SpeedPoints.Value.Average * this.Race.Distance);
      this.SpeedMedian.Value = TimeSpan.FromSeconds(this.SpeedPoints.Value.Median * this.Race.Distance);
      this.SpeedDeviation.Value = TimeSpan.FromSeconds(this.SpeedPoints.Value.Deviation * this.Race.Distance);

      this.FrontRunnersCount.Value = runningStyles.Count(s => s == RunningStyle.FrontRunner);
      this.StalkersCount.Value = runningStyles.Count(s => s == RunningStyle.Stalker);
      this.SotpsCount.Value = runningStyles.Count(s => s == RunningStyle.Sotp);
      this.SaveRunnersCount.Value = runningStyles.Count(s => s == RunningStyle.SaveRunner);

      this.RoughMedian.Value = this.RoughPoints.Value.Median;
      this.RoughDeviation.Value = this.RoughPoints.Value.Deviation;

      this.IsAnalyzed.Value = true;
    }

    public class LightRaceInfo
    {
      public RaceData Data { get; }

      public RaceSubjectInfo Subject { get; }

      public RaceHorseData? TopHorse => this.TopHorses.FirstOrDefault(rh => rh.ResultOrder == 1);

      public IReadOnlyList<RaceHorseData> TopHorses { get; }

      public RunningStyle TopRunningStyle { get; }

      public IReadOnlyList<RunningStyle> RunningStyles { get; }

      public double ResultTimePerMeter { get; }

      public double ResultTimeDeviationValue { get; }

      /// <summary>
      /// 荒れ度
      /// </summary>
      public double RoughRate { get; }

      public LightRaceInfo(RaceData race, IReadOnlyList<RaceHorseData> topHorses, RaceStandardTimeMasterData raceStandardTime)
      {
        var topHorse = topHorses.OrderBy(h => h.ResultOrder).FirstOrDefault() ?? new();

        this.Data = race;
        this.TopHorses = topHorses;
        this.Subject = new RaceSubjectInfo(race);
        this.RunningStyles = topHorses.OrderBy(h => h.ResultOrder)
          .Take(3)
          .Select(rh => rh.RunningStyle)
          .Where(rs => rs != RunningStyle.Unknown)
          .ToArray();
        this.TopRunningStyle = this.RunningStyles.FirstOrDefault();

        this.ResultTimePerMeter = (double)topHorse.ResultTime.TotalSeconds / race.Distance;
        this.RoughRate = topHorses.Where(rh => rh.ResultOrder <= 3)
              .Select(rh => (double)rh.Popular * rh.Popular)
              .Append(0)    // Sum時の例外防止
              .Sum() / (1 * 1 + 2 * 2 + 3 * 3);

        if (raceStandardTime.SampleCount > 0)
        {
          this.ResultTimeDeviationValue = 100 - StatisticSingleArray.CalcDeviationValue(this.ResultTimePerMeter, raceStandardTime.Average, raceStandardTime.Deviation);
        }
      }
    }
  }
}
