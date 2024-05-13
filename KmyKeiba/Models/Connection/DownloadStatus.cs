using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Common;
using KmyKeiba.Models.Data;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Connection
{
  internal class DownloadStatus
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public static DownloadStatus Instance => _instance;
    private static readonly DownloadStatus _instance = new();
    private bool _isInitializationDownloading = false;

    public ReactiveProperty<bool> CanSaveOthers { get; } = new();

    public ReactiveProperty<bool> IsLongDownloadMonth { get; } = new();

    public ReactiveProperty<bool> IsError { get; } = new();
    public ReactiveProperty<bool> IsRTError { get; } = new();
    public ReactiveProperty<string> ErrorMessage { get; } = new();
    public ReactiveProperty<string> RTErrorMessage { get; } = new();

    public ReactiveProperty<bool> IsBusy => DownloaderConnector.Instance.IsBusy;
    public ReactiveProperty<bool> IsRTBusy => DownloaderConnector.Instance.IsRTBusy;

    public ReactiveProperty<bool> IsDownloading { get; } = new();
    public ReactiveProperty<bool> IsRTDownloading { get; } = new();

    public ReactiveProperty<bool> IsProcessing { get; } = new();
    public ReactiveProperty<bool> IsRTProcessing { get; } = new();

    public ReactiveProperty<DownloadingType> DownloadingType { get; } = new();
    public ReactiveProperty<DownloadLink> DownloadingLink { get; } = new();
    public ReactiveProperty<DownloadLink> RTDownloadingLink { get; } = new();
    public ReactiveProperty<int> DownloadingYear { get; } = new();
    public ReactiveProperty<int> DownloadingMonth { get; } = new();

    public ReactiveProperty<LoadingProcessValue> LoadingProcess { get; } = new();
    public ReactiveProperty<LoadingProcessValue> RTLoadingProcess { get; } = new();
    public ReactiveProperty<DownloadingDataspec> RTDownloadingDataspec { get; } = new();

    public ReactiveProperty<ProcessingStep> ProcessingStep { get; } = new();
    public ReactiveProperty<ProcessingStep> RTProcessingStep { get; } = new();

    public ReactiveProperty<int> ProcessingProgress { get; } = new();
    public ReactiveProperty<int> ProcessingProgressMax { get; } = new();
    public ReactiveProperty<bool> HasProcessingProgress { get; } = new();

    public ReactiveProperty<bool> CanCancel { get; } = new();
    public ReactiveProperty<bool> IsCancelProcessing { get; } = new();

    public ReactiveProperty<StatusFeeling> DownloadingStatus { get; }
    public ReactiveProperty<StatusFeeling> RTDownloadingStatus { get; }

    public ReactiveProperty<bool> IsRTPaused { get; } = new();

    private DownloadStatus()
    {
      var postProcesses = new[]
      {
        Connection.ProcessingStep.StandardTime,
        Connection.ProcessingStep.PreviousRaceDays,
        Connection.ProcessingStep.RiderWinRates,
        Connection.ProcessingStep.HorseExtraData,
        Connection.ProcessingStep.ResetHorseExtraData,
        Connection.ProcessingStep.RunningStyle,
        Connection.ProcessingStep.MigrationFrom250,
        Connection.ProcessingStep.MigrationFrom322,
        Connection.ProcessingStep.MigrationFrom430,
        Connection.ProcessingStep.MigrationFrom500,
      };

      this.DownloadingStatus =
        this.ProcessingStep
          .Select(p => !postProcesses.Contains(p))
          .CombineLatest(this.LoadingProcess, (step, process) => step && process != LoadingProcessValue.Writing)
          .CombineLatest(JrdbDownloaderModel.Instance.CanSaveOthers, (a, b) => a && b)
          .Select(b => b ? StatusFeeling.Standard : StatusFeeling.Bad)
          .ToReactiveProperty();
      this.RTDownloadingStatus =
        this.RTProcessingStep
          .Select(p => !postProcesses.Contains(p))
          //.CombineLatest(this.RTLoadingProcess, (step, process) => step && process != LoadingProcessValue.Writing)
          .Select(b => b ? StatusFeeling.Standard : StatusFeeling.Bad)
          .ToReactiveProperty();

      this.DownloadingStatus.Subscribe(_ => UpdateCanSave());
      this.RTDownloadingStatus.Subscribe(_ => UpdateCanSave());
      this.ProcessingStep.Subscribe(_ => UpdateCanSave());
      JrdbDownloaderModel.Instance.CanSaveOthers.Subscribe(_ => UpdateCanSave());
      JrdbDownloaderModel.Instance.DownloadingYear.Skip(1).Subscribe(val => this.DownloadingYear.Value = val);
      JrdbDownloaderModel.Instance.DownloadingMonth.Skip(1).Subscribe(val => this.DownloadingMonth.Value = val);
    }

    private void UpdateCanSave()
    {
      var canSave = this.DownloadingStatus.Value != StatusFeeling.Bad && this.RTDownloadingStatus.Value != StatusFeeling.Bad &&
        JrdbDownloaderModel.Instance.CanSaveOthers.Value;
      var canCancel = canSave || this.IsProcessing.Value || !JrdbDownloaderModel.Instance.CanSaveOthers.Value;
      if (this.CanSaveOthers.Value != canSave || this.CanCancel.Value != canCancel)
      {
        // このプロパティはViewModel内のReactiveCommandのCanExecuteにも使われる
        // この場合、UIスレッドから書き換えないとエラーになるっぽい
        ThreadUtil.InvokeOnUiThread(() =>
        {
          this.CanSaveOthers.Value = canSave;
          this.CanCancel.Value = canCancel;
        });

        logger.Debug($"他のスレッドからDBに保存可能: {canSave}, キャンセル可能: {canCancel}");
      }
    }

    public async Task OnDownloadProgress(DownloaderTaskData task)
    {
      var config = DownloadConfig.Instance;

      var p = task.Parameter.Split(',');
      int.TryParse(p[0], out var year);
      int.TryParse(p[1], out var month);
      var mode = p[3];

      this.HasProcessingProgress.Value = true;
      this.ProcessingProgress.Value = task.Progress;
      this.ProcessingProgressMax.Value = task.ProgressMax;

      this.DownloadingYear.Value = year;
      this.DownloadingMonth.Value = month;

      this.LoadingProcess.Value = task.Result switch
      {
        "opening" => LoadingProcessValue.Opening,
        "downloading" => LoadingProcessValue.Downloading,
        "loading" => LoadingProcessValue.Loading,
        "writing" => LoadingProcessValue.Writing,
        "processing" => LoadingProcessValue.Processing,
        "closing" => LoadingProcessValue.Closing,
        "checkingjravannews" => LoadingProcessValue.CheckingJraVanNews,
        _ => LoadingProcessValue.Unknown,
      };
      this.DownloadingType.Value = mode switch
      {
        "race" => Connection.DownloadingType.Race,
        "odds" => Connection.DownloadingType.Odds,
        _ => default,
      };

      // 現在ダウンロード中の年月を保存する
      if (!this._isInitializationDownloading)
      {
        if (task.Parameter.Contains("central") && (config.CentralDownloadedMonth.Value != month || config.CentralDownloadedYear.Value != year))
        {
          config.CentralDownloadedMonth.Value = month;
          config.CentralDownloadedYear.Value = year;
          using var db = new MyContext();
          await ConfigUtil.SetIntValueAsync(db, SettingKey.LastDownloadCentralDate, year * 100 + month);
          logger.Info($"{year}/{month:00} の中央競馬通常データダウンロード完了");

          this.IsLongDownloadMonth.Value = year == 2002 && month == 12;
        }
        if (task.Parameter.Contains("local") && (config.LocalDownloadedMonth.Value != month || config.LocalDownloadedYear.Value != year))
        {
          config.LocalDownloadedMonth.Value = month;
          config.LocalDownloadedYear.Value = year;
          using var db = new MyContext();
          await ConfigUtil.SetIntValueAsync(db, SettingKey.LastDownloadLocalDate, year * 100 + month);
          logger.Info($"{year}/{month:00} の地方競馬通常データダウンロード完了");
        }
      }
    }

    public Task OnRTDownloadProgress(DownloaderTaskData task)
    {
      var p = task.Parameter.Split(',');

      this.RTDownloadingDataspec.Value = p[2] switch
      {
        "1" => DownloadingDataspec.RB12,
        "2" => DownloadingDataspec.RB15,
        "3" => DownloadingDataspec.RB30,
        "4" => DownloadingDataspec.RB11,
        "5" => DownloadingDataspec.RB14,
        "6" => DownloadingDataspec.RB41,
        "7" => DownloadingDataspec.RB13,
        "8" => DownloadingDataspec.RB17,
        _ => DownloadingDataspec.Unknown,
      };

      this.RTLoadingProcess.Value = task.Result switch
      {
        "opening" => LoadingProcessValue.Opening,
        "downloading" => LoadingProcessValue.Downloading,
        "loading" => LoadingProcessValue.Loading,
        "writing" => LoadingProcessValue.Writing,
        "processing" => LoadingProcessValue.Processing,
        "closing" => LoadingProcessValue.Closing,
        "checkingjravannews" => LoadingProcessValue.CheckingJraVanNews,
        _ => LoadingProcessValue.Unknown,
      };

      return Task.CompletedTask;
    }
  }
}
