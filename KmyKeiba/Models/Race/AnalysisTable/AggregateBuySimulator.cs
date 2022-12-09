using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Race.Tickets;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KmyKeiba.JVLink.Entities.HorseWeight;

namespace KmyKeiba.Models.Race.AnalysisTable
{
  public class AggregateBuySimulator
  {
    public ReactiveCollection<AggregateBuyItem> Items { get; } = new();

    public Result CalcPayoff(PayoffInfo payoff, OddsInfo odds, IReadOnlyList<RaceHorseAnalyzer> horses, IReadOnlyList<(RaceHorseMark Mark, short Number)> markData)
    {
      var tickets = this.Items.SelectMany(i => i.ToTickets(markData));
      payoff.UpdateTicketsData(tickets
        .Select(t => TicketItem.FromData(t, horses.Select(h => h.Data).ToArray(), odds))
        .Where(t => t != null)
        .Select(t => t!), horses.Select(h => h.Data).ToArray());

      var result = new Result
      {
        PaidMoney = payoff.PayMoneySum.Value,
        PayoffMoney = payoff.HitMoneySum.Value,
        Income = payoff.Income.Value,
      };
      return result;
    }

    public class Result
    {
      public int PaidMoney { get; init; }

      public int PayoffMoney { get; init; }

      public int Income { get; init; }
    }
  }
}
