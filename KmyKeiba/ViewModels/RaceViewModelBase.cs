using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Connection;
using KmyKeiba.Models.Race;
using KmyKeiba.Models.Race.ExNumber;
using KmyKeiba.Models.Race.Finder;
using KmyKeiba.Models.Race.HorseMark;
using KmyKeiba.Models.Race.Memo;
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

    public ExternalNumberConfigModel ExternalNumber => ExternalNumberConfigModel.Default;

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

    #region 馬券

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

    #endregion

    #region スクリプト

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

    #endregion

    #region 拡張メモ

    public ICommand AddMemoConfigCommand =>
      this._addMemoConfigCommand ??=
        new AsyncReactiveCommand<object>().WithSubscribe(obj => this.model.Info.Value?.MemoEx.Value?.AddConfigAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _addMemoConfigCommand;

    public ICommand EditMemoConfigCommand =>
      this._editMemoConfigCommand ??=
        new ReactiveCommand<RaceMemoItem>().WithSubscribe(obj => this.model.Info.Value?.MemoEx.Value?.StartEditRaceMemoConfig(obj.Config)).AddTo(this._disposables);
    private ICommand? _editMemoConfigCommand;

    public ICommand SaveMemoConfigCommand =>
      this._saveMemoConfigCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.MemoEx.Value?.UpdateConfigAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _saveMemoConfigCommand;

    public ICommand UpMemoOrderCommand =>
      this._upMemoOrderCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.MemoEx.Value?.UpConfigOrderAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _upMemoOrderCommand;

    public ICommand DownMemoOrderCommand =>
      this._downMemoOrderCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.MemoEx.Value?.DownConfigOrderAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _downMemoOrderCommand;

    public ICommand DeleteMemoConfigCommand =>
      this._deleteMemoConfigCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.MemoEx.Value?.DeleteConfigAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _deleteMemoConfigCommand;

    #endregion

    #region 拡張メモのラベル

    public ICommand AddLabelConfigCommand =>
      this._addLabelConfigCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.MemoEx.Value?.LabelConfig.AddConfigAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _addLabelConfigCommand;

    public ICommand AddLabelConfigItemCommand =>
      this._addLabelConfigItemCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.MemoEx.Value?.LabelConfig.ActiveConfig.Value?.AddItemAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _addLabelConfigItemCommand;

    public ICommand DeleteLabelConfigItemCommand =>
      this._deleteLabelConfigItemCommand ??=
        new AsyncReactiveCommand<PointLabelConfigItem>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.MemoEx.Value?.LabelConfig.ActiveConfig.Value?.RemoveItemAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _deleteLabelConfigItemCommand;

    public ICommand UpLabelConfigItemCommand =>
      this._upLabelConfigItemCommand ??=
        new AsyncReactiveCommand<PointLabelConfigItem>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.MemoEx.Value?.LabelConfig.ActiveConfig.Value?.UpItemAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _upLabelConfigItemCommand;

    public ICommand DownLabelConfigItemCommand =>
      this._downLabelConfigItemCommand ??=
        new AsyncReactiveCommand<PointLabelConfigItem>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.MemoEx.Value?.LabelConfig.ActiveConfig.Value?.DownItemAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _downLabelConfigItemCommand;

    public ICommand DeleteLabelConfigCommand =>
      this._deleteLabelConfigCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.MemoEx.Value?.LabelConfig.DeleteConfigAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _deleteLabelConfigCommand;

    #endregion

    #region 拡張分析テーブル

    public ICommand LoadExAnalysisTableCommand =>
      this._loadExAnalysisTableCommand ??=
        new AsyncReactiveCommand<Models.Race.AnalysisTable.AnalysisTableSurface>().WithSubscribe(table => this.model.Info.Value?.AnalysisTable.Value?.AnalysisTableWithReloadAsync(table) ?? Task.CompletedTask).AddTo(this._disposables);
    private AsyncReactiveCommand<Models.Race.AnalysisTable.AnalysisTableSurface>? _loadExAnalysisTableCommand;

    public ICommand AddAnalysisTableConfigCommand =>
      this._addAnalysisTableConfigCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.AnalysisTable.Value?.Config.AddTableAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _addAnalysisTableConfigCommand;

    public ICommand RemoveAnalysisTableConfigCommand =>
      this._removeAnalysisTableConfigCommand ??=
        new AsyncReactiveCommand<Models.Race.AnalysisTable.AnalysisTableSurface>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.AnalysisTable.Value?.Config.RemoveTableAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _removeAnalysisTableConfigCommand;

    public ICommand UpAnalysisTableConfigCommand =>
      this._upAnalysisTableConfigCommand ??=
        new AsyncReactiveCommand<Models.Race.AnalysisTable.AnalysisTableSurface>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.AnalysisTable.Value?.Config.UpTableAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _upAnalysisTableConfigCommand;

    public ICommand DownAnalysisTableConfigCommand =>
     this._downAnalysisTableConfigCommand ??=
       new AsyncReactiveCommand<Models.Race.AnalysisTable.AnalysisTableSurface>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.AnalysisTable.Value?.Config.DownTableAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _downAnalysisTableConfigCommand;

    public ICommand AddAnalysisTableRowConfigCommand =>
      this._addAnalysisTableConfigItemCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.AnalysisTable.Value?.Config.AddTableRowAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _addAnalysisTableConfigItemCommand;

    public ICommand DeleteAnalysisTableRowConfigCommand =>
      this._deleteAnalysisTableConfigItemCommand ??=
        new AsyncReactiveCommand<Models.Race.AnalysisTable.AnalysisTableRow>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.AnalysisTable.Value?.Config.RemoveTableRowAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _deleteAnalysisTableConfigItemCommand;

    public ICommand UpAnalysisTableRowConfigCommand =>
      this._upAnalysisTableConfigItemCommand ??=
        new AsyncReactiveCommand<Models.Race.AnalysisTable.AnalysisTableRow>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.AnalysisTable.Value?.Config.UpTableRowAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _upAnalysisTableConfigItemCommand;

    public ICommand DownAnalysisTableRowConfigCommand =>
     this._downAnalysisTableConfigItemCommand ??=
       new AsyncReactiveCommand<Models.Race.AnalysisTable.AnalysisTableRow>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.AnalysisTable.Value?.Config.DownTableRowAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _downAnalysisTableConfigItemCommand;

    public ICommand UnselectAnalysisTableRowWeightCommand =>
      this._unselectAnalysisTableRowWeightCommand ??=
        new ReactiveCommand<Models.Race.AnalysisTable.AnalysisTableRow>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.AnalysisTable.Value?.Config.UnselectTableRowWeight(obj)).AddTo(this._disposables);
    private ICommand? _unselectAnalysisTableRowWeightCommand;

    public ICommand UnselectAnalysisTableRowWeight2Command =>
      this._unselectAnalysisTableRowWeight2Command ??=
        new ReactiveCommand<Models.Race.AnalysisTable.AnalysisTableRow>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.AnalysisTable.Value?.Config.UnselectTableRowWeight2(obj)).AddTo(this._disposables);
    private ICommand? _unselectAnalysisTableRowWeight2Command;

    public ICommand UnselectAnalysisTableRowWeight3Command =>
      this._unselectAnalysisTableRowWeight3Command ??=
        new ReactiveCommand<Models.Race.AnalysisTable.AnalysisTableRow>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.AnalysisTable.Value?.Config.UnselectTableRowWeight3(obj)).AddTo(this._disposables);
    private ICommand? _unselectAnalysisTableRowWeight3Command;

    public ICommand UnselectAnalysisTableRowParentCommand =>
      this._unselectAnalysisTableRowParentCommand ??=
        new ReactiveCommand<Models.Race.AnalysisTable.AnalysisTableRow>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.AnalysisTable.Value?.Config.UnselectTableRowParent(obj)).AddTo(this._disposables);
    private ICommand? _unselectAnalysisTableRowParentCommand;

    public ICommand UpdateAnalysisTablesCommand =>
      this._updateAnalysisTablesCommand ??=
        new ReactiveCommand<object>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.AnalysisTable.Value?.ReloadTables()).AddTo(this._disposables);
    private ICommand? _updateAnalysisTablesCommand;

    #endregion

    #region 拡張分析の重み

    public ICommand AddAnalysisTableWeightCommand =>
      this._addAnalysisTableWeightCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.AnalysisTable.Value?.Config.AddWeightAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _addAnalysisTableWeightCommand;

    public ICommand RemoveAnalysisTableWeightCommand =>
      this._removeAnalysisTableWeightCommand ??=
        new AsyncReactiveCommand<Models.Race.AnalysisTable.AnalysisTableWeight>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.AnalysisTable.Value?.Config.RemoveWeightAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _removeAnalysisTableWeightCommand;

    public ICommand AddAnalysisTableWeightRowCommand =>
      this._addAnalysisTableWeightRowCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.AnalysisTable.Value?.Config.AddWeightRowAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _addAnalysisTableWeightRowCommand;

    public ICommand DeleteAnalysisTableWeightRowCommand =>
      this._deleteAnalysisTableWeightRowCommand ??=
        new AsyncReactiveCommand<Models.Race.AnalysisTable.AnalysisTableWeightRow>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.AnalysisTable.Value?.Config.RemoveTableWeightRowAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _deleteAnalysisTableWeightRowCommand;

    public ICommand AddAnalysisTableWeightRowBulkCommand =>
      this._addAnalysisTableWeightRowBulkCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.AnalysisTable.Value?.Config.AddWeightRowsBulkAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _addAnalysisTableWeightRowBulkCommand;

    public ICommand ReplaceAnalysisTableWeightRowBulkCommand =>
      this._replaceAnalysisTableWeightRowBulkCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.AnalysisTable.Value?.Config.ReplaceWeightRowsBulkAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _replaceAnalysisTableWeightRowBulkCommand;

    public ICommand ClearAnalysisTableWeightRowsCommand =>
      this._clearAnalysisTableWeightRowsCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.AnalysisTable.Value?.Config.ClearWeightRowsAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _clearAnalysisTableWeightRowsCommand;

    public ICommand UpAnalysisTableWeightRowCommand =>
      this._upAnalysisTableWeightRowCommand ??=
        new AsyncReactiveCommand<Models.Race.AnalysisTable.AnalysisTableWeightRow>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.AnalysisTable.Value?.Config.UpTableWeightRowAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _upAnalysisTableWeightRowCommand;

    public ICommand DownAnalysisTableWeightRowCommand =>
      this._downAnalysisTableWeightRowCommand ??=
        new AsyncReactiveCommand<Models.Race.AnalysisTable.AnalysisTableWeightRow>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.AnalysisTable.Value?.Config.DownTableWeightRowAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _downAnalysisTableWeightRowCommand;

    #endregion

    #region 重みの区切り

    public ICommand AddAnalysisTableDelimiterCommand =>
      this._addAnalysisTableDelimiterCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.AnalysisTable.Value?.Config.AddDelimiterAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _addAnalysisTableDelimiterCommand;

    public ICommand RemoveAnalysisTableDelimiterCommand =>
      this._removeAnalysisTableDelimiterCommand ??=
        new AsyncReactiveCommand<Models.Race.AnalysisTable.ValueDelimiter>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.AnalysisTable.Value?.Config.RemoveDelimiterAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _removeAnalysisTableDelimiterCommand;

    public ICommand AddAnalysisTableDelimiterRowCommand =>
      this._addAnalysisTableDelimiterRowCommand ??=
        new AsyncReactiveCommand<object>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.AnalysisTable.Value?.Config.AddDelimiterRowAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _addAnalysisTableDelimiterRowCommand;

    public ICommand DeleteAnalysisTableDelimiterRowCommand =>
      this._deleteAnalysisTableDelimiterRowCommand ??=
        new AsyncReactiveCommand<Models.Race.AnalysisTable.ValueDelimiterRow>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.AnalysisTable.Value?.Config.RemoveDelimiterRowAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _deleteAnalysisTableDelimiterRowCommand;

    public ICommand UpAnalysisTableDelimiterRowCommand =>
      this._upAnalysisTableDelimiterRowCommand ??=
        new AsyncReactiveCommand<Models.Race.AnalysisTable.ValueDelimiterRow>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.AnalysisTable.Value?.Config.UpDelimiterRowAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _upAnalysisTableDelimiterRowCommand;

    public ICommand DownAnalysisTableDelimiterRowCommand =>
      this._downAnalysisTableDelimiterRowCommand ??=
        new AsyncReactiveCommand<Models.Race.AnalysisTable.ValueDelimiterRow>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.AnalysisTable.Value?.Config.DownDelimiterRowAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _downAnalysisTableDelimiterRowCommand;

    public ICommand AddAnalysisTableSelectedDelimiterCommand =>
      this._addAnalysisTableSelectedDelimiterCommand ??=
        new ReactiveCommand().WithSubscribe(obj => this.model.Info.Value?.AnalysisTable.Value?.Config.AddSelectedDelimiter()).AddTo(this._disposables);
    private ICommand? _addAnalysisTableSelectedDelimiterCommand;

    public ICommand RemoveAnalysisTableSelectedDelimiterCommand =>
      this._removeAnalysisTableSelectedDelimiterCommand ??=
        new ReactiveCommand().WithSubscribe(obj => this.model.Info.Value?.AnalysisTable.Value?.Config.RemoveSelectedDelimiter()).AddTo(this._disposables);
    private ICommand? _removeAnalysisTableSelectedDelimiterCommand;

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

    #region 拡張分析

    public ICommand AggregateTablesCommand => this._aggregateTablesCommand ??=
      new ReactiveCommand().WithSubscribe(() => this.model.Info.Value?.AnalysisTable.Value?.Aggregate.BeginLoad()).AddTo(this._disposables);
    private ICommand? _aggregateTablesCommand;

    public ICommand ApplyAggregateSuggestionMarksCommand => this._applyAggregateSuggestionMarksCommand ??=
      new AsyncReactiveCommand(this.CanSave).WithSubscribe(() => this.model.Info.Value?.AnalysisTable.Value?.Aggregate.ApplyHorseMarksAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _applyAggregateSuggestionMarksCommand;

    #endregion

    #region 拡張検索

    public ICommand AddFinderConfigCommand =>
      this._addFinderConfigCommand ??=
        new AsyncReactiveCommand<FinderModel>(this.CanSave).WithSubscribe(obj => obj?.Input.AddConfigAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _addFinderConfigCommand;

    public ICommand LoadFinderConfigCommand =>
      this._loadFinderConfigCommand ??=
        new ReactiveCommand<FinderModel>().WithSubscribe(obj => obj?.Input.LoadConfig()).AddTo(this._disposables);
    private ICommand? _loadFinderConfigCommand;

    public ICommand RemoveFinderConfigCommand =>
      this._removeFinderConfigCommand ??=
        new AsyncReactiveCommand<FinderModel>(this.CanSave).WithSubscribe(obj => obj?.Input.RemoveConfigAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _removeFinderConfigCommand;

    public ICommand ClearFinderCacheCommand =>
      this._clearFinderCacheCommand ??=
        new ReactiveCommand<FinderModel>().WithSubscribe(obj => obj?.ClearCache()).AddTo(this._disposables);
    private ICommand? _clearFinderCacheCommand;

    #endregion

    #region 印

    public ICommand AddMarkConfigCommand =>
      this._addMarkConfigCommand ??=
        new AsyncReactiveCommand(this.CanSave).WithSubscribe(_ => this.model.Info.Value?.HorseMark.Value?.Config.AddConfigAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _addMarkConfigCommand;

    public ICommand RemoveMarkConfigCommand =>
      this._removeMarkConfigCommand ??=
        new AsyncReactiveCommand(this.CanSave).WithSubscribe(() => this.model.Info.Value?.HorseMark.Value?.Config.RemoveConfigAsync() ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _removeMarkConfigCommand;

    public ICommand UpMarkConfigCommand =>
      this._upMarkConfigCommand ??=
        new AsyncReactiveCommand<HorseMarkConfig>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.HorseMark.Value?.Config.UpConfigAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _upMarkConfigCommand;

    public ICommand DownMarkConfigCommand =>
      this._downMarkConfigCommand ??=
        new AsyncReactiveCommand<HorseMarkConfig>(this.CanSave).WithSubscribe(obj => this.model.Info.Value?.HorseMark.Value?.Config.DownConfigAsync(obj) ?? Task.CompletedTask).AddTo(this._disposables);
    private ICommand? _downMarkConfigCommand;

    #endregion

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
