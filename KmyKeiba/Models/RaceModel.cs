using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models
{
  internal class RaceModel
  {
    private const string defaultRaceKey = "2010073110010511";

    public readonly ReactiveProperty<string> RaceKey = new();

    public RaceModel()
    {
      this.RaceKey.Value = defaultRaceKey;
    }
  }
}
