using KmyKeiba.JVLink.Wrappers.JVLib;
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

    public struct OddsData : IEntityBase
    {
      public DateTime LastModified { get; set; }

      public RaceDataStatus DataStatus { get; set; }

      public string RaceKey { get; set; }

      public short Frame1 { get; init; }

      public short Frame2 { get; init; }

      public float Odds { get; init; }

      public override int GetHashCode()
        => $"{this.RaceKey}{this.Frame1} {this.Frame2}".GetHashCode();
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
        .Where((o) => o.Odds != "00000" && o.Odds != "*****" && o.Odds != "-----" && !string.IsNullOrWhiteSpace(o.Odds)).OrderBy((o) => o.Odds).Take(20))
      {
        short.TryParse(data.Kumi.Substring(0, 1), out short frame1);
        short.TryParse(data.Kumi.Substring(1, 1), out short frame2);
        if (frame1 > 8 || frame1 <= 0 || frame2 > 8 || frame2 <= 0)
        {
          continue;
        }

        float.TryParse(data.Odds, out float oval);

        od.Odds.Add(new OddsData
        {
          DataStatus = od.DataStatus,
          LastModified = od.LastModified,
          RaceKey = od.RaceKey,
          Frame1 = frame1,
          Frame2 = frame2,
          Odds = oval / 10,
        });
      }

      return od;
    }

    public override int GetHashCode()
      => this.RaceKey.GetHashCode();
  }
}
