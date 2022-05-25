using KmyKeiba.Data.Db;
using KmyKeiba.Data.Wrappers;
using KmyKeiba.Downloader.Math;
using KmyKeiba.Downloader.Models;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.JVLink.Wrappers;
using KmyKeiba.Shared;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;

namespace KmyKeiba.Downloader
{
  internal class Program
  {
    private static string selfPath = string.Empty;
    private static DownloaderTaskData? currentTask;

    [STAThread]
    public static void Main(string[] args)
    {
      selfPath = Assembly.GetEntryAssembly()?.Location.Replace("Downloader.dll", "Downloader.exe") ?? string.Empty;

      // マイグレーション
      Exception? dbError = null;
      try
      {
        Task.Run(async () =>
        {
          using var db = new MyContext();
          db.Database.SetCommandTimeout(1200);
          await db.Database.MigrateAsync();
        }).Wait();
      }
      catch (Exception ex)
      {
        // TODO: logs
        dbError = ex;
      }

      var command = string.Empty;
      if (args.Length >= 1)
      {
        command = args[0];
      }

      if (command == DownloaderCommand.Initialization.GetCommandText())
      {
        var version = args[1];

        // さっきのマイグレーションでやることは全部終わってるので、ここでアプリ終了
        using var db = new MyContext();
        db.DownloaderTasks!.RemoveRange(db.DownloaderTasks!);
        var data = new DownloaderTaskData
        {
          Command = DownloaderCommand.Initialization,
          IsFinished = true,
          Error = version != Constrants.ApplicationVersion ? DownloaderError.InvalidVersion : DownloaderError.Succeed,
        };
        data.Result = !string.IsNullOrEmpty(dbError?.Message) ? dbError!.Message : data.Error.GetErrorText();
        db.DownloaderTasks!.Add(data);
        db.SaveChanges();
        return;
      }
      else if (command == DownloaderCommand.DownloadSetup.GetCommandText())
      {
        if (args.Length >= 3)
        {
          _ = int.TryParse(args[2], out var beforeProcessNumber);
          try
          {
            if (beforeProcessNumber != 0)
            {
              Process.GetProcessById(beforeProcessNumber)?.Kill();
            }
          }
          catch
          {
            // 切り捨てる
            // TODO: log
          }
        }

        var task = GetTask(args[1], DownloaderCommand.DownloadSetup);
        if (task != null)
        {
          currentTask = task;
          StartLoad(task);
        }
      }
      else if (command == DownloaderCommand.OpenJvlinkConfigs.GetCommandText())
      {
        var task = GetTask(args[1], DownloaderCommand.OpenJvlinkConfigs);
        if (task != null)
        {
          try
          {
            JVLinkObject.Central.OpenConfigWindow();
            SetTask(task, t => t.IsFinished = true);
          }
          catch (Exception ex)
          {
            SetTask(task, t =>
            {
              t.IsFinished = true;
              t.Error = DownloaderError.NotInstalledCom;
              t.Result = ex.GetType().Name + "/" + ex.Message;
            });
          }
        }
      }
      else if (command == DownloaderCommand.OpenNvlinkConfigs.GetCommandText())
      {
        var task = GetTask(args[1], DownloaderCommand.OpenNvlinkConfigs);
        if (task != null)
        {
          try
          {
            JVLinkObject.Local.OpenConfigWindow();
            SetTask(task, t => t.IsFinished = true);
          }
          catch (Exception ex)
          {
            SetTask(task, t =>
            {
              t.IsFinished = true;
              t.Error = DownloaderError.NotInstalledCom;
              t.Result = ex.GetType().Name + "/" + ex.Message;
            });
          }
        }
      }



      //StartLoad();

      //StartSetPreviousRaceDays();
      //StartRunningStyleTraining();
      //StartRunningStylePredicting();
      //StartMakingStandardTimeMasterData();
    }

