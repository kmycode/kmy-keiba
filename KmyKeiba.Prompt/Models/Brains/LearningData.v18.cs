using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.JVLink.Wrappers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Prompt.Models.Brains
{
  class LearningDatav18
  {
    public float Weather;
    public float Course;
    public float Ground;
    public float Condition;
    public float TrackOption;
    public float Distance;
    public float Grade;
    public float Season;
    public float RiderWeight;
    public float HandiCap;
    public float Weight;
    public float WeightDiff;
    public float Sex;
    public float Age;
    // public float MyRunningStyle;
    // public float MySecondRunningStyle;
    // public float MyRunningStyleUseRate;
    // public float MySecondRunningStyleUseRate;
    public float Speed;
    public float IntervalDays;

    // 騎手の成績
    public float RiderWinRate;
    public float WeatherRiderWinRate;
    public float HorseRiderWinRate;
    public float CourseRiderWinRate;
    public float GroundRiderWinRate;
    public float DistanceRiderWinRate;
    // public float RunningStyleRiderWinRate;
    // public float SecondRunningStyleRiderWinRate;
    public float FrontRunnerRiderWinRate;
    public float StalkerRiderWinRate;
    public float SotpRiderWinRate;
    public float SaveRunnerRiderWinRate;

    // 馬の成績
    public float WinRate;
    public float SameGradeWinRate;
    public float NearDistanceWinRate;
    public float CourseWinRate;
    public float GroundWinRate;
    public float ConditionWinRate;
    public float RiderWeightWinRate;
    public float SeasonWinRate;
    public float FrontRunnerWinRate;
    public float StalkerWinRate;
    public float SotpWinRate;
    public float SaveRunnerWinRate;

    // コースの成績
    public float CourseFrameWinRate;
    // public float CourseRunningStyleWinRate;
    // public float CourseSecondRunningStyleWinRate;
    public float CourseFrontRunnerWinRate;
    public float CourseStalkerWinRate;
    public float CourseSotpWinRate;
    public float CourseSaveRunnerWinRate;

    // 調教師の成績
    public float TrainerWinRate;
    public float ConditionTrainerWinRate;
    public float CourseTrainerWinRate;

    public float MyPoint;
    public float Enemy1Point;
    public float Enemy2Point;
    public float Enemy3Point;

    // 同じレースに出てくる、各作戦をとりそうな馬の割合
    public float EnemyFrontRunnerCount;
    public float EnemyStalkerCount;
    public float EnemySotpCount;
    public float EnemySaveRunnerCount;

    // レースのペースを追加
    public float Race1RiderWinRate;
    public float Race1Weather;
    public float Race1Course;
    // public float Race1Ground;
    public float Race1Condition;
    public float Race1Distance;
    public float Race1Grade;
    public float Race1RunningStyle;
    public float Race1A3HalongTimeOrder;
    public float Race1Pace;
    public float Race1Result;
    public float Race1ResultTime;
    public float Race1Speed;
    public float Race1CornerOrder1;
    public float Race1CornerOrder2;
    public float Race1CornerOrder3;
    public float Race1CornerOrder4;

    public float Race2RiderWinRate;
    public float Race2Weather;
    public float Race2Course;
    // public float Race2Ground;
    public float Race2Condition;
    public float Race2Distance;
    public float Race2Grade;
    public float Race2RunningStyle;
    public float Race2A3HalongTimeOrder;
    public float Race2Pace;
    public float Race2Result;
    public float Race2ResultTime;
    public float Race2Speed;
    public float Race2CornerOrder1;
    public float Race2CornerOrder2;
    public float Race2CornerOrder3;
    public float Race2CornerOrder4;

    public float Race3RiderWinRate;
    public float Race3Weather;
    public float Race3Course;
    // public float Race3Ground;
    public float Race3Condition;
    public float Race3Distance;
    public float Race3Grade;
    public float Race3RunningStyle;
    public float Race3A3HalongTimeOrder;
    public float Race3Pace;
    public float Race3Result;
    public float Race3ResultTime;
    public float Race3Speed;
    public float Race3CornerOrder1;
    public float Race3CornerOrder2;
    public float Race3CornerOrder3;
    public float Race3CornerOrder4;

    public float Race4RiderWinRate;
    public float Race4Weather;
    public float Race4Course;
    // public float Race4Ground;
    public float Race4Condition;
    public float Race4Distance;
    public float Race4Grade;
    public float Race4RunningStyle;
    public float Race4A3HalongTimeOrder;
    public float Race4Pace;
    public float Race4Result;
    public float Race4ResultTime;
    public float Race4Speed;

    public float Race5RiderWinRate;
    public float Race5Weather;
    public float Race5Course;
    // public float Race5Ground;
    public float Race5Condition;
    public float Race5Distance;
    public float Race5Grade;
    public float Race5RunningStyle;
    public float Race5A3HalongTimeOrder;
    public float Race5Pace;
    public float Race5Result;
    public float Race5ResultTime;
    public float Race5Speed;

    public float Result;

    public const int VERSION = 18;

    public static async Task<LearningDatav18> CreateAsync(MyContextBase db, RaceData race, RaceHorseData horse, IEnumerable<(RaceData Race, RaceHorseData Horse)> horseHistories, IEnumerable<(RaceData Race, RaceHorseData Horse)> otherHorseHistories)
    {
      var lastYear = race.StartTime.Date.AddMonths(-15);
      var raceDate = race.StartTime.Date;
      var pastRaces = db.Races!.Where((r) => r.StartTime < raceDate && r.StartTime >= lastYear);

      var isLocal = race.Course.GetCourseType() == RaceCourseType.Local;
      var riderWinRate = (float)(await db.RaceHorses!
        .Join(pastRaces, (h) => h.RaceKey, (r) => r.Key, (h, r) => new { h.RiderName, h.ResultOrder, })
        .CountAsync((r) => r.RiderName == horse.RiderName && r.ResultOrder <= 3 && r.ResultOrder != 0)) /
        Math.Max(1, await db.RaceHorses!
            .Join(pastRaces, (h) => h.RaceKey, (r) => r.Key, (h, r) => new { h.RiderName, h.ResultOrder, })
            .CountAsync((r) => r.RiderName == horse.RiderName && r.ResultOrder != 0));
      var allHorses = await db.RaceHorses!.Where((h) => h.RaceKey == race.Key).ToArrayAsync();

      RunningStyle GetRunningStyle(IEnumerable<(RaceData Race, RaceHorseData Horse)> histories, int nth = 0)
      {
        var runningStyle = RunningStyle.Unknown;
        var runningStyles = histories
          .OrderByDescending((h) => h.Race.StartTime)
          .Take(10)
          .GroupBy((h) => h.Horse.RunningStyle);
        runningStyle = runningStyles.OrderByDescending((g) => g.Count()).ElementAtOrDefault(nth)?.Key ?? runningStyle;
        return runningStyle;
      }

      float GetIntervalDate(DateTime? from, DateTime to)
      {
        if (from == null)
        {
          return 0.4f;
        }

        var interval = (int)((to - (DateTime)from).TotalSeconds / 86400);

        var val = interval / 7;
        if (val > 5)
        {
          val = 5 + (interval - 35) / 28 * 4;
        }
        if (val > 50)
        {
          val = 50;
        }
        return val / 50f;
      }

      var runningStyle = GetRunningStyle(horseHistories);
      var secondRunningStyle = GetRunningStyle(horseHistories, 1);
      var prevRaceStartTime = horseHistories.OrderByDescending((h) => h.Race.StartTime).FirstOrDefault().Race?.StartTime;
      var d = new LearningDatav18
      {
        RiderWinRate = riderWinRate,
        Season = race.StartTime.Date.DayOfYear / 366f,
        RiderWeight = horse.RiderWeight / 80f,
        HandiCap = (horse.RiderWeight - allHorses.Average((h) => h.RiderWeight)) / 10f + 0.5f,
        Weather = (short)race.TrackWeather / 6f,
        Course = (short)race.Course / 100f,
        Ground = (short)race.TrackGround / 4f,
        Condition = (short)race.TrackCondition / 4f,
        TrackOption = (short)race.TrackOption / 6f,
        Distance = Math.Min(race.Distance, 5000) / 5000f,
        Grade = GetGrade(isLocal, race.Name, race.Grade, new RaceSubjectType[] { race.SubjectAge2, race.SubjectAge3, race.SubjectAge4, race.SubjectAge5, race.SubjectAgeYounger, }, race.SubjectName),
        Weight = horse.Weight / 999f,
        WeightDiff = (Math.Min(30f, Math.Max(-30f, horse.WeightDiff)) + 30) / 60f,
        Sex = (horse.Sex == HorseSex.Castrated ? 1.5f : (float)horse.Sex) / 2f,
        Age = Math.Min(horse.Age, (short)10) / 10f,
        // MyRunningStyle = (float)runningStyle / 4f,
        // MySecondRunningStyle = (float)secondRunningStyle / 4f,
        // MyRunningStyleUseRate = (float)horseHistories.Count((h) => h.Horse.RunningStyle == runningStyle) / Math.Max(1, horseHistories.Count()),
        // MySecondRunningStyleUseRate = (float)horseHistories.Count((h) => h.Horse.RunningStyle == secondRunningStyle) / Math.Max(1, horseHistories.Count()),
        FrontRunnerWinRate = (float)horseHistories.Count((h) => h.Horse.RunningStyle == RunningStyle.FrontRunner && h.Horse.ResultOrder != 0 && h.Horse.ResultOrder <= (h.Race.HorsesCount <= 7 ? 2 : 3)) / Math.Max(1, horseHistories.Count()),
        StalkerWinRate = (float)horseHistories.Count((h) => h.Horse.RunningStyle == RunningStyle.Stalker && h.Horse.ResultOrder != 0 && h.Horse.ResultOrder <= (h.Race.HorsesCount <= 7 ? 2 : 3)) / Math.Max(1, horseHistories.Count()),
        SotpWinRate = (float)horseHistories.Count((h) => h.Horse.RunningStyle == RunningStyle.Sotp && h.Horse.ResultOrder != 0 && h.Horse.ResultOrder <= (h.Race.HorsesCount <= 7 ? 2 : 3)) / Math.Max(1, horseHistories.Count()),
        SaveRunnerWinRate = (float)horseHistories.Count((h) => h.Horse.RunningStyle == RunningStyle.SaveRunner && h.Horse.ResultOrder != 0 && h.Horse.ResultOrder <= (h.Race.HorsesCount <= 7 ? 2 : 3)) / Math.Max(1, horseHistories.Count()),
        MyPoint = GetRaceHorsePoint(otherHorseHistories.Where((h) => h.Horse.Name == horse.Name)),
        IntervalDays = GetIntervalDate(prevRaceStartTime, race.StartTime),

        Result = (horse.ResultOrder <= (race.HorsesCount <= 7 ? 2 : 3)) ? 1f : 0f,
      };

      {
        var info = await db.RaceHorses!
          .Where((h) => h.RiderName == horse.RiderName && h.ResultOrder != 0)
          .Join(pastRaces.Where((r) => r.TrackWeather == race.TrackWeather), (h) => h.RaceKey, (r) => r.Key, (h, r) => new { h.ResultOrder, r.HorsesCount, })
          .ToArrayAsync();
        d.WeatherRiderWinRate = (float)info.Count((i) => i.HorsesCount <= 7 ? i.ResultOrder <= 2 : i.ResultOrder <= 3) /
                                Math.Max(1, info.Count());
      }
      {
        var info = await db.RaceHorses!
          .Where((h) => h.RiderName == horse.RiderName && h.Name == horse.Name && h.ResultOrder != 0)
          .Join(pastRaces, (h) => h.RaceKey, (r) => r.Key, (h, r) => new { h.ResultOrder, r.HorsesCount, })
          .ToArrayAsync();
        d.HorseRiderWinRate = (float)info.Count((i) => i.HorsesCount <= 7 ? i.ResultOrder <= 2 : i.ResultOrder <= 3) /
                                Math.Max(1, info.Count());
      }
      {
        var info = await db.RaceHorses!
          .Where((h) => h.RiderName == horse.RiderName && h.Course == horse.Course && h.ResultOrder != 0)
          .Join(pastRaces, (h) => h.RaceKey, (r) => r.Key, (h, r) => new { h.ResultOrder, r.HorsesCount, })
          .ToArrayAsync();
        d.CourseRiderWinRate = (float)info.Count((i) => i.HorsesCount <= 7 ? i.ResultOrder <= 2 : i.ResultOrder <= 3) /
                                Math.Max(1, info.Count());
      }
      {
        var info = await db.RaceHorses!
          .Where((h) => h.RiderName == horse.RiderName && h.ResultOrder != 0)
          .Join(pastRaces.Where((r) => r.TrackGround == race.TrackGround), (h) => h.RaceKey, (r) => r.Key, (h, r) => new { h.ResultOrder, r.HorsesCount, })
          .ToArrayAsync();
        d.GroundRiderWinRate = (float)info.Count((i) => i.HorsesCount <= 7 ? i.ResultOrder <= 2 : i.ResultOrder <= 3) /
                                Math.Max(1, info.Count());
      }
      {
        var info = await db.RaceHorses!
          .Where((h) => h.RiderName == horse.RiderName && h.ResultOrder != 0)
          .Join(pastRaces.Where((r) => r.Distance >= race.Distance - 100 && r.Distance <= race.Distance + 100), (h) => h.RaceKey, (r) => r.Key, (h, r) => new { h.ResultOrder, r.HorsesCount, })
          .ToArrayAsync();
        d.DistanceRiderWinRate = (float)info.Count((i) => i.HorsesCount <= 7 ? i.ResultOrder <= 2 : i.ResultOrder <= 3) /
                                Math.Max(1, info.Count());
      }
      {
        var info = await db.RaceHorses!
          .Where((h) => h.RiderName == horse.RiderName && h.ResultOrder != 0)
          .Join(pastRaces, (h) => h.RaceKey, (r) => r.Key, (h, r) => new { h.RunningStyle, h.ResultOrder, r.HorsesCount, r.StartTime })
          .OrderByDescending((h) => h.StartTime)
          .Take(2000)
          .ToArrayAsync();
        d.FrontRunnerRiderWinRate = (float)info.Count((i) => i.RunningStyle == RunningStyle.FrontRunner && i.HorsesCount <= 7 ? i.ResultOrder <= 2 : i.ResultOrder <= 3) /
                                Math.Max(1, info.Count((i) => i.RunningStyle == RunningStyle.FrontRunner));
        d.StalkerRiderWinRate = (float)info.Count((i) => i.RunningStyle == RunningStyle.Stalker && i.HorsesCount <= 7 ? i.ResultOrder <= 2 : i.ResultOrder <= 3) /
                                Math.Max(1, info.Count((i) => i.RunningStyle == RunningStyle.Stalker));
        d.SotpRiderWinRate = (float)info.Count((i) => i.RunningStyle == RunningStyle.Sotp && i.HorsesCount <= 7 ? i.ResultOrder <= 2 : i.ResultOrder <= 3) /
                                Math.Max(1, info.Count((i) => i.RunningStyle == RunningStyle.Sotp));
        d.SaveRunnerRiderWinRate = (float)info.Count((i) => i.RunningStyle == RunningStyle.SaveRunner && i.HorsesCount <= 7 ? i.ResultOrder <= 2 : i.ResultOrder <= 3) /
                                Math.Max(1, info.Count((i) => i.RunningStyle == RunningStyle.SaveRunner));
      }

      {
        var info = await db.RaceHorses!
          .Where((h) => h.Course == horse.Course && h.FrameNumber == horse.FrameNumber && h.ResultOrder != 0)
          .Join(pastRaces, (h) => h.RaceKey, (r) => r.Key, (h, r) => new { h.ResultOrder, r.HorsesCount, })
          .ToArrayAsync();
        d.CourseFrameWinRate = (float)info.Count((i) => i.HorsesCount <= 7 ? i.ResultOrder <= 2 : i.ResultOrder <= 3) /
                                Math.Max(1, info.Count());
      }
      {
        var info = await db.RaceHorses!
          .Where((h) => h.Course == horse.Course && h.ResultOrder != 0)
          .Join(pastRaces, (h) => h.RaceKey, (r) => r.Key, (h, r) => new { h.RunningStyle, h.ResultOrder, r.HorsesCount, r.StartTime, })
          .OrderByDescending((h) => h.StartTime)
          .Take(2000)
          .ToArrayAsync();
        d.CourseFrontRunnerWinRate = (float)info.Count((i) => i.RunningStyle == RunningStyle.FrontRunner && i.HorsesCount <= 7 ? i.ResultOrder <= 2 : i.ResultOrder <= 3) /
                                Math.Max(1, info.Count((i) => i.RunningStyle == RunningStyle.FrontRunner));
        d.CourseStalkerWinRate = (float)info.Count((i) => i.RunningStyle == RunningStyle.Stalker && i.HorsesCount <= 7 ? i.ResultOrder <= 2 : i.ResultOrder <= 3) /
                                Math.Max(1, info.Count((i) => i.RunningStyle == RunningStyle.Stalker));
        d.CourseSotpWinRate = (float)info.Count((i) => i.RunningStyle == RunningStyle.Sotp && i.HorsesCount <= 7 ? i.ResultOrder <= 2 : i.ResultOrder <= 3) /
                                Math.Max(1, info.Count((i) => i.RunningStyle == RunningStyle.Sotp));
        d.CourseSaveRunnerWinRate = (float)info.Count((i) => i.RunningStyle == RunningStyle.SaveRunner && i.HorsesCount <= 7 ? i.ResultOrder <= 2 : i.ResultOrder <= 3) /
                                Math.Max(1, info.Count((i) => i.RunningStyle == RunningStyle.SaveRunner));
      }

      {
        var info = await db.RaceHorses!
          .Where((h) => h.Name == horse.Name && h.ResultOrder != 0)
          .Join(pastRaces, (h) => h.RaceKey, (r) => r.Key,
            (h, r) => new { h.ResultOrder, r.HorsesCount, h.Course, r.TrackGround, r.TrackCondition, RaceName = r.Name, r.Distance, r.StartTime,
              r.Grade, r.SubjectAge2, r.SubjectAge3, r.SubjectAge4, r.SubjectAge5, r.SubjectAgeYounger, r.SubjectName, h.RiderWeight, })
          .ToArrayAsync();
        d.WinRate = (float)info.Count((i) => i.HorsesCount <= 7 ? i.ResultOrder <= 2 : i.ResultOrder <= 3) /
                                (float)Math.Max(1, info.Count());

        var a = info
          .Where((i) => GetGrade(i.Course.GetCourseType() == RaceCourseType.Local, i.RaceName, i.Grade, new RaceSubjectType[] { i.SubjectAge2, i.SubjectAge3, i.SubjectAge4, i.SubjectAge5, i.SubjectAgeYounger, }, i.SubjectName) >= d.Grade - 0.01f);
        d.SameGradeWinRate = (float)a.Count((i) => i.HorsesCount <= 7 ? i.ResultOrder <= 2 : i.ResultOrder <= 3) /
          (float)Math.Max(1, a.Count());

        a = info
          .Where((i) => i.RiderWeight >= horse.RiderWeight - 0.5f && i.RiderWeight <= horse.RiderWeight + 0.5f);
        d.RiderWeightWinRate = (float)a.Count((i) => i.HorsesCount <= 7 ? i.ResultOrder <= 2 : i.ResultOrder <= 3) /
          (float)Math.Max(1, a.Count());

        int GetSeason(int month)
        {
          var season = month / 3;
          if (season >= 4)
          {
            season = 0;     // 0冬　1春　2夏　3秋
          }
          return season;
        }
        a = info
          .Where((i) => GetSeason(i.StartTime.Month) == GetSeason(race.StartTime.Month));
        d.SeasonWinRate = (float)a.Count((i) => i.HorsesCount <= 7 ? i.ResultOrder <= 2 : i.ResultOrder <= 3) /
          (float)Math.Max(1, a.Count());

        a = info
          .Where((i) => i.Distance >= race.Distance - 200 && i.Distance <= race.Distance + 200);
        d.NearDistanceWinRate = a.Count((i) => i.HorsesCount <= 7 ? i.ResultOrder <= 2 : i.ResultOrder <= 3) /
          (float)Math.Max(1, a.Count());

        a = info.Where((i) => i.Course == race.Course);
        d.CourseWinRate = a.Count((i) => i.HorsesCount <= 7 ? i.ResultOrder <= 2 : i.ResultOrder <= 3) /
          (float)Math.Max(1, a.Count());

        a = info.Where((i) => i.TrackGround == race.TrackGround);
        d.GroundWinRate = a.Count((i) => i.HorsesCount <= 7 ? i.ResultOrder <= 2 : i.ResultOrder <= 3) /
          (float)Math.Max(1, a.Count());

        a = info.Where((i) => i.TrackCondition == race.TrackCondition);
        d.ConditionWinRate = a.Count((i) => i.HorsesCount <= 7 ? i.ResultOrder <= 2 : i.ResultOrder <= 3) /
          (float)Math.Max(1, a.Count());
      }

      {
        var info = await db.RaceHorses!
          .Where((h) => h.TrainerName == horse.TrainerName && !string.IsNullOrEmpty(h.TrainerName) && h.ResultOrder != 0)
          .Join(pastRaces, (h) => h.RaceKey, (r) => r.Key, (h, r) => new { r.Course, r.TrackCondition, h.ResultOrder, r.HorsesCount, r.StartTime, })
          .OrderByDescending((h) => h.StartTime)
          .Take(2000)
          .ToArrayAsync();
        d.TrainerWinRate = (float)info.Count((i) => i.HorsesCount <= 7 ? i.ResultOrder <= 2 : i.ResultOrder <= 3) /
          Math.Max(1, info.Count());

        var a = info.Where((i) => i.Course == race.Course);
        d.CourseTrainerWinRate = a.Count((i) => i.HorsesCount <= 7 ? i.ResultOrder <= 2 : i.ResultOrder <= 3) /
          (float)Math.Max(1, a.Count());
        
        a = info.Where((i) => i.TrackCondition == race.TrackCondition);
        d.ConditionTrainerWinRate = a.Count((i) => i.HorsesCount <= 7 ? i.ResultOrder <= 2 : i.ResultOrder <= 3) /
          (float)Math.Max(1, a.Count());
      }

      {
        var otherHorseRunningStyles = otherHorseHistories
          .Where((h) => h.Horse.Name != horse.Name)
          .GroupBy((h) => h.Horse.Name)
          .Select((h) => GetRunningStyle(h))
          .ToArray();
        d.EnemyFrontRunnerCount = (float)otherHorseRunningStyles.Count((r) => r == RunningStyle.FrontRunner) / Math.Max(1, race.HorsesCount);
        d.EnemyStalkerCount = (float)otherHorseRunningStyles.Count((r) => r == RunningStyle.Stalker) / Math.Max(1, race.HorsesCount);
        d.EnemySotpCount = (float)otherHorseRunningStyles.Count((r) => r == RunningStyle.Sotp) / Math.Max(1, race.HorsesCount);
        d.EnemySaveRunnerCount = (float)otherHorseRunningStyles.Count((r) => r == RunningStyle.SaveRunner) / Math.Max(1, race.HorsesCount);
      }

      var otherPoints = otherHorseHistories
        .Where((h) => h.Horse.Name != horse.Name)
        .GroupBy((h) => h.Horse.Name)
        .Select((g) => GetRaceHorsePoint(g))
        .OrderByDescending((p) => p)
        .Take(3)
        .ToArray();
      d.Enemy1Point = otherPoints.ElementAtOrDefault(0);
      d.Enemy2Point = otherPoints.ElementAtOrDefault(1);
      d.Enemy3Point = otherPoints.ElementAtOrDefault(2);

      var oldRaceCount = 0;
      var speedSum = 0f;

      for (var i = 1; i <= 5; i++)
      {
        var r = horseHistories.ElementAtOrDefault(i - 1);

        var fields = typeof(LearningData)
          .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
          .Where((f) => f.Name.StartsWith("Race" + i))
          .ToArray();
        if (!fields.Any())
        {
          break;
        }
        void SetValue(string fieldName, float value)
        {
          fields!.FirstOrDefault((f) => f.Name == "Race" + i + fieldName)!.SetValue(d, value);
        }
        async Task<float> GetCourseRacePaceAsync(RaceData rc)
        {
          var raceTopTime = await db.RaceHorses!
            .Where((h) => h.RaceKey == rc.Key && h.ResultOrder == 1)
            .Join(pastRaces, (h) => h.RaceKey, (r) => r.Key, (h, r) => new { h.ResultTime, })
            .FirstOrDefaultAsync();
          if (raceTopTime != null)
          {
            var sameCourseResults = await db.RaceHorses!
              .Where((h) => h.ResultOrder == 1 && h.Course == rc.Course)
              .Join(pastRaces.Where((r) => r.Distance == rc.Distance && r.TrackGround == rc.TrackGround), (h) => h.RaceKey, (r) => r.Key, (h, r) => new { h.ResultTime })
              .ToArrayAsync();
            if (sameCourseResults.Any())
            {
              var average = sameCourseResults.Average((r) => (float)r.ResultTime.TotalMilliseconds / 1000);
              var topTime = (float)raceTopTime.ResultTime.TotalMilliseconds / 300_000f;
              return (average - topTime) / 30f + 0.5f;
            }
          }

          return 0.5f;
        }

        if (r.Horse != null)
        {
          var oldRiderWinRate = (float)await db.RaceHorses!
            .Join(pastRaces, (h) => h.RaceKey, (r) => r.Key, (h, r) => new { h.ResultOrder, h.RiderName, })
            .CountAsync((rr) => rr.RiderName == r.Horse.RiderName && rr.ResultOrder <= 3 && rr.ResultOrder != 0) /
            Math.Max(1, await db.RaceHorses!
            .Join(pastRaces, (h) => h.RaceKey, (r) => r.Key, (h, r) => new { h.RiderName, h.ResultOrder, })
            .CountAsync((rr) => rr.RiderName == r.Horse.RiderName && rr.ResultOrder != 0));

          var isLocalRace = r.Race.Course.GetCourseType() == RaceCourseType.Local;
          SetValue("RiderWinRate", oldRiderWinRate);
          SetValue("Weather", (short)r.Race.TrackWeather / 6f);
          SetValue("Course", (short)r.Race.Course / 100f);
          // SetValue("Ground", (short)r.Race.TrackGround / 4f);
          SetValue("Condition", (short)r.Race.TrackCondition / 4f);
          SetValue("Distance", Math.Min(r.Race.Distance, 5000) / 5000f);
          SetValue("Grade", GetGrade(isLocalRace, r.Race.Name, r.Race.Grade, new RaceSubjectType[] { r.Race.SubjectAge2, r.Race.SubjectAge3, r.Race.SubjectAge4, r.Race.SubjectAge5, r.Race.SubjectAgeYounger, }, r.Race.SubjectName));
          SetValue("RunningStyle", (short)r.Horse.RunningStyle / 4f);
          SetValue("A3HalongTimeOrder", r.Horse.AfterThirdHalongTimeOrder == 0 ? 0.5f : (float)(r.Horse.AfterThirdHalongTimeOrder - 1) / Math.Max(1, r.Race.HorsesCount - 1));
          SetValue("Pace", await GetCourseRacePaceAsync(r.Race));
          if (r.Horse.ResultOrder != 0)
          {
            SetValue("Result", (r.Horse.ResultOrder <= (r.Race.HorsesCount <= 7 ? 2 : 3)) ? 1f : 0f);
          }
          SetValue("ResultTime", (float)r.Horse.ResultTime.TotalMilliseconds / 300_000f);

          var speed = await CalcSpeedValueAsync(db, r.Race, r.Horse);
          SetValue("Speed", speed);

          if (i <= 3)
          {
            var v4 = 1 - (float)(r.Horse.FourthCornerOrder - 1) / Math.Max(1, r.Race.HorsesCount - 1);
            var v3 = r.Horse.ThirdCornerOrder != 0 ?
              1 - (float)(r.Horse.ThirdCornerOrder - 1) / Math.Max(1, r.Race.HorsesCount - 1) : v4;
            var v2 = r.Horse.SecondCornerOrder != 0 ?
              1 - (float)(r.Horse.ThirdCornerOrder - 1) / Math.Max(1, r.Race.HorsesCount - 1) : v3;
            var v1 = r.Horse.FirstCornerOrder != 0 ?
              1 - (float)(r.Horse.ThirdCornerOrder - 1) / Math.Max(1, r.Race.HorsesCount - 1) : v2;
            SetValue("CornerOrder1", v1);
            SetValue("CornerOrder2", v2);
            SetValue("CornerOrder3", v3);
            SetValue("CornerOrder4", v4);
          }

          oldRaceCount++;
          speedSum += speed;
        }
        else
        {
          // 前走情報がないときのデフォルト値
          var isLocalRace = horse.Course.GetCourseType() == RaceCourseType.Local;
          SetValue("RiderWinRate", 0f);
          // SetValue("Weather", 0f);
          SetValue("Course", (short)horse.Course / 100f);
          // SetValue("Ground", 0f);
          // SetValue("Condition", 0f);
          // SetValue("Distance", 0f);
          SetValue("Grade", 0f);
          SetValue("RunningStyle", 0f);
          SetValue("A3HalongTimeOrder", 1f);
          SetValue("Pace", 0.5f);
          SetValue("Result", 0);
          SetValue("ResultTime", 1f);
          SetValue("Speed", 0f);
        }
      }

      if (oldRaceCount > 0)
      {
        d.Speed = speedSum / oldRaceCount;
      }
      else
      {
        d.Speed = 0.5f;
      }

      return d;
    }

    public const int RACE_BASETIME_CACHE_VERSION = 1;

    public static async Task<float> CalcSpeedValueAsync(MyContextBase db, RaceData race, RaceHorseData horse)
    {
      if (race.CourseBaseTimeCacheVersion == RACE_BASETIME_CACHE_VERSION)
      {
        if (race.CourseBaseTimeCache == 0)
        {
          return 0.5f;
        }

        var thisSpeed = (race.CourseBaseTimeCache * 10 - horse.ResultTime.TotalSeconds * 10) *
          (1 / (race.CourseBaseTimeCache * 10) * 1000) + (horse.RiderWeight - 55) * 2 +
          ((short)race.TrackCondition - 1) * 8 + 80;
        return Math.Clamp((float)thisSpeed / 180f, 0f, 1f);
      }

      var pastLimit = race.StartTime.AddMonths(-18);
      var raceDate = race.StartTime.Date;
      var sameCourseInfo = await db.RaceHorses!
        .Join(db.Races!, (h) => h.RaceKey, (r) => r.Key, (h, r) => new { Horse = h, Race = r, })
        .Where((r) => r.Race.StartTime < raceDate && r.Race.StartTime >= pastLimit)
        .Where((r) => r.Race.Course == race.Course && r.Race.CourseType == race.CourseType && r.Race.Distance == race.Distance && r.Race.TrackGround == race.TrackGround)
        .Where((r) => r.Horse.ResultOrder != 0 && r.Horse.ResultOrder <= 3 &&
          ((short)r.Race.Course >= 30 ||
            (r.Race.SubjectAge2 == RaceSubjectType.Win3 || r.Race.SubjectAge3 == RaceSubjectType.Win3 || r.Race.SubjectAge4 == RaceSubjectType.Win3 || r.Race.SubjectAge5 == RaceSubjectType.Win3 || r.Race.SubjectAgeYounger == RaceSubjectType.Win3) ||
            (r.Race.SubjectAge2 == RaceSubjectType.Win2 || r.Race.SubjectAge3 == RaceSubjectType.Win2 || r.Race.SubjectAge4 == RaceSubjectType.Win2 || r.Race.SubjectAge5 == RaceSubjectType.Win2 || r.Race.SubjectAgeYounger == RaceSubjectType.Win2)))
        .OrderByDescending((r) => r.Race.StartTime)
        .Select((r) => r.Horse.ResultTime)
        .Take(500)
        .ToArrayAsync();

      race.CourseBaseTimeCacheVersion = RACE_BASETIME_CACHE_VERSION;
      if (sameCourseInfo.Any())
      {
        var sameCourseBaseSpeed = sameCourseInfo.Average((c) => c.TotalSeconds);
        if (sameCourseBaseSpeed != 0)
        {
          var thisSpeed = (sameCourseBaseSpeed * 10 - horse.ResultTime.TotalSeconds * 10) *
            (1 / (sameCourseBaseSpeed * 10) * 1000) + (horse.RiderWeight - 55) * 2 +
            ((short)race.TrackCondition - 1) * 8 + 80;

          race.CourseBaseTimeCache = (float)sameCourseBaseSpeed;

          return Math.Clamp((float)thisSpeed / 180f, 0f, 1f);
        }
      }
      return 0.5f;
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
        val = cls switch
        {
          RaceClass.ClassA => 0.4f,
          RaceClass.ClassB => 0.3f,
          RaceClass.ClassC => 0.2f,
          RaceClass.ClassD => 0.2f,
          RaceClass.Age => 0.35f,
          RaceClass.Money => subject.Money > 0 ? Math.Min(subject.Money / 10000, 1000) / 2200f : 0.2f,
          _ => 0f,
        };
      }
      if (grade != RaceGrade.Unknown)
      {
        val = grade switch
        {
          RaceGrade.Grade1 => 0.95f,
          RaceGrade.Grade2 => 0.85f,
          RaceGrade.Grade3 => 0.85f,
          RaceGrade.NoNamedGrade => 0.8f,
          RaceGrade.NonGradeSpecial => 0.7f,
          RaceGrade.Listed => 0.7f,
          RaceGrade.LocalGrade1 => 0.6f,
          RaceGrade.LocalGrade2 => 0.5f,
          RaceGrade.LocalGrade3 => 0.5f,
          RaceGrade.LocalNoNamedGrade => 0.5f,
          RaceGrade.LocalListed => 0.5f,
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
        val = maxSubject switch
        {
          RaceSubjectType.NewComer => 0.5f,
          RaceSubjectType.Unraced => 0.5f,
          RaceSubjectType.Maiden => 0.45f,
          RaceSubjectType.Win1 => 0.6f,
          RaceSubjectType.Win2 => 0.7f,
          RaceSubjectType.Win3 => 0.8f,
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

    public string ToCacheString()
    {
      return string.Join(",", this.ToArray()) + "," + this.Result;
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
