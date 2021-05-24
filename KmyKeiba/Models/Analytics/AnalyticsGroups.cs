using KmyKeiba.Data.DataObjects;
using KmyKeiba.Data.Db;
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

    public abstract string CreateAnalyticsGroup(RaceHorseDataObject horse);

    protected HorseAnalyticsGroupBase(string label)
    {
      this.Label = label;
    }
  }

  class SelfHorseAnalyticsGroup : HorseAnalyticsGroupBase
  {
    public override string CreateAnalyticsGroup(RaceHorseDataObject horse)
      => $"Horse.Name LIKE '{horse.Data.Name}'";

    public SelfHorseAnalyticsGroup() : base("この馬の成績")
    {
    }
  }

  class RiderHorseAnalyticsGroup : HorseAnalyticsGroupBase
  {
    public override string CreateAnalyticsGroup(RaceHorseDataObject horse)
      => $"Horse.RiderCode LIKE '{horse.Data.RiderCode}'";

    public RiderHorseAnalyticsGroup() : base("騎手の成績")
    {
    }
  }

  class CourseHorseAnalyticsGroup : HorseAnalyticsGroupBase
  {
    public override string CreateAnalyticsGroup(RaceHorseDataObject horse)
      => $"Horse.CourseCode = {(short)horse.Data.Course}";

    public CourseHorseAnalyticsGroup() : base("競馬場の傾向")
    {
    }
  }
}
