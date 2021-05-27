using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  public class CourseWeatherCondition : EntityBase
  {
    public string RaceKeyWithoutRaceNum { get; set; } = string.Empty;

    public RaceCourseWeather Weather { get; set; }

    public RaceCourseCondition TurfCondition { get; set; }

    public RaceCourseCondition DirtCondition { get; set; }

    internal CourseWeatherCondition()
    {
    }

    internal static CourseWeatherCondition FromJV(JVData_Struct.JV_WE_WEATHER we)
    {
      int.TryParse(we.TenkoBaba.TenkoCD, out int weather);
      int.TryParse(we.TenkoBaba.SibaBabaCD, out int turf);
      int.TryParse(we.TenkoBaba.DirtBabaCD, out int dirt);

      var obj = new CourseWeatherCondition()
      {
        LastModified = we.head.MakeDate.ToDateTime(),
        DataStatus = we.head.DataKubun.ToDataStatus(),
        RaceKeyWithoutRaceNum = we.id.ToRaceKeyWithoutRaceNum(),
        Weather = (RaceCourseWeather)weather,
        TurfCondition = (RaceCourseCondition)turf,
        DirtCondition = (RaceCourseCondition)dirt,
      };
      return obj;
    }

    public override int GetHashCode() => this.RaceKeyWithoutRaceNum.GetHashCode();
  }
}
