﻿using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  /// <summary>
  /// ３連複
  /// </summary>
  public class TrifectaOdds : EntityBase
  {
    public string RaceKey { get; set; } = string.Empty;

    public List<OddsData> Odds { get; } = new();

    public struct OddsData : IEntityBase
    {
      public DateTime LastModified { get; set; }

      public RaceDataStatus DataStatus { get; set; }

      public string RaceKey { get; set; }

      public short HorseNumber1 { get; init; }

      public short HorseNumber2 { get; init; }

      public short HorseNumber3 { get; init; }

      public float Odds { get; init; }

      public override int GetHashCode()
        => $"{this.RaceKey}{this.HorseNumber1} {this.HorseNumber2} {this.HorseNumber3}".GetHashCode();
    }

    internal static TrifectaOdds FromJV(JVData_Struct.JV_O6_ODDS_SANRENTAN odds)
    {
      var od = new TrifectaOdds
      {
        DataStatus = odds.head.DataKubun.ToDataStatus(),
        LastModified = odds.head.MakeDate.ToDateTime(),
        RaceKey = odds.id.ToRaceKey(),
      };

      int.TryParse(odds.TorokuTosu, out int horsesCount);
      foreach (var data in odds.OddsSanrentanInfo
        .Where((o) => o.Odds != "0000000" && o.Odds != "*******" && o.Odds != "-------" && !string.IsNullOrWhiteSpace(o.Odds)).OrderBy((o) => o.Odds).Take(20))
      {
        short.TryParse(data.Kumi.Substring(0, 2), out short num1);
        short.TryParse(data.Kumi.Substring(2, 2), out short num2);
        short.TryParse(data.Kumi.Substring(4, 2), out short num3);
        if (num1 > horsesCount || num1 <= 0 || num2 > horsesCount || num2 <= 0 || num3 > horsesCount || num3 <= 0)
        {
          continue;
        }

        float.TryParse(data.Odds, out float oval);

        od.Odds.Add(new OddsData
        {
          DataStatus = od.DataStatus,
          LastModified = od.LastModified,
          RaceKey = od.RaceKey,
          HorseNumber1 = num1,
          HorseNumber2 = num2,
          HorseNumber3 = num3,
          Odds = oval / 10,
        });
      }

      return od;
    }

    public override int GetHashCode()
      => this.RaceKey.GetHashCode();
  }
}
