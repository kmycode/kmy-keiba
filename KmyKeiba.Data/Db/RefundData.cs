using KmyKeiba.JVLink.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  [Index(nameof(RaceKey))]
  public class RefundData : DataBase<Refund>
  {
    [StringLength(20)]
    public string RaceKey { get; set; } = string.Empty;

    public short SingleNumber1 { get; set; }

    public int SingleNumber1Money { get; set; }

    public short SingleNumber2 { get; set; }

    public int SingleNumber2Money { get; set; }

    public short SingleNumber3 { get; set; }

    public int SingleNumber3Money { get; set; }

    public short PlaceNumber1 { get; set; }

    public int PlaceNumber1Money { get; set; }

    public short PlaceNumber2 { get; set; }

    public int PlaceNumber2Money { get; set; }

    public short PlaceNumber3 { get; set; }

    public int PlaceNumber3Money { get; set; }

    public short PlaceNumber4 { get; set; }

    public int PlaceNumber4Money { get; set; }

    public short PlaceNumber5 { get; set; }

    public int PlaceNumber5Money { get; set; }

    public short Frame1Number1 { get; set; }
    public short Frame2Number1 { get; set; }

    public int FrameNumber1Money { get; set; }

    public short Frame1Number2 { get; set; }
    public short Frame2Number2 { get; set; }

    public int FrameNumber2Money { get; set; }

    public short Frame1Number3 { get; set; }
    public short Frame2Number3 { get; set; }

    public int FrameNumber3Money { get; set; }

    public short Quinella1Number1 { get; set; }
    public short Quinella2Number1 { get; set; }
    public int QuinellaNumber1Money { get; set; }
    public short Quinella1Number2 { get; set; }
    public short Quinella2Number2 { get; set; }
    public int QuinellaNumber2Money { get; set; }
    public short Quinella1Number3 { get; set; }
    public short Quinella2Number3 { get; set; }
    public int QuinellaNumber3Money { get; set; }

    public short QuinellaPlace1Number1 { get; set; }
    public short QuinellaPlace2Number1 { get; set; }
    public int QuinellaPlaceNumber1Money { get; set; }
    public short QuinellaPlace1Number2 { get; set; }
    public short QuinellaPlace2Number2 { get; set; }
    public int QuinellaPlaceNumber2Money { get; set; }
    public short QuinellaPlace1Number3 { get; set; }
    public short QuinellaPlace2Number3 { get; set; }
    public int QuinellaPlaceNumber3Money { get; set; }
    public short QuinellaPlace1Number4 { get; set; }
    public short QuinellaPlace2Number4 { get; set; }
    public int QuinellaPlaceNumber4Money { get; set; }
    public short QuinellaPlace1Number5 { get; set; }
    public short QuinellaPlace2Number5 { get; set; }
    public int QuinellaPlaceNumber5Money { get; set; }
    public short QuinellaPlace1Number6 { get; set; }
    public short QuinellaPlace2Number6 { get; set; }
    public int QuinellaPlaceNumber6Money { get; set; }
    public short QuinellaPlace1Number7 { get; set; }
    public short QuinellaPlace2Number7 { get; set; }
    public int QuinellaPlaceNumber7Money { get; set; }

    public short Exacta1Number1 { get; set; }
    public short Exacta2Number1 { get; set; }
    public int ExactaNumber1Money { get; set; }
    public short Exacta1Number2 { get; set; }
    public short Exacta2Number2 { get; set; }
    public int ExactaNumber2Money { get; set; }
    public short Exacta1Number3 { get; set; }
    public short Exacta2Number3 { get; set; }
    public int ExactaNumber3Money { get; set; }

    public short Trio1Number1 { get; set; }
    public short Trio2Number1 { get; set; }
    public short Trio3Number1 { get; set; }
    public int TrioNumber1Money { get; set; }
    public short Trio1Number2 { get; set; }
    public short Trio2Number2 { get; set; }
    public short Trio3Number2 { get; set; }
    public int TrioNumber2Money { get; set; }
    public short Trio1Number3 { get; set; }
    public short Trio2Number3 { get; set; }
    public short Trio3Number3 { get; set; }
    public int TrioNumber3Money { get; set; }

    public short Trifecta1Number1 { get; set; }
    public short Trifecta2Number1 { get; set; }
    public short Trifecta3Number1 { get; set; }
    public int TrifectaNumber1Money { get; set; }
    public short Trifecta1Number2 { get; set; }
    public short Trifecta2Number2 { get; set; }
    public short Trifecta3Number2 { get; set; }
    public int TrifectaNumber2Money { get; set; }
    public short Trifecta1Number3 { get; set; }
    public short Trifecta2Number3 { get; set; }
    public short Trifecta3Number3 { get; set; }
    public int TrifectaNumber3Money { get; set; }
    public short Trifecta1Number4 { get; set; }
    public short Trifecta2Number4 { get; set; }
    public short Trifecta3Number4 { get; set; }
    public int TrifectaNumber4Money { get; set; }
    public short Trifecta1Number5 { get; set; }
    public short Trifecta2Number5 { get; set; }
    public short Trifecta3Number5 { get; set; }
    public int TrifectaNumber5Money { get; set; }
    public short Trifecta1Number6 { get; set; }
    public short Trifecta2Number6 { get; set; }
    public short Trifecta3Number6 { get; set; }
    public int TrifectaNumber6Money { get; set; }

    public override void SetEntity(Refund race)
    {
      this.RaceKey = race.RaceKey;

      int i = 0;
      if (race.Single.Count >= i + 1)
      {
        this.SingleNumber1 = race.Single[i].HorseNumber;
        this.SingleNumber1Money = race.Single[i].Money;
      }
      i++;
      if (race.Single.Count >= i + 1)
      {
        this.SingleNumber2 = race.Single[i].HorseNumber;
        this.SingleNumber2Money = race.Single[i].Money;
      }
      i++; if (race.Single.Count >= i + 1)
      {
        this.SingleNumber3 = race.Single[i].HorseNumber;
        this.SingleNumber3Money = race.Single[i].Money;
      }
      i++;

      i = 0;
      if (race.Place.Count >= i + 1)
      {
        this.PlaceNumber1 = race.Place[i].HorseNumber;
        this.PlaceNumber1Money = race.Place[i].Money;
      }
      i++;
      if (race.Place.Count >= i + 1)
      {
        this.PlaceNumber2 = race.Place[i].HorseNumber;
        this.PlaceNumber2Money = race.Place[i].Money;
      }
      i++;
      if (race.Place.Count >= i + 1)
      {
        this.PlaceNumber3 = race.Place[i].HorseNumber;
        this.PlaceNumber3Money = race.Place[i].Money;
      }
      i++;
      if (race.Place.Count >= i + 1)
      {
        this.PlaceNumber4 = race.Place[i].HorseNumber;
        this.PlaceNumber4Money = race.Place[i].Money;
      }
      i++;
      if (race.Place.Count >= i + 1)
      {
        this.PlaceNumber5 = race.Place[i].HorseNumber;
        this.PlaceNumber5Money = race.Place[i].Money;
      }
      i++;

      i = 0;
      if (race.Quinella.Count >= i + 1)
      {
        this.Quinella1Number1 = race.Quinella[i].HorseNumber1;
        this.Quinella2Number1 = race.Quinella[i].HorseNumber2;
        this.QuinellaNumber1Money = race.Quinella[i].Money;
      }
      i++;
      if (race.Quinella.Count >= i + 1)
      {
        this.Quinella1Number2 = race.Quinella[i].HorseNumber1;
        this.Quinella2Number2 = race.Quinella[i].HorseNumber2;
        this.QuinellaNumber2Money = race.Quinella[i].Money;
      }
      i++;
      if (race.Quinella.Count >= i + 1)
      {
        this.Quinella1Number3 = race.Quinella[i].HorseNumber1;
        this.Quinella2Number3 = race.Quinella[i].HorseNumber2;
        this.QuinellaNumber3Money = race.Quinella[i].Money;
      }
      i++;

      i = 0;
      if (race.QuinellaPlace.Count >= i + 1)
      {
        this.QuinellaPlace1Number1 = race.QuinellaPlace[i].HorseNumber1;
        this.QuinellaPlace2Number1 = race.QuinellaPlace[i].HorseNumber2;
        this.QuinellaPlaceNumber1Money = race.QuinellaPlace[i].Money;
      }
      i++;
      if (race.QuinellaPlace.Count >= i + 1)
      {
        this.QuinellaPlace1Number2 = race.QuinellaPlace[i].HorseNumber1;
        this.QuinellaPlace2Number2 = race.QuinellaPlace[i].HorseNumber2;
        this.QuinellaPlaceNumber2Money = race.QuinellaPlace[i].Money;
      }
      i++;
      if (race.QuinellaPlace.Count >= i + 1)
      {
        this.QuinellaPlace1Number3 = race.QuinellaPlace[i].HorseNumber1;
        this.QuinellaPlace2Number3 = race.QuinellaPlace[i].HorseNumber2;
        this.QuinellaPlaceNumber3Money = race.QuinellaPlace[i].Money;
      }
      i++;
      if (race.QuinellaPlace.Count >= i + 1)
      {
        this.QuinellaPlace1Number4 = race.QuinellaPlace[i].HorseNumber1;
        this.QuinellaPlace2Number4 = race.QuinellaPlace[i].HorseNumber2;
        this.QuinellaPlaceNumber4Money = race.QuinellaPlace[i].Money;
      }
      i++;
      if (race.QuinellaPlace.Count >= i + 1)
      {
        this.QuinellaPlace1Number5 = race.QuinellaPlace[i].HorseNumber1;
        this.QuinellaPlace2Number5 = race.QuinellaPlace[i].HorseNumber2;
        this.QuinellaPlaceNumber5Money = race.QuinellaPlace[i].Money;
      }
      i++;
      if (race.QuinellaPlace.Count >= i + 1)
      {
        this.QuinellaPlace1Number6 = race.QuinellaPlace[i].HorseNumber1;
        this.QuinellaPlace2Number6 = race.QuinellaPlace[i].HorseNumber2;
        this.QuinellaPlaceNumber6Money = race.QuinellaPlace[i].Money;
      }
      i++;
      if (race.QuinellaPlace.Count >= i + 1)
      {
        this.QuinellaPlace1Number7 = race.QuinellaPlace[i].HorseNumber1;
        this.QuinellaPlace2Number7 = race.QuinellaPlace[i].HorseNumber2;
        this.QuinellaPlaceNumber7Money = race.QuinellaPlace[i].Money;
      }
      i++;

      i = 0;
      if (race.Exacta.Count >= i + 1)
      {
        this.Exacta1Number1 = race.Exacta[i].HorseNumber1;
        this.Exacta2Number1 = race.Exacta[i].HorseNumber2;
        this.ExactaNumber1Money = race.Exacta[i].Money;
      }
      i++;
      if (race.Exacta.Count >= i + 1)
      {
        this.Exacta1Number2 = race.Exacta[i].HorseNumber1;
        this.Exacta2Number2 = race.Exacta[i].HorseNumber2;
        this.ExactaNumber2Money = race.Exacta[i].Money;
      }
      i++;
      if (race.Exacta.Count >= i + 1)
      {
        this.Exacta1Number3 = race.Exacta[i].HorseNumber1;
        this.Exacta2Number3 = race.Exacta[i].HorseNumber2;
        this.ExactaNumber3Money = race.Exacta[i].Money;
      }
      i++;

      i = 0;
      if (race.FrameNumber.Count >= i + 1)
      {
        this.Frame1Number1 = race.FrameNumber[i].FrameNumber1;
        this.Frame2Number1 = race.FrameNumber[i].FrameNumber2;
        this.FrameNumber1Money = race.FrameNumber[i].Money;
      }
      i++;
      if (race.FrameNumber.Count >= i + 1)
      {
        this.Frame1Number2 = race.FrameNumber[i].FrameNumber1;
        this.Frame2Number2 = race.FrameNumber[i].FrameNumber2;
        this.FrameNumber2Money = race.FrameNumber[i].Money;
      }
      i++;
      if (race.FrameNumber.Count >= i + 1)
      {
        this.Frame1Number3 = race.FrameNumber[i].FrameNumber1;
        this.Frame2Number3 = race.FrameNumber[i].FrameNumber2;
        this.FrameNumber3Money = race.FrameNumber[i].Money;
      }
      i++;

      i = 0;
      if (race.Trio.Count >= i + 1)
      {
        this.Trio1Number1 = race.Trio[i].HorseNumber1;
        this.Trio2Number1 = race.Trio[i].HorseNumber2;
        this.Trio3Number1 = race.Trio[i].HorseNumber3;
        this.TrioNumber1Money = race.Trio[i].Money;
      }
      i++;
      if (race.Trio.Count >= i + 1)
      {
        this.Trio1Number2 = race.Trio[i].HorseNumber1;
        this.Trio2Number2 = race.Trio[i].HorseNumber2;
        this.Trio3Number2 = race.Trio[i].HorseNumber3;
        this.TrioNumber2Money = race.Trio[i].Money;
      }
      i++;
      if (race.Trio.Count >= i + 1)
      {
        this.Trio1Number3 = race.Trio[i].HorseNumber1;
        this.Trio2Number3 = race.Trio[i].HorseNumber2;
        this.Trio3Number3 = race.Trio[i].HorseNumber3;
        this.TrioNumber3Money = race.Trio[i].Money;
      }
      i++;

      i = 0;
      if (race.Trifecta.Count >= i + 1)
      {
        this.Trifecta1Number1 = race.Trifecta[i].HorseNumber1;
        this.Trifecta2Number1 = race.Trifecta[i].HorseNumber2;
        this.Trifecta3Number1 = race.Trifecta[i].HorseNumber3;
        this.TrifectaNumber1Money = race.Trifecta[i].Money;
      }
      i++;
      if (race.Trifecta.Count >= i + 1)
      {
        this.Trifecta1Number2 = race.Trifecta[i].HorseNumber1;
        this.Trifecta2Number2 = race.Trifecta[i].HorseNumber2;
        this.Trifecta3Number2 = race.Trifecta[i].HorseNumber3;
        this.TrifectaNumber2Money = race.Trifecta[i].Money;
      }
      i++;
      if (race.Trifecta.Count >= i + 1)
      {
        this.Trifecta1Number3 = race.Trifecta[i].HorseNumber1;
        this.Trifecta2Number3 = race.Trifecta[i].HorseNumber2;
        this.Trifecta3Number3 = race.Trifecta[i].HorseNumber3;
        this.TrifectaNumber3Money = race.Trifecta[i].Money;
      }
      i++;
      if (race.Trifecta.Count >= i + 1)
      {
        this.Trifecta1Number4 = race.Trifecta[i].HorseNumber1;
        this.Trifecta2Number4 = race.Trifecta[i].HorseNumber2;
        this.Trifecta3Number4 = race.Trifecta[i].HorseNumber3;
        this.TrifectaNumber4Money = race.Trifecta[i].Money;
      }
      i++;
      if (race.Trifecta.Count >= i + 1)
      {
        this.Trifecta1Number5 = race.Trifecta[i].HorseNumber1;
        this.Trifecta2Number5 = race.Trifecta[i].HorseNumber2;
        this.Trifecta3Number5 = race.Trifecta[i].HorseNumber3;
        this.TrifectaNumber5Money = race.Trifecta[i].Money;
      }
      i++;
      if (race.Trifecta.Count >= i + 1)
      {
        this.Trifecta1Number6 = race.Trifecta[i].HorseNumber1;
        this.Trifecta2Number6 = race.Trifecta[i].HorseNumber2;
        this.Trifecta3Number6 = race.Trifecta[i].HorseNumber3;
        this.TrifectaNumber6Money = race.Trifecta[i].Money;
      }
      i++;
    }

    public IReadOnlyList<(short, int)> GetPlaceBetsResults()
    {
      return new List<(short, int)>
      {
        (this.PlaceNumber1, this.PlaceNumber1Money),
        (this.PlaceNumber2, this.PlaceNumber2Money),
        (this.PlaceNumber3, this.PlaceNumber3Money),
        (this.PlaceNumber4, this.PlaceNumber4Money),
        (this.PlaceNumber5, this.PlaceNumber5Money),
      }.Where(d => d.Item1 != default).ToArray();
    }

    public IReadOnlyList<(short, short, int)> GetQuinellaPlaceResults()
    {
      return new List<(short, short, int)>
      {
        (this.QuinellaPlace1Number1, this.QuinellaPlace2Number1, this.QuinellaPlaceNumber1Money),
        (this.QuinellaPlace1Number2, this.QuinellaPlace2Number2, this.QuinellaPlaceNumber2Money),
        (this.QuinellaPlace1Number3, this.QuinellaPlace2Number3, this.QuinellaPlaceNumber3Money),
        (this.QuinellaPlace1Number4, this.QuinellaPlace2Number4, this.QuinellaPlaceNumber4Money),
        (this.QuinellaPlace1Number5, this.QuinellaPlace2Number5, this.QuinellaPlaceNumber5Money),
        (this.QuinellaPlace1Number6, this.QuinellaPlace2Number6, this.QuinellaPlaceNumber6Money),
        (this.QuinellaPlace1Number7, this.QuinellaPlace2Number7, this.QuinellaPlaceNumber7Money),
      }.Where(d => d.Item1 != default).ToArray();
    }

    public override int GetHashCode()
      => this.RaceKey.GetHashCode();

    public override bool IsEquals(DataBase<Refund> b)
    {
      var c = (RefundData)b;
      return c.RaceKey == this.RaceKey;
    }
  }
}
