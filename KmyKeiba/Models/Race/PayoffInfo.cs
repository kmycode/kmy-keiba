using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race
{
  public class PayoffInfo : IDisposable
  {
    private CompositeDisposable? _ticketsDisposables;
    private BettingTicketInfo? _tickets;

    public RefundData Payoff { get; }

    public PayoffItemCollection<SinglePayoffItem> Singles { get; } = new();

    public PayoffItemCollection<PlacePayoffItem> Places { get; } = new();

    public PayoffItemCollection<FrameNumberPayoffItem> Frames { get; } = new();

    public PayoffItemCollection<QuinellaPlacePayoffItem> QuinellaPlaces { get; } = new();

    public PayoffItemCollection<QuinellaPayoffItem> Quinellas { get; } = new();

    public PayoffItemCollection<ExactaPayoffItem> Exactas { get; } = new();

    public PayoffItemCollection<TrioPayoffItem> Trios { get; } = new();

    public PayoffItemCollection<TrifectaPayoffItem> Trifectas { get; } = new();

    public ReactiveProperty<int> HitMoneySum { get; } = new();

    public ReactiveProperty<int> PayMoneySum { get; } = new();

    public ReactiveProperty<int> ReturnMoneySum { get; } = new();

    public ReactiveProperty<int> Income { get; } = new();

    public ReactiveProperty<ValueComparation> IncomeComparation { get; } = new();

    public PayoffInfo(RefundData payoff)
    {
      this.Payoff = payoff;

      if (payoff.SingleNumber1 != default)
        this.Singles.Add(new SinglePayoffItem { Number1 = payoff.SingleNumber1, Money = payoff.SingleNumber1Money, });
      if (payoff.SingleNumber2 != default)
        this.Singles.Add(new SinglePayoffItem { Number1 = payoff.SingleNumber2, Money = payoff.SingleNumber2Money, });
      if (payoff.SingleNumber3 != default)
        this.Singles.Add(new SinglePayoffItem { Number1 = payoff.SingleNumber3, Money = payoff.SingleNumber3Money, });
      if (payoff.PlaceNumber1 != default)
        this.Places.Add(new PlacePayoffItem { Number1 = payoff.PlaceNumber1, Money = payoff.PlaceNumber1Money, });
      if (payoff.PlaceNumber2 != default)
        this.Places.Add(new PlacePayoffItem { Number1 = payoff.PlaceNumber2, Money = payoff.PlaceNumber2Money, });
      if (payoff.PlaceNumber3 != default)
        this.Places.Add(new PlacePayoffItem { Number1 = payoff.PlaceNumber3, Money = payoff.PlaceNumber3Money, });
      if (payoff.Frame1Number1 != default)
        this.Frames.Add(new FrameNumberPayoffItem { Frame1 = payoff.Frame1Number1, Frame2 = payoff.Frame2Number1, Money = payoff.FrameNumber1Money, });
      if (payoff.Frame1Number2 != default)
        this.Frames.Add(new FrameNumberPayoffItem { Frame1 = payoff.Frame1Number2, Frame2 = payoff.Frame2Number2, Money = payoff.FrameNumber1Money, });
      if (payoff.QuinellaPlace1Number1 != default)
        this.QuinellaPlaces.Add(new QuinellaPlacePayoffItem { Number1 = payoff.QuinellaPlace1Number1, Number2 = payoff.QuinellaPlace2Number1, Money = payoff.QuinellaPlaceNumber1Money, });
      if (payoff.QuinellaPlace1Number2 != default)
        this.QuinellaPlaces.Add(new QuinellaPlacePayoffItem { Number1 = payoff.QuinellaPlace1Number2, Number2 = payoff.QuinellaPlace2Number2, Money = payoff.QuinellaPlaceNumber2Money, });
      if (payoff.QuinellaPlace1Number3 != default)
        this.QuinellaPlaces.Add(new QuinellaPlacePayoffItem { Number1 = payoff.QuinellaPlace1Number3, Number2 = payoff.QuinellaPlace2Number3, Money = payoff.QuinellaPlaceNumber3Money, });
      if (payoff.QuinellaPlace1Number4 != default)
        this.QuinellaPlaces.Add(new QuinellaPlacePayoffItem { Number1 = payoff.QuinellaPlace1Number4, Number2 = payoff.QuinellaPlace2Number4, Money = payoff.QuinellaPlaceNumber4Money, });
      if (payoff.QuinellaPlace1Number5 != default)
        this.QuinellaPlaces.Add(new QuinellaPlacePayoffItem { Number1 = payoff.QuinellaPlace1Number5, Number2 = payoff.QuinellaPlace2Number5, Money = payoff.QuinellaPlaceNumber5Money, });
      if (payoff.QuinellaPlace1Number6 != default)
        this.QuinellaPlaces.Add(new QuinellaPlacePayoffItem { Number1 = payoff.QuinellaPlace1Number6, Number2 = payoff.QuinellaPlace2Number6, Money = payoff.QuinellaPlaceNumber6Money, });
      if (payoff.QuinellaPlace1Number7 != default)
        this.QuinellaPlaces.Add(new QuinellaPlacePayoffItem { Number1 = payoff.QuinellaPlace1Number7, Number2 = payoff.QuinellaPlace2Number7, Money = payoff.QuinellaPlaceNumber7Money, });
      if (payoff.Quinella1Number1 != default)
        this.Quinellas.Add(new QuinellaPayoffItem { Number1 = payoff.Quinella1Number1, Number2 = payoff.Quinella2Number1, Money = payoff.QuinellaNumber1Money, });
      if (payoff.Quinella1Number2 != default)
        this.Quinellas.Add(new QuinellaPayoffItem { Number1 = payoff.Quinella1Number2, Number2 = payoff.Quinella2Number2, Money = payoff.QuinellaNumber2Money, });
      if (payoff.Quinella1Number3 != default)
        this.Quinellas.Add(new QuinellaPayoffItem { Number1 = payoff.Quinella1Number3, Number2 = payoff.Quinella2Number3, Money = payoff.QuinellaNumber3Money, });
      if (payoff.Exacta1Number1 != default)
        this.Exactas.Add(new ExactaPayoffItem { Number1 = payoff.Exacta1Number1, Number2 = payoff.Exacta2Number1, Money = payoff.ExactaNumber1Money, });
      if (payoff.Exacta1Number2 != default)
        this.Exactas.Add(new ExactaPayoffItem { Number1 = payoff.Exacta1Number2, Number2 = payoff.Exacta2Number2, Money = payoff.ExactaNumber2Money, });
      if (payoff.Exacta1Number3 != default)
        this.Exactas.Add(new ExactaPayoffItem { Number1 = payoff.Exacta1Number3, Number2 = payoff.Exacta2Number3, Money = payoff.ExactaNumber3Money, });
      if (payoff.Trio1Number1 != default)
        this.Trios.Add(new TrioPayoffItem { Number1 = payoff.Trio1Number1, Number2 = payoff.Trio2Number1, Number3 = payoff.Trio3Number1, Money = payoff.TrioNumber1Money, });
      if (payoff.Trio1Number2 != default)
        this.Trios.Add(new TrioPayoffItem { Number1 = payoff.Trio1Number2, Number2 = payoff.Trio2Number2, Number3 = payoff.Trio3Number2, Money = payoff.TrioNumber2Money, });
      if (payoff.Trio1Number3 != default)
        this.Trios.Add(new TrioPayoffItem { Number1 = payoff.Trio1Number3, Number2 = payoff.Trio2Number3, Number3 = payoff.Trio3Number3, Money = payoff.TrioNumber3Money, });
      if (payoff.Trifecta1Number1 != default)
        this.Trifectas.Add(new TrifectaPayoffItem { Number1 = payoff.Trifecta1Number1, Number2 = payoff.Trifecta2Number1, Number3 = payoff.Trifecta3Number1, Money = payoff.TrifectaNumber1Money, });
      if (payoff.Trifecta1Number2 != default)
        this.Trifectas.Add(new TrifectaPayoffItem { Number1 = payoff.Trifecta1Number2, Number2 = payoff.Trifecta2Number2, Number3 = payoff.Trifecta3Number2, Money = payoff.TrifectaNumber2Money, });
      if (payoff.Trifecta1Number3 != default)
        this.Trifectas.Add(new TrifectaPayoffItem { Number1 = payoff.Trifecta1Number3, Number2 = payoff.Trifecta2Number3, Number3 = payoff.Trifecta3Number3, Money = payoff.TrifectaNumber3Money, });
      if (payoff.Trifecta1Number4 != default)
        this.Trifectas.Add(new TrifectaPayoffItem { Number1 = payoff.Trifecta1Number4, Number2 = payoff.Trifecta2Number4, Number3 = payoff.Trifecta3Number4, Money = payoff.TrifectaNumber4Money, });
      if (payoff.Trifecta1Number5 != default)
        this.Trifectas.Add(new TrifectaPayoffItem { Number1 = payoff.Trifecta1Number5, Number2 = payoff.Trifecta2Number5, Number3 = payoff.Trifecta3Number5, Money = payoff.TrifectaNumber5Money, });
      if (payoff.Trifecta1Number6 != default)
        this.Trifectas.Add(new TrifectaPayoffItem { Number1 = payoff.Trifecta1Number6, Number2 = payoff.Trifecta2Number6, Number3 = payoff.Trifecta3Number6, Money = payoff.TrifectaNumber6Money, });
    }

    public void SetTickets(BettingTicketInfo tickets, IReadOnlyList<RaceHorseData> horses)
    {
      if (this._ticketsDisposables != null)
      {
        this._ticketsDisposables.Dispose();
      }

      this._ticketsDisposables = new();
      this._tickets = tickets;

      tickets.Tickets.CollectionChangedAsObservable()
        .Concat(Observable.FromEvent<EventHandler, EventArgs>(
            h => (s, e) => h(e),
            dele => tickets.Tickets.TicketCountChanged += dele,
            dele => tickets.Tickets.TicketCountChanged -= dele))
        .Subscribe(_ => this.UpdateTicketsData(this._tickets.Tickets, horses))
        .AddTo(this._ticketsDisposables);

      this.UpdateTicketsData(this._tickets.Tickets, horses);
    }

    public void UpdateTicketsData(IEnumerable<TicketItem> tickets, IReadOnlyList<RaceHorseData> horses)
    {
      var hits = tickets.SelectMany(t => t
        .GetHitRows(this.Payoff.Trifecta1Number1, this.Payoff.Trifecta2Number1, this.Payoff.Trifecta3Number1, this.Payoff.Frame1Number1, this.Payoff.Frame2Number1)
        .Concat(t.GetHitRows(this.Payoff.Trifecta1Number2, this.Payoff.Trifecta2Number2, this.Payoff.Trifecta3Number2, this.Payoff.Frame1Number2, this.Payoff.Frame2Number2))
        .Concat(t.GetHitRows(this.Payoff.Trifecta1Number3, this.Payoff.Trifecta2Number3, this.Payoff.Trifecta3Number3, this.Payoff.Frame1Number3, this.Payoff.Frame2Number3))
        .Concat(t.GetHitRows(this.Payoff.Trifecta1Number4, this.Payoff.Trifecta2Number4, this.Payoff.Trifecta3Number4, this.Payoff.Frame1Number3, this.Payoff.Frame2Number3))
        .Concat(t.GetHitRows(this.Payoff.Trifecta1Number5, this.Payoff.Trifecta2Number5, this.Payoff.Trifecta3Number5, this.Payoff.Frame1Number3, this.Payoff.Frame2Number3))
        .Concat(t.GetHitRows(this.Payoff.Trifecta1Number6, this.Payoff.Trifecta2Number6, this.Payoff.Trifecta3Number6, this.Payoff.Frame1Number3, this.Payoff.Frame2Number3))
        ).Distinct().ToArray();
      foreach (var item in this.Singles)
      {
        var hit = hits.Where(h => h.Number1 == item.Number1 && h.Type == TicketType.Single);
        item.IsHit.Value = hit.Any();
        item.Count.Value = hit.Any() ? hit.Sum(h => h.DataCount) : 0;
      }
      foreach (var item in this.Places)
      {
        var hit = hits.Where(h => h.Number1 == item.Number1 && h.Type == TicketType.Place);
        item.IsHit.Value = hit.Any();
        item.Count.Value = hit.Any() ? hit.Sum(h => h.DataCount) : 0;
      }
      foreach (var item in this.Frames)
      {
        var hit = hits.Where(h => h.Number1 == item.Frame1 && h.Number2 == item.Frame2 && h.Type == TicketType.FrameNumber);
        item.IsHit.Value = hit.Any();
        item.Count.Value = hit.Any() ? hit.Sum(h => h.DataCount) : 0;
      }
      foreach (var item in this.QuinellaPlaces)
      {
        var hit = hits.Where(h => h.Number1 == item.Number1 && h.Number2 == item.Number2 && h.Type == TicketType.QuinellaPlace);
        item.IsHit.Value = hit.Any();
        item.Count.Value = hit.Any() ? hit.Sum(h => h.DataCount) : 0;
      }
      foreach (var item in this.Quinellas)
      {
        var hit = hits.Where(h => h.Number1 == item.Number1 && h.Number2 == item.Number2 && h.Type == TicketType.Quinella);
        item.IsHit.Value = hit.Any();
        item.Count.Value = hit.Any() ? hit.Sum(h => h.DataCount) : 0;
      }
      foreach (var item in this.Exactas)
      {
        var hit = hits.Where(h => h.Number1 == item.Number1 && h.Number2 == item.Number2 && h.Type == TicketType.Exacta);
        item.IsHit.Value = hit.Any();
        item.Count.Value = hit.Any() ? hit.Sum(h => h.DataCount) : 0;
      }
      foreach (var item in this.Trios)
      {
        var hit = hits.Where(h => h.Number1 == item.Number1 && h.Number2 == item.Number2 && h.Number3 == item.Number3 && h.Type == TicketType.Trio);
        item.IsHit.Value = hit.Any();
        item.Count.Value = hit.Any() ? hit.Sum(h => h.DataCount) : 0;
      }
      foreach (var item in this.Trifectas)
      {
        var hit = hits.Where(h => h.Number1 == item.Number1 && h.Number2 == item.Number2 && h.Number3 == item.Number3 && h.Type == TicketType.Trifecta);
        item.IsHit.Value = hit.Any();
        item.Count.Value = hit.Any() ? hit.Sum(h => h.DataCount) : 0;
      }
      this.Singles.UpdateMoneySum();
      this.Places.UpdateMoneySum();
      this.Frames.UpdateMoneySum();
      this.QuinellaPlaces.UpdateMoneySum();
      this.Quinellas.UpdateMoneySum();
      this.Exactas.UpdateMoneySum();
      this.Trios.UpdateMoneySum();
      this.Trifectas.UpdateMoneySum();
      var itemCollections = Enumerable.Empty<PayoffItem>().Concat(this.Singles)
        .Concat(this.Places).Concat(this.Frames).Concat(this.QuinellaPlaces).Concat(this.Quinellas)
        .Concat(this.Exactas).Concat(this.Trios).Concat(this.Trifectas);

      var paySum = 0;
      var hitSum = 0;
      var returnSum = 0;
      if (tickets.Any())
      {
        paySum = tickets.Sum(t => t.Count.Value * t.CountAvailableRows(horses) * 100);
        returnSum = tickets.Sum(t => t.Count.Value * t.Rows.Count * 100) - paySum;
      }
      if (itemCollections.Any(i => i.IsHit.Value))
      {
        hitSum = itemCollections.Where(i => i.IsHit.Value).Sum(i => i.Count.Value * i.Money);
      }
      this.PayMoneySum.Value = paySum;
      this.HitMoneySum.Value = hitSum;
      this.ReturnMoneySum.Value = returnSum;
      this.Income.Value = hitSum - paySum;
      this.IncomeComparation.Value = this.Income.Value > 0 ? ValueComparation.Good : this.Income.Value < 0 ? ValueComparation.Bad : ValueComparation.Standard;
    }

    public void Dispose()
    {
      this._ticketsDisposables?.Dispose();
    }
  }

  public abstract class PayoffItem
  {
    public int Money { get; init; }

    public ReactiveProperty<int> Count { get; } = new();

    public ReactiveProperty<bool> IsHit { get; } = new();

    public ReactiveProperty<ValueComparation> Comparation { get; } = new();
  }

  public class SinglePayoffItem : PayoffItem
  {
    public short Number1 { get; init; }
  }

  public class PlacePayoffItem : PayoffItem
  {
    public short Number1 { get; init; }
  }

  public class FrameNumberPayoffItem : PayoffItem
  {
    public short Frame1 { get; init; }

    public short Frame2 { get; init; }
  }

  public class QuinellaPlacePayoffItem : PayoffItem
  {
    public short Number1 { get; init; }

    public short Number2 { get; init; }
  }

  public class QuinellaPayoffItem : PayoffItem
  {
    public short Number1 { get; init; }

    public short Number2 { get; init; }
  }

  public class ExactaPayoffItem : PayoffItem
  {
    public short Number1 { get; init; }

    public short Number2 { get; init; }
  }

  public class TrioPayoffItem : PayoffItem
  {
    public short Number1 { get; init; }

    public short Number2 { get; init; }

    public short Number3 { get; init; }
  }

  public class TrifectaPayoffItem : PayoffItem
  {
    public short Number1 { get; init; }

    public short Number2 { get; init; }

    public short Number3 { get; init; }
  }

  public class PayoffItemCollection<T> : ReactiveCollection<T> where T : PayoffItem
  {
    public ReactiveProperty<int> HitMoneySum { get; } = new();

    public void UpdateMoneySum()
    {
      var hits = this.Where(i => i.IsHit.Value);
      if (hits.Any())
      {
        this.HitMoneySum.Value = hits.Sum(i => i.Money * i.Count.Value);
      }
      else
      {
        this.HitMoneySum.Value = 0;
      }

      foreach (var item in this)
      {
        item.Comparation.Value = item.IsHit.Value ? ValueComparation.Good : ValueComparation.Standard;
      }
    }
  }
}
