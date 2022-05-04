using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  public class Training : EntityBase
  {
    public string HorseKey { get; set; } = string.Empty;

    public TrainingCenter Center { get; set; }

    public DateTime StartTime { get; set; }

    public short FirstLapTime { get; set; }

    public short SecondLapTime { get; set; }

    public short ThirdLapTime { get; set; }

    public short FourthLapTime { get; set; }

    public static Training FromJV(JVData_Struct.JV_HC_HANRO tr)
    {
      short.TryParse(tr.TresenKubun, out short center);
      short.TryParse(tr.LapTime1, out short lap1);
      short.TryParse(tr.LapTime2, out short lap2);
      short.TryParse(tr.LapTime3, out short lap3);
      short.TryParse(tr.LapTime4, out short lap4);

      var obj = new Training
      {
        LastModified = tr.head.MakeDate.ToDateTime(),
        DataStatus = tr.head.DataKubun.ToDataStatus(),
        HorseKey = tr.KettoNum,
        StartTime = tr.ChokyoDate.ToDateTime(tr.ChokyoTime),
        Center = (TrainingCenter)center,
        FirstLapTime = lap1,
        SecondLapTime = lap2,
        ThirdLapTime = lap3,
        FourthLapTime = lap4,
      };
      return obj;
    }
  }

  public enum TrainingCenter : short
  {
    Miura = 0,
    Ritto = 1,
  }

  public enum WoodtipCourse : short
  {
    A = 0,
    B = 1,
    C = 2,
    D = 3,
    E = 4,
  }

  public enum WoodtipDirection : short
  {
    Right = 0,
    Left = 1,
  }
}
