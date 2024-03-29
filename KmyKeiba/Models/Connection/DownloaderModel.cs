﻿using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Common;
using KmyKeiba.Models.Data;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace KmyKeiba.Models.Connection
{
  internal class DownloaderModel : IDisposable
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public static DownloaderModel Instance => _instance ??= new();
    private static DownloaderModel? _instance;

    private bool _isUpdateRtForce = false;
    private bool _isUpdateRtHeavyForce = false;
    private readonly CompositeDisposable _disposables = new();
    private readonly DownloaderConnector _downloader = DownloaderConnector.Instance;
    public ReactiveProperty<bool> IsCancelProcessing { get; } = new();

    private bool _isInitializationDownloading = false;

    private Connection.ProcessingStep AllSteps => this.IsBuildExtraData.Value ? Connection.ProcessingStep.All : (Connection.ProcessingStep.All & ~Connection.ProcessingStep.HorseExtraData);

    public ReactiveProperty<bool> IsInitializationError { get; } = new();

    public ReactiveProperty<bool> IsError { get; } = new();

    public ReactiveProperty<bool> IsRTError { get; } = new();

    public ReactiveProperty<bool> IsBusy => this._downloader.IsBusy;

    public ReactiveProperty<bool> IsRTBusy => this._downloader.IsRTBusy;

    public ReactiveProperty<bool> IsDownloading { get; } = new();

    public ReactiveProperty<bool> IsProcessing { get; } = new();

    public ReactiveProperty<bool> IsLongDownloadMonth { get; } = new();

    public ReactiveProperty<bool> IsRTDownloading { get; } = new();

    public ReactiveProperty<bool> IsRTProcessing { get; } = new();

    public ReactiveProperty<string> ErrorMessage { get; } = new();

    public ReactiveProperty<string> RTErrorMessage { get; } = new();

    public ReactiveProperty<bool> IsInitialized { get; } = new();

    public ReactiveProperty<int> StartYear { get; } = new(1986);

    public ReactiveProperty<int> StartMonth { get; } = new(1);

    public IReadOnlyList<int> StartYearSelection { get; }

    public IReadOnlyList<int> StartMonthSelection { get; }

    public ReactiveProperty<DownloadingType> DownloadingType { get; } = new();

    public ReactiveProperty<DownloadLink> DownloadingLink { get; } = new();

    public ReactiveProperty<DownloadLink> RTDownloadingLink { get; } = new();

    public ReactiveProperty<int> DownloadingYear { get; } = new();

    public ReactiveProperty<int> DownloadingMonth { get; } = new();

    public ReactiveProperty<int> CentralDownloadedYear { get; } = new();

    public ReactiveProperty<int> CentralDownloadedMonth { get; } = new();

    public ReactiveProperty<int> LocalDownloadedYear { get; } = new();

    public ReactiveProperty<int> LocalDownloadedMonth { get; } = new();

    public ReactiveProperty<int> JrdbDownloadedYear { get; } = new();

    public ReactiveProperty<int> JrdbDownloadedMonth { get; } = new();

    public ReactiveProperty<bool> IsDownloadCentral { get; } = new();

    public ReactiveProperty<bool> IsDownloadLocal { get; } = new();

    public ReactiveProperty<bool> IsDownloadJrdb { get; } = new();

    public ReactiveProperty<bool> IsBuildMasterData { get; } = new();

    public ReactiveProperty<bool> IsRTDownloadCentral { get; } = new();

    public ReactiveProperty<bool> IsRTDownloadCentralAfterThursdayOnly { get; } = new();

    public ReactiveProperty<bool> IsRTDownloadLocal { get; } = new();

    public ReactiveProperty<bool> IsRTDownloadJrdb { get; } = new();

    public ReactiveProperty<bool> IsDownloadSlop { get; } = new();

    public ReactiveProperty<bool> IsDownloadBlod { get; } = new();

    public ReactiveProperty<bool> IsBuildExtraData { get; } = new();

    public ReactiveProperty<LoadingProcessValue> LoadingProcess { get; } = new();

    public ReactiveProperty<LoadingProcessValue> RTLoadingProcess { get; } = new();

    public ReactiveProperty<DownloadingDataspec> RTDownloadingDataspec { get; } = new();

    public ReactiveProperty<ProcessingStep> ProcessingStep { get; } = new();

    public ReactiveProperty<ProcessingStep> RTProcessingStep { get; } = new();

    public ReactiveProperty<int> ProcessingProgressMax { get; } = new();

    public ReactiveProperty<int> ProcessingProgress { get; } = new();

    public ReactiveProperty<bool> HasProcessingProgress { get; } = new();

    public ReactiveProperty<bool> CanSaveOthers { get; } = new();

    public ReactiveProperty<bool> CanCancel { get; } = new();

    public ReactiveProperty<StatusFeeling> DownloadingStatus { get; }

    public ReactiveProperty<StatusFeeling> RTDownloadingStatus { get; }

    public ReactiveProperty<int> NextRTUpdateSeconds { get; } = new();

    public ReactiveProperty<bool> IsWaitingNextRTUpdate { get; } = new();

    public ReactiveProperty<bool> IsRTPaused { get; } = new();

    public ReactiveProperty<string> JrdbId { get; } = new();

    public ReactiveProperty<string> JrdbPassword { get; } = new();

    private DownloaderModel()
    {
      logger.Debug("ダウンロードモデルの初期化");

      this.StartYearSelection = Enumerable.Range(1986, DateTime.Now.Year - 1986 + 1).ToArray();
      this.StartMonthSelection = Enumerable.Range(1, 12).ToArray();

      this.DownloadingStatus =
        this.ProcessingStep
          .Select(p => p != Connection.ProcessingStep.StandardTime && p != Connection.ProcessingStep.PreviousRaceDays && p != Connection.ProcessingStep.RiderWinRates && p != Connection.ProcessingStep.MigrationFrom250 && p != Connection.ProcessingStep.MigrationFrom322 && p != Connection.ProcessingStep.HorseExtraData && p != Connection.ProcessingStep.MigrationFrom430)
          .CombineLatest(this.LoadingProcess, (step, process) => step && process != LoadingProcessValue.Writing)
          .CombineLatest(JrdbDownloaderModel.Instance.CanSaveOthers, (a, b) => a && b)
          .Select(b => b ? StatusFeeling.Standard : StatusFeeling.Bad)
          .ToReactiveProperty().AddTo(this._disposables);
      this.RTDownloadingStatus =
        this.RTProcessingStep.Select(p => (p != Connection.ProcessingStep.StandardTime && p != Connection.ProcessingStep.PreviousRaceDays && p != Connection.ProcessingStep.RiderWinRates) ? StatusFeeling.Unknown : StatusFeeling.Bad)
        .ToReactiveProperty().AddTo(this._disposables);

      this.JrdbId.Skip(2).Subscribe(async val => await ConfigUtil.SetStringValueAsync(SettingKey.JrdbId, val)).AddTo(this._disposables);
      this.JrdbPassword.Skip(2).Subscribe(async val => await ConfigUtil.SetStringValueAsync(SettingKey.JrdbPassword, val)).AddTo(this._disposables);

      void UpdateCanSave()
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
      this.LoadingProcess.Subscribe(_ => UpdateCanSave()).AddTo(this._disposables);
      this.ProcessingStep.Subscribe(_ => UpdateCanSave()).AddTo(this._disposables);
      JrdbDownloaderModel.Instance.CanSaveOthers.Subscribe(_ => UpdateCanSave()).AddTo(this._disposables);
      JrdbDownloaderModel.Instance.DownloadingYear.Skip(1).Subscribe(val => this.DownloadingYear.Value = val).AddTo(this._disposables);
      JrdbDownloaderModel.Instance.DownloadingMonth.Skip(1).Subscribe(val => this.DownloadingMonth.Value = val).AddTo(this._disposables);

      // 設定を保存
      this.IsDownloadCentral.SkipWhile(_ => !this.IsInitialized.Value).Where(_ => !this.IsBuildMasterData.Value).Subscribe(async v => await ConfigUtil.SetIntValueAsync(SettingKey.IsDownloadCentral, v ? 1 : 0)).AddTo(this._disposables);
      this.IsDownloadLocal.SkipWhile(_ => !this.IsInitialized.Value).Where(_ => !this.IsBuildMasterData.Value).Subscribe(async v => await ConfigUtil.SetIntValueAsync(SettingKey.IsDownloadLocal, v ? 1 : 0)).AddTo(this._disposables);
      this.IsDownloadJrdb.SkipWhile(_ => !this.IsInitialized.Value).Where(_ => !this.IsBuildMasterData.Value).Subscribe(async v => await ConfigUtil.SetIntValueAsync(SettingKey.IsDownloadJrdb, v ? 1 : 0)).AddTo(this._disposables);
      this.IsRTDownloadCentralAfterThursdayOnly.SkipWhile(_ => !this.IsInitialized.Value).Subscribe(async v => await ConfigUtil.SetIntValueAsync(SettingKey.IsDownloadCentralOnThursdayAfterOnly, v ? 1 : 0)).AddTo(this._disposables);
      this.IsRTDownloadCentral.SkipWhile(_ => !this.IsInitialized.Value).Subscribe(async v => await ConfigUtil.SetIntValueAsync(SettingKey.IsRTDownloadCentral, v ? 1 : 0)).AddTo(this._disposables);
      this.IsRTDownloadLocal.SkipWhile(_ => !this.IsInitialized.Value).Subscribe(async v => await ConfigUtil.SetIntValueAsync(SettingKey.IsRTDownloadLocal, v ? 1 : 0)).AddTo(this._disposables);
      this.IsRTDownloadJrdb.SkipWhile(_ => !this.IsInitialized.Value).Subscribe(async v => await ConfigUtil.SetIntValueAsync(SettingKey.IsRTDownloadJrdb, v ? 1 : 0)).AddTo(this._disposables);
      this.IsDownloadBlod.SkipWhile(_ => !this.IsInitialized.Value).Subscribe(async v => await ConfigUtil.SetIntValueAsync(SettingKey.IsNotDownloadHorseBloods, v ? 0 : 1)).AddTo(this._disposables);
      this.IsDownloadSlop.SkipWhile(_ => !this.IsInitialized.Value).Subscribe(async v => await ConfigUtil.SetIntValueAsync(SettingKey.IsNotDownloadTrainings, v ? 0 : 1)).AddTo(this._disposables);
      this.IsBuildExtraData.SkipWhile(_ => !this.IsInitialized.Value).Subscribe(async v => await ConfigUtil.SetIntValueAsync(SettingKey.IsNotBuildExtraData, v ? 0 : 1)).AddTo(this._disposables);
    }

    public async Task<bool> InitializeAsync()
    {
      logger.Info("ダウンローダの初期処理開始");

      var downloader = this._downloader;
      var isFirst = !downloader.IsExistsDatabase;

      try
      {
        await downloader.InitializeAsync();
        this.IsInitialized.Value = true;

        using var db = new MyContext();
        var central = await ConfigUtil.GetIntValueAsync(db, SettingKey.LastDownloadCentralDate);
        var local = await ConfigUtil.GetIntValueAsync(db, SettingKey.LastDownloadLocalDate);
        var jrdb = await ConfigUtil.GetIntValueAsync(db, SettingKey.LastDownloadJrdbDate);
        this.CentralDownloadedYear.Value = central / 100;
        this.CentralDownloadedMonth.Value = central % 100;
        this.LocalDownloadedYear.Value = local / 100;
        this.LocalDownloadedMonth.Value = local % 100;
        this.JrdbDownloadedYear.Value = jrdb / 100;
        this.JrdbDownloadedMonth.Value = jrdb % 100;

        var isNotDownloadBlod = await ConfigUtil.GetIntValueAsync(db, SettingKey.IsNotDownloadHorseBloods);
        var isNotDownloadSlop = await ConfigUtil.GetIntValueAsync(db, SettingKey.IsNotDownloadTrainings);
        var isNotBuildExtra = await ConfigUtil.GetIntValueAsync(db, SettingKey.IsNotBuildExtraData);
        this.IsDownloadBlod.Value = isNotDownloadBlod == 0;
        this.IsDownloadSlop.Value = isNotDownloadSlop == 0;
        this.IsBuildExtraData.Value = isNotBuildExtra == 0;

        logger.Info($"設定 {nameof(SettingKey.LastDownloadCentralDate)}: {central}, {nameof(SettingKey.LastDownloadLocalDate)}: {local}");

        var isCentral = await ConfigUtil.GetIntValueAsync(db, SettingKey.IsDownloadCentral);
        var isLocal = await ConfigUtil.GetIntValueAsync(db, SettingKey.IsDownloadLocal);
        var isJrdb = await ConfigUtil.GetIntValueAsync(db, SettingKey.IsDownloadJrdb);
        this.IsDownloadCentral.Value = isCentral != 0;
        this.IsDownloadLocal.Value = isLocal != 0;
        this.IsDownloadJrdb.Value = isJrdb != 0;
        var isRTCentral = await ConfigUtil.GetIntValueAsync(db, SettingKey.IsRTDownloadCentral);
        var isRTLocal = await ConfigUtil.GetIntValueAsync(db, SettingKey.IsRTDownloadLocal);
        var isRTJrdb = await ConfigUtil.GetIntValueAsync(db, SettingKey.IsRTDownloadJrdb);
        this.IsRTDownloadCentral.Value = isRTCentral != 0;
        this.IsRTDownloadLocal.Value = isRTLocal != 0;
        this.IsRTDownloadJrdb.Value = isRTJrdb != 0;
        logger.Info($"設定 {nameof(SettingKey.IsDownloadCentral)}: {isCentral}, {nameof(SettingKey.IsDownloadLocal)}: {isLocal}, {nameof(SettingKey.IsDownloadJrdb)}: {isJrdb} / {nameof(SettingKey.IsRTDownloadCentral)}: {isRTCentral}, {nameof(SettingKey.IsRTDownloadLocal)}: {isRTLocal}, {nameof(SettingKey.IsRTDownloadJrdb)}: {isRTJrdb}");

        this.IsRTDownloadCentralAfterThursdayOnly.Value = (await ConfigUtil.GetIntValueAsync(db, SettingKey.IsDownloadCentralOnThursdayAfterOnly)) != 0;

        this.JrdbId.Value = await ConfigUtil.GetStringValueAsync(db, SettingKey.JrdbId);
        this.JrdbPassword.Value = await ConfigUtil.GetStringValueAsync(db, SettingKey.JrdbPassword);

        // アプリ起動時デフォルトで設定されるダウンロード開始年月を設定する
        var centralDownloadedYear = (this.IsDownloadCentral.Value && this.CentralDownloadedYear.Value != default) ? this.CentralDownloadedYear.Value : default;
        var centralDownloadedMonth = (this.IsDownloadCentral.Value && this.CentralDownloadedYear.Value != default) ? this.CentralDownloadedMonth.Value : default ;
        var localDownloadedYear = (this.IsDownloadLocal.Value && this.LocalDownloadedYear.Value != default) ? this.LocalDownloadedYear.Value : default;
        var localDownloadedMonth = (this.IsDownloadLocal.Value && this.LocalDownloadedYear.Value != default) ? this.LocalDownloadedMonth.Value : default;
        var jrdbDownloadedYear = (this.IsDownloadJrdb.Value && this.JrdbDownloadedYear.Value != default) ? this.LocalDownloadedYear.Value : default;
        var jrdbDownloadedMonth = (this.IsDownloadJrdb.Value && this.JrdbDownloadedYear.Value != default) ? this.LocalDownloadedMonth.Value : default;
        logger.Debug($"保存されていたデータ: 中央競馬DL年月: {centralDownloadedYear * 100 + centralDownloadedMonth}, 地方競馬DL年月: {localDownloadedYear * 100 + localDownloadedMonth}");
        if (centralDownloadedYear != default && localDownloadedYear != default)
        {
          if (centralDownloadedYear < localDownloadedYear)
          {
            this.StartYear.Value = centralDownloadedYear;
            this.StartMonth.Value = centralDownloadedMonth;
          }
          else if (centralDownloadedYear > localDownloadedYear)
          {
            this.StartYear.Value = localDownloadedYear;
            this.StartMonth.Value = localDownloadedMonth;
          }
          else
          {
            this.StartYear.Value = centralDownloadedYear;
            this.StartMonth.Value = Math.Min(centralDownloadedMonth, localDownloadedMonth);
          }
        }
        else if (centralDownloadedYear != default)
        {
          this.StartYear.Value = centralDownloadedYear;
          this.StartMonth.Value = centralDownloadedMonth;
        }
        else if (localDownloadedYear != default)
        {
          this.StartYear.Value = localDownloadedYear;
          this.StartMonth.Value = localDownloadedMonth;
        }
        else
        {
          this.StartYear.Value = 2000;
          this.StartMonth.Value = 1;
        }
        logger.Debug($"画面に反映するデータ: 中央競馬DL年月: {centralDownloadedYear * 100 + centralDownloadedMonth}, 地方競馬DL年月: {localDownloadedYear * 100 + localDownloadedMonth}");
        logger.Info("初期化完了、最新データのダウンロードを開始");

        // 最新情報をダウンロードする
        this.StartRTDownloadLoop();
      }
      catch (DownloaderCommandException ex)
      {
        logger.Fatal("初期化でダウンローダとの連携時にエラーが発生", ex);
        this.IsInitializationError.Value = true;
        this.ErrorMessage.Value = !string.IsNullOrEmpty(ex.Message) ? ex.Message : ex.Error.GetErrorText();
        isFirst = false;
      }
      catch (Exception ex)
      {
        logger.Fatal("初期化で想定外のエラーが発生", ex);
        this.IsInitializationError.Value = true;
        this.ErrorMessage.Value = "予期しないエラーが発生しました";
        isFirst = false;
      }

      return isFirst;
    }

    private void StartRTDownloadLoop()
    {
      var today = DateOnly.FromDateTime(DateTime.Today);

      async Task DownloadRTAsync(DateOnly date)
      {
        var isSucceed = true;

        if (this.IsRTDownloadCentral.Value)
        {
          var isDownload = true;
          var isDownloadAfterThursday = (await ConfigUtil.GetIntValueAsync(SettingKey.IsDownloadCentralOnThursdayAfterOnly)) != 0;
          if (isDownloadAfterThursday)
          {
            var weekday = today.DayOfWeek;
            isDownload = weekday == DayOfWeek.Thursday || weekday == DayOfWeek.Friday || weekday == DayOfWeek.Saturday || weekday == DayOfWeek.Sunday;
          }
          if (isDownload)
          {
            logger.Info($"中央競馬の最新情報取得を開始 木～月のみ更新: {isDownloadAfterThursday}");
            await this.DownloadRTAsync(DownloadLink.Central, date);
            isSucceed = !this.IsRTError.Value;
          }
        }
        if (this.IsRTDownloadLocal.Value && isSucceed)
        {
          logger.Info("地方競馬の最新情報取得を開始");
          await this.DownloadRTAsync(DownloadLink.Local, date);
        }
      }

      async Task<bool> DownloadPlanOfRacesAsync(int year, int month, int day)
      {
        // 自動更新用のフラグでセットアップデータを落としてるが、これでよい
        // （自動更新したいデータが１週間より前のものだとRTからダウンロードできなくなるので）
        var isSucceed = true;
        if (this.IsRTDownloadCentral.Value)
        {
          logger.Info($"中央競馬の標準データ更新を開始 {year}/{month}/{day}");
          var date = new DateOnly(year, month, day).AddDays(-2);
          await this.DownloadAsync(DownloadLink.Central, date.Year, date.Month, date.Day);
          if (this.IsError.Value)
          {
            logger.Info("ダウンロード失敗を検出");
            isSucceed = false;
          }
        }
        if (this.IsRTDownloadLocal.Value && isSucceed)
        {
          logger.Info($"地方競馬の標準データ更新を開始 {year}/{month}/{day}");
          await this.DownloadAsync(DownloadLink.Local, year, month, day);
          if (this.IsError.Value)
          {
            logger.Info("ダウンロード失敗を検出");
            isSucceed = false;
          }
        }
        if (this.IsRTDownloadJrdb.Value && isSucceed)
        {
          var isDownload = true;
          var isDownloadAfterThursday = (await ConfigUtil.GetIntValueAsync(SettingKey.IsDownloadCentralOnThursdayAfterOnly)) != 0;
          if (isDownloadAfterThursday)
          {
            var weekday = today.DayOfWeek;
            isDownload = weekday == DayOfWeek.Friday || weekday == DayOfWeek.Saturday || weekday == DayOfWeek.Sunday;
          }

          if (isDownload)
          {
            logger.Info("JRDBの最新情報取得を開始");
            var date = new DateOnly(year, month, day);
            await this.DownloadJrdbRTAsync(date, date.AddMonths(1));
          }
        }
        if (isSucceed)
        {
          var newLastLaunchDate = today.Year * 10000 + today.Month * 100 + today.Day;
          var newNormalDataHour = DateTime.Now.Hour;
          await ConfigUtil.SetIntValueAsync(SettingKey.LastLaunchDate, newLastLaunchDate);
          await ConfigUtil.SetIntValueAsync(SettingKey.LastDownloadNormalDataHour, newNormalDataHour);
          logger.Debug($"設定を保存: 最終起動: {newLastLaunchDate}, 標準データ取得時: {newNormalDataHour}");
        }

        return isSucceed;
      }

      async Task DownloadHolidayResultsAsync()
      {
        if (this.IsRTDownloadCentral.Value)
        {
          // 日曜日や月曜日午前は、土日のレース結果などをRTでしか提供していない様子
          if (today.DayOfWeek == DayOfWeek.Sunday)
          {
            logger.Info("土曜日の中央競馬の結果を取得");
            await this.DownloadRTAsync(DownloadLink.Central, today.AddDays(-1));
          }
          else if (today.DayOfWeek == DayOfWeek.Monday)
          {
            logger.Info("土日の中央競馬の結果を取得");
            await this.DownloadRTAsync(DownloadLink.Central, today.AddDays(-2));
            await this.DownloadRTAsync(DownloadLink.Central, today.AddDays(-1));
          }
          else if (!this.IsRTDownloadCentralAfterThursdayOnly.Value)
          {
            logger.Info("前日の中央競馬の結果を取得");
            await this.DownloadRTAsync(DownloadLink.Central, today.AddDays(-1));
          }
        }
      }

      Task.Run(async () =>
      {
        var lastDownloadNormalData = DateTime.MinValue;

        var lastLaunchDay = 0;
        var year = 0;
        var month = 0;
        var day = 0;
        var lastStandardTimeUpdatedYear = 0;

        using var db = new MyContext();

        {
          lastLaunchDay = await ConfigUtil.GetIntValueAsync(db, SettingKey.LastLaunchDate);
          year = lastLaunchDay / 10000;
          month = lastLaunchDay / 100 % 100;
          day = lastLaunchDay % 100;

          lastStandardTimeUpdatedYear = await ConfigUtil.GetIntValueAsync(db, SettingKey.LastUpdateStandardTimeYear);
        }
        logger.Info($"最後に標準タイムを更新した年: {lastStandardTimeUpdatedYear}");

        // データベースのマイグレーション処理を自動的に開始
        var dbver = await ConfigUtil.GetIntValueAsync(db, SettingKey.DatabaseVersion);
        if (dbver < 250)
        {
          await this.ProcessAsync(DownloadLink.Both, this.ProcessingStep, false, Connection.ProcessingStep.MigrationFrom250);
          await ConfigUtil.SetIntValueAsync(db, SettingKey.DatabaseVersion, 250);
          dbver = 250;
        }
        if (dbver < 322)
        {
          await this.ProcessAsync(DownloadLink.Both, this.ProcessingStep, false, Connection.ProcessingStep.MigrationFrom322);
          await ConfigUtil.SetIntValueAsync(db, SettingKey.DatabaseVersion, 322);
          dbver = 322;
        }
        if (dbver < 430)
        {
          await this.ProcessAsync(DownloadLink.Both, this.ProcessingStep, false, Connection.ProcessingStep.MigrationFrom430);
          await ConfigUtil.SetIntValueAsync(db, SettingKey.DatabaseVersion, 430);
          dbver = 430;
        }

        // 最初に最終起動からの差分を落とす
        // （真っ先にやらないと、ユーザーが先に過去データダウンロードを開始する可能性あり）
        // RTと同時に開始すると、JVLinkAgentが落ちる可能性が高まる？
        {
          this._isInitializationDownloading = true;

          if (lastLaunchDay != default)
          {
            var lastLaunch = new DateOnly(year, month, day);
            logger.Info($"最終起動日: {lastLaunch:yyyy/MM/dd}");

            var minDate = today.AddMonths(-3);
            if (lastLaunch < minDate)
            {
              lastLaunch = minDate;
              logger.Info($"最終更新日が３か月より前だったので調整します {lastLaunch:yyyy/MM/dd}");
            }

            // 毎日初回起動時に必ずダウンロードする
            if (lastLaunch != today)
            {
              var isSucceed = await DownloadPlanOfRacesAsync(year, month, day);
              if (isSucceed)
              {
                lastDownloadNormalData = DateTime.Now;
              }
              else
              {
                lastDownloadNormalData = lastLaunch.ToDateTime(default);
              }
            }
            else
            {
              var hour = await ConfigUtil.GetIntValueAsync(SettingKey.LastDownloadNormalDataHour);
              lastDownloadNormalData = DateTime.Today.AddHours(hour);
            }
          }

          // 年を跨ぐ場合は基準タイムの更新も行う
          if (lastStandardTimeUpdatedYear != today.Year)
          {
            logger.Info($"標準タイムを再計算します");
            await this.ProcessAsync(DownloadLink.Both, this.ProcessingStep, false, Connection.ProcessingStep.StandardTime);
          }

          logger.Info("初期ダウンロード完了");
          this._isInitializationDownloading = false;
        }

        async Task DownloadOddsOfCentralHolidaysAsync()
        {
          if (this.IsRTDownloadCentral.Value)
          {
            // 地方と違って中央競馬は、必ずレース翌日にレース結果を蓄積系データとして提供するとは限らないらしい
            // 翌日以降のレースデータも取得しておきたい
            if (today.DayOfWeek == DayOfWeek.Saturday)
            {
              logger.Debug("土曜日につき翌日のデータをダウンロード");
              await this.DownloadRTAsync(DownloadLink.Central, today.AddDays(1));
            }
            else if (today.DayOfWeek == DayOfWeek.Friday)
            {
              // 重賞レースの前売りがある場合、オッズが更新されることがある
              // （※前売りがあるのはG1の中でも一部で、年に数回程度）
              logger.Debug("金曜日につき翌日以降のデータをダウンロード");
              await this.DownloadRTAsync(DownloadLink.Central, today.AddDays(1));
              await this.DownloadRTAsync(DownloadLink.Central, today.AddDays(2));
            }
            else if (!this.IsRTDownloadCentralAfterThursdayOnly.Value)
            {
              logger.Debug("とりあえず翌日のデータをダウンロード");
              await this.DownloadRTAsync(DownloadLink.Central, today.AddDays(1));
            }
          }
        }

        var isCentralChecked = false;
        if (this.IsRTDownloadCentral.Value)
        {
          if (lastLaunchDay == default || new DateOnly(year, month, day) < today)
          {
            await DownloadOddsOfCentralHolidaysAsync();
            await DownloadHolidayResultsAsync();
          }
          isCentralChecked = true;
        }

        while (true)
        {
          var now = DateTime.Now;

          try
          {
            // アプリ起動中に日付を跨いだ場合
            var currentDay = DateOnly.FromDateTime(DateTime.Today);
            if (today != currentDay)
            {
              logger.Info("アプリ起動中に日付変更");
              today = currentDay;
              await DownloadHolidayResultsAsync();
            }

            // メインのダウンロード処理
            if (this.CanSaveOthers.Value && !JrdbDownloaderModel.Instance.IsDownloading.Value)
            {
              logger.Info("最新情報取得を開始");
              await DownloadRTAsync(today);
              await DownloadOddsOfCentralHolidaysAsync();
            }

            // レース予定データはRTではなくこっちにあるみたい。定期的にチェックする
            if ((lastDownloadNormalData.AddMinutes(240) < now || this._isUpdateRtHeavyForce)
                && !this.IsDownloading.Value)
            {
              logger.Info("翌日以降の予定を更新");
              var isSucceed = await DownloadPlanOfRacesAsync(lastDownloadNormalData.Year, lastDownloadNormalData.Month, lastDownloadNormalData.Day);
              if (isSucceed)
              {
                lastDownloadNormalData = DateTime.Now;
              }
            }

            // アプリ起動した後に中央競馬DLを有効にした場合
            // 地方競馬の場合、前日の結果がRTにしかないということはなさそう？
            if (!isCentralChecked && this.IsRTDownloadCentral.Value && !this.IsRTDownloading.Value)
            {
              logger.Info("中央競馬DLが有効になったので前日の結果を取得");
              await DownloadHolidayResultsAsync();
              isCentralChecked = true;
            }
          }
          catch (Exception ex)
          {
            logger.Error("最新情報ダウンロード中にエラーが発生しました", ex);
          }
          finally
          {
            this._isUpdateRtForce = false;
            this._isUpdateRtHeavyForce = false;
          }

          this.IsWaitingNextRTUpdate.Value = true;
          while ((DateTime.Now - now).TotalMinutes < 5 && !this._isUpdateRtForce && !this._isUpdateRtHeavyForce)
          {
            // ５分に１回ずつ更新する
            this.NextRTUpdateSeconds.Value = 60 * 5 - (int)(DateTime.Now - now).TotalSeconds;
            await Task.Delay(1000);
          }
          this.NextRTUpdateSeconds.Value = 0;
          if (this.IsRTPaused.Value)
          {
            logger.Debug("一時停止の設定を検出");
          }
          while (this.IsRTPaused.Value && !this._isUpdateRtForce && !this._isUpdateRtHeavyForce)
          {
            // 一時停止
            await Task.Delay(1000);
          }
          this.IsWaitingNextRTUpdate.Value = false;
        }
      });
    }

    public void BeginResetHorseExtraData()
    {
      Task.Run(async () =>
      {
        logger.Info("拡張データのリセットを開始");
        await this.ProcessAsync(DownloadLink.Both, this.ProcessingStep, false, Connection.ProcessingStep.ResetHorseExtraData);
      });
    }

    public void BeginDownload()
    {
      Task.Run(async () =>
      {
        if (this.IsBuildMasterData.Value)
        {
          logger.Info("マスターデータ更新を開始");
          await this.ProcessAsync(DownloadLink.Both, this.ProcessingStep, false, this.AllSteps);
        }
        else
        {
          if (!this.IsDownloadJrdb.Value)
          {
            var link = (DownloadLink)0;
            if (this.IsDownloadCentral.Value) link |= DownloadLink.Central;
            if (this.IsDownloadLocal.Value) link |= DownloadLink.Local;
            //if (this.IsDownloadJrdb.Value) link |= DownloadLink.Jrdb;

            logger.Info($"セットアップデータダウンロードを開始 リンク: {link}");
            await this.DownloadAsync(link);
          }
          else
          {
            logger.Info($"JRDBセットアップデータダウンロードを開始");
            await this.DownloadJrdbAsync(new DateTime(this.StartYear.Value, this.StartMonth.Value, 1), null);
          }
        }
      });
    }

    private async Task ProcessAsync(DownloadLink link, ReactiveProperty<ProcessingStep> step, bool isRt, ProcessingStep steps, bool isFlagSetManually = false)
    {
      this.IsCancelProcessing.Value = false;
      this.IsError.Value = false;

      try
      {
        if (isRt)
        {
          this.IsRTDownloading.Value = true;
          this.RTDownloadingLink.Value = link;
          this.IsRTProcessing.Value = true;
        }
        else
        {
          this.IsDownloading.Value = true;
          this.DownloadingLink.Value = link;
          this.IsProcessing.Value = true;
        }

        if (steps.HasFlag(Connection.ProcessingStep.InvalidData))
        {
          step.Value = Connection.ProcessingStep.InvalidData;
          logger.Info($"後処理進捗変更: {step.Value}, リンク: {link}");
          await ShapeDatabaseModel.RemoveInvalidDataAsync();
        }
        if (steps.HasFlag(Connection.ProcessingStep.MigrationFrom250) && !this.IsCancelProcessing.Value)
        {
          step.Value = Connection.ProcessingStep.MigrationFrom250;
          logger.Info($"後処理進捗変更: {step.Value}, リンク: {link}, isRT: {isRt}");
          await ShapeDatabaseModel.MigrateFrom250Async(isCanceled: this.IsCancelProcessing);
        }
        if (steps.HasFlag(Connection.ProcessingStep.MigrationFrom322) && !this.IsCancelProcessing.Value)
        {
          step.Value = Connection.ProcessingStep.MigrationFrom322;
          logger.Info($"後処理進捗変更: {step.Value}, リンク: {link}, isRT: {isRt}");
          await ShapeDatabaseModel.MigrateFrom322Async(isCanceled: this.IsCancelProcessing);
        }
        if (steps.HasFlag(Connection.ProcessingStep.MigrationFrom430) && !this.IsCancelProcessing.Value)
        {
          step.Value = Connection.ProcessingStep.MigrationFrom430;
          logger.Info($"後処理進捗変更: {step.Value}, リンク: {link}, isRT: {isRt}");
          await ShapeDatabaseModel.MigrateFrom430Async(isCanceled: this.IsCancelProcessing);
        }
        if (steps.HasFlag(Connection.ProcessingStep.RunningStyle) && !this.IsCancelProcessing.Value)
        {
          step.Value = Connection.ProcessingStep.RunningStyle;
          logger.Info($"後処理進捗変更: {step.Value}, リンク: {link}, isRT: {isRt}");
          if (link.HasFlag(DownloadLink.Central))
          {
            ShapeDatabaseModel.TrainRunningStyle(isForce: !isRt);
          }
          ShapeDatabaseModel.StartRunningStylePredicting();
        }
        if (steps.HasFlag(Connection.ProcessingStep.PreviousRaceDays) && !this.IsCancelProcessing.Value)
        {
          step.Value = Connection.ProcessingStep.PreviousRaceDays;
          logger.Info($"後処理進捗変更: {step.Value}, リンク: {link}, isRT: {isRt}");
          try
          {
            this.HasProcessingProgress.Value = true;
            await ShapeDatabaseModel.SetHorseExtraDataAsync(isCanceled: this.IsCancelProcessing,
              progress: this.ProcessingProgress, progressMax: this.ProcessingProgressMax);
          }
          // catch は不要
          finally
          {
            this.HasProcessingProgress.Value = false;
          }
        }
        if (steps.HasFlag(Connection.ProcessingStep.RiderWinRates) && !this.IsCancelProcessing.Value)
        {
          step.Value = Connection.ProcessingStep.RiderWinRates;
          logger.Info($"後処理進捗変更: {step.Value}, リンク: {link}, isRT: {isRt}");
          try
          {
            this.HasProcessingProgress.Value = true;
            await ShapeDatabaseModel.SetRiderWinRatesAsync(isCanceled: this.IsCancelProcessing,
              progress: this.ProcessingProgress, progressMax: this.ProcessingProgressMax);
          }
          // catch は不要
          finally
          {
            this.HasProcessingProgress.Value = false;
          }
        }
        if (steps.HasFlag(Connection.ProcessingStep.RaceSubjectInfos) && !this.IsCancelProcessing.Value)
        {
          step.Value = Connection.ProcessingStep.RaceSubjectInfos;
          logger.Info($"後処理進捗変更: {step.Value}, リンク: {link}, isRT: {isRt}");
          await ShapeDatabaseModel.SetRaceSubjectDisplayInfosAsync(isCanceled: this.IsCancelProcessing);
        }
        if (steps.HasFlag(Connection.ProcessingStep.ResetHorseExtraData) && !this.IsCancelProcessing.Value)
        {
          step.Value = Connection.ProcessingStep.ResetHorseExtraData;
          logger.Info($"後処理進捗変更: {step.Value}, リンク: {link}, isRT: {isRt}");
          await ShapeDatabaseModel.ResetHorseExtraTableDataAsync();
        }
        if (steps.HasFlag(Connection.ProcessingStep.HorseExtraData) && !this.IsCancelProcessing.Value)
        {
          step.Value = Connection.ProcessingStep.HorseExtraData;
          logger.Info($"後処理進捗変更: {step.Value}, リンク: {link}, isRT: {isRt}");
          try
          {
            this.HasProcessingProgress.Value = true;
            await ShapeDatabaseModel.SetHorseExtraTableDataAsync(isCanceled: this.IsCancelProcessing,
              progress: this.ProcessingProgress, progressMax: this.ProcessingProgressMax);
          }
          finally
          {
            this.HasProcessingProgress.Value = false;
          }
        }

        // 途中から再開できないものは最後に
        if (steps.HasFlag(Connection.ProcessingStep.StandardTime) && !this.IsCancelProcessing.Value)
        {
          step.Value = Connection.ProcessingStep.StandardTime;
          logger.Info($"後処理進捗変更: {step.Value}, リンク: {link}, isRT: {isRt}");
          try
          {
            this.HasProcessingProgress.Value = true;
            await ShapeDatabaseModel.MakeStandardTimeMasterDataAsync(1990, isCanceled: this.IsCancelProcessing,
              progressMax: this.ProcessingProgressMax, progress: this.ProcessingProgress);
            await ConfigUtil.SetIntValueAsync(SettingKey.LastUpdateStandardTimeYear, DateTime.Today.Year);
          }
          finally
          {
            this.HasProcessingProgress.Value = false;
          }
        }

        this.RacesUpdated?.Invoke(this, EventArgs.Empty);
      }
      finally
      {
        if (!isFlagSetManually)
        {
          if (isRt)
          {
            this.IsRTDownloading.Value = false;
            this.IsRTProcessing.Value = false;
          }
          else
          {
            this.IsDownloading.Value = false;
            this.IsProcessing.Value = false;
          }
        }
        step.Value = Connection.ProcessingStep.Unknown;
        this.IsCancelProcessing.Value = false;
        logger.Info($"後処理完了 リンク: {link}, isRT: {isRt}");
      }
    }

    private async Task DownloadAsync(DownloadLink link, int year = 0, int month = 0, int startDay = 0)
    {
      logger.Info($"通常データのダウンロードを開始します リンク: {link}");
      logger.Debug($"現在のダウンロード状態: {this.IsDownloading.Value}");

      if (link == DownloadLink.Both)
      {
        await this.DownloadAsync(DownloadLink.Central, year, month, startDay);
        await this.DownloadAsync(DownloadLink.Local, year, month, startDay);
        return;
      }

      this.IsError.Value = false;
      this.IsDownloading.Value = true;

      var downloader = this._downloader;
      var linkName = link == DownloadLink.Central ? "central" : "local";

      var startYear = year;
      if (year == 0) startYear = this.StartYear.Value;
      var startMonth = month;
      if (month == 0) startMonth = this.StartMonth.Value;
      logger.Info($"開始年月: {startYear}/{startMonth}");

      try
      {
        this.DownloadingLink.Value = link;
        await downloader.DownloadAsync(linkName, "race", startYear, startMonth, this.OnDownloadProgress, startDay);
        this.RacesUpdated?.Invoke(this, EventArgs.Empty);

        logger.Info("通常データのダウンロード完了。後処理に移行します");
        await this.ProcessAsync(link, this.ProcessingStep, false,
          this.AllSteps & ~Connection.ProcessingStep.StandardTime, isFlagSetManually: true);
      }
      catch (DownloaderCommandException ex)
      {
        logger.Error("通常データのダウンロードに失敗しました。ダウンローダがエラーを返しました", ex);
        this.ErrorMessage.Value = ex.Error.GetErrorText();
        this.IsError.Value = true;
      }
      catch (Exception ex)
      {
        logger.Error("通常データのダウンロードに失敗しました", ex);
        this.ErrorMessage.Value = ex.Message;
        this.IsError.Value = true;
      }
      finally
      {
        this.IsDownloading.Value = false;
        this.IsProcessing.Value = false;
        this.IsCancelProcessing.Value = false;
        this.ProcessingStep.Value = Connection.ProcessingStep.Unknown;
        this.LoadingProcess.Value = LoadingProcessValue.Unknown;

        this.StartMonth.Value = startMonth;
        this.StartYear.Value = startYear;
      }

      logger.Info("ダウンロード処理を終了します");
    }

    private async Task DownloadRTAsync(DownloadLink link, DateOnly date)
    {
      logger.Info($"RTデータのダウンロードを開始します リンク: {link}");
      logger.Debug($"現在のダウンロード状態: {this.IsRTDownloading.Value}");

      if (link == DownloadLink.Both)
      {
        await this.DownloadRTAsync(DownloadLink.Central, date);
        await this.DownloadRTAsync(DownloadLink.Local, date);
        return;
      }

      this.IsRTError.Value = false;
      this.IsRTDownloading.Value = true;
      this.RTDownloadingLink.Value = link;

      var downloader = this._downloader;
      var linkName = link == DownloadLink.Central ? "central" : "local";
      logger.Info($"開始年月: {date:yyyy/MM/dd}");

      try
      {
        await downloader.DownloadRTAsync(linkName, date, this.OnRTDownloadProgress);
        this.RacesUpdated?.Invoke(this, EventArgs.Empty);

        logger.Info("RTデータのダウンロード完了。後処理に移行します");
        await this.ProcessAsync(link, this.RTProcessingStep, true, Connection.ProcessingStep.InvalidData | Connection.ProcessingStep.RunningStyle, isFlagSetManually: true);

        logger.Debug("RT後処理完了");
        this.RacesUpdated?.Invoke(this, EventArgs.Empty);
      }
      catch (DownloaderCommandException ex)
      {
        logger.Error("RTデータのダウンロードに失敗しました。ダウンローダがエラーを返しました", ex);
        this.RTErrorMessage.Value = ex.Error.GetErrorText();
        this.IsRTError.Value = true;
      }
      catch (Exception ex)
      {
        logger.Error("通常データのダウンロードに失敗しました", ex);
        this.RTErrorMessage.Value = ex.Message;
        this.IsRTError.Value = true;
      }
      finally
      {
        this.IsRTDownloading.Value = false;
        this.IsRTProcessing.Value = false;
        this.IsCancelProcessing.Value = false;
        this.RTProcessingStep.Value = Connection.ProcessingStep.Unknown;
        this.RTLoadingProcess.Value = LoadingProcessValue.Unknown;
      }

      logger.Info("ダウンロード処理を終了します");
    }

    private async Task DownloadJrdbAsync(DateTime from, DateTime? to)
    {
      var tod = to ?? DateTime.MaxValue;

      logger.Info($"JRDBデータのダウンロードを開始します {from:yyyyMMdd} - {to:yyyyMMdd}");

      try
      {
        this.IsError.Value = false;
        this.IsDownloading.Value = true;
        this.DownloadingLink.Value = DownloadLink.Jrdb;
        this.DownloadingType.Value = Connection.DownloadingType.Jrdb;
        await JrdbDownloaderModel.Instance.LoadAsync(from, tod, this.JrdbId.Value, this.JrdbPassword.Value);
      }
      catch (JrdbDownloadException ex)
      {
        logger.Error("JRDBデータのダウンロードに失敗しました", ex);
        this.ErrorMessage.Value = ex.Message;
        this.IsError.Value = true;
      }
      catch (Exception ex)
      {
        logger.Error("JRDBデータのダウンロードに失敗しました（ハンドルされていない例外）", ex);
        this.ErrorMessage.Value = "不明なエラーが発生しました";
        this.IsError.Value = true;
      }
      finally
      {
        this.IsDownloading.Value = false;
      }

      logger.Info("ダウンロード処理を終了します");
    }

    private async Task DownloadJrdbRTAsync(DateOnly date, DateOnly to)
    {
      await this.DownloadJrdbAsync(date.ToDateTime(default), to.ToDateTime(default));
    }

    private async Task OnDownloadProgress(DownloaderTaskData task)
    {
      var p = task.Parameter.Split(',');
      int.TryParse(p[0], out var year);
      int.TryParse(p[1], out var month);
      var mode = p[3];

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
        if (task.Parameter.Contains("central") && (this.CentralDownloadedMonth.Value != month || this.CentralDownloadedYear.Value != year))
        {
          this.CentralDownloadedMonth.Value = month;
          this.CentralDownloadedYear.Value = year;
          using var db = new MyContext();
          await ConfigUtil.SetIntValueAsync(db, SettingKey.LastDownloadCentralDate, year * 100 + month);
          logger.Info($"{year}/{month:00} の中央競馬通常データダウンロード完了");

          this.IsLongDownloadMonth.Value = year == 2002 && month == 12;
        }
        if (task.Parameter.Contains("local") && (this.LocalDownloadedMonth.Value != month || this.LocalDownloadedYear.Value != year))
        {
          this.LocalDownloadedMonth.Value = month;
          this.LocalDownloadedYear.Value = year;
          using var db = new MyContext();
          await ConfigUtil.SetIntValueAsync(db, SettingKey.LastDownloadLocalDate, year * 100 + month);
          logger.Info($"{year}/{month:00} の地方競馬通常データダウンロード完了");
        }
      }
    }

    private Task OnRTDownloadProgress(DownloaderTaskData task)
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

    public async Task CancelDownloadAsync()
    {
      if (JrdbDownloaderModel.Instance.IsDownloading.Value)
      {
        JrdbDownloaderModel.Instance.IsCanceled.Value = true;
      }
      else
      {
        await this._downloader.CancelCurrentTaskAsync();
        this.IsCancelProcessing.Value = true;
      }
      logger.Warn("ダウンロードが中止されました");
    }

    public async Task OpenJvlinkConfigAsync()
    {
      this.IsError.Value = false;
      logger.Info("JVLink設定を開きます");

      try
      {
        await this._downloader.OpenJvlinkConfigAsync();
      }
      catch (Exception ex)
      {
        logger.Error("JVLink設定を開こうとしましたがエラーが発生しました", ex);
        this.IsError.Value = true;
        this.ErrorMessage.Value = ex.Message;
      }
    }

    public async Task OpenNvlinkConfigAsync()
    {
      this.IsError.Value = false;
      logger.Info("NVLink設定を開きます");

      try
      {
        await this._downloader.OpenNvlinkConfigAsync();
      }
      catch (Exception ex)
      {
        logger.Error("NVLink設定を開こうとしましたがエラーが発生しました", ex);
        this.IsError.Value = true;
        this.ErrorMessage.Value = ex.Message;
      }
    }

    public void UpdateRtDataForce()
    {
      this._isUpdateRtForce = true;
    }

    public void UpdateRtDataHeavyForce()
    {
      this._isUpdateRtHeavyForce = true;
    }

    public void Dispose()
    {
      this._disposables.Dispose();
      this._downloader.Dispose();
      logger.Debug("ダウンローダのオブジェクト破棄");
    }

    public event EventHandler? RacesUpdated;
  }

  [Flags]
  enum DownloadLink
  {
    [Label("中央")]
    Central = 0b01,

    [Label("地方")]
    Local = 0b10,

    [Label("JRDB")]
    Jrdb = 0b100,

    Both = 0b11,
    All = 0b111,
  }

  enum DownloadingType
  {
    [Label("レース")]
    Race,

    [Label("オッズ")]
    Odds,

    [Label("JRDB")]
    Jrdb,
  }

  enum LoadingProcessValue
  {
    Unknown,

    [Label("接続オープン中")]
    Opening,

    [Label("ダウンロード中")]
    Downloading,

    [Label("データ読み込み中")]
    Loading,

    [Label("データ保存中")]
    Writing,

    [Label("後処理中")]
    Processing,

    [Label("接続クローズ中")]
    Closing,

    [Label("JRA-VANからのお知らせが表示されています")]
    CheckingJraVanNews,
  }

  enum DownloadMode
  {
    Continuous,
    WithStartDate,
  }

  enum DownloadingDataspec
  {
    Unknown,

    [Label("レース結果")]
    RB12,

    [Label("レース情報")]
    RB15,

    [Label("タイム型MING")]
    RB13,

    [Label("対戦型MING")]
    RB17,

    [Label("オッズ")]
    RB30,

    [Label("馬体重")]
    RB11,

    [Label("変更情報")]
    RB14,

    [Label("時系列オッズ")]
    RB41,
  }

  [Flags]
  enum ProcessingStep
  {
    Unknown = 0,

    [Label("不正なデータを処理中")]
    InvalidData = 1,

    [Label("脚質を計算中")]
    RunningStyle = 2,

    [Label("基準タイムを計算中")]
    StandardTime = 4,

    [Label("馬データの成型中")]
    PreviousRaceDays = 8,

    [Label("騎手の勝率を計算中")]
    RiderWinRates = 16,

    [Label("地方競馬のレース条件を解析中")]
    RaceSubjectInfos = 32,

    [Label("データをマイグレーション中 (from 2.5.0)")]
    MigrationFrom250 = 64,

    [Label("データをマイグレーション中 (from 3.2.2)")]
    MigrationFrom322 = 128,

    [Label("拡張情報を作成中")]
    HorseExtraData = 256,

    [Label("拡張情報をリセット中")]
    ResetHorseExtraData = 512,

    [Label("データをマイグレーション中 (from 4.3.0)")]
    MigrationFrom430 = 1024,

    All = InvalidData | RunningStyle | StandardTime | PreviousRaceDays | RiderWinRates | RaceSubjectInfos | HorseExtraData,
  }
}
