using KmyKeiba.JVLink.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  [Index(nameof(StartTime))]
  [Index(nameof(Key))]
  [Index(nameof(Course))]
  public class RaceData : DataBase<Race>
  {
    [StringLength(20)]
    public string Key { get; set; } = string.Empty;

    [NotMapped]
    public short Kaiji
    {
      get
      {
        if (this.Key.Length >= 12)
        {
          short.TryParse(this.Key.Substring(10, 2), out var value);
          return value;
        }
        return default;
      }
    }
    public short Nichiji { get; set; }

    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(24)]
    public string Name6Chars { get; set; } = string.Empty;

    [StringLength(120)]
    public string SubName { get; set; } = string.Empty;

    public short GradeId { get; set; }

    public RaceCourse Course { get; set; }
    
    [StringLength(4)]
    public string CourseType { get; set; } = string.Empty;

    public short TrackCode { get; set; }

    public TrackGround TrackGround { get; set; }

    public TrackCornerDirection TrackCornerDirection { get; set; }

    public TrackType TrackType { get; set; }

    public TrackOption TrackOption { get; set; }

    public RaceCourseWeather TrackWeather { get; set; }

    public bool IsWeatherSetManually { get; set; }

    public RaceCourseCondition TrackCondition { get; set; }

    public bool IsConditionSetManually { get; set; }

    public RaceRiderWeightRule RiderWeight { get; set; }

    public RaceHorseAreaRule Area { get; set; }

    public RaceHorseSexRule Sex { get; set; }

    public RaceCrossRaceRule Cross { get; set; }

    public short Distance { get; set; }

    public short CourseRaceNumber { get; set; }

    public string SubjectName { get; set; } = string.Empty;

    public string SubjectDisplayInfo { get; set; } = string.Empty;

    public string SubjectInfo1 { get; set; } = string.Empty;

    public string SubjectInfo2 { get; set; } = string.Empty;

    public RaceGrade Grade { get; set; }

    public RaceSubjectType SubjectAge2 { get; set; }

    public RaceSubjectType SubjectAge3 { get; set; }

    public RaceSubjectType SubjectAge4 { get; set; }

    public RaceSubjectType SubjectAge5 { get; set; }

    public RaceSubjectType SubjectAgeYounger { get; set; }

    public short HorsesCount { get; set; }

    public short ResultHorsesCount { get; set; }

    public DateTime StartTime { get; set; }

    public int CornerPositionInfos { get; set; }

    [StringLength(80)]
    public string Corner1Result { get; set; } = string.Empty;

    [NotMapped]
    public short Corner1Position => (short)(this.CornerPositionInfos / 10_00_00_00 % 10);

    [NotMapped]
    public short Corner1Number => (short)(this.CornerPositionInfos / 1_00_00_00 % 10);

    [StringLength(80)]
    public string Corner2Result { get; set; } = string.Empty;

    [NotMapped]
    public short Corner2Position => (short)(this.CornerPositionInfos / 10_00_00 % 10);

    [NotMapped]
    public short Corner2Number => (short)(this.CornerPositionInfos / 1_00_00 % 10);

    [StringLength(80)]
    public string Corner3Result { get; set; } = string.Empty;

    [NotMapped]
    public short Corner3Position => (short)(this.CornerPositionInfos / 10_00 % 10);

    [NotMapped]
    public short Corner3Number => (short)(this.CornerPositionInfos / 1_00 % 10);

    [StringLength(80)]
    public string Corner4Result { get; set; } = string.Empty;

    [NotMapped]
    public short Corner4Position => (short)(this.CornerPositionInfos / 10 % 10);

    [NotMapped]
    public short Corner4Number => (short)(this.CornerPositionInfos / 1 % 10);

    public byte[] LapTimes { get; set; } = Array.Empty<byte>();

    public string? Memo { get; set; }

    public byte[] PrizeMoney { get; set; } = Array.Empty<byte>();

    public int PrizeMoney1 { get; set; }

    public short BeforeHaronTime3 { get; set; }

    public short BeforeHaronTime4 { get; set; }

    public short AfterHaronTime3 { get; set; }

    public short AfterHaronTime4 { get; set; }

    public short SteeplechaseMileTime { get; set; }

    public override void SetEntity(Race race)
    {
      this.Key = race.Key;
      this.LastModified = race.LastModified;
      this.DataStatus = race.DataStatus;

      if (!string.IsNullOrEmpty(this.SubjectDisplayInfo))
      {
        if (this.Name != race.Name || this.SubName != race.SubName || this.SubjectName != race.Subject.Name)
        {
          this.SubjectDisplayInfo = string.Empty;
        }
      }
      this.Name = race.Name;
      this.Name6Chars = race.Name6Chars;
      this.SubName = race.SubName;
      this.SubjectName = race.Subject.Name;
      this.GradeId = race.GradeId;
      this.Course = race.Course;
      this.CourseType = race.CourseType;
      this.Nichiji = race.Nichiji;
      this.TrackCode = race.TrackCode;
      this.TrackGround = race.TrackGround;
      this.TrackCornerDirection = race.TrackCornerDirection;
      this.TrackType = race.TrackType;
      this.TrackOption = race.TrackOption;
      this.RiderWeight = race.RiderWeight;
      this.Area = race.Area;
      this.Sex = race.Sex;
      this.Cross = race.Cross;
      this.Distance = race.Distance;
      this.CourseRaceNumber = race.CourseRaceNumber;
      this.HorsesCount = race.HorsesCount;
      this.ResultHorsesCount = race.ResultHorsesCount;
      this.StartTime = race.StartTime;
      this.CornerPositionInfos =
        race.Corner1Position * 10_00_00_00 + race.Corner1Number * 1_00_00_00 +
        race.Corner2Position * 10_00_00 + race.Corner2Number * 1_00_00 +
        race.Corner3Position * 10_00 + race.Corner3Number * 1_00 +
        race.Corner4Position * 10 + race.Corner4Number * 1;
      this.Corner1Result = race.Corner1Result;
      this.Corner2Result = race.Corner2Result;
      this.Corner3Result = race.Corner3Result;
      this.Corner4Result = race.Corner4Result;
      this.BeforeHaronTime3 = race.BeforeHaronTime3;
      this.BeforeHaronTime4 = race.BeforeHaronTime4;
      this.AfterHaronTime3 = race.AfterHaronTime3;
      this.AfterHaronTime4 = race.AfterHaronTime4;
      this.SteeplechaseMileTime = race.SteeplechaseMileTime;

      var lapTimes = new byte[race.LapTimes.Length * 2];
      for (var i = 0; i < race.LapTimes.Length; i++)
      {
        lapTimes[i * 2] = (byte)((race.LapTimes[i] >> 8) & 255);
        lapTimes[i * 2 + 1] = (byte)(race.LapTimes[i] & 255);
      }
      this.LapTimes = lapTimes;

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
      if (race.Course >= RaceCourse.Foreign)
      {
        if (this.Grade == RaceGrade.Grade1)
          this.Grade = RaceGrade.ForeignGrade1;
        if (this.Grade == RaceGrade.Grade2)
          this.Grade = RaceGrade.ForeignGrade2;
        if (this.Grade == RaceGrade.Grade3)
          this.Grade = RaceGrade.ForeignGrade3;
      }
      else if (race.Course >= RaceCourse.LocalMinValue && race.Course < RaceCourse.Foreign)
      {
        if (this.Grade == RaceGrade.Grade1)
          this.Grade = RaceGrade.LocalGrade1;
        if (this.Grade == RaceGrade.Grade2)
          this.Grade = RaceGrade.LocalGrade2;
        if (this.Grade == RaceGrade.Grade3)
          this.Grade = RaceGrade.LocalGrade3;
        if (this.Grade == RaceGrade.NoNamedGrade)
          this.Grade = RaceGrade.LocalNoNamedGrade;
        if (this.Grade == RaceGrade.NonGradeSpecial)
          this.Grade = RaceGrade.LocalNonGradeSpecial;
      }

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

      if (this.TrackOption == TrackOption.Unknown)
      {
        if (this.Course != RaceCourse.ObihiroBannei)
        {
          this.TrackOption = TrackOption.Inside;
        }
      }

      var prizeMoney = new byte[12 * 4];
      var prizeMoneyIndex = 0;
      void SetPrizeMoney(int money)
      {
        prizeMoney![prizeMoneyIndex++] = (byte)(money >> 24 & 255);
        prizeMoney![prizeMoneyIndex++] = (byte)(money >> 16 & 255);
        prizeMoney![prizeMoneyIndex++] = (byte)(money >> 8 & 255);
        prizeMoney![prizeMoneyIndex++] = (byte)(money & 255);
      }
      SetPrizeMoney(race.PrizeMoney1);
      SetPrizeMoney(race.PrizeMoney2);
      SetPrizeMoney(race.PrizeMoney3);
      SetPrizeMoney(race.PrizeMoney4);
      SetPrizeMoney(race.PrizeMoney5);
      SetPrizeMoney(race.PrizeMoney6);
      SetPrizeMoney(race.PrizeMoney7);
      SetPrizeMoney(race.ExtraPrizeMoney1);
      SetPrizeMoney(race.ExtraPrizeMoney2);
      SetPrizeMoney(race.ExtraPrizeMoney3);
      SetPrizeMoney(race.ExtraPrizeMoney4);
      SetPrizeMoney(race.ExtraPrizeMoney5);
      this.PrizeMoney = prizeMoney;
      this.PrizeMoney1 = race.PrizeMoney1;
    }

    public short[] GetLapTimes()
    {
      if (this.LapTimes == null || this.LapTimes.Length == 0)
      {
        return Array.Empty<short>();
      }

      var times = new short[this.LapTimes.Length / 2];
      for (var i = 0; i < this.LapTimes.Length; i += 2)
      {
        var time = (this.LapTimes[i] << 8) + this.LapTimes[i + 1];
        times[i / 2] = (short)time;
      }

      return times;
    }

    public int[] GetPrizeMoneys()
    {
      if (this.PrizeMoney == null || this.PrizeMoney.Length < 7 * 4)
      {
        return Array.Empty<int>();
      }

      var arr = new int[7];
      for (var i = 0; i < 7 * 4; i += 4)
      {
        arr[i / 4] = (this.PrizeMoney[i] << 24) | (this.PrizeMoney[i + 1] << 16) | (this.PrizeMoney[i + 2] << 8) | (this.PrizeMoney[i + 3]);
      }
      return arr;
    }

    public int[] GetExtraPrizeMoneys()
    {
      if (this.PrizeMoney == null || this.PrizeMoney.Length < 12 * 4)
      {
        return Array.Empty<int>();
      }

      var arr = new int[5];
      for (var i = 7 * 4; i < 12 * 4; i += 4)
      {
        arr[i / 4 - 7] = (this.PrizeMoney[i] << 24) | (this.PrizeMoney[i + 1] << 16) | (this.PrizeMoney[i + 2] << 8) | (this.PrizeMoney[i + 3]);
      }
      return arr;
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
