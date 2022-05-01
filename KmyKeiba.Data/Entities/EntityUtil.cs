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

    public static string ToRaceKeyWithoutRaceNum(this JVData_Struct.RACE_ID2 id)
    {
      return id.Year + id.MonthDay + id.JyoCD + id.Kaiji + id.Nichiji;
    }

    public static DateTime ToDateTime(this JVData_Struct.YMD dt)
    {
      int.TryParse(dt.Year, out int year);
      int.TryParse(dt.Month, out int month);
      int.TryParse(dt.Day, out int day);
      if (year == 0 || month == 0 || day == 0)
      {
        return default;
      }
      return new DateTime(year, month, day);
    }

    public static DateTime ToDateTime(this JVData_Struct.MDHM dt, JVData_Struct.RACE_ID id)
    {
      int.TryParse(dt.Month, out int month);
      int.TryParse(dt.Day, out int day);
      int.TryParse(dt.Hour, out int hour);
      int.TryParse(dt.Minute, out int minute);

      if (month == default)
      {
        return default;
      }

      int.TryParse(id.Year, out int raceYear);
      int.TryParse(id.MonthDay.Substring(0, 2), out int raceMonth);
      var year = raceYear;
      if (raceMonth == 1 && month == 12)
      {
        year--;
      }

      return new DateTime(year, month, day, hour, minute, 0);
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
