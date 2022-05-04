using KmyKeiba.JVLink.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  [Index(nameof(HorseKey))]
  public class TrainingData : DataBase<Training>
  {
    [StringLength(16)]
    public string HorseKey { get; set; } = string.Empty;

    public TrainingCenter Center { get; set; }

    public DateTime StartTime { get; set; }

    public short FirstLapTime { get; set; }

    public short SecondLapTime { get; set; }

    public short ThirdLapTime { get; set; }

    public short FourthLapTime { get; set; }

    public override void SetEntity(Training race)
    {
      this.LastModified = race.LastModified;
      this.DataStatus = race.DataStatus;
      this.HorseKey = race.HorseKey;
      this.Center = race.Center;
      this.StartTime = race.StartTime;
      this.FirstLapTime = race.FirstLapTime;
      this.SecondLapTime = race.SecondLapTime;
      this.ThirdLapTime = race.ThirdLapTime;
      this.FourthLapTime = race.FourthLapTime;
    }

    public override bool IsEquals(DataBase<Training> b)
    {
      var obj = (TrainingData)b;
      return this.HorseKey == obj.HorseKey && this.StartTime == obj.StartTime;
    }
  }
}
