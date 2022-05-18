using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Image;
using KmyKeiba.Models.Script;
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

    public ReactiveCollection<RaceHorseAnalyzer> Horses { get; } = new();

    public ReactiveCollection<RaceHorseAnalyzer> HorsesResultOrdered { get; } = new();

    public ReactiveProperty<RaceHorseAnalyzer?> ActiveHorse { get; } = new();

    public ReactiveCollection<RaceCorner> Corners { get; } = new();

    public RaceCourseSummaryImage CourseSummaryImage { get; } = new();

    public RaceSubjectInfo Subject { get; }

    public PayoffInfo? Payoff { get; }

    public ReactiveProperty<OddsInfo?> Odds { get; } = new();

    public ScriptManager Script { get; }

    public string Name => this.Subject.DisplayName;

    private RaceInfo(RaceData race)
    {
      this.Data = race;
      this.Subject = new(race);
      this.Script = new(this);

      this.Script.Execute();

      var details = RaceCourses.TryGetCourses(race);
      foreach (var detail in details)
      {
        this.CourseDetails.Add(detail);
      }

      this.TrendAnalyzers = new RaceTrendAnalysisSelector(race);
      this.CourseSummaryImage.Race = race;
    }

    private RaceInfo(RaceData race, PayoffInfo? payoff) : this(race)
    {
      this.Payoff = payoff;
    }

    private void SetHorsesDelay(IReadOnlyList<RaceHorseAnalyzer> horses)
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
      while (!this.Horses.Any())
      {
        await Task.Delay(10);
      }
    }

    public void SetActiveHorse(int num)
    {
      this.ActiveHorse.Value = this.Horses.FirstOrDefault(h => h.Data.Number == num);

      // 遅延でデータ読み込み
      if (this.ActiveHorse.Value?.BloodSelectors?.IsRequestedInitialization == true)
      {
        Task.Run(async () => {
          using var db = new MyContext();
          await this.ActiveHorse.Value.BloodSelectors.InitializeBloodListAsync(db);
        });
      }
    }

    public static async Task<RaceInfo?> FromKeyAsync(MyContext db, string key)
    {
      var race = await db.Races!.FirstOrDefaultAsync(r => r.Key == key);
      if (race == null)
      {
        return null;
      }

      var payoff = await db.Refunds!.FirstOrDefaultAsync(r => r.RaceKey == key);
      PayoffInfo? payoffInfo = null;
      if (payoff != null)
      {
        payoffInfo = new PayoffInfo(payoff);
      }

      return await FromDataAsync(db, race, payoffInfo);
    }

    public static Task<RaceInfo> FromDataAsync(MyContext db, RaceData race, PayoffInfo? payoff)
    {
      var info = new RaceInfo(race, payoff);

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
        var horseInfos = new List<RaceHorseAnalyzer>();
        foreach (var horse in horses)
        {
          var histories = new List<RaceHorseAnalyzer>();
          foreach (var history in horseAllHistories.Where(h => h.RaceHorse.Key == horse.Key))
          {
            var historyStandardTime = await AnalysisUtil.GetRaceStandardTimeAsync(db, history.Race);
            histories.Add(new RaceHorseAnalyzer(history.Race, history.RaceHorse, historyStandardTime));
          }

          horseInfos.Add(new RaceHorseAnalyzer(race, horse, horses, histories, standardTime)
          {
            TrendAnalyzers = new RaceHorseTrendAnalysisSelector(race, horse),
            RiderTrendAnalyzers = new RaceRiderTrendAnalysisSelector(race, horse),
            TrainerTrendAnalyzers = new RaceTrainerTrendAnalysisSelector(race, horse),
            BloodSelectors = new RaceHorseBloodTrendAnalysisSelectorMenu(race, horse),
          });
        }
        info.SetHorsesDelay(horseInfos);

        // オッズ
        var frameOdds = await db.FrameNumberOdds!.FirstOrDefaultAsync(o => o.RaceKey == race.Key);
        var quinellaPlaceOdds = await db.QuinellaPlaceOdds!.FirstOrDefaultAsync(o => o.RaceKey == race.Key);
        var quinellaOdds = await db.QuinellaOdds!.FirstOrDefaultAsync(o => o.RaceKey == race.Key);
        var exactaOdds = await db.ExactaOdds!.FirstOrDefaultAsync(o => o.RaceKey == race.Key);
        var trioOdds = await db.TrioOdds!.FirstOrDefaultAsync(o => o.RaceKey == race.Key);
        var trifectaOdds = await db.TrifectaOdds!.FirstOrDefaultAsync(o => o.RaceKey == race.Key);
        info.Odds.Value = new OddsInfo(horses, frameOdds, quinellaPlaceOdds, quinellaOdds, exactaOdds, trioOdds, trifectaOdds);

        // 調教
        var historyStartDate = race.StartTime.AddMonths(-4);
        var trainings = await db.Trainings!
          .Where(t => horseKeys.Contains(t.HorseKey) && t.StartTime <= race.StartTime && t.StartTime > historyStartDate)
          .OrderByDescending(t => t.StartTime)
          .ToArrayAsync();
        var woodTrainings = await db.WoodtipTrainings!
          .Where(t => horseKeys.Contains(t.HorseKey) && t.StartTime <= race.StartTime && t.StartTime > historyStartDate)
          .OrderByDescending(t => t.StartTime)
          .ToArrayAsync();
        foreach (var horse in horseInfos)
        {
          horse.Training.Value = new TrainingAnalyzer(
            trainings.Where(t => t.HorseKey == horse.Data.Key).Take(50).ToArray(),
            woodTrainings.Where(t => t.HorseKey == horse.Data.Key).Take(50).ToArray()
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
