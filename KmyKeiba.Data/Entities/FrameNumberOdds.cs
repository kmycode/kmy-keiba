﻿using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  public class FrameNumberOdds : EntityBase
  {
    public string RaceKey { get; set; } = string.Empty;

    public List<OddsData> Odds { get; } = new();

    public struct OddsData
    {
      public short Frame1 { get; init; }

      public short Frame2 { get; init; }

      public short Odds { get; init; }
    }

    public static FrameNumberOdds FromJV(JVData_Struct.JV_O1_ODDS_TANFUKUWAKU odds)
    {
      var od = new FrameNumberOdds
      {
        DataStatus = odds.head.DataKubun.ToDataStatus(),
        LastModified = odds.head.MakeDate.ToDateTime(),
        RaceKey = odds.id.ToRaceKey(),
      };

      foreach (var data in odds.OddsWakurenInfo
        .Where((o) => o.Odds != "00000" && o.Odds != "*****" && o.Odds != "-----" && !string.IsNullOrWhiteSpace(o.Odds)))
      {
        short.TryParse(data.Kumi.Substring(0, 1), out short frame1);
        short.TryParse(data.Kumi.Substring(1, 1), out short frame2);
        if (frame1 > 8 || frame1 <= 0 || frame2 > 8 || frame2 <= 0)
        {
          continue;
        }

        short.TryParse(data.Odds, out short oval);

        od.Odds.Add(new OddsData
        {
          Frame1 = frame1,
          Frame2 = frame2,
          Odds = oval,
        });
      }

      return od;
    }

    public override int GetHashCode()
      => this.RaceKey.GetHashCode();
  }
}
