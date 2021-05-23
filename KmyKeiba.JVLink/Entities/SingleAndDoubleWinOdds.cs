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

    public List<SingleOddsData> SingleOdds { get; } = new();

    public struct SingleOddsData
    {
      public int HorseNumber { get; init; }

      public float Odds { get; init; }

      public int Popular { get; init; }
    }

    internal static SingleAndDoubleWinOdds FromJV(JVData_Struct.JV_O1_ODDS_TANFUKUWAKU odds)
    {
      var od = new SingleAndDoubleWinOdds
      {
        DataStatus = odds.head.DataKubun.ToDataStatus(),
        LastModified = odds.head.MakeDate.ToDateTime(),
        RaceKey = odds.id.ToRaceKey(),
      };

      int.TryParse(odds.TorokuTosu, out int horsesCount);
      foreach (var data in odds.OddsTansyoInfo)
      {
        int.TryParse(data.Umaban, out int horseNumber);
        if (horseNumber > horsesCount || horseNumber <= 0)
        {
          continue;
        }

        float.TryParse(data.Odds, out float oval);
        int.TryParse(data.Ninki, out int popular);
        od.SingleOdds.Add(new SingleOddsData
        {
          HorseNumber = horseNumber,
          Odds = oval / 10,
          Popular = popular,
        });
      }

      return od;
    }
  }
}
