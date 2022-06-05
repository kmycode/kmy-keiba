﻿using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Race;
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

    public ICommand? SetWeatherCommand { get; }

    public ICommand? SetConditionCommand { get; }

    #endregion
  }
}
