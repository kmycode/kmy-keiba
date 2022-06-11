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
    public RaceWinnerHorseTrendAnalyzer(int sizeMax, RaceData race, RaceHorseData horse) : base(sizeMax, race, horse)
    {
    }
  }
}
