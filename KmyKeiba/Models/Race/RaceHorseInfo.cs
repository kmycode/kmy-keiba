using KmyKeiba.Data.Db;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race
{
  public class RaceHorseInfo
  {
    public RaceHorseData Data { get; private init; } = new();

    private RaceHorseInfo()
    {
    }

    public static async Task<RaceHorseInfo?> FromKeyAsync(MyContext db, string raceKey, string key)
    {
      var horse = await db.RaceHorses!.FirstOrDefaultAsync(rh => rh.RaceKey == raceKey && rh.Key == key);
      if (horse == null)
      {
        return null;
      }

      return await FromDataAsync(db, horse);
    }

    public static Task<RaceHorseInfo> FromDataAsync(MyContext db, RaceHorseData horse)
    {
      return Task.FromResult(new RaceHorseInfo
      {
        Data = horse,
      });
    }
  }
}
