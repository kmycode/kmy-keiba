using KmyKeiba.Data.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis
{
  public class RaceWinnerHorseTrendAnalyzer : RaceHorseTrendAnalyzerBase
  {
    public RaceWinnerHorseTrendAnalyzer(RaceData race, RaceHorseData horse) : base(race, horse)
    {
    }
  }
}
