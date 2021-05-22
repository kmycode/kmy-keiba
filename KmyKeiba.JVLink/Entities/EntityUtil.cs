using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  static class EntityUtil
  {
    public static string ToRaceKey(this JVData_Struct.RACE_ID id)
    {
      return id.Year + id.MonthDay + id.JyoCD + id.Kaiji + id.Nichiji + id.RaceNum;
    }

    public static DateTime ToDateTime(this JVData_Struct.YMD dt)
    {
      int.TryParse(dt.Year, out int year);
      int.TryParse(dt.Month, out int month);
      int.TryParse(dt.Day, out int day);
      return new DateTime(year, month, day);
    }

    public static RaceDataStatus ToDataStatus(this string val)
    {
      var dataStatus = RaceDataStatus.Unknown;
      switch (val)
      {
        case "A":
          dataStatus = RaceDataStatus.Local;
          break;
        case "B":
          dataStatus = RaceDataStatus.Foreign;
          break;
        default:
          {
            if (int.TryParse(val, out int status))
            {
              dataStatus = (RaceDataStatus)status;
            }
            break;
          }
      }

      return dataStatus;
    }
  }
}
