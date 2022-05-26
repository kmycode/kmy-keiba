using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.RList;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race
{
  internal class RaceModel : IDisposable
  {
    private readonly CompositeDisposable _disposables = new();
    private IDisposable? ticketUpdated;

    public ReactiveProperty<string> RaceKey { get; } = new(string.Empty);

    public ReactiveProperty<RaceInfo?> Info { get; } = new();

    public RaceList RaceList { get; } = new();

    public ReactiveProperty<bool> IsLoaded { get; }

    public ReactiveProperty<bool> IsFirstLoadStarted { get; } = new();

    public RaceModel()
    {
      this.IsLoaded = this.Info
        .Select(i => i != null)
        .ToReactiveProperty()
        .AddTo(this._disposables);

      this.RaceList.SelectedRaceKey.Subscribe(key =>
      {
        if (key != null)
        {
          if (key != this.RaceKey.Value)
          {
            this.RaceKey.Value = key;
            this.UpdateCurrentRaceInfo();
          }
        }
        else
        {
          // TODO: レース選択解除状態
        }
      });
    }

    public void OnDatabaseInitialized()
    {
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

    public void OnSelectedRaceUpdated()
    {
      Task.Run(async () =>
      {
        if (this.Info.Value != null)
        {
          await this.Info.Value.CheckCanUpdateAsync();
        }
      });
    }

    public void UpdateCurrentRaceInfo()
    {
      Task.Run(async () =>
      {
        this.IsFirstLoadStarted.Value = true;

        this.Info.Value?.Dispose();
        this.ticketUpdated?.Dispose();

        var race = await RaceInfo.FromKeyAsync(this.RaceKey.Value);
        this.Info.Value = race;

        if (race == null)
        {
          return;
        }

        if (this.Info.Value?.Payoff != null)
        {
          this.ticketUpdated = this.Info.Value.Payoff.Income.SkipWhile(i => i == 0).Subscribe(income =>
          {
            if (race != null)
            {
              this.RaceList.UpdatePayoff(race.Data.Key, income);
            }
          });
        }
        else
        {
          race!.WaitTicketsAndCallback(tickets =>
          {
            Action act = () =>
            {
              if (tickets.Tickets.Any())
              {
                var money = tickets.Tickets.Sum(t => t.Count.Value * t.Rows.Count * 100);
                this.RaceList.UpdatePayoff(race.Data.Key, money * -1);
              }
              else
              {
                this.RaceList.UpdatePayoff(race.Data.Key, 0);
              }
            };

            var dis = new CompositeDisposable();
            Observable.FromEvent<EventHandler, EventArgs>(a => (s, e) => a(e), dele => tickets.Tickets.TicketCountChanged += dele, dele => tickets.Tickets.TicketCountChanged -= dele)
              .Subscribe(_ => act()).AddTo(dis);
            Observable.FromEvent<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(a => (s, e) => a(e), dele => tickets.Tickets.CollectionChanged += dele, dele => tickets.Tickets.CollectionChanged -= dele)
              .Subscribe(_ => act()).AddTo(dis);
            this.ticketUpdated = dis;
          });
        }
      });
    }

    public void Dispose()
    {
      this._disposables.Dispose();
      this.RaceList.Dispose();
    }
  }
}