    private static DownloaderTaskData? GetTask(string idStr, DownloaderCommand command)
    {
      uint.TryParse(idStr, out var id);
      if (id == default)
      {
        return null;
      }

      using var db = new MyContext();
      var task = db.DownloaderTasks!.Find(id);
      if (task == null)
      {
        db.DownloaderTasks!.Add(new DownloaderTaskData
        {
          Id = id,
          IsFinished = true,
          Error = DownloaderError.ApplicationError,
          Command = DownloaderCommand.DownloadSetup,
        });
        return null;
      }
      if (task.Command != command)
      {
        task.IsFinished = true;
        task.Error = DownloaderError.ApplicationError;
        db.SaveChanges();
        return null;
      }

      return task;
    }

    private static void SetTask(DownloaderTaskData task, Action<DownloaderTaskData> changes)
    {
      using var db = new MyContext();
      db.DownloaderTasks!.Attach(task);
      changes(task);
      db.SaveChanges();
    }

    private static void StartLoad(DownloaderTaskData task)
    {
      var loader = new JVLinkLoader();
      var isLoaded = false;

      Task.Run(async () =>
      {
        await LoadAsync(loader, task);
        isLoaded = true;

        loader.Dispose();
      });

      using var db = new MyContext();
      db.DownloaderTasks!.Attach(task);

      while (!isLoaded)
      {
        Console.Write($"\rDWN [{loader.Downloaded.Value} / {loader.DownloadSize.Value}] LD [{loader.Loaded.Value} / {loader.LoadSize.Value}] ENT({loader.LoadEntityCount.Value}) SV [{loader.Saved.Value} / {loader.SaveSize.Value}] PC [{loader.Processed.Value} / {loader.ProcessSize.Value}]");

        task.Result = loader.Process.ToString().ToLower();
        db.SaveChanges();

        Task.Delay(1000).Wait();
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
        return;
      }

      int.TryParse(parameters[0], out var startYear);
      int.TryParse(parameters[1], out var startMonth);
      if (startYear < 1986 || startYear > end.Year || startMonth < 0 || startMonth > 12)
      {
        task.Error = DownloaderError.ApplicationError;
        task.IsFinished = true;
        await db.SaveChangesAsync();
        return;
      }

      var link = parameters[2] == "central" ? JVLinkObject.Central : parameters[2] == "local" ? JVLinkObject.Local : null;
      if (link == null)
      {
        task.Error = DownloaderError.ApplicationError;
        task.IsFinished = true;
        await db.SaveChangesAsync();
        return;
      }

      var specs = parameters[3] == "odds" ? new string[] { "O1", "O2", "O3", "O4", "O5", "O6", } :
        parameters[3] == "race" ? new string[] { "RA", "SE", "WH", "WE", "AV", "UM", "HN", "SK", "JC", "HC", "WC", "HR", } :
        new string[] { };

      var dataspec = JVLinkDataspec.Race | JVLinkDataspec.Blod | JVLinkDataspec.Diff | JVLinkDataspec.Slop | JVLinkDataspec.Toku;
      if (parameters[2] == "central")
      {
        dataspec |= JVLinkDataspec.Wood;
      }
      if (parameters[3] == "odds")
      {
        dataspec = JVLinkDataspec.Race;
      }

      if (parameters[2] == "local")
      {
        startYear = System.Math.Max(startYear, 2005);
        if (parameters[3] == "odds")
        {
          startYear = System.Math.Max(startYear, 2010);
        }
      }

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

          task.Parameter = $"{year},{month},{string.Join(',', parameters.Skip(2))}";
          await db.SaveChangesAsync();

          var start = new DateTime(year, month, 1);
          var option = (DateTime.Now - start).Days > 300 ? JVLinkOpenOption.Setup : JVLinkOpenOption.Normal;

          Console.WriteLine($"{year} 年 {month} 月");
          await loader.LoadAsync(link,
            dataspec,
            //JVLinkDataspec.Race,
            JVLinkOpenOption.Setup,
            raceKey: null,
            startTime: start,
            endTime: start.AddMonths(1),
            loadSpecs: specs);
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

    public static Task RestartProgramAsync(bool isIncrement)
    {
      if (currentTask == null)
      {
        return Task.CompletedTask;
      }

      Console.WriteLine();
      Console.WriteLine();

      var myProcess = Process.GetCurrentProcess();
      var myProcessNumber = myProcess?.Id ?? 0;

      var parameters = currentTask.Parameter.Split(',');
      int.TryParse(parameters[0], out var year);
      int.TryParse(parameters[1], out var month);
      if (isIncrement)
      {
        month++;
        if (month > 12)
        {
          month = 1;
          year++;
        }

        using var db = new MyContext();
        db.DownloaderTasks!.Attach(currentTask);
        currentTask.Parameter = $"{year},{month},{string.Join(',', parameters.Skip(2))}";
        db.SaveChanges();
        CheckShutdown(db);
      }
      else
      {
        CheckShutdown();
      }

      try
      {
        Process.Start(new ProcessStartInfo
        {
          FileName = "cmd",
          ArgumentList =
          {
            "/c",
            selfPath,
            currentTask.Command.GetCommandText(),
            currentTask.Id.ToString(),
            myProcessNumber.ToString(),
          },
#if !DEBUG
          CreateNoWindow = true,
#endif
        });
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        Console.WriteLine(ex.StackTrace);

        using var db = new MyContext();
        db.DownloaderTasks!.Attach(currentTask);
        currentTask.IsFinished = true;
        currentTask.Error = DownloaderError.ApplicationRuntimeError;
        db.SaveChanges();

        return Task.CompletedTask;
      }

      Environment.Exit(0);

      return Task.CompletedTask;
    }

    public static void CheckShutdown(MyContext? db = null)
    {
      var isDispose = db == null;

      db ??= new MyContext();
      var tasks = db.DownloaderTasks!.Where(t => t.Command == DownloaderCommand.Shutdown);
      if (tasks.Any())
      {
        Environment.Exit(0);
      }

      if (currentTask != null)
      {
        var task = db.DownloaderTasks!.FirstOrDefault(t => t.Id == currentTask.Id);
        if (task != null && task.IsCanceled)
        {
          Environment.Exit(0);
        }
      }

      if (isDispose)
      {
        db.Dispose();
      }
    }

    public static void Exit() => Environment.Exit(-1);

    private static void StartSetPreviousRaceDays()
    {
      Task.Run(async () => await SetPreviousRaceDays()).Wait();
    }

    private static async Task SetPreviousRaceDays()
    {
      var count = 0;
      var prevCommitCount = 0;

      using var db = new MyContext();

      // 通常のクエリは３分まで
      db.Database.SetCommandTimeout(180);
      await db.BeginTransactionAsync();

      var allTargets = db.RaceHorses!
        .Where(rh => rh.PreviousRaceDays == 0)
        .Where(rh => rh.Key != "0000000000")
        .GroupBy(rh => rh.Key)
        .Select(g => g.Key);

      var targets = await allTargets.Take(96).ToArrayAsync();

      while (targets.Any())
      {
        var targetHorses = await db.RaceHorses!
          .Where(rh => targets.Contains(rh.Key))
          .Select(rh => new { rh.Id, rh.Key, rh.RaceKey, })
          .ToArrayAsync();

        foreach (var horse in targets)
        {
          var races = targetHorses.Where(rh => rh.Key == horse).OrderBy(rh => rh.RaceKey);
          var beforeRaceDh = DateTime.MinValue;
          var isFirst = true;

          foreach (var race in races)
          {
            var y = race.RaceKey.Substring(0, 4);
            var m = race.RaceKey.Substring(4, 2);
            var d = race.RaceKey.Substring(6, 2);
            _ = int.TryParse(y, out var year);
            _ = int.TryParse(m, out var month);
            _ = int.TryParse(d, out var day);
            var dh = new DateTime(year, month, day);

            var attach = new RaceHorseData { Id = race.Id, };
            db.RaceHorses!.Attach(attach);
            if (!isFirst)
            {
              attach.PreviousRaceDays = System.Math.Max((short)(dh - beforeRaceDh).TotalDays, (short)1);
            }
            else
            {
              attach.PreviousRaceDays = -1;
              isFirst = false;
            }

            beforeRaceDh = dh;
            count++;
          }
        }

        await db.SaveChangesAsync();
        if (count >= prevCommitCount + 10000)
        {
          await db.CommitAsync();
          prevCommitCount = count;
        }
        Console.Write($"\r完了: {count}");

        targets = await allTargets.Take(96).ToArrayAsync();
      }
    }

    private static void StartRunningStyleTraining()
    {
      var rs = new PredictRunningStyleModel();

      Console.WriteLine("トレーニング中...");
      rs.Training();

      Console.WriteLine("保存中...");
      rs.SaveFile("runningstyleml.mml");
    }

    private static void StartRunningStylePredicting()
    {
      var rs = new PredictRunningStyleModel();

      Console.WriteLine("ロード中...");
      rs.OpenFile("runningstyleml.mml");

      Console.WriteLine("予想中...");
      var done = -1;
      var sum = 0;
      while (done != 0)
      {
        done = rs.PredictAsync(10_0000).Result;
        sum += done;
        Console.WriteLine($"{sum} 完了");
      }
    }

    private static void StartMakingStandardTimeMasterData()
    {
      Console.WriteLine("マスターデータ作成中...");
      Task.Run(async () =>
      {
        try
        {
          using var db = new MyContext();
          await db.BeginTransactionAsync();

          var grounds = new[] { TrackGround.Turf, TrackGround.Dirt, TrackGround.TurfToDirt, };
          var types = new[] { TrackType.Flat, TrackType.Steeplechase, };
          var conditions = new[] { RaceCourseCondition.Unknown, RaceCourseCondition.Standard, RaceCourseCondition.Good, RaceCourseCondition.Soft, RaceCourseCondition.Yielding, };
          var distances = new[] { 0, 1000, 1400, 1800, 2200, 2800, 10000, };

          for (var i = 1; i < 99; i++)
          {
            var course = (RaceCourse)i;
            if (string.IsNullOrEmpty(course.GetName()))
            {
              continue;
            }

            for (var year = 1990; year < 2022 - 1; year++)
            {
              var totalSampels = 0;

              var startTime = new DateTime(year, 1, 1);
              var endTime = new DateTime(year + 2, 1, 1);

              Console.WriteLine($"[{year}] {course.GetName()}");

              // 中央競馬のデータに地方重賞出場する地方馬の過去成績がいくつか紛れてる
              // それが１つでもあると、中央からしばらく後に地方のデータ取得した後に支障が発生したりする
              // 重複集計をなくすには、このメソッドの引数に範囲となる年を指定するのがいいかも
              // if (await db.RaceStandardTimes!.AnyAsync(rt => rt.Course == course && rt.SampleStartTime == startTime && rt.SampleEndTime == endTime))
              // {
              //   continue;
              // }

              var targets = await db.Races!
                .Where(r => r.Course == course && r.StartTime >= startTime && r.StartTime < endTime && r.Distance > 0)
                //.Join(db.RaceHorses!.Where(rh => rh.ResultOrder == 1), r => r.Key, rh => rh.RaceKey, (r, rh) => new { rh.ResultTime, r.Distance, rh.AfterThirdHalongTime, r.TrackGround, r.TrackType, r.TrackCondition, })
                .Select((r) => new { r.Key, r.Distance, r.TrackGround, r.TrackType, r.TrackCondition, })
                .ToArrayAsync();
              if (targets.Length == 0)
              {
                continue;
              }

              var keys = targets.Select(r => r.Key).ToArray();
              var allHorses = await db.RaceHorses!
                .Where(rh => keys.Contains(rh.RaceKey) && rh.ResultOrder > 0)
                .Select(rh => new { rh.ResultTime, rh.RaceKey, rh.AfterThirdHalongTime, })
                .ToArrayAsync();

              var exists = await db.RaceStandardTimes!
                .Where(st => st.Course == course && st.SampleStartTime == startTime && st.SampleEndTime == endTime)
                .ToArrayAsync();

              foreach (var ground in grounds)
              {
                foreach (var type in types)
                {
                  foreach (var condition in conditions)
                  {
                    for (var di = 0; di < distances.Length - 1; di++)
                    {
                      // 障害に距離は必要ない
                      if (di != 0 && type == TrackType.Steeplechase)
                      {
                        continue;
                      }

                      var distanceMin = distances[di];
                      var distanceMax = distances[di + 1];

                      var timesQuery = targets
                        .Where(r => r.TrackGround == ground && r.TrackType == type);
                      if (condition != RaceCourseCondition.Unknown)
                      {
                        timesQuery = timesQuery.Where(r => r.TrackCondition == condition);
                      }
                      if (type != TrackType.Steeplechase)
                      {
                        timesQuery = timesQuery.Where(r => r.Distance >= distanceMin && r.Distance < distanceMax);
                      }
                      
                      var times = timesQuery
                        .GroupJoin(allHorses, t => t.Key, h => h.RaceKey, (t, hs) => new { Race = t, Horses = hs, })
                        .ToArray();

                      var arr = times.SelectMany(t => t.Horses.Select(h => h.ResultTime.TotalSeconds / t.Race.Distance)).ToArray();
                      var arr2 = times.SelectMany(t => t.Horses.Select(h => h.AfterThirdHalongTime.TotalSeconds)).ToArray();
                      var statistic = new StatisticSingleArray
                      {
                        Values = arr,
                      };
                      var statistic2 = new StatisticSingleArray
                      {
                        Values = arr2,
                      };

                      RaceStandardTimeMasterData data;

                      var old = exists.FirstOrDefault(st => st.Ground == ground &&
                        st.TrackType == type && st.Condition == condition &&
                        st.Distance == (short)distanceMin && st.DistanceMax == (short)distanceMax);
                      if (old != null)
                      {
                        data = old;
                        data.SampleCount = times.Length;
                        data.Average = statistic.Average;
                        data.Median = statistic.Median;
                        data.Deviation = statistic.Deviation;
                        data.A3FAverage = statistic2.Average;
                        data.A3FMedian = statistic2.Median;
                        data.A3FDeviation = statistic2.Deviation;
                      }
                      else
                      {
                        data = new RaceStandardTimeMasterData
                        {
                          Course = course,
                          Ground = ground,
                          TrackType = type,
                          Condition = condition,
                          SampleStartTime = startTime,
                          SampleEndTime = endTime,
                          SampleCount = times.Length,
                          Distance = (short)distanceMin,
                          DistanceMax = (short)distanceMax,
                          Average = statistic.Average,
                          Median = statistic.Median,
                          Deviation = statistic.Deviation,
                          A3FAverage = statistic2.Average,
                          A3FMedian = statistic2.Median,
                          A3FDeviation = statistic2.Deviation,
                        };
                        await db.RaceStandardTimes!.AddAsync(data);
                      }

                      totalSampels += data.SampleCount;
                      if (totalSampels >= 10000)
                      {
                        await db.SaveChangesAsync();
                        totalSampels = 0;
                      }
                    }
                  }
                }
              }

              // 毎年
              await db.SaveChangesAsync();
            }

            // コースごと
            await db.CommitAsync();
          }
        }
        catch
        {

        }
      }).Wait();
    }
  }
}