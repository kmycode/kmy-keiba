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

    public short Lap1Time { get; set; }

    public short Lap2Time { get; set; }

    public short Lap3Time { get; set; }

    public short Lap4Time { get; set; }

    public short Lap5Time { get; set; }

    public short Lap6Time { get; set; }

    public short Lap7Time { get; set; }

    public short Lap8Time { get; set; }

    public short Lap9Time { get; set; }

    public short Lap10Time { get; set; }

    public static WoodtipTraining FromJV(JVData_Struct.JV_WC_WOOD tr)
    {
      short.TryParse(tr.TresenKubun, out short center);
      short.TryParse(tr.Course, out short course);
      short.TryParse(tr.BabaAround, out short baba);
      short.TryParse(tr.LapTime1, out short lap1);
      short.TryParse(tr.LapTime2, out short lap2);
      short.TryParse(tr.LapTime3, out short lap3);
      short.TryParse(tr.LapTime4, out short lap4);
      short.TryParse(tr.LapTime5, out short lap5);
      short.TryParse(tr.LapTime6, out short lap6);
      short.TryParse(tr.LapTime7, out short lap7);
      short.TryParse(tr.LapTime8, out short lap8);
      short.TryParse(tr.LapTime9, out short lap9);
      short.TryParse(tr.LapTime10, out short lap10);

      var obj = new WoodtipTraining
      {
        LastModified = tr.head.MakeDate.ToDateTime(),
        DataStatus = tr.head.DataKubun.ToDataStatus(),
        HorseKey = tr.KettoNum,
        StartTime = tr.ChokyoDate.ToDateTime(tr.ChokyoTime),
        Center = (TrainingCenter)center,
        Course = (WoodtipCourse)course,
        Direction = (WoodtipDirection)baba,
        Lap1Time = lap1,
        Lap2Time = lap2,
        Lap3Time = lap3,
        Lap4Time = lap4,
        Lap5Time = lap5,
        Lap6Time = lap6,
        Lap7Time = lap7,
        Lap8Time = lap8,
        Lap9Time = lap9,
        Lap10Time = lap10,
      };
      return obj;
    }
  }
}
