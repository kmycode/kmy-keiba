using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KmyKeiba.ViewModels
{
  internal class RaceWindowViewModel : RaceViewModelBase
  {
    public string FirstMessage => string.Empty;

    public bool IsMainWindow => false;


    public RaceWindowViewModel(string key, string horseKey)
    {
      this.model.SetRaceKey(key);
      this.SetActiveHorse(horseKey);
    }

    public void SetActiveHorse(string horseKey)
    {
      _ = this.model.SetActiveHorseAsync(horseKey);
    }

    public ICommand BuyCommand =>
      this._buyCommand ??= new ReactiveCommand(new ReactiveProperty<bool>(false));
    private ReactiveCommand? _buyCommand;

    #region RaceDetail

    public ICommand? UpdateRaceInfoCommand { get; }

    public ICommand? ChangeActiveHorseCommand { get; }

    public ICommand? UpdateScriptCommand { get; }

    public ICommand? SetWeatherCommand { get; }

    public ICommand? SetConditionCommand { get; }

    public ICommand? SetTrioBlockCommand { get; }

    public ICommand? SetTrifectaBlockCommand { get; }

    public ICommand? SetTicketTypeCommand { get; }

    public ICommand? SetTicketFormTypeCommand { get; }

    public ICommand? BuyTicketCommand { get; }

    public ICommand? RemoveTicketCommand { get; }

    public ICommand? UpdateSelectedTicketsCommand { get; }

    public ICommand? UpdateSelectedTicketCountsCommand { get; }

    public ICommand? ApproveScriptMarksCommand { get; }

    public ICommand? ApproveScriptTicketsCommand { get; }

    public ICommand? ApproveReplacingScriptTicketsCommand { get; }

    #endregion
  }
}
