using KmyKeiba.Data.DataObjects;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Logics.Tabs
{
  class RiderTabFrame : TabFrame
  {
    public ReactiveProperty<RiderDataObject> Rider { get; } = new();
  }
}
