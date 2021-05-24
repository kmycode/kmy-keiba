using KmyKeiba.JVLink.Entities;
using KmyKeiba.JVLink.Wrappers;
using KmyKeiba.Models.Data;
using KmyKeiba.Data.DataObjects;
using KmyKeiba.Models.Logics.Tabs;
using KmyKeiba.Models.Threading;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KmyKeiba.Data.Db;

namespace KmyKeiba.Models.Logics
{
  class MainModel
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);
    private readonly JVLinkLoader loader = new();
    private bool isInitialLoaded = false;

    public ObservableCollection<TabFrame> Tabs { get; } = new();

    public ReactiveProperty<int> UpdateSize { get; } = new(1);

    public ReactiveProperty<int> Updated { get; } = new();

    public ReactiveProperty<bool> IsUpdating { get; } = new();

    public ReactiveProperty<bool> IsUpdateError { get; } = new();

    public ReactiveProperty<DateTime> ShowDate { get; } = new(DateTime.Today);

    public MainModel()
    {
      _ = this.InitializeAsync();
    }

    private async Task InitializeAsync()
    {
      var dbm = new DatabaseConfigManager();
      await dbm.TryMigrateAsync();

      this.ShowDate.Subscribe((d) => _ = this.LoadRacesAsync());

      if (!this.isInitialLoaded)
      {
        await this.LoadRacesAsync();
      }
    }

    public async Task LoadRacesAsync()
    {
      logger.Info("Start loading main tab races");
      this.isInitialLoaded = true;

      var tabs = this.Tabs.OfType<RaceListTabFrame>();
      if (tabs.Count() > 1)
      {
        foreach (var ttab in tabs.Skip(1).ToArray())
        {
          this.Tabs.Remove(ttab);
        }
      }
      var tab = tabs.FirstOrDefault();
      if (tab == null)
      {
        tab = new RaceListTabFrame(new());
        this.Tabs.Add(tab);
      }

      try
      {
        using (var db = new MyContext())
        {
          var raceData = await db.Races!
            .Where((r) => r.StartTime >= this.ShowDate.Value.AddDays(0) && r.StartTime < this.ShowDate.Value.AddDays(1))
            .OrderBy((r) => r.Key)
            .ToArrayAsync()
            .ConfigureAwait(false);
          var races = raceData.Select((r) => new RaceDataObject(r)).ToArray();

          UiThreadUtil.Dispatcher?.Invoke(() =>
          {
            tab.Races.Clear();
            tab.Races.AddRange(races);
          });

          logger.Info($"{races.Length} race(s) loaded");
        }

        await this.UpdateExistsTabsAsync();
      }
      catch (Exception ex)
      {
        logger.Error("Load error", ex);
      }
    }

    public async Task OpenRaceAsync(RaceDataObject race)
    {
      try
      {
        var tab = new RaceTabFrame
        {
          Race = { Value = race },
        };
        this.Tabs.Add(tab);

        using (var db = new MyContext())
        {
          await race.SetRaceHorsesAsync(db, 1).ConfigureAwait(false);
        }
        _ = Task.Run(async () =>
        {
          using (var db = new MyContext())
          {
            await Task.WhenAll(race.Horses.Select((h) => h.RequestUniformBitmapAsync(db)).ToArray());
          }
        });

        if (race.Data.DataStatus < RaceDataStatus.PreliminaryGradeFull)
        {
          _ = Task.Run(() =>
          {
            _ = this.UpdateRaceAsync(race.Data.Key);
          });
        }
      }
      catch
      {
      }
    }

    public async Task OpenRiderAsync(string code)
    {
      try
      {
        RiderDataObject rider;

        using (var db = new MyContext())
        {
          rider = await RiderDataObject.CreateAsync(db, code).ConfigureAwait(false);
        }

        UiThreadUtil.Dispatcher?.Invoke(() =>
        {
          var tab = new RiderTabFrame
          {
            Rider = { Value = rider },
          };
          this.Tabs.Add(tab);
        });
      }
      catch (Exception ex)
      {
      }
    }

    public async Task UpdateRacesByTabIndexAsync(int index)
    {
      var tab = this.Tabs[index];
      if (tab is RaceTabFrame r)
      {
        await this.UpdateRaceAsync(r.Race.Value.Data.Key);
      }
    }

    public async Task UpdateTodayRacesAsync()
      => await this.UpdateRacesAsync(null, DateTime.Today);

    public async Task UpdateRecentRacesAsync()
      => await this.UpdateRacesAsync(null, DateTime.Today.AddDays(-7));

    public async Task UpdateRaceAsync(string key)
      => await this.UpdateRacesAsync(key, null);

    public async Task UpdateFutureRacesAsync()
    {
      this.UpdateSize.Value = 1;
      this.Updated.Value = 0;
      this.IsUpdating.Value = true;
      this.IsUpdateError.Value = false;

      if (JVLinkObject.Central.IsError && JVLinkObject.Local.IsError)
      {
        this.IsUpdateError.Value = true;
        return;
      }

      try
      {
        // 将来のレースを更新する
        try
        {
          await this.loader.LoadCentralAsync(DateTime.Today);
        }
        catch
        {
        }

        try
        {
          await this.loader.LoadLocalAsync(DateTime.Today);
        }
        catch
        {
        }

        await this.UpdateExistsTabsAsync();
        this.IsUpdating.Value = false;
      }
      catch
      {
        this.IsUpdateError.Value = true;
      }
    }

    private async Task UpdateRacesAsync(string? raceKey, DateTime? date)
    {
      if (this.IsUpdating.Value)
      {
        return;
      }

      this.UpdateSize.Value = 1;
      this.Updated.Value = 0;
      this.IsUpdating.Value = true;
      this.IsUpdateError.Value = false;

      if (JVLinkObject.Central.IsError && JVLinkObject.Local.IsError)
      {
        this.IsUpdateError.Value = true;
        return;
      }

      try
      {
        // 今日のレースを更新する
        IEnumerable<(string Key, RaceCourse Course)> raceKeys;
        using (var db = new MyContext())
        {
          var from = DateTime.Today.AddDays(-7);
          var table = db.Races!.Where((r) => r.StartTime >= from);
          if (raceKey != null)
          {
            table = table.Where((r) => r.Key == raceKey);
          }
          else if (date != null)
          {
            var d = (DateTime)date;
            table = table.Where((r) => r.StartTime >= date);
            // table = table.Where((r) => (r.StartTime > DateTime.Now && (short)r.DataStatus < 2) || (r.StartTime <= DateTime.Now && (short)r.DataStatus < 6));
          }
          else
          {
            table = table.Where((r) => (r.StartTime > DateTime.Now && (short)r.DataStatus < 2) || (r.StartTime <= DateTime.Now && (short)r.DataStatus < 6));
          }

          var keys = await table
            .Select((r) => new { r.Key, r.Course, })
            .ToArrayAsync()
            .ConfigureAwait(false);
          raceKeys = keys.Select((k) => (k.Key, k.Course));
        }

        this.UpdateSize.Value = raceKeys.Count();
        foreach (var key in raceKeys)
        {
          try
          {
            var type = key.Course.GetCourseType();
            var link = type switch
            {
              RaceCourseType.Central => JVLinkObject.Central,
              RaceCourseType.Local => JVLinkObject.Local,
              _ => null,
            };
            if (link != null && !link.IsError)
            {
              await this.loader.LoadAsync(link, JVLinkDataspec.RB12, JVLinkOpenOption.RealTime, key.Key, null, null).ConfigureAwait(false);
              await this.loader.LoadAsync(link, JVLinkDataspec.RB31, JVLinkOpenOption.RealTime, key.Key, null, null);
              await this.UpdateExistsTabsAsync();
            }

            if (this.loader.IsError.Value)
            {
              this.IsUpdateError.Value = true;
            }
          }
          catch
          {
            this.IsUpdateError.Value = true;
          }

          if (this.loader.IsError.Value)
          {
            this.IsUpdateError.Value = true;
          }
          this.Updated.Value++;
        }

        if (!this.IsUpdateError.Value)
        {
          this.IsUpdating.Value = false;
        }
      }
      catch
      {
        this.IsUpdateError.Value = true;
      }
    }

    private async Task UpdateExistsTabsAsync()
    {
      using (var db = new MyContext())
      {
        await this.UpdateExistsTabsAsync(db);
      }
    }

    private async Task UpdateExistsTabsAsync(MyContext db)
    {
      foreach (var tab in this.Tabs.OfType<RaceTabFrame>())
      {
        var race = await db.Races!.FirstOrDefaultAsync((r) => r.Key == tab.Race.Value.Data.Key);
        if (race != null)
        {
          var obj = new RaceDataObject(race);
          await obj.SetRaceHorsesAsync(db);
          tab.Race.Value = obj;

          _ = Task.Run(async () =>
          {
            using (var d = new MyContext())
            {
              await Task.WhenAll(obj.Horses.Select((h) => h.RequestUniformBitmapAsync(d)).ToArray());
            }
          });
        }
      }
    }

    public async Task MarkHorseAsync(RaceHorseDataObject horse, RaceHorseMark mark)
    {
      if (horse == null)
      {
        return;
      }

      using (var db = new MyContext())
      {
        var h = await db.RaceHorses!.FindAsync(horse.Data.Id);
        if (h != null)
        {
          h.Mark = mark;
          horse.Mark.Value = mark;
          await db.SaveChangesAsync();
        }
      }
    }

    public void CloseUpdateError()
    {
      this.IsUpdating.Value = false;
      this.IsUpdateError.Value = false;
    }

    public void CloseTab(TabFrame tab)
    {
      this.Tabs.Remove(tab);

      if (tab is IDisposable disposable)
      {
        disposable.Dispose();
      }
    }

    public void OpenJVLinkConfig()
    {
      JVLinkObject.Central.OpenConfigWindow();
    }

    public void OpenNVLinkConfig()
    {
      JVLinkObject.Local.OpenConfigWindow();
    }
  }
}
