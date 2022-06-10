using KmyKeiba.Data.Db;
using KmyKeiba.Downloader.Injection;
using KmyKeiba.JVLink.Wrappers;
using KmyKeiba.Shared;
using log4net.Repository.Hierarchy;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

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
        //args = new[] { "movielist", "94" };
        //args = new[] { "download", "368" };

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
          UpdateTrainingMovieList();
          KillMe();
        }
      }
      else if (command == DownloaderCommand.KillRealTimeHost.GetCommandText())
      {
        var task = GetTask(args[1], DownloaderCommand.KillRealTimeHost);
        if (task != null)
        {
          currentTask = task;
          KillHost();
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

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    public static bool IsShowJraVanNews(ref IntPtr handle, ref string title)
    {
      /*
      var processes = Process.GetProcesses();
      foreach (var process in processes.Where(p => !string.IsNullOrEmpty(p.MainWindowTitle)))
      {
        if (process.MainWindowTitle.Contains("JRA-VANからのお知らせ"))
        {
          var isVisibility = IsWindowVisible(process.MainWindowHandle);
          if (isVisibility)
          {
            handle = process.MainWindowHandle;
          }
          return isVisibility;
        }
      }
      */
      var process = Process.GetCurrentProcess();
      var mainWnd = process.MainWindowHandle;
      if (mainWnd != IntPtr.Zero && IsWindowVisible(mainWnd) && !string.IsNullOrEmpty(process.MainWindowTitle))
      {
        handle = mainWnd;
        title = process.MainWindowTitle;
        return true;
      }
      return false;
    }

    public static void CheckJraVanNews()
    {
      var isFirst = true;
      IntPtr handle = default;
      var title = string.Empty;
      while (IsShowJraVanNews(ref handle, ref title))
      {
        if (isFirst)
        {
          logger.Warn($"JRA-VANからのお知らせを検出 タイトル:{title} ハンドル:{handle}");
          isFirst = false;
        }

        var form = new BlockingForm(handle);
        JVLinkObject.Central.MainWindowHandle = (int)form.Handle;
        Application.Run(form);

        Task.Delay(1000).Wait();
      }
      if (!isFirst)
      {
        logger.Warn("お知らせを閉じました");
      }
    }
  }

  internal static class ResultExtensions
  {
    public static DownloaderError ToDownloaderError(this JVLinkMovieResult result)
    {
      return result switch
      {
        JVLinkMovieResult.ServerError => DownloaderError.ServerError,
        JVLinkMovieResult.InvalidServerResponse => DownloaderError.ServerError,
        JVLinkMovieResult.AuthenticationError => DownloaderError.AuthenticationError,
        JVLinkMovieResult.InternalError => DownloaderError.ServerError,
        JVLinkMovieResult.InvalidKey => DownloaderError.LicenceKeyExpired,
        JVLinkMovieResult.InMaintance => DownloaderError.InMaintance,
        JVLinkMovieResult.NotFound => DownloaderError.TargetsNotExists,
        JVLinkMovieResult.RacingViewerNotAvailable => DownloaderError.RacingViewerNotAvailable,
        JVLinkMovieResult.Succeed => DownloaderError.Succeed,
        _ => DownloaderError.ApplicationRuntimeError,
      };
    }
  }
}