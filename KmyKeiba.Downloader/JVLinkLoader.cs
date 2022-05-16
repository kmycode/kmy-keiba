using KmyKeiba.JVLink.Entities;
using KmyKeiba.JVLink.Wrappers;
using KmyKeiba.Data.Db;
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

namespace KmyKeiba.Downloader
{
  class JVLinkLoader : IDisposable
  {
    private static int alreadyOpenCount = 0;
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

    public ReactiveProperty<int> LoadEntityCount { get; } = new(0);

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

    public async Task LoadCentralAsync(DateTime from, DateTime? to = null)
    {
      this.SetParameters(from, to);
      await this.LoadCentralAsync();
    }

    public async Task LoadLocalAsync(DateTime from, DateTime? to = null)
    {
      this.SetParameters(from, to);
      await this.LoadLocalAsync();
    }

    private void SetParameters(DateTime from, DateTime? to = null)
    {
      this.StartTime.Value = from;
      this.IsSetEndTime.Value = to != null;
      if (to != null)
      {
        this.EndTime.Value = (DateTime)to;
      }
    }

    public async Task LoadCentralAsync()
      => await this.LoadAsync(() => JVLinkObject.Central);

    public async Task LoadLocalAsync()
      => await this.LoadAsync(() => JVLinkObject.Local);

    private async Task LoadAsync(Func<JVLinkObject> linkGetter)
    {
      this.CrearErrors();
      JVLinkObject? link = null;
      try
      {
        link = linkGetter();
        if (link.IsError)
        {
          throw new Exception();
        }
      }
      catch (Exception)
      {
        this.IsLocalError.Value = true;
      }

      if (link != null)
      {
        var opt = this.StartTime.Value >= DateTime.Today.AddYears(-1) ?
          JVLinkOpenOption.Normal : JVLinkOpenOption.Setup;
        await this.LoadAsync(link, JVLinkDataspec.Race, opt);
      }
    }

    public async Task LoadAsync(JVLinkObject link, JVLinkDataspec dataspec, JVLinkOpenOption option, string? raceKey,
      DateTime? startTime, DateTime? endTime, IEnumerable<string>? loadSpecs = null)
    {
      if (startTime != null)
      {
        this.StartTime.Value = (DateTime)startTime;
      }
      if (endTime != null)
      {
        this.EndTime.Value = (DateTime)endTime;
        this.IsSetEndTime.Value = true;
      }
      else
      {
        this.IsSetEndTime.Value = false;
      }

      await this.LoadAsync(link, dataspec, option, raceKey, loadSpecs: loadSpecs);
    }

    private async Task LoadAsync(JVLinkObject link, JVLinkDataspec dataspec, JVLinkOpenOption option, string? raceKey = null, IEnumerable<string>? loadSpecs = null)
    {
      this.ResetProgresses();
      this.IsLoading.Value = true;

      await Task.Run(async () =>
      {
        try
        {
          logger.Info("Start Load JVLink");
          logger.Info($"Load Type: {link.GetType().Name}");

          if (option != JVLinkOpenOption.RealTime)
          {
            var reader = link.StartRead(dataspec,
               option,
               this.StartTime.Value,
               this.IsSetEndTime.Value ? this.EndTime.Value : null);
            await this.LoadAsync(reader, loadSpecs, true, false);
          }
          else
          {
            if (raceKey == null)
            {
              var reader = link.StartRead(dataspec,
                 JVLinkOpenOption.RealTime, DateTime.Today);
              await this.LoadAsync(reader, loadSpecs, false, true);
            }
            else
            {
              var reader = link.StartRead(dataspec,
                  JVLinkOpenOption.RealTime, raceKey);
              await this.LoadAsync(reader, loadSpecs, false, true);
            }
          }

          logger.Info("JVLink load Completed");
        }
        catch (JVLinkException<JVLinkLoadResult> ex)
        {
          this.LoadErrorCode.Value = ex.Code;
          logger.Error($"error {ex.Code}");

          if (ex.Code == JVLinkLoadResult.AlreadyOpen)
          {
            alreadyOpenCount++;
            if (alreadyOpenCount >= 10)
            {
              alreadyOpenCount = 0;
              link.Dispose();
            }
          }

          logger.Error("error", ex);

          // TODO: ネット接続切れた時は再起動しないなどの処理が必要（ダウンローダとアプリを連携してからやる）
          if (ex.Code != JVLinkLoadResult.SetupCanceled)
          {
            _ = Program.RestartProgramAsync(false);
          }
          else
          {
            Program.Exit();
          }
        }
        catch (JVLinkException<JVLinkReadResult> ex)
        {
          this.ReadErrorCode.Value = ex.Code;
          logger.Error($"error {ex.Code}");
          logger.Error("error", ex);

          _ = Program.RestartProgramAsync(false);
        }
        catch (Exception ex)
        {
          this.IsDatabaseError.Value = true;
          logger.Error("error", ex);

          _ = Program.RestartProgramAsync(false);
        }
        finally
        {
          this.IsLoading.Value = false;
        }
      });
    }

