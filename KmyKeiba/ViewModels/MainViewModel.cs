using KmyKeiba.Common;
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

#pragma warning disable CS0067
    public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
  }
}
