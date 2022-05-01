using KmyKeiba.Data.DataObjects;
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

    public ObservableCollection<RaceDataObject> Races { get; }

    public ReactiveProperty<bool> IsRaceLoadError { get; } = new(false);

    public ReactiveProperty<bool> IsRacesEmpty { get; } = new(false);

    public RaceListTabFrame(ObservableCollection<RaceDataObject> races)
    {
      this.Races = races;
      this.CanClose = false;

      this.Races.CollectionChanged += (_, _) => this.IsRacesEmpty.Value = !this.Races.Any();
      this.IsRacesEmpty.Value = !this.Races.Any();
    }

    public void Dispose() => this.disposables.Dispose();
  }
}
