using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Image;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race
{
  public class RaceInfo
  {
    public RaceData Data { get; }

    public ReactiveCollection<RaceCourseDetail> CourseDetails { get; } = new();

    public RaceTrendAnalysisSelector TrendAnalyzers { get; }

    public ReactiveCollection<RaceHorseAnalysisData> Horses { get; } = new();

    public ReactiveCollection<RaceHorseAnalysisData> HorsesResultOrdered { get; } = new();

    public ReactiveProperty<RaceHorseAnalysisData?> ActiveHorse { get; } = new();

    public ReactiveCollection<RaceCorner> Corners { get; } = new();

    public RaceCourseSummaryImage CourseSummaryImage { get; } = new();

    public RaceSubjectInfo Subject { get; }

    public string Name => this.Subject.DisplayName;

    private RaceInfo(MyContext db, RaceData race)
    {
      this.Data = race;
      this.Subject = new(race);

      var details = RaceCourses.TryGetCourses(race);
      foreach (var detail in details)
      {
        this.CourseDetails.Add(detail);
      }

      this.TrendAnalyzers = new RaceTrendAnalysisSelector(db, race);
      this.CourseSummaryImage.Race = race;
    }

    private void SetHorsesDelay(IReadOnlyList<RaceHorseAnalysisData> horses)
    {
      ThreadUtil.InvokeOnUiThread(() =>
      {
        this.Horses.AddRangeOnScheduler(horses.OrderBy(h => h.Data.Number));
        this.HorsesResultOrdered.AddRangeOnScheduler(
          horses.Where(h => h.Data.ResultOrder > 0).OrderBy(h => h.Data.ResultOrder).Concat(
            horses.Where(h => h.Data.ResultOrder == 0).OrderBy(h => h.Data.Number).OrderBy(h => h.Data.AbnormalResult)));
      });
    }

    public async Task WaitAllSetupsAsync()
    {
      while (!this.Horses.Any() || !this.Horses.All(h => h.Rider.Value != null && h.Trainer.Value != null))
      {
        await Task.Delay(10);
      }
    }

    public void SetActiveHorse(int num)
    {
      this.ActiveHorse.Value = this.Horses.FirstOrDefault(h => h.Data.Number == num);
    }

    public static async Task<RaceInfo?> FromKeyAsync(MyContext db, string key)
    {
      var race = await db.Races!.FirstOrDefaultAsync(r => r.Key == key);
      if (race == null)
      {
        return null;
      }

      return await FromDataAsync(db, race);
    }

    public static Task<RaceInfo> FromDataAsync(MyContext db, RaceData race)
    {
      var info = new RaceInfo(db, race);

      AddCorner(info.Corners, race.Corner1Result, race.Corner1Number, race.Corner1Position, race.Corner1LapTime);
      AddCorner(info.Corners, race.Corner2Result, race.Corner2Number, race.Corner2Position, race.Corner2LapTime);
      AddCorner(info.Corners, race.Corner3Result, race.Corner3Number, race.Corner3Position, race.Corner3LapTime);
      AddCorner(info.Corners, race.Corner4Result, race.Corner4Number, race.Corner4Position, race.Corner4LapTime);

      // 以降の情報は遅延読み込み
      _ = Task.Run(async () =>
      {
        var horses = await db.RaceHorses!.Where(rh => rh.RaceKey == race.Key).ToArrayAsync();
        var horseKeys = horses.Select(h => h.Key).ToArray();
        var horseAllHistories = await db.RaceHorses!
          .Where(rh => horseKeys.Contains(rh.Key))
          .Join(db.Races!, rh => rh.RaceKey, r => r.Key, (rh, r) => new { Race = r, RaceHorse = rh, })
          .Where(d => d.Race.StartTime < race.StartTime)
          .OrderByDescending(d => d.Race.StartTime)
          .ToArrayAsync();
        var standardTime = await AnalysisUtil.GetRaceStandardTimeAsync(db, race);

        // コーナー順位の色分け
        var firstHorse = horses.FirstOrDefault(h => h.ResultOrder == 1);
        var secondHorse = horses.FirstOrDefault(h => h.ResultOrder == 2);
        var thirdHorse = horses.FirstOrDefault(h => h.ResultOrder == 3);
        ThreadUtil.InvokeOnUiThread(() =>
        {
          foreach (var corner in info.Corners)
          {
            corner.Image.SetOrders(firstHorse?.Number ?? 0, secondHorse?.Number ?? 0, thirdHorse?.Number ?? 0);
          }
        });

        // 各馬の情報
        var horseInfos = new List<RaceHorseAnalysisData>();
        foreach (var horse in horses)
        {
          var histories = new List<RaceHorseAnalysisData>();
          foreach (var history in horseAllHistories.Where(h => h.RaceHorse.Key == horse.Key))
          {
            var historyStandardTime = await AnalysisUtil.GetRaceStandardTimeAsync(db, history.Race);
            histories.Add(new RaceHorseAnalysisData(history.Race, history.RaceHorse, historyStandardTime));
          }

          horseInfos.Add(new RaceHorseAnalysisData(race, horse, horses, histories, standardTime));
        }
        info.SetHorsesDelay(horseInfos);

        // 騎手
        var historyStartDate = race.StartTime.AddYears(-1);
        var riderKeys = horses.Select(h => h.RiderCode).Where(c => !string.IsNullOrWhiteSpace(c) && c.Any(cc => cc != '0')).ToArray();
        var riderHorses = await db.RaceHorses!
          .Join(db.Races!, rh => rh.RaceKey, r => r.Key, (rh, r) => new { Race = r, RaceHorse = rh, })
          .Where(d => riderKeys.Contains(d.RaceHorse.RiderCode))
          .Where(d => d.Race.StartTime < race.StartTime && d.Race.StartTime >= historyStartDate)
          .ToArrayAsync();
        foreach (var horse in horseInfos)
        {
          var history = new List<RaceHorseAnalysisData>();
          foreach (var item in riderHorses.Where(d => d.RaceHorse.RiderCode == horse.Data.RiderCode))
          {
            var historyStandardTime = await AnalysisUtil.GetRaceStandardTimeAsync(db, item.Race);
            history.Add(new RaceHorseAnalysisData(item.Race, item.RaceHorse, historyStandardTime));
          }
          horse.Rider.Value = new RiderAnalysisData(history, horse);
        }

        // 調教師
        var trainerKeys = horses.Select(h => h.TrainerCode).Where(c => !string.IsNullOrWhiteSpace(c) && c.Any(cc => cc != '0')).ToArray();
        var trainerHorses = await db.RaceHorses!
          .Join(db.Races!, rh => rh.RaceKey, r => r.Key, (rh, r) => new { Race = r, RaceHorse = rh, })
          .Where(d => trainerKeys.Contains(d.RaceHorse.TrainerCode))
          .Where(d => d.Race.StartTime < race.StartTime && d.Race.StartTime >= historyStartDate)
          .ToArrayAsync();
        foreach (var horse in horseInfos)
        {
          var history = new List<RaceHorseAnalysisData>();
          foreach (var item in riderHorses.Where(d => d.RaceHorse.TrainerCode == horse.Data.TrainerCode))
          {
            var historyStandardTime = await AnalysisUtil.GetRaceStandardTimeAsync(db, item.Race);
            history.Add(new RaceHorseAnalysisData(item.Race, item.RaceHorse, historyStandardTime));
          }
          horse.Trainer.Value = new TrainerAnalysisData(history);
        }

        // 調教
        historyStartDate = race.StartTime.AddMonths(-4);
        var trainings = await db.Trainings!
          .Where(t => horseKeys.Contains(t.HorseKey) && t.StartTime > historyStartDate)
          .OrderByDescending(t => t.StartTime)
          .ToArrayAsync();
        var woodTrainings = await db.WoodtipTrainings!
          .Where(t => horseKeys.Contains(t.HorseKey) && t.StartTime > historyStartDate)
          .OrderByDescending(t => t.StartTime)
          .ToArrayAsync();
        foreach (var horse in horseInfos)
        {
          horse.Training.Value = new TrainingAnalysisData(
            trainings.Where(t => t.HorseKey == horse.Data.Key).Take(100).ToArray(),
            woodTrainings.Where(t => t.HorseKey == horse.Data.Key).Take(100).ToArray()
            );
        }
      });

      return Task.FromResult(info);
    }

    private static void AddCorner(IList<RaceCorner> corners, string result, int num, int pos, TimeSpan lap)
    {
      if (string.IsNullOrWhiteSpace(result))
      {
        return;
      }

      var corner = RaceCorner.FromString(result);
      corner.Number = num;
      corner.Position = pos;
      corner.LapTime = lap;

      corners.Add(corner);
    }
  }
}
