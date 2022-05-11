using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis
{
  public class TrainerAnalysisData
  {
    public IReadOnlyList<RaceHorseAnalysisData> Source { get; }

    public TrainerAnalysisData(IReadOnlyList<RaceHorseAnalysisData> source)
    {
      this.Source = source;
    }
  }
}
