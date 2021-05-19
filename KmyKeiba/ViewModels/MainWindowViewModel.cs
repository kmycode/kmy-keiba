using Prism.Mvvm;
using System;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using KmyKeiba.Models.Logics;
using System.Collections.ObjectModel;
using KmyKeiba.JVLink.Entities;

namespace KmyKeiba.ViewModels
{
  public class MainWindowViewModel : BindableBase
  {
    private readonly MainModel model = new();

    public ObservableCollection<Race> Races => this.model.Races;

    public MainWindowViewModel()
    {
      var context = new MyContext();
      context.Database.Migrate();
      context.Dispose();

      this.model.Load();
    }
  }
}
