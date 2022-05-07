using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis.Generic;
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
      [Label("スピード")]
      Speed,

      [Label("脚質")]
      RunningStyle,
    }

    public RaceData Race { get; }

    public IReadOnlyList<RaceData> Source => this._source;
    private readonly ReactiveCollection<RaceData> _source = new();

    public ReactiveProperty<bool> IsLoaded { get; } = new();

    public ReactiveCollection<RaceTrendSpeed> Speeds { get; } = new();

    public ReactiveCollection<RaceTrendRunningStyle> RunningStyles { get; } = new();

    public RaceTrendAnalyzer(RaceData race, IEnumerable<RaceData> source)
    {
      this.Race = race;
      this.SetRaces(source);
    }

    public RaceTrendAnalyzer(RaceData race)
    {
      this.Race = race;
    }

    public void SetRaces(IEnumerable<RaceData> source)
    {
      if (this.IsLoaded.Value)
      {
        return;
      }
      this.IsLoaded.Value = true;

      foreach (var race in source)
      {
        this._source.Add(race);
      }

      this.Analyze();
    }

    private void Analyze()
    {
      // TODO: 実装
      this.MenuItemsPrivate.SetValue(Key.Speed, "並み");
    }
  }

  public enum RaceTrendSpeed
  {
    [Label("分析前")]
    Undefined,
  }

  public enum RaceTrendRunningStyle
  {
    [Label("分析前")]
    Undefined,
  }
}
