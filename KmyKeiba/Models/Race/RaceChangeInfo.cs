using KmyKeiba.Data.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race
{
  public class RaceChangeInfo
  {
    public RaceChangeData Data { get; }

    public RaceHorseData? Horse { get; }

    public RaceData Race { get; }

    public RaceChangeInfo(RaceChangeData data, RaceData race, IReadOnlyList<RaceHorseData> horses)
    {
      this.Data = data;

      if (data.HorseNumber != default)
      {
        this.Horse = horses.FirstOrDefault(h => h.Number == data.HorseNumber);
      }
      this.Race = race;
    }
  }
}
