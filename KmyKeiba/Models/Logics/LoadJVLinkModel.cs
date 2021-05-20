using KmyKeiba.JVLink.Entities;
using KmyKeiba.JVLink.Wrappers;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.DataObjects;
using KmyKeiba.Models.Threading;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace KmyKeiba.Models.Logics
{
  class LoadJVLinkModel : IDisposable
  {
    private readonly CompositeDisposable disposables = new();

    public ReactiveProperty<DateTime> StartTime { get; } = new(DateTime.Today);

    public ReactiveProperty<DateTime> EndTime { get; } = new(DateTime.Today);

    public ReactiveProperty<bool> IsSetEndTime { get; } = new(false);

    public ReactiveProperty<bool> IsLoading { get; } = new(false);

    public ReactiveProperty<int> Downloaded { get; } = new(0);

    public ReactiveProperty<int> DownloadSize { get; } = new(0);

    public ReactiveProperty<int> Saved { get; } = new(0);

    public ReactiveProperty<int> SaveSize { get; } = new(0);

    public ReadOnlyReactiveProperty<double> DownloadProgress { get; }

    public ReadOnlyReactiveProperty<double> SaveProgress { get; }

    public LoadJVLinkModel()
    {
      this.DownloadProgress = this.Downloaded
        .Merge(this.DownloadSize)
        .Select((_) => (double)this.Downloaded.Value / Math.Max(1, this.DownloadSize.Value))
        .ToReadOnlyReactiveProperty(0.0)
        .AddTo(this.disposables);
      this.SaveProgress = this.Saved
        .Merge(this.SaveSize)
        .Select((_) => (double)this.Saved.Value / Math.Max(1, this.SaveSize.Value))
        .ToReadOnlyReactiveProperty(0.0)
        .AddTo(this.disposables);
    }

    public void OpenCentralConfig()
    {
      JVLinkObject.Central.OpenConfigWindow();
    }

    public void OpenLocalConfig()
    {
      JVLinkObject.Local.OpenConfigWindow();
    }

    public async Task LoadCentralAsync() => await this.LoadAsync(JVLinkObject.Central);

    public async Task LoadLocalAsync() => await this.LoadAsync(JVLinkObject.Local);

    private async Task LoadAsync(JVLinkObject link)
    {
      this.SaveSize.Value = 0;
      this.Saved.Value = 0;
      this.DownloadSize.Value = 0;
      this.Downloaded.Value = 0;

      this.IsLoading.Value = true;

      await Task.Run(async () =>
      {
        var reader = link.StartRead(JVLinkDataspec.Race,
          JVLinkOpenOption.Normal,
          this.StartTime.Value,
          this.IsSetEndTime.Value ? this.EndTime.Value : null);

        UiThreadUtil.Dispatcher?.Invoke(() =>
        {
          this.DownloadSize.Value = reader.DownloadCount;
          this.Downloaded.Value = reader.DownloadedCount;
        });
        while (reader.DownloadCount > reader.DownloadedCount)
        {
          UiThreadUtil.Dispatcher?.Invoke(() =>
          {
            this.Downloaded.Value = reader.DownloadedCount;
          });
          Task.Delay(100).Wait();
        }

        var data = reader.Load();
        var saved = 0;
        UiThreadUtil.Dispatcher?.Invoke(() =>
        {
          this.SaveSize.Value = data.Races.Count + data.RaceHorses.Count;
          var timer = new DispatcherTimer();
          timer.Interval = TimeSpan.FromSeconds(1);
          timer.Tick += (_, _) =>
          {
            this.Saved.Value = saved;
            if (saved == this.SaveSize.Value)
            {
              timer.Stop();
            }
          };
          timer.Start();
        });

        using (var db = new MyContext())
        {
          {
            var ids = data.Races.Select((r) => r.Key).ToList();
            var dataItems = await db.Races!
              .Where((r) => ids.Contains(r.Key))
              .ToArrayAsync();
            foreach (var item in dataItems.Join(data.Races, (d) => d.Key, (e) => e.Key, (d, e) => new { Data = d, Entity = e, }))
            {
              var obj = new RaceDataObject(item.Data);
              obj.SetEntity(item.Entity);
              saved++;
            }
            foreach (var item in data.Races.Where((e) => !dataItems.Any((d) => d.Key == e.Key)))
            {
              var obj = new RaceDataObject(item);
              await db.Races!.AddAsync(obj.Data);
            }
            saved = data.Races.Count;
          }
          {
            var ids = data.RaceHorses.Select((r) => r.Name).ToList();
            var dataItems = await db.RaceHorses!
              .Where((r) => ids.Contains(r.Name))
              .ToArrayAsync();
            foreach (var item in dataItems.Join(data.RaceHorses, (d) => d.Name, (e) => e.Name, (d, e) => new { Data = d, Entity = e, }))
            {
              var obj = new RaceHorseDataObject(item.Data);
              obj.SetEntity(item.Entity);
              saved++;
            }
            foreach (var item in data.RaceHorses.Where((e) => !dataItems.Any((d) => d.Name == e.Name)))
            {
              var obj = new RaceHorseDataObject(item);
              await db.RaceHorses!.AddAsync(obj.Data);
            }
            saved = data.Races.Count + data.RaceHorses.Count;
          }

          await db.SaveChangesAsync();
        }
      });

      this.IsLoading.Value = false;
    }

    public void Dispose()
    {
      this.disposables.Dispose();
    }
  }
}
