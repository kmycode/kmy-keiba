using KmyKeiba.Data.Db;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race
{
  public class PayoffInfo
  {
    public RefundData Payoff { get; }

    public ReactiveCollection<SinglePayoffItem> Singles { get; } = new();

    public ReactiveCollection<PlacePayoffItem> Places { get; } = new();

    public ReactiveCollection<FrameNumberPayoffItem> Frames { get; } = new();

    public ReactiveCollection<QuinellaPlacePayoffItem> QuinellaPlaces { get; } = new();

    public ReactiveCollection<QuinellaPayoffItem> Quinellas { get; } = new();

    public ReactiveCollection<ExactaPayoffItem> Exactas { get; } = new();

    public ReactiveCollection<TrioPayoffItem> Trios { get; } = new();

    public ReactiveCollection<TrifectaPayoffItem> Trifectas { get; } = new();

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
  }

  public class SinglePayoffItem
  {
    public short Number1 { get; init; }

    public int Money { get; init; }
  }

  public class PlacePayoffItem
  {
    public short Number1 { get; init; }

    public int Money { get; init; }
  }

  public class FrameNumberPayoffItem
  {
    public short Frame1 { get; init; }

    public short Frame2 { get; init; }

    public int Money { get; init; }
  }

  public class QuinellaPlacePayoffItem
  {
    public short Number1 { get; init; }

    public short Number2 { get; init; }

    public int Money { get; init; }
  }

  public class QuinellaPayoffItem
  {
    public short Number1 { get; init; }

    public short Number2 { get; init; }

    public int Money { get; init; }
  }

  public class ExactaPayoffItem
  {
    public short Number1 { get; init; }

    public short Number2 { get; init; }

    public int Money { get; init; }
  }

  public class TrioPayoffItem
  {
    public short Number1 { get; init; }

    public short Number2 { get; init; }

    public short Number3 { get; init; }

    public int Money { get; init; }
  }

  public class TrifectaPayoffItem
  {
    public short Number1 { get; init; }

    public short Number2 { get; init; }

    public short Number3 { get; init; }

    public int Money { get; init; }
  }
}
