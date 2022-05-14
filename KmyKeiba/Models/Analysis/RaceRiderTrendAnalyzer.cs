using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis.Generic;
using KmyKeiba.Models.Analysis.Math;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis
{
  public class RaceRiderTrendAnalyzer : RaceHorseTrendAnalyzerBase
  {
    public RaceRiderTrendAnalyzer(RaceData race, RaceHorseData horse) : base(race, horse)
    {
    }

    protected override void Analyze(IReadOnlyList<RaceHorseAnalysisData> source)
    {
      base.Analyze(source);
    }
  }
}
