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

    public List<OddsData> Odds { get; } = new();

    public struct OddsData
    {
      public int HorseNumber { get; init; }

      public float Odds { get; init; }

      public int Popular { get; init; }

      public float PlaceOddsMin { get; init; }

      public float PlaceOddsMax { get; init; }
    }

    public static SingleAndDoubleWinOdds FromJV(JVData_Struct.JV_O1_ODDS_TANFUKUWAKU odds)
    {
      var od = new SingleAndDoubleWinOdds
      {
        DataStatus = odds.head.DataKubun.ToDataStatus(),
        LastModified = odds.head.MakeDate.ToDateTime(),
        RaceKey = odds.id.ToRaceKey(),
      };

      int.TryParse(odds.TorokuTosu, out int horsesCount);
      foreach (var data in odds.OddsTansyoInfo.Join(odds.OddsFukusyoInfo, (t) => t.Umaban, (f) => f.Umaban, (t, f) => new { Single = t, Multiple = f, }))
      {
        int.TryParse(data.Single.Umaban, out int horseNumber);
        if (horseNumber > horsesCount || horseNumber <= 0)
        {
          continue;
        }

        float.TryParse(data.Single.Odds, out float oval);
        int.TryParse(data.Single.Ninki, out int popular);
        float.TryParse(data.Multiple.OddsHigh, out float oval2max);
        float.TryParse(data.Multiple.OddsLow, out float oval2min);

        od.Odds.Add(new OddsData
        {
          HorseNumber = horseNumber,
          Odds = oval / 10,
          Popular = popular,
          PlaceOddsMax = oval2max / 10,
          PlaceOddsMin = oval2min / 10,
        });
      }

      return od;
    }

    public override int GetHashCode()
      => this.RaceKey.GetHashCode();
  }
}
