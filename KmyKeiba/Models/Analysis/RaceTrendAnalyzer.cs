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
    }

    public RaceData Race { get; }

    private IReadOnlyList<RaceHorseData> _raceHorses { get; set; }

    public IReadOnlyList<LightRaceInfo> Source => this._source;
    private readonly ReactiveCollection<LightRaceInfo> _source = new();

    public StatisticSingleArray SpeedPoints { get; } = new();

    public StatisticDoubleArray SpeedDatePoints { get; private set; } = StatisticDoubleArray.Empty;

    public StatisticSingleArray RunningStylePoints { get; } = new();

    public StatisticDoubleArray RunningStyleDatePoints { get; private set; } = StatisticDoubleArray.Empty;

    public RaceTrendAnalyzer(RaceData race)
    {
      this.Race = race;
    }

    public void SetRaces(IEnumerable<LightRaceInfo> source, IEnumerable<RaceHorseData> raceHorses)
    {
      if (this.IsLoaded.Value)
      {
        return;
      }
      this.IsLoaded.Value = true;

      this._source.AddRangeOnScheduler(source);
      this._raceHorses = raceHorses.ToArray();

      this.Analyze(source.ToArray());
    }

    private void Analyze(IList<LightRaceInfo> source)
    {
      var topHorse = this._raceHorses.FirstOrDefault(rh => rh.ResultOrder == 1) ?? new RaceHorseData();

      var startDate = new DateTime(1980, 1, 1);
      var datePoints = new StatisticSingleArray
      {
        Values = source.Select(s => (s.Data.StartTime.Date - startDate).TotalDays).ToArray(),
      };

      this.SpeedPoints.Values = source.Select(s => s.ResultTimePerMeter).ToArray();
      this.SpeedDatePoints = new StatisticDoubleArray(datePoints, this.SpeedPoints);
      this.RunningStylePoints.Values = source.SelectMany(s => s.RunningStyles).Select(r => (double)r).ToArray();
      this.RunningStyleDatePoints = new StatisticDoubleArray(datePoints, this.RunningStylePoints);

      var raceTime = this._raceHorses.FirstOrDefault(rh => rh.ResultOrder == 1)?.ResultTime ?? TimeSpan.Zero;
      var speedD = 100 - this.SpeedPoints.CalcDeviationValue(topHorse.ResultTime.TotalSeconds) / this.Race.Distance;

      var parameters = new List<AnalysisParameter>();
      parameters.Add(new(Key.Speed, "平均", TimeSpan.FromSeconds(this.SpeedPoints.Average * this.Race.Distance).ToString("mm\\:ss"), "", AnalysisParameterType.Standard));
      parameters.Add(new(Key.Speed, "中央値", TimeSpan.FromSeconds(this.SpeedPoints.Median * this.Race.Distance).ToString("mm\\:ss"), "", AnalysisParameterType.Standard));
      parameters.Add(new(Key.Speed, "標準偏差", TimeSpan.FromSeconds(this.SpeedPoints.Deviation * this.Race.Distance).ToString("mm\\:ss"), "", AnalysisParameterType.Standard));
      parameters.Add(new(Key.Speed, "予想タイム", TimeSpan.FromSeconds(this.SpeedDatePoints.CalcRegressionValue((this.Race.StartTime.Date - startDate).TotalDays) * this.Race.Distance).ToString("mm\\:ss"), "", AnalysisParameterType.Standard));
      parameters.Add(new(Key.RunningStyle, "平均", this.RunningStylePoints.Average.ToString("0.00"), "", AnalysisParameterType.Standard));
      parameters.Add(new(Key.RunningStyle, "中央値", this.RunningStylePoints.Median.ToString("0.00"), "", AnalysisParameterType.Standard));
      parameters.Add(new(Key.RunningStyle, "標準偏差", this.RunningStylePoints.Deviation.ToString("0.00"), "", AnalysisParameterType.Standard));

      var runningStyleDeviation = this.RunningStylePoints.Deviation;
      var predictRunningStyle = (this.RunningStyleDatePoints.CalcRegressionValue((this.Race.StartTime.Date - startDate).TotalDays) + 0.4999);
      parameters.Add(new(Key.RunningStyle, "予想脚質", ((RunningStyle)(int)(predictRunningStyle - runningStyleDeviation)).ToLabelString().Substring(0, 1) + "～" +
                                                      ((RunningStyle)(int)(predictRunningStyle + runningStyleDeviation)).ToLabelString().Substring(0, 1), "", AnalysisParameterType.Standard));
      this.Parameters.AddRangeOnScheduler(parameters);

      this.MenuItemsPrivate.AddValues(new[] {
        (Key.Speed, raceTime.ToString("mm\\:ss")),
        (Key.RunningStyle, AnalysisUtil.GetRunningStyleInfo(this.Race, topHorse).RunningStyle.ToLabelString()),
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

      public LightRaceInfo(RaceData race, IReadOnlyList<RaceHorseData> topHorses)
      {
        var topHorse = topHorses.OrderBy(h => h.ResultOrder).FirstOrDefault() ?? new();

        this.Data = race;
        this.TopHorses = topHorses;
        this.Subject = new RaceSubjectInfo(race);
        this.RunningStyles = topHorses.OrderBy(h => h.ResultOrder).Take(3).Select(rh => AnalysisUtil.GetRunningStyleInfo(race, rh).RunningStyle).ToArray();
        this.TopRunningStyle = this.RunningStyles.FirstOrDefault();

        this.ResultTimePerMeter = (float)topHorse.ResultTime.TotalSeconds / race.Distance;
      }
    }
  }
}
