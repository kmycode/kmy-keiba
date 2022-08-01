using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.Expand
{
  public class PointLabelModel
  {
  }

  public class PointLabelConfig
  {
    public ReactiveProperty<string> Name { get; } = new();

    public ReactiveCollection<PointLabelItem> Items { get; } = new();
  }

  public class PointLabelItem
  {
    public ReactiveProperty<string> Name { get; } = new();
  }
}
