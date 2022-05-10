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

    public IReadOnlyList<RaceAnalysisData> Source => this._source;
    private readonly ReactiveCollection<RaceAnalysisData> _source = new();

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

    public void SetRaces(IEnumerable<RaceAnalysisData> source)
    {
      if (this.IsLoaded.Value)
      {
        return;
      }
      this.IsLoaded.Value = true;

      this._source.AddRangeOnScheduler(source);

      this.Analyze(source.ToArray());
    }

    private void Analyze(IList<RaceAnalysisData> source)
    {
      var startDate = new DateTime(1980, 1, 1);
      var datePoints = new StatisticSingleArray
      {
        Values = source.Select(s => (s.Data.StartTime.Date - startDate).TotalDays).ToArray(),
      };

      this.SpeedPoints.Value.Values = source.Select(s => s.TopHorse.ResultTimePerMeter).ToArray();
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
  }
}
