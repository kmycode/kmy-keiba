﻿using Prism.Mvvm;
using System;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using KmyKeiba.Models.Logics;
using System.Collections.ObjectModel;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Logics.Tabs;
using System.Windows.Input;
using Prism.Commands;
using Reactive.Bindings;
using System.Reactive.Linq;
using Prism.Services.Dialogs;
using KmyKeiba.Data.DataObjects;
using KmyKeiba.JVLink.Wrappers;
using KmyKeiba.Data.Db;
using System.Threading.Tasks;

namespace KmyKeiba.ViewModels
{
  public class MainWindowViewModel : BindableBase
  {
    private readonly IDialogService dialogService;
    private readonly MainModel model = new();

    public ObservableCollection<TabFrame> Tabs => this.model.Tabs;

    public ReactiveProperty<int> TabIndex { get; } = new(0);

    public ReactiveProperty<int> UpdateSize => this.model.UpdateSize;

    public ReactiveProperty<int> Updated => this.model.Updated;

    public ReactiveProperty<bool> IsUpdating => this.model.IsUpdating;

    public ReactiveProperty<bool> IsUpdateError => this.model.IsUpdateError;

    public ReactiveProperty<DateTime> ShowDate => this.model.ShowDate;

    public MainWindowViewModel(IDialogService dialogService)
    {
      this.dialogService = dialogService;

      this.Tabs.CollectionChanged += (_, _) =>
      {
        if (this.TabIndex.Value < 0)
        {
          this.TabIndex.Value = 0;
        }
      };

      this.OpenRaceCommand.Subscribe((r) => this.RunTask(() => this.model.OpenRaceAsync(r)));
      this.OpenRiderCommand.Subscribe((c) => this.RunTask(() => this.model.OpenRiderAsync(c)));
      this.OpenJVLinkLoadDialogCommand.Subscribe(() =>
      {
        this.dialogService.ShowDialog("LoadJVLinkDialog");
        _ = this.model.LoadRacesAsync();
      });
      this.OpenPredictRunningStyleCommand.Subscribe(() =>
      {
        this.dialogService.ShowDialog("PredictRunningStyleDialog");
        _ = this.model.LoadRacesAsync();
      });

      this.OpenJVLinkConfigCommand.Subscribe(() => this.model.OpenJVLinkConfig());
      this.OpenNVLinkConfigCommand.Subscribe(() => this.model.OpenNVLinkConfig());
      this.CloseTabCommand.Subscribe((t) => this.model.CloseTab(t));
      this.CloseUpdateErrorCommand.Subscribe(() => this.model.CloseUpdateError());

      this.UpdateTodayRacesCommand = this.ShowDate
        .Select((d) => d == DateTime.Today)
        .ToReactiveCommand();
      this.UpdateTodayRacesCommand.Subscribe(() => this.RunTask(() => this.model.UpdateTodayRacesAsync()));

      this.UpdateRecentRacesCommand.Subscribe(() => this.RunTask(() => this.model.UpdateRecentRacesAsync()));
      this.UpdateFutureRacesCommand.Subscribe(() => this.RunTask(() => this.model.UpdateFutureRacesAsync()));

      this.UpdateCurrentRaceCommand = this.TabIndex
        .Select((i) => (this.model.Tabs.Count > i && i >= 0) ? this.model.Tabs[i] is RaceTabFrame : false)
        .ToReactiveCommand();
      this.UpdateCurrentRaceCommand.Subscribe(() => this.RunTask(() => this.model.UpdateRacesByTabIndexAsync(this.TabIndex.Value)));

      this.MarkHorseCommand.Subscribe((v) => this.RunTask(() => this.model.MarkHorseAsync(v.Horse, v.Mark)));

      // _ = this.model.LoadRacesAsync();
    }

    private void RunTask(Func<Task> action)
    {
      action();
      // Task.Run(() => action());
    }

    public ReactiveCommand<RaceDataObject> OpenRaceCommand { get; } = new();

    public ReactiveCommand<string> OpenRiderCommand { get; } = new();

    public ReactiveCommand OpenJVLinkLoadDialogCommand { get; } = new();

    public ReactiveCommand OpenPredictRunningStyleCommand { get; } = new();

    public ReactiveCommand OpenJVLinkConfigCommand { get; } = new();

    public ReactiveCommand OpenNVLinkConfigCommand { get; } = new();

    public ReactiveCommand<TabFrame> CloseTabCommand { get; } = new();

    public ReactiveCommand UpdateTodayRacesCommand { get; }

    public ReactiveCommand UpdateRecentRacesCommand { get; } = new();

    public ReactiveCommand UpdateFutureRacesCommand { get; } = new();

    public ReactiveCommand UpdateCurrentRaceCommand { get; }

    public ReactiveCommand CloseUpdateErrorCommand { get; } = new();

    public ReactiveCommand<MarkHorseCommandParameter> MarkHorseCommand { get; } = new();

    public struct MarkHorseCommandParameter
    {
      public RaceHorseDataObject Horse { get; init; }

      public RaceHorseMark Mark { get; init; }
    }
  }
}
