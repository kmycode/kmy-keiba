using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  public class HorseRiderChange : EntityBase
  {
    public string RaceKey { get; set; } = string.Empty;

    public int HorseNumber { get; set; }

    public float RiderWeight { get; set; }

    public string RiderName { get; set; } = string.Empty;

    public string RiderCode { get; set; } = string.Empty;

    internal HorseRiderChange()
    {
    }

    internal static HorseRiderChange FromJV(JVData_Struct.JV_JC_INFO jc)
    {
      int.TryParse(jc.Umaban.Trim(), out int num);
      int.TryParse(jc.JCInfoAfter.Futan.Trim(), out int riderWeight);

      var obj = new HorseRiderChange()
      {
        LastModified = jc.head.MakeDate.ToDateTime(),
        DataStatus = jc.head.DataKubun.ToDataStatus(),
        RaceKey = jc.id.ToRaceKey(),
        HorseNumber = num,
        RiderCode = jc.JCInfoAfter.KisyuCode,
        RiderName = jc.JCInfoAfter.KisyuName.Trim(),
        RiderWeight = (float)riderWeight / 10,
      };
      return obj;
    }

    public override int GetHashCode()
      => (this.RaceKey + this.HorseNumber).GetHashCode();
  }
}
