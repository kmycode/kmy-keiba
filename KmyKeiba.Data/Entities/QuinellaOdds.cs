using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  /// <summary>
  /// 馬連
  /// </summary>
  public class QuinellaOdds : EntityBase
  {
    public string RaceKey { get; set; } = string.Empty;

    public List<OddsData> Odds { get; } = new();

    public struct OddsData
    {
      public short HorseNumber1 { get; init; }

      public short HorseNumber2 { get; init; }

      public uint Odds { get; init; }
    }

    public static QuinellaOdds FromJV(JVData_Struct.JV_O2_ODDS_UMAREN odds)
    {
      var od = new QuinellaOdds
      {
        DataStatus = odds.head.DataKubun.ToDataStatus(),
        LastModified = odds.head.MakeDate.ToDateTime(),
        RaceKey = odds.id.ToRaceKey(),
      };

      int.TryParse(odds.TorokuTosu, out int horsesCount);
      foreach (var data in odds.OddsUmarenInfo
        .Where((o) => o.Odds != "000000" && o.Odds != "******" && o.Odds != "------" && !string.IsNullOrWhiteSpace(o.Odds)))
      {
        short.TryParse(data.Kumi.Substring(0, 2), out short num1);
        short.TryParse(data.Kumi.Substring(2, 2), out short num2);
        if (num1 > horsesCount || num1 <= 0 || num2 > horsesCount || num2 <= 0)
        {
          continue;
        }

        uint.TryParse(data.Odds, out uint oval);

        od.Odds.Add(new OddsData
        {
          HorseNumber1 = num1,
          HorseNumber2 = num2,
          Odds = oval,
        });
      }

      return od;
    }

    public override int GetHashCode()
      => this.RaceKey.GetHashCode();
  }
}
