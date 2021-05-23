using Prism.Mvvm;
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

      this.OpenRaceCommand.Subscribe((r) => _ = this.model.OpenRaceAsync(r));
      this.OpenJVLinkLoadDialogCommand.Subscribe(() =>
      {
        this.dialogService.ShowDialog("LoadJVLinkDialog");
        _ = this.model.LoadRacesAsync();
      });
      this.OpenJVLinkConfigCommand.Subscribe(() => this.model.OpenJVLinkConfig());
      this.OpenNVLinkConfigCommand.Subscribe(() => this.model.OpenNVLinkConfig());
      this.CloseTabCommand.Subscribe((t) => this.model.CloseTab(t));
      this.CloseUpdateErrorCommand.Subscribe(() => this.model.CloseUpdateError());

      this.UpdateTodayRacesCommand = this.ShowDate
        .Select((d) => d == DateTime.Today)
        .ToReactiveCommand();
      this.UpdateTodayRacesCommand.Subscribe(() => _ = this.model.UpdateTodayRacesAsync());

      this.UpdateRecentRacesCommand.Subscribe(() => _ = this.model.UpdateRecentRacesAsync());
      this.UpdateFutureRacesCommand.Subscribe(() => _ = this.model.UpdateFutureRacesAsync());

      this.UpdateCurrentRaceCommand = this.TabIndex
        .Select((i) => (this.model.Tabs.Count > i && i >= 0) ? this.model.Tabs[i] is RaceTabFrame : false)
        .ToReactiveCommand();
      this.UpdateCurrentRaceCommand.Subscribe(() => _ = this.model.UpdateRacesByTabIndexAsync(this.TabIndex.Value));

      // _ = this.model.LoadRacesAsync();
    }

    public ReactiveCommand<RaceDataObject> OpenRaceCommand { get; } = new();

    public ReactiveCommand OpenJVLinkLoadDialogCommand { get; } = new();

    public ReactiveCommand OpenJVLinkConfigCommand { get; } = new();

    public ReactiveCommand OpenNVLinkConfigCommand { get; } = new();

    public ReactiveCommand<TabFrame> CloseTabCommand { get; } = new();

    public ReactiveCommand UpdateTodayRacesCommand { get; }

    public ReactiveCommand UpdateRecentRacesCommand { get; } = new();

    public ReactiveCommand UpdateFutureRacesCommand { get; } = new();

    public ReactiveCommand UpdateCurrentRaceCommand { get; }

    public ReactiveCommand CloseUpdateErrorCommand { get; } = new();
  }
}
