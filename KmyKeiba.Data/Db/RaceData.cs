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
  [Index(nameof(Key), nameof(Name), nameof(Course))]
  public class RaceData : DataBase<Race>
  {
    [StringLength(20)]
    public string Key { get; set; } = string.Empty;

    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(24)]
    public string Name6Chars { get; set; } = string.Empty;

    [StringLength(120)]
    public string SubName { get; set; } = string.Empty;

    public RaceCourse Course { get; set; }
    
    [StringLength(4)]
    public string CourseType { get; set; } = string.Empty;

    public TrackGround TrackGround { get; set; }

    public TrackCornerDirection TrackCornerDirection { get; set; }

    public TrackType TrackType { get; set; }

    public TrackOption TrackOption { get; set; }

    public RaceCourseWeather TrackWeather { get; set; }

    public bool IsWeatherSetManually { get; set; }

    public RaceCourseCondition TrackCondition { get; set; }

    public bool IsConditionSetManually { get; set; }

    public short Distance { get; set; }

    public short CourseRaceNumber { get; set; }

    public string SubjectName { get; set; } = string.Empty;

    public RaceGrade Grade { get; set; }

    public RaceSubjectType SubjectAge2 { get; set; }

    public RaceSubjectType SubjectAge3 { get; set; }

    public RaceSubjectType SubjectAge4 { get; set; }

    public RaceSubjectType SubjectAge5 { get; set; }

    public RaceSubjectType SubjectAgeYounger { get; set; }

    public short HorsesCount { get; set; }

    public DateTime StartTime { get; set; }

    [StringLength(80)]
    public string Corner1Result { get; set; } = string.Empty;

    public short Corner1Position { get; set; }

    public short Corner1Number { get; set; }

    public TimeSpan Corner1LapTime { get; set; }

    [StringLength(80)]
    public string Corner2Result { get; set; } = string.Empty;

    public short Corner2Position { get; set; }

    public short Corner2Number { get; set; }

    public TimeSpan Corner2LapTime { get; set; }

    [StringLength(80)]
    public string Corner3Result { get; set; } = string.Empty;

    public short Corner3Position { get; set; }

    public short Corner3Number { get; set; }

    public TimeSpan Corner3LapTime { get; set; }

    [StringLength(80)]
    public string Corner4Result { get; set; } = string.Empty;

    public short Corner4Position { get; set; }

    public short Corner4Number { get; set; }

    public TimeSpan Corner4LapTime { get; set; }

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
      this.CourseType = race.CourseType;
      this.TrackGround = race.TrackGround;
      this.TrackCornerDirection = race.TrackCornerDirection;
      this.TrackType = race.TrackType;
      this.TrackOption = race.TrackOption;
      this.Distance = race.Distance;
      this.CourseRaceNumber = race.CourseRaceNumber;
      this.HorsesCount = race.HorsesCount;
      this.StartTime = race.StartTime;
      this.Corner1Result = race.Corner1Result;
      this.Corner1Position = race.Corner1Position;
      this.Corner1Number = race.Corner1Number;
      this.Corner1LapTime = race.Corner1LapTime;
      this.Corner2Result = race.Corner2Result;
      this.Corner2Position = race.Corner2Position;
      this.Corner2Number = race.Corner2Number;
      this.Corner2LapTime = race.Corner2LapTime;
      this.Corner3Result = race.Corner3Result;
      this.Corner3Position = race.Corner3Position;
      this.Corner3Number = race.Corner3Number;
      this.Corner3LapTime = race.Corner3LapTime;
      this.Corner4Result = race.Corner4Result;
      this.Corner4Position = race.Corner4Position;
      this.Corner4Number = race.Corner4Number;
      this.Corner4LapTime = race.Corner4LapTime;

      // NVLink（地方競馬）でRealTimeデータを取得するときに欠損していることがある
      if (race.TrackWeather != RaceCourseWeather.Unknown)
      {
        this.TrackWeather = race.TrackWeather;
        this.IsWeatherSetManually = false;
      }
      if (race.TrackCondition != RaceCourseCondition.Unknown)
      {
        this.TrackCondition = race.TrackCondition;
        this.IsConditionSetManually = false;
      }

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
