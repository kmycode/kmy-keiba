using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  public class SingleAndDoubleWinOdds : EntityBase
  {
    public string RaceKey { get; set; } = string.Empty;

    public DateTime Time { get; set; }

    public bool IsDetermined
    {
      get
      {
        var status = (OddsDataStatus)(short)this.DataStatus;
        return status == OddsDataStatus.Determined || status == OddsDataStatus.DeterminedInMonday || status == OddsDataStatus.Last || status == OddsDataStatus.Canceled;
      }
    }

    public List<OddsData> Odds { get; } = new();

    public struct OddsData
    {
      public short HorseNumber { get; init; }

      public short Odds { get; init; }

      public short Popular { get; init; }

      public short PlaceOddsMin { get; init; }

      public short PlaceOddsMax { get; init; }
    }

    public static SingleAndDoubleWinOdds FromJV(JVData_Struct.JV_O1_ODDS_TANFUKUWAKU odds)
    {
      var od = new SingleAndDoubleWinOdds
      {
        DataStatus = odds.head.DataKubun.ToDataStatus(),
        LastModified = odds.head.MakeDate.ToDateTime(),
        RaceKey = odds.id.ToRaceKey(),
        Time = odds.HappyoTime.ToDateTime(odds.id),
      };

      int.TryParse(odds.TorokuTosu, out int horsesCount);
      foreach (var data in odds.OddsTansyoInfo.Join(odds.OddsFukusyoInfo, (t) => t.Umaban, (f) => f.Umaban, (t, f) => new { Single = t, Multiple = f, }))
      {
        short.TryParse(data.Single.Umaban, out short horseNumber);
        if (horseNumber > horsesCount || horseNumber <= 0)
        {
          continue;
        }

        short.TryParse(data.Single.Odds, out short oval);
        short.TryParse(data.Single.Ninki, out short popular);
        short.TryParse(data.Multiple.OddsHigh, out short oval2max);
        short.TryParse(data.Multiple.OddsLow, out short oval2min);

        od.Odds.Add(new OddsData
        {
          HorseNumber = horseNumber,
          Odds = oval,
          Popular = popular,
          PlaceOddsMax = oval2max,
          PlaceOddsMin = oval2min,
        });
      }

      return od;
    }

    public override int GetHashCode()
      => (this.RaceKey + this.Time).GetHashCode();
  }
}
