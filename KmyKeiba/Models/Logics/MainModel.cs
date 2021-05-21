using KmyKeiba.JVLink.Entities;
using KmyKeiba.JVLink.Wrappers;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.DataObjects;
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

namespace KmyKeiba.Models.Logics
{
  class MainModel
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public ObservableCollection<TabFrame> Tabs { get; } = new();

    public MainModel()
    {
    }

    public async Task LoadAsync()
    {
      logger.Info("Start loading main tab races");
      this.Tabs.Clear();

      try
      {
        using (var db = new MyContext())
        {
          var raceData = await db.Races!
            .Where((r) => r.StartTime >= DateTime.Today && r.StartTime < DateTime.Today.AddDays(1))
            .OrderBy((r) => r.Key)
            .ToArrayAsync();
          var races = raceData.Select((r) => new RaceDataObject(r)).ToArray();

          this.Tabs.Add(new RaceListTabFrame(new(races)));

          logger.Info($"{races.Length} race(s) loaded");
        }
      }
      catch (Exception ex)
      {
        logger.Error("Load error", ex);
        this.Tabs.Add(new RaceListTabFrame(new())
        {
          IsRaceLoadError = { Value = true, },
        });
      }
    }

    public async Task OpenRaceAsync(RaceDataObject race)
    {
      using (var db = new MyContext())
      {
        await race.SetRaceHorsesAsync(db, 1);
      }

      this.Tabs.Add(new RaceTabFrame
      {
        Race = { Value = race },
      });
    }

    public void CloseTab(TabFrame tab)
    {
      this.Tabs.Remove(tab);

      if (tab is IDisposable disposable)
      {
        disposable.Dispose();
      }
    }
  }
}
