using KmyKeiba.Models.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.Finder
{
  public class FinderRaceHorseItem
  {
    public RaceHorseAnalyzer Analyzer { get; }

    public double Pci { get; }

    public double Rpci { get; }

    public FinderRaceHorseItem(RaceHorseAnalyzer analyzer)
    {
      this.Analyzer = analyzer;

      if (analyzer.Race.Distance >= 800)
      {
        var baseTime = (analyzer.Data.ResultTime.TotalSeconds - analyzer.Data.AfterThirdHalongTime.TotalSeconds) / (analyzer.Race.Distance - 600) * 600;
        this.Pci = baseTime / analyzer.Data.AfterThirdHalongTime.TotalSeconds * 100 - 50;
        this.Rpci = baseTime / analyzer.Data.AfterThirdHalongTime.TotalSeconds;
      }
    }
  }
}
