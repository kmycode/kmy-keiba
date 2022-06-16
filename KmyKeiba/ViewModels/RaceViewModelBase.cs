using KmyKeiba.JVLink.Entities;
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
        new ReactiveCommand<uint>().WithSubscribe((id) => this.model.Info.Value?.SetActiveHorse(id));
    private ReactiveCommand<uint>? _changeHorseNumberCommand;

    public ICommand UpdateScriptCommand =>
      this._updateScriptCommand ??=
        new AsyncReactiveCommand().WithSubscribe(() => this.model.Info.Value != null ? this.model.Info.Value.Script.UpdateAsync() : Task.CompletedTask);
    private AsyncReactiveCommand? _updateScriptCommand;

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

#pragma warning disable CS0067
    public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
  }
}
