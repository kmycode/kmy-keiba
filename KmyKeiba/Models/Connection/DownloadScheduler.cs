using CefSharp.DevTools.Network;
using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Connection.Connector;
using KmyKeiba.Models.Connection.PostProcess;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static KmyKeiba.Models.Analysis.RaceHorseMatchResult;

namespace KmyKeiba.Models.Connection
{
    internal class DownloadScheduler
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public static DownloadScheduler Instance => _instance ??= new();
    private static DownloadScheduler? _instance;

    private readonly ConnectorCollection _connectors = ConnectorCollection.GenerateDefaults();

    private bool _isInitializationDownloading = false;
    private DateTime _lastUpdatedToday = default;
    private DateTime _lastUpdatedPlanOfRace = default;
    private DateTime _lastUpdatedPreviousRace = default;
    private int _lastStandardTimeUpdatedYear = default;
    private bool _isUpdateRtForce;
    private bool _isUpdateRtHeavyForce;

    private DownloadConfig Config => DownloadConfig.Instance;

    public ReactiveProperty<int> NextRTUpdateSeconds { get; } = new();
    public ReactiveProperty<bool> IsWaitingNextRTUpdate { get; } = new();

    private DownloadScheduler()
    {
    }

    public Task BeginRTDownloadLoopAsync()
    {
      Task.Run(async () =>
      {
        await this.FirstDownloadOnAppLaunchAsync();
        await UpdateDiffAsync();

        await this.LoopAsync();
      });

      return Task.CompletedTask;
    }

    private async Task FirstDownloadOnAppLaunchAsync()
    {
      this._isInitializationDownloading = true;
      DateTime lastLaunch;
      bool hasUnknownResultRaces = false;

      using (var db = new MyContext())
      {
        await db.TryBeginTransactionAsync();

        this._lastStandardTimeUpdatedYear = ConfigUtil.GetIntValue(SettingKey.LastUpdateStandardTimeYear);
        logger.Info($"最後に標準タイムを更新した年: {this._lastStandardTimeUpdatedYear}");

        {
          if (DateTime.TryParse(ConfigUtil.GetStringValue(SettingKey.LastDownloadPlanOfRaceDate), out var lastDownloadedPlanOfRaceDate))
          {
            this._lastUpdatedPlanOfRace = lastDownloadedPlanOfRaceDate;
          }

          if (DateTime.TryParse(ConfigUtil.GetStringValue(SettingKey.LastDownloadPreviousRaceDate), out var lastDownloadPreviousRaceDate))
          {
            this._lastUpdatedPreviousRace = lastDownloadPreviousRaceDate;
          }

          if (DateTime.TryParse(ConfigUtil.GetStringValue(SettingKey.LastLaunchDateEx), out lastLaunch))
          {
            logger.Info($"最終起動日: {lastLaunch:yyyy/MM/dd}");

            var minDate = DateTime.Today.AddMonths(-3);
            if (lastLaunch < minDate)
            {
              lastLaunch = minDate;
              logger.Info($"最終更新日が３か月より前だったので調整します {lastLaunch:yyyy/MM/dd}");
            }

            if (lastLaunch.Date != DateTime.Today &&
              await db.Races!.AnyAsync(r => r.StartTime >= lastLaunch && r.StartTime <= DateTime.Today && r.DataStatus < JVLink.Entities.RaceDataStatus.PreliminaryGradeFull))
            {
              hasUnknownResultRaces = true;
            }
          }

          await ConfigUtil.SetStringValueAsync(db, SettingKey.LastLaunchDateEx, DateTime.Now.ToString());
        }

        logger.Info("初期ダウンロード完了");
        this._isInitializationDownloading = false;

        await db.SaveChangesAsync();
        await db.CommitAsync();
      }

      // 前回起動から今日までのレース結果をダウンロード
      if (hasUnknownResultRaces)
      {
        await this.DownloadLinkAsync(lastLaunch.Year, lastLaunch.Month);
      }
    }

    private async Task DownloadLinkAsync(int year, int month)
    {
      var connectors = new IConnector[]
      {
          Connectors.Central,
          Connectors.Local,
      };
      foreach (var connector in connectors.Where(c => c.IsRTAvailable.Value))
      {
        await connector.DownloadAsync(new DateOnly(year, month, 1), DateOnly.FromDateTime(DateTime.Today));
      }

      await PostProcessing.RunAsync(DownloadStatus.Instance.RTProcessingStep, true, PostProcessings.AfterDownload);
    }

