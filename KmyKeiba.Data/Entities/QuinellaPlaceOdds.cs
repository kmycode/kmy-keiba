﻿using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  /// <summary>
  /// ワイド
  /// </summary>
  public class QuinellaPlaceOdds : EntityBase
  {
    public string RaceKey { get; set; } = string.Empty;

    public List<OddsData> Odds { get; } = new();

    public struct OddsData
    {
      public short HorseNumber1 { get; init; }

      public short HorseNumber2 { get; init; }

      public ushort PlaceOddsMin { get; init; }

      public ushort PlaceOddsMax { get; init; }
    }

    public static QuinellaPlaceOdds FromJV(JVData_Struct.JV_O3_ODDS_WIDE odds)
    {
      var od = new QuinellaPlaceOdds
      {
        DataStatus = odds.head.DataKubun.ToDataStatus(),
        LastModified = odds.head.MakeDate.ToDateTime(),
        RaceKey = odds.id.ToRaceKey(),
      };

      int.TryParse(odds.TorokuTosu, out int horsesCount);
      foreach (var data in odds.OddsWideInfo
        .Where((o) => o.OddsLow != "00000" && o.OddsLow != "*****" && o.OddsLow != "------" && !string.IsNullOrWhiteSpace(o.OddsLow)))
      {
        short.TryParse(data.Kumi.Substring(0, 2), out short num1);
        short.TryParse(data.Kumi.Substring(2, 2), out short num2);
        if (num1 > horsesCount || num1 <= 0 || num2 > horsesCount || num2 <= 0)
        {
          continue;
        }

        ushort.TryParse(data.OddsHigh, out ushort oval2max);
        ushort.TryParse(data.OddsLow, out ushort oval2min);

        od.Odds.Add(new OddsData
        {
          HorseNumber1 = num1,
          HorseNumber2 = num2,
          PlaceOddsMax = oval2max,
          PlaceOddsMin = oval2min,
        });
      }

      return od;
    }

    public override int GetHashCode()
      => this.RaceKey.GetHashCode();
  }
}
