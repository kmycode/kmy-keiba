using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
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

    public ICommand SetTrioBlockCommand =>
      this._setTrioBlockCommand ??=
        new ReactiveCommand<OddsBlock<TrioOdds.OddsData>>().WithSubscribe(p =>
        {
          if (this.model.Info.Value?.Odds.Value != null)
          {
            this.model.Info.Value.Odds.Value!.CurrentTrios.Value = p;
          }
        });
    private ReactiveCommand<OddsBlock<TrioOdds.OddsData>> _setTrioBlockCommand;

    public ICommand SetTrifectaBlockCommand =>
      this._setTrifectaBlockCommand ??=
        new ReactiveCommand<OddsBlock<TrifectaOdds.OddsData>>().WithSubscribe(p =>
        {
          if (this.model.Info.Value?.Odds.Value != null)
          {
            this.model.Info.Value.Odds.Value!.CurrentTrifectas.Value = p;
          }
        });
    private ReactiveCommand<OddsBlock<TrifectaOdds.OddsData>> _setTrifectaBlockCommand;

#pragma warning disable CS0067
    public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
  }
}
