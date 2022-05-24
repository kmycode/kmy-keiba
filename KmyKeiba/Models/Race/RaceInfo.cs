using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Image;
using KmyKeiba.Models.Script;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race
{
  public class RaceInfo : IDisposable
  {
    private readonly CompositeDisposable _disposables = new();

    public RaceData Data { get; }

    public ReactiveProperty<RaceAnalyzer?> RaceAnalyzer { get; } = new();

    public ReactiveProperty<bool> HasResults { get; } = new();

    public ReactiveProperty<bool> HasHorses { get; } = new();

    public ReactiveProperty<bool> IsLoadCompleted { get; } = new();

    public ReactiveProperty<bool> IsLoadError { get; } = new();

    public bool CanChangeWeathers { get; }

    public bool IsCanceled => this.Data.DataStatus == RaceDataStatus.Canceled;

    public ReactiveProperty<RaceCourseWeather> Weather { get; } = new();

    public ReactiveProperty<RaceCourseCondition> Condition { get; } = new();

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

    public ReactiveProperty<BettingTicketInfo?> Tickets { get; } = new();

    public ScriptManager Script { get; }

    public ReactiveProperty<bool> CanExecuteScript { get; } = new();

    public double TimeDeviationValue => this.HorsesResultOrdered.FirstOrDefault()?.ResultTimeDeviationValue ?? 0;

    public double A3HTimeDeviationValue => this.HorsesResultOrdered.FirstOrDefault()?.A3HResultTimeDeviationValue ?? 0;

    public string Name => this.Subject.DisplayName;

    private RaceInfo(RaceData race)
    {
      this.Data = race;
      this.Subject = new(race);
      this.Script = new(this);

      this.Weather.Value = race.TrackWeather;
      this.Condition.Value = race.TrackCondition;
      this.CanChangeWeathers = race.TrackWeather == RaceCourseWeather.Unknown || race.IsWeatherSetManually;

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

    private void SetHorsesDelay(IReadOnlyList<RaceHorseAnalyzer> horses, RaceStandardTimeMasterData standardTime)
    {
      ThreadUtil.InvokeOnUiThread(() =>
      {
        this.Horses.AddRangeOnScheduler(horses.OrderBy(h => h.Data.Number));
        this.HorsesResultOrdered.AddRangeOnScheduler(
          horses.Where(h => h.Data.ResultOrder > 0).OrderBy(h => h.Data.ResultOrder).Concat(
            horses.Where(h => h.Data.ResultOrder == 0).OrderBy(h => h.Data.Number).OrderBy(h => h.Data.AbnormalResult)));
        this.RaceAnalyzer.Value = new RaceAnalyzer(this.Data, horses.Select(h => h.Data).ToArray(), standardTime);

        this.HasResults.Value = this.Horses.Any(h => h.Data.ResultOrder > 0);
        this.HasHorses.Value = true;
      });
    }

    public async Task WaitHorsesSetupAsync()
    {
      while (!this.Horses.Any())
      {
        await Task.Delay(10);
      }
    }

    public void WaitTicketsAndCallback(Action<BettingTicketInfo> callback)
    {
      if (this.Tickets.Value != null)
      {
        callback(this.Tickets.Value);
      }
      else
      {
        IDisposable? disposable = null;
        disposable = this.Tickets.Where(t => t != null).Subscribe(t =>
        {
          disposable?.Dispose();
          callback(t!);
        }).AddTo(this._disposables);
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

    public async Task SetWeatherAsync(string key)
    {
      await this.WaitHorsesSetupAsync();

      short.TryParse(key, out var value);

      using var db = new MyContext();
      db.Races!.Attach(this.Data);

      this.Weather.Value = this.Data.TrackWeather = (RaceCourseWeather)value;
      this.Data.IsWeatherSetManually = true;

      await db.SaveChangesAsync();
    }

    public async Task SetConditionAsync(string key)
    {
      await this.WaitHorsesSetupAsync();

      short.TryParse(key, out var value);

      using var db = new MyContext();
      db.Races!.Attach(this.Data);

      this.Condition.Value = this.Data.TrackCondition = (RaceCourseCondition)value;
      this.Data.IsConditionSetManually = true;

      await db.SaveChangesAsync();
    }

    public void Dispose()
    {
      this._disposables.Dispose();
      this.RaceAnalyzer.Value?.Dispose();
      this.TrendAnalyzers.Dispose();
      foreach (var h in this.Horses)
      {
        h.Dispose();
      }
      this.Odds.Value?.Dispose();
      this.Tickets.Value?.Dispose();
      this.Payoff?.Dispose();
    }

    public static async Task<RaceInfo?> FromKeyAsync(string key)
    {
      using var db = new MyContext();

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

      return await FromDataAsync(race, payoffInfo);
    }

    public static Task<RaceInfo> FromDataAsync(RaceData race, PayoffInfo? payoff)
    {
      var db = new MyContext();

      var info = new RaceInfo(race, payoff);

      AddCorner(info.Corners, race.Corner1Result, race.Corner1Number, race.Corner1Position, race.Corner1LapTime);
      AddCorner(info.Corners, race.Corner2Result, race.Corner2Number, race.Corner2Position, race.Corner2LapTime);
      AddCorner(info.Corners, race.Corner3Result, race.Corner3Number, race.Corner3Position, race.Corner3LapTime);
      AddCorner(info.Corners, race.Corner4Result, race.Corner4Number, race.Corner4Position, race.Corner4LapTime);

      // 以降の情報は遅延読み込み
      _ = Task.Run(async () =>
      {
        try
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
          var horseHistoryKeys = horseAllHistories.Select(h => h.RaceHorse.RaceKey).ToArray();
          var horseHistorySameHorses = await db.RaceHorses!
            .Where(h => h.ResultOrder >= 1 && h.ResultOrder <= 5)
            .Where(h => horseHistoryKeys.Contains(h.RaceKey))
            .ToArrayAsync();
          foreach (var horse in horses)
          {
            var histories = new List<RaceHorseAnalyzer>();
            foreach (var history in horseAllHistories.Where(h => h.RaceHorse.Key == horse.Key))
            {
              var historyStandardTime = await AnalysisUtil.GetRaceStandardTimeAsync(db, history.Race);
              var sameHorses = horseHistorySameHorses.Where(h => h.RaceKey == history.RaceHorse.RaceKey);
              histories.Add(new RaceHorseAnalyzer(history.Race, history.RaceHorse, sameHorses.ToArray(), historyStandardTime));
            }

            horseInfos.Add(new RaceHorseAnalyzer(race, horse, horses, histories, standardTime)
            {
              TrendAnalyzers = new RaceHorseTrendAnalysisSelector(race, horse),
              RiderTrendAnalyzers = new RaceRiderTrendAnalysisSelector(race, horse),
              TrainerTrendAnalyzers = new RaceTrainerTrendAnalysisSelector(race, horse),
              BloodSelectors = new RaceHorseBloodTrendAnalysisSelectorMenu(race, horse),
            });
          }
          {
            // タイム指数の相対評価
            var timedvMax = horseInfos.Where(i => (i.History?.TimeDeviationValue ?? default) != default).OrderByDescending(i => i.History?.TimeDeviationValue ?? 0.0).Skip(2).FirstOrDefault()?.History?.TimeDeviationValue;
            var timedvMin = horseInfos.Where(i => (i.History?.TimeDeviationValue ?? default) != default).OrderBy(i => i.History?.TimeDeviationValue ?? 0.0).Skip(2).FirstOrDefault()?.History?.TimeDeviationValue;
            var a3htimedvMax = horseInfos.Where(i => (i.History?.A3HTimeDeviationValue ?? default) != default).OrderByDescending(i => i.History?.A3HTimeDeviationValue ?? 0.0).Skip(2).FirstOrDefault()?.History?.A3HTimeDeviationValue;
            var a3htimedvMin = horseInfos.Where(i => (i.History?.A3HTimeDeviationValue ?? default) != default).OrderBy(i => i.History?.A3HTimeDeviationValue ?? 0.0).Skip(2).FirstOrDefault()?.History?.A3HTimeDeviationValue;
            foreach (var horse in horseInfos)
            {
              if (horse.History != null && timedvMax != null && timedvMin != null)
              {
                horse.History.TimeDVComparation = horse.History.TimeDeviationValue + 2 >= timedvMax ? ValueComparation.Good :
                  horse.History.TimeDeviationValue - 2 <= timedvMin ? ValueComparation.Bad : ValueComparation.Standard;
              }
              if (horse.History != null && a3htimedvMax != null && a3htimedvMin != null)
              {
                horse.History.A3HTimeDVComparation = horse.History.A3HTimeDeviationValue + 2 >= a3htimedvMax ? ValueComparation.Good :
                  horse.History.A3HTimeDeviationValue - 2 <= a3htimedvMin ? ValueComparation.Bad : ValueComparation.Standard;
              }
            }
          }
          info.SetHorsesDelay(horseInfos, standardTime);

          // オッズ
          var frameOdds = await db.FrameNumberOdds!.FirstOrDefaultAsync(o => o.RaceKey == race.Key);
          var quinellaPlaceOdds = await db.QuinellaPlaceOdds!.FirstOrDefaultAsync(o => o.RaceKey == race.Key);
          var quinellaOdds = await db.QuinellaOdds!.FirstOrDefaultAsync(o => o.RaceKey == race.Key);
          var exactaOdds = await db.ExactaOdds!.FirstOrDefaultAsync(o => o.RaceKey == race.Key);
          var trioOdds = await db.TrioOdds!.FirstOrDefaultAsync(o => o.RaceKey == race.Key);
          var trifectaOdds = await db.TrifectaOdds!.FirstOrDefaultAsync(o => o.RaceKey == race.Key);
          info.Odds.Value = new OddsInfo(horses, frameOdds, quinellaPlaceOdds, quinellaOdds, exactaOdds, trioOdds, trifectaOdds);

          // 購入済馬券
          var tickets = await db.Tickets!.Where(t => t.RaceKey == race.Key).ToArrayAsync();
          info.Tickets.Value = new BettingTicketInfo(horseInfos, info.Odds.Value, tickets);

          // 払い戻し情報を更新
          info.Payoff?.SetTickets(info.Tickets.Value);

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

          // スクリプト
          info.CanExecuteScript.Value = true;
          ThreadUtil.InvokeOnUiThread(async () =>
          {
            await info.Script.UpdateAsync();
          });

          info.IsLoadCompleted.Value = true;
        }
        catch
        {
          // TODO log
          info.IsLoadError.Value = true;
          info.IsLoadCompleted.Value = true;
        }
        finally
        {
          db.Dispose();
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
