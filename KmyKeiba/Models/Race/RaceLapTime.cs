using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race
{
  public class RaceLapTime
  {
    public short LapTime { get; }

    public short LapNumber { get; }

    public ValueComparation Comparation { get; private set; }

    public RaceLapTime(short number, short time)
    {
      this.LapTime = time;
      this.LapNumber = number;
    }

    public static IReadOnlyList<RaceLapTime> GetAsList(RaceData race)
    {
      var list = race.GetLapTimes().Select((l, i) => new RaceLapTime((short)(i + 1), l)).ToArray();
      if (list.Any())
      {
        var max = list.Select(l => l.LapTime).OrderBy(l => l).ElementAtOrDefault(1);
        var min = list.Select(l => l.LapTime).OrderByDescending(l => l).ElementAtOrDefault(1);
        foreach (var item in list)
        {
          item.Comparation = AnalysisUtil.CompareValue(item.LapTime, max, min, true);
        }
      }
      return list;
    }
  }
}
