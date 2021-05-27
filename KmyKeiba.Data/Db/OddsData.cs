using KmyKeiba.JVLink.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  public class FrameNumberOddsData : DataBase<FrameNumberOdds.OddsData>
  {
    public string RaceKey { get; set; } = string.Empty;

    public short Frame1 { get; set; }

    public short Frame2 { get; set; }

    public float Odds { get; set; }

    public override void SetEntity(FrameNumberOdds.OddsData odds)
    {
      this.RaceKey = odds.RaceKey;
      this.Frame1 = odds.Frame1;
      this.Frame2 = odds.Frame2;
      this.Odds = odds.Odds;
    }

    public override bool IsEquals(DataBase<FrameNumberOdds.OddsData> b)
    {
      var c = (FrameNumberOddsData)b;
      return this.RaceKey == c.RaceKey && this.Frame1 == c.Frame1 && this.Frame2 == c.Frame2;
    }

    public override int GetHashCode()
    {
      return (this.RaceKey + this.Frame1 + this.Frame2).GetHashCode();
    }
  }

  public class QuinellaPlaceOddsData : DataBase<QuinellaPlaceOdds.OddsData>
  {
    public string RaceKey { get; set; } = string.Empty;

    public short HorseNumber1 { get; set; }

    public short HorseNumber2 { get; set; }

    public float PlaceOddsMin { get; set; }

    public float PlaceOddsMax { get; set; }

    public override void SetEntity(QuinellaPlaceOdds.OddsData odds)
    {
      this.RaceKey = odds.RaceKey;
      this.HorseNumber1 = odds.HorseNumber1;
      this.HorseNumber2 = odds.HorseNumber2;
      this.PlaceOddsMax = odds.PlaceOddsMax;
      this.PlaceOddsMin = odds.PlaceOddsMin;
    }

    public override bool IsEquals(DataBase<QuinellaPlaceOdds.OddsData> b)
    {
      var c = (QuinellaPlaceOddsData)b;
      return this.RaceKey == c.RaceKey && this.HorseNumber1 == c.HorseNumber1 && this.HorseNumber2 == c.HorseNumber2;
    }

    public override int GetHashCode()
    {
      return (this.RaceKey + this.HorseNumber1 + this.HorseNumber2).GetHashCode();
    }
  }

  public class QuinellaOddsData : DataBase<QuinellaOdds.OddsData>
  {
    public string RaceKey { get; set; } = string.Empty;

    public short HorseNumber1 { get; set; }

    public short HorseNumber2 { get; set; }

    public float Odds { get; set; }

    public override void SetEntity(QuinellaOdds.OddsData odds)
    {
      this.RaceKey = odds.RaceKey;
      this.HorseNumber1 = odds.HorseNumber1;
      this.HorseNumber2 = odds.HorseNumber2;
      this.Odds = odds.Odds;
    }

    public override bool IsEquals(DataBase<QuinellaOdds.OddsData> b)
    {
      var c = (QuinellaOddsData)b;
      return this.RaceKey == c.RaceKey && this.HorseNumber1 == c.HorseNumber1 && this.HorseNumber2 == c.HorseNumber2;
    }

    public override int GetHashCode()
    {
      return (this.RaceKey + this.HorseNumber1 + this.HorseNumber2).GetHashCode();
    }
  }

  public class ExactaOddsData : DataBase<ExactaOdds.OddsData>
  {
    public string RaceKey { get; set; } = string.Empty;

    public short HorseNumber1 { get; set; }

    public short HorseNumber2 { get; set; }

    public float Odds { get; set; }

    public override void SetEntity(ExactaOdds.OddsData odds)
    {
      this.RaceKey = odds.RaceKey;
      this.HorseNumber1 = odds.HorseNumber1;
      this.HorseNumber2 = odds.HorseNumber2;
      this.Odds = odds.Odds;
    }

    public override bool IsEquals(DataBase<ExactaOdds.OddsData> b)
    {
      var c = (ExactaOddsData)b;
      return this.RaceKey == c.RaceKey && this.HorseNumber1 == c.HorseNumber1 && this.HorseNumber2 == c.HorseNumber2;
    }

    public override int GetHashCode()
    {
      return (this.RaceKey + this.HorseNumber1 + this.HorseNumber2).GetHashCode();
    }
  }

  public class TrioOddsData : DataBase<TrioOdds.OddsData>
  {
    public string RaceKey { get; set; } = string.Empty;

    public short HorseNumber1 { get; set; }

    public short HorseNumber2 { get; set; }

    public short HorseNumber3 { get; set; }

    public float Odds { get; set; }

    public override void SetEntity(TrioOdds.OddsData odds)
    {
      this.RaceKey = odds.RaceKey;
      this.HorseNumber1 = odds.HorseNumber1;
      this.HorseNumber2 = odds.HorseNumber2;
      this.HorseNumber3 = odds.HorseNumber3;
      this.Odds = odds.Odds;
    }

    public override bool IsEquals(DataBase<TrioOdds.OddsData> b)
    {
      var c = (TrioOddsData)b;
      return this.RaceKey == c.RaceKey && this.HorseNumber1 == c.HorseNumber1 && this.HorseNumber2 == c.HorseNumber2 && this.HorseNumber3 == c.HorseNumber3;
    }

    public override int GetHashCode()
    {
      return (this.RaceKey + this.HorseNumber1 + this.HorseNumber2 + this.HorseNumber3).GetHashCode();
    }
  }

  public class TrifectaOddsData : DataBase<TrifectaOdds.OddsData>
  {
    public string RaceKey { get; set; } = string.Empty;

    public short HorseNumber1 { get; set; }

    public short HorseNumber2 { get; set; }

    public short HorseNumber3 { get; set; }

    public float Odds { get; set; }

    public override void SetEntity(TrifectaOdds.OddsData odds)
    {
      this.RaceKey = odds.RaceKey;
      this.HorseNumber1 = odds.HorseNumber1;
      this.HorseNumber2 = odds.HorseNumber2;
      this.HorseNumber3 = odds.HorseNumber3;
      this.Odds = odds.Odds;
    }

    public override bool IsEquals(DataBase<TrifectaOdds.OddsData> b)
    {
      var c = (TrifectaOddsData)b;
      return this.RaceKey == c.RaceKey && this.HorseNumber1 == c.HorseNumber1 && this.HorseNumber2 == c.HorseNumber2 && this.HorseNumber3 == c.HorseNumber3;
    }

    public override int GetHashCode()
    {
      return (this.RaceKey + this.HorseNumber1 + this.HorseNumber2 + this.HorseNumber3).GetHashCode();
    }
  }
}
