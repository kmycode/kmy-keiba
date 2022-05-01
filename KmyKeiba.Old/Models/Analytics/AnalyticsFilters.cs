using KmyKeiba.Data.DataObjects;
using KmyKeiba.Data.Db;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analytics
{
  abstract class HorseAnalyticsFilterBase : IHorseAnalyticsFilter
  {
    public string Label { get; }

    public ReactiveProperty<bool> IsEnabled { get; } = new();

    protected HorseAnalyticsFilterBase(string label)
    {
      this.Label = label;
    }

    public abstract IEnumerable<HorseRaceAnalyticsData> Filtering(IEnumerable<HorseRaceAnalyticsData> data, RaceHorseDataObject horse);
  }

  class SameWeatherHorseAnalyticsFilter : HorseAnalyticsFilterBase
  {
    public override IEnumerable<HorseRaceAnalyticsData> Filtering(IEnumerable<HorseRaceAnalyticsData> data, RaceHorseDataObject horse)
      => data.Where((d) => d.Race.TrackWeather == horse.Race.Value.Data.TrackWeather);

    public SameWeatherHorseAnalyticsFilter() : base("天気")
    {
    }
  }

  class NearCourseDistanceHorseAnalyticsFilter : HorseAnalyticsFilterBase
  {
    public override IEnumerable<HorseRaceAnalyticsData> Filtering(IEnumerable<HorseRaceAnalyticsData> data, RaceHorseDataObject horse)
      => data.Where((d) => d.Race.Distance >= horse.Race.Value.Data.Distance - 100 &&
                           d.Race.Distance <= horse.Race.Value.Data.Distance + 100);

    public NearCourseDistanceHorseAnalyticsFilter() : base("距離 ±100m")
    {
    }
  }

  class SameRunningStyleHorseAnalyticsFilter : HorseAnalyticsFilterBase
  {
    public override IEnumerable<HorseRaceAnalyticsData> Filtering(IEnumerable<HorseRaceAnalyticsData> data, RaceHorseDataObject horse)
      => data.Where((d) => d.Horse.RunningStyle == horse.MajorRunningStyle.Value);

    public SameRunningStyleHorseAnalyticsFilter() : base("脚質")
    {
    }
  }

  class SameCourseHorseAnalyticsFilter : HorseAnalyticsFilterBase
  {
    public override IEnumerable<HorseRaceAnalyticsData> Filtering(IEnumerable<HorseRaceAnalyticsData> data, RaceHorseDataObject horse)
      => data.Where((d) => d.Horse.Course == horse.Data.Course);

    public SameCourseHorseAnalyticsFilter() : base("競馬場")
    {
    }
  }

  class SameCourseConditionHorseAnalyticsFilter : HorseAnalyticsFilterBase
  {
    public override IEnumerable<HorseRaceAnalyticsData> Filtering(IEnumerable<HorseRaceAnalyticsData> data, RaceHorseDataObject horse)
      => data.Where((d) => d.Race.TrackCondition == horse.Race.Value.Data.TrackCondition);

    public SameCourseConditionHorseAnalyticsFilter() : base("競馬場の状態")
    {
    }
  }
}
