using KmyKeiba.Models.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.Finder
{
  public class FinderRaceItem
  {
    public RaceAnalyzer Analyzer { get; }

    public IReadOnlyList<FinderRaceHorseItem> TopHorses { get; }

    public double Pci3 { get; }

    public FinderRaceItem(RaceAnalyzer analyzer)
    {
      this.Analyzer = analyzer;
      this.TopHorses = analyzer.TopHorses.Select(rh => new FinderRaceHorseItem(rh, null)).ToArray();

      if (this.TopHorses.Any())
      {
        if (analyzer.Data.Distance >= 800)
        {
          this.Pci3 = this.TopHorses.Average(rh => rh.Pci);
        }
      }
    }
  }
}
