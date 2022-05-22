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
  public class RaceTrendAnalyzer : TrendAnalyzer, IDisposable
  {
    private readonly CompositeDisposable _disposables = new();

    public RaceData Race { get; }

    public IReadOnlyList<RaceAnalyzer> Source => this._source;
    private readonly ReactiveCollection<RaceAnalyzer> _source = new();

    public ReactiveProperty<StatisticSingleArray> SpeedPoints { get; } = new(new StatisticSingleArray());

    public ReactiveProperty<StatisticDoubleArray> SpeedDatePoints { get; } = new(StatisticDoubleArray.Empty);

    public ReactiveProperty<StatisticSingleArray> RoughPoints { get; } = new(new StatisticSingleArray());

    public ReactiveProperty<StatisticDoubleArray> RoughDatePoints { get; } = new(StatisticDoubleArray.Empty);

    public ReactiveProperty<TimeSpan> SpeedAverage { get; } = new();

    public ReactiveProperty<TimeSpan> SpeedMedian { get; } = new();

    public ReactiveProperty<TimeSpan> SpeedDeviation { get; } = new();

    public ReactiveProperty<ResultOrderGradeMap> FrontRunnersGrade { get; } = new();

    public ReactiveProperty<ResultOrderGradeMap> StalkersGrade { get; } = new();

    public ReactiveProperty<ResultOrderGradeMap> SotpsGrade { get; } = new();

    public ReactiveProperty<ResultOrderGradeMap> SaveRunnersGrade { get; } = new();

    public ReactiveProperty<double> InsideFramePlaceBitsRate { get; } = new();

    public ReactiveProperty<double> OutsideFramePlaceBitsRate { get; } = new();

    public ReactiveProperty<double> RoughMedian { get; } = new();

    public ReactiveProperty<double> RoughDeviation { get; } = new();

    public RaceTrendAnalyzer(RaceData race)
    {
      this.Race = race;
    }

    public void SetSource(IEnumerable<RaceAnalyzer> source)
    {
      if (this.IsLoaded.Value)
      {
        return;
      }
      this.IsLoaded.Value = true;

      this._source.AddRangeOnScheduler(source);

      this.Analyze(source.ToArray());
    }

    private void Analyze(IList<RaceAnalyzer> source)
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

      var horses = source.SelectMany(s => s.TopHorses).Select(h => h.Data);
      this.FrontRunnersGrade.Value = new ResultOrderGradeMap(horses.Where(h => h.RunningStyle == RunningStyle.FrontRunner).ToArray());
      this.StalkersGrade.Value = new ResultOrderGradeMap(horses.Where(h => h.RunningStyle == RunningStyle.Stalker).ToArray());
      this.SotpsGrade.Value = new ResultOrderGradeMap(horses.Where(h => h.RunningStyle == RunningStyle.Sotp).ToArray());
      this.SaveRunnersGrade.Value = new ResultOrderGradeMap(horses.Where(h => h.RunningStyle == RunningStyle.SaveRunner).ToArray());

      this.RoughMedian.Value = this.RoughPoints.Value.Median;
      this.RoughDeviation.Value = this.RoughPoints.Value.Deviation;

      var placeBitsAllCount = source.SelectMany(s => s.TopHorses).Count(h => h.Data.ResultOrder <= 3);
      this.InsideFramePlaceBitsRate.Value = source.SelectMany(s => s.TopHorses.Where(h => h.Data.ResultOrder <= 3 && h.Data.Number / (float)s.Data.HorsesCount <= 1 / 3f)).Count() / (double)placeBitsAllCount;
      this.OutsideFramePlaceBitsRate.Value = source.SelectMany(s => s.TopHorses.Where(h => h.Data.ResultOrder <= 3 && h.Data.Number / (float)s.Data.HorsesCount >= 2 / 3f)).Count() / (double)placeBitsAllCount;

      this.IsAnalyzed.Value = true;
    }
  }
}
