using KmyKeiba.Data.Db;
using KmyKeiba.Shared;
using Microsoft.EntityFrameworkCore;
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
    private static void StartRTHost()
    {
      if (File.Exists(Constrants.RTHostFilePath))
      {
        var processIdStr = File.ReadAllText(Constrants.RTHostFilePath).Trim();
        int.TryParse(processIdStr, out var processId);

        try
        {
          var oldHost = Process.GetProcessById(processId);
          if (oldHost.MainModule?.ModuleName?.Contains("KmyKeiba.Downloader") == true)
          {
            logger.Warn($"すでにホストプログラム {processId} が起動しています。処理を中止します");
            KillMe();
          }
        }
        catch
        {
          logger.Warn($"前回のホストプログラム {processId} の起動を検出できませんでした");
        }
      }
      File.WriteAllText(Constrants.RTHostFilePath, Process.GetCurrentProcess().Id.ToString());

      logger.Info("ホストプロセスを開始します");
      isHost = true;
      HostAsync();
    }

    private static void HostAsync()
    {
      while (true)
      {
        try
        {
          using var db = new MyContext();
          var task = db.DownloaderTasks!.FirstOrDefault(t => t.Command == DownloaderCommand.DownloadRealTimeData &&
            !t.IsStarted && !t.IsCanceled && !t.IsFinished);

          if (task != null)
          {
            void SetAsCurrentTask()
            {
              task.IsStarted = true;
              task.ProcessId = Environment.ProcessId;
              db.SaveChanges();
              currentTask = task;

              logger.Info($"新しいタスク {task.Id} を検出");
              Console.WriteLine($"タスク {task.Id} を開始します\n");
            }

            if (task.Command == DownloaderCommand.DownloadRealTimeData)
            {
              SetAsCurrentTask();
              StartLoad(task, true);
            }
          }

          CheckCurrentTasks(db);

          CheckShutdown(db, isForce: true);
          Console.WriteLine("[HOST] Waiting new tasks... ");
          Task.Delay(1000).Wait();
        }
        catch (Exception ex)
        {
          logger.Error("ホストプロセスでエラーが発生しました", ex);
        }
      }
    }

    private static void CheckCurrentTasks(MyContext db)
    {
      var tasks = db.DownloaderTasks!.Where(t => !t.IsFinished && t.ProcessId != default).ToArray();

      var isSave = false;
      foreach (var task in tasks)
      {
        try
        {
          // プロセスが存在しなければ例外
          Process.GetProcessById(task.ProcessId);
        }
        catch
        {
          task.IsCanceled = true;
          task.IsFinished = true;
          task.Error = DownloaderError.ApplicationRuntimeError;
          isSave = true;
          logger.Warn($"タスク {task.Id} は、担当プロセス {task.ProcessId} が見つからないのでキャンセルしました");
        }
      }
      if (isSave)
      {
        db.SaveChanges();
      }
    }

    private static void KillHost()
    {
      if (currentTask == null)
      {
        return;
      }

      if (File.Exists(Constrants.RTHostFilePath))
      {
        var processIdStr = File.ReadAllText(Constrants.RTHostFilePath).Trim();
        int.TryParse(processIdStr, out var processId);

        try
        {
          var oldHost = Process.GetProcessById(processId);
          if (oldHost.MainModule?.ModuleName?.Contains("KmyKeiba.Downloader") == true)
          {
            oldHost.Kill();
          }
        }
        catch
        {
          logger.Warn($"前回のホストプログラム {processId} の起動を検出できませんでした");
        }
      }

      SetTask(currentTask, t =>
      {
        t.IsFinished = true;
      });
    }
  }

  internal class TaskCanceledAndContinueProgramException : Exception
  {
  }
}
