using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Data;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KmyKeiba.Models.Connection.Connector
{
  public abstract class LinkConnectorBase : IConnector
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public abstract DownloadLink Link { get; }

    protected virtual bool CanDownloadRT => true;

    public ReactiveProperty<bool> IsAvailable { get; } = new();
    public ReactiveProperty<bool> IsRTAvailable { get; } = new();

    public ReactiveProperty<bool> IsRunning { get; } = new();

    public ReactiveProperty<bool> IsSaving { get; } = new();

    public ReactiveProperty<ConnectorErrorInfo?> Error { get; } = new();

    public async Task DownloadAsync(DateOnly start, DateOnly end)
    {
      await this.DownloadAsync(start);
    }

    public async Task DownloadRTAsync(DateOnly start, DateOnly end)
    {
      await this.DownloadRTAsync(start);
    }

    public async Task ResumeDownloadAsync()
    {
      await this.DownloadAsync(DateOnly.FromDateTime(DateTime.Today), isResume: true);
    }

    protected Task DownloadAsync()
    {
      var config = DownloadConfig.Instance;

      return this.DownloadAsync(new DateOnly(config.StartYear.Value, config.StartMonth.Value, 1));
    }

    protected async Task DownloadAsync(DateOnly start, int startDay = 1, bool isResume = false)
    {
      await this.UpdateDownloadYearConfigsAsync();

      var config = DownloadConfig.Instance;
      var state = DownloadStatus.Instance;
      var link = this.Link;

      logger.Info($"通常データのダウンロードを開始します リンク: {link}");
      logger.Debug($"現在のダウンロード状態: {state.IsDownloading.Value}");

      state.IsError.Value = false;
      state.IsDownloading.Value = true;

      var downloader = DownloaderConnector.Instance;
      var linkName = link == DownloadLink.Central ? "central" : "local";

      var startYear = start.Year;
      var startMonth = start.Month;
      logger.Info($"開始年月: {startYear}/{startMonth}");

      try
      {
        state.DownloadingLink.Value = link;

        if (isResume)
        {
          await downloader.ResumeTaskAsync(state.OnDownloadProgress);
        }
        else
        {
          await downloader.DownloadAsync(linkName, "race", startYear, startMonth, state.OnDownloadProgress, startDay);
        }
        DownloaderModel.Instance.OnRacesUpdated();

        logger.Info("通常データのダウンロード完了");
      }
      catch (DownloaderCommandException ex)
      {
        logger.Error("通常データのダウンロードに失敗しました。ダウンローダがエラーを返しました", ex);
        state.ErrorMessage.Value = ex.Error.GetErrorText();
        state.IsError.Value = true;
      }
      catch (Exception ex)
      {
        logger.Error("通常データのダウンロードに失敗しました", ex);
        state.ErrorMessage.Value = ex.Message;
        state.IsError.Value = true;
      }
      finally
      {
        state.IsDownloading.Value = false;
        state.LoadingProcess.Value = LoadingProcessValue.Unknown;

        if (!isResume)
        {
          config.StartMonth.Value = startMonth;
          config.StartYear.Value = startYear;
        }

        state.HasProcessingProgress.Value = false;
      }

      logger.Info("ダウンロード処理を終了します");
    }

    protected abstract Task UpdateDownloadYearConfigsAsync();

    protected async Task DownloadRTAsync(DateOnly date)
    {
      if (!this.CanDownloadRT) return;

      var config = DownloadConfig.Instance;
      var state = DownloadStatus.Instance;
      var link = this.Link;

      logger.Info($"RTデータのダウンロードを開始します リンク: {link}");
      logger.Debug($"現在のダウンロード状態: {state.IsRTDownloading.Value}");

      state.IsRTError.Value = false;
      state.IsRTDownloading.Value = true;
      state.RTDownloadingLink.Value = link;

      var downloader = DownloaderConnector.Instance;
      var linkName = link == DownloadLink.Central ? "central" : "local";
      logger.Info($"開始年月: {date:yyyy/MM/dd}");

      try
      {
        await downloader.DownloadRTAsync(linkName, date, state.OnRTDownloadProgress);
        DownloaderModel.Instance.OnRacesUpdated();

        logger.Info("RTデータのダウンロード完了");
      }
      catch (DownloaderCommandException ex)
      {
        logger.Error("RTデータのダウンロードに失敗しました。ダウンローダがエラーを返しました", ex);
        state.RTErrorMessage.Value = ex.Error.GetErrorText();
        state.IsRTError.Value = true;
      }
      catch (Exception ex)
      {
        logger.Error("通常データのダウンロードに失敗しました", ex);
        state.RTErrorMessage.Value = ex.Message;
        state.IsRTError.Value = true;
      }
      finally
      {
        state.IsRTDownloading.Value = false;
        state.RTLoadingProcess.Value = LoadingProcessValue.Unknown;
      }

      logger.Info("ダウンロード処理を終了します");
    }
  }
}
