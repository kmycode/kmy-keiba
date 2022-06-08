using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.JVLink.Wrappers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Downloader
{
  internal partial class Program
  {
    private static void StartLoad(DownloaderTaskData task, bool isRealTime = false)
    {
      var loader = new JVLinkLoader();
      var isLoaded = false;
      var isDbLooping = false;

      Task.Run(() =>
      {
        var loopCount = 0;
        isDbLooping = true;

        void UpdateProcess()
        {
          var p = loader.Process.ToString().ToLower();
          if (p != task.Result)
          {
            try
            {
              SetTask(task, t =>
              {
                t.Result = p;
              });
            }
            catch (Exception ex)
            {
              logger.Warn("ダウンロード状態のタスクへの書き込みでエラー", ex);
            }
            logger.Info($"ダウンロード状態が {p} に移行しました");
          }
        }

        // トランザクションを開始する前に、データ保存中という情報をアプリに渡す
        loader.StartingTransaction += (sender, e) =>
        {
          UpdateProcess();

          if (!isRealTime)
          {
            // SaveChangesが終わる前にトランザクション始まる？
            Task.Delay(1000).Wait();
          }
        };

        while (!isLoaded)
        {
          Console.Write($"\rDWN [{loader.Downloaded.Value} / {loader.DownloadSize.Value}] LD [{loader.Loaded.Value} / {loader.LoadSize.Value}] ENT({loader.LoadEntityCount.Value}) SV [{loader.Saved.Value} / {loader.SaveSize.Value}] PC [{loader.Processed.Value} / {loader.ProcessSize.Value}]");

          UpdateProcess();

          loopCount++;
          if (loopCount % 60 == 0)
          {
            logger.Info($"DWN [{loader.Downloaded.Value} / {loader.DownloadSize.Value}] LD [{loader.Loaded.Value} / {loader.LoadSize.Value}] ENT({loader.LoadEntityCount.Value}) SV [{loader.Saved.Value} / {loader.SaveSize.Value}] PC [{loader.Processed.Value} / {loader.ProcessSize.Value}]");
          }

          Task.Delay(800).Wait();
        }

        if (!task.IsFinished)
        {
          try
          {
            SetTask(task, t => t.IsFinished = true);
          }
          catch (Exception ex)
          {
            logger.Error("タスクへの完了報告書き込みに失敗", ex);
          }
        }

        isDbLooping = false;
      });

      var isHostTaskCanceled = false;
      try
      {
        if (isRealTime)
        {
          logger.Info("ダウンロードを開始します（RT）");
          RTLoadAsync(loader, task).Wait();
        }
        else
        {
          logger.Info("ダウンロードを開始します（セットアップ／通常）");
          LoadAsync(loader, task).Wait();
        }
      }
      catch (TaskCanceledAndContinueProgramException)
      {
        logger.Warn("ダウンロードはキャンセルされました");
        isHostTaskCanceled = true;
      }
      catch (Exception ex)
      {
        logger.Error("ダウンロードでエラーが発生しました", ex);
      }
      finally
      {
        loader.Dispose();
        isLoaded = true;
      }

      while (isDbLooping)
      {
        Task.Delay(50).Wait();
      }

      if (!isHostTaskCanceled && !isHost)
      {
        KillMe();
      }
    }

    private static async Task LoadAsync(JVLinkLoader loader, DownloaderTaskData task)
    {
      var end = DateTime.Now.AddMonths(1);

      using var db = new MyContext();
      db.DownloaderTasks!.Attach(task);

      var parameters = task.Parameter.Split(',');
      if (parameters.Length < 3)
      {
        task.Error = DownloaderError.ApplicationError;
        task.IsFinished = true;
        await db.SaveChangesAsync();
        logger.Error("タスクのパラメータが足りません");
        return;
      }

      int.TryParse(parameters[0], out var startYear);
      int.TryParse(parameters[1], out var startMonth);
      var startDay = 0;
      if (startMonth > 40)
      {
        startDay = startMonth % 100;
        startMonth /= 100;
      }
      if (startYear < 1986 || startYear > end.Year || startMonth < 0 || startMonth > 12)
      {
        task.Error = DownloaderError.ApplicationError;
        task.IsFinished = true;
        await db.SaveChangesAsync();
        logger.Error($"開始年月が誤りです {parameters[0]} {parameters[1]}");
        return;
      }

      var link = parameters[2] == "central" ? JVLinkObject.Central : parameters[2] == "local" ? JVLinkObject.Local : null;
      if (link == null)
      {
        task.Error = DownloaderError.ApplicationError;
        task.IsFinished = true;
        await db.SaveChangesAsync();
        logger.Error($"リンクの指定が誤りです {parameters[2]}");
        return;
      }
      if (link.Type == JVLinkObjectType.Unknown || link.IsError)
      {
        Shutdown(DownloaderError.NotInstalledCom);
        return;
      }

      var specs1 = new string[] { "RA", "SE", "WH", "WE", "AV", "UM", "HN", "SK", "JC", "HC", "WC", "HR", };
      var specs2 = new string[] { "O1", "O2", "O3", "O4", "O5", "O6", };
      var dataspec1 = JVLinkDataspec.Race | JVLinkDataspec.Blod | JVLinkDataspec.Diff | JVLinkDataspec.Slop | JVLinkDataspec.Toku;
      if (parameters[2] == "central")
      {
        dataspec1 |= JVLinkDataspec.Wood;
      }
      var dataspec2 = JVLinkDataspec.Race;

      if (parameters[2] == "local" && startYear < 2005)
      {
        startYear = 2005;
        startMonth = 1;
      }

      var mode = parameters[3];

      var option = (DateTime.Now.Year * 12 + DateTime.Now.Month) - (startYear * 12 + startMonth) > 11 ? JVLinkOpenOption.Setup : JVLinkOpenOption.Normal;

      for (var year = startYear; year <= end.Year; year++)
      {
        for (var month = 1; month <= 12; month++)
        {
          if (year == startYear && month < startMonth)
          {
            continue;
          }
          if (year == end.Year && month > end.Month)
          {
            break;
          }

          var start = new DateTime(year, month, System.Math.Max(1, startDay));

          Console.WriteLine($"{year} 年 {month} 月");
          logger.Info($"{year} 年 {month} 月");

          if (mode != "odds")
          {
            mode = "race";
            task.Parameter = $"{year},{month},{parameters[2]},{mode},{string.Join(',', parameters.Skip(4))}";
            await db.SaveChangesAsync();
            Console.WriteLine("race");
            logger.Info("レースのダウンロードを開始します");
            await loader.LoadAsync(link,
              dataspec1,
              option,
              raceKey: null,
              startTime: start,
              endTime: start.AddMonths(1),
              loadSpecs: specs1);
          }

          mode = "odds";
          task.Parameter = $"{year},{month},{parameters[2]},{mode},{string.Join(',', parameters.Skip(4))}";
          await db.SaveChangesAsync();
          Console.WriteLine("\nodds");
          logger.Info("オッズのダウンロードを開始します");
          await loader.LoadAsync(link,
            dataspec2,
            option,
            raceKey: null,
            startTime: start,
            endTime: start.AddMonths(1),
            loadSpecs: specs2);

          mode = "race";

          Console.WriteLine();
          Console.WriteLine();

          CheckShutdown(db);
        }
      }
      /*
      await loader.LoadAsync(JVLinkObject.Central,
        JVLinkDataspec.Race | JVLinkDataspec.Blod | JVLinkDataspec.Diff,
        JVLinkOpenOption.Normal,
        raceKey: null,
        startTime: DateTime.Today.AddMonths(-1),
        endTime: null);
      */
    }

    private static async Task RTLoadAsync(JVLinkLoader loader, DownloaderTaskData task)
    {
      MyContext? db = new();
      db.DownloaderTasks!.Attach(task);

      var parameters = task.Parameter.Split(',');
      if (parameters.Length < 2)
      {
        task.Error = DownloaderError.ApplicationError;
        task.IsFinished = true;
        await db.SaveChangesAsync();
        logger.Error("タスクのパラメータが足りません");
        return;
      }

      var date = parameters[0];
      var type = parameters[1];
      var spec = parameters[2];
      var skip = parameters[3];

      var link = type == "central" ? JVLinkObject.Central : type == "local" ? JVLinkObject.Local : null;
      if (link == null)
      {
        task.Error = DownloaderError.ApplicationError;
        task.IsFinished = true;
        await db.SaveChangesAsync();
        logger.Error($"リンクの指定が誤りです {parameters[2]}");
        return;
      }
      if (link.Type == JVLinkObjectType.Unknown || link.IsError)
      {
        Shutdown(DownloaderError.NotInstalledCom);
        return;
      }

      var todayFormat = DateTime.Today.ToString("yyyyMMdd");
      if (date == "today")
      {
        date = todayFormat;
      }
      int.TryParse(date.AsSpan(0, 4), out var year);
      int.TryParse(date.AsSpan(4, 2), out var month);
      int.TryParse(date.AsSpan(6, 2), out var day);

      var dataspecs = new[]
      {
        JVLinkDataspec.RB12,
        JVLinkDataspec.RB15,
        JVLinkDataspec.RB30,
        JVLinkDataspec.RB11,
        JVLinkDataspec.RB14,
      };
      int.TryParse(spec, out var startIndex);
      if (startIndex == default || startIndex > dataspecs.Length)
      {
        // Restartメソッドを考慮し、エラーにはしない
        task.IsFinished = true;
        await db.SaveChangesAsync();
        logger.Warn("このタスクはすでに完了している可能性があります");
        return;
      }

      var start = new DateTime(year, month, day);

      int.TryParse(skip, out var skipCount);
      var query = db.Races!
        .Where(r => r.StartTime.Date >= start)
        .OrderBy(r => r.StartTime)
        .Select(r => new { r.Key, r.StartTime, r.Course, r.Grade, r.DataStatus, });
      var races = await query.ToArrayAsync();
      if (!races.Any())
      {
        task.Error = DownloaderError.TargetsNotExists;
        task.IsFinished = true;
        await db.SaveChangesAsync();
        logger.Warn($"以下の時刻以降のレースが存在しません 開始: {start}");
        return;
      }

      races = races.Where(r =>
      {
        if (type == "central")
        {
          // 中央レースと地方重賞とか
          var result = r.Course <= RaceCourse.CentralMaxValue || r.Grade == RaceGrade.LocalGrade1 || r.Grade == RaceGrade.LocalGrade2 || r.Grade == RaceGrade.LocalGrade3 || r.Grade == RaceGrade.LocalNoNamedGrade;
          if (result)
          {
            // 馬券が金曜日日販売になるのは一部のG1レースのみ
            result = r.Grade == RaceGrade.Grade1 || r.Grade == RaceGrade.Grade2 || r.Grade == RaceGrade.Grade3 || r.StartTime.Date == start;
          }
          return result;
        }
        else
        {
          return r.StartTime.Date == start && r.Course >= RaceCourse.LocalMinValue;
        }
      }).ToArray();

      var skiped = races.Skip(skipCount).ToArray();
      if (!races.Any())
      {
        startIndex++;
        skipCount = 0;
      }

      Console.WriteLine("realtime");
      logger.Info("リアルタイムデータのダウンロードを開始します");
      for (var i = startIndex - 1; i < dataspecs.Length; i++)
      {
        logger.Info($"spec: {dataspecs[i]}");

        var currentRaceIndex = 0;
        var targets = races;
        if (i == startIndex - 1)
        {
          targets = skiped;
          currentRaceIndex = skipCount;
        }

        foreach (var race in targets)
        {
          var useKey = race.Key;
          if (dataspecs[i] == JVLinkDataspec.RB14 || dataspecs[i] == JVLinkDataspec.RB12 || dataspecs[i] == JVLinkDataspec.RB15 || dataspecs[i] == JVLinkDataspec.RB11)
          {
            useKey = null;
          }
          else if (dataspecs[i] == JVLinkDataspec.RB30)
          {
            // オッズは各レースごとに落とすから時間がかかる
            if (race.DataStatus >= RaceDataStatus.PreliminaryGrade3)
            {
              continue;
            }
          }

          task.Parameter = $"{parameters[0]},{parameters[1]},{i + 1},{currentRaceIndex},{string.Join(',', parameters.Skip(4))}";
          await db.SaveChangesAsync();
          CheckShutdown(db);

          await loader.LoadAsync(link,
            dataspecs[i],
            JVLinkOpenOption.RealTime,
            raceKey: useKey,
            startTime: start,
            endTime: null,
            loadSpecs: null);

          if (useKey == null)
          {
            break;
          }
          currentRaceIndex++;
        }
      }
    }
  }
}
