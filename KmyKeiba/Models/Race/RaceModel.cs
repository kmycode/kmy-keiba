using KmyKeiba.Models.Data;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race
{
  internal class RaceModel : IDisposable
  {
    private const string defaultRaceKey = "2020072844070111";    // 地方
    //private const string defaultRaceKey = "2020032807010710";    // 中央
    private readonly MyContext _db = new();
    private readonly CompositeDisposable _disposables = new();

    public ReactiveProperty<string> RaceKey { get; } = new(string.Empty);

    public ReactiveProperty<RaceInfo?> Info { get; } = new();

    public ReactiveProperty<bool> IsLoaded { get; }

    public RaceModel()
    {
      this.IsLoaded = this.Info
        .Select(i => i != null)
        .ToReactiveProperty()
        .AddTo(this._disposables);

      this.RaceKey.Value = defaultRaceKey;

      Task.Run(async () =>
      {
        var race = await RaceInfo.FromKeyAsync(this._db, this.RaceKey.Value);
        this.Info.Value = race;
      });
    }

    public void Dispose()
    {
      this._disposables.Dispose();
    }
  }
}
