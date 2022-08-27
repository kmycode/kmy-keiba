using KmyKeiba.Models.Analysis.Generic;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.Finder
{
  public class FinderTab : ICheckableItem
  {
    public int TabId { get; set; }

    public ReactiveProperty<bool> IsChecked { get; } = new();
  }
}
