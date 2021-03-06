using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Analysis.Table;
using KmyKeiba.Models.Common;
using KmyKeiba.Models.Connection;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Image;
using KmyKeiba.Models.Injection;
using KmyKeiba.Models.Race.Tickets;
using KmyKeiba.Models.Script;
using KmyKeiba.Shared;
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
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    private static IBuyer? _buyer => __buyer ??= InjectionManager.GetInstance<IBuyer>(InjectionManager.Buyer);
    private static IBuyer? __buyer;

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

    public RaceWinnerHorseTrendAnalysisSelector WinnerTrendAnalyzers { get; }

    public ReactiveCollection<RaceHorseAnalyzer> Horses { get; } = new();

    public ReactiveCollection<RaceHorseAnalyzer> HorsesResultOrdered { get; } = new();

    public ReactiveProperty<RaceHorseAnalyzer?> ActiveHorse { get; } = new();

    public AnalysisTableList AnalysisTables { get; } = new();

    public ReactiveProperty<bool> HasAnalysisTables { get; } = new();

    public ReactiveCollection<RaceCorner> Corners { get; } = new();

    public RaceCourseSummaryImage CourseSummaryImage { get; } = new();

    public RaceSubjectInfo Subject { get; }

    public PayoffInfo? Payoff { get; }

    public ReactiveProperty<OddsInfo?> Odds { get; } = new();

    public ReactiveProperty<BettingTicketInfo?> Tickets { get; } = new();

    public ReactiveCollection<RaceChangeInfo> Changes { get; } = new();

    public ScriptManager Script { get; }

    public ReactiveProperty<bool> CanExecuteScript { get; } = new();

    public ReactiveProperty<bool> CanUpdate { get; } = new();

    public ReactiveProperty<bool> IsNewDataHasResults { get; } = new();

    public ReactiveProperty<bool> IsWillTrendAnalyzersResetedOnUpdate { get; } = new();

    public ReactiveProperty<bool> CanBuy { get; } = new();

    public ReactiveProperty<TimeSpan> BuyLimit { get; } = new();

    public ReactiveProperty<bool> IsBeforeLimitTime { get; } = new();

    public ReactiveProperty<StatusFeeling> LimitStatus { get; } = new();

    public ReactiveProperty<bool> IsWaitingResults { get; } = new();

    public ReactiveProperty<PurchaseStatusCode> PurchaseStatus { get; } = new();

    public ReactiveProperty<StatusFeeling> PurchaseStatusFeeling { get; } = new();

    public double TimeDeviationValue => this.HorsesResultOrdered.FirstOrDefault()?.ResultTimeDeviationValue ?? 0;

    public double A3HTimeDeviationValue => this.HorsesResultOrdered.FirstOrDefault()?.A3HResultTimeDeviationValue ?? 0;

    public string Name => this.Subject.DisplayName;

    public bool IsAvoidCaching { get; private set; }

    public RaceMovieInfo Movie => this._movie ??= new(this.Data);
    private RaceMovieInfo? _movie;

    public string PurchaseLabel { get; }

    private RaceInfo(RaceData race)
    {
      logger.Debug($"{race.Key} のレースの初期化を開始します");

      this.Data = race;
      this.Subject = new(race);
      this.Script = ScriptManager.GetInstance(this);

      this.PurchaseLabel = _buyer?.GetPurchaseLabel(race) ?? "投票不可";
      logger.Info($"馬券購入ラベル: {this.PurchaseLabel}");

      this.Weather.Value = race.TrackWeather;
      this.Condition.Value = race.TrackCondition;
      this.CanChangeWeathers = race.TrackWeather == RaceCourseWeather.Unknown || race.IsWeatherSetManually;

      var details = RaceCourses.TryGetCourses(race);
      foreach (var detail in details)
      {
        this.CourseDetails.Add(detail);
      }

      this.TrendAnalyzers = new RaceTrendAnalysisSelector(race);
      this.WinnerTrendAnalyzers = new RaceWinnerHorseTrendAnalysisSelector(race);
      this.CourseSummaryImage.Race = race;

      this.AnalysisTables.ObserveAddChanged().Subscribe(_ => this.HasAnalysisTables.Value = true).AddTo(this._disposables);

      var buyLimitTime = this.Data.StartTime.AddMinutes(-2);
      logger.Debug($"購入締め切り: {buyLimitTime} レース開始: {race.StartTime}");

      this.CanBuy.Value = _buyer?.CanBuy(race) == true;
      if (this.CanBuy.Value)
      {
        this.WaitTicketsAndCallback(tickets =>
        {
          tickets.Tickets
            .CollectionChangedAsObservable()
            .CombineLatest(this.PurchaseStatus, (_, pur) => tickets.Tickets.Any() && pur != PurchaseStatusCode.Checking)
            .TakeWhile(_ => DateTime.Now < buyLimitTime)
            .Subscribe(val => this.CanBuy.Value = val)
            .AddTo(this._disposables);
          this.CanBuy.Value = tickets.Tickets.Any();
        });
      }

      IDisposable? buyLimitTimeDisposable = null;
      buyLimitTimeDisposable = Observable.Interval(TimeSpan.FromSeconds(1))
        .Subscribe(_ =>
        {
          if (buyLimitTime > DateTime.Now)
          {
            var d = buyLimitTime - DateTime.Now;
            if (d.TotalDays < 1)
            {
              this.IsBeforeLimitTime.Value = true;
              this.BuyLimit.Value = buyLimitTime - DateTime.Now;
              this.LimitStatus.Value = this.BuyLimit.Value.TotalSeconds < 300 ? StatusFeeling.Bad : StatusFeeling.Unknown;
            }
            else
            {
              // 締め切りがあと１日を超えている場合は、タイマーを表示しない
              // （表示上の都合）
              this.IsBeforeLimitTime.Value = false;
            }
          }
          else
          {
            // 締め切りを過ぎた
            this.IsBeforeLimitTime.Value = false;
            this.IsWaitingResults.Value = true;
            this.CanBuy.Value = false;
            logger.Debug("馬券購入締め切りを過ぎました");

            if (buyLimitTimeDisposable != null)
            {
              buyLimitTimeDisposable.Dispose();
              this._disposables.Remove(buyLimitTimeDisposable);
            }
          }
        })
        .AddTo(this._disposables);
    }

    private RaceInfo(RaceData race, PayoffInfo? payoff) : this(race)
    {
      this.Payoff = payoff;
    }

    private void SetHorsesDelay(IReadOnlyList<RaceHorseAnalyzer> horses, RaceStandardTimeMasterData standardTime)
    {
      ThreadUtil.InvokeOnUiThread(() =>
      {
        foreach (var horse in horses.OrderBy(h => h.Data.Number))
        {
          this.Horses.Add(horse);
        }
        foreach (var horse in horses.Where(h => h.Data.ResultOrder > 0).OrderBy(h => h.Data.ResultOrder).Concat(
            horses.Where(h => h.Data.ResultOrder == 0 && h.Data.AbnormalResult != RaceAbnormality.Unknown).OrderBy(h => h.Data.Number).OrderBy(h => h.Data.AbnormalResult)))
        {
          this.HorsesResultOrdered.Add(horse);
        }
        this.RaceAnalyzer.Value = new RaceAnalyzer(this.Data, horses.Select(h => h.Data).ToArray(), standardTime);
        this.RaceAnalyzer.Value.SetMatches(horses);

        this.HasResults.Value = this.Horses.Any(h => h.Data.ResultOrder > 0);
        this.HasHorses.Value = true;

        logger.Info($"馬が設定されました {horses.Count}");
        logger.Debug($"基準タイムのサンプル数: {standardTime.SampleCount}");
      });
    }

    public async Task WaitHorsesSetupAsync()
    {
      while (!this.HasHorses.Value)
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

    public void SetActiveHorse(uint id)
    {
      this.ActiveHorse.Value = this.Horses.FirstOrDefault(h => h.Data.Id == id);
      logger.Debug($"ID {id} の馬 {this.ActiveHorse.Value?.Data.Name} を選択しました");

      // 遅延でデータ読み込み
      if (this.ActiveHorse.Value != null)
      {
        Task.Run(async () => {
          if (this.ActiveHorse.Value.BloodSelectors?.IsRequestedInitialization == true)
          {
            using var db = new MyContext();
            try
            {
              await this.ActiveHorse.Value.BloodSelectors.InitializeBloodListAsync(db);
            }
            catch (Exception ex)
            {
              logger.Warn($"{this.ActiveHorse.Value.Data.Name} の血統情報がロードできませんでした", ex);
            }
          }

          var timeout = 0;
          while (this.ActiveHorse.Value.Training.Value == null && timeout < 30_000)
          {
            await Task.Delay(50);
            timeout += 50;
          }
          if (this.ActiveHorse.Value.Training.Value != null)
          {
            await this.ActiveHorse.Value.Training.Value.UpdateTrainingListAsync();
          }
        });
      }
    }

    public async Task SetWeatherAsync(string key)
    {
      await this.WaitHorsesSetupAsync();

      short.TryParse(key, out var value);

      try
      {
        using var db = new MyContext();
        db.Races!.Attach(this.Data);

        this.Weather.Value = this.Data.TrackWeather = (RaceCourseWeather)value;
        this.Data.IsWeatherSetManually = true;

        await db.SaveChangesAsync();

        this.IsAvoidCaching = true;
        RaceInfoCacheManager.Remove(this.Data.Key);
        await this.CheckCanUpdateAsync();

        logger.Debug($"天気情報を {this.Weather.Value} に変更しました key: {key}");
      }
      catch (Exception ex)
      {
        logger.Warn($"天気情報を {key} に変更できませんでした", ex);
      }
    }

    public async Task SetConditionAsync(string key)
    {
      await this.WaitHorsesSetupAsync();

      short.TryParse(key, out var value);

      try
      {
        using var db = new MyContext();
        db.Races!.Attach(this.Data);

        this.Condition.Value = this.Data.TrackCondition = (RaceCourseCondition)value;
        this.Data.IsConditionSetManually = true;

        await db.SaveChangesAsync();

        this.IsAvoidCaching = true;
        RaceInfoCacheManager.Remove(this.Data.Key);
        await this.CheckCanUpdateAsync();

        logger.Debug($"馬場状態を {this.Weather.Value} に変更しました key: {key}");
      }
      catch (Exception ex)
      {
        logger.Warn($"馬場状態を {key} に変更できませんでした", ex);
      }
    }

    public static bool IsUpdateNeeded(RaceData old, IReadOnlyList<RaceHorseData> oldHorses, RaceInfo @new)
    {
      return IsUpdateNeeded(old, @new.Data, oldHorses, @new.Horses.Select(h => h.Data).ToArray());
    }

    public static bool IsUpdateNeeded(RaceData old, RaceData @new, IReadOnlyList<RaceHorseData> oldHorses, IReadOnlyList<RaceHorseData> newHorses)
    {
      var isUpdate = @new.DataStatus != old.DataStatus ||
        @new.TrackWeather != old.TrackWeather ||
        @new.TrackCondition != old.TrackCondition ||
        @new.TrackGround != old.TrackGround ||
        @new.Course != old.Course ||
        @new.CourseType != old.CourseType ||
        @new.TrackOption != old.TrackOption ||
        @new.TrackCornerDirection != old.TrackCornerDirection ||
        @new.StartTime != old.StartTime ||
        @new.Corner4Result != old.Corner4Result;
      logger.Debug($"レース基本情報を確認: {isUpdate}");

      if (!isUpdate)
      {
        isUpdate = newHorses
          .Join(oldHorses, h => h.Key, h => h.Key, (nh, h) => new
          {
            IsUpdate = nh.DataStatus != h.DataStatus || nh.ResultOrder != h.ResultOrder || nh.AbnormalResult != h.AbnormalResult ||
            nh.RiderName != h.RiderName || nh.RiderWeight != h.RiderWeight || nh.Weight != h.Weight || nh.Odds != h.Odds,
          })
          .Any(d => d.IsUpdate);
        logger.Debug($"馬のオッズ、状態を確認: {isUpdate}");
      }

      return isUpdate;
    }

    public async Task CheckCanUpdateAsync()
    {
      if (this.IsAvoidCaching)
      {
        // 天候馬場を手動で変更したとき
        this.CanUpdate.Value = true;
        this.IsWillTrendAnalyzersResetedOnUpdate.Value = true;
        return;
      }

      // LastModifiedに時刻は記録されないので、各項目を比較するしかない
      try
      {
        using var db = new MyContext();
        var newData = await db.Races!.FirstOrDefaultAsync(r => r.Key == this.Data.Key);
        if (newData != null)
        {
          var newHorses = await db.RaceHorses!
            .Where(rh => rh.RaceKey == this.Data.Key)
            .ToArrayAsync();
          var isUpdate = IsUpdateNeeded(this.Data, newData, this.Horses.Select(h => h.Data).ToArray(), newHorses);

          if (!isUpdate && this.Payoff == null)
          {
            var refund = await db.Refunds!.FirstOrDefaultAsync(r => r.RaceKey == this.Data.Key);
            if (refund != null)
            {
              isUpdate = true;
            }
            logger.Debug($"払い戻し状況を確認: {isUpdate}");
          }

          if (isUpdate)
          {
            this.CanUpdate.Value = isUpdate;
            this.IsWillTrendAnalyzersResetedOnUpdate.Value = this.IsWillResetTrendAnalyzersDataOnUpdate(newData);
            this.IsNewDataHasResults.Value = this.Data.DataStatus <= RaceDataStatus.Horses2 && newData.DataStatus >= RaceDataStatus.PreliminaryGrade3;
          }

          logger.Debug($"表示中のレース {this.Data.Key} の更新状態を確認しました。結果: {isUpdate}");
        }
      }
      catch (Exception ex)
      {
        logger.Error($"{this.Data.Key} のレースの更新可能状態を確認できませんでした", ex);
      }
    }

    public static bool IsWillResetTrendAnalyzersDataOnUpdate(RaceData oldData, RaceData newData)
    {
      // 更新の時に傾向検索結果をリセットする必要があるか
      return !(newData.TrackWeather == oldData.TrackWeather && newData.TrackCondition == oldData.TrackCondition &&
        newData.Distance == oldData.Distance && newData.TrackGround == oldData.TrackGround &&
        newData.TrackOption == oldData.TrackOption && newData.TrackCornerDirection == oldData.TrackCornerDirection &&
        newData.SubjectAge2 == oldData.SubjectAge2 && newData.SubjectAge3 == oldData.SubjectAge3 &&
        newData.SubjectAge4 == oldData.SubjectAge4 && newData.SubjectAge5 == oldData.SubjectAge5 &&
        newData.SubjectAgeYounger == oldData.SubjectAgeYounger && newData.SubjectName == oldData.SubjectName);
    }

    private bool IsWillResetTrendAnalyzersDataOnUpdate(RaceData newData)
    {
      if (this.IsAvoidCaching)
      {
        return true;
      }

      return IsWillResetTrendAnalyzersDataOnUpdate(this.Data, newData);
    }

    public async Task BuyAsync()
    {
      if (!this.CanBuy.Value || this.Tickets.Value == null)
      {
        logger.Warn($"購入できません CanBuy: {this.CanBuy.Value} / Tickets: {this.Tickets.Value}");
        return;
      }

      var tickets = this.Tickets.Value.Tickets.Select(t => t.Data).ToArray();
      if (!tickets.Any())
      {
        logger.Warn("設定されている馬券がないため購入を中止しました");
        return;
      }

      this.PurchaseStatus.Value = PurchaseStatusCode.Checking;
      this.PurchaseStatusFeeling.Value = StatusFeeling.Standard;

      var buyer = _buyer!;
      await buyer.CreateNewPurchase(this.Data)
        .AddTicketRange(tickets)
        .SendAsync(result =>
        {
          if (result)
          {
            this.PurchaseStatus.Value = PurchaseStatusCode.Succeed;
            this.PurchaseStatusFeeling.Value = StatusFeeling.Good;
          }
          else
          {
            this.PurchaseStatus.Value = PurchaseStatusCode.Failed;
            this.PurchaseStatusFeeling.Value = StatusFeeling.Bad;
          }
          logger.Info($"購入処理が完了 結果: {result}");
        });
    }

    public void Dispose()
    {
      this._disposables.Dispose();
      this.RaceAnalyzer.Value?.Dispose();
      this.TrendAnalyzers.Dispose();
      this.AnalysisTables.Dispose();
      foreach (var h in this.Horses)
      {
        h.Dispose();
      }
      this.Odds.Value?.Dispose();
      this.Tickets.Value?.Dispose();
      this.Payoff?.Dispose();
    }

    public static async Task<RaceInfo?> FromKeyAsync(string key, bool withTransaction = true, bool isCache = true)
    {
      logger.Info($"{key} のレースを読み込みます");

      using var db = new MyContext();

      var race = await db.Races!.FirstOrDefaultAsync(r => r.Key == key);
      if (race == null)
      {
        logger.Warn("レースがDBから見つかりませんでした");
        return null;
      }

      var payoff = await db.Refunds!.FirstOrDefaultAsync(r => r.RaceKey == key);
      PayoffInfo? payoffInfo = null;
      if (payoff != null)
      {
        logger.Debug("払い戻し情報が見つかりました");
        payoffInfo = new PayoffInfo(payoff);
      }

      return await FromDataAsync(race, payoffInfo, withTransaction, isCache);
    }

    private static async Task<RaceInfo> FromDataAsync(RaceData race, PayoffInfo? payoff, bool withTransaction, bool isCache)
    {
      logger.Debug($"{race.Key} のレース情報を読み込みます");

      var db = new MyContext();
      if (DownloaderModel.Instance.CanSaveOthers.Value && withTransaction)
      {
        logger.Debug("念のためトランザクションを開始します");
        await db.BeginTransactionAsync();
      }

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
          var cache = RaceInfoCacheManager.TryGetCache(race.Key);

          var horses = await db.RaceHorses!.Where(rh => rh.RaceKey == race.Key).ToArrayAsync();
          logger.Info($"馬の数: {horses.Length}, レース情報に記録されている馬の数: {race.HorsesCount}");

          var horseKeys = horses.Select(h => h.Key).ToArray();
          var horseAllHistories = cache?.HorseAllHistories;
          if (horseAllHistories == null)
          {
            horseAllHistories = (await db.RaceHorses!
              .Where(rh => horseKeys.Contains(rh.Key))
              .Where(rh => rh.Key != "0000000000")
              .Join(db.Races!, rh => rh.RaceKey, r => r.Key, (rh, r) => new { Race = r, RaceHorse = rh, })
              .Where(d => d.Race.StartTime < race.StartTime)
              .OrderByDescending(d => d.Race.StartTime)
              .ToArrayAsync())
              .Select(d => (d.Race, d.RaceHorse))
              .ToArray();
          }
          var standardTime = await AnalysisUtil.GetRaceStandardTimeAsync(db, race);
          logger.Debug($"馬の過去レースの総数: {horseAllHistories.Count}");

          // 時系列オッズ
          var oddsTimeline = await db.SingleOddsTimelines!.Where(o => o.RaceKey == race.Key).ToArrayAsync();
          logger.Info($"時系列オッズ {oddsTimeline.Length}件");

          // 各馬の情報
          var horseHistorySameHorses = cache?.HorseHistorySameHorses;
          if (horseHistorySameHorses == null)
          {
            var horseHistoryKeys = horseAllHistories.Select(h => h.RaceHorse.RaceKey).ToArray();
            horseHistorySameHorses = await db.RaceHorses!
              .Where(h => horseHistoryKeys.Contains(h.RaceKey))
              .Where(h => h.ResultOrder >= 1 && h.ResultOrder <= 5)
              .ToArrayAsync();
          }
          logger.Debug($"馬の過去レースの同走馬数: {horseHistorySameHorses.Count}");

          var horseInfos = new List<RaceHorseAnalyzer>();
          foreach (var horse in horses)
          {
            var histories = new List<RaceHorseAnalyzer>();
            foreach (var history in horseAllHistories.Where(h => h.RaceHorse.Key == horse.Key))
            {
              var historyStandardTime = await AnalysisUtil.GetRaceStandardTimeAsync(db, history.Race);
              var sameHorses = horseHistorySameHorses.Where(h => h.RaceKey == history.RaceHorse.RaceKey);
              histories.Add(new RaceHorseAnalyzer(history.Race, history.RaceHorse, sameHorses.ToArray(), historyStandardTime));
            }

            var riderWinRate = await AnalysisUtil.GetRiderWinRateAsync(db, race, horse.RiderCode);

            var analyzer = new RaceHorseAnalyzer(race, horse, horses, histories, standardTime, riderWinRate)
            {
              TrendAnalyzers = new RaceHorseTrendAnalysisSelector(race, horse, histories),
              RiderTrendAnalyzers = new RaceRiderTrendAnalysisSelector(race, horse),
              TrainerTrendAnalyzers = new RaceTrainerTrendAnalysisSelector(race, horse),
              BloodSelectors = new RaceHorseBloodTrendAnalysisSelectorMenu(race, horse),
            };
            analyzer.SetOddsTimeline(oddsTimeline);

            horseInfos.Add(analyzer);
            logger.Debug($"馬 {horse.Name} の情報を登録");
          }
          {
            // タイム指数の相対評価
            var timedvMax = horseInfos.Where(i => (i.History?.TimeDeviationValue ?? default) != default).OrderByDescending(i => i.History?.TimeDeviationValue ?? 0.0).Skip(2).FirstOrDefault()?.History?.TimeDeviationValue;
            var timedvMin = horseInfos.Where(i => (i.History?.TimeDeviationValue ?? default) != default).OrderBy(i => i.History?.TimeDeviationValue ?? 0.0).Skip(2).FirstOrDefault()?.History?.TimeDeviationValue;
            var a3htimedvMax = horseInfos.Where(i => (i.History?.A3HTimeDeviationValue ?? default) != default).OrderByDescending(i => i.History?.A3HTimeDeviationValue ?? 0.0).Skip(2).FirstOrDefault()?.History?.A3HTimeDeviationValue;
            var a3htimedvMin = horseInfos.Where(i => (i.History?.A3HTimeDeviationValue ?? default) != default).OrderBy(i => i.History?.A3HTimeDeviationValue ?? 0.0).Skip(2).FirstOrDefault()?.History?.A3HTimeDeviationValue;
            var ua3htimedvMax = horseInfos.Where(i => (i.History?.UntilA3HTimeDeviationValue ?? default) != default).OrderByDescending(i => i.History?.UntilA3HTimeDeviationValue ?? 0.0).Skip(2).FirstOrDefault()?.History?.UntilA3HTimeDeviationValue;
            var ua3htimedvMin = horseInfos.Where(i => (i.History?.UntilA3HTimeDeviationValue ?? default) != default).OrderBy(i => i.History?.UntilA3HTimeDeviationValue ?? 0.0).Skip(2).FirstOrDefault()?.History?.UntilA3HTimeDeviationValue;
            var riderPlaceRateMax = horseInfos.Where(i => i.RiderAllCount > 0).Select(i => i.RiderPlaceBitsRate).OrderByDescending(i => i).Skip(2).FirstOrDefault();
            var riderPlaceRateMin = horseInfos.Where(i => i.RiderAllCount > 0).Select(i => i.RiderPlaceBitsRate).OrderBy(i => i).Skip(2).FirstOrDefault();
            foreach (var horse in horseInfos)
            {
              if (horse.History != null)
              {
                if (horse.History.BeforeRaces.Where(r => r.Data.ResultTime.TotalSeconds > 0).Take(10)
                  .Count(r => r.Race.TrackGround != race.TrackGround || r.Race.TrackType != race.TrackType || Math.Abs(r.Race.Distance - race.Distance) >= 400) >= 4)
                {
                  // 条件の大きく異なるレース
                  horse.History.TimeDVComparation = horse.History.A3HTimeDVComparation = horse.History.UntilA3HTimeDVComparation = ValueComparation.Warning;
                }
                else
                {
                  if (timedvMax != null && timedvMin != null)
                  {
                    horse.History.TimeDVComparation = horse.History.TimeDeviationValue + 0.5 >= timedvMax ? ValueComparation.Good :
                      horse.History.TimeDeviationValue - 0.5 <= timedvMin ? ValueComparation.Bad : ValueComparation.Standard;
                  }
                  if (a3htimedvMax != null && a3htimedvMin != null)
                  {
                    horse.History.A3HTimeDVComparation = horse.History.A3HTimeDeviationValue + 0.5 >= a3htimedvMax ? ValueComparation.Good :
                      horse.History.A3HTimeDeviationValue - 0.5 <= a3htimedvMin ? ValueComparation.Bad : ValueComparation.Standard;
                  }
                  if (ua3htimedvMax != null && ua3htimedvMin != null)
                  {
                    horse.History.UntilA3HTimeDVComparation = horse.History.UntilA3HTimeDeviationValue + 0.5 >= ua3htimedvMax ? ValueComparation.Good :
                      horse.History.UntilA3HTimeDeviationValue - 0.5 <= ua3htimedvMin ? ValueComparation.Bad : ValueComparation.Standard;
                  }
                }
              }
              if (riderPlaceRateMax != 0)
              {
                horse.RiderPlaceBitsRateComparation = horse.RiderPlaceBitsRate + 0.02 >= riderPlaceRateMax ? ValueComparation.Good :
                horse.RiderPlaceBitsRate - 0.02 <= riderPlaceRateMin ? ValueComparation.Bad : ValueComparation.Standard;
              }
            }
          }
          logger.Debug("馬のタイム指数相対評価を設定");
          info.SetHorsesDelay(horseInfos, standardTime);

          // オッズ
          var frameOdds = cache?.Refund != null ? cache.FrameNumberOdds : await db.FrameNumberOdds!.FirstOrDefaultAsync(o => o.RaceKey == race.Key);
          var quinellaPlaceOdds = cache?.Refund != null ? cache.QuinellaPlaceOdds : await db.QuinellaPlaceOdds!.FirstOrDefaultAsync(o => o.RaceKey == race.Key);
          var quinellaOdds = cache?.Refund != null ? cache.QuinellaOdds : await db.QuinellaOdds!.FirstOrDefaultAsync(o => o.RaceKey == race.Key);
          var exactaOdds = cache?.Refund != null ? cache.ExactaOdds : await db.ExactaOdds!.FirstOrDefaultAsync(o => o.RaceKey == race.Key);
          var trioOdds = cache?.Refund != null ? cache.TrioOdds : await db.TrioOdds!.FirstOrDefaultAsync(o => o.RaceKey == race.Key);
          var trifectaOdds = cache?.Refund != null ? cache.TrifectaOdds : await db.TrifectaOdds!.FirstOrDefaultAsync(o => o.RaceKey == race.Key);
          info.Odds.Value = new OddsInfo(horses, frameOdds, quinellaPlaceOdds, quinellaOdds, exactaOdds, trioOdds, trifectaOdds);
          logger.Info($"オッズ 枠連:{frameOdds != null} ワイド:{quinellaPlaceOdds != null} 馬連:{quinellaOdds != null} 馬単:{exactaOdds != null} 三連複:{trioOdds != null} 三連単:{trifectaOdds != null}");

          // 購入済馬券
          var tickets = await db.Tickets!.Where(t => t.RaceKey == race.Key).ToArrayAsync();
          info.Tickets.Value = new BettingTicketInfo(horseInfos, info.Odds.Value, tickets);
          logger.Info($"保存されている馬券数: {tickets.Length}");

          // 払い戻し情報を更新
          info.Payoff?.SetTickets(info.Tickets.Value, horses);

          // 最新情報／ついでに分析テーブル
          var changes = await db.RaceChanges!.Where(c => c.RaceKey == race.Key).ToArrayAsync();
          ThreadUtil.InvokeOnUiThread(async () =>
          {
            var isWeightAdded = false;
            foreach (var change in changes.Select(c => new RaceChangeInfo(c, race, horses)))
            {
              if (change.Data.ChangeType == RaceChangeType.HorseWeight)
              {
                if (isWeightAdded) continue;
                isWeightAdded = true;
              }
              info.Changes.Add(change);
            }

            foreach (var table in ApplicationConfiguration.Current.Value.AnalysisTableGenerators)
            {
              info.AnalysisTables.Add(await table.GenerateAsync(info));
            }
            var first = info.AnalysisTables.FirstOrDefault();
            if (first != null)
            {
              first.IsActive.Value = true;
            }
          });
          logger.Debug($"最新情報の数: {changes.Length}");

          // すべてのデータ読み込み完了
          info.IsLoadCompleted.Value = true;
          info.CanExecuteScript.Value = true;
          logger.Info("レースの読み込みが完了しました");

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

          // 調教
          var historyStartDate = race.StartTime.AddMonths(-3);
          var trainings = cache?.Trainings ?? await db.Trainings!
            .Where(t => horseKeys.Contains(t.HorseKey) && t.StartTime <= race.StartTime && t.StartTime > historyStartDate)
            .OrderByDescending(t => t.StartTime)
            .ToArrayAsync();
          var woodTrainings = cache?.WoodtipTrainings ?? await db.WoodtipTrainings!
            .Where(t => horseKeys.Contains(t.HorseKey) && t.StartTime <= race.StartTime && t.StartTime > historyStartDate)
            .OrderByDescending(t => t.StartTime)
            .ToArrayAsync();
          logger.Info($"坂路調教: {trainings.Count}, ウッドチップ調教: {woodTrainings.Count}");
          foreach (var horse in horseInfos)
          {
            horse.Training.Value = new TrainingAnalyzer(
              trainings.Where(t => t.HorseKey == horse.Data.Key).ToArray(),
              woodTrainings.Where(t => t.HorseKey == horse.Data.Key).ToArray()
              );
          }

          // キャッシング
          if (isCache)
          {
            RaceInfoCacheManager.Register(info, horseAllHistories, horseHistorySameHorses, trainings, woodTrainings,
              info.Payoff?.Payoff, frameOdds, quinellaPlaceOdds, quinellaOdds, exactaOdds, trioOdds, trifectaOdds);
          }
        }
        catch (Exception ex)
        {
          logger.Error("レース情報の読み込みに失敗しました", ex);
          info.IsLoadError.Value = true;
          info.IsLoadCompleted.Value = true;
        }
        finally
        {
          db.Dispose();
        }
      });

      return info;
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

  public enum PurchaseStatusCode
  {
    Unknown,

    [Label("購入を検証中")]
    Checking,

    [Label("購入成功")]
    Succeed,

    [Label("購入に失敗した可能性があります")]
    Failed,
  }
}
