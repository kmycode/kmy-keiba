using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Race;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KmyKeiba.ViewModels
{
  internal class MainViewModel : INotifyPropertyChanged
  {
    private readonly RaceModel model = new();

    public ReactiveProperty<RaceInfo?> Race => this.model.Info;

    public ReactiveProperty<bool> IsLoaded => this.model.IsLoaded;

    public MainViewModel()
    {
      // TODO: いずれModelにうつす
      ThemeUtil.Current = ApplicationTheme.Dark;
    }

    /*
    public ICommand UpdateRaceTrendAnalysisCommand =>
      this._updateRaceTrendAnalysisCommand ??=
        new ReactiveCommand().WithSubscribe(() => this.model.BeginUpdateRaceTrendAnalysis());
    private ReactiveCommand? _updateRaceTrendAnalysisCommand;
    */

    public ICommand ChangeActiveHorseCommand =>
      this._changeHorseNumberCommand ??=
        new ReactiveCommand<short>().WithSubscribe((num) => this.model.Info.Value?.SetActiveHorse(num));
    private ReactiveCommand<short>? _changeHorseNumberCommand;

    public ICommand UpdateScriptCommand =>
      this._updateScriptCommand ??=
        new AsyncReactiveCommand().WithSubscribe(() => this.model.Info.Value != null ? this.model.Info.Value.Script.UpdateAsync() : Task.CompletedTask);
    private AsyncReactiveCommand? _updateScriptCommand;

    public ICommand SetWeatherCommand =>
      this._setWeatherCommand ??=
        new AsyncReactiveCommand<string>().WithSubscribe(p => this.model.Info.Value != null ? this.model.Info.Value.SetWeatherAsync(p) : Task.CompletedTask);
    private AsyncReactiveCommand<string>? _setWeatherCommand;

    public ICommand SetConditionCommand =>
      this._setConditionCommand ??=
        new AsyncReactiveCommand<string>().WithSubscribe(p => this.model.Info.Value != null ? this.model.Info.Value.SetConditionAsync(p) : Task.CompletedTask);
    private AsyncReactiveCommand<string>? _setConditionCommand;

    public ICommand SetDoubleCircleMarkCommand =>
      this._setDoubleCircleMarkCommand ??=
        new AsyncReactiveCommand<RaceHorseAnalyzer>().WithSubscribe(p => this.model.ChangeHorseMarkAsync(RaceHorseMark.DoubleCircle, p));
    private AsyncReactiveCommand<RaceHorseAnalyzer>? _setDoubleCircleMarkCommand;

    public ICommand SetCircleMarkCommand =>
      this._setCircleMarkCommand ??=
        new AsyncReactiveCommand<RaceHorseAnalyzer>().WithSubscribe(p => this.model.ChangeHorseMarkAsync(RaceHorseMark.Circle, p));
    private AsyncReactiveCommand<RaceHorseAnalyzer>? _setCircleMarkCommand;

    public ICommand SetTriangleMarkCommand =>
      this._setTriangleMarkCommand ??=
        new AsyncReactiveCommand<RaceHorseAnalyzer>().WithSubscribe(p => this.model.ChangeHorseMarkAsync(RaceHorseMark.Triangle, p));
    private AsyncReactiveCommand<RaceHorseAnalyzer>? _setTriangleMarkCommand;

    public ICommand SetFilledTriangleMarkCommand =>
      this._setFilledTriangleMarkCommand ??=
        new AsyncReactiveCommand<RaceHorseAnalyzer>().WithSubscribe(p => this.model.ChangeHorseMarkAsync(RaceHorseMark.FilledTriangle, p));
    private AsyncReactiveCommand<RaceHorseAnalyzer>? _setFilledTriangleMarkCommand;

    public ICommand SetStarMarkCommand =>
      this._setStarMarkCommand ??=
        new AsyncReactiveCommand<RaceHorseAnalyzer>().WithSubscribe(p => this.model.ChangeHorseMarkAsync(RaceHorseMark.Star, p));
    private AsyncReactiveCommand<RaceHorseAnalyzer>? _setStarMarkCommand;

    public ICommand SetDeletedMarkCommand =>
      this._setDeletedMarkCommand ??=
        new AsyncReactiveCommand<RaceHorseAnalyzer>().WithSubscribe(p => this.model.ChangeHorseMarkAsync(RaceHorseMark.Deleted, p));
    private AsyncReactiveCommand<RaceHorseAnalyzer>? _setDeletedMarkCommand;

    public ICommand SetDefaultMarkCommand =>
      this._setDefaultMarkCommand ??=
        new AsyncReactiveCommand<RaceHorseAnalyzer>().WithSubscribe(p => this.model.ChangeHorseMarkAsync(RaceHorseMark.Default, p));
    private AsyncReactiveCommand<RaceHorseAnalyzer>? _setDefaultMarkCommand;

#pragma warning disable CS0067
    public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
  }
}