    private void ResetProgresses()
    {
      this.LoadSize.Value = 1;
      this.Loaded.Value = 0;
      this.SaveSize.Value = 1;
      this.Saved.Value = 0;
      this.DownloadSize.Value = 1;
      this.Downloaded.Value = 0;
      this.ProcessSize.Value = 1;
      this.Processed.Value = 0;
    }

    private async Task LoadAsync(IJVLinkReader reader, IEnumerable<string>? loadSpecs, bool isProcessing, bool isRealtime)
    {
      this.ResetProgresses();

      this.DownloadSize.Value = reader.DownloadCount;
      logger.Info($"Download: {reader.DownloadCount}");

      while (this.DownloadSize.Value > this.Downloaded.Value)
      {
        this.Downloaded.Value = reader.DownloadedCount;
        Task.Delay(80).Wait();
      }
      logger.Info("Download completed");

      JVLinkReaderData data = new();
      var isLoaded = false;
      var loadTask = Task.Run(async () =>
      {
        while (!isLoaded)
        {
          await Task.Delay(80);
          this.Loaded.Value = reader.ReadedCount;
          this.LoadEntityCount.Value = reader.ReadedEntityCount;
        }
      });

      // while (!loadTask.IsCompleted && loadException == null)
      // {
      // }

      this.LoadSize.Value = reader.ReadCount;
      logger.Info("Load start");
      try
      {
        data = reader.Load(loadSpecs);
      }
      catch (Exception ex)
      {
        throw new Exception("Load error", ex);
      }
      isLoaded = true;

      // readerのDisposeが完了しない場合がある
      var isDisposed = false;
      var isDone = false;
      _ = Task.Run(async () =>
      {
        var loadStartTime = DateTime.Now;

        await LoadAfterAsync(data, isProcessing, isRealtime);
        isDone = true;

        if (!isDisposed)
        {
          var d = (DateTime.Now - loadStartTime).TotalSeconds;
          if (d < 0) d = 0;
          await Task.Delay(TimeSpan.FromSeconds(System.Math.Max(30.0 - d, 0.1)));

          if (!isDisposed)
          {
            await Program.RestartProgramAsync(true);
          }
        }
      });

      // EntityFrameworkメソッドの呼び出しでスレッドが変わることがあるので、ここで破棄する
      if (reader.Type == JVLinkObjectType.Local)
      {
        GC.Collect();
      }
      reader.Dispose();
      isDisposed = true;

      // ロード処理を待機。終わるまで制御を戻さない
      while (!isDone)
      {
        await Task.Delay(100);
      }
    }

