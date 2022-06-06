using KmyKeiba.Data.Db;
using KmyKeiba.Downloader.Injection;
using KmyKeiba.JVLink.Wrappers;
using KmyKeiba.Shared;
using log4net.Repository.Hierarchy;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;

namespace KmyKeiba.Downloader
{
  internal partial class Program
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    private static string selfPath = string.Empty;
    private static DownloaderTaskData? currentTask;
    private static int retryDownloadCount;
    private static bool isCheckShutdown = true;
    private static bool isHost = false;

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

#if !DEBUG
      var rootLogger = ((Hierarchy)logger.Logger.Repository).Root;
      if (File.Exists(Constrants.DebugFilePath))
      {
        rootLogger.Level = log4net.Core.Level.All;
        logger.Info("ログレベル: All (デバッグファイルが見つかりました)");
      }
      else
      {
        rootLogger.Level = log4net.Core.Level.Info;
        logger.Info("ログレベル: Info");
      }
#else
      logger.Info("ログレベル: All");
#endif

      Console.WriteLine("\n\n\n============= Start Program ==============\n");

      if (args.FirstOrDefault() != "kill")
      {
        object oldArgs = args;

        //args = new[] { "dwrt", "31", };
        //args = new[] { "movie", "1266", };
        //args = new[] { "kill", "2960" };
        //args = new[] { "download", "5" };

        if ((object)args != oldArgs)
        {
          isCheckShutdown = false;
          logger.Info($"下記のパラメータはデバッグのために隠蔽されました: {string.Join(',', oldArgs)}");
        }
      }
      logger.Info($"コマンドラインパラメータ: {string.Join(',', args)}");
      Console.WriteLine(string.Join(',', args));

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
          KillProcess(args, 2);
        }

        var task = GetTask(args[1], DownloaderCommand.DownloadSetup);
        if (task != null)
        {
          SetCurrentTask(task);
          StartLoad(task);
          KillMe();
        }
      }
      else if (command == DownloaderCommand.DownloadRealTimeData.GetCommandText())
      {
        if (args.Length >= 4)
        {
          KillProcess(args, 2);
        }

        StartRTHost();
        /*
        var task = GetTask(args[1], DownloaderCommand.DownloadRealTimeData);
        if (task != null)
        {
          currentTask = task;
          StartLoad(task, true);
        }
        */
      }
      else if (command == DownloaderCommand.OpenJvlinkConfigs.GetCommandText())
      {
        var task = GetTask(args[1], DownloaderCommand.OpenJvlinkConfigs);
        if (task != null)
        {
          try
          {
            if (JVLinkObject.Central.Type == JVLinkObjectType.Unknown || JVLinkObject.Central.IsError)
            {
              throw new InvalidOperationException("コンポーネントがインストールされていません");
            }
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
            if (JVLinkObject.Local.Type == JVLinkObjectType.Unknown || JVLinkObject.Local.IsError)
            {
              throw new InvalidOperationException("コンポーネントがインストールされていません");
            }
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
          KillMe(isForce: true);
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
      else if (command == DownloaderCommand.OpenMovieList.GetCommandText())
      {
        var task = GetTask(args[1], DownloaderCommand.OpenMovieList);
        if (task != null)
        {
          currentTask = task;
          CreateTrainingMovieList();
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

    private static void KillProcess(string[] args, int startIndex)
    {
      _ = int.TryParse(args[startIndex], out var beforeProcessNumber);
      _ = int.TryParse(args[startIndex + 1], out retryDownloadCount);

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

    private static void SetCurrentTask(DownloaderTaskData task)
    {
      currentTask = task;
      SetTask(task, t => t.ProcessId = Process.GetCurrentProcess().Id);
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
        return;
      }

      var p = task.Parameter.Split(',');
      if (p.Length < 1)
      {
        SetTask(task, t =>
        {
          t.Error = DownloaderError.ApplicationError;
          t.IsFinished = true;
        });
        logger.Error("タスクのパラメータが足りません");
        return;
      }

      var horseKey = p[0];
      logger.Info($"キー: {horseKey} の調教動画一覧を取得します");

      using var db = new MyContext();

      try
      {
        IEnumerable<DateOnly> list;

        try
        {
          var link = JVLinkObject.Central;
          using var reader = link.OpenMovie(JVLinkTrainingMovieType.Horse, horseKey);
          list = reader.ReadKeys()
            .Select(k =>
            {
              var dateStr = k[..8];
              short.TryParse(k.Substring(0, 4), out var year);
              short.TryParse(k.Substring(4, 2), out var month);
              short.TryParse(k.Substring(6, 2), out var day);
              return new DateOnly(year, month, day);
            }).ToArray();
          logger.Info($"動画の数: {list.Count()}");
        }
        catch (JVLinkException<JVLinkMovieResult> ex) when (ex.Code == JVLinkMovieResult.NotFound)
        {
          // 動画の数はゼロということ
          list = Enumerable.Empty<DateOnly>();
          logger.Info($"動画の数: {list.Count()}");
        }

        var trainings = db.Trainings!.Where(t => t.HorseKey == horseKey).ToArray();
        var woodTrainings = db.WoodtipTrainings!.Where(t => t.HorseKey == horseKey).ToArray();
        foreach (var data in trainings)
        {
          data.MovieStatus = list.Any(i => i.Year == data.StartTime.Year && i.Month == data.StartTime.Month && i.Day == data.StartTime.Day)
            ? MovieStatus.Available : MovieStatus.Unavailable;
        }
        foreach (var data in woodTrainings)
        {
          data.MovieStatus = list.Any(i => i.Year == data.StartTime.Year && i.Month == data.StartTime.Month && i.Day == data.StartTime.Day)
            ? MovieStatus.Available : MovieStatus.Unavailable;
        }
        db.SaveChanges();
      }
      catch (JVLinkException<JVLinkMovieResult> ex)
      {
        logger.Error($"動画リストダウンロードでエラーが発生しました {ex.Code}", ex);
        task.Error = DownloaderError.ApplicationRuntimeError;
        task.Result = ex.Message;
      }
      catch (Exception ex)
      {
        logger.Error("動画リストダウンロードでエラーが発生しました", ex);
      }

      db.DownloaderTasks!.Attach(task);

      task.IsFinished = true;
      db.SaveChanges();
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