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

    public MainWindowViewModel(IDialogService dialogService)
    {
      this.dialogService = dialogService;

      this.OpenRaceCommand.Subscribe((r) => this.model.OpenRace(r));
      this.OpenJVLinkLoadDialogCommand.Subscribe(() => this.dialogService.ShowDialog("LoadJVLinkDialog"));

      this.model.LoadAsync();
    }

    public ReactiveCommand<RaceDataObject> OpenRaceCommand { get; } = new();

    public ReactiveCommand OpenJVLinkLoadDialogCommand { get; } = new();
  }
}
