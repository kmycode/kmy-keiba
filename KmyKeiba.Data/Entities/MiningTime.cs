using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  public class MiningTime : EntityBase
  {
    public string RaceKey { get; set; } = string.Empty;

    public IReadOnlyList<MiningTimeItem> Items { get; set; } = Array.Empty<MiningTimeItem>();

    public static MiningTime FromJV(JVData_Struct.JV_DM_INFO min)
    {
      var items = new List<MiningTimeItem>();
      foreach (var horse in min.DMInfo)
      {
        short.TryParse(horse.Umaban, out var number);
        short.TryParse(horse.DMTime, out var time);
        short.TryParse(horse.DMGosaP, out var diffp);
        short.TryParse(horse.DMGosaM, out var diffm);
        if (number == default) continue;
        items.Add(new MiningTimeItem
        {
          HorseNumber = number,
          MiningTime = (short)((time / 10000) * 600 + (time / 100 % 100) * 10 + (time % 100)),
          MiningTimeDiffShorter = diffp,
          MiningTimeDiffLonger = diffm,
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

    public struct MiningTimeItem
    {
      public short HorseNumber { get; set; }

      public short MiningTime { get; set; }

      public short MiningTimeDiffLonger { get; set; }

      public short MiningTimeDiffShorter { get; set; }
    }
  }
}
