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
using Microsoft.Data.Sqlite;

namespace KmyKeiba.Downloader
{
  class JVLinkLoader : IDisposable
  {
    private static int alreadyOpenCount = 0;
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    private readonly CompositeDisposable disposables = new();

    public LoadProcessing Process { get; set; }

    public ReactiveProperty<DateTime> StartTime { get; } = new(DateTime.Today);

    public ReactiveProperty<DateTime> EndTime { get; } = new(DateTime.Today);

    public ReactiveProperty<bool> IsSetEndTime { get; } = new(false);

    public ReactiveProperty<int> Downloaded { get; } = new(0);

    public ReactiveProperty<int> DownloadSize { get; } = new(1);

    public ReactiveProperty<int> Loaded { get; } = new(0);

    public ReactiveProperty<int> LoadSize { get; } = new(1);

    public ReactiveProperty<int> LoadEntityCount { get; } = new(0);

    public ReactiveProperty<int> Saved { get; } = new(0);

    public ReactiveProperty<int> SaveSize { get; } = new(1);

    public ReactiveProperty<int> Processed { get; } = new(0);

    public ReactiveProperty<int> ProcessSize { get; } = new(1);

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

      logger.Info($"ロード開始 [{this.StartTime.Value} - {this.EndTime.Value}]");
      logger.Info($"終了日時の指定状態：{this.IsSetEndTime.Value}");

      await this.LoadAsync(link, dataspec, option, raceKey, loadSpecs: loadSpecs);
    }

