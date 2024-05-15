using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Data;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KmyKeiba.Models.Connection.Connector
{
  internal class JrdbConnector : IConnector
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    private bool _isInitializedObject;

    public DownloadLink Link => DownloadLink.Jrdb;

    public ReactiveProperty<string> JrdbId { get; } = new();
    public ReactiveProperty<string> JrdbPassword { get; } = new();

    public ReactiveProperty<bool> IsAvailable { get; } = new();
    public ReactiveProperty<bool> IsRTAvailable { get; } = new();
    public ReactiveProperty<bool> IsRunning { get; } = new();
    public ReactiveProperty<bool> IsSaving { get; } = new();

    public ReactiveProperty<ConnectorErrorInfo?> Error { get; } = new();

    public JrdbConnector()
    {
    }

    public async Task InitializeAsync(MyContext db)
    {
      if (this._isInitializedObject) return;

      this.JrdbId.Value = ConfigUtil.GetStringValue(SettingKey.JrdbId);
      this.JrdbPassword.Value = ConfigUtil.GetStringValue(SettingKey.JrdbPassword);

      this.JrdbId.Skip(1).Subscribe(async val => await ConfigUtil.SetStringValueAsync(SettingKey.JrdbId, val));
      this.JrdbPassword.Skip(1).Subscribe(async val => await ConfigUtil.SetStringValueAsync(SettingKey.JrdbPassword, val));

      this._isInitializedObject = true;
    }

    public async Task DownloadAsync(DateOnly start, DateOnly end)
    {
      await this.DownloadJrdbAsync(start.ToDateTime(default), end.ToDateTime(default));
    }

    public async Task DownloadRTAsync(DateOnly start, DateOnly end)
    {
      var isDownload = true;
      var isDownloadAfterThursday = (ConfigUtil.GetIntValue(SettingKey.IsDownloadCentralOnThursdayAfterOnly)) != 0;
      if (isDownloadAfterThursday)
      {
        var weekday = DateTime.Today.DayOfWeek;
        isDownload = weekday == DayOfWeek.Friday || weekday == DayOfWeek.Saturday || weekday == DayOfWeek.Sunday;
      }

      if (isDownload)
      {
        logger.Info("JRDBの最新情報取得を開始");
        await this.DownloadJrdbRTAsync(start, start.AddMonths(1));
      }
    }

    private async Task DownloadJrdbAsync(DateTime from, DateTime? to)
    {
      var state = DownloadStatus.Instance;
      var tod = to ?? DateTime.MaxValue;

      logger.Info($"JRDBデータのダウンロードを開始します {from:yyyyMMdd} - {to:yyyyMMdd}");

      try
      {
        state.IsError.Value = false;
        state.IsDownloading.Value = true;
        state.DownloadingLink.Value = DownloadLink.Jrdb;
        state.DownloadingType.Value = DownloadingType.Jrdb;
        await JrdbDownloaderModel.Instance.LoadAsync(from, tod, this.JrdbId.Value, this.JrdbPassword.Value);
      }
      catch (JrdbDownloadException ex)
      {
        logger.Error("JRDBデータのダウンロードに失敗しました", ex);
        state.ErrorMessage.Value = ex.Message;
        state.IsError.Value = true;
      }
      catch (Exception ex)
      {
        logger.Error("JRDBデータのダウンロードに失敗しました（ハンドルされていない例外）", ex);
        state.ErrorMessage.Value = "不明なエラーが発生しました";
        state.IsError.Value = true;
      }
      finally
      {
        state.IsDownloading.Value = false;
      }

      logger.Info("ダウンロード処理を終了します");
    }

    private async Task DownloadJrdbRTAsync(DateOnly date, DateOnly to)
    {
      await this.DownloadJrdbAsync(date.ToDateTime(default), to.ToDateTime(default));
    }
  }
}
