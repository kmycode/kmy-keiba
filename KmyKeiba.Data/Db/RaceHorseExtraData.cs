using KmyKeiba.JVLink.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  [Index(nameof(RaceKey), nameof(Key))]
  public class RaceHorseExtraData : AppDataBase
  {
    public string RaceKey { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public short MiningTime { get; set; }

    public short MiningTimeDiffLonger { get; set; }

    public short MiningTimeDiffShorter { get; set; }

    public short MiningMatchScore { get; set; }

    public short Pci { get; set; }

    public short Pci3 { get; set; }

    public short Rpci { get; set; }

    public short After3HaronOrder { get; set; }

    public short Before3HaronTimeFixed { get; set; }

    public short BaseTime { get; set; }

    public short BaseTimeAs3Haron { get; set; }

    public short CornerOrderDiff2 { get; set; }

    public short CornerOrderDiff3 { get; set; }

    public short CornerOrderDiff4 { get; set; }

    public short CornerOrderDiffGoal { get; set; }

    public short CornerInsideCount { get; set; }

    public short CornerMiddleCount { get; set; }

    public short CornerOutsideCount { get; set; }

    public short CornerAloneCount { get; set; }

    public void SetData(string key, string raceKey, short number, MiningTime? time = null, MiningMatch? match = null)
    {
      this.Key = key;
      this.RaceKey = raceKey;

      if (time != null)
      {
        var item = time.Items.FirstOrDefault(i => i.HorseNumber == number);
        if (item.MiningTime != default)
        {
          this.MiningTime = item.MiningTime;
          this.MiningTimeDiffShorter = item.MiningTimeDiffShorter;
          this.MiningTimeDiffLonger = item.MiningTimeDiffLonger;
        }
      }
      if (match != null)
      {
        var item = match.Items.FirstOrDefault(i => i.HorseNumber == number);
        if (item.MiningScore != default)
        {
          this.MiningMatchScore = item.MiningScore;
        }
      }
    }
  }
}
