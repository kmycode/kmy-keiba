using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Injection;
using KmyKeiba.Models.Race.Memo;
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
    private CompositeDisposable _currentRaceDisposables = new();

    private string _raceKey = string.Empty;

    public ReactiveProperty<RaceInfo?> Info { get; } = new();

    public RaceList RaceList { get; } = new();

    public ReactiveProperty<bool> IsLoaded { get; }

    public ReactiveProperty<bool> IsFirstLoadStarted { get; } = new();

    public ReactiveProperty<bool> IsViewExpection { get; } = new();

    public ReactiveProperty<bool> IsViewResult { get; } = new();

    public ReactiveProperty<bool> IsSelectedAllHorses { get; } = new(true);

    public ReactiveProperty<string> FirstMessage { get; } = new();

    public ReactiveProperty<string> ErrorMessage { get; } = new();

    public ReactiveProperty<bool> IsError { get; } = new();

    public RaceModel()
    {
      this.IsLoaded = this.Info
        .Select(i => i != null)
        .ToReactiveProperty()
        .AddTo(this._disposables);

      {
        var firstMessages = new[]
        {
          "左側のタイムラインで各レースを右クリックすれば\n新規ウィンドウで開きます",
          "レース画面でレースのタイトルをクリックすれば\nどの画面からでもレースのメモを編集できます",
          "レース馬一覧で馬の名前をクリックすれば\nメモを編集できます",
          "レース馬一覧で馬の単勝オッズをクリックすれば\n発走１時間前の時系列オッズが見れます",
          "分析画面で、レースを多角的に分析できます\n分析項目は「設定」で編集できます",
          "拡張メモを編集して、馬の様々なデータを記録しましょう",
          "強力な検索機能はレース画面の中にあります",
          "血統検索では、同じ間柄の馬を複数登録すると\nそこだけOR検索となります",
          "一括実行は寝ている間にやりましょう",
          "土日以外の中央競馬がある時は\nRT設定のチェックを外すのを忘れずに",
          "データ保存による操作不能がしつこい時は\n一時的に自動更新を止めることができます",
          "右下のVerボタンが緑色になっていれば\nアップデート可能です\nボタンを押すとダウンロード先リンクが表示されます",
          "[Ver.5変更点]\n拡張分析の各項目編集ならびに外部指数の設定は、\n設定画面に移動しました",
        };
        var index = new Random().Next(firstMessages.Length);
        ApplicationConfiguration.Current.Skip(1).Select(c => c.IsFirstMessageVisible).Subscribe(v =>
        {
          if (v)
          {
            this.FirstMessage.Value = firstMessages[index];
          }
          else
          {
            this.FirstMessage.Value = string.Empty;
          }
        }).AddTo(this._disposables);
      }

      this.RaceList.SelectedRaceKey.Subscribe(key =>
      {
        logger.Info($"レースキー変更 {this._raceKey} -> {key}");
        if (key != null)
        {
          this.SetRaceKey(key);
        }
        else
        {
          this.Info.Value = null;
        }
      }).AddTo(this._disposables);

      logger.Info("メインモデル初期化完了");
    }

    public void OnDatabaseInitialized()
    {
      Task.Run(async () =>
      {
        try
        {
          await this.RaceList.UpdateListAsync();
        }
        catch (Exception ex)
        {
          logger.Error($"{this.RaceList.Date.Value} のリスト作成でエラーが発生しました", ex);
        }
      });

      Task.Run(async () =>
      {
        return;

        // TODO: メモリリークを調べる

        var races = this.RaceList.Courses.SelectMany(c => c.Races);
        while (races.Count() < 71) await Task.Delay(50);
        races = races.ToArray();

        try
        {
          foreach (var race in races)
          {
            var oldInfo = this.Info.Value;

            ThreadUtil.InvokeOnUiThread(() =>
            {
              race.OnSelected();
            });

            while (this.Info.Value == oldInfo || this.Info.Value?.IsLoadCompleted.Value != true)
            {
              await Task.Delay(50);
            }
          }
        }
        catch (Exception ex)
        {

        }
      });
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
          logger.Warn($"選択中のレース {this._raceKey} が正常にロードされていません");
        }
      });
    }

    public void SetRaceKey(string key)
    {
      if (this._raceKey == key) return;

      this._raceKey = key;
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
          if (oldInfo != null)
          {
            oldInfo.UpdateCache();
          }
          if (oldInfo != null && oldInfo.Data.Key == key)
          {
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

          if (oldInfo?.Tickets.Value != null)
          {
            oldInfo.Tickets.Value.Tickets.TicketCountChanged -= this.Tickets_TicketCountChanged;
          }
          this._currentRaceDisposables.Dispose();
          this._currentRaceDisposables = new();

          var raceKey = key ?? this._raceKey;

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
            this.Info.Value.Payoff.Income.SkipWhile(i => i == 0).Subscribe(income =>
            {
              if (race != null)
              {
                this.RaceList.UpdatePayoff(race.Data.Key, income, this.Info.Value.Payoff.PayMoneySum.Value > 0 || this.Info.Value.Payoff.ReturnMoneySum.Value > 0);
              }
            }).AddTo(this._currentRaceDisposables);
          }
          else
          {
            // レースリストには、購入した馬券の点数をそのまま表示する
            race!.WaitTicketsAndCallback(tickets =>
            {
              tickets.Tickets.CollectionChangedAsObservable()
                .Subscribe(_ => this.Tickets_TicketCountChanged(null, EventArgs.Empty))
                .AddTo(this._currentRaceDisposables);

              // FromEventPatternはなぜか動かない
              tickets.Tickets.TicketCountChanged += this.Tickets_TicketCountChanged;
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
            oldInfo.Dispose();
            logger.Debug("旧オブジェクトの破棄完了");
          }

          while (race.MemoEx.Value == null)
          {
            await Task.Delay(100);
          }
          Observable.FromEventPattern<PointLabelChangedEventArgs>(
            ev => race.MemoEx.Value.PointLabelChangedForRaceList += ev,
            ev => race.MemoEx.Value.PointLabelChangedForRaceList -= ev)
          .Subscribe(ev =>
          {
            this.RaceList.UpdateColor(ev.EventArgs.Color, ev.EventArgs.IsVisible);
          }).AddTo(this._currentRaceDisposables);
          Observable.FromEventPattern(
            ev => race.MemoEx.Value.PointLabelOrderChangedForRaceList += ev,
            ev => race.MemoEx.Value.PointLabelOrderChangedForRaceList -= ev)
          .Subscribe(async _ =>
          {
            await this.RaceList.UpdateAllColorsAsync();
          }).AddTo(this._currentRaceDisposables);

          race.HasCheckedHorse.Subscribe(isChecked =>
          {
            this.RaceList.UpdateHasCheckedHorse(race.Data.Key, isChecked);
          }).AddTo(this._currentRaceDisposables);
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

    private void Tickets_TicketCountChanged(object? sender, EventArgs e)
    {
      var tickets = this.Info.Value?.Tickets.Value;
      if (tickets == null)
      {
        return;
      }
      var race = this.Info.Value!;

      if (tickets.Tickets.Any())
      {
        var money = tickets.Tickets.Sum(t => t.Count.Value * t.Rows.Count * 100);
        this.RaceList.UpdatePayoff(race.Data.Key, money * -1, true);
      }
      else
      {
        this.RaceList.UpdatePayoff(race.Data.Key, 0, false);
      }
    }

    public void Dispose()
    {
      logger.Info($"レース {this._raceKey} 保持中のモデルは破棄されます");
      this._disposables.Dispose();
      this.Info.Value?.Dispose();
      this.RaceList.Dispose();
      this._currentRaceDisposables.Dispose();
      logger.Debug("破棄が完了しました");
    }
  }
}
