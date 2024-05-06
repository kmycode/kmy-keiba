using KmyKeiba.Data.Db;
using KmyKeiba.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Downloader
{
  internal partial class Program
  {
    public static void CheckShutdown(bool isForce = false)
    {
      if (!isCheckShutdown)
      {
        return;
      }

      if (File.Exists(Constrants.ShutdownFilePath))
      {
        logger.Warn("シャットダウンファイルが存在したのでシャットダウンします");
        KillMe();
        //Environment.Exit(0);
      }

      if (currentTask != null)
      {
        var liveFileName = Path.Combine(Constrants.AppDataDir, "live");
#if DEBUG
        if (!File.Exists(liveFileName))
#else
        if (!File.Exists(liveFileName) || File.GetLastWriteTime(liveFileName) < DateTime.Now.AddMinutes(-5))
#endif
        {
          logger.Warn("ライブファイルが存在しないか、更新時刻を超過していたのでシャットダウンします");
          KillMe();
        }
      }

      if (currentTask != null)
      {
        var task = DownloaderTaskDataExtensions.FindOrDefault(currentTask.Id);
        if (task != null && task.IsCanceled)
        {
          if (!isHost || isForce)
          {
            logger.Warn("タスクが存在しないか、キャンセルされていたのでシャットダウンします");
            KillMe();
            //Environment.Exit(0);
          }
          else
          {
            logger.Warn("タスクが存在しないか、キャンセルされていたので処理を中止します");
            throw new TaskCanceledAndContinueProgramException();
          }
        }
      }
      else
      {
        if (!isHost || isForce)
        {
          logger.Warn("現在のタスクが見つかりませんでした。シャットダウンします");
          KillMe();
          //Environment.Exit(0);
        }
        else
        {
          throw new TaskCanceledAndContinueProgramException();
        }
      }
    }

    private static void KillMe(bool isForce = false)
    {
      logger.Info("自殺を開始します");

      if (!isHost && !isForce && (currentTask == null || !currentTask.Parameter.Contains("local")))
      {
        logger.Info("中央競馬：正常終了");
        Environment.Exit(0);
        return;
      }

      var myProcess = Process.GetCurrentProcess();
      var myProcessNumber = myProcess?.Id ?? 0;

      logger.Info($"自分を殺すプロセスを開始します {myProcessNumber} Name:{myProcess?.ProcessName}");
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

    public static async Task RestartProgramAsync(bool isIncrement, bool isForce = false)
    {
      logger.Info($"プログラムを再起動します インクリメント:{isIncrement}");

      var myProcess = Process.GetCurrentProcess();
      var myProcessNumber = myProcess?.Id ?? 0;

      if (currentTask == null)
      {
        logger.Warn("現在のタスクが見つかりませんでした");
        if (!isHost || isForce)
        {
          KillMe();
        }
        else
        {
          throw new TaskCanceledAndContinueProgramException();
        }
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

        if (!isHost || isForce)
        {
          KillMe();
        }
        else
        {
          throw new TaskCanceledAndContinueProgramException();
        }
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

          currentTask.Parameter = $"{year},{month},{parameters[2]},{mode},{string.Join(',', parameters.Skip(4))}";
          currentTask.IsStarted = false;
          currentTask.ProcessId = default;
          DownloaderTaskDataExtensions.Save(currentTask);

          logger.Info(currentTask.Parameter);
          CheckShutdown();
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

        currentTask.Parameter = $"{parameters[0]},{parameters[1]},{kind},{skip + 1},{string.Join(',', parameters.Skip(4))}";
        currentTask.IsStarted = false;
        currentTask.ProcessId = default;
        DownloaderTaskDataExtensions.Save(currentTask);

        logger.Info(currentTask.Parameter);
        CheckShutdown();

        shellParams.Add(currentTask.Id.ToString());
        shellParams.Add(myProcessNumber.ToString());
        shellParams.Add((retryDownloadCount + 1).ToString());
      }

      try
      {
        logger.Info($"新しいプロセスを開始します");
        var info = new ProcessStartInfo
        {
          FileName = selfPath,
          ArgumentList =
          {
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

        currentTask.IsFinished = true;
        currentTask.Error = DownloaderError.ApplicationRuntimeError;
        DownloaderTaskDataExtensions.Save(currentTask);

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
  }
}
