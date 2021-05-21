using KmyKeiba.JVLink.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Data
{
  public class RaceData : DataBase<Race>
  {
    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Name6Chars { get; set; } = string.Empty;

    public string SubName { get; set; } = string.Empty;

    public RaceCourse Course { get; set; }

    public int CourseRaceNumber { get; set; }

    public string SubjectName { get; set; } = string.Empty;

    public int HorsesCount { get; set; }

    public DateTime StartTime { get; set; }

    public override void SetEntity(Race race)
    {
      this.Key = race.Key;
      this.LastModified = race.LastModified;
      this.Name = race.Name;
      this.Name6Chars = race.Name6Chars;
      this.SubName = race.SubName;
      this.SubjectName = race.Subject.Name;
      this.Course = race.Course;
      this.CourseRaceNumber = race.CourseRaceNumber;
      this.HorsesCount = race.HorsesCount;
      this.StartTime = race.StartTime;
    }

    public override bool IsEquals(DataBase<Race> b)
    {
      var c = (RaceData)b;
      return this.Key == c.Key;
    }

    public override int GetHashCode()
    {
      return this.Key.GetHashCode();
    }
  }
}
