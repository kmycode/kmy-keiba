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
        var baseTime = (analyzer.Data.ResultTime.TotalSeconds - analyzer.Data.AfterThirdHalongTime.TotalSeconds) / (analyzer.Race.Distance - 600) * 600;
        this.Pci = baseTime / analyzer.Data.AfterThirdHalongTime.TotalSeconds * 100 - 50;
        this.Rpci = baseTime / analyzer.Data.AfterThirdHalongTime.TotalSeconds;
      }

      this.Payoff = payoff;
      if (payoff != null)
      {
        this.PlaceBetsPayoff = payoff.GetPlaceBetsAsList()
          .FirstOrDefault(i => i.Item1 == analyzer.Data.Number).Item2;
        this.FramePayoff = payoff.GetFrameNumberAsList()
          .FirstOrDefault(i => i.Item1 == analyzer.Data.FrameNumber || i.Item2 == analyzer.Data.FrameNumber).Item3;
        this.QuinellaPlacePayoff = payoff.GetQuinellaPlaceAsList()
          .FirstOrDefault(i => i.Item1 == analyzer.Data.Number || i.Item2 == analyzer.Data.Number).Item3;
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
