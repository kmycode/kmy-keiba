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

      [Label("スピード")]
      Speed,

      [Label("脚質")]
      RunningStyle,
    }

    public RaceData Race { get; }

    public IReadOnlyList<LightRaceInfo> Source => this._source;
    private readonly ReactiveCollection<LightRaceInfo> _source = new();

    public ReactiveProperty<bool> IsLoaded { get; } = new();

    public StatisticSingleArray SpeedPoints { get; } = new();

    public StatisticSingleArray RunningStylePoints { get; } = new();

    public RaceTrendAnalyzer(RaceData race)
    {
      this.Race = race;
    }

    public void SetRaces(IEnumerable<LightRaceInfo> source)
    {
      if (this.IsLoaded.Value)
      {
        return;
      }
      this.IsLoaded.Value = true;

      this._source.AddRangeOnScheduler(source);

      this.Analyze(source.ToArray());
    }

    private void Analyze(IList<LightRaceInfo> source)
    {
      this.SpeedPoints.Values = source.Select(s => s.ResultTimePerMeter).ToArray();

      // TODO: 実装
      this.MenuItemsPrivate.AddValues(new[] {
        (Key.Speed, "並み"),
        (Key.RunningStyle, "並み"),
      });

      var parameters = new List<AnalysisParameter>();

      parameters.Add(new(Key.Speed, "平均", this.SpeedPoints.Average.ToString(".0000"), "", AnalysisParameterType.Standard));
      parameters.Add(new(Key.Speed, "中央値", this.SpeedPoints.Median.ToString(".0000"), "", AnalysisParameterType.Standard));
      parameters.Add(new(Key.Speed, "標準偏差", this.SpeedPoints.Deviation.ToString(".0000"), "", AnalysisParameterType.Standard));
      parameters.Add(new(Key.Speed, "日付との相関係数", "TODO", "", AnalysisParameterType.Standard));
      parameters.Add(new(Key.Speed, "回帰直線の傾き", "TODO", "", AnalysisParameterType.Standard));

      this.Parameters.AddRangeOnScheduler(parameters);
    }

    public class LightRaceInfo
    {
      public RaceData Data { get; }

      public RaceSubjectInfo Subject { get; }

      public RaceHorseData? TopHorse => this.TopHorses.FirstOrDefault(rh => rh.ResultOrder == 1);

      public IReadOnlyList<RaceHorseData> TopHorses { get; }

      public double ResultTimePerMeter { get; }

      public LightRaceInfo(RaceData race, IReadOnlyList<RaceHorseData> topHorses)
      {
        var topHorse = topHorses.OrderBy(h => h.ResultOrder).FirstOrDefault() ?? new();

        this.Data = race;
        this.TopHorses = topHorses;
        this.Subject = new RaceSubjectInfo(race);

        this.ResultTimePerMeter = (float)topHorse.ResultTime.TotalSeconds / race.Distance;
      }
    }
  }
}
