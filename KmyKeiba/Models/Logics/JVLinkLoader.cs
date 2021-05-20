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
  class JVLinkLoader : IDisposable
  {
    private readonly CompositeDisposable disposables = new();

    public ReactiveProperty<DateTime> StartTime { get; } = new(DateTime.Today);

    public ReactiveProperty<DateTime> EndTime { get; } = new(DateTime.Today);

    public ReactiveProperty<bool> IsSetEndTime { get; } = new(false);

    public ReactiveProperty<bool> IsLoading { get; } = new(false);

    public ReactiveProperty<int> Downloaded { get; } = new(0);

    public ReactiveProperty<int> DownloadSize { get; } = new(1);

    public ReactiveProperty<int> Loaded { get; } = new(0);

    public ReactiveProperty<int> LoadSize { get; } = new(1);

    public ReactiveProperty<int> Saved { get; } = new(0);

    public ReactiveProperty<int> SaveSize { get; } = new(1);

    public ReactiveProperty<JVLinkLoadResult> LoadErrorCode { get; } = new();

    public ReactiveProperty<JVLinkReadResult> ReadErrorCode { get; } = new();

    public ReactiveProperty<bool> IsDatabaseError { get; } = new(false);

    public ReadOnlyReactiveProperty<bool> IsError { get; }

    public JVLinkLoader()
    {
      this.IsError = this.LoadErrorCode
        .Select((c) => c != JVLinkLoadResult.Succeed)
        .CombineLatest(
          this.ReadErrorCode.Select((c) => c != JVLinkReadResult.Succeed),
          (a, b) => a || b)
        .CombineLatest(
          this.IsDatabaseError,
          (a, b) => a || b)
        .ToReadOnlyReactiveProperty(false)
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
      this.LoadSize.Value = 1;
      this.Loaded.Value = 0;
      this.SaveSize.Value = 1;
      this.Saved.Value = 0;
      this.DownloadSize.Value = 1;
      this.Downloaded.Value = 0;
      this.LoadErrorCode.Value = JVLinkLoadResult.Succeed;
      this.ReadErrorCode.Value = JVLinkReadResult.Succeed;
      this.IsDatabaseError.Value = false;

      this.IsLoading.Value = true;

      await Task.Run(async () =>
      {
        try
        {
          using (var reader = link.StartRead(JVLinkDataspec.Race,
            JVLinkOpenOption.Normal,
            this.StartTime.Value,
            this.IsSetEndTime.Value ? this.EndTime.Value : null))
          {
            this.DownloadSize.Value = reader.DownloadCount;
            this.Downloaded.Value = reader.DownloadedCount;

            while (reader.DownloadCount > reader.DownloadedCount)
            {
              this.Downloaded.Value = reader.DownloadedCount;
              Task.Delay(80).Wait();
            }

            JVLinkReaderData data = new();
            var loadTask = Task.Run(() =>
            {
              data = reader.Load();
            });

            this.LoadSize.Value = reader.ReadCount;
            while (reader.ReadCount > reader.ReadedCount || !loadTask.IsCompleted)
            {
              await Task.Delay(80);
              this.Loaded.Value = reader.ReadedCount;
            }

            var saved = 0;
            this.SaveSize.Value = data.Races.Count + data.RaceHorses.Count;

            UiThreadUtil.Dispatcher?.Invoke(() =>
            {
              var timer = new DispatcherTimer();
              timer.Interval = TimeSpan.FromMilliseconds(80);
              timer.Tick += (_, _) =>
              {
                this.Saved.Value = saved;
                if (saved >= this.SaveSize.Value)
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
              }

              await db.SaveChangesAsync();
            }
          }
        }
        catch (JVLinkException<JVLinkLoadResult> ex)
        {
          this.LoadErrorCode.Value = ex.Code;
        }
        catch (JVLinkException<JVLinkReadResult> ex)
        {
          this.ReadErrorCode.Value = ex.Code;
        }
        catch
        {
          this.IsDatabaseError.Value = true;
        }
        finally
        {
          this.IsLoading.Value = false;
        }
      });
    }

    public void Dispose()
    {
      this.disposables.Dispose();
    }
  }
}
