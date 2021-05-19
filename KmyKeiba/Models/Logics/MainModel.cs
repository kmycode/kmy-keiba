using KmyKeiba.JVLink.Entities;
using KmyKeiba.JVLink.Wrappers;
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
    public ObservableCollection<Race> Races { get; } = new();

    public MainModel()
    {

    }

    public void Load()
    {
      Task.Run(() =>
      {
        var link = JVLinkObject.Local;
        var now = DateTime.Now;
        var reader = link.StartRead(JVLinkDataspec.Race,
          JVLinkOpenOption.Normal,
          new DateTime(now.Year, now.Month, now.Day - 1),
          new DateTime(now.Year, now.Month, now.Day));
        var data = reader.Load();

        UiThreadUtil.Dispatcher?.Invoke(() =>
        {
          this.Races.AddRange(data.Races);
        });
      });
    }
  }
}
