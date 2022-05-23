using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.RList;
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
    //private const string defaultRaceKey = "2020072844070111";    // 地方
    private const string defaultRaceKey = "2020032807010710";    // 中央
    private readonly CompositeDisposable _disposables = new();

    public ReactiveProperty<string> RaceKey { get; } = new(string.Empty);

    public ReactiveProperty<RaceInfo?> Info { get; } = new();

    public RaceList RaceList { get; } = new();

    public ReactiveProperty<bool> IsLoaded { get; }

    public RaceModel()
    {
      this.IsLoaded = this.Info
        .Select(i => i != null)
        .ToReactiveProperty()
        .AddTo(this._disposables);

      this.RaceKey.Value = defaultRaceKey;

      this.RaceList.SelectedRaceKey.Subscribe(key =>
      {
        if (key != null)
        {
          if (key != this.RaceKey.Value)
          {
            Task.Run(async () =>
            {
              this.RaceKey.Value = key;
              this.Info.Value?.Dispose();

              var race = await RaceInfo.FromKeyAsync(this.RaceKey.Value);
              this.Info.Value = race;
            });
          }
        }
        else
        {
          // TODO: レース選択解除状態
        }
      });

      Task.Run(async () =>
      {
        try
        {
          await this.RaceList.UpdateListAsync();
        }
        catch
        {
          // TODO: log
        }

        //var race = await RaceInfo.FromKeyAsync(this._db, this.RaceKey.Value);
        //this.Info.Value = race;
      });
    }

    public async Task ChangeHorseMarkAsync(RaceHorseMark mark, RaceHorseAnalyzer horse)
    {
      using var db = new MyContext();
      db.RaceHorses!.Attach(horse.Data);

      horse.Mark.Value = horse.Data.Mark = mark;

      await db.SaveChangesAsync();
    }

    public void Dispose()
    {
      this._disposables.Dispose();
      this.RaceList.Dispose();
    }
  }
}
