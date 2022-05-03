using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis
{
  /// <summary>
  /// レースの傾向などを解析
  /// </summary>
  internal class RaceAnalyzer
  {
    public RaceInfo Race { get; }

    public RaceAnalyzer(RaceInfo race)
    {
      this.Race = race;
    }

    public async Task PredictAsync(MyContext db)
    {
    }

    public async Task AnalysisResultAsync(MyContext db)
    {
    }
  }
}
