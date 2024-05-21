using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Common;
using KmyKeiba.Models.Connection.Connector;
using KmyKeiba.Models.Connection.PostProcess;
using KmyKeiba.Models.Data;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.LinkLabel;

namespace KmyKeiba.Models.Connection
{
    internal class DownloaderModel
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public static DownloaderModel Instance => _instance ??= new();
    private static DownloaderModel? _instance;

    private readonly ConnectorCollection _connectors = ConnectorCollection.GenerateDefaults();

    public JrdbConnector Jrdb => Connectors.Jrdb;

    public DownloadConfig Config => DownloadConfig.Instance;

    public DownloadStatus State => DownloadStatus.Instance;

    public DownloadScheduler Scheduler => DownloadScheduler.Instance;

    public ReactiveProperty<bool> IsInitialized { get; } = new();
    public ReactiveProperty<bool> IsInitializationError { get; } = new();

    public ReactiveProperty<bool> CanSaveOthers => this.State.CanSaveOthers;

    public event EventHandler? RacesUpdated;
    public void OnRacesUpdated() => this.RacesUpdated?.Invoke(this, EventArgs.Empty);

    private DownloaderModel()
    {
      logger.Debug("ダウンロードモデルの初期化");
    }

    public async Task<bool> InitializeAsync(IReactiveProperty<string> message)
    {
      logger.Info("ダウンローダの初期処理開始");

      var downloader = DownloaderConnector.Instance;

      // 初回起動
      var isFirstLaunch = !downloader.IsExistsDatabase;

      try
      {
        message.Value = "ダウンローダとの接続を初期化中...";
        await downloader.InitializeAsync();
        this.IsInitialized.Value = true;

        {
          using var db = new MyContext();
          await db.TryBeginTransactionAsync();

          message.Value = "ダウンロード設定を初期化中...";
          await DownloadConfig.Instance.InitializeAsync(db);
          await Connectors.Jrdb.InitializeAsync(db);

          await db.SaveChangesAsync();
          await db.CommitAsync();
        }

        // このメソッドは内部でトランザクションをおこなう
        await this.MigrateDatabaseAsync(message);

        logger.Info("初期化完了、最新データのダウンロードを開始");

        // 最新情報をダウンロードする
        await this.Scheduler.BeginRTDownloadLoopAsync();
      }
      catch (DownloaderCommandException ex)
      {
        logger.Fatal("初期化でダウンローダとの連携時にエラーが発生", ex);
        this.IsInitializationError.Value = true;
        this.State.ErrorMessage.Value = !string.IsNullOrEmpty(ex.Message) ? ex.Message : ex.Error.GetErrorText();
        isFirstLaunch = false;
      }
      catch (Exception ex)
      {
        logger.Fatal("初期化で想定外のエラーが発生", ex);
        this.IsInitializationError.Value = true;
        this.State.ErrorMessage.Value = "予期しないエラーが発生しました";
        isFirstLaunch = false;
      }

      return isFirstLaunch;
    }

    private async Task MigrateDatabaseAsync(IReactiveProperty<string> message)
    {
      var state = DownloadStatus.Instance;

      // データベースのマイグレーション処理を自動的に開始
      var dbver = ConfigUtil.GetIntValue(SettingKey.DatabaseVersion);

      if (dbver < 500)
      {
        var initializations = new PostProcessingCollection();
        if (dbver < 250) initializations.Add(PostProcessings.MigrateFrom250);
        if (dbver < 322) initializations.Add(PostProcessings.MigrateFrom322);
        if (dbver < 430) initializations.Add(PostProcessings.MigrateFrom430);
        if (dbver < 500) initializations.Add(PostProcessings.MigrateFrom500);

        message.Value = "データベースのマイグレーション中...";
        await PostProcessing.RunAsync(state.ProcessingStep, false, initializations);

        await ConfigUtil.SetIntValueAsync(SettingKey.DatabaseVersion, 500);
      }
    }

    public async Task OpenJvlinkConfigAsync()
    {
      var state = DownloadStatus.Instance;

      state.IsError.Value = false;
      logger.Info("JVLink設定を開きます");

      try
      {
        await DownloaderConnector.Instance.OpenJvlinkConfigAsync();
      }
      catch (Exception ex)
      {
        logger.Error("JVLink設定を開こうとしましたがエラーが発生しました", ex);
        state.IsError.Value = true;
        state.ErrorMessage.Value = ex.Message;
      }
    }

    public async Task OpenNvlinkConfigAsync()
    {
      var state = DownloadStatus.Instance;

      state.IsError.Value = false;
      logger.Info("NVLink設定を開きます");

      try
      {
        await DownloaderConnector.Instance.OpenNvlinkConfigAsync();
      }
      catch (Exception ex)
      {
        logger.Error("NVLink設定を開こうとしましたがエラーが発生しました", ex);
        state.IsError.Value = true;
        state.ErrorMessage.Value = ex.Message;
      }
    }

    public void BeginDownload()
    {
      var state = DownloadStatus.Instance;
      var config = DownloadConfig.Instance;

      Task.Run(async () =>
      {
        if (config.IsBuildMasterData.Value)
        {
          logger.Info("マスターデータ更新を開始");
        }
        else
        {
          logger.Info($"セットアップデータダウンロードを開始");
          await this._connectors.DownloadAsync(new DateOnly(config.StartYear.Value, config.StartMonth.Value, 1));
        }

        await PostProcessing.RunAsync(state.ProcessingStep, false, PostProcessings.AfterDownload);
      });
    }

    public void UpdateRtDataForce() => this.Scheduler.UpdateRtDataForce();

    public void UpdateRtDataHeavyForce() => this.Scheduler.UpdateRtDataHeavyForce();

    public void CancelDownload()
    {
      var state = DownloadStatus.Instance;

      if (JrdbDownloaderModel.Instance.IsDownloading.Value)
      {
        JrdbDownloaderModel.Instance.IsCanceled.Value = true;
      }
      else
      {
        DownloaderConnector.Instance.CancelCurrentTask();
        state.IsCancelProcessing.Value = true;
      }
      logger.Warn("ダウンロードが中止されました");
    }

    public void InterruptDownload()
    {
      if (JrdbDownloaderModel.Instance.IsDownloading.Value) return;

      DownloaderConnector.Instance.InterruptCurrentTask();
      DownloadStatus.Instance.HasInterruptedDownloadTask.Value = true;
    }

    public void ResumeDownload()
    {
      DownloadStatus.Instance.HasInterruptedDownloadTask.Value = false;

      Task.Run(async () =>
      {
        await this._connectors.ResumeDownloadAsync();
      });
    }

    public void BeginResetHorseExtraData()
    {
      Task.Run(async () =>
      {
        logger.Info("拡張データのリセットを開始");
        await PostProcessings.ResetHorseExtraData.RunAsync();
      });
    }
  }
}
