using Prism.Mvvm;
using System;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using KmyKeiba.Models.Logics;
using System.Collections.ObjectModel;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Logics.Tabs;
using ReactiveUI;
using System.Windows.Input;
using Prism.Commands;

namespace KmyKeiba.ViewModels
{
  public class MainWindowViewModel : BindableBase
  {
    private readonly MainModel model = new();

    public ObservableCollection<Race> Races => this.model.Races;

    public ObservableCollection<TabFrame> Tabs => this.model.Tabs;

    public MainWindowViewModel()
    {
      var context = new MyContext();
      context.Database.Migrate();
      context.Dispose();

      this.model.LoadAsync();
    }

    public ICommand OpenRaceCommand =>
      this._openRaceCommand ??= new DelegateCommand<Race>((r) => this.model.OpenRace(r));
    private ICommand? _openRaceCommand;
  }
}
