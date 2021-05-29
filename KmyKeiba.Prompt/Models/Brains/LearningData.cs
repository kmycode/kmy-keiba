using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.JVLink.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Prompt.Models.Brains
{
  class LearningData
  {
    public float RiderWinRate;
    public float Weather;
    public float Course;
    public float Ground;
    public float Condition;
    public float TrackOption;
    public float Distance;
    public float Grade;
    public float Weight;
    public float WeightDiff;
    public float NearDistanceTime;
    public float MyPoint;
    public float Enemy1Point;
    public float Enemy2Point;
    public float Enemy3Point;

    public float Race1RiderWinRate;
    public float Race1Weather;
    public float Race1Course;
    // public float Race1Ground;
    // public float Race1Condition;
    public float Race1Distance;
    public float Race1Grade;
    public float Race1RunningStyle;
    public float Race1A3HalongTimeOrder;
    public float Race1Result;
    // public float Race1ResultTime;

    public float Race2RiderWinRate;
    public float Race2Weather;
    public float Race2Course;
    // public float Race2Ground;
    // public float Race2Condition;
    public float Race2Distance;
    public float Race2Grade;
    public float Race2RunningStyle;
    public float Race2A3HalongTimeOrder;
    public float Race2Result;
    // public float Race2ResultTime;

    public float Race3RiderWinRate;
    public float Race3Weather;
    public float Race3Course;
    // public float Race3Ground;
    // public float Race3Condition;
    public float Race3Distance;
    public float Race3Grade;
    public float Race3RunningStyle;
    public float Race3A3HalongTimeOrder;
    public float Race3Result;
    // public float Race3ResultTime;

    public float Race4RiderWinRate;
    public float Race4Weather;
    public float Race4Course;
    // public float Race4Ground;
    // public float Race4Condition;
    public float Race4Distance;
    public float Race4Grade;
    public float Race4RunningStyle;
    public float Race4A3HalongTimeOrder;
    public float Race4Result;
    // public float Race4ResultTime;

    public float Race5RiderWinRate;
    public float Race5Weather;
    public float Race5Course;
    // public float Race5Ground;
    // public float Race5Condition;
    public float Race5Distance;
    public float Race5Grade;
    public float Race5RunningStyle;
    public float Race5A3HalongTimeOrder;
    public float Race5Result;
    // public float Race5ResultTime;

    public float Result;

    public static LearningData Create(RaceData race, RaceHorseData horse, IEnumerable<(RaceData Race, RaceHorseData Horse)> horseHistories, IEnumerable<(RaceData Race, RaceHorseData Horse)> otherHorseHistories, IEnumerable<RaceHorseData> riderGrades)
    {
      var isLocal = race.Course.GetCourseType() == RaceCourseType.Local;
      var riderWinRate = (float)riderGrades.Count((r) => r.RiderCode == horse.RiderCode && r.ResultOrder <= 3 && r.ResultOrder != 0) /
        Math.Max(1, riderGrades.Count((r) => r.RiderCode == horse.RiderCode && r.ResultOrder != 0));
      var nearDistanceTime = horseHistories
        .Where((h) => h.Horse.ResultTime.TotalSeconds > 0 && h.Horse.ResultOrder > 0)
        .OrderBy((h) => h.Horse.ResultTime)
        .OrderBy((h) => Math.Abs(race.Distance - h.Race.Distance))
        .Select((h) => h.Horse.ResultTime)
        .FirstOrDefault();

      var d = new LearningData
      {
        RiderWinRate = riderWinRate,
        Weather = (short)race.TrackWeather / 6f,
        Course = (short)race.Course / 100f,
        Ground = (short)race.TrackGround / 4f,
        Condition = (short)race.TrackCondition / 4f,
        TrackOption = (short)race.TrackOption / 6f,
        Distance = Math.Min(race.Distance, 5000) / 5000f,
        Grade = GetGrade(isLocal, race.Name, race.Grade, new RaceSubjectType[] { race.SubjectAge2, race.SubjectAge3, race.SubjectAge4, race.SubjectAge5, race.SubjectAgeYounger, }, race.SubjectName),
        Weight = horse.Weight / 999f,
        WeightDiff = (Math.Min(30f, Math.Max(-30f, horse.WeightDiff)) + 30) / 60f,
        NearDistanceTime = (float)nearDistanceTime.TotalMilliseconds / 6000f,
        MyPoint = GetRaceHorsePoint(otherHorseHistories.Where((h) => h.Horse.Name == horse.Name)),

        Result = (race.HorsesCount <= 7 ? horse.ResultOrder <= 2 : horse.ResultOrder <= 3) ? 1 : 0,
      };

      var otherPoints = otherHorseHistories
        .Where((h) => h.Horse.Name != horse.Name)
        .GroupBy((h) => h.Horse.Name)
        .Select((g) => GetRaceHorsePoint(g))
        .OrderByDescending((p) => p)
        .ToArray();
      d.Enemy1Point = otherPoints.ElementAtOrDefault(0);
      d.Enemy2Point = otherPoints.ElementAtOrDefault(1);
      d.Enemy3Point = otherPoints.ElementAtOrDefault(2);

      var index = 1;
      foreach (var r in horseHistories)
      {
        var fields = typeof(LearningData)
          .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
          .Where((f) => f.Name.StartsWith("Race" + index))
          .ToArray();
        if (!fields.Any())
        {
          break;
        }

        var oldRiderWinRate = (float)riderGrades.Count((rr) => rr.RiderCode == r.Horse.RiderCode && rr.ResultOrder <= 3 && rr.ResultOrder != 0) /
          Math.Max(1, riderGrades.Count((rr) => rr.RiderCode == r.Horse.RiderCode && rr.ResultOrder != 0));

        void SetValue(string fieldName, float value)
        {
          fields!.FirstOrDefault((f) => f.Name == "Race" + index + fieldName)!.SetValue(d, value);
        }

        var isLocalRace = r.Race.Course.GetCourseType() == RaceCourseType.Local;
        SetValue("RiderWinRate", oldRiderWinRate);
        SetValue("Weather", (short)r.Race.TrackWeather / 6f);
        SetValue("Course", (short)r.Race.Course / 100f);
        // SetValue("Ground", (short)r.Race.TrackGround / 4f);
        // SetValue("Condition", (short)r.Race.TrackCondition / 4f);
        SetValue("Distance", Math.Min(r.Race.Distance, 5000) / 5000f);
        SetValue("Grade", GetGrade(isLocalRace, r.Race.Name, r.Race.Grade, new RaceSubjectType[] { r.Race.SubjectAge2, r.Race.SubjectAge3, r.Race.SubjectAge4, r.Race.SubjectAge5, r.Race.SubjectAgeYounger, }, r.Race.SubjectName));
        SetValue("RunningStyle", (short)r.Horse.RunningStyle / 4f);
        SetValue("A3HalongTimeOrder", (float)(r.Horse.AfterThirdHalongTimeOrder - 1) / Math.Max(1, r.Race.HorsesCount - 1));
        SetValue("Result", (r.Race.HorsesCount <= 7 ? r.Horse.ResultOrder <= 2 : r.Horse.ResultOrder <= 3) ? 1 : 0);
        // SetValue("ResultTime", (float)r.Horse.ResultTime.TotalMilliseconds / 6000);

        index++;
      }

      return d;
    }

    private static float GetRaceHorsePoint(IEnumerable<(RaceData Race, RaceHorseData Horse)> history)
    {
      var point = 0f;
      var raceCount = 0;
      foreach (var horse in history.Where((h) => h.Horse.ResultOrder > 0).OrderByDescending((h) => h.Race.StartTime).Take(5))
      {
        var isLocalRace = horse.Race.Course.GetCourseType() == RaceCourseType.Local;
        var cls = GetGrade(isLocalRace, horse.Race.Name, horse.Race.Grade, new RaceSubjectType[] { horse.Race.SubjectAge2, horse.Race.SubjectAge3, horse.Race.SubjectAge4, horse.Race.SubjectAge5, horse.Race.SubjectAgeYounger, }, horse.Race.SubjectName);
        var order = 1f - ((float)(horse.Horse.ResultOrder - 1) / Math.Max(1, horse.Race.HorsesCount - 1));
        point += cls + order;
        raceCount++;
      }

      if (raceCount == 0)
      {
        return 0f;
      }
      else
      {
        return point / raceCount / 2;
      }
    }

    private static float GetGrade(bool isLocal, string raceName, RaceGrade grade, IEnumerable<RaceSubjectType> subjects, string subjectName)
    {
      var subject = RaceSubject.Parse(subjectName);
      var cls = subject.MaxClass;

      if (grade != RaceGrade.Unknown)
      {
        if (isLocal)
        {
          grade = grade switch
          {
            RaceGrade.Grade1 => RaceGrade.LocalGrade1,
            RaceGrade.Grade2 => RaceGrade.LocalGrade2,
            RaceGrade.Grade3 => RaceGrade.LocalGrade3,
            RaceGrade.NoNamedGrade => RaceGrade.LocalNoNamedGrade,
            RaceGrade.NonGradeSpecial => RaceGrade.LocalNonGradeSpecial,
            RaceGrade.Listed => RaceGrade.LocalListed,
            _ => grade,
          };
        }
      }

      var val = 0f;

      if (cls != RaceClass.Unknown)
      {
        val += cls switch
        {
          RaceClass.ClassA => 0.3f,
          RaceClass.ClassB => 0.2f,
          RaceClass.ClassC => 0.1f,
          RaceClass.ClassD => 0.05f,
          RaceClass.Age => 0.25f,
          RaceClass.Money => subject.Money > 0 ? Math.Min(subject.Money / 10000, 1000) / 1000f : 0.2f,
          _ => 0f,
        };
      }
      if (grade != RaceGrade.Unknown)
      {
        val += grade switch
        {
          RaceGrade.Grade1 => 0.85f,
          RaceGrade.Grade2 => 0.7f,
          RaceGrade.Grade3 => 0.6f,
          RaceGrade.NoNamedGrade => 0.6f,
          RaceGrade.NonGradeSpecial => 0.4f,
          RaceGrade.Listed => 0.4f,
          RaceGrade.LocalGrade1 => 0.1f,
          RaceGrade.LocalGrade2 => 0.05f,
          RaceGrade.LocalGrade3 => 0.025f,
          RaceGrade.LocalNoNamedGrade => 0.025f,
          RaceGrade.LocalListed => 0.025f,
          _ => 0f,
        };
        if (!isLocal && (raceName.Contains("皐月賞") || raceName.Contains("東京優駿") || raceName.Contains("菊花賞")))
        {
          val = 1.0f;
        }
      }
      if (!isLocal)
      {
        val = Math.Min(1.0f, val + 0.1f);
      }

      var availableSubjects = subjects
        .Where((s) => s != RaceSubjectType.Unknown)
        .Distinct();
      if (availableSubjects.Any())
      {
        var maxSubject = availableSubjects.FirstOrDefault();
        val -= maxSubject switch
        {
          RaceSubjectType.NewComer => 0.1f,
          RaceSubjectType.Unraced => 0.1f,
          RaceSubjectType.Maiden => 0.15f,
          RaceSubjectType.Win1 => 0.1f,
          RaceSubjectType.Win2 => 0.08f,
          RaceSubjectType.Win3 => 0.05f,
          _ => 0f,
        };
      }

      return Math.Min(val, 1.0f);
    }

    public float[] ToArray()
    {
      var fields = this.GetType().GetFields().Where((f) => f.IsPublic && !f.IsStatic);
      var values = fields
        .Where((f) => f.Name != nameof(Result))
        // .OrderBy((f) => f.Name)
        .Select((f) => f.GetValue(this))
        .Where((f) => f != null)
        .Select((f) => (float)(f!))
        .ToArray();
      return values;
    }

    public static int GetShape()
    {
      var fields = typeof(LearningData).GetFields().Where((f) => f.IsPublic && !f.IsStatic);
      return fields.Count() - 1;    // Resultを除く
    }

    public static string[] GetFieldNames()
    {
      var fields = typeof(LearningData).GetFields().Where((f) => f.IsPublic && !f.IsStatic);
      return fields.Select((f) => f.Name).Where((n) => n != "Result").ToArray();
    }
  }
}
