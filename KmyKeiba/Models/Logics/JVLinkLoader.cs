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

    public ReactiveProperty<int> Processed { get; } = new(0);

    public ReactiveProperty<int> ProcessSize { get; } = new(1);

    public ReactiveProperty<JVLinkLoadResult> LoadErrorCode { get; } = new();

    public ReactiveProperty<JVLinkReadResult> ReadErrorCode { get; } = new();

    public ReactiveProperty<bool> IsDatabaseError { get; } = new(false);

    public ReadOnlyReactiveProperty<bool> IsError { get; }

    public ReactiveProperty<bool> IsCentralError { get; } = new();

    public ReactiveProperty<bool> IsLocalError { get; } = new();

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
        .CombineLatest(this.IsCentralError, (a, b) => a || b)
        .CombineLatest(this.IsLocalError, (a, b) => a || b)
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

    private void CrearErrors()
    {
      this.IsCentralError.Value = this.IsLocalError.Value = false;
      this.LoadErrorCode.Value = JVLinkLoadResult.Succeed;
      this.ReadErrorCode.Value = JVLinkReadResult.Succeed;
      this.IsDatabaseError.Value = false;
    }

    public async Task LoadCentralAsync()
    {
      this.CrearErrors();
      JVLinkObject? link = null;
      try
      {
        link = JVLinkObject.Central;
      }
      catch (Exception)
      {
        this.IsCentralError.Value = true;
      }

      if (link != null)
      {
        await this.LoadAsync(link);
      }
    }

    public async Task LoadLocalAsync()
    {
      this.CrearErrors();
      JVLinkObject? link = null;
      try
      {
        link = JVLinkObject.Local;
      }
      catch (Exception)
      {
        this.IsLocalError.Value = true;
      }

      if (link != null)
      {
        await this.LoadAsync(link);
      }
    }

    private async Task LoadAsync(JVLinkObject link)
    {
      this.LoadSize.Value = 1;
      this.Loaded.Value = 0;
      this.SaveSize.Value = 1;
      this.Saved.Value = 0;
      this.DownloadSize.Value = 1;
      this.Downloaded.Value = 0;
      this.ProcessSize.Value = 1;
      this.Processed.Value = 0;

      this.IsLoading.Value = true;

      await Task.Run(async () =>
      {
        try
        {
          logger.Info("Start Load JVLink");
          logger.Info($"Load Type: {link.GetType().Name}");

          var isOpenRealTime = !this.IsSetEndTime.Value || this.EndTime.Value >= DateTime.Today;

          using (var reader = link.StartRead(JVLinkDataspec.Race | JVLinkDataspec.Diff,
            JVLinkOpenOption.Normal,
            this.StartTime.Value,
            this.IsSetEndTime.Value ? this.EndTime.Value.AddDays(1) : null))
          {
            await this.LoadAsync(reader, !isOpenRealTime);
          }

          if (isOpenRealTime)
          {
            using (var reader = link.StartRead(JVLinkDataspec.RB15,
              JVLinkOpenOption.RealTime,
              DateTime.Today))
            {
              await this.LoadAsync(reader, true);
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

    private async Task LoadAsync(IJVLinkReader reader, bool isProcessing)
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
            .OrderBy((i) => i.Entity.DataStatus))
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

        await SaveAsync(data.Races,
          db.Races!,
          (e) => e.Key,
          (d) => d.Key,
          (list) => e => list.Contains(e.Key));
        await SaveAsync(data.RaceHorses,
          db.RaceHorses!,
          (e) => e.Name + e.RaceKey,
          (d) => d.Name + d.RaceKey,
          (list) => e => list.Contains(e.Name + e.RaceKey));

        await db.SaveChangesAsync();

        // 後処理　元データにはないデータを追加する

        if (isProcessing)
        {
          // それぞれの馬に、第３ハロンタイムの順位をつける（LINQでやると時間がかかる）
          IEnumerable<string> ids = db.RaceHorses!
            .Where((h) => h.AfterThirdHalongTimeOrder == 0 && h.AfterThirdHalongTime != default)
            .Select((r) => r.RaceKey)
            .Distinct()
            .ToArray();
          this.ProcessSize.Value = (int)Math.Ceiling(ids.Count() / 64.0f);
          this.Processed.Value = 0;
          while (ids.Any())
          {
            var arr = string.Join("','", ids.Take(64));
            await db.Database.ExecuteSqlRawAsync($@"
UPDATE racehorses, (SELECT racekey,`name`,afterthirdhalongtime,ROW_NUMBER() OVER(PARTITION BY racekey ORDER BY afterthirdhalongtime ASC) halongOrder
FROM racehorses WHERE racekey IN ('{arr}') AND afterthirdhalongtime <> '00:00:00') AS buf
SET racehorses.afterthirdhalongtimeorder=buf.halongOrder
WHERE racehorses.racekey IN ('{arr}') AND racehorses.RaceKey=buf.racekey AND racehorses.`Name`=buf.`name`");

            ids = ids.Skip(64);
            this.Processed.Value++;
          }
        }
      }
    }

    public void Dispose()
    {
      this.disposables.Dispose();
    }
  }
}
