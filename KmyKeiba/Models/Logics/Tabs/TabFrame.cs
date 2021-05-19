using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Logics.Tabs
{
  public abstract class TabFrame
  {
    public ReactiveProperty<double> VerticalScroll { get; } = new();

    public bool CanClose { get; set; } = true;
  }
}
