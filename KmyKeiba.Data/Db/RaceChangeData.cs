using KmyKeiba.JVLink.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  public class RaceChangeData : AppDataBase
  {
    [StringLength(20)]
    public string RaceKey { get; set; } = string.Empty;

    public short HorseNumber { get; set; }

    public RaceChangeType ChangeType { get; set; }

    public static IReadOnlyList<RaceChangeData> GetData(HorseWeight entity)
    {
      var list = new List<RaceChangeData>();
      foreach (var info in entity.Infos)
      {
        list.Add(new RaceChangeData
        {
          RaceKey = entity.RaceKey,
          ChangeType = RaceChangeType.HorseWeight,
          HorseNumber = (short)info.HorseNumber,
        });
      }
      return list;
    }

    public static RaceChangeData GetData(CourseWeatherCondition entity)
    {
      return new RaceChangeData
      {
        RaceKey = entity.RaceKeyWithoutRaceNum,
        ChangeType = RaceChangeType.TrackWeatherCondition,
      };
    }

    public static RaceChangeData GetData(HorseRiderChange entity)
    {
      return new RaceChangeData
      {
        RaceKey = entity.RaceKey,
        HorseNumber = (short)entity.HorseNumber,
        ChangeType = RaceChangeType.Rider,
      };
    }

    public static RaceChangeData GetData(HorseAbnormality entity)
    {
      return new RaceChangeData
      {
        RaceKey = entity.RaceKey,
        HorseNumber = (short)entity.HorseNumber,
        ChangeType = RaceChangeType.AbnormalResult,
      };
    }

    public static RaceChangeData GetData(RaceCourseChange entity)
    {
      return new RaceChangeData
      {
        RaceKey = entity.RaceKey,
        ChangeType = RaceChangeType.Course,
      };
    }

    public static RaceChangeData GetData(RaceStartTimeChange entity)
    {
      return new RaceChangeData
      {
        RaceKey = entity.RaceKey,
        ChangeType = RaceChangeType.StartTime,
      };
    }
  }

  public enum RaceChangeType : short
  {
    Unknown = 0,
    HorseWeight = 1,
    TrackWeatherCondition = 2,
    AbnormalResult = 3,
    Rider = 4,
    StartTime = 5,
    Course = 6,
  }
}
