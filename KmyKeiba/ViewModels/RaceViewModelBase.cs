using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Analysis.Table;
using KmyKeiba.Models.Connection;
using KmyKeiba.Models.Race;
using KmyKeiba.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KmyKeiba.ViewModels
{
  internal class RaceViewModelBase : INotifyPropertyChanged, IDisposable
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    protected readonly CompositeDisposable _disposables = new();
    protected readonly RaceModel model = new();
    protected readonly DownloaderModel downloader = DownloaderModel.Instance;

    public DownloaderModel Downloader => this.downloader;

    public ReactiveProperty<RaceInfo?> Race => this.model.Info;

    public ReactiveProperty<bool> IsLoaded => this.model.IsLoaded;

    public ReactiveProperty<bool> IsFirstRaceLoadStarted => this.model.IsFirstLoadStarted;

    public ReactiveProperty<bool> IsViewExpection => this.model.IsViewExpection;

    public ReactiveProperty<bool> IsViewResult => this.model.IsViewResult;

    public ReactiveProperty<bool> IsSelectedAllHorses => this.model.IsSelectedAllHorses;

    public ReactiveProperty<bool> CanSave => this.downloader.CanSaveOthers;

    public ReactiveProperty<bool> IsModelError => this.model.IsError;

    public ReactiveProperty<string> ModelErrorMessage => this.model.ErrorMessage;

    public bool IsRunningAsAdministrator { get; } = JVLinkServiceWatcher.IsRunningAsAdministrator();

    public string VersionNumber => Constrants.ApplicationVersion;

    public RaceViewModelBase()
    {
      this.model.AddTo(this._disposables);
    }

    public void Dispose()
    {
      try
      {
        this._disposables.Dispose();
      }
      catch (Exception ex)
      {
        try
        {
          logger.Warn($"レース {this.Race.Value?.Data.Key} を保持するビューモデルの破棄に失敗", ex);
        }
        catch (Exception ex2)
        {
          // 破棄済のReactivePropertyのプロパティを参照するときに例外を出すかもしれないので（※出さない）
          logger.Warn($"レースを保持するビューモデルの破棄に失敗", ex);
          logger.Warn($"保持するレースのデータ参照に失敗", ex2);
        }
      }
    }

    public ICommand ChangeActiveHorseCommand =>
      this._changeHorseNumberCommand ??=
        new ReactiveCommand<uint>().WithSubscribe((id) => this.model.Info.Value?.SetActiveHorse(id)).AddTo(this._disposables);
    private ReactiveCommand<uint>? _changeHorseNumberCommand;

    public ICommand UpdateScriptCommand =>
      this._updateScriptCommand ??=
        new AsyncReactiveCommand().WithSubscribe(() => this.model.Info.Value != null ? this.model.Info.Value.Script.UpdateAsync() : Task.CompletedTask).AddTo(this._disposables);
    private AsyncReactiveCommand? _updateScriptCommand;

    public ICommand SetTrioBlockCommand =>
      this._setTrioBlockCommand ??=
        new ReactiveCommand<OddsBlock<TrioOdds.OddsData>>().WithSubscribe(p =>
        {
          if (this.model.Info.Value?.Odds.Value != null)
          {
            this.model.Info.Value.Odds.Value!.CurrentTrios.Value = p;
          }
        }).AddTo(this._disposables);
    private ReactiveCommand<OddsBlock<TrioOdds.OddsData>>? _setTrioBlockCommand;

    public ICommand SetTrifectaBlockCommand =>
      this._setTrifectaBlockCommand ??=
        new ReactiveCommand<OddsBlock<TrifectaOdds.OddsData>>().WithSubscribe(p =>
        {
          if (this.model.Info.Value?.Odds.Value != null)
          {
            this.model.Info.Value.Odds.Value!.CurrentTrifectas.Value = p;
          }
        }).AddTo(this._disposables);
    private ReactiveCommand<OddsBlock<TrifectaOdds.OddsData>>? _setTrifectaBlockCommand;

    public ICommand SetTicketTypeCommand =>
      this._setTicketTypeCommand ??=
        new ReactiveCommand<string>().WithSubscribe(p => this.model.Info.Value?.Tickets.Value?.SetType(p)).AddTo(this._disposables);
    private ReactiveCommand<string>? _setTicketTypeCommand;

    public ICommand SetTicketFormTypeCommand =>
      this._setTicketFormTypeCommand ??=
        new ReactiveCommand<string>().WithSubscribe(p => this.model.Info.Value?.Tickets.Value?.SetFormType(p)).AddTo(this._disposables);
    private ReactiveCommand<string>? _setTicketFormTypeCommand;

    public ICommand BuyTicketCommand =>
      this._buyTicketCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(p => this.model.Info.Value?.Tickets.Value != null ? this.model.Info.Value.Tickets.Value!.BuyAsync() : Task.CompletedTask).AddTo(this._disposables);
    private AsyncReactiveCommand<object>? _buyTicketCommand;

    public ICommand RemoveTicketCommand =>
      this._removeTicketCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(p => this.model.Info.Value?.Tickets.Value != null ? this.model.Info.Value.Tickets.Value!.RemoveTicketAsync() : Task.CompletedTask).AddTo(this._disposables);
    private AsyncReactiveCommand<object>? _removeTicketCommand;

    public ICommand UpdateSelectedTicketsCommand =>
      this._updateSelectedTicketsCommand ??=
        new ReactiveCommand<object>().WithSubscribe(p => this.model.Info.Value?.Tickets.Value?.UpdateIsSelected()).AddTo(this._disposables);
    private ReactiveCommand<object>? _updateSelectedTicketsCommand;

    public ICommand UpdateSelectedTicketCountsCommand =>
      this._updateSelectedTicketCountsCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(p => this.model.Info.Value?.Tickets.Value?.UpdateTicketCountAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private AsyncReactiveCommand<object>? _updateSelectedTicketCountsCommand;

    public ICommand ApproveScriptMarksCommand =>
      this._approveScriptMarksCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(p => this.model.Info.Value?.Script.ApproveMarksAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private AsyncReactiveCommand<object>? _approveScriptMarksCommand;

    public ICommand ApproveScriptTicketsCommand =>
      this._approveScriptTicketsCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(p => this.model.Info.Value?.Script.ApproveTicketsAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private AsyncReactiveCommand<object>? _approveScriptTicketsCommand;

    public ICommand ApproveReplacingScriptTicketsCommand =>
      this._approveReplacingScriptTicketsCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(p => this.model.Info.Value?.Script.ApproveReplacingTicketsAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private AsyncReactiveCommand<object>? _approveReplacingScriptTicketsCommand;

    public ICommand LoadAnalysisTableRowCommand =>
      this._loadAnalysisTableRowCommand ??=
        new AsyncReactiveCommand<AnalysisTableRow>().WithSubscribe(async row => await row.LoadAsync()).AddTo(this._disposables);
    private AsyncReactiveCommand<AnalysisTableRow>? _loadAnalysisTableRowCommand;

    public ICommand LoadAnalysisTableCommand =>
      this._loadAnalysisTableCommand ??=
        new AsyncReactiveCommand<AnalysisTable>().WithSubscribe(async table => await table.LoadAllAsync()).AddTo(this._disposables);
    private AsyncReactiveCommand<AnalysisTable>? _loadAnalysisTableCommand;

    public ICommand AddMemoConfigCommand =>
      this._addMemoConfigCommand ??=
        new AsyncReactiveCommand<object>().WithSubscribe(obj => this.model.Info.Value?.MemoEx.Value?.AddRaceMemoConfigAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _addMemoConfigCommand;

    public ICommand OpenRaceWindowCommand =>
      this._openRaceWindowCommand ??=
        new ReactiveCommand<object>().WithSubscribe(obj =>
        {
          string? raceKey = null;
          string? raceHorseKey = null;
          if (obj is RaceHorseAnalyzer rht)
          {
            raceKey = rht.Race.Key;
            raceHorseKey = rht.Data.Key;
          }
          if (obj is RaceData rd)
          {
            raceKey = rd.Key;
          }
          if (obj is RaceHorseData rh)
          {
            raceKey = rh.RaceKey;
            raceHorseKey = rh.Key;
          }
          if (raceKey != null)
          {
            if (raceHorseKey == null)
            {
              OpenRaceRequest.Default.Request(raceKey);
            }
            else
            {
              OpenRaceRequest.Default.Request(raceKey, raceHorseKey);
            }
          }
        }).AddTo(this._disposables);
    private ReactiveCommand<object>? _openRaceWindowCommand;

#pragma warning disable CS0067
    public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
  }
}
