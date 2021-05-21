using KmyKeiba.Models.DataObjects;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Logics.Tabs
{
  class RaceListTabFrame : TabFrame, IDisposable
  {
    private readonly CompositeDisposable disposables = new();

    public ObservableCollection<RaceDataObject> Races { get; set; } = new();

    public ReactiveProperty<bool> IsRaceLoadError { get; } = new(false);

    public RaceListTabFrame()
    {
      this.CanClose = false;
    }

    public void Dispose() => this.disposables.Dispose();
  }
}
