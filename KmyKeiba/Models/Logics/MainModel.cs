using KmyKeiba.JVLink.Entities;
using KmyKeiba.JVLink.Wrappers;
using KmyKeiba.Models.Logics.Tabs;
using KmyKeiba.Models.Threading;
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
    public ObservableCollection<TabFrame> Tabs { get; } = new();

    public ObservableCollection<Race> Races { get; } = new();

    public MainModel()
    {
    }

    public async Task LoadAsync()
    {
      await Task.Run(() =>
      {
        var link = JVLinkObject.Local;
        var now = DateTime.Now;
        var reader = link.StartRead(JVLinkDataspec.Race,
          JVLinkOpenOption.Normal,
          new DateTime(now.Year, now.Month, now.Day - 1),
          new DateTime(now.Year, now.Month, now.Day));

        while (reader.DownloadCount > reader.DownloadedCount)
        {
          Task.Delay(100).Wait();
        }

        var data = reader.Load();

        UiThreadUtil.Dispatcher?.Invoke(() =>
        {
          this.Races.AddRange(data.Races);

          this.Tabs.Add(new RaceListTabFrame
          {
            Races = this.Races,
          });
        });
      });
    }

    public void OpenRace(Race race)
    {
      this.Tabs.Add(new RaceTabFrame
      {
        Race = { Value = race },
      });
    }
  }
}