    private async Task LoadAfterAsync(JVLinkReaderData data, bool isProcessing, bool isRealtime)
    {
      var saved = 0;
      this.SaveSize.Value = data.Races.Count + data.RaceHorses.Count + data.ExactaOdds.Count
        + data.FrameNumberOdds.Count + data.QuinellaOdds.Count + data.QuinellaPlaceOdds.Count +
         data.TrifectaOdds.Count + data.TrioOdds.Count + data.BornHorses.Count +
        data.Refunds.Count + data.Trainings.Count + data.WoodtipTrainings.Count + data.Horses.Count + data.HorseBloods.Count;
      logger.Info($"Save size: {this.SaveSize.Value}");

      var timer = new ReactiveTimer(TimeSpan.FromMilliseconds(80));
      timer.Subscribe((t) =>
      {
        this.Saved.Value = saved;
        if (saved >= this.SaveSize.Value)
        {
          timer.Dispose();
        }
      });
      timer.Start();

      using var db = new MyContext();

      async Task SaveDicAsync<E, D, I, KEY>(Dictionary<KEY, E> entities, DbSet<D> dataSet, Func<E, I> entityId, Func<D, I> dataId, Func<IEnumerable<I>, Expression<Func<D, bool>>> dataIdSelector)
        where E : EntityBase where D : DataBase<E>, new() where I : IComparable<I>, IEquatable<I> where KEY : IComparable
      {
        await SaveAsync(entities.Select(e => e.Value).ToList(), dataSet, entityId, dataId, dataIdSelector);
      }

      async Task SaveAsync<E, D, I>(IEnumerable<E> entities, DbSet<D> dataSet, Func<E, I> entityId, Func<D, I> dataId, Func<IEnumerable<I>, Expression<Func<D, bool>>> dataIdSelector)
        where E : IEntityBase where D : DataBase<E>, new() where I : IComparable<I>, IEquatable<I>
      {
        var position = entities;

        while (position.Any())
        {
          var chunk = position.Take(10000);
          await SaveAsyncPrivate(chunk, dataSet, entityId, dataId, dataIdSelector);

          position = position.Skip(10000);
        }
      }

      async Task SaveAsyncPrivate<E, D, I>(IEnumerable<E> entities, DbSet<D> dataSet, Func<E, I> entityId, Func<D, I> dataId, Func<IEnumerable<I>, Expression<Func<D, bool>>> dataIdSelector)
        where E : IEntityBase where D : DataBase<E>, new() where I : IComparable<I>, IEquatable<I>
      {
        var copyed = entities.ToList();

        var changed = 0;

        var ids = entities.Select(entityId).Distinct().ToList();
        var dataItems = await dataSet!
          .Where(dataIdSelector(ids))
          .ToArrayAsync();
        foreach (var item in dataItems
          .Join(entities, (d) => dataId(d), (e) => entityId(e), (d, e) => new { Data = d, Entity = e, })
          .OrderBy((i) => (short)i.Entity.DataStatus))
        {
          item.Data.SetEntity(item.Entity);
          copyed.Remove(item.Entity);
          changed++;

          if (changed == 1000)
          {
            await db.SaveChangesAsync();
            saved += changed;
            changed = 0;
          }
        }
        await db.SaveChangesAsync();
        saved += changed;

        var newItems = copyed
          .Where((e) => !dataItems.Any((d) => dataId(d)!.Equals(entityId(e))))
          .Select((item) =>
          {
            var obj = new D();
            obj.SetEntity(item);
            return obj;
          });

        // 大量のデータを分割して保存する
        var position = newItems;
        while (position.Any())
        {
          var chunk = position.Take(1000);
          await dataSet.AddRangeAsync(chunk);
          await db.SaveChangesAsync();
          saved += chunk.Count();

          position = position.Skip(1000);
        }
        // saved += items.Count();
      }

      await db.BeginTransactionAsync();

      await SaveDicAsync(data.RaceHorses,
        db.RaceHorses!,
        (e) => e.Name + e.RaceKey,
        (d) => d.Name + d.RaceKey,
        (list) => e => list.Contains(e.Name + e.RaceKey));
      await db.CommitAsync();

      await SaveDicAsync(data.Races,
        db.Races!,
        (e) => e.Key,
        (d) => d.Key,
        (list) => e => list.Contains(e.Key));
      await SaveDicAsync(data.Horses,
        db.Horses!,
        (e) => e.Code,
        (d) => d.Code,
        (list) => e => list.Contains(e.Code));
      await SaveDicAsync(data.HorseBloods,
        db.HorseBloods!,
        (e) => e.Key,
        (d) => d.Key,
        (list) => e => list.Contains(e.Key));
      await db.CommitAsync();
      await SaveDicAsync(data.BornHorses,
        db.BornHorses!,
        (e) => e.Code,
        (d) => d.Code,
        (list) => e => list.Contains(e.Code));
      await db.CommitAsync();

      await SaveDicAsync(data.FrameNumberOdds,
        db.FrameNumberOdds!,
        (e) => e.RaceKey,
        (d) => d.RaceKey,
        (list) => e => list.Contains(e.RaceKey));
      await SaveDicAsync(data.QuinellaPlaceOdds,
        db.QuinellaPlaceOdds!,
        (e) => e.RaceKey,
        (d) => d.RaceKey,
        (list) => e => list.Contains(e.RaceKey));
      await SaveDicAsync(data.QuinellaOdds,
        db.QuinellaOdds!,
        (e) => e.RaceKey,
        (d) => d.RaceKey,
        (list) => e => list.Contains(e.RaceKey));
      await SaveDicAsync(data.ExactaOdds,
        db.ExactaOdds!,
        (e) => e.RaceKey,
        (d) => d.RaceKey,
        (list) => e => list.Contains(e.RaceKey));
      await SaveDicAsync(data.TrioOdds,
        db.TrioOdds!,
        (e) => e.RaceKey,
        (d) => d.RaceKey,
        (list) => e => list.Contains(e.RaceKey));
      await SaveDicAsync(data.TrifectaOdds,
        db.TrifectaOdds!,
        (e) => e.RaceKey,
        (d) => d.RaceKey,
        (list) => e => list.Contains(e.RaceKey));
      await db.CommitAsync();

      await SaveDicAsync(data.Refunds,
        db.Refunds!,
        (e) => e.RaceKey,
        (d) => d.RaceKey,
        (list) => e => list.Contains(e.RaceKey));

      await SaveAsync(data.SingleAndDoubleWinOdds.Where((o) => o.Value.Time != default).Select((o) => o.Value),
        db.SingleOddsTimelines!,
        (e) => e.RaceKey + e.Time.Month + "_" + e.Time.Day + "_" + e.Time.Hour + "_" + e.Time.Minute,
        (d) => d.RaceKey + d.Time.Month + "_" + d.Time.Day + "_" + d.Time.Hour + "_" + d.Time.Minute,
        (list) => e => list.Contains(e.RaceKey + e.Time.Month + "_" + e.Time.Day + "_" + e.Time.Hour + "_" + e.Time.Minute));
      await SaveDicAsync(data.Trainings,
        db.Trainings!,
        (e) => e.HorseKey + e.StartTime,
        (d) => d.HorseKey + d.StartTime,
        (list) => e => list.Contains(e.HorseKey + e.StartTime));
      await SaveDicAsync(data.WoodtipTrainings,
        db.WoodtipTrainings!,
        (e) => e.HorseKey + e.StartTime,
        (d) => d.HorseKey + d.StartTime,
        (list) => e => list.Contains(e.HorseKey + e.StartTime));
      await db.CommitAsync();

      // 保存後のデータに他のデータを追加する
      this.ProcessSize.Value = 0;
      this.Processed.Value = 0;
      this.ProcessSize.Value += data.SingleAndDoubleWinOdds.Count;

      // 単勝オッズを設定する
      {
        var oddsRaceKeys = data.SingleAndDoubleWinOdds.Select((o) => o.Value.RaceKey).ToArray();
        var oddsRaceHorses = await db.RaceHorses!
          .Where((r) => oddsRaceKeys.Contains(r.RaceKey))
          .ToArrayAsync();

        foreach (var odds in data.SingleAndDoubleWinOdds)
        {
          var horses = oddsRaceHorses
            .Where((h) => h.RaceKey == odds.Value.RaceKey);
          foreach (var horse in horses)
          {
            var o = odds.Value.Odds.FirstOrDefault((oo) => oo.HorseNumber == horse.Number);
            if (o.HorseNumber != default)
            {
              horse.Odds = (short)o.Odds;
              horse.Popular = o.Popular;
              horse.PlaceOddsMax = (short)o.PlaceOddsMax;
              horse.PlaceOddsMin = (short)o.PlaceOddsMin;
            }
          }

          this.Processed.Value++;
        }

        await db.SaveChangesAsync();
      }

      if (isRealtime)
      {
        this.ProcessSize.Value += data.HorseWeights.Count;
        this.ProcessSize.Value += data.CourseWeatherConditions.Count;
        this.ProcessSize.Value += data.HorseAbnormalities.Count;
        this.ProcessSize.Value += data.HorseRiderChanges.Count;

        // 馬の体重を設定する
        {
          foreach (var weight in data.HorseWeights)
          {
            var horses = await db.RaceHorses!
              .Where((h) => h.RaceKey == weight.RaceKey)
              .ToArrayAsync();
            foreach (var info in weight.Infos.Join(horses, (i) => i.HorseNumber, (h) => h.Number, (i, h) => new { Info = i, Horse = h, }))
            {
              info.Horse.Weight = info.Info.Weight;
              info.Horse.WeightDiff = info.Info.WeightDiff;
            }

            this.Processed.Value++;
          }

          await db.SaveChangesAsync();
        }

        // 天候、馬場
        {
          foreach (var weather in data.CourseWeatherConditions)
          {
            var races = await db.Races!
              .Where((r) => r.Key.StartsWith(weather.RaceKeyWithoutRaceNum))
              .ToArrayAsync();
            foreach (var race in races)
            {
              if (weather.Weather != RaceCourseWeather.Unknown)
              {
                race.TrackWeather = weather.Weather;
              }
              if (race.TrackGround == TrackGround.Turf && weather.TurfCondition != RaceCourseCondition.Unknown)
              {
                race.TrackCondition = weather.TurfCondition;
              }
              else if ((race.TrackGround == TrackGround.Dirt || race.TrackGround == TrackGround.TurfToDirt || race.TrackGround == TrackGround.Sand) &&
                weather.DirtCondition != RaceCourseCondition.Unknown)
              {
                race.TrackCondition = weather.DirtCondition;
              }
            }

            this.Processed.Value++;
          }

          await db.SaveChangesAsync();
        }

        // 馬の状態
        {
          foreach (var ab in data.HorseAbnormalities)
          {
            var horse = await db.RaceHorses!
              .FirstOrDefaultAsync((h) => h.RaceKey == ab.RaceKey && h.Number == ab.HorseNumber);
            if (horse != null)
            {
              horse.AbnormalResult = ab.AbnormalResult;
            }

            this.Processed.Value++;
          }

          await db.SaveChangesAsync();
        }

        // 騎手
        {
          foreach (var ab in data.HorseRiderChanges)
          {
            var horse = await db.RaceHorses!
              .FirstOrDefaultAsync((h) => h.RaceKey == ab.RaceKey && h.Number == ab.HorseNumber);
            if (horse != null)
            {
              horse.RiderCode = ab.RiderCode;
              horse.RiderName = ab.RiderName;
              horse.RiderWeight = ab.RiderWeight;
            }

            this.Processed.Value++;
          }

          await db.SaveChangesAsync();
        }
      }

      // 後処理　元データにはないデータを追加する

      if (isProcessing)
      {
        /*
        // それぞれの馬に、第３ハロンタイムの順位をつける（LINQでやると時間がかかる）
        IEnumerable<string> ids = db.RaceHorses!
          .Where((h) => h.AfterThirdHalongTimeOrder == 0 && h.AfterThirdHalongTime > TimeSpan.Zero)
          .Select((r) => r.RaceKey)
          .Distinct();
        this.ProcessSize.Value += (int)Math.Ceiling(ids.Count() / 64.0f);
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
        */
      }
    }

    public void Dispose()
    {
      this.disposables.Dispose();
    }
  }
}