    private async Task LoadAsync(JVLinkObject link, JVLinkDataspec dataspec, JVLinkOpenOption option, string? raceKey = null, IEnumerable<string>? loadSpecs = null)
    {
      this.ResetProgresses();

      logger.Info($"ロードを開始します {link.Type}");
      logger.Info($"dataspec: {dataspec}");
      logger.Info($"option: {option}");
      logger.Info($"racekey: {raceKey}");
      if (loadSpecs != null)
      {
        logger.Info($"specs: {string.Join(',', loadSpecs)}");
      }
      else
      {
        logger.Info("specs: 制限なし");
      }

      await Task.Run(async () =>
      {
        try
        {
          this.Process = LoadProcessing.Opening;

          IJVLinkReader StartReadWithTimeout(Func<IJVLinkReader> reader)
          {
            var isSucceed = false;
            var start = DateTime.Now;

            var waitSeconds = link.Type == JVLinkObjectType.Local ?
              (DateTime.Now - this.StartTime.Value).TotalDays / 365 * 60 + 60 :  // 1年あたり60秒
              60;

            Task.Run(async () =>
            {
              while (!isSucceed)
              {
                await Task.Delay(1000);

                if (DateTime.Now - start > TimeSpan.FromSeconds(waitSeconds))
                {
                  logger.Warn("接続オープンに失敗したので強制終了します");
                  await Program.RestartProgramAsync(false);
                }
                Program.CheckShutdown();
              }
            });

            var result = reader();
            isSucceed = true;
            return result;
          }

          if (option != JVLinkOpenOption.RealTime)
          {
            logger.Info("接続オープン：セットアップまたは通常データ");
            var reader = StartReadWithTimeout(() => link.StartRead(dataspec,
               option,
               this.StartTime.Value,
               this.IsSetEndTime.Value ? this.EndTime.Value : null));
            await this.LoadAsync(reader, loadSpecs, true, false);
          }
          else
          {
            if (raceKey == null)
            {
              logger.Info("接続オープン：日付");
              var reader = StartReadWithTimeout(() => link.StartRead(dataspec,
                 JVLinkOpenOption.RealTime, this.StartTime.Value.Date));
              await this.LoadAsync(reader, loadSpecs, false, true);
            }
            else
            {
              logger.Info("接続オープン：特定レース");
              var reader = StartReadWithTimeout(() => link.StartRead(dataspec,
                  JVLinkOpenOption.RealTime, raceKey));
              await this.LoadAsync(reader, loadSpecs, false, true);
            }
          }

          logger.Info("ロードが完了しました");
        }
        catch (JVLinkException<JVLinkLoadResult> ex)
        {
          logger.Error($"ロードでエラーが発生 {ex.Code}", ex);

          if (ex.Code == JVLinkLoadResult.AlreadyOpen)
          {
            alreadyOpenCount++;
            if (alreadyOpenCount >= 10)
            {
              alreadyOpenCount = 0;
              link.Dispose();
            }
          }
          if (ex.Code == JVLinkLoadResult.SetupCanceled)
          {
            Program.Shutdown(DownloaderError.SetupDialogCanceled);
          }
          else if (ex.Code == JVLinkLoadResult.LicenceKeyExpired)
          {
            Program.Shutdown(DownloaderError.LicenceKeyExpired);
          }
          else if (ex.Code == JVLinkLoadResult.LicenceKeyNotSet)
          {
            Program.Shutdown(DownloaderError.LicenceKeyNotSet);
          }
          else if (ex.Code == JVLinkLoadResult.InMaintance)
          {
            Program.Shutdown(DownloaderError.InMaintance);
          }
          else if (ex.Code == JVLinkLoadResult.InvalidDataspec || ex.Code == JVLinkLoadResult.InvalidDatespecAndOption ||
            ex.Code == JVLinkLoadResult.InvalidFromTime || ex.Code == JVLinkLoadResult.InvalidKey ||
            ex.Code == JVLinkLoadResult.InvalidOption || ex.Code == JVLinkLoadResult.InvalidRegistry ||
            ex.Code == JVLinkLoadResult.InvalidServerApplication)
          {
            Program.Shutdown(DownloaderError.ApplicationError);
          }
          else
          {
            _ = Program.RestartProgramAsync(false);
          }
        }
        catch (JVLinkException<JVLinkReadResult> ex)
        {
          logger.Error($"データ読み込みでエラーが発生 {ex.Code}", ex);

          _ = Program.RestartProgramAsync(false);
        }
        catch (Exception ex)
        {
          logger.Error("不明なエラーが発生", ex);

          _ = Program.RestartProgramAsync(false);
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
      logger.Info($"必要なダウンロード数: {reader.DownloadCount}");
      this.Process = LoadProcessing.Downloading;

      var waitCount = 0;
      var lastUpdatedDownloadCount = 0;
      var stayCount = 0;

      // ダウンロードはリンク上で非同期で行われるため、待機処理をここに入れる
      while (this.DownloadSize.Value > this.Downloaded.Value)
      {
        this.Downloaded.Value = reader.DownloadedCount;
        Task.Delay(80).Wait();

        waitCount += 80;
        if (waitCount > 10_000)
        {
          Program.CheckShutdown();
          waitCount = 0;
        }
        if (reader.DownloadedCount < 0)
        {
          var code = (JVLinkLoadResult)reader.DownloadedCount;
          logger.Warn($"ダウンロード中にエラーが発生: {code}");

          // 地方競馬では、きちんとネットにつながってるはずなのにこのようなエラーが出ることがある様子
          if (code == JVLinkLoadResult.DownloadFailed)
          {
            throw JVLinkException.GetError((JVLinkLoadResult)reader.DownloadedCount);
          }
          Program.RestartProgramAsync(false).Wait();
        }
        else
        {
          // ダウンロード数が増えなかったときのタイムアウト
          // 地方競馬ではダウンロード中にネット接続が切れてもNVStatusでエラーを返さないことがあるため、その対応。ちゃんとデバッグして
          if (reader.DownloadedCount != lastUpdatedDownloadCount)
          {
            lastUpdatedDownloadCount = reader.DownloadedCount;
            stayCount = 0;
          }
          else
          {
            stayCount += 80;
            if (stayCount >= 60_000)
            {
              logger.Warn($"ダウンロード数が {reader.DownloadedCount} から増えないのでタイムアウトします");
              Program.RestartProgramAsync(false).Wait();
            }
          }
        }
      }
      logger.Info("ダウンロードが完了しました");

      waitCount = 0;
      var isDisposed = false;

      JVLinkReaderData data = new();
      var isLoaded = false;
      var loadTimeout = 0;
      var loadTask = Task.Run(async () =>
      {
        while (!isLoaded)
        {
          await Task.Delay(80);

          if (this.Loaded.Value != reader.ReadedCount)
          {
            this.Loaded.Value = reader.ReadedCount;
            this.LoadEntityCount.Value = reader.ReadedEntityCount;
            loadTimeout = 0;
          }
          else
          {
            loadTimeout += 80;
            if (loadTimeout >= 100_000)
            {
              await Program.RestartProgramAsync(false);
              loadTimeout = 0;
            }
          }

          waitCount += 80;
          if (waitCount > 10_000)
          {
            Program.CheckShutdown();
            waitCount = 0;
          }
        }
      });

      // while (!loadTask.IsCompleted && loadException == null)
      // {
      // }

      this.LoadSize.Value = reader.ReadCount;
      logger.Info($"データのロードを開始します　ロード数: {reader.ReadCount}");
      this.Process = LoadProcessing.Loading;
      try
      {
        data = reader.Load(loadSpecs);
      }
      catch (Exception ex)
      {
        logger.Error("ロードでエラーが発生しました", ex);
        throw new Exception("Load error", ex);
      }
      isLoaded = true;
      Program.CheckShutdown();

      // readerのDisposeが完了しない場合がある
      var isDone = false;
      _ = Task.Run(async () =>
      {
        try
        {
          var loadStartTime = DateTime.Now;
          logger.Info("ロード完了");

          await LoadAfterAsync(data, isProcessing, isRealtime);
          isDone = true;

          if (!isDisposed)
          {
            this.Process = LoadProcessing.Closing;
            var d = (DateTime.Now - loadStartTime).TotalSeconds;
            if (d < 0) d = 0;
            await Task.Delay(TimeSpan.FromSeconds(System.Math.Max(30.0 - d, 0.1)));

            if (!isDisposed)
            {
              logger.Warn("接続のクローズが完了しないため、強制的に破棄します");
              await Program.RestartProgramAsync(true);
            }
          }
        }
        catch (Exception ex)
        {
          logger.Error("後処理の過程でエラーが発生", ex);
          await Program.RestartProgramAsync(false);
        }
      });

      // EntityFrameworkメソッドの呼び出しでスレッドが変わることがあるので、ここで破棄する
      if (reader.Type == JVLinkObjectType.Local)
      {
        GC.Collect();
      }
      reader.Dispose();
      isDisposed = true;
      logger.Info("接続のクローズが完了しました");

      // ロード処理を待機。終わるまで制御を戻さない
      while (!isDone)
      {
        await Task.Delay(100);
      }
      logger.Info("ロード処理が正常に完了しました");
    }

    private async Task LoadAfterAsync(JVLinkReaderData data, bool isProcessing, bool isRealtime)
    {
      var saved = 0;
      this.SaveSize.Value = data.Races.Count + data.RaceHorses.Count + data.ExactaOdds.Count
        + data.FrameNumberOdds.Count + data.QuinellaOdds.Count + data.QuinellaPlaceOdds.Count +
         data.TrifectaOdds.Count + data.TrioOdds.Count + data.BornHorses.Count +
        data.Refunds.Count + data.Trainings.Count + data.WoodtipTrainings.Count + data.Horses.Count + data.HorseBloods.Count;
      logger.Info($"保存数: {this.SaveSize.Value}");

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

      this.Process = LoadProcessing.Writing;

      // トランザクションが始まるので、ここで待機しないとProgram.csからこの値をDBに保存できず、メインアプリにWritingが伝わらなくなる
      this.StartingTransaction?.Invoke(this, EventArgs.Empty);

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
          Program.CheckShutdown(db);
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
          if (item.Data.DataStatus <= item.Entity.DataStatus || item.Data.LastModified <= item.Entity.LastModified)
          {
            if (item.Entity.DataStatus == RaceDataStatus.Local && item.Data.DataStatus <= RaceDataStatus.Canceled)
            {
              // 地方重賞などについて、JV-LinkではDataStatusをLocalに設定しているが、UmaConnでは通常レースと同様に1～9の値を設定している
              // JV-Linkから来るデータは開始時刻など一部情報が欠損しているが、UmaConnから来る情報はそれがない。よってJV-Linkの情報をとばす
              // 条件分岐の際は、中央競馬のみを落とす場合／先に中央を落として後日地方を追加で落とす場合も考慮する
            }
            else
            {
              item.Data.SetEntity(item.Entity);
            }
          }
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
        var newObjCount = 0;
        while (position.Any())
        {
          var chunk = position.Take(1000);
          await dataSet.AddRangeAsync(chunk);
          await db.SaveChangesAsync();

          var count = chunk.Count();
          saved += count;
          newObjCount += count;

          position = position.Skip(1000);
        }
        // saved += items.Count();

        logger.Info($"保存　新規: {newObjCount}, 変更: {changed}");
      }

      {
        // efcoreのdb.Database.SetConnectionTimeoutがなぜか効かないので、30分待つ
        var isSucceed = false;
        var tryCount = 0;
        while (!isSucceed)
        {
          try
          {
            logger.Info("トランザクションの開始を試みます");
            await db.BeginTransactionAsync();
            logger.Info("トランザクションが正常に開始されました");
            isSucceed = true;
          }
          catch (SqliteException ex) when (ex.SqliteErrorCode == 5)  // file locked
          {
            tryCount++;
            if (tryCount >= 30 * 60)
            {
              logger.Fatal("トランザクション開始でエラー発生。プログラムをシャットダウンします", ex);
              Program.Shutdown(DownloaderError.DatabaseTimeout);
            }

            await Task.Delay(1000);
          }
        }
      }

      logger.Info($"RaceHorsesの保存を開始 {data.RaceHorses.Count}");
      await SaveDicAsync(data.RaceHorses,
        db.RaceHorses!,
        (e) => e.Name + e.RaceKey,
        (d) => d.Name + d.RaceKey,
        (list) => e => list.Contains(e.Name + e.RaceKey));
      await db.CommitAsync();

      logger.Info($"Racesの保存を開始 {data.Races.Count}");
      await SaveDicAsync(data.Races,
        db.Races!,
        (e) => e.Key,
        (d) => d.Key,
        (list) => e => list.Contains(e.Key));
      logger.Info($"Horsesの保存を開始 {data.Horses.Count}");
      await SaveDicAsync(data.Horses,
        db.Horses!,
        (e) => e.Code,
        (d) => d.Code,
        (list) => e => list.Contains(e.Code));
      logger.Info($"HorseBloodsの保存を開始 {data.HorseBloods.Count}");
      await SaveDicAsync(data.HorseBloods,
        db.HorseBloods!,
        (e) => e.Key,
        (d) => d.Key,
        (list) => e => list.Contains(e.Key));
      await db.CommitAsync();
      logger.Info($"BornHorsesの保存を開始 {data.BornHorses.Count}");
      await SaveDicAsync(data.BornHorses,
        db.BornHorses!,
        (e) => e.Code,
        (d) => d.Code,
        (list) => e => list.Contains(e.Code));
      await db.CommitAsync();

      logger.Info($"FrameNumberOddsの保存を開始 {data.FrameNumberOdds.Count}");
      await SaveDicAsync(data.FrameNumberOdds,
        db.FrameNumberOdds!,
        (e) => e.RaceKey,
        (d) => d.RaceKey,
        (list) => e => list.Contains(e.RaceKey));
      logger.Info($"QuinellaPlaceOddsの保存を開始 {data.QuinellaPlaceOdds.Count}");
      await SaveDicAsync(data.QuinellaPlaceOdds,
        db.QuinellaPlaceOdds!,
        (e) => e.RaceKey,
        (d) => d.RaceKey,
        (list) => e => list.Contains(e.RaceKey));
      logger.Info($"QuinellaOddsの保存を開始 {data.QuinellaOdds.Count}");
      await SaveDicAsync(data.QuinellaOdds,
        db.QuinellaOdds!,
        (e) => e.RaceKey,
        (d) => d.RaceKey,
        (list) => e => list.Contains(e.RaceKey));
      logger.Info($"ExactaOddsの保存を開始 {data.ExactaOdds.Count}");
      await SaveDicAsync(data.ExactaOdds,
        db.ExactaOdds!,
        (e) => e.RaceKey,
        (d) => d.RaceKey,
        (list) => e => list.Contains(e.RaceKey));
      logger.Info($"TrioOddsの保存を開始 {data.TrioOdds.Count}");
      await SaveDicAsync(data.TrioOdds,
        db.TrioOdds!,
        (e) => e.RaceKey,
        (d) => d.RaceKey,
        (list) => e => list.Contains(e.RaceKey));
      logger.Info($"TrifectaOddsの保存を開始 {data.TrifectaOdds.Count}");
      await SaveDicAsync(data.TrifectaOdds,
        db.TrifectaOdds!,
        (e) => e.RaceKey,
        (d) => d.RaceKey,
        (list) => e => list.Contains(e.RaceKey));
      await db.CommitAsync();

      logger.Info($"Refundsの保存を開始 {data.Refunds.Count}");
      await SaveDicAsync(data.Refunds,
        db.Refunds!,
        (e) => e.RaceKey,
        (d) => d.RaceKey,
        (list) => e => list.Contains(e.RaceKey));

      logger.Info($"SingleAndDoubleWinOddsの保存を開始 {data.SingleAndDoubleWinOdds.Count}");
      await SaveAsync(data.SingleAndDoubleWinOdds.Where((o) => o.Value.Time != default).Select((o) => o.Value),
        db.SingleOddsTimelines!,
        (e) => e.RaceKey + e.Time.Month + "_" + e.Time.Day + "_" + e.Time.Hour + "_" + e.Time.Minute,
        (d) => d.RaceKey + d.Time.Month + "_" + d.Time.Day + "_" + d.Time.Hour + "_" + d.Time.Minute,
        (list) => e => list.Contains(e.RaceKey + e.Time.Month + "_" + e.Time.Day + "_" + e.Time.Hour + "_" + e.Time.Minute));
      logger.Info($"Trainingsの保存を開始 {data.Trainings.Count}");
      await SaveDicAsync(data.Trainings,
        db.Trainings!,
        (e) => e.HorseKey + e.StartTime.Year + "_" + e.StartTime.Month + "_" + e.StartTime.Day + "_" + e.StartTime.Hour + "_" + e.StartTime.Minute,
        (d) => d.HorseKey + d.StartTime.Year + "_" + d.StartTime.Month + "_" + d.StartTime.Day + "_" + d.StartTime.Hour + "_" + d.StartTime.Minute,
        (list) => e => list.Contains(e.HorseKey + e.StartTime.Year + "_" + e.StartTime.Month + "_" + e.StartTime.Day + "_" + e.StartTime.Hour + "_" + e.StartTime.Minute));
      logger.Info($"WoodtipTrainingsの保存を開始 {data.WoodtipTrainings.Count}");
      await SaveDicAsync(data.WoodtipTrainings,
        db.WoodtipTrainings!,
        (e) => e.HorseKey + e.StartTime.Year + "_" + e.StartTime.Month + "_" + e.StartTime.Day + "_" + e.StartTime.Hour + "_" + e.StartTime.Minute,
        (d) => d.HorseKey + d.StartTime.Year + "_" + d.StartTime.Month + "_" + d.StartTime.Day + "_" + d.StartTime.Hour + "_" + d.StartTime.Minute,
        (list) => e => list.Contains(e.HorseKey + e.StartTime.Year + "_" + e.StartTime.Month + "_" + e.StartTime.Day + "_" + e.StartTime.Hour + "_" + e.StartTime.Minute));
      await db.CommitAsync();

      // 保存後のデータに他のデータを追加する
      this.ProcessSize.Value = 0;
      this.Processed.Value = 0;
      this.ProcessSize.Value += data.SingleAndDoubleWinOdds.Count;
      this.Process = LoadProcessing.Processing;

      // 単勝オッズを設定する
      {
        var oddsRaceKeys = data.SingleAndDoubleWinOdds.Select((o) => o.Value.RaceKey).ToArray();
        var oddsRaceHorses = await db.RaceHorses!
          .Where((r) => oddsRaceKeys.Contains(r.RaceKey))
          .ToArrayAsync();

        logger.Info($"単勝・複勝オッズの各馬への設定を開始します {data.SingleAndDoubleWinOdds.Count}");
        foreach (var odds in data.SingleAndDoubleWinOdds)
        {
          var horses = oddsRaceHorses
            .Where((h) => h.RaceKey == odds.Value.RaceKey);
          foreach (var horse in horses)
          {
            var o = odds.Value.Odds.FirstOrDefault((oo) => oo.HorseNumber == horse.Number);
            if (o.HorseNumber != default && horse.CanSetOdds(o.Odds))
            {
              horse.Odds = (short)o.Odds;
              horse.Popular = o.Popular;
              horse.PlaceOddsMax = (short)o.PlaceOddsMax;
              horse.PlaceOddsMin = (short)o.PlaceOddsMin;
            }
          }

          this.Processed.Value++;
          Program.CheckShutdown(db);
        }

        await db.SaveChangesAsync();
      }

      if (isRealtime)
      {
        this.ProcessSize.Value += data.HorseWeights.Count;
        this.ProcessSize.Value += data.CourseWeatherConditions.Count;
        this.ProcessSize.Value += data.HorseAbnormalities.Count;
        this.ProcessSize.Value += data.HorseRiderChanges.Count;
        this.ProcessSize.Value += data.RaceStartTimeChanges.Count;
        this.ProcessSize.Value += data.RaceCourseChanges.Count;

        // 古いデータは削除
        var today = DateTime.Today;
        var olds = db.RaceChanges!.Where(c => c.LastModified < today);
        db.RaceChanges!.RemoveRange(olds);
        await db.SaveChangesAsync();

        async Task AddDataAsync(IEnumerable<RaceChangeData> data)
        {
          var raceKeys = data.Select(d => d.RaceKey);
          var exists = await db!.RaceChanges!.Where(c => raceKeys.Contains(c.RaceKey)).ToArrayAsync();
          foreach (var d in data)
          {
            var ex = exists.FirstOrDefault(c => c.ChangeType == d.ChangeType && c.RaceKey == d.RaceKey && c.HorseNumber == d.HorseNumber);
            if (ex != null)
            {
              ex.LastModified = DateTime.Now;
            }
            else
            {
              d.LastModified = DateTime.Now;
              await db!.RaceChanges!.AddAsync(d);
            }
          }
        }

        // 馬の体重を設定する
        {
          logger.Info($"馬体重を設定します {data.HorseWeights.Count}");
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
            Program.CheckShutdown(db);
          }

          await AddDataAsync(data.HorseWeights.SelectMany(c => RaceChangeData.GetData(c)));
          await db.SaveChangesAsync();
        }

        // コース
        {
          logger.Info($"コース変更を設定します {data.RaceCourseChanges.Count}");
          foreach (var st in data.RaceCourseChanges)
          {
            var race = await db.Races!
              .FirstOrDefaultAsync((h) => h.Key == st.RaceKey);
            if (race != null)
            {
              race.TrackGround = st.TrackGround;
              race.TrackCornerDirection = st.TrackCornerDirection;
              race.TrackType = st.TrackType;
              race.TrackOption = st.TrackOption;
            }

            this.Processed.Value++;
            Program.CheckShutdown(db);
          }

          await AddDataAsync(data.RaceCourseChanges.Select(c => RaceChangeData.GetData(c)));
          await db.SaveChangesAsync();
        }

        // 天候、馬場
        {
          logger.Info($"天気、馬場を設定します {data.CourseWeatherConditions.Count}");
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
                race.IsWeatherSetManually = false;
              }
              if (race.TrackGround == TrackGround.Turf && weather.TurfCondition != RaceCourseCondition.Unknown)
              {
                race.TrackCondition = weather.TurfCondition;
                race.IsConditionSetManually = false;
              }
              else if ((race.TrackGround == TrackGround.Dirt || race.TrackGround == TrackGround.TurfToDirt || race.TrackGround == TrackGround.Sand) &&
                weather.DirtCondition != RaceCourseCondition.Unknown)
              {
                race.TrackCondition = weather.DirtCondition;
                race.IsConditionSetManually = false;
              }
            }

            this.Processed.Value++;
            Program.CheckShutdown(db);
          }

          await AddDataAsync(data.CourseWeatherConditions.Select(c => RaceChangeData.GetData(c)));
          await db.SaveChangesAsync();
        }

