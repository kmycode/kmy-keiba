using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  public class MiningMatch : EntityBase
  {
    public string RaceKey { get; set; } = string.Empty;

    public IReadOnlyList<MiningMatchItem> Items { get; set; } = Array.Empty<MiningMatchItem>();

    public static MiningMatch FromJV(JVData_Struct.JV_TM_INFO min)
    {
      var items = new List<MiningMatchItem>();
      foreach (var horse in min.TMInfo)
      {
        short.TryParse(horse.Umaban, out var number);
        short.TryParse(horse.TMScore, out var score);
        if (number == default) continue;
        items.Add(new MiningMatchItem
        {
          HorseNumber = number,
          MiningScore = score,
        });
      }

      return new()
      {
        LastModified = min.head.MakeDate.ToDateTime(),
        DataStatus = min.head.DataKubun.ToDataStatus(),
        RaceKey = min.id.ToRaceKey(),
        Items = items,
      };
    }

    public struct MiningMatchItem
    {
      public short HorseNumber { get; set; }

      public short MiningScore { get; set; }
    }
  }
}
