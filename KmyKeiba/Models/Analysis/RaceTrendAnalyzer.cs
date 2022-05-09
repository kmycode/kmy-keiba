using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis.Generic;
using KmyKeiba.Models.Analysis.Math;
using KmyKeiba.Models.Race;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis
{
  /// <summary>
  /// 過去レースと比較したレースの傾向を解析
  /// </summary>
  public class RaceTrendAnalyzer : TrendAnalyzer<RaceTrendAnalyzer.Key>
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

    public RaceData Race { get; }

    private IReadOnlyList<RaceHorseData> _raceHorses { get; set; } = Array.Empty<RaceHorseData>();

    private RaceStandardTimeMasterData _standardTime { get; set; } = new();

    public IReadOnlyList<LightRaceInfo> Source => this._source;
    private readonly ReactiveCollection<LightRaceInfo> _source = new();

    public StatisticSingleArray SpeedPoints { get; } = new();

    public StatisticDoubleArray SpeedDatePoints { get; private set; } = StatisticDoubleArray.Empty;

    public StatisticSingleArray RoughPoints { get; } = new();

    public StatisticDoubleArray RoughDatePoints { get; private set; } = StatisticDoubleArray.Empty;

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

      this.SpeedPoints.Values = source.Select(s => s.ResultTimePerMeter).ToArray();
      this.SpeedDatePoints = new StatisticDoubleArray(datePoints, this.SpeedPoints);
      var runningStyles = source.SelectMany(s => s.RunningStyles);
      this.RoughPoints.Values = source.Select(s => s.RoughRate).ToArray();
      this.RoughDatePoints = new StatisticDoubleArray(datePoints, this.RoughPoints);

      var raceTime = this._raceHorses.FirstOrDefault(rh => rh.ResultOrder == 1)?.ResultTime ?? TimeSpan.Zero;
      var speedD = 100 - this.SpeedPoints.CalcDeviationValue(topHorse.ResultTime.TotalSeconds) / this.Race.Distance;
      
      var parameters = new List<AnalysisParameter>();
      parameters.Add(new(Key.Speed, "平均", TimeSpan.FromSeconds(this.SpeedPoints.Average * this.Race.Distance).ToString("mm\\:ss"), "", AnalysisParameterType.Standard));
      parameters.Add(new(Key.Speed, "中央値", TimeSpan.FromSeconds(this.SpeedPoints.Median * this.Race.Distance).ToString("mm\\:ss"), "", AnalysisParameterType.Standard));
      parameters.Add(new(Key.Speed, "標準偏差", TimeSpan.FromSeconds(this.SpeedPoints.Deviation * this.Race.Distance).ToString("mm\\:ss"), "", AnalysisParameterType.Standard));
      parameters.Add(new(Key.Speed, "傾向予想", TimeSpan.FromSeconds(this.SpeedDatePoints.CalcRegressionValue((this.Race.StartTime.Date - startDate).TotalDays) * this.Race.Distance).ToString("mm\\:ss"), "", AnalysisParameterType.Standard));
      parameters.Add(new(Key.RunningStyle, "逃げ", runningStyles.Count(rs => rs == RunningStyle.FrontRunner).ToString(), "", AnalysisParameterType.Standard));
      parameters.Add(new(Key.RunningStyle, "先行", runningStyles.Count(rs => rs == RunningStyle.Stalker).ToString(), "", AnalysisParameterType.Standard));
      parameters.Add(new(Key.RunningStyle, "差し", runningStyles.Count(rs => rs == RunningStyle.Sotp).ToString(), "", AnalysisParameterType.Standard));
      parameters.Add(new(Key.RunningStyle, "追込", runningStyles.Count(rs => rs == RunningStyle.SaveRunner).ToString(), "", AnalysisParameterType.Standard));
      parameters.Add(new(Key.RoughRate, "平均", this.RoughPoints.Average.ToString("0.00"), "", AnalysisParameterType.Standard));
      parameters.Add(new(Key.RoughRate, "中央値", this.RoughPoints.Median.ToString("0.00"), "", AnalysisParameterType.Standard));
      parameters.Add(new(Key.RoughRate, "標準偏差", this.RoughPoints.Deviation.ToString("0.00"), "", AnalysisParameterType.Standard));
      parameters.Add(new(Key.RoughRate, "傾向予想", this.RoughDatePoints.CalcRegressionValue(myRace.RoughRate).ToString("0.00"), "", AnalysisParameterType.Standard));

      var speedRoughPoint = new StatisticDoubleArray(this.SpeedPoints, this.RoughPoints);
      parameters.Add(new(Key.RoughRate, "タイムとの相関係数", speedRoughPoint.CorrelationCoefficient.ToString("0.00"), "", AnalysisParameterType.Standard));

      this.Parameters.AddRangeOnScheduler(parameters);

      this.MenuItemsPrivate.AddValues(new[] {
        (Key.Speed, raceTime.ToString("mm\\:ss")),
        (Key.RunningStyle, topHorse.RunningStyle.ToLabelString()),
        (Key.RoughRate, myRace.RoughRate.ToString("0.0")),
      });

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
