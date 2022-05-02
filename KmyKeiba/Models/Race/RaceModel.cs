using KmyKeiba.Models.Data;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race
{
  internal class RaceModel
  {
    private const string defaultRaceKey = "2010073110010511";

    public readonly ReactiveProperty<string> RaceKey = new();

    public readonly ReactiveProperty<RaceInfo> Info = new();

    public RaceModel()
    {
      var db = new MyContext();

      this.RaceKey.Value = defaultRaceKey;
      
      var info = new RaceInfo(db, this.RaceKey.Value);
      info.InitializeAsync()
        .ContinueWith((task) => this.Info.Value = info);
    }
  }
}
