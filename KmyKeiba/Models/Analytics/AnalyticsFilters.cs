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

    public abstract string Filtering(RaceHorseDataObject horse);
  }

  class SameWeatherHorseAnalyticsFilter : HorseAnalyticsFilterBase
  {
    public override string Filtering(RaceHorseDataObject horse)
      => $"Races.TrackWeather = {(short)horse.Race.Value.Data.TrackWeather}";

    public SameWeatherHorseAnalyticsFilter() : base("天気")
    {
    }
  }

  class NearCourseDistanceHorseAnalyticsFilter : HorseAnalyticsFilterBase
  {
    public override string Filtering(RaceHorseDataObject horse)
      => $"Races.Distance >= {horse.Race.Value.Data.Distance} - 100 AND Races.Distance <= {horse.Race.Value.Data.Distance}";

    public NearCourseDistanceHorseAnalyticsFilter() : base("距離 ±100m")
    {
    }
  }

  class SameRunningStyleHorseAnalyticsFilter : HorseAnalyticsFilterBase
  {
    public override string Filtering(RaceHorseDataObject horse)
      => $"Horse.RunningStyle = {(short)horse.MajorRunningStyle.Value}";

    public SameRunningStyleHorseAnalyticsFilter() : base("脚質")
    {
    }
  }
}
