using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis
{
  /// <summary>
  /// レースに出場した馬の成績を解析
  /// </summary>
  internal class RaceHorseAnalyzer
  {
    public RaceInfo Race { get; }

    public RaceHorseInfo Horse { get; }

    public RaceHorseAnalyzer(RaceInfo race, RaceHorseInfo horse)
    {
      this.Race = race;
      this.Horse = horse;
    }

    public static async Task<RaceHorseAnalyzer?> FromRaceHorse(MyContext db, RaceHorseInfo horse)
    {
      var info = await RaceInfo.FromKeyAsync(db, horse.Data.RaceKey);
      return new RaceHorseAnalyzer(info, horse);
    }

    public async Task PredictAsync(MyContext db)
    {
    }

    public async Task AnalysisResultAsync(MyContext db)
    {
    }
  }
}
