using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  public class WoodtipTraining : EntityBase
  {
    public string HorseKey { get; set; } = string.Empty;

    public TrainingCenter Center { get; set; }

    public DateTime StartTime { get; set; }

    public WoodtipCourse Course { get; set; }

    public WoodtipDirection Direction { get; set; }

    public float Lap1Time { get; set; }

    public float Lap2Time { get; set; }

    public float Lap3Time { get; set; }

    public float Lap4Time { get; set; }

    public float Lap5Time { get; set; }

    public float Lap6Time { get; set; }

    public float Lap7Time { get; set; }

    public float Lap8Time { get; set; }

    public float Lap9Time { get; set; }

    public float Lap10Time { get; set; }

    public static WoodtipTraining FromJV(JVData_Struct.JV_WC_WOODTIP tr)
    {
      short.TryParse(tr.TresenKubun, out short center);
      short.TryParse(tr.CourseCD, out short course);
      short.TryParse(tr.Baba, out short baba);
      float.TryParse(tr.LapTime1, out float lap1);
      float.TryParse(tr.LapTime2, out float lap2);
      float.TryParse(tr.LapTime3, out float lap3);
      float.TryParse(tr.LapTime4, out float lap4);
      float.TryParse(tr.LapTime5, out float lap5);
      float.TryParse(tr.LapTime6, out float lap6);
      float.TryParse(tr.LapTime7, out float lap7);
      float.TryParse(tr.LapTime8, out float lap8);
      float.TryParse(tr.LapTime9, out float lap9);
      float.TryParse(tr.LapTime10, out float lap10);

      var obj = new WoodtipTraining
      {
        LastModified = tr.head.MakeDate.ToDateTime(),
        DataStatus = tr.head.DataKubun.ToDataStatus(),
        HorseKey = tr.KettoNum,
        StartTime = tr.ChokyoDate.ToDateTime(),
        Center = (TrainingCenter)center,
        Course = (WoodtipCourse)course,
        Direction = (WoodtipDirection)baba,
        Lap1Time = lap1 / 10,
        Lap2Time = lap2 / 10,
        Lap3Time = lap3 / 10,
        Lap4Time = lap4 / 10,
        Lap5Time = lap5 / 10,
        Lap6Time = lap6 / 10,
        Lap7Time = lap7 / 10,
        Lap8Time = lap8 / 10,
        Lap9Time = lap9 / 10,
        Lap10Time = lap10 / 10,
      };
      return obj;
    }
  }
}
