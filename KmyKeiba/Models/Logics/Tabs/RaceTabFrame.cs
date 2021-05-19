using KmyKeiba.JVLink.Entities;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Logics.Tabs
{
  class RaceTabFrame : TabFrame
  {
    public ReactiveProperty<Race> Race { get; } = new();
  }
}
