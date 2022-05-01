using KmyKeiba.JVLink.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  public class SingleOddsTimeline : DataBase<SingleAndDoubleWinOdds>
  {
    public string RaceKey { get; set; } = string.Empty;
    public DateTime Time { get; set; }
    public float Odds1 { get; set; }
    public float Odds2 { get; set; }
    public float Odds3 { get; set; }
    public float Odds4 { get; set; }
    public float Odds5 { get; set; }
    public float Odds6 { get; set; }
    public float Odds7 { get; set; }
    public float Odds8 { get; set; }
    public float Odds9 { get; set; }
    public float Odds10 { get; set; }
    public float Odds11 { get; set; }
    public float Odds12 { get; set; }
    public float Odds13 { get; set; }
    public float Odds14 { get; set; }
    public float Odds15 { get; set; }
    public float Odds16 { get; set; }
    public float Odds17 { get; set; }
    public float Odds18 { get; set; }
    public float Odds19 { get; set; }
    public float Odds20 { get; set; }
    public float Odds21 { get; set; }
    public float Odds22 { get; set; }
    public float Odds23 { get; set; }
    public float Odds24 { get; set; }
    public float Odds25 { get; set; }
    public float Odds26 { get; set; }
    public float Odds27 { get; set; }
    public float Odds28 { get; set; }

    public override void SetEntity(SingleAndDoubleWinOdds race)
    {
      this.RaceKey = race.RaceKey;
      this.Time = race.Time;
      this.Odds1 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 1).Odds;
      this.Odds2 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 2).Odds;
      this.Odds3 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 3).Odds;
      this.Odds4 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 4).Odds;
      this.Odds5 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 5).Odds;
      this.Odds6 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 6).Odds;
      this.Odds7 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 7).Odds;
      this.Odds8 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 8).Odds;
      this.Odds9 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 9).Odds;
      this.Odds10 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 10).Odds;
      this.Odds11 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 11).Odds;
      this.Odds12 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 12).Odds;
      this.Odds13 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 13).Odds;
      this.Odds14 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 14).Odds;
      this.Odds15 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 15).Odds;
      this.Odds16 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 16).Odds;
      this.Odds17 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 17).Odds;
      this.Odds18 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 18).Odds;
      this.Odds19 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 19).Odds;
      this.Odds20 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 20).Odds;
      this.Odds21 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 21).Odds;
      this.Odds22 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 22).Odds;
      this.Odds23 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 23).Odds;
      this.Odds24 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 24).Odds;
      this.Odds25 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 25).Odds;
      this.Odds26 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 26).Odds;
      this.Odds27 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 27).Odds;
      this.Odds28 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 28).Odds;
    }

    public override bool IsEquals(DataBase<SingleAndDoubleWinOdds> b)
    {
      var obj = (SingleOddsTimeline)b;
      return this.RaceKey == obj.RaceKey && this.Time == obj.Time;
    }
  }

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
