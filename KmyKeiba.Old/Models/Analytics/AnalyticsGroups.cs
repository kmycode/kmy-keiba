using KmyKeiba.Data.DataObjects;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analytics
{
  abstract class HorseAnalyticsGroupBase : IHorseAnalyticsGroup
  {
    public string Label { get; }

    private IReadOnlyList<HorseRaceAnalyticsData>? cache;

    public virtual IReadOnlyList<HorseRaceAnalyticsData>? GetCache(RaceHorseDataObject horse)
    {
      return this.cache;
    }

    public virtual void SetCache(IReadOnlyList<HorseRaceAnalyticsData> cache, RaceHorseDataObject horse)
    {
      this.cache = cache;
    }

    public abstract IEnumerable<HorseRaceAnalyticsData> CreateAnalyticsGroup(IEnumerable<HorseRaceAnalyticsData> horses, RaceHorseDataObject horse);

    protected HorseAnalyticsGroupBase(string label)
    {
      this.Label = label;
    }
  }

  class SelfHorseAnalyticsGroup : HorseAnalyticsGroupBase
  {
    public override IEnumerable<HorseRaceAnalyticsData> CreateAnalyticsGroup(IEnumerable<HorseRaceAnalyticsData> horses, RaceHorseDataObject horse)
      => horses.Where((h) => h.Horse.Name == horse.Data.Name);

    public SelfHorseAnalyticsGroup() : base("この馬の成績")
    {
    }
  }

  class RiderHorseAnalyticsGroup : HorseAnalyticsGroupBase
  {
    private static readonly Dictionary<string, IReadOnlyList<HorseRaceAnalyticsData>> caches = new();

    public override IReadOnlyList<HorseRaceAnalyticsData>? GetCache(RaceHorseDataObject horse)
    {
      if (caches.TryGetValue(horse.Data.RiderCode, out var value))
      {
        return value;
      }
      return null;
    }

    public override void SetCache(IReadOnlyList<HorseRaceAnalyticsData> cache, RaceHorseDataObject horse)
    {
      caches[horse.Data.RiderCode] = cache;
    }

    public override IEnumerable<HorseRaceAnalyticsData> CreateAnalyticsGroup(IEnumerable<HorseRaceAnalyticsData> horses, RaceHorseDataObject horse)
      => horses.Where((h) => h.Horse.RiderCode == horse.Data.RiderCode);

    public RiderHorseAnalyticsGroup() : base("騎手の成績")
    {
    }
  }

  class CourseHorseAnalyticsGroup : HorseAnalyticsGroupBase
  {
    private static readonly Dictionary<RaceCourse, IReadOnlyList<HorseRaceAnalyticsData>> caches = new();

    public override IReadOnlyList<HorseRaceAnalyticsData>? GetCache(RaceHorseDataObject horse)
    {
      if (caches.TryGetValue(horse.Data.Course, out var value))
      {
        return value;
      }
      return null;
    }

    public override void SetCache(IReadOnlyList<HorseRaceAnalyticsData> cache, RaceHorseDataObject horse)
    {
      caches[horse.Data.Course] = cache;
    }

    public override IEnumerable<HorseRaceAnalyticsData> CreateAnalyticsGroup(IEnumerable<HorseRaceAnalyticsData> horses, RaceHorseDataObject horse)
      => horses.Where((h) => h.Horse.Course == horse.Data.Course);

    public CourseHorseAnalyticsGroup() : base("競馬場の傾向")
    {
    }
  }
}
