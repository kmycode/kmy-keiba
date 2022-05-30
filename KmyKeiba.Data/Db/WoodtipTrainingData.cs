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
  public class WoodtipTrainingData : DataBase<WoodtipTraining>
  {
    [StringLength(16)]
    public string HorseKey { get; set; } = string.Empty;

    public TrainingCenter Center { get; set; }

    public DateTime StartTime { get; set; }

    public WoodtipCourse Course { get; set; }

    public WoodtipDirection Direction { get; set; }

    public short Lap1Time { get; set; }

    public short Lap2Time { get; set; }

    public short Lap3Time { get; set; }

    public short Lap4Time { get; set; }

    public short Lap5Time { get; set; }

    public short Lap6Time { get; set; }

    public short Lap7Time { get; set; }

    public short Lap8Time { get; set; }

    public short Lap9Time { get; set; }

    public short Lap10Time { get; set; }

    public MovieStatus MovieStatus { get; set; }

    public override void SetEntity(WoodtipTraining race)
    {
      this.LastModified = race.LastModified;
      this.DataStatus = race.DataStatus;
      this.HorseKey = race.HorseKey;
      this.Center = race.Center;
      this.StartTime = race.StartTime;
      this.Course = race.Course;
      this.Direction = race.Direction;
      this.Lap1Time = race.Lap1Time;
      this.Lap2Time = race.Lap2Time;
      this.Lap3Time = race.Lap3Time;
      this.Lap4Time = race.Lap4Time;
      this.Lap5Time = race.Lap5Time;
      this.Lap6Time = race.Lap6Time;
      this.Lap7Time = race.Lap7Time;
      this.Lap8Time = race.Lap8Time;
      this.Lap9Time = race.Lap9Time;
      this.Lap10Time = race.Lap10Time;
    }

    public override bool IsEquals(DataBase<WoodtipTraining> b)
    {
      var obj = (WoodtipTrainingData)b;
      return this.HorseKey == obj.HorseKey && this.StartTime == obj.StartTime;
    }
  }
}
