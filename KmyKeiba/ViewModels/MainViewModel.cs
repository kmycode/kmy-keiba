using KmyKeiba.Common;
using KmyKeiba.Models.Race;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public event PropertyChangedEventHandler? PropertyChanged;
  }
}
