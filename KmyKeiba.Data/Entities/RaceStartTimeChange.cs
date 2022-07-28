using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  public class RaceStartTimeChange : EntityBase
  {
    public string RaceKey { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public DateTime ChangeTime { get; set; }

    internal RaceStartTimeChange()
    {
    }

    public static RaceStartTimeChange FromJV(JVData_Struct.JV_TC_INFO tc)
    {
      int.TryParse(tc.TCInfoAfter.Ji, out var hours);
      int.TryParse(tc.TCInfoAfter.Fun, out var minutes);

      var obj = new RaceStartTimeChange()
      {
        LastModified = tc.head.MakeDate.ToDateTime(),
        DataStatus = tc.head.DataKubun.ToDataStatus(),
        RaceKey = tc.id.ToRaceKey(),
        StartTime = new DateTime(2000, 1, 1, hours, minutes, 0),
        ChangeTime = tc.HappyoTime.ToDateTime(tc.id),
      };
      return obj;
    }

    public override int GetHashCode()
      => (this.RaceKey).GetHashCode();
  }
}
