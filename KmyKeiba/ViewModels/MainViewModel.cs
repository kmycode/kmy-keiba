using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Connection;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race;
using KmyKeiba.Models.Race.AnalysisTable;
using KmyKeiba.Models.Race.AnalysisTable.Script;
using KmyKeiba.Models.Race.ExNumber;
using KmyKeiba.Models.Race.Finder;
using KmyKeiba.Models.RList;
using KmyKeiba.Models.Script;
using KmyKeiba.Models.Setting;
using KmyKeiba.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KmyKeiba.ViewModels
{
  internal class MainViewModel : RaceViewModelBase
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public ScriptBulkModel ScriptBulk { get; } = new();

    public ReactiveProperty<AppSettingsModel?> AppSettings { get; } = new();

    public bool IsMainWindow => true;

    public RaceList RaceList => this.model.RaceList;

    public ReactiveProperty<bool> IsInitializationError => this.downloader.IsInitializationError;

    public ReactiveProperty<string> DownloaderErrorMessage => this.downloader.State.ErrorMessage;

    public ReactiveProperty<bool> IsInitialized { get; } = new ReactiveProperty<bool>();

    public ReactiveProperty<bool> IsLongDownloadMonth => this.downloader.State.IsLongDownloadMonth;

    public ReactiveProperty<string> FirstMessage => this.model.FirstMessage;

    public UpdateChecker Update { get; } = new();

    public OpenDialogRequest Dialog { get; } = new();

    public ReactiveProperty<DialogType> CurrentDialog { get; } = new();

    public ReactiveProperty<bool> IsDialogOpen { get; }

    public OpenRaceRequest RaceWindow => OpenRaceRequest.Default;

    public OpenErrorSavingMemoRequest ErrorSavingMemo => OpenErrorSavingMemoRequest.Default;

    public ReactiveProperty<string> ErrorSavingMemoText { get; } = new();

    public OpenErrorConfiguringRequest ErrorConfiguring => OpenErrorConfiguringRequest.Default;

    public ReactiveProperty<string> ErrorConfigringMessage { get; } = new();

    public MainViewModel()
    {
      logger.Debug("アプリ関係オブジェクトの生成開始");

      //ReactivePropertyScheduler.SetDefault(DispatcherScheduler.Current);
      this.IsDialogOpen = this.CurrentDialog.Select(d => d != DialogType.Unknown).ToReactiveProperty().AddTo(this._disposables);

      // モデル同士のイベントをつなぐ
      this.downloader.IsInitialized.Where(i => i).Subscribe(i => this.model.OnDatabaseInitialized()).AddTo(this._disposables);
      Observable.FromEvent<EventHandler, EventArgs>(
        e => (s, a) => e(a),
        dele => this.downloader.RacesUpdated += dele,
        dele => this.downloader.RacesUpdated -= dele)
        .Subscribe(_ => ThreadUtil.InvokeOnUiThread(async () =>
        {
          await this.RaceList.UpdateListAsync();
          OpenRaceRequest.Default.Update();
        })).AddTo(this._disposables);
      Observable.FromEvent<EventHandler, EventArgs>(
        e => (s, a) => e(a),
        dele => this.RaceList.SelectedRaceUpdated += dele,
        dele => this.RaceList.SelectedRaceUpdated -= dele)
        .Subscribe(_ => ThreadUtil.InvokeOnUiThread(() =>
        {
          this.model.OnSelectedRaceUpdated();
        })).AddTo(this._disposables);

      // 構成スクリプト
      // 構成スクリプトがasync functionを返した場合、非同期実行になってしまい一部の設定が遅延反映になるかまたは無効
      // （サンプルではfunctionを返してるので大丈夫だと思うが）
      // 参考: C#ではasyncメソッドはawaitされるまでの処理はすべて同期的に実行される。
      //       このRunAsyncの中でawaitするのはスクリプトがasync functionを返した場合に限られるので、
      //       それさえなければ動機メソッドと同等の振る舞いをすると期待
      _ = ConfigureScript.RunAsync();

      ScriptManager.Initialize();
      Task.Run(async () =>
      {
        var isFirst = await this.downloader.InitializeAsync();
        if (isFirst)
        {
          // 初期設定画面
          logger.Debug("初めての利用のようです。初期画面を表示します");
          this.CurrentDialog.Value = DialogType.Download;
        }

        // 各種初期化処理
        using var db = new MyContext();
        await FinderConfigUtil.InitializeAsync(db);
        await AnalysisTableScriptUtil.InitializeAsync(db);
        await FinderConfigUtil.InitializeAsync(db);
        await ExternalNumberUtil.InitializeAsync(db);
        await CheckHorseUtil.InitializeAsync(db);
        await AnalysisTableUtil.InitializeAsync(db);

        // DBのプリセット
        await DatabasePresetModel.SetPresetsAsync();

        // 初期化完了
        this.IsInitialized.Value = true;

        // 初期化が終わったので設定可能に
        this.AppSettings.Value = AppSettingsModel.Instance;

        // アップデートチェック
        await this.Update.CheckAsync();
      });

      logger.Debug("ビューモデルの初期化完了");
    }

    public void OnApplicationExit()
    {
      logger.Debug("アプリ終了処理開始");
      DownloaderConnector.Instance.Dispose();
      logger.Debug("アプリ終了処理完了");
    }

    #region MainCommands

    public ICommand OpenDownloadDialogCommand =>
      this._openDownloadDialogCommand ??=
        new ReactiveCommand().WithSubscribe(() => this.CurrentDialog.Value = DialogType.Download);
    private ReactiveCommand? _openDownloadDialogCommand;

    public ICommand OpenRTDownloadDialogCommand =>
      this._openRTDownloadDialogCommand ??=
        new ReactiveCommand().WithSubscribe(() => this.CurrentDialog.Value = DialogType.RTDownload);
    private ReactiveCommand? _openRTDownloadDialogCommand;

    public ICommand OpenScriptBulkDialogCommand =>
      this._openScriptBulkDialogCommand ??=
        new ReactiveCommand().WithSubscribe(() => this.CurrentDialog.Value = DialogType.ScriptBulk);
    private ReactiveCommand? _openScriptBulkDialogCommand;

    public ICommand OpenSettingDialogCommand =>
      this._openSettingDialogCommand ??=
        new ReactiveCommand().WithSubscribe(() => this.CurrentDialog.Value = DialogType.Setting);
    private ReactiveCommand? _openSettingDialogCommand;

    public ICommand OpenVersionDialogCommand =>
      this._openVersionDialogCommand ??=
        new ReactiveCommand().WithSubscribe(() => this.CurrentDialog.Value = DialogType.Version);
    private ReactiveCommand? _openVersionDialogCommand;

    public ICommand CloseDialogCommand =>
      this._closeDialogCommand ??=
        new ReactiveCommand().WithSubscribe(() => this.CurrentDialog.Value = DialogType.Unknown);
    private ReactiveCommand? _closeDialogCommand;

    public ICommand OpenCentralRaceLiveCommand =>
      this._openCentralRaceLiveCommand ??=
        new ReactiveCommand().WithSubscribe(() =>
        {
          System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
          {
            UseShellExecute = true,
            FileName = "https://sp.gch.jp/",
          });
        });
    private ReactiveCommand? _openCentralRaceLiveCommand;

    public ICommand OpenLocalRaceLiveCommand =>
      this._openLocalRaceLiveCommand ??=
        new ReactiveCommand().WithSubscribe(() =>
        {
          System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
          {
            UseShellExecute = true,
            FileName = "https://keiba-lv-st.jp/",
          });
        });
    private ReactiveCommand? _openLocalRaceLiveCommand;

    public ICommand BuyCommand =>
      this._buyCommand ??=
        new AsyncReactiveCommand<object>().WithSubscribe(_ => this.Race.Value?.BuyAsync() ?? Task.CompletedTask);
    private AsyncReactiveCommand<object>? _buyCommand;

    public ICommand ExecuteScriptBulkCommand =>
      this._executeScriptBulkCommand ??=
        new ReactiveCommand().WithSubscribe(() => this.ScriptBulk.BeginExecute());
    private ReactiveCommand? _executeScriptBulkCommand;

    public ICommand CancelScriptBulkCommand =>
      this._cancelScriptBulkCommand ??=
        new ReactiveCommand<object>().WithSubscribe(_ => this.ScriptBulk.Cancel());
    private ReactiveCommand<object>? _cancelScriptBulkCommand;

    #endregion

    #region RaceList

    public ICommand MoveToNextDayCommand =>
      this._moveToNextDayCommand ??=
        new ReactiveCommand().WithSubscribe(this.model.RaceList.MoveToNextDay);
    private ReactiveCommand? _moveToNextDayCommand;

    public ICommand MoveToPrevDayCommand =>
      this._moveToPrevDayCommand ??=
        new ReactiveCommand().WithSubscribe(this.model.RaceList.MoveToPrevDay);
    private ReactiveCommand? _moveToPrevDayCommand;

    #endregion

    #region Link連携・ダウンロード

    public ICommand OpenJvlinkConfigCommand =>
      this._openJvlinkConfigCommand ??=
        new AsyncReactiveCommand<object>().WithSubscribe(async _ => await this.downloader.OpenJvlinkConfigAsync());
    private AsyncReactiveCommand<object>? _openJvlinkConfigCommand;

    public ICommand OpenNvlinkConfigCommand =>
      this._openNvlinkConfigCommand ??=
        new AsyncReactiveCommand<object>().WithSubscribe(async _ => await this.downloader.OpenNvlinkConfigAsync());
    private AsyncReactiveCommand<object>? _openNvlinkConfigCommand;

    public ICommand StartDownloadCommand =>
      this._startDownloadCommand ??=
        new ReactiveCommand(this.downloader.State.IsRTBusy.Select(r => !r)).WithSubscribe(this.downloader.BeginDownload);
    private ReactiveCommand? _startDownloadCommand;

    public ICommand ResetHorseExtraDataCommand =>
      this._resetHorseExtraDataCommand ??=
        new ReactiveCommand(this.downloader.State.IsRTBusy.Select(r => !r)).WithSubscribe(this.downloader.BeginResetHorseExtraData);
    private ICommand? _resetHorseExtraDataCommand;

    public ICommand CancelDownloadCommand =>
      this._cancelDownloadCommand ??=
        new ReactiveCommand<object>(this.downloader.State.CanCancel).WithSubscribe(_ => this.downloader.CancelDownload());
    private ReactiveCommand<object>? _cancelDownloadCommand;

    public ICommand UpdateRtDataForceCommand =>
      this._updateRtDataForceCommand ??=
        new ReactiveCommand(this.downloader.State.IsBusy.Select(r => !r)).WithSubscribe(this.downloader.UpdateRtDataForce);
    private ReactiveCommand? _updateRtDataForceCommand;

    public ICommand UpdateRtDataHeavyForceCommand =>
      this._updateRtDataHeavyForceCommand ??=
        new ReactiveCommand(this.downloader.State.IsBusy.Select(r => !r)).WithSubscribe(this.downloader.UpdateRtDataHeavyForce);
    private ReactiveCommand? _updateRtDataHeavyForceCommand;

    #endregion

    #region RaceDetail

    public ICommand SetWeatherCommand =>
      this._setWeatherCommand ??=
        new AsyncReactiveCommand<string>(this.CanSave).WithSubscribe(p => this.model.Info.Value != null ? this.model.Info.Value.SetWeatherAsync(p) : Task.CompletedTask);
    private AsyncReactiveCommand<string>? _setWeatherCommand;

    public ICommand SetConditionCommand =>
      this._setConditionCommand ??=
        new AsyncReactiveCommand<string>(this.CanSave).WithSubscribe(p => this.model.Info.Value != null ? this.model.Info.Value.SetConditionAsync(p) : Task.CompletedTask);
    private AsyncReactiveCommand<string>? _setConditionCommand;

    #endregion

    #region 拡張分析テーブルの設定

    public ICommand AddAnalysisTableConfigCommand =>
      this._addAnalysisTableConfigCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(obj => this.AppSettings.Value?.AnalysisTableConfig.AddTableAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _addAnalysisTableConfigCommand;

    public ICommand RemoveAnalysisTableConfigCommand =>
      this._removeAnalysisTableConfigCommand ??=
        new AsyncReactiveCommand<AnalysisTableSurface>(this.CanSave).WithSubscribe(obj => this.AppSettings.Value?.AnalysisTableConfig.RemoveTableAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _removeAnalysisTableConfigCommand;

    public ICommand UpAnalysisTableConfigCommand =>
      this._upAnalysisTableConfigCommand ??=
        new AsyncReactiveCommand<AnalysisTableSurface>(this.CanSave).WithSubscribe(obj => this.AppSettings.Value?.AnalysisTableConfig.UpTableAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _upAnalysisTableConfigCommand;

    public ICommand DownAnalysisTableConfigCommand =>
     this._downAnalysisTableConfigCommand ??=
       new AsyncReactiveCommand<AnalysisTableSurface>(this.CanSave).WithSubscribe(obj => this.AppSettings.Value?.AnalysisTableConfig.DownTableAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _downAnalysisTableConfigCommand;

    public ICommand AddAnalysisTableRowConfigCommand =>
      this._addAnalysisTableConfigItemCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(obj => this.AppSettings.Value?.AnalysisTableConfig.AddTableRowAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _addAnalysisTableConfigItemCommand;

    public ICommand DeleteAnalysisTableRowConfigCommand =>
      this._deleteAnalysisTableConfigItemCommand ??=
        new AsyncReactiveCommand<AnalysisTableRow>(this.CanSave).WithSubscribe(obj => this.AppSettings.Value?.AnalysisTableConfig.RemoveTableRowAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _deleteAnalysisTableConfigItemCommand;

    public ICommand UpAnalysisTableRowConfigCommand =>
      this._upAnalysisTableConfigItemCommand ??=
        new AsyncReactiveCommand<AnalysisTableRow>(this.CanSave).WithSubscribe(obj => this.AppSettings.Value?.AnalysisTableConfig.UpTableRowAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _upAnalysisTableConfigItemCommand;

    public ICommand DownAnalysisTableRowConfigCommand =>
     this._downAnalysisTableConfigItemCommand ??=
       new AsyncReactiveCommand<AnalysisTableRow>(this.CanSave).WithSubscribe(obj => this.AppSettings.Value?.AnalysisTableConfig.DownTableRowAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _downAnalysisTableConfigItemCommand;

    public ICommand UnselectAnalysisTableRowWeightCommand =>
      this._unselectAnalysisTableRowWeightCommand ??=
        new ReactiveCommand<AnalysisTableRow>(this.CanSave).WithSubscribe(obj => this.AppSettings.Value?.AnalysisTableConfig.UnselectTableRowWeight(obj)).AddTo(this._disposables);
    private ICommand? _unselectAnalysisTableRowWeightCommand;

    public ICommand UnselectAnalysisTableRowWeight2Command =>
      this._unselectAnalysisTableRowWeight2Command ??=
        new ReactiveCommand<AnalysisTableRow>(this.CanSave).WithSubscribe(obj => this.AppSettings.Value?.AnalysisTableConfig.UnselectTableRowWeight2(obj)).AddTo(this._disposables);
    private ICommand? _unselectAnalysisTableRowWeight2Command;

    public ICommand UnselectAnalysisTableRowWeight3Command =>
      this._unselectAnalysisTableRowWeight3Command ??=
        new ReactiveCommand<AnalysisTableRow>(this.CanSave).WithSubscribe(obj => this.AppSettings.Value?.AnalysisTableConfig.UnselectTableRowWeight3(obj)).AddTo(this._disposables);
    private ICommand? _unselectAnalysisTableRowWeight3Command;

    public ICommand UnselectAnalysisTableRowParentCommand =>
      this._unselectAnalysisTableRowParentCommand ??=
        new ReactiveCommand<AnalysisTableRow>(this.CanSave).WithSubscribe(obj => this.AppSettings.Value?.AnalysisTableConfig.UnselectTableRowParent(obj)).AddTo(this._disposables);
    private ICommand? _unselectAnalysisTableRowParentCommand;

    #endregion

    #region 拡張分析の重み

    public ICommand AddAnalysisTableWeightCommand =>
      this._addAnalysisTableWeightCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(obj => this.AppSettings.Value?.AnalysisTableConfig.AddWeightAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _addAnalysisTableWeightCommand;

    public ICommand RemoveAnalysisTableWeightCommand =>
      this._removeAnalysisTableWeightCommand ??=
        new AsyncReactiveCommand<AnalysisTableWeight>(this.CanSave).WithSubscribe(obj => this.AppSettings.Value?.AnalysisTableConfig.RemoveWeightAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _removeAnalysisTableWeightCommand;

    public ICommand AddAnalysisTableWeightRowCommand =>
      this._addAnalysisTableWeightRowCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(obj => this.AppSettings.Value?.AnalysisTableConfig.AddWeightRowAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _addAnalysisTableWeightRowCommand;

    public ICommand DeleteAnalysisTableWeightRowCommand =>
      this._deleteAnalysisTableWeightRowCommand ??=
        new AsyncReactiveCommand<AnalysisTableWeightRow>(this.CanSave).WithSubscribe(obj => this.AppSettings.Value?.AnalysisTableConfig.RemoveTableWeightRowAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _deleteAnalysisTableWeightRowCommand;

    public ICommand AddAnalysisTableWeightRowBulkCommand =>
      this._addAnalysisTableWeightRowBulkCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(obj => this.AppSettings.Value?.AnalysisTableConfig.AddWeightRowsBulkAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _addAnalysisTableWeightRowBulkCommand;

    public ICommand ReplaceAnalysisTableWeightRowBulkCommand =>
      this._replaceAnalysisTableWeightRowBulkCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(obj => this.AppSettings.Value?.AnalysisTableConfig.ReplaceWeightRowsBulkAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _replaceAnalysisTableWeightRowBulkCommand;

    public ICommand ClearAnalysisTableWeightRowsCommand =>
      this._clearAnalysisTableWeightRowsCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(obj => this.AppSettings.Value?.AnalysisTableConfig.ClearWeightRowsAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _clearAnalysisTableWeightRowsCommand;

    public ICommand UpAnalysisTableWeightRowCommand =>
      this._upAnalysisTableWeightRowCommand ??=
        new AsyncReactiveCommand<AnalysisTableWeightRow>(this.CanSave).WithSubscribe(obj => this.AppSettings.Value?.AnalysisTableConfig.UpTableWeightRowAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _upAnalysisTableWeightRowCommand;

    public ICommand DownAnalysisTableWeightRowCommand =>
      this._downAnalysisTableWeightRowCommand ??=
        new AsyncReactiveCommand<AnalysisTableWeightRow>(this.CanSave).WithSubscribe(obj => this.AppSettings.Value?.AnalysisTableConfig.DownTableWeightRowAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _downAnalysisTableWeightRowCommand;

    #endregion

    #region 重みの区切り

    public ICommand AddAnalysisTableDelimiterCommand =>
      this._addAnalysisTableDelimiterCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(obj => this.AppSettings.Value?.AnalysisTableConfig.AddDelimiterAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _addAnalysisTableDelimiterCommand;

    public ICommand RemoveAnalysisTableDelimiterCommand =>
      this._removeAnalysisTableDelimiterCommand ??=
        new AsyncReactiveCommand<ValueDelimiter>(this.CanSave).WithSubscribe(obj => this.AppSettings.Value?.AnalysisTableConfig.RemoveDelimiterAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _removeAnalysisTableDelimiterCommand;

    public ICommand AddAnalysisTableDelimiterRowCommand =>
      this._addAnalysisTableDelimiterRowCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(obj => this.AppSettings.Value?.AnalysisTableConfig.AddDelimiterRowAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _addAnalysisTableDelimiterRowCommand;

    public ICommand DeleteAnalysisTableDelimiterRowCommand =>
      this._deleteAnalysisTableDelimiterRowCommand ??=
        new AsyncReactiveCommand<ValueDelimiterRow>(this.CanSave).WithSubscribe(obj => this.AppSettings.Value?.AnalysisTableConfig.RemoveDelimiterRowAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _deleteAnalysisTableDelimiterRowCommand;

    public ICommand UpAnalysisTableDelimiterRowCommand =>
      this._upAnalysisTableDelimiterRowCommand ??=
        new AsyncReactiveCommand<ValueDelimiterRow>(this.CanSave).WithSubscribe(obj => this.AppSettings.Value?.AnalysisTableConfig.UpDelimiterRowAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _upAnalysisTableDelimiterRowCommand;

    public ICommand DownAnalysisTableDelimiterRowCommand =>
      this._downAnalysisTableDelimiterRowCommand ??=
        new AsyncReactiveCommand<ValueDelimiterRow>(this.CanSave).WithSubscribe(obj => this.AppSettings.Value?.AnalysisTableConfig.DownDelimiterRowAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _downAnalysisTableDelimiterRowCommand;

    public ICommand AddAnalysisTableSelectedDelimiterCommand =>
      this._addAnalysisTableSelectedDelimiterCommand ??=
        new ReactiveCommand().WithSubscribe(obj => this.AppSettings.Value?.AnalysisTableConfig.AddSelectedDelimiter()).AddTo(this._disposables);
    private ICommand? _addAnalysisTableSelectedDelimiterCommand;

    public ICommand RemoveAnalysisTableSelectedDelimiterCommand =>
      this._removeAnalysisTableSelectedDelimiterCommand ??=
        new ReactiveCommand().WithSubscribe(obj => this.AppSettings.Value?.AnalysisTableConfig.RemoveSelectedDelimiter()).AddTo(this._disposables);
    private ICommand? _removeAnalysisTableSelectedDelimiterCommand;

    #endregion

    #region ATスクリプト

    public ICommand AddAnalysisTableScriptCommand =>
      this._addAnalysisTableScriptCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(obj => this.AnalysisTableScriptConfig.AddConfigAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _addAnalysisTableScriptCommand;

    public ICommand RemoveAnalysisTableScriptCommand =>
      this._removeAnalysisTableScriptCommand ??=
        new AsyncReactiveCommand<AnalysisTableScriptItem>(this.CanSave).WithSubscribe(obj => this.AnalysisTableScriptConfig.RemoveConfigAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _removeAnalysisTableScriptCommand;

    #endregion

    #region 外部指数

    public ICommand AddExternalNumberConfigCommand => this._addExternalNumberConfigCommand ??=
      new AsyncReactiveCommand(this.CanSave).WithSubscribe(_ => this.ExternalNumber.AddConfigAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _addExternalNumberConfigCommand;

    public ICommand RemoveExternalNumberConfigCommand => this._removeExternalNumberConfigCommand ??=
      new AsyncReactiveCommand<ExternalNumberConfigItem>(this.CanSave).WithSubscribe(obj => this.ExternalNumber.RemoveConfigAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _removeExternalNumberConfigCommand;

    public ICommand LoadExternalNumbersCommand => this._loadExternalNumbersCommand ??=
      new ReactiveCommand<ExternalNumberConfigItem>(this.CanSave).WithSubscribe(obj => obj.BeginLoadDb()).AddTo(this._disposables);
    private ICommand? _loadExternalNumbersCommand;

    #endregion
  }

  public enum DialogType
  {
    Unknown,
    Download,
    RTDownload,
    ScriptBulk,
    Setting,
    Version,
    ErrorSavingMemo,
    ErrorConfigring,
  }
}