        // 馬の状態
        {
          logger.Info($"馬の出走状態を設定します {data.HorseAbnormalities.Count}");
          foreach (var ab in data.HorseAbnormalities)
          {
            var horse = await db.RaceHorses!
              .FirstOrDefaultAsync((h) => h.RaceKey == ab.RaceKey && h.Number == ab.HorseNumber);
            if (horse != null)
            {
              horse.AbnormalResult = ab.AbnormalResult;
            }

            this.Processed.Value++;
            Program.CheckShutdown(db);
          }

          await AddDataAsync(data.HorseAbnormalities.Select(c => RaceChangeData.GetData(c)));
          await db.SaveChangesAsync();
        }

        // 騎手
        {
          logger.Info($"騎手変更を設定します {data.HorseRiderChanges.Count}");
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
            Program.CheckShutdown(db);
          }

          await AddDataAsync(data.HorseRiderChanges.Select(c => RaceChangeData.GetData(c)));
          await db.SaveChangesAsync();
        }

        // 開始時刻
        {
          logger.Info($"レース開始時刻を設定します {data.RaceStartTimeChanges.Count}");
          foreach (var st in data.RaceStartTimeChanges)
          {
            var race = await db.Races!
              .FirstOrDefaultAsync((h) => h.Key == st.RaceKey);
            if (race != null)
            {
              race.StartTime = new DateTime(race.StartTime.Year, race.StartTime.Month, race.StartTime.Day, st.StartTime.Hour, st.StartTime.Minute, 0);
            }

            this.Processed.Value++;
            Program.CheckShutdown(db);
          }

          await AddDataAsync(data.RaceStartTimeChanges.Select(c => RaceChangeData.GetData(c)));
          await db.SaveChangesAsync();
        }
      }
    }

    public void Dispose()
    {
      logger.Info("接続を終了します");
      this.disposables.Dispose();
      logger.Info("接続は終了しました");
    }

    public event EventHandler? StartingTransaction;
  }

  enum LoadProcessing
  {
    Unknown,
    Opening,
    Downloading,
    Loading,
    Writing,
    Processing,
    Closing,
  }
}
