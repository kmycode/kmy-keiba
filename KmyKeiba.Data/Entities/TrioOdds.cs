﻿using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  /// <summary>
  /// 3連複
  /// </summary>
  public class TrioOdds : EntityBase
  {
    public string RaceKey { get; set; } = string.Empty;

    public List<OddsData> Odds { get; } = new();

    public struct OddsData
    {
      public short HorseNumber1 { get; init; }

      public short HorseNumber2 { get; init; }

      public short HorseNumber3 { get; init; }

      public uint Odds { get; init; }
    }

    public static TrioOdds FromJV(JVData_Struct.JV_O5_ODDS_SANREN odds)
    {
      var od = new TrioOdds
      {
        DataStatus = odds.head.DataKubun.ToDataStatus(),
        LastModified = odds.head.MakeDate.ToDateTime(),
        RaceKey = odds.id.ToRaceKey(),
      };

      int.TryParse(odds.TorokuTosu, out int horsesCount);
      foreach (var data in odds.OddsSanrenInfo
        .Where((o) => o.Odds != "000000" && o.Odds != "******" && o.Odds != "------" && !string.IsNullOrWhiteSpace(o.Odds)))
      {
        short.TryParse(data.Kumi.Substring(0, 2), out short num1);
        short.TryParse(data.Kumi.Substring(2, 2), out short num2);
        short.TryParse(data.Kumi.Substring(4, 2), out short num3);
        if (num1 > horsesCount || num1 <= 0 || num2 > horsesCount || num2 <= 0 || num3 > horsesCount || num3 <= 0)
        {
          continue;
        }

        uint.TryParse(data.Odds, out uint oval);

        od.Odds.Add(new OddsData
        {
          HorseNumber1 = num1,
          HorseNumber2 = num2,
          HorseNumber3 = num3,
          Odds = oval,
        });
      }

      return od;
    }

    public override int GetHashCode()
      => this.RaceKey.GetHashCode();
  }
}
