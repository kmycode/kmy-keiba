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

    public ReactiveProperty<double> TimeDeviationValue { get; } = new();

    public ReactiveProperty<double> A3HTimeDeviationValue { get; } = new();

    public ReactiveProperty<double> UntilA3HTimeDeviationValue { get; } = new();

    public ReactiveProperty<double> FrontRunnersPlaceBitsRate { get; } = new();

    public ReactiveProperty<double> StalkersPlaceBitsRate { get; } = new();

    public ReactiveProperty<double> SotpsPlaceBitsRate { get; } = new();

    public ReactiveProperty<double> SaveRunnersPlaceBitsRate { get; } = new();

    public ReactiveProperty<int> FrontRunnersCount { get; } = new();

    public ReactiveProperty<int> StalkersCount { get; } = new();

    public ReactiveProperty<int> SotpsCount { get; } = new();

    public ReactiveProperty<int> SaveRunnersCount { get; } = new();

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
      this.IsLoading.Value = false;

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

      var timePoint = new StatisticSingleArray(source.Select(h => h.ResultTimeDeviationValue).Where(v => v != default).ToArray());
      var a3htimePoint = new StatisticSingleArray(source.Select(h => h.A3HResultTimeDeviationValue).Where(v => v != default).ToArray());
      var ua3htimePoint = new StatisticSingleArray(source.Select(h => h.UntilA3HResultTimeDeviationValue).Where(v => v != default).ToArray());
      this.TimeDeviationValue.Value = timePoint.Median;
      this.A3HTimeDeviationValue.Value = a3htimePoint.Median;
      this.UntilA3HTimeDeviationValue.Value = ua3htimePoint.Median;

      var horses = source.SelectMany(s => s.TopHorses).Select(h => h.Data);
      this.FrontRunnersCount.Value = runningStyles.Count(s => s == RunningStyle.FrontRunner);
      this.StalkersCount.Value = runningStyles.Count(s => s == RunningStyle.Stalker);
      this.SotpsCount.Value = runningStyles.Count(s => s == RunningStyle.Sotp);
      this.SaveRunnersCount.Value = runningStyles.Count(s => s == RunningStyle.SaveRunner);
      this.FrontRunnersPlaceBitsRate.Value = source.Count(s => s.TopHorses.Any(h => h.Data.ResultOrder <= 3 && h.Data.RunningStyle == RunningStyle.FrontRunner)) / (double)source.Count;
      this.StalkersPlaceBitsRate.Value = source.Count(s => s.TopHorses.Any(h => h.Data.ResultOrder <= 3 && h.Data.RunningStyle == RunningStyle.Stalker)) / (double)source.Count;
      this.SotpsPlaceBitsRate.Value = source.Count(s => s.TopHorses.Any(h => h.Data.ResultOrder <= 3 && h.Data.RunningStyle == RunningStyle.Sotp)) / (double)source.Count;
      this.SaveRunnersPlaceBitsRate.Value = source.Count(s => s.TopHorses.Any(h => h.Data.ResultOrder <= 3 && h.Data.RunningStyle == RunningStyle.SaveRunner)) / (double)source.Count;

      this.RoughMedian.Value = this.RoughPoints.Value.Median;
      this.RoughDeviation.Value = this.RoughPoints.Value.Deviation;

      this.InsideFramePlaceBitsRate.Value = source.Count(s => s.TopHorses.Any(h => h.Data.ResultOrder <= 3 && h.Data.Number / (float)s.Data.HorsesCount <= 1 / 3f)) / (double)source.Count;
      this.OutsideFramePlaceBitsRate.Value = source.Count(s => s.TopHorses.Any(h => h.Data.ResultOrder <= 3 && h.Data.Number / (float)s.Data.HorsesCount >= 2 / 3f)) / (double)source.Count;

      this.IsAnalyzed.Value = true;
    }
  }
}
