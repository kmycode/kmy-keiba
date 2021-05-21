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
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace KmyKeiba.Models.Logics
{
  class JVLinkLoader : IDisposable
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

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
          logger.Info("Start Load JVLink");
          logger.Info($"Load Type: {link.GetType().Name}");

          using (var reader = link.StartRead(JVLinkDataspec.Race,
            JVLinkOpenOption.Normal,
            this.StartTime.Value,
            this.IsSetEndTime.Value ? this.EndTime.Value.AddDays(1) : null))
          {
            await this.LoadAsync(reader);
          }

          if (!this.IsSetEndTime.Value || this.EndTime.Value >= DateTime.Today)
          {
            using (var reader = link.StartRead(JVLinkDataspec.RB15,
              JVLinkOpenOption.RealTime,
              DateTime.Today))
            {
              await this.LoadAsync(reader);
            }
          }

          logger.Info("JVLink load Completed");
        }
        catch (JVLinkException<JVLinkLoadResult> ex)
        {
          this.LoadErrorCode.Value = ex.Code;
          logger.Error($"error {ex.Code}");
          logger.Error("error", ex);
        }
        catch (JVLinkException<JVLinkReadResult> ex)
        {
          this.ReadErrorCode.Value = ex.Code;
          logger.Error($"error {ex.Code}");
          logger.Error("error", ex);
        }
        catch (Exception ex)
        {
          this.IsDatabaseError.Value = true;
          logger.Error("error", ex);
        }
        finally
        {
          this.IsLoading.Value = false;
        }
      });
    }

    private async Task LoadAsync(IJVLinkReader reader)
    {
      this.DownloadSize.Value = reader.DownloadCount;
      logger.Info($"Download: {reader.DownloadCount}");

      while (this.DownloadSize.Value > this.Downloaded.Value)
      {
        this.Downloaded.Value = reader.DownloadedCount;
        Task.Delay(80).Wait();
      }
      logger.Info("Download completed");

      JVLinkReaderData data = new();
      Exception? loadException = null;
      var loadTask = Task.Run(() =>
      {
        logger.Info("Load start");
        try
        {
          data = reader.Load();
        }
        catch (Exception ex)
        {
          loadException = ex;
        }
      });

      this.LoadSize.Value = reader.ReadCount;
      while (!loadTask.IsCompleted && loadException == null)
      {
        await Task.Delay(80);
        this.Loaded.Value = reader.ReadedCount;
      }

      if (loadException != null)
      {
        throw new Exception("Load error", loadException);
      }

      var saved = 0;
      this.SaveSize.Value = data.Races.Count + data.RaceHorses.Count;
      logger.Info($"Save size: {this.SaveSize.Value}");

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
        async Task SaveAsync<E, D, I>(List<E> entities, DbSet<D> dataSet, Func<E, I> entityId, Func<D, I> dataId, Func<IEnumerable<I>, Expression<Func<D, bool>>> dataIdSelector) where E : EntityBase where D : DataBase<E>, new()
        {
          var ids = entities.Select(entityId).ToList();
          var dataItems = await dataSet!
            .Where(dataIdSelector(ids))
            .ToArrayAsync();
          foreach (var item in dataItems
            .Join(entities, (d) => dataId(d), (e) => entityId(e), (d, e) => new { Data = d, Entity = e, })
            .Where((i) => i.Entity.LastModified >= i.Data.LastModified))
          {
            item.Data.SetEntity(item.Entity);
            saved++;
          }
          foreach (var item in entities.Where((e) => !dataItems.Any((d) => dataId(d)!.Equals(entityId(e)))))
          {
            var obj = new D();
            obj.SetEntity(item);
            await dataSet.AddAsync(obj);
            saved++;
          }
        }

        {
          /*
          var ids = data.Races.Select((r) => r.Key).ToList();
          var dataItems = await db.Races!
            .Where((r) => ids.Contains(r.Key))
            .ToArrayAsync();
          foreach (var item in dataItems
            .Join(data.Races, (d) => d.Key, (e) => e.Key, (d, e) => new { Data = d, Entity = e, })
            .Where((i) => i.Entity.LastModified >= i.Data.LastModified))
          {
            item.Data.SetEntity(item.Entity);
            saved++;
          }
          foreach (var item in data.Races.Where((e) => !dataItems.Any((d) => d.Key == e.Key)))
          {
            var obj = new RaceData();
            obj.SetEntity(item);
            await db.Races!.AddAsync(obj);
            saved++;
          }
          */
          await SaveAsync(data.Races,
            db.Races!,
            (e) => e.Key,
            (d) => d.Key,
            (list) => e => list.Contains(e.Key));
        }
        {
          /*
          var ids = data.RaceHorses.Select((r) => r.Name + r.RaceKey).ToList();
          var dataItems = await db.RaceHorses!
            .Where((r) => ids.Contains(r.Name + r.RaceKey))
            .ToArrayAsync();
          foreach (var item in dataItems
            .Join(data.RaceHorses, (d) => d.Name + d.RaceKey, (e) => e.Name + e.RaceKey, (d, e) => new { Data = d, Entity = e, })
            .Where((i) => i.Entity.LastModified >= i.Data.LastModified))
          {
            item.Data.SetEntity(item.Entity);
            saved++;
          }
          foreach (var item in data.RaceHorses.Where((e) => !dataItems.Any((d) => d.Name == e.Name && d.RaceKey == e.RaceKey)))
          {
            var obj = new RaceHorseDataObject(item);
            await db.RaceHorses!.AddAsync(obj.Data);
            saved++;
          }
          */
          await SaveAsync(data.RaceHorses,
            db.RaceHorses!,
            (e) => e.Name + e.RaceKey,
            (d) => d.Name + d.RaceKey,
            (list) => e => list.Contains(e.Name + e.RaceKey));
        }

        await db.SaveChangesAsync();
      }
    }

    public void Dispose()
    {
      this.disposables.Dispose();
    }
  }
}
