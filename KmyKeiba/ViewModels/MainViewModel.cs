using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Connection;
using KmyKeiba.Models.Race;
using KmyKeiba.Models.RList;
using KmyKeiba.Models.Script;
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

    public bool IsMainWindow => true;

    public RaceList RaceList => this.model.RaceList;

    public ReactiveProperty<bool> IsInitializationError => this.downloader.IsInitializationError;

    public ReactiveProperty<string> DownloaderErrorMessage => this.downloader.ErrorMessage;

    public ReactiveProperty<bool> IsInitialized => this.downloader.IsInitialized;

    public string FirstMessage => this.model.FirstMessage;

    public OpenDialogRequest Dialog { get; } = new();

    public ReactiveProperty<DialogType> CurrentDialog { get; } = new();

    public ReactiveProperty<bool> IsDialogOpen { get; }

    public OpenRaceRequest RaceWindow => OpenRaceRequest.Default;

    public string VersionNumber => Constrants.ApplicationVersion;

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
        })).AddTo(this._disposables);
      Observable.FromEvent<EventHandler, EventArgs>(
        e => (s, a) => e(a),
        dele => this.RaceList.SelectedRaceUpdated += dele,
        dele => this.RaceList.SelectedRaceUpdated -= dele)
        .Subscribe(_ => ThreadUtil.InvokeOnUiThread(() =>
        {
          this.model.OnSelectedRaceUpdated();
        })).AddTo(this._disposables);

      // TODO: いずれModelにうつす
      ThemeUtil.Current = ApplicationTheme.Dark;

      Task.Run(async () =>
      {
        var isFirst = await this.downloader.InitializeAsync();
        if (isFirst)
        {
          // 初期設定画面
          logger.Debug("初めての利用のようです。初期画面を表示します");
          this.CurrentDialog.Value = DialogType.Download;
        }
      });

      logger.Debug("ビューモデルの初期化完了");
    }

    public void OnApplicationExit()
    {
      logger.Debug("アプリ終了処理開始");
      this.downloader.Dispose();
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

    public ICommand OpenVersionDialogCommand =>
      this._openVersionDialogCommand ??=
        new ReactiveCommand().WithSubscribe(() => this.CurrentDialog.Value = DialogType.Version);
    private ReactiveCommand? _openVersionDialogCommand;

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

    public ICommand CloseDialogCommand =>
      this._closeDialogCommand ??=
        new ReactiveCommand().WithSubscribe(() => this.CurrentDialog.Value = DialogType.Unknown);
    private ReactiveCommand? _closeDialogCommand;

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
        new ReactiveCommand().WithSubscribe(() => this.model.RaceList.MoveToNextDay());
    private ReactiveCommand? _moveToNextDayCommand;

    public ICommand MoveToPrevDayCommand =>
      this._moveToPrevDayCommand ??=
        new ReactiveCommand().WithSubscribe(() => this.model.RaceList.MoveToPrevDay());
    private ReactiveCommand? _moveToPrevDayCommand;

    public ICommand OpenJvlinkConfigCommand =>
      this._openJvlinkConfigCommand ??=
        new AsyncReactiveCommand<object>().WithSubscribe(async p => await this.downloader.OpenJvlinkConfigAsync());
    private AsyncReactiveCommand<object>? _openJvlinkConfigCommand;

    public ICommand OpenNvlinkConfigCommand =>
      this._openNvlinkConfigCommand ??=
        new AsyncReactiveCommand<object>().WithSubscribe(async p => await this.downloader.OpenNvlinkConfigAsync());
    private AsyncReactiveCommand<object>? _openNvlinkConfigCommand;

    public ICommand StartDownloadCommand =>
      this._startDownloadCommand ??=
        new ReactiveCommand().WithSubscribe(() => this.downloader.BeginDownload());
    private ReactiveCommand? _startDownloadCommand;

    public ICommand CancelDownloadCommand =>
      this._cancelDownloadCommand ??=
        new AsyncReactiveCommand<object>().WithSubscribe(async p => await this.downloader.CancelDownloadAsync());
    private AsyncReactiveCommand<object>? _cancelDownloadCommand;

    #endregion

    #region RaceDetail

    public ICommand UpdateRaceInfoCommand =>
      this._updateRaceInfoCommand ??=
        new ReactiveCommand().WithSubscribe(() => this.model.UpdateCurrentRace());
    private ReactiveCommand? _updateRaceInfoCommand;

    public ICommand ChangeActiveHorseCommand =>
      this._changeHorseNumberCommand ??=
        new ReactiveCommand<uint>().WithSubscribe((id) => this.model.Info.Value?.SetActiveHorse(id));
    private ReactiveCommand<uint>? _changeHorseNumberCommand;

    public ICommand UpdateScriptCommand =>
      this._updateScriptCommand ??=
        new AsyncReactiveCommand().WithSubscribe(() => this.model.Info.Value != null ? this.model.Info.Value.Script.UpdateAsync() : Task.CompletedTask);
    private AsyncReactiveCommand? _updateScriptCommand;

    public ICommand SetWeatherCommand =>
      this._setWeatherCommand ??=
        new AsyncReactiveCommand<string>(this.CanSave).WithSubscribe(p => this.model.Info.Value != null ? this.model.Info.Value.SetWeatherAsync(p) : Task.CompletedTask);
    private AsyncReactiveCommand<string>? _setWeatherCommand;

    public ICommand SetConditionCommand =>
      this._setConditionCommand ??=
        new AsyncReactiveCommand<string>(this.CanSave).WithSubscribe(p => this.model.Info.Value != null ? this.model.Info.Value.SetConditionAsync(p) : Task.CompletedTask);
    private AsyncReactiveCommand<string>? _setConditionCommand;

    public ICommand SetTrioBlockCommand =>
      this._setTrioBlockCommand ??=
        new ReactiveCommand<OddsBlock<TrioOdds.OddsData>>().WithSubscribe(p =>
        {
          if (this.model.Info.Value?.Odds.Value != null)
          {
            this.model.Info.Value.Odds.Value!.CurrentTrios.Value = p;
          }
        });
    private ReactiveCommand<OddsBlock<TrioOdds.OddsData>>? _setTrioBlockCommand;

    public ICommand SetTrifectaBlockCommand =>
      this._setTrifectaBlockCommand ??=
        new ReactiveCommand<OddsBlock<TrifectaOdds.OddsData>>().WithSubscribe(p =>
        {
          if (this.model.Info.Value?.Odds.Value != null)
          {
            this.model.Info.Value.Odds.Value!.CurrentTrifectas.Value = p;
          }
        });
    private ReactiveCommand<OddsBlock<TrifectaOdds.OddsData>>? _setTrifectaBlockCommand;

    public ICommand SetTicketTypeCommand =>
      this._setTicketTypeCommand ??=
        new ReactiveCommand<string>().WithSubscribe(p => this.model.Info.Value?.Tickets.Value?.SetType(p));
    private ReactiveCommand<string>? _setTicketTypeCommand;

    public ICommand SetTicketFormTypeCommand =>
      this._setTicketFormTypeCommand ??=
        new ReactiveCommand<string>().WithSubscribe(p => this.model.Info.Value?.Tickets.Value?.SetFormType(p));
    private ReactiveCommand<string>? _setTicketFormTypeCommand;

    public ICommand BuyTicketCommand =>
      this._buyTicketCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(p => this.model.Info.Value?.Tickets.Value != null ? this.model.Info.Value.Tickets.Value!.BuyAsync() : Task.CompletedTask);
    private AsyncReactiveCommand<object>? _buyTicketCommand;

    public ICommand RemoveTicketCommand =>
      this._removeTicketCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(p => this.model.Info.Value?.Tickets.Value != null ? this.model.Info.Value.Tickets.Value!.RemoveTicketAsync() : Task.CompletedTask);
    private AsyncReactiveCommand<object>? _removeTicketCommand;

    public ICommand UpdateSelectedTicketsCommand =>
      this._updateSelectedTicketsCommand ??=
        new ReactiveCommand<object>().WithSubscribe(p => this.model.Info.Value?.Tickets.Value?.UpdateIsSelected());
    private ReactiveCommand<object>? _updateSelectedTicketsCommand;

    public ICommand UpdateSelectedTicketCountsCommand =>
      this._updateSelectedTicketCountsCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(p => this.model.Info.Value?.Tickets.Value?.UpdateTicketCountAsync() ?? Task.CompletedTask);
    private AsyncReactiveCommand<object>? _updateSelectedTicketCountsCommand;

    public ICommand ApproveScriptMarksCommand =>
      this._approveScriptMarksCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(p => this.model.Info.Value?.Script.ApproveMarksAsync() ?? Task.CompletedTask);
    private AsyncReactiveCommand<object>? _approveScriptMarksCommand;

    public ICommand ApproveScriptTicketsCommand =>
      this._approveScriptTicketsCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(p => this.model.Info.Value?.Script.ApproveTicketsAsync() ?? Task.CompletedTask);
    private AsyncReactiveCommand<object>? _approveScriptTicketsCommand;

    public ICommand ApproveReplacingScriptTicketsCommand =>
      this._approveReplacingScriptTicketsCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(p => this.model.Info.Value?.Script.ApproveReplacingTicketsAsync() ?? Task.CompletedTask);
    private AsyncReactiveCommand<object>? _approveReplacingScriptTicketsCommand;

    #endregion
  }

  public enum DialogType
  {
    Unknown,
    Download,
    RTDownload,
    ScriptBulk,
    Version,
  }
}
