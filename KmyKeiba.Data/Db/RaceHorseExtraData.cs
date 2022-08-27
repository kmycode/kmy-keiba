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
