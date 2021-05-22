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

    public TrackGround TrackGround { get; set; }

    public TrackCornerDirection TrackCornerDirection { get; set; }

    public TrackType TrackType { get; set; }

    public TrackOption TrackOption { get; set; }

    public int Distance { get; set; }

    public int CourseRaceNumber { get; set; }

    public string SubjectName { get; set; } = string.Empty;

    public RaceGrade Grade { get; set; }

    public RaceSubjectType SubjectAge2 { get; set; }

    public RaceSubjectType SubjectAge3 { get; set; }

    public RaceSubjectType SubjectAge4 { get; set; }

    public RaceSubjectType SubjectAge5 { get; set; }

    public RaceSubjectType SubjectAgeYounger { get; set; }

    public int HorsesCount { get; set; }

    public DateTime StartTime { get; set; }

    public override void SetEntity(Race race)
    {
      this.Key = race.Key;
      this.LastModified = race.LastModified;
      this.DataStatus = race.DataStatus;
      this.Name = race.Name;
      this.Name6Chars = race.Name6Chars;
      this.SubName = race.SubName;
      this.SubjectName = race.Subject.Name;
      this.Course = race.Course;
      this.TrackGround = race.TrackGround;
      this.TrackCornerDirection = race.TrackCornerDirection;
      this.TrackType = race.TrackType;
      this.TrackOption = race.TrackOption;
      this.Distance = race.Distance;
      this.CourseRaceNumber = race.CourseRaceNumber;
      this.HorsesCount = race.HorsesCount;
      this.StartTime = race.StartTime;

      this.Grade = race.Subject.Grade;
      foreach (var sub in race.Subject.AgeSubjects)
      {
        switch (sub.Age)
        {
          case 2:
            this.SubjectAge2 = sub.Type;
            break;
          case 3:
            this.SubjectAge3 = sub.Type;
            break;
          case 4:
            this.SubjectAge4 = sub.Type;
            break;
          case 5:
            this.SubjectAge5 = sub.Type;
            break;
          case 6:
            this.SubjectAgeYounger = sub.Type;
            break;
        }
      }
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
