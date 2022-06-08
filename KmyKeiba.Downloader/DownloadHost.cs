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
      HostAsync().Wait();
    }

    private static async Task HostAsync()
    {
      while (true)
      {
        try
        {
          using var db = new MyContext();
          var task = await db.DownloaderTasks!.FirstOrDefaultAsync(t => t.Command == DownloaderCommand.DownloadRealTimeData &&
          !t.IsStarted && !t.IsCanceled && !t.IsFinished);

          if (task != null)
          {
            async Task SetAsCurrentTaskAsync()
            {
              task.IsStarted = true;
              task.ProcessId = Environment.ProcessId;
              await db.SaveChangesAsync();
              currentTask = task;

              logger.Info($"新しいタスク {task.Id} を検出");
              Console.WriteLine($"タスク {task.Id} を開始します\n");
            }

            if (task.Command == DownloaderCommand.DownloadRealTimeData)
            {
              await SetAsCurrentTaskAsync();
              StartLoad(task, true);
            }
          }

          await CheckCurrentTasksAsync(db);

          CheckShutdown(db, isForce: true);
          Console.WriteLine("[HOST] Waiting new tasks... ");
          await Task.Delay(1000);
        }
        catch (Exception ex)
        {
          logger.Error("ホストプロセスでエラーが発生しました", ex);
        }
      }
    }

    private static async Task CheckCurrentTasksAsync(MyContext db)
    {
      var tasks = await db.DownloaderTasks!.Where(t => !t.IsFinished && t.ProcessId != default).ToArrayAsync();

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
          task.Error = DownloaderError.ApplicationRuntimeError;
          isSave = true;
          logger.Warn($"タスク {task.Id} は、担当プロセス {task.ProcessId} が見つからないのでキャンセルしました");
        }
      }
      if (isSave)
      {
        await db.SaveChangesAsync();
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
