using KmyKeiba.Data.Db;
using KmyKeiba.Models.Data;
using KmyKeiba.Shared;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Connection
{
  internal class DownloaderConnector : IDisposable
  {
    private readonly CompositeDisposable _disposables = new();
    private readonly ReactiveProperty<DownloaderTaskData?> currentTask = new();
    private readonly ReactiveProperty<DownloaderTaskData?> currentRTTask = new();
    private readonly string _downloaderPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, @"downloader\\KmyKeiba.Downloader.exe");

    public static DownloaderConnector Instance => _default ??= new();
    private static DownloaderConnector? _default;

    public ReactiveProperty<bool> IsConnecting { get; } = new();

    public ReactiveProperty<bool> IsBusy { get; }

    public ReactiveProperty<bool> IsRTBusy { get; }

    public bool IsExistsDatabase
    {
      get
      {
        return File.Exists(Constrants.DatabasePath);
      }
    }

    private DownloaderConnector()
    {
      this.IsBusy = this.currentTask.Select(t => t != null).ToReactiveProperty().AddTo(this._disposables);
      this.IsRTBusy = this.currentRTTask.Select(t => t != null).ToReactiveProperty().AddTo(this._disposables);
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

    private async Task<DownloaderTaskData> WaitForFinished(uint taskDataId, Func<DownloaderTaskData, Task>? processing)
    {
      if (taskDataId == default)
      {
        throw new ArgumentException();
      }

      var tryCount = 0;
      while (true)
      {
        try
        {
          // いちいち作らないと、外部によって更新されたデータが取得できなくなる（DbContext内部でオブジェクトをキャッシュしてる？）
          using var db = new MyContext();
          var item = await db.DownloaderTasks!.FindAsync(taskDataId);
          if (item != null)
          {
            if (item.IsFinished)
            {
              if (!item.IsCanceled)
              {
                db.Remove(item);
                await db.SaveChangesAsync();
              }
              return item;
            }
            else
            {
              if (processing != null)
              {
                await processing(item);
              }
            }
          }
        }
        catch
        {
          // TODO: log
          tryCount++;
          if (tryCount > 1200)
          {
            throw new DownloaderCommandException(DownloaderError.ConnectionTimeout);
          }
        }

        await Task.Delay(50);
      }
    }

    public async Task InitializeAsync()
    {
      if (File.Exists(Constrants.ShutdownFilePath))
      {
        File.Delete(Constrants.ShutdownFilePath);
      }

      this.ExecuteDownloader(DownloaderCommand.Initialization, Constrants.ApplicationVersion);

      while (!File.Exists(Constrants.DatabasePath))
      {
        await Task.Delay(100);
      }

      var tryCount = 0;

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
              throw new DownloaderCommandException(task.Error, task.Result);
            }

            canConnect = true;
          }
        }
        catch (DownloaderCommandException ex)
        {
          throw ex;
        }
        catch
        {
          // TODO: logs
          // 最新task取得の時点でデータベースが初期化されていない可能性があるので、そのエラーを受ける
          tryCount++;
          if (tryCount > 600)
          {
            throw new DownloaderCommandException(DownloaderError.ConnectionTimeout);
          }

          await Task.Delay(100);
        }
      }
    }

    public async Task<bool> DownloadAsync(string link, string type, int startYear, int startMonth, Func<DownloaderTaskData, Task>? progress, int startDay = 0)
    {
      return await this.DownloadAsync(link, type, new DateOnly(startYear, startMonth, startDay), progress, false);
    }

    public async Task<bool> DownloadRTAsync(string link, DateOnly date, Func<DownloaderTaskData, Task>? progress)
    {
      return await this.DownloadAsync(link, string.Empty, date, progress, true);
    }

    private async Task<bool> DownloadAsync(string link, string type, DateOnly start, Func<DownloaderTaskData, Task>? progress, bool isRealTime)
    {
      if (isRealTime ? this.IsRTBusy.Value : this.IsBusy.Value)
      {
        throw new InvalidOperationException();
      }

      try
      {
        using var db = new MyContext();
        DownloaderTaskData task;
        if (isRealTime)
        {
          task = new DownloaderTaskData
          {
            Command = DownloaderCommand.DownloadRealTimeData,
            Parameter = $"{start:yyyyMMdd},{link},1,0",
          };
          this.currentRTTask.Value = task;
        }
        else
        {
          task = new DownloaderTaskData
          {
            Command = DownloaderCommand.DownloadSetup,
            Parameter = $"{start.Year},{start.Month},{link},{type}",
          };
          this.currentTask.Value = task;
        }
        await db.DownloaderTasks!.AddAsync(task);
        await db.SaveChangesAsync();

        this.ExecuteDownloader(task.Command, task.Id.ToString());
        var result = await this.WaitForFinished(task.Id, progress);

        if (result.Error != DownloaderError.Succeed)
        {
          throw new DownloaderCommandException(result.Error, result.Result);
        }
        if (result.IsCanceled)
        {
          // return false;
          throw new DownloaderCommandException(DownloaderError.Canceled);
        }
      }
      finally
      {
        if (isRealTime)
        {
          this.currentRTTask.Value = null;
        }
        else
        {
          this.currentTask.Value = null;
        }
      }

      return true;
    }

    private async Task OpenConfigAsync(DownloaderCommand command)
    {
      if (this.IsBusy.Value)
      {
        throw new InvalidOperationException();
      }

      try
      {
        using var db = new MyContext();
        var task = new DownloaderTaskData
        {
          Command = command,
        };
        this.currentTask.Value = task;
        await db.DownloaderTasks!.AddAsync(task);
        await db.SaveChangesAsync();

        this.ExecuteDownloader(task.Command, task.Id.ToString());
        var result = await WaitForFinished(task.Id, null);
        if (result.Error != DownloaderError.Succeed)
        {
          throw new DownloaderCommandException(result.Error, result.Result);
        }
      }
      finally
      {
        this.currentTask.Value = null;
      }
    }

    public async Task OpenJvlinkConfigAsync()
    {
      await this.OpenConfigAsync(DownloaderCommand.OpenJvlinkConfigs);
    }

    public async Task OpenNvlinkConfigAsync()
    {
      await this.OpenConfigAsync(DownloaderCommand.OpenNvlinkConfigs);
    }

    public async Task CancelCurrentTaskAsync()
    {
      if (!this.IsBusy.Value)
      {
        return;
      }

      using var db = new MyContext();
      db.DownloaderTasks!.Attach(this.currentTask.Value!);
      this.currentTask.Value!.IsCanceled = true;
      this.currentTask.Value!.IsFinished = true;
      await db.SaveChangesAsync();

      this.currentTask.Value = null;
    }

    public void Dispose()
    {
      File.WriteAllText(Constrants.ShutdownFilePath, string.Empty);
      this._disposables.Dispose();
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
