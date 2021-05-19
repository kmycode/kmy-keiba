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
  }
}
