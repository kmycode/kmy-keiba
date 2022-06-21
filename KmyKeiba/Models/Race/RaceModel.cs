using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Injection;
using KmyKeiba.Models.RList;
using KmyKeiba.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race
{
  internal class RaceModel : IDisposable
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    private readonly CompositeDisposable _disposables = new();
    private IDisposable? ticketUpdated;

    public ReactiveProperty<string> RaceKey { get; } = new(string.Empty);

    public ReactiveProperty<RaceInfo?> Info { get; } = new();

    public RaceList RaceList { get; } = new();

    public ReactiveProperty<bool> IsLoaded { get; }

    public ReactiveProperty<bool> IsFirstLoadStarted { get; } = new();

    public ReactiveProperty<bool> IsViewExpection { get; } = new();

    public ReactiveProperty<bool> IsViewResult { get; } = new();

    public ReactiveProperty<bool> IsSelectedAllHorses { get; } = new(true);

    public string FirstMessage { get; }

    public ReactiveProperty<string> ErrorMessage { get; } = new();

    public ReactiveProperty<bool> IsError { get; } = new();

    public RaceModel()
    {
      this.IsLoaded = this.Info
        .Select(i => i != null)
        .ToReactiveProperty()
        .AddTo(this._disposables);

      var firstMessages = new string[]
      {
        "寂しい画面ですね。",
        "昨日はいくら勝ちましたか？",
        "今日はいい天気でしょうか？",
      };
      this.FirstMessage = firstMessages[new Random().Next(firstMessages.Length)];

      this.RaceList.SelectedRaceKey.Subscribe(key =>
      {
        logger.Info($"レースキー変更 {this.RaceKey.Value} -> {key}");
        if (key != null)
        {
          if (key != this.RaceKey.Value)
          {
            this.RaceKey.Value = key;
            this.LoadCurrentRace();
          }
        }
        else
        {
          this.Info.Value = null;
        }
      });

      logger.Info("メインモデル初期化完了");
    }

    public void OnDatabaseInitialized()
    {
      Task.Run(async () =>
      {
        try
        {
          await this.RaceList.UpdateListAsync();

#if DEBUG
          // UmaConnしか使ってない人用に中央競馬の一部のデータを埋め込み
          var dataGenerator = InjectionManager.GetInstance<IInternalDataGenerator>(InjectionManager.InternalDataGenerator);
          if (dataGenerator != null)
          {
            await dataGenerator.GenerateBaseStandardTimeDataAsync();
          }
#endif
        }
        catch (Exception ex)
        {
          logger.Error($"{this.RaceList.Date.Value} のリスト作成でエラーが発生しました", ex);
        }
      });
    }

    public async Task ChangeHorseMarkAsync(RaceHorseMark mark, RaceHorseAnalyzer horse)
    {
      try
      {
        using var db = new MyContext();
        db.RaceHorses!.Attach(horse.Data);

        horse.Mark.Value = horse.Data.Mark = mark;

        await db.SaveChangesAsync();
        logger.Info($"{horse.Data.Name}(racekey:{horse.Data.RaceKey}) の星を変更しました {mark}");
      }
      catch (Exception ex)
      {
        logger.Error($"{horse.Data.Name}(racekey:{horse.Data.RaceKey}) の星を変更できませんでした {mark}", ex);
      }
    }

    public void OnSelectedRaceUpdated()
    {
      Task.Run(async () =>
      {
        if (this.Info.Value != null)
        {
          logger.Info($"選択中のレース {this.Info.Value.Data.Key} の更新を検査します");
          await this.Info.Value.CheckCanUpdateAsync();
        }
        else
        {
          logger.Warn($"選択中のレース {this.RaceKey.Value} が正常にロードされていません");
        }
      });
    }

    public void SetRaceKey(string key)
    {
      this.RaceKey.Value = key;
      this.LoadCurrentRace();
    }

    public void UpdateCurrentRace()
    {
      this.LoadCurrentRace(this.Info.Value?.Data.Key);
    }

    public async Task SetActiveHorsesAsync(IEnumerable<string> horseKeys)
    {
      foreach (var key in horseKeys)
      {
        await this.SetActiveHorseAsync(key);
      }
    }

    public async Task SetActiveHorseAsync(string horseKey)
    {
      if (string.IsNullOrEmpty(horseKey))
      {
        return;
      }

      while (this.Info.Value == null)
      {
        await Task.Delay(50);
      }
      await this.Info.Value.WaitHorsesSetupAsync();

      var targetHorse = this.Info.Value.Horses.FirstOrDefault(h => h.Data.Key == horseKey);
      if (targetHorse != null)
      {
        targetHorse.IsActive.Value = true;
      }
    }

    private void LoadCurrentRace(string? key = null)
    {
      logger.Info($"レース {key} のロードを開始します");
      this.IsError.Value = false;

      Task.Run(async () =>
      {
        try
        {
          this.IsFirstLoadStarted.Value = true;

          // 現在のレースを更新した場合、必要な情報を記録する
          var oldSelectedHorseId = 0u;
          var oldInfo = this.Info.Value;
          var isUpdateCurrentInfo = false;
          if (oldInfo != null && oldInfo.Data.Key == key)
          {
            isUpdateCurrentInfo = true;

            if (!this.IsSelectedAllHorses.Value)
            {
              oldSelectedHorseId = oldInfo.ActiveHorse.Value?.Data.Id ?? 0u;
              logger.Info($"現在のレースの更新のようです。選択中馬ID: {oldSelectedHorseId}");
            }
          }

          if (oldInfo != null && !oldInfo.IsAvoidCaching)
          {
            RaceInfoCacheManager.Register(oldInfo);
          }

          this.ticketUpdated?.Dispose();

          var raceKey = key ?? this.RaceKey.Value;

          var race = await RaceInfo.FromKeyAsync(raceKey);
          this.Info.Value = race;

          if (race == null)
          {
            logger.Warn($"ID {key} のレースが正常に読み込めませんでした");
            return;
          }

          if (this.Info.Value?.Payoff != null)
          {
            // 払い戻し情報をもとに、払い戻し額をレースリストに表示する
            this.ticketUpdated = this.Info.Value.Payoff.Income.SkipWhile(i => i == 0).Subscribe(income =>
            {
              if (race != null)
              {
                this.RaceList.UpdatePayoff(race.Data.Key, income, this.Info.Value.Payoff.PayMoneySum.Value > 0 || this.Info.Value.Payoff.ReturnMoneySum.Value > 0);
              }
            });
          }
          else
          {
            // レースリストには、購入した馬券の点数をそのまま表示する
            race!.WaitTicketsAndCallback(tickets =>
            {
              Action act = () =>
              {
                if (tickets.Tickets.Any())
                {
                  var money = tickets.Tickets.Sum(t => t.Count.Value * t.Rows.Count * 100);
                  this.RaceList.UpdatePayoff(race.Data.Key, money * -1, true);
                }
                else
                {
                  this.RaceList.UpdatePayoff(race.Data.Key, 0, false);
                }
              };

              this.ticketUpdated = tickets.Tickets.CollectionChangedAsObservable()
                .Concat(Observable.FromEvent<EventHandler, EventArgs>(
                  a => (s, e) => a(e),
                  dele => tickets.Tickets.TicketCountChanged += dele,
                  dele => tickets.Tickets.TicketCountChanged -= dele))
                .Subscribe(_ => act());
            });
          }

          await race.WaitHorsesSetupAsync();
          logger.Info("すべての馬情報のロード完了を検出");

          var isCached = RaceInfoCacheManager.TryApplyTrendAnalyzers(race);
          logger.Info($"キャッシュ検出: {isCached}");

          if (this.IsViewExpection.Value)
          {
            // レースの更新時に馬情報が空になるのを修正する
            if (oldSelectedHorseId != 0)
            {
              race.SetActiveHorse(oldSelectedHorseId);
            }
            else
            {
              this.IsSelectedAllHorses.Value = true;
            }
          }
          else
          {
            this.IsSelectedAllHorses.Value = true;
          }
          if (!race.HasResults.Value && this.IsViewResult.Value)
          {
            // 切り替える前のレースで結果を表示／新しく切り替わったレースに結果はないとき、表示を切り替える
            this.IsViewExpection.Value = true;
            this.IsViewResult.Value = false;
          }

          if (oldInfo != null)
          {
            if (isUpdateCurrentInfo)
            {
              //await race.CopyTrendAnalyzersFromAsync(oldInfo);
            }
            oldInfo.Dispose();
            logger.Debug("旧オブジェクトの破棄完了");
          }
        }
        catch (Exception ex)
        {
          logger.Error($"レースの {key} への切り替えでエラーが発生しました", ex);
          this.IsError.Value = true;
          this.ErrorMessage.Value = "レースの切り替えでエラーが発生しました";

          if (this.Info.Value != null)
          {
            this.Info.Value.Dispose();
            this.Info.Value = null;
          }
        }
      });
    }

    public void Dispose()
    {
      logger.Info($"レース {this.RaceKey.Value} 保持中のモデルは破棄されます");
      this._disposables.Dispose();
      this.Info.Value?.Dispose();
      this.RaceList.Dispose();
      logger.Debug("破棄が完了しました");
    }
  }
}
