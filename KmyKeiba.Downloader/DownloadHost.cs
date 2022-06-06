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

      isHost = true;
      HostAsync().Wait();
    }

    private static async Task HostAsync()
    {
      while (true)
      {
        using var db = new MyContext();
        var task = await db.DownloaderTasks!.FirstOrDefaultAsync(t => !t.IsStarted);

        if (task != null)
        {
          logger.Info($"新しいタスク {task.Id} を検出");
          Console.WriteLine($"タスク {task.Id} を開始します\n");

          task.IsStarted = true;
          await db.SaveChangesAsync();

          if (task.Command == DownloaderCommand.DownloadRealTimeData)
          {
            currentTask = task;
            StartLoad(task, true);
          }
        }

        CheckShutdown(db, isForce: true);
        Console.WriteLine("[HOST] Waitint new tasks... ");
        await Task.Delay(1000);
      }
    }
  }

  internal class TaskCanceledAndContinueProgramException : Exception
  {
  }
}
