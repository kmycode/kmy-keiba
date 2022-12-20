using CefSharp.Callback;
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
      var ticketItems = tickets
        .Select(t => TicketItem.FromData(t, horses.Select(h => h.Data).ToArray(), odds))
        .Where(t => t != null)
        .Select(t => t!);
      payoff.UpdateTicketsData(ticketItems, horses.Select(h => h.Data).ToArray());

      ticketItems.Where(t => t.Type == TicketType.Single).Sum(t => t.Rows.Count * 100);

      var result = new Result
      {
        PaidMoney = payoff.PayMoneySum.Value,
        PayoffMoney = payoff.HitMoneySum.Value,
      };
      result.ResultPerTicketTypes[TicketType.Single] = this.ToResultObject(TicketType.Single, payoff.Singles, ticketItems);
      result.ResultPerTicketTypes[TicketType.Place] = this.ToResultObject(TicketType.Place, payoff.Places, ticketItems);
      result.ResultPerTicketTypes[TicketType.QuinellaPlace] = this.ToResultObject(TicketType.QuinellaPlace, payoff.QuinellaPlaces, ticketItems);
      result.ResultPerTicketTypes[TicketType.Quinella] = this.ToResultObject(TicketType.Quinella, payoff.Quinellas, ticketItems);
      result.ResultPerTicketTypes[TicketType.Exacta] = this.ToResultObject(TicketType.Exacta, payoff.Exactas, ticketItems);
      result.ResultPerTicketTypes[TicketType.Trio] = this.ToResultObject(TicketType.Trio, payoff.Trios, ticketItems);
      result.ResultPerTicketTypes[TicketType.Trifecta] = this.ToResultObject(TicketType.Trifecta, payoff.Trifectas, ticketItems);
      return result;
    }

    private Result ToResultObject(TicketType type, IPayoffItemCollection payoff, IEnumerable<TicketItem> items)
    {
      var targets = items.Where(t => t.Type == type).ToArray();
      if (targets.Any())
      {
        return new Result()
        {
          PayoffMoney = payoff.HitMoneySum.Value,
          PaidMoney = targets.Sum(t => t.Rows.Count * 100),
        };
      }
      return new();
    }

    public class Result
    {
      public int PaidMoney { get; init; }

      public int PayoffMoney { get; init; }

      public int Income => this.PayoffMoney - this.PaidMoney;

      public Dictionary<TicketType, Result> ResultPerTicketTypes { get; } = new();
    }
  }
}
