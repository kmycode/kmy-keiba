using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  public class HorseAbnormality : EntityBase
  {
    public string RaceKey { get; set; } = string.Empty;

    public RaceAbnormality AbnormalResult { get; set; }

    public int HorseNumber { get; set; }

    public DateTime ChangeTime { get; set; }

    public static HorseAbnormality FromJV(JVData_Struct.JV_AV_INFO av)
    {
      int.TryParse(av.Umaban, out int number);

      var obj = new HorseAbnormality
      {
        LastModified = av.head.MakeDate.ToDateTime(),
        DataStatus = av.head.DataKubun.ToDataStatus(),
        RaceKey = av.id.ToRaceKey(),
        HorseNumber = number,
        ChangeTime = av.HappyoTime.ToDateTime(av.id),
      };
      obj.AbnormalResult = (short)obj.DataStatus == 1 ? RaceAbnormality.Scratched : RaceAbnormality.ExcludedByStewards;
      return obj;
    }

    public override int GetHashCode()
      => (this.RaceKey + this.HorseNumber).GetHashCode();
  }
}