    private async Task UpdateDiffAsync()
    {
      var now = DateTime.Now;
      var state = DownloadStatus.Instance;

      var isPostProcess = false;

      if (!state.CanSaveOthers.Value || JrdbDownloaderModel.Instance.IsDownloading.Value) return;

      // 年を跨ぐ場合は基準タイムの更新も行う
      if (this._lastStandardTimeUpdatedYear != now.Year)
      {
        logger.Info($"標準タイムを再計算します");
        await PostProcessings.StandardTime.RunAsync();
        this._lastStandardTimeUpdatedYear = now.Year;
      }

      if (now - this._lastUpdatedPreviousRace >= TimeSpan.FromHours(8) || this._isUpdateRtHeavyForce)
      {
        await this.DownloadPreviousDayResultsAsync();

        this._lastUpdatedPreviousRace = now;
        await ConfigUtil.SetStringValueAsync(SettingKey.LastDownloadPreviousRaceDate, now.ToString());
        isPostProcess = true;
      }

      if (now - this._lastUpdatedPlanOfRace >= TimeSpan.FromHours(4) || this._isUpdateRtHeavyForce)
      {
        await this.DownloadPlanOfRacesAsync();

        this._lastUpdatedPlanOfRace = now;
        await ConfigUtil.SetStringValueAsync(SettingKey.LastDownloadPlanOfRaceDate, now.ToString());

        this._isUpdateRtHeavyForce = false;
        isPostProcess = true;
      }

      if (now - this._lastUpdatedToday >= TimeSpan.FromMinutes(5) || this._isUpdateRtForce)
      {
        await this.DownloadTodayNewsAsync();

        this._lastUpdatedToday = now;
        this._isUpdateRtForce = false;
        isPostProcess = true;
      }

      if (isPostProcess && this._connectors.Any(c => c.IsRTAvailable.Value))
      {
        await this.RTPostProcessingAsync();
      }

      this.IsWaitingNextRTUpdate.Value = true;
    }

    private async Task LoopAsync()
    {
      var state = DownloadStatus.Instance;

      while (true)
      {
        this.IsWaitingNextRTUpdate.Value = true;

        while ((DateTime.Now - this._lastUpdatedToday).TotalMinutes < 5 && !this._isUpdateRtForce && !this._isUpdateRtHeavyForce)
        {
          // ５分に１回ずつ更新する
          this.NextRTUpdateSeconds.Value = 60 * 5 - (int)(DateTime.Now - this._lastUpdatedToday).TotalSeconds;
          await Task.Delay(1000);
        }

        if (!this._connectors.Any(c => c.IsRTAvailable.Value))
        {
          await Task.Delay(1000);
          continue;
        }

        this.NextRTUpdateSeconds.Value = 0;

        if (state.IsRTPaused.Value)
        {
          logger.Debug("一時停止の設定を検出");
        }
        while (state.IsRTPaused.Value && !this._isUpdateRtForce && !this._isUpdateRtHeavyForce)
        {
          // 一時停止
          await Task.Delay(1000);
        }

        this.IsWaitingNextRTUpdate.Value = false;

        await this.UpdateDiffAsync();
      }
    }

    private async Task<bool> DownloadPlanOfRacesAsync()
    {
      var today = DateOnly.FromDateTime(DateTime.Today);

      var days = new[]
      {
        today.AddDays(1),
        today.AddDays(2),
        today.AddDays(3),
      };

      var isSucceed = await this.DownloadRTWithDaysAsync(days);

      if (isSucceed)
      {
        // 通常レース予定は今週データから取得するのだが、地方・中央ともに通常データ取得でもいけるかも
        await this.DownloadLinkAsync(today.Year, today.Month);
      }

      return isSucceed;
    }

    private async Task<bool> DownloadTodayNewsAsync()
    {
      return await this.DownloadRTWithDaysAsync(new[] { DateOnly.FromDateTime(DateTime.Today) });
    }

    private async Task<bool> DownloadPreviousDayResultsAsync()
    {
      var today = DateOnly.FromDateTime(DateTime.Today);

      var days = new[]
      {
        today.AddDays(-1),
        today.AddDays(-2),
        today.AddDays(-3),
      };

      return await this.DownloadRTWithDaysAsync(days);
    }

    private async Task<bool> DownloadRTWithDaysAsync(IEnumerable<DateOnly> days)
    {
      var isSucceed = true;

      var state = DownloadStatus.Instance;

      foreach (var day in days)
      {
        await this._connectors.DownloadRTAsync(day, day.AddDays(1));

        if (state.IsRTError.Value)
        {
          isSucceed = false;
          break;
        }
      }

      return isSucceed;
    }

    private async Task RTPostProcessingAsync()
    {
      await PostProcessing.RunAsync(DownloadStatus.Instance.RTProcessingStep, true, PostProcessings.AfterRTDownload);
    }

    public void UpdateRtDataForce()
    {
      this._isUpdateRtForce = true;
    }

    public void UpdateRtDataHeavyForce()
    {
      this._isUpdateRtHeavyForce = true;
    }
  }
}
