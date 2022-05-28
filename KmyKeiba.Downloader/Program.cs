using KmyKeiba.Data.Db;
using KmyKeiba.Data.Wrappers;
using KmyKeiba.Downloader.Math;
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
    private static int retryDownloadCount;

    [STAThread]
    public static void Main(string[] args)
    {
      selfPath = Assembly.GetEntryAssembly()?.Location.Replace("Downloader.dll", "Downloader.exe") ?? string.Empty;

      Console.WriteLine("\n\n\n============= Start Program ==============\n");

      if (args.FirstOrDefault() != "kill")
      {
        //args = new[] { "dwrt", "134", };
        //args = new[] { "kill", "2960" };
      }

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
          _ = int.TryParse(args[3], out retryDownloadCount);
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
          KillMe();
        }
      }
      else if (command == DownloaderCommand.DownloadRealTimeData.GetCommandText())
      {
        if (args.Length >= 4)
        {
          _ = int.TryParse(args[2], out var beforeProcessNumber);
          _ = int.TryParse(args[3], out retryDownloadCount);
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

        var task = GetTask(args[1], DownloaderCommand.DownloadRealTimeData);
        if (task != null)
        {
          currentTask = task;
          StartLoad(task, true);
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
          KillMe();
        }
      }
      else if (command == "kill")
      {
        _ = int.TryParse(args[1], out var beforeProcessNumber);
        try
        {
          if (beforeProcessNumber != 0)
          {
            Process.GetProcessById(beforeProcessNumber)?.Kill();
          }
        }
        catch (Exception ex)
        {
          // 切り捨てる
          // TODO: log
          Console.WriteLine(ex.Message);
          Console.ReadKey();
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

    private static void StartLoad(DownloaderTaskData task, bool isRealTime = false)
    {
      var loader = new JVLinkLoader();
      var isLoaded = false;

      Task.Run(async () =>
      {
        if (isRealTime)
        {
          await RTLoadAsync(loader, task);
        }
        else
        {
          await LoadAsync(loader, task);
        }
        isLoaded = true;

        loader.Dispose();
      });

      using var db = new MyContext();
      db.DownloaderTasks!.Attach(task);

      while (!isLoaded)
      {
        Console.Write($"\rDWN [{loader.Downloaded.Value} / {loader.DownloadSize.Value}] LD [{loader.Loaded.Value} / {loader.LoadSize.Value}] ENT({loader.LoadEntityCount.Value}) SV [{loader.Saved.Value} / {loader.SaveSize.Value}] PC [{loader.Processed.Value} / {loader.ProcessSize.Value}]");

        var p = loader.Process.ToString().ToLower();
        if (p != task.Result)
        {
          task.Result = p;
          db.SaveChanges();
        }

        Task.Delay(800).Wait();
      }

      task.IsFinished = true;
      db.SaveChanges();

      KillMe();
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

          if (mode != "odds")
          {
            mode = "race";
            task.Parameter = $"{year},{month},{parameters[2]},{mode},{string.Join(',', parameters.Skip(4))}";
            await db.SaveChangesAsync();
            Console.WriteLine("race");
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
          await loader.LoadAsync(link,
            dataspec2,
            option,
            raceKey: null,
            startTime: start,
            endTime: start.AddMonths(1),
            loadSpecs: specs2);

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
      for (var i = startIndex - 1; i < dataspecs.Length; i++)
      {
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
            startTime: null,
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

    private static void KillMe()
    {
      if (currentTask?.Parameter.Contains("central") == true)
      {
        Environment.Exit(0);
        return;
      }

      var myProcess = Process.GetCurrentProcess();
      var myProcessNumber = myProcess?.Id ?? 0;

      Process.Start(new ProcessStartInfo
      {
        FileName = "cmd",
        ArgumentList =
          {
            "/c",
            selfPath,
            "kill",
            myProcessNumber.ToString(),
          },
#if !DEBUG
          CreateNoWindow = true,
#endif
      });

      Environment.Exit(0);
    }

    public static async Task RestartProgramAsync(bool isIncrement)
    {
      var myProcess = Process.GetCurrentProcess();
      var myProcessNumber = myProcess?.Id ?? 0;

      if (currentTask == null)
      {
        KillMe();
        return;
      }

      if (retryDownloadCount >= 16)
      {
        SetTask(currentTask, t =>
        {
          t.IsFinished = true;
          t.Error = DownloaderError.Timeout;
        });

        KillMe();
        return;
      }

      Console.WriteLine();
      Console.WriteLine();

      var shellParams = new List<string>();
      var parameters = currentTask.Parameter.Split(',');

      if (currentTask.Command == DownloaderCommand.DownloadSetup)
      {
        int.TryParse(parameters[0], out var year);
        int.TryParse(parameters[1], out var month);
        var mode = parameters[3];
        if (isIncrement)
        {
          retryDownloadCount = 0;

          if (mode == "race")
          {
            mode = "odds";
          }
          else
          {
            mode = "race";

            month++;
            if (month > 12)
            {
              month = 1;
              year++;
            }
          }

          using var db = new MyContext();
          db.DownloaderTasks!.Attach(currentTask);
          currentTask.Parameter = $"{year},{month},{parameters[2]},{mode},{string.Join(',', parameters.Skip(4))}";
          await db.SaveChangesAsync();
          CheckShutdown(db);
        }
        else
        {
          CheckShutdown();
        }

        shellParams.Add(currentTask.Id.ToString());
        shellParams.Add(myProcessNumber.ToString());
        shellParams.Add((retryDownloadCount + 1).ToString());
      }
      else
      {
        int.TryParse(parameters[2], out var kind);
        int.TryParse(parameters[3], out var skip);

        using var db = new MyContext();
        db.DownloaderTasks!.Attach(currentTask);
        currentTask.Parameter = $"{parameters[0]},{parameters[1]},{kind},{skip + 1},{string.Join(',', parameters.Skip(4))}";
        await db.SaveChangesAsync();
        CheckShutdown(db);

        shellParams.Add(currentTask.Id.ToString());
        shellParams.Add(myProcessNumber.ToString());
        shellParams.Add((retryDownloadCount + 1).ToString());
      }


      try
      {
        var info = new ProcessStartInfo
        {
          FileName = "cmd",
          ArgumentList =
          {
            "/c",
            selfPath,
            currentTask.Command.GetCommandText(),
          },
#if !DEBUG
          CreateNoWindow = true,
#endif
        };
        foreach (var param in shellParams)
        {
          info.ArgumentList.Add(param);
        }
        Process.Start(info);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        Console.WriteLine(ex.StackTrace);

        using var db = new MyContext();
        db.DownloaderTasks!.Attach(currentTask);
        currentTask.IsFinished = true;
        currentTask.Error = DownloaderError.ApplicationRuntimeError;
        await db.SaveChangesAsync();

        return;
      }
      finally
      {
        KillMe();
      }

      Environment.Exit(0);
    }

    public static void Shutdown(DownloaderError error, string? message = null)
    {
      if (currentTask != null)
      {
        SetTask(currentTask, t =>
        {
          t.IsFinished = true;
          t.Error = error;
          t.Result = message ?? string.Empty;
        });
      }

      KillMe();
      // Environment.Exit(0);
    }

    public static void CheckShutdown(MyContext? db = null)
    {
      var isDispose = db == null;

      if (File.Exists(Constrants.ShutdownFilePath))
      {
        KillMe();
        //Environment.Exit(0);
      }

      db ??= new MyContext();

      if (currentTask != null)
      {
        var task = db.DownloaderTasks!.FirstOrDefault(t => t.Id == currentTask.Id);
        if (task != null && task.IsCanceled)
        {
          KillMe();
          //Environment.Exit(0);
        }
      }
      else
      {
        KillMe();
        //Environment.Exit(0);
      }

      if (isDispose)
      {
        db.Dispose();
      }
    }

    public static void Exit() => Environment.Exit(-1);
  }
}