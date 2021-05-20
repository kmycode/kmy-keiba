using KmyKeiba.Models.DataObjects;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Logics.Tabs
{
  class RaceListTabFrame : TabFrame
  {
    public ObservableCollection<RaceDataObject> Races { get; set; } = new();

    public RaceListTabFrame()
    {
      this.CanClose = false;
    }
  }
}
