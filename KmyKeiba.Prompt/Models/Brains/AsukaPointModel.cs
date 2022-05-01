using KmyKeiba.Data.Db;
using KmyKeiba.Data.Wrappers;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KmyKeiba.Prompt.Models.Brains.LearningData;

namespace KmyKeiba.Prompt.Models.Brains
{
  class AsukaPointRace
  {
    public RaceData Race { get; init; } = new();

    public RefundData? Refund { get; init; }

    public List<AsukaPointHorse> Horses { get; } = new();

    public async static Task<AsukaPointRace> CreateAsync(MyContext db, RaceData race)
    {
      var refund = await db.Refunds!.FirstOrDefaultAsync((r) => r.RaceKey == race.Key);
      var horses = await db.RaceHorses!.Where((h) => h.RaceKey == race.Key).ToArrayAsync();
      var horseNames = horses.Select((h) => h.Name).ToArray();

      var pastRaces = db.Races!.Where((r) => r.StartTime < race.StartTime).OrderByDescending((r) => r.StartTime);
      var pastRaceHorses = pastRaces
        .Join(db.RaceHorses!, (r) => r.Key, (h) => h.RaceKey, (r, h) => new { Race = r, Horse = h, });
      var history = await pastRaceHorses
        .Where((h) => horseNames.Contains(h.Horse.Name))
        .ToArrayAsync();

      RaceHorseData[] pastBattleOrderList;
      try
      {
        pastBattleOrderList = history
          .Select((h) => h.Horse)
          .GroupBy((h) => h.Name)
          .Select((h) => h.First())
          .Where((h) => history.GroupBy((hh) => hh.Race.Key).Any((r) => r.Count() >= 2 && r.Any((rh) => rh.Horse.Name == h.Name)))
          .OrderBy((h) => h, new HorseHistoryResultComparer(history.Select((h) => h.Horse)))
          .ToArray();
      }
      catch
      {
        pastBattleOrderList = Array.Empty<RaceHorseData>();
      }

      var points = new AsukaPointRace
      {
        Race = race,
        Refund = refund,
      };
      foreach (var horse in horses)
      {
        var point = await AsukaPointHorse.CreateAsync(db, race, horse, history.Select((h) => (h.Race, h.Horse)), pastBattleOrderList);
        points.Horses.Add(point);
      }
      return points;
    }
  }

  class AsukaPointHorse
  {
    public RaceHorseData Horse { get; init; } = new();

    public float Prediction;

    public float HistoryPointsAverage;
    public float Momentum;
    public float Momentum2;
    public float PastBattleOrder;

    public static async Task<AsukaPointHorse> CreateAsync(MyContext db,
            RaceData race,
            RaceHorseData horse,
            IEnumerable<(RaceData Race, RaceHorseData Horse)> allHorsesHistory,
            IEnumerable<RaceHorseData> pastBattleOrderList)
    {
      var pastRaces = db.Races!.Where((r) => r.StartTime < race.StartTime).OrderByDescending((r) => r.StartTime);
      var pastRaceHorses = pastRaces
        .Join(db.RaceHorses!, (r) => r.Key, (h) => h.RaceKey, (r, h) => new { Race = r, Horse = h, });

      var reliability = 1f - (float)(horse.Popular) / Math.Max(1, race.HorsesCount - 1);

      // 過去のレース結果
      var history = allHorsesHistory
        .Where((h) => h.Horse.Name == horse.Name && h.Horse.AbnormalResult == RaceAbnormality.Unknown)
        .ToArray();
      var historyPoints = history
        .Take(10)
        .Select((h) => GetRaceHorsePoint(h.Race, h.Horse))
        .ToArray();
      var historyPointsWeights = new float[] { 0.15f, 0.15f, 0.15f, 0.15f, 0.1f, 0.1f, 0.05f, 0.05f, 0.05f, 0.05f, };
      var historyPointsAverage = history.Any() ? historyPoints
        .Take(historyPointsWeights.Length)
        .Select((v, i) => v * historyPointsWeights[i])
        .Sum() / historyPointsWeights.Take(historyPoints.Count()).Sum() : 0f;

      // 勢い
      var momentumWeights = new float[] { 0.3f, 0.25f, 0.2f, 0.15f, 0.1f, };
      var momentum = historyPoints.Any() ? historyPoints
        .Take(momentumWeights.Length)
        .Select((v, i) => v * momentumWeights[i])
        .Sum() / momentumWeights.Take(historyPoints.Count()).Sum() : 0f;

      // 勢い２
      var momentum2Weights = new float[] { 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, };
      var momentum2 = history.Any() ? history
        .Take(momentum2Weights.Length)
        .Select((h, i) => h.Horse.ResultOrder <= 3 ? momentum2Weights[i] * GetGrade(h.Race) : 0)
        .Sum() / 1f : 0f;

      // 対戦表
      var pastBattleOrderIndex = pastBattleOrderList
        .Select((v, i) => new { Value = v, Index = i, })
        .FirstOrDefault((i) => i.Value.Name == horse.Name)?.Index ?? -1f;
      if (pastBattleOrderIndex < 0)
      {
        if (pastBattleOrderList.Count() > 0)
        {
          pastBattleOrderIndex = pastBattleOrderList.Count() - 1;
        }
        else
        {
          pastBattleOrderIndex = 1;
        }
      }
      var pastBattleOrder = 1 - pastBattleOrderIndex / Math.Max(1f, pastBattleOrderList.Count());

      var d = new AsukaPointHorse
      {
        Horse = horse,
        Prediction = historyPointsAverage +
          momentum +
          momentum2 * 0.1f +
          pastBattleOrder * 0.3f,
        HistoryPointsAverage = historyPointsAverage,
        Momentum = momentum,
        Momentum2 = momentum2,
        PastBattleOrder = pastBattleOrder,
      };
      d.Prediction *= reliability * 0.3f + 0.7f;
      return d;
    }

    private static float GetRaceHorsePoint(RaceData race, RaceHorseData horse)
    {
      var isLocalRace = race.Course.GetCourseType() == RaceCourseType.Local;
      var cls = GetGrade(isLocalRace, race.Name, race.Grade, new RaceSubjectType[] { race.SubjectAge2, race.SubjectAge3, race.SubjectAge4, race.SubjectAge5, race.SubjectAgeYounger, }, race.SubjectName);
      var order = 1f - (float)(horse.ResultOrder - 1) / Math.Max(1, race.HorsesCount - 1);
      var reliability = 1f - (float)(horse.Popular) / Math.Max(1, race.HorsesCount - 1);
      return (cls * 0.4f + order * 0.6f) * (reliability * 0.3f + 0.7f);
    }

    private static float GetGrade(RaceData race)
      => GetGrade(race.Course.GetCourseType() == RaceCourseType.Local,
        race.Name,
        race.Grade,
        new[] { race.SubjectAge2, race.SubjectAge3, race.SubjectAge4, race.SubjectAge5, race.SubjectAgeYounger, },
        race.SubjectName);

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
  }
}
