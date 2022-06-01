using KmyKeiba.Data.Db;
using KmyKeiba.Data.Wrappers;
using KmyKeiba.Downloader.Injection;
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
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    private static string selfPath = string.Empty;
    private static DownloaderTaskData? currentTask;
    private static int retryDownloadCount;

    [STAThread]
    public static void Main(string[] args)
    {
      selfPath = Assembly.GetEntryAssembly()?.Location.Replace("Downloader.dll", "Downloader.exe") ?? string.Empty;
      var selfPathDir = Path.GetDirectoryName(selfPath) ?? "./";

      //CreateTrainingMovieList();
      //return;

      log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(Path.Combine(selfPathDir, "log4net.config")));
      log4net.GlobalContext.Properties["pid"] = System.Diagnostics.Process.GetCurrentProcess().Id;

      logger.Info("================================");
      logger.Info("==                            ==");
      logger.Info("==            開始            ==");
      logger.Info("==                            ==");
      logger.Info("================================");
      logger.Info($"Version: {Constrants.ApplicationVersion}");

      Console.WriteLine("\n\n\n============= Start Program ==============\n");

      if (args.FirstOrDefault() != "kill")
      {
        object oldArgs = args;

        //args = new[] { "movie", "1266", };
        //args = new[] { "kill", "2960" };

        if ((object)args != oldArgs)
        {
          logger.Info($"下記のパラメータはデバッグのために隠蔽されました: {string.Join(',', oldArgs)}");
        }
      }
      logger.Info($"コマンドラインパラメータ: {string.Join(',', args)}");

      // JV-LinkのIDを設定
      var softwareId = InjectionManager.GetInstance<ICentralSoftwareIdGetter>(InjectionManager.CentralSoftwareIdGetter);
      if (softwareId != null)
      {
        JVLinkObject.CentralInitializationKey = softwareId.InitializationKey;
        logger.Info("ソフトIDを設定しました");
      }
      else
      {
        logger.Warn("ソフトIDが見つからなかったので、デフォルト値を設定します");
      }

      var command = string.Empty;
      if (args.Length >= 1)
      {
        command = args[0];
        logger.Info($"コマンド {command}");
      }
      else
      {
        logger.Warn("コマンドが見つかりません");
      }

      if (command == DownloaderCommand.Initialization.GetCommandText())
      {
        var version = args[1];
        logger.Info("初期化を開始します");

        try
        {
          logger.Info("DBのマイグレーションを開始");
          Task.Run(async () =>
          {
            using var db = new MyContext();
            db.Database.SetCommandTimeout(1200);
            await db.Database.MigrateAsync();
          }).Wait();
          logger.Info("DBのマイグレーション完了");

          using var db = new MyContext();
          db.DownloaderTasks!.RemoveRange(db.DownloaderTasks!);
          var data = new DownloaderTaskData
          {
            Command = DownloaderCommand.Initialization,
            IsFinished = true,
            Error = version != Constrants.ApplicationVersion ? DownloaderError.InvalidVersion : DownloaderError.Succeed,
          };
          data.Result = data.Error.GetErrorText();
          db.DownloaderTasks!.Add(data);
          db.SaveChanges();

          if (data.Error == DownloaderError.InvalidVersion)
          {
            logger.Error($"バージョンが異なります アプリ:{version} ダウンローダ:{Constrants.ApplicationVersion}");
          }
          if (data.Error == DownloaderError.Succeed)
          {
            logger.Info("初期化に成功しました");
          }
        }
        catch (Exception ex)
        {
          logger.Error("初期化に失敗しました", ex);
        }
        return;
      }
      else if (command == DownloaderCommand.DownloadSetup.GetCommandText())
      {
        if (args.Length >= 3)
        {
          _ = int.TryParse(args[2], out var beforeProcessNumber);
          _ = int.TryParse(args[3], out retryDownloadCount);

          logger.Warn($"プロセス {beforeProcessNumber} をキルします");

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
            logger.Warn($"プロセス {beforeProcessNumber} のキルに失敗しました", ex);
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

          logger.Warn($"プロセス {beforeProcessNumber} をキルします");

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
            logger.Warn($"プロセス {beforeProcessNumber} のキルに失敗しました", ex);
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
            logger.Error("中央競馬設定画面のオープンに失敗しました", ex);
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
            logger.Error("地方競馬設定画面のオープンに失敗しました", ex);
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
      else if (command == DownloaderCommand.OpenMovie.GetCommandText())
      {
        var task = GetTask(args[1], DownloaderCommand.OpenMovie);
        if (task != null)
        {
          currentTask = task;
          OpenMovie();
          KillMe();
        }
      }
      else if (command == "kill")
      {
        _ = int.TryParse(args[1], out var beforeProcessNumber);
        logger.Warn($"プロセス {beforeProcessNumber} をキルします");

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
          logger.Warn($"プロセス {beforeProcessNumber} のキルに失敗しました", ex);
          Console.WriteLine(ex.Message);
          Console.ReadKey();
        }
      }
      else
      {
        logger.Warn("このパラメータは対応していません");
      }

      logger.Info("完了");

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
        logger.Warn($"ID {id} のタスクが見つかりませんでした");
        return null;
      }
      if (task.Command != command)
      {
        task.IsFinished = true;
        task.Error = DownloaderError.ApplicationError;
        db.SaveChanges();
        logger.Warn($"ID {id} のタスクは {task.Command} を期待していましたが、実際にコマンドラインパラメータとして送られてきたコマンドは {command} でした");
        return null;
      }

      return task;
    }

    private static void SetTask(DownloaderTaskData task, Action<DownloaderTaskData> changes)
    {
      try
      {
        using var db = new MyContext();
        db.DownloaderTasks!.Attach(task);
        changes(task);
        db.SaveChanges();
      }
      catch (Exception ex)
      {
        logger.Error("タスクへのデータ書き込みに失敗しました", ex);
      }
    }

    private static void StartLoad(DownloaderTaskData task, bool isRealTime = false)
    {
      var loader = new JVLinkLoader();
      var isLoaded = false;
      var isDbLooping = false;

      Task.Run(() =>
      {
        using var db = new MyContext();
        db.DownloaderTasks!.Attach(task);

        var loopCount = 0;
        isDbLooping = true;

        while (!isLoaded)
        {
          Console.Write($"\rDWN [{loader.Downloaded.Value} / {loader.DownloadSize.Value}] LD [{loader.Loaded.Value} / {loader.LoadSize.Value}] ENT({loader.LoadEntityCount.Value}) SV [{loader.Saved.Value} / {loader.SaveSize.Value}] PC [{loader.Processed.Value} / {loader.ProcessSize.Value}]");

          var p = loader.Process.ToString().ToLower();
          if (p != task.Result)
          {
            task.Result = p;
            db.SaveChanges();
            logger.Info($"ダウンロード状態が {p} に移行しました");
          }

          loopCount++;
          if (loopCount % 60 == 0)
          {
            logger.Info($"DWN [{loader.Downloaded.Value} / {loader.DownloadSize.Value}] LD [{loader.Loaded.Value} / {loader.LoadSize.Value}] ENT({loader.LoadEntityCount.Value}) SV [{loader.Saved.Value} / {loader.SaveSize.Value}] PC [{loader.Processed.Value} / {loader.ProcessSize.Value}]");
          }

          Task.Delay(800).Wait();
        }

        task.IsFinished = true;
        db.SaveChanges();

        isDbLooping = false;
      });

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
        isLoaded = true;
      }
      catch (Exception ex)
      {
        logger.Error("ダウンロードでエラーが発生しました", ex);
      }
      finally
      {
        loader.Dispose();
      }

      while (isDbLooping)
      {
        Task.Delay(50).Wait();
      }

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
      logger.Info("自殺を開始します");

      if (currentTask?.Parameter.Contains("central") == true)
      {
        logger.Info("中央競馬：正常終了");
        Environment.Exit(0);
        return;
      }

      var myProcess = Process.GetCurrentProcess();
      var myProcessNumber = myProcess?.Id ?? 0;

      logger.Info($"自分を殺すプロセスを開始します {myProcessNumber}");
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
      logger.Info($"プログラムを再起動します インクリメント:{isIncrement}");

      var myProcess = Process.GetCurrentProcess();
      var myProcessNumber = myProcess?.Id ?? 0;

      if (currentTask == null)
      {
        logger.Warn("現在のタスクが見つかりませんでした");
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
        logger.Warn("リトライ回数が上限に達しました");

        KillMe();
        return;
      }

      Console.WriteLine();
      Console.WriteLine();

      var shellParams = new List<string>();
      var parameters = currentTask.Parameter.Split(',');

      logger.Info($"現在のタスクのコマンド: {currentTask.Command}");

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
          logger.Info(currentTask.Parameter);
          CheckShutdown(db);
        }
        else
        {
          logger.Info("データを保存せずにシャットダウンします");
          CheckShutdown();
        }

        shellParams.Add(currentTask.Id.ToString());
        shellParams.Add(myProcessNumber.ToString());
        shellParams.Add((retryDownloadCount + 1).ToString());
      }
      else
      {
        logger.Info("リアルタイム更新を再起動");

        int.TryParse(parameters[2], out var kind);
        int.TryParse(parameters[3], out var skip);

        using var db = new MyContext();
        db.DownloaderTasks!.Attach(currentTask);
        currentTask.Parameter = $"{parameters[0]},{parameters[1]},{kind},{skip + 1},{string.Join(',', parameters.Skip(4))}";
        await db.SaveChangesAsync();
        logger.Info(currentTask.Parameter);
        CheckShutdown(db);

        shellParams.Add(currentTask.Id.ToString());
        shellParams.Add(myProcessNumber.ToString());
        shellParams.Add((retryDownloadCount + 1).ToString());
      }

      try
      {
        logger.Info($"新しいプロセスを開始します");
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
        logger.Info($"パラメータ: {string.Join('/', info.ArgumentList)}");
        Process.Start(info);
      }
      catch (Exception ex)
      {
        logger.Error("プロセス起動でエラーが発生しました", ex);
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

      logger.Info("完了");
      Environment.Exit(0);
    }

    public static void Shutdown(DownloaderError error, string? message = null)
    {
      logger.Info($"シャットダウンを試みます　コード:{error}");

      if (currentTask != null)
      {
        logger.Info($"タスク {currentTask.Id}");
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
        logger.Warn("シャットダウンファイルが存在したのでシャットダウンします");
        KillMe();
        //Environment.Exit(0);
      }

      if (currentTask != null)
      {
        var liveFileName = Path.Combine(Constrants.AppDataPath, "live");
        if (!File.Exists(liveFileName) || File.GetLastWriteTime(liveFileName) < DateTime.Now.AddMinutes(-5))
        {
          logger.Warn("ライブファイルが存在しないか、更新時刻を超過していたのでシャットダウンします");
          KillMe();
        }
      }

      db ??= new MyContext();

      if (currentTask != null)
      {
        var task = db.DownloaderTasks!.FirstOrDefault(t => t.Id == currentTask.Id);
        if (task != null && task.IsCanceled)
        {
          logger.Warn("タスクが存在しないか、キャンセルされていたのでシャットダウンします");
          KillMe();
          //Environment.Exit(0);
        }
      }
      else
      {
        logger.Warn("現在のタスクが見つかりませんでした。シャットダウンします");
        KillMe();
        //Environment.Exit(0);
      }

      if (isDispose)
      {
        db.Dispose();
      }
    }

    private static void OpenMovie()
    {
      var task = currentTask;
      if (task == null)
      {
        logger.Warn("動画再生のタスクが見つかりませんでした");
        return;
      }

      logger.Info($"動画再生を開始します。パラメータ: {task.Parameter}");

      var p = task.Parameter.Split(',');
      if (p.Length < 3)
      {
        logger.Error("パラメータの数が足りません");
        return;
      }
      var raceKey = p[0];
      var typeStr = p[1];
      var linkName = p[2];

      var link = linkName == "central" ? JVLinkObject.Central : JVLinkObject.Local;
      short.TryParse(typeStr, out var type);

      try
      {
        logger.Info($"動画再生をリンクに問い合わせます {type} / {raceKey}");

        link.PlayMovie((JVLinkMovieType)type, raceKey);
        SetTask(task, t =>
        {
          t.IsFinished = true;
          t.Result = "ok";
        });

        logger.Info("動画再生に成功しました");
      }
      catch (JVLinkException<JVLinkMovieResult> ex)
      {
        logger.Error($"動画再生に失敗しました {ex.Code}", ex);

        SetTask(task, t =>
        {
          t.IsFinished = true;
          t.Result = JVLinkException.GetAttribute(ex.Code).Message;
          t.Error = DownloaderError.TargetsNotExists;
        });
      }
      catch (Exception ex)
      {
        logger.Error("動画再生でエラーが発生しました", ex);
      }
    }

    private static void CreateTrainingMovieList()
    {
      var task = currentTask;
      if (task == null)
      {
        // return;
        task = new();
      }

      JVLinkObject? link = null;
      try
      {
        link = JVLinkObject.Central;
        using var reader = link.OpenMovie(JVLinkTrainingMovieType.Weekly, "20220528");

        var list = reader.ReadKeys();
      }
      catch (JVLinkException<JVLinkMovieResult> ex)
      {
        task.Error = DownloaderError.ApplicationRuntimeError;
        task.Result = ex.Message;
      }
      catch (Exception ex)
      {
        logger.Error("動画リストダウンロードでエラーが発生しました", ex);
      }

      using var db = new MyContext();
      // db.DownloaderTasks!.Attach(task);

      task.IsFinished = true;
      db.SaveChanges();

      KillMe();
    }
  }

  internal static class ResultExtensions
  {
    public static DownloaderError ToDownloaderError(this JVLinkMovieResult result)
    {
      return result switch
      {
        JVLinkMovieResult.ServerError => DownloaderError.ServerError,
        JVLinkMovieResult.AuthenticationError => DownloaderError.AuthenticationError,
        JVLinkMovieResult.InternalError => DownloaderError.ServerError,
        JVLinkMovieResult.InvalidKey => DownloaderError.LicenceKeyExpired,
        JVLinkMovieResult.InMaintance => DownloaderError.InMaintance,
        JVLinkMovieResult.NotFound => DownloaderError.TargetsNotExists,
        JVLinkMovieResult.Succeed => DownloaderError.Succeed,
        _ => DownloaderError.ApplicationRuntimeError,
      };
    }
  }
}