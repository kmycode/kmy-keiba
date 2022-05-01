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

    public float FirstLapTime { get; set; }

    public float SecondLapTime { get; set; }

    public float ThirdLapTime { get; set; }

    public float FourthLapTime { get; set; }

    public static Training FromJV(JVData_Struct.JV_HC_HANRO tr)
    {
      short.TryParse(tr.TresenKubun, out short center);
      float.TryParse(tr.LapTime1, out float lap1);
      float.TryParse(tr.LapTime2, out float lap2);
      float.TryParse(tr.LapTime3, out float lap3);
      float.TryParse(tr.LapTime4, out float lap4);

      var obj = new Training
      {
        LastModified = tr.head.MakeDate.ToDateTime(),
        DataStatus = tr.head.DataKubun.ToDataStatus(),
        HorseKey = tr.KettoNum,
        StartTime = tr.ChokyoDate.ToDateTime(),
        Center = (TrainingCenter)center,
        FirstLapTime = lap1 / 10,
        SecondLapTime = lap2 / 10,
        ThirdLapTime = lap3 / 10,
        FourthLapTime = lap4 / 10,
      };
      return obj;
    }
  }

  public enum TrainingCenter : short
  {
    Miura = 0,
    Ritto = 1,
  }
}
