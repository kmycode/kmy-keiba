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
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

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

      // アプリが強制終了した場合に備え、ファイルの更新時刻を定期的にアップデートする
      var liveFileName = Path.Combine(Constrants.AppDataDir, "live");
      File.WriteAllText(liveFileName, DateTime.Now.ToString());
      Observable.Interval(TimeSpan.FromSeconds(30)).Subscribe(_ =>
      {
        try
        {
          File.WriteAllText(liveFileName, DateTime.Now.ToString());
        }
        catch (Exception ex)
        {
          logger.Error("ライブファイルがアップデートできませんでした", ex);
        }
      }).AddTo(this._disposables);
    }

    private Process? ExecuteDownloader(DownloaderCommand command, params string[] arguments)
    {
      // 32bitアプリなので、cmdを経由して起動する
      var info = new ProcessStartInfo
      {
        FileName = "cmd",
        CreateNoWindow = true,    // コンソール画面非表示
#if !DEBUG
#endif
      };
      info.ArgumentList.Add("/c");
      info.ArgumentList.Add(_downloaderPath);
      info.ArgumentList.Add(command.GetCommandText());
      foreach (var arg in arguments)
      {
        info.ArgumentList.Add(arg);
      }
      logger.Info($"ダウンローダのコマンド: {command}");
      logger.Info($"ダウンローダ起動パラメータ: {string.Join(',', arguments)}");

      try
      {
        var p = Process.Start(info);
        logger.Info("ダウンローダを起動しました");
        return p;
      }
      catch (Exception ex)
      {
        logger.Error("ダウンローダの起動に失敗しました", ex);
        throw new DownloaderCommandException(DownloaderError.ProcessNotStarted, ex.Message, ex);
      }
    }

    private async Task<DownloaderTaskData> WaitForFinished(uint taskDataId, Func<DownloaderTaskData, Task>? processing)
    {
      if (taskDataId == default)
      {
        throw new ArgumentException(nameof(taskDataId));
      }

      var tryCount = 0;
      var tryCountForProcessCheck = 600;
      var loopStart = DateTime.Now;
      var lastProcessIdChecked = DateTime.Now;
      var isProcessStarted = false;
      while (true)
      {
        try
        {
          var item = DownloaderTaskDataExtensions.FindOrDefault(taskDataId);
          if (item != null)
          {
            if (item.IsFinished)
            {
              logger.Info($"ダウンローダのタスク {taskDataId} 完了を検知しました");
              if (!item.IsCanceled)
              {
                DownloaderTaskDataExtensions.Remove(item);
                logger.Info("正常終了");
              }
              return item;
            }
            else
            {
              if (item.ProcessId != default)
              {
                isProcessStarted = true;
                lastProcessIdChecked = DateTime.Now;
              }

              if (!isProcessStarted &&
                (loopStart.AddMinutes(5) < DateTime.Now || lastProcessIdChecked.AddMinutes(3) < DateTime.Now))
              {
                if (item.ProcessId == default)
                {
                  item.IsCanceled = true;
                  item.Error = DownloaderError.ConnectionTimeout;
                  DownloaderTaskDataExtensions.Save(item);
                  logger.Warn($"タスク {taskDataId} がタイムアウトしました。プロセスが割り当てられていません");
                  return item;
                }
              }
              if (processing != null)
              {
                await processing(item);
              }

              if (isProcessStarted && ++tryCountForProcessCheck >= 1200)
              {
                var isRunning = await this.IsLaunchingProcessAsync(item.ProcessId);
                if (!isRunning && !item.IsFinished)
                {
                  throw new DownloaderCommandException(DownloaderError.NotRunningDownloader);
                }
                tryCountForProcessCheck = 0;
              }
            }
          }
          else
          {
            throw new NullReferenceException();
          }
        }
        catch (NullReferenceException ex)
        {
          logger.Error($"タスク {taskDataId} が検出できませんでした", ex);
          throw new DownloaderCommandException(DownloaderError.ApplicationRuntimeError);
        }
        catch (DownloaderCommandException ex) when (ex.Error == DownloaderError.NotRunningDownloader)
        {
          logger.Error($"タスク {taskDataId} の実行が確認できませんでした", ex);
          throw new DownloaderCommandException(ex.Error);
        }
        catch (Exception ex)
        {
          logger.Warn($"タスク {taskDataId} の待ち処理で例外が発生しました", ex);

          tryCount++;
          if (tryCount > 1200)
          {
            logger.Error($"タスク {taskDataId} の待ち処理を中止します", ex);
            throw new DownloaderCommandException(DownloaderError.ConnectionTimeout);
          }
        }

        await Task.Delay(50);
      }
    }

    private async Task<bool> IsLaunchingProcessAsync(int processId)
    {
      logger.Info($"プロセス {processId} の起動を確認します");
      var fileName = Path.Combine(Constrants.AppDataDir, $"process_{processId}");
      if (File.Exists(fileName))
      {
        File.Delete(fileName);
      }

      var process = this.ExecuteDownloader(DownloaderCommand.CheckProcessId, processId.ToString());
      if (process != null)
      {
        await process.WaitForExitAsync();

        if (File.Exists(fileName))
        {
          File.Delete(fileName);
          return true;
        }
      }

      return false;
    }

    public async Task InitializeAsync()
    {
      logger.Info("ダウンローダとの接続初期化を開始します");

      if (File.Exists(Constrants.ShutdownFilePath))
      {
        logger.Debug("シャットダウンファイルを削除");
        File.Delete(Constrants.ShutdownFilePath);
      }

      this.ExecuteDownloader(DownloaderCommand.Initialization, Constrants.ApplicationVersion);

      while (!File.Exists(Constrants.DatabasePath))
      {
        await Task.Delay(100);
      }
      logger.Info("データベースファイルの存在を確認");

      var tryCount = 0;

      var canConnect = false;
      while (!canConnect)
      {
        try
        {
          // downloaderがtaskを完了させるのを待つ
          var task = DownloaderTaskDataExtensions.ToArray().FirstOrDefault(t => t.Command == DownloaderCommand.Initialization);
          if (task != null)
          {
            DownloaderTaskDataExtensions.Remove(task);
            logger.Debug("接続確認");

            if (task.Error == DownloaderError.InvalidVersion)
            {
              logger.Error($"ダウンローダとアプリのバージョンが異なります アプリのバージョン: {Constrants.ApplicationVersion}");
              throw new DownloaderCommandException(task.Error, task.Result);
            }

            canConnect = true;
          }
        }
        catch (DownloaderCommandException ex)
        {
          logger.Fatal("初期化でエラーが発生しました", ex);

          if (ex.InnerException == null)
          {
            throw new DownloaderCommandException(ex);
          }
          else
          {
            throw new DownloaderCommandException(ex, ex.InnerException);
          }
        }
        catch (Exception ex)
        {
          logger.Error("初期化でエラーが発生しました", ex);

          // 最新task取得の時点でデータベースが初期化されていない可能性があるので、そのエラーを受ける
          tryCount++;
          if (tryCount > 600)
          {
            logger.Fatal("初期化エラーのため初期化を中止します");
            throw new DownloaderCommandException(DownloaderError.ConnectionTimeout);
          }

          await Task.Delay(100);
        }
      }
    }

    public async Task<bool> DownloadAsync(string link, string type, int startYear, int startMonth, Func<DownloaderTaskData, Task>? progress, int startDay = 0)
    {
      return await this.DownloadAsync(link, type, new DateOnly(startYear, startMonth, Math.Max(1, startDay)), progress, false);
    }

    public async Task<bool> DownloadRTAsync(string link, DateOnly date, Func<DownloaderTaskData, Task>? progress)
    {
      return await this.DownloadAsync(link, string.Empty, date, progress, true);
    }

    private async Task<bool> DownloadAsync(string link, string type, DateOnly start, Func<DownloaderTaskData, Task>? progress, bool isRealTime)
    {
      if (link == "central")
      {
        var serviceStatus = JVLinkServiceWatcher.CheckAndTryStart();
        if (serviceStatus == JVLinkServiceResult.StartFailed)
        {
          throw new DownloaderCommandException(DownloaderError.NotRunningJVLinkAgent);
        }
        else if (serviceStatus == JVLinkServiceResult.NotFound)
        {
          throw new DownloaderCommandException(DownloaderError.NotInstalledCom);
        }
      }

      logger.Info($"ダウンロードを開始します RT:{isRealTime} リンク:{link} タイプ:{type} 開始年月:{start}");

      if (isRealTime ? this.IsRTBusy.Value : this.IsBusy.Value)
      {
        logger.Warn("すでにダウンロード中です");
        throw new InvalidOperationException();
      }

      try
      {
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
            Parameter = $"{start.Year},{start.Month}{start.Day:00},{link},{type}",
          };
          this.currentTask.Value = task;
        }

        await this.PublishTaskAsync(task, progress);
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

    public async Task OpenMovieAsync(MovieType type, DownloadLink link, string key)
    {
      var task = new DownloaderTaskData
      {
        Command = DownloaderCommand.OpenMovie,
        Parameter = key + "," + (short)type + "," + (link == DownloadLink.Central ? "central" : "local"),
      };
      await this.PublishTaskAsync(task);
    }

    public async Task UpdateMovieListAsync(string horseKey)
    {
      var task = new DownloaderTaskData
      {
        Command = DownloaderCommand.OpenMovieList,
        Parameter = horseKey,
      };
      await this.PublishTaskAsync(task);
    }

    private async Task PublishTaskAsync(DownloaderTaskData task, Func<DownloaderTaskData, Task>? progressing = null)
    {
      DownloaderTaskDataExtensions.Add(task);

      logger.Info($"タスク発行 ID: {task.Id}, コマンド: {task.Command}, パラメータ: {task.Parameter}");

      this.ExecuteDownloader(task.Command, task.Id.ToString());
      var result = await WaitForFinished(task.Id, progressing);
      if (result.Error != DownloaderError.Succeed)
      {
        logger.Error($"タスクがエラーを返しました {task.Id} {result.Error} {result.Result}");
        throw new DownloaderCommandException(result.Error, result.Result);
      }
      if (result.IsCanceled)
      {
        logger.Warn($"タスクがキャンセルされました {task.Id}");
        throw new DownloaderCommandException(DownloaderError.Canceled);
      }
    }

    private async Task OpenConfigAsync(DownloaderCommand command)
    {
      if (this.IsBusy.Value)
      {
        logger.Warn("すでにダウンロード中です");
        throw new InvalidOperationException();
      }

      try
      {
        this.currentTask.Value = new DownloaderTaskData
        {
          Command = command,
        };
        await this.PublishTaskAsync(this.currentTask.Value);
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

    public async Task UnzipLhaAsync(string path, string distDir)
    {
      var process = this.ExecuteDownloader(DownloaderCommand.Unlha, path, distDir);
      if (process != null)
      {
        await process.WaitForExitAsync();
      }
      else
      {
        throw new Exception("LHA解凍に失敗しました");
      }
    }

    public async Task CancelCurrentTaskAsync()
    {
      if (!this.IsBusy.Value)
      {
        logger.Warn("キャンセルしようとしましたが、そのような状態ではないので処理を中止しました");
        return;
      }

      try
      {
        DownloaderTaskDataExtensions.Save(this.currentTask.Value!);

        logger.Info($"タスク {this.currentTask.Value!.Id} をキャンセルしました");
      }
      catch (Exception ex)
      {
        logger.Warn($"タスク {this.currentTask.Value?.Id} のキャンセルに失敗しました", ex);
      }

      this.currentTask.Value = null;
    }

    public void Dispose()
    {
      logger.Info("シャットダウンファイルを作成します");
      File.WriteAllText(Constrants.ShutdownFilePath, string.Empty);
      this._disposables.Dispose();
    }
  }

  public class DownloaderCommandException : Exception
  {
    public DownloaderError Error { get; }

    public DownloaderCommandException(DownloaderError error) : base("ダウンローダとの連携でエラーが発生しました。エラーコード=" + error)
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

    public DownloaderCommandException(DownloaderCommandException original) : this(original.Error, original.Message)
    {
    }

    public DownloaderCommandException(DownloaderCommandException original, Exception inner) : this(original.Error, original.Message, inner)
    {
    }
  }

  public enum MovieType
  {
    Race = 0,
    Paddock = 1,
    MultiCameras = 2,
    Patrol = 3,
    Training = 11,
  }
}
