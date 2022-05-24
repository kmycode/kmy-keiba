using KmyKeiba.Data.Db;
using KmyKeiba.Models.Data;
using KmyKeiba.Shared;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Connection
{
  internal class DownloaderConnector
  {
    private readonly string _downloaderPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, @"downloader\\KmyKeiba.Downloader.exe");

    public static DownloaderConnector Default => _default ??= new();
    private static DownloaderConnector? _default;

    public ReactiveProperty<bool> IsConnecting { get; } = new();

    public bool IsExistsDatabase
    {
      get
      {
        return !File.Exists(Constrants.DatabasePath);
      }
    }

    private void ExecuteDownloader(DownloaderCommand command, params string[] arguments)
    {

      // 32bitアプリなので、cmdを経由して起動する
      var info = new ProcessStartInfo
      {
        FileName = "cmd",
#if !DEBUG
        CreateNoWindow = true,    // コンソール画面非表示
#endif
      };
      info.ArgumentList.Add("/c");
      info.ArgumentList.Add(_downloaderPath);
      info.ArgumentList.Add(command.GetCommandText());
      foreach (var arg in arguments)
      {
        info.ArgumentList.Add(arg);
      }

      try
      {
        Process.Start(info);
      }
      catch (Exception ex)
      {
        // TODO: logs
        throw new DownloaderCommandException(DownloaderError.ProcessNotStarted, ex.Message, ex);
      }
    }

    private async Task<DownloaderTaskData> WaitForFinished(MyContext db, uint taskDataId)
    {
      if (taskDataId == default)
      {
        throw new ArgumentException();
      }

      while (true)
      {
        try
        {
          var item = await db.DownloaderTasks!.FindAsync(taskDataId);
          if (item != null && item.Id == taskDataId && item.IsFinished)
          {
            db.Remove(item);
            await db.SaveChangesAsync();
            return item;
          }
          await Task.Delay(50);
        }
        catch
        {
          // TODO: log
        }
      }
    }

    public async Task InitializeAsync()
    {
      this.ExecuteDownloader(DownloaderCommand.Initialization, Constrants.ApplicationVersion);

      while (!File.Exists(Constrants.DatabasePath))
      {
        await Task.Delay(100);
      }

      var canConnect = false;
      while (!canConnect)
      {
        try
        {
          using var db = new MyContext();

          // downloaderがtaskを完了させるのを待つ
          var task = await db.DownloaderTasks!.FirstOrDefaultAsync(t => t.Command == DownloaderCommand.Initialization);
          if (task != null)
          {
            db.DownloaderTasks!.Remove(task);
            await db.SaveChangesAsync();

            if (task.Error == DownloaderError.InvalidVersion)
            {
              throw new DownloaderCommandException(task.Error);
            }

            canConnect = true;
          }
        }
        catch
        {
          await Task.Delay(100);
        }
      }
    }
  }

  public class DownloaderCommandException : Exception
  {
    public DownloaderError Error { get; }

    public DownloaderCommandException(DownloaderError error)
    {
      this.Error = error;
    }

    public DownloaderCommandException(DownloaderError error, string message) : base(message)
    {
      this.Error = error;
    }

    public DownloaderCommandException(DownloaderError error, string message, Exception inner) : base(message, inner)
    {
      this.Error = error;
    }
  }
}
