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
using KmyKeiba.Models.DataObjects;

namespace KmyKeiba.ViewModels
{
  public class MainWindowViewModel : BindableBase
  {
    private readonly IDialogService dialogService;
    private readonly MainModel model = new();

    public ObservableCollection<TabFrame> Tabs => this.model.Tabs;

    public ReactiveProperty<int> TabIndex { get; } = new(0);

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

      this.OpenRaceCommand.Subscribe((r) => this.model.OpenRace(r));
      this.OpenJVLinkLoadDialogCommand.Subscribe(() =>
      {
        this.dialogService.ShowDialog("LoadJVLinkDialog");
        this.model.LoadAsync();
      });
      this.CloseTabCommand.Subscribe((t) => this.model.CloseTab(t));

      this.model.LoadAsync();
    }

    public ReactiveCommand<RaceDataObject> OpenRaceCommand { get; } = new();

    public ReactiveCommand OpenJVLinkLoadDialogCommand { get; } = new();

    public ReactiveCommand<TabFrame> CloseTabCommand { get; } = new();
  }
}
