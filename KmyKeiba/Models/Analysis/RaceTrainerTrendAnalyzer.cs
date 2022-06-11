using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis
{
  public class RaceTrainerTrendAnalyzer : RaceHorseTrendAnalyzerBase
  {
    public RaceTrainerTrendAnalyzer(int sizeMax, RaceData race, RaceHorseData horse) : base(sizeMax, race, horse)
    {
    }

    protected override void Analyze(IReadOnlyList<RaceHorseAnalyzer> source)
    {
      base.Analyze(source);
    }
  }
}
