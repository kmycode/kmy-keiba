using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Analysis.Generic;
using KmyKeiba.Models.Common;
using KmyKeiba.Models.Connection;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Image;
using KmyKeiba.Models.Injection;
using KmyKeiba.Models.Race.AnalysisTable.Script;
using KmyKeiba.Models.Race.ExNumber;
using KmyKeiba.Models.Race.Finder;
using KmyKeiba.Models.Race.HorseMark;
using KmyKeiba.Models.Race.Memo;
using KmyKeiba.Models.Race.Tickets;
using KmyKeiba.Models.Script;
using KmyKeiba.Models.Setting;
using KmyKeiba.Shared;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
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
    private readonly List<uint> _delayLoadedHorses = new();

    public RaceData Data { get; }

    public ReactiveProperty<RaceAnalyzer?> RaceAnalyzer { get; } = new();

    public ReactiveProperty<bool> HasResults { get; } = new();

    public ReactiveProperty<bool> HasHorses { get; } = new();

    public ReactiveProperty<bool> HasCheckedHorse { get; } = new();

    public ReactiveProperty<bool> IsLoadCompleted { get; } = new();

    public ReactiveProperty<bool> IsLoadError { get; } = new();

    public bool CanChangeWeathers { get; }

    public bool IsBanei => this.Data.Course == RaceCourse.ObihiroBannei;

    public bool IsCanceled => this.Data.DataStatus == RaceDataStatus.Canceled;

    public ReactiveProperty<RaceCourseWeather> Weather { get; } = new();

    public ReactiveProperty<RaceCourseCondition> Condition { get; } = new();

    public ReactiveCollection<RaceCourseDetail> CourseDetails { get; } = new();

    public ReactiveCollection<RaceHorseAnalyzer> Horses { get; } = new();

    public ReactiveCollection<RaceHorseAnalyzer> HorsesResultOrdered { get; } = new();

    public ReactiveProperty<RaceHorseAnalyzer?> ActiveHorse { get; } = new();

    public ReactiveProperty<bool> HasAnalysisTables { get; } = new();

    public ReactiveCollection<RaceCorner> Corners { get; } = new();

    public ReactiveCollection<RaceLapTime> LapTimes { get; } = new();

    public bool HasLapTimes { get; }

    public bool HasHaronTimes { get; }

    public RaceCourseSummaryImage CourseSummaryImage { get; } = new();

    public RaceSubjectInfo Subject { get; }

    public PayoffInfo? Payoff { get; }

    public ReactiveProperty<OddsInfo?> Odds { get; } = new();

    public ReactiveProperty<BettingTicketInfo?> Tickets { get; } = new();

    public ReactiveCollection<RaceChangeInfo> Changes { get; } = new();

    public RaceFinder Finder { get; }

    public ReactiveProperty<RaceMemoModel?> MemoEx { get; } = new();

    public ReactiveProperty<FinderModel?> FinderModel { get; } = new();

    public ReactiveProperty<HorseMarkModel?> HorseMark { get; } = new();

    public ReactiveProperty<AnalysisTable.AnalysisTableModel> AnalysisTable { get; } = new();

    public ScriptManager Script { get; }

    public ReactiveProperty<bool> CanExecuteScript { get; } = new();

    public ReactiveProperty<bool> CanUpdate { get; } = new();

    public ReactiveProperty<bool> IsNewDataHasResults { get; } = new();

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

      foreach (var lapTime in RaceLapTime.GetAsList(race))
      {
        this.LapTimes.Add(lapTime);
      }
      this.HasLapTimes = this.LapTimes.Any();
      this.HasHaronTimes = race.AfterHaronTime3 != default || race.AfterHaronTime4 != default ||
        race.BeforeHaronTime3 != default || race.BeforeHaronTime4 != default;

      this.Finder = new RaceFinder(race);
      this.CourseSummaryImage.Race = race;

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
      var sortedHorses = (horses.All(h => h.Data.Number == default) ? horses.OrderBy(h => h.Data.Name) : horses.OrderBy(h => h.Data.Number)).ToArray();

      var finderModel = new FinderModel(this.Data, null, sortedHorses);
      finderModel.Input.CopyFrom(AppGeneralConfig.Instance.DefaultRaceSetting.Input);
      this.FinderModel.Value = finderModel;

      foreach (var horse in horses)
      {
        horse.IsChecked.Subscribe(_ => this.HasCheckedHorse.Value = this.Horses.Any(h => h.IsChecked.Value)).AddTo(this._disposables);
      }
      this.HasCheckedHorse.Value = horses.Any(h => h.IsChecked.Value);

      ThreadUtil.InvokeOnUiThread(() =>
      {
        foreach (var horse in sortedHorses)
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

      if (this.ActiveHorse.Value == null) return;

      Task.Run(async () =>
      {
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
        else
        {
          this.ActiveHorse.Value.BloodSelectors?.UpdateGenerationRates();
        }
      });

      if (this._delayLoadedHorses.Contains(id)) return;
      this._delayLoadedHorses.Add(id);

      // 遅延でデータ読み込み
      if (this.ActiveHorse.Value != null)
      {
        if (this.ActiveHorse.Value.FinderModel.Value != null)
        {
          // デフォルトの検索条件を設定
          var model = this.ActiveHorse.Value.FinderModel.Value;
          model.Input.HorseOfCurrentRace.IsUnspecified.Value = false;
          model.Input.HorseOfCurrentRace.IsActiveHorse.Value = true;
          model.Input.HorseOfCurrentRace.IsActiveHorseSelf.Value = true;
          model.Input.OtherSetting.IsExpandedResult.Value = true;
          model.BeginLoad();
        }

        Task.Run(async () => {
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
      return GetTrendAnalyzerResetType(oldData, newData) != RaceUpdateType.None;
    }

    public static RaceUpdateType GetTrendAnalyzerResetType(RaceData oldData, RaceData newData)
    {
      var type = RaceUpdateType.None;
      if (newData.TrackWeather != oldData.TrackWeather)
      {
        type |= RaceUpdateType.Weather;
      }
      if (newData.TrackGround != oldData.TrackGround)
      {
        type |= RaceUpdateType.Ground;
      }
      if (newData.Distance != oldData.Distance)
      {
        type |= RaceUpdateType.Distance;
      }
      if (newData.TrackCondition != oldData.TrackCondition)
      {
        type |= RaceUpdateType.Condition;
      }
      if (newData.TrackOption != oldData.TrackOption)
      {
        type |= RaceUpdateType.Option;
      }
      if (newData.TrackCornerDirection != oldData.TrackCornerDirection)
      {
        type |= RaceUpdateType.CornerDirection;
      }
      if (newData.SubjectAge2 != oldData.SubjectAge2 || newData.SubjectAge3 != oldData.SubjectAge3 ||
        newData.SubjectAge4 != oldData.SubjectAge4 || newData.SubjectAge5 != oldData.SubjectAge5 ||
        newData.SubjectAgeYounger != oldData.SubjectAgeYounger || newData.SubjectName != oldData.SubjectName)
      {
        type |= RaceUpdateType.Subject;
      }
      return type;
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

    public void UpdateCache()
    {
      RaceInfoCacheManager.UpdateCache(this.Data.Key, null);
    }

    public void Dispose()
    {
      this._disposables.Dispose();
      this.RaceAnalyzer.Value?.Dispose();
      this.AnalysisTable.Value?.Dispose();
      this.Finder.Dispose();
      this.FinderModel.Value?.Dispose();
      this.MemoEx.Value?.Dispose();
      foreach (var h in this.Horses)
      {
        h.Dispose();
      }
      this.Odds.Value?.Dispose();
      this.Tickets.Value?.Dispose();
      this.Payoff?.Dispose();

      //Models.Analysis.RaceAnalyzer.OutputLogCount();
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

      AddCorner(info.Corners, race.Corner1Result, race.Corner1Number, race.Corner1Position);
      AddCorner(info.Corners, race.Corner2Result, race.Corner2Number, race.Corner2Position);
      AddCorner(info.Corners, race.Corner3Result, race.Corner3Number, race.Corner3Position);
      AddCorner(info.Corners, race.Corner4Result, race.Corner4Number, race.Corner4Position);

      // 以降の情報は遅延読み込み
      _ = Task.Run(async () =>
      {
        try
        {
          var cache = RaceInfoCacheManager.TryGetCache(race.Key);

          var standardTime = await AnalysisUtil.GetRaceStandardTimeAsync(db, race);

          var factory = new RaceHorseAnalyzerRaceListFactory(race.Key)
          {
            IsComparation = true,
            IsDetail = true,
            IsHorseAllHistories = true,
            IsHorseHistorySameHorses = true,
            IsJrdbData = true,
            IsOddsTimeline = true,
          };
          var horseInfos = await factory.ToAnalyzerAsync(db, cache);
          if (horseInfos == null)
          {
            throw new InvalidOperationException();
          }
          foreach (var obj in factory.Disposables)
          {
            info._disposables.Add(obj);
          }
          var sortedHorses = horseInfos.All(h => h.Data.Number == default) ? horseInfos.OrderBy(h => h.Data.Name) : horseInfos.OrderBy(h => h.Data.Number);
          var horses = horseInfos.Select(h => h.Data).ToArray();
          var jrdbHorses = horseInfos.Where(h => h.JrdbData != null).Select(h => h.JrdbData!).ToArray();
          var horseKeys = horses.Select(h => h.Key).ToArray();

          info.SetHorsesDelay(horseInfos, standardTime);
          info.AnalysisTable.Value = new AnalysisTable.AnalysisTableModel(info.Data, sortedHorses.ToArray(), cache?.AnalysisTable);

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
          ThreadUtil.InvokeOnUiThread(() =>
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
          });
          logger.Debug($"最新情報の数: {changes.Length}");

          // 印
          info.HorseMark.Value = await HorseMarkModel.CreateAsync(db, race.Key, jrdbHorses, sortedHorses.ToArray());

          // すべてのデータ読み込み完了
          info.IsLoadCompleted.Value = true;
          info.CanExecuteScript.Value = true;
          logger.Info("レースの読み込みが完了しました");

          // 拡張メモ
          info.MemoEx.Value = new RaceMemoModel(race, horseInfos);
          await info.MemoEx.Value.LoadAsync(db);

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
            RaceInfoCacheManager.Register(info, factory.HorseAllHistories, factory.HorseHistorySameRaceHorses, factory.HorseDetails, factory.HorseSales, trainings, woodTrainings,
               /*info.Finder*/null, /*info.AnalysisTable.Value?.ToCache()*/null, info.Payoff?.Payoff, frameOdds, quinellaPlaceOdds, quinellaOdds, exactaOdds, trioOdds, trifectaOdds);
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

    private static void AddCorner(IList<RaceCorner> corners, string result, int num, int pos)
    {
      if (string.IsNullOrWhiteSpace(result))
      {
        return;
      }

      var corner = RaceCorner.FromString(result);
      corner.Number = num;
      corner.Position = pos;

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

  [Flags]
  public enum RaceUpdateType
  {
    None = 0,
    Weather = 1,
    Condition = 2,
    Distance = 4,
    Ground = 8,
    Odds = 16,
    Option = 32,
    CornerDirection = 64,
    Subject = 128,
    All = int.MaxValue,
  }
}
