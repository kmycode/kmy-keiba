using KmyKeiba.Data.Db;
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

    public RaceAnalyzer RaceAnalyzer { get; }

    public RefundData? Payoff { get; }

    public double Pci { get; }

    public double Pci3 { get; }

    public double Rpci { get; }

    public int PlaceBetsPayoff { get; }

    public int FramePayoff { get; }

    public int QuinellaPlacePayoff { get; }

    public int QuinellaPayoff { get; }

    public int ExactaPayoff { get; }

    public int TrioPayoff { get; }

    public int TrifectaPayoff { get; }

    public FinderRaceHorseItem(RaceHorseAnalyzer analyzer, RefundData? payoff)
    {
      this.Analyzer = analyzer;
      this.RaceAnalyzer = new RaceAnalyzer(analyzer.Race,
        analyzer.CurrentRace?.TopHorses ?? Array.Empty<RaceHorseData>(),
        analyzer.CurrentRace?.StandardTime ?? AnalysisUtil.DefaultStandardTime);

      if (analyzer.Race.Distance >= 800)
      {
        this.Pci = AnalysisUtil.CalcPci(analyzer.Race, analyzer.Data);

        if (analyzer.CurrentRace != null)
        {
          if (analyzer.CurrentRace.TopHorses.Count >= 3)
          {
            this.Pci3 = analyzer.CurrentRace.TopHorses.Average(h => AnalysisUtil.CalcPci(analyzer.Race, h));
          }

          var topHorse = analyzer.CurrentRace.TopHorse;
          if (topHorse != null)
          {
            this.Rpci = AnalysisUtil.CalcRpci(analyzer.Race, topHorse);
          }
        }
      }

      this.Payoff = payoff;
      if (payoff != null)
      {
        this.PlaceBetsPayoff = payoff.GetPlaceBetsAsList()
          .FirstOrDefault(i => i.Item1 == analyzer.Data.Number).Item2;
        this.FramePayoff = payoff.GetFrameNumberAsList()
          .FirstOrDefault(i => i.Item1 == analyzer.Data.FrameNumber || i.Item2 == analyzer.Data.FrameNumber).Item3;
        this.QuinellaPlacePayoff = payoff.GetQuinellaPlaceAsList()
          .Where(i => i.Item1 == analyzer.Data.Number || i.Item2 == analyzer.Data.Number)
          .Select(i => i.Item3)
          .Append(0)
          .Sum();
        this.QuinellaPayoff = payoff.GetQuinellaAsList()
          .FirstOrDefault(i => i.Item1 == analyzer.Data.Number || i.Item2 == analyzer.Data.Number).Item3;
        this.ExactaPayoff = payoff.GetExactaAsList()
          .FirstOrDefault(i => i.Item1 == analyzer.Data.Number || i.Item2 == analyzer.Data.Number).Item3;
        this.TrioPayoff = payoff.GetTrioAsList()
          .FirstOrDefault(i => i.Item1 == analyzer.Data.Number || i.Item2 == analyzer.Data.Number || i.Item3 == analyzer.Data.Number).Item4;
        this.TrifectaPayoff = payoff.GetTrifectaAsList()
          .FirstOrDefault(i => i.Item1 == analyzer.Data.Number || i.Item2 == analyzer.Data.Number || i.Item3 == analyzer.Data.Number).Item4;
      }
    }
  }
}
