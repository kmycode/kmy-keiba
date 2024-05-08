using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Connection.Connector;
using KmyKeiba.Models.Data;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Connection
{
  internal class DownloadConfig
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public static DownloadConfig Instance => _instance;
    private static readonly DownloadConfig _instance = new();

    private bool _isInitializedObject;

    private ReactiveProperty<bool> IsInitialized => DownloaderModel.Instance.IsInitialized;

    public ReactiveProperty<int> StartYear { get; } = new(1986);
    public ReactiveProperty<int> StartMonth { get; } = new(1);

    public IReadOnlyList<int> StartYearSelection { get; }
    public IReadOnlyList<int> StartMonthSelection { get; }

    public ReactiveProperty<bool> IsDownloadCentral => Connectors.Central.IsAvailable;
    public ReactiveProperty<bool> IsDownloadLocal => Connectors.Local.IsAvailable;
    public ReactiveProperty<bool> IsDownloadJrdb => Connectors.Jrdb.IsAvailable;
    public ReactiveProperty<bool> IsBuildMasterData { get; } = new();

    public ReactiveProperty<bool> IsRTDownloadCentral => Connectors.Central.IsRTAvailable;
    public ReactiveProperty<bool> IsRTDownloadCentralAfterThursdayOnly { get; } = new();
    public ReactiveProperty<bool> IsRTDownloadLocal => Connectors.Local.IsRTAvailable;
    public ReactiveProperty<bool> IsRTDownloadJrdb => Connectors.Jrdb.IsRTAvailable;

    public ReactiveProperty<bool> IsDownloadSlop { get; } = new();
    public ReactiveProperty<bool> IsDownloadBlod { get; } = new();

    public ReactiveProperty<bool> IsBuildExtraData { get; } = new();

    public ReactiveProperty<int> CentralDownloadedYear { get; } = new();
    public ReactiveProperty<int> CentralDownloadedMonth { get; } = new();
    public ReactiveProperty<int> LocalDownloadedYear { get; } = new();
    public ReactiveProperty<int> LocalDownloadedMonth { get; } = new();
    public ReactiveProperty<int> JrdbDownloadedYear { get; } = new();
    public ReactiveProperty<int> JrdbDownloadedMonth { get; } = new();

    private DownloadConfig()
    {
      this.StartYearSelection = Enumerable.Range(1986, DateTime.Now.Year - 1986 + 1).ToArray();
      this.StartMonthSelection = Enumerable.Range(1, 12).ToArray();

      // 設定を保存
      this.IsDownloadCentral.SkipWhile(_ => !this.IsInitialized.Value).Where(_ => !this.IsBuildMasterData.Value).Subscribe(async v => await ConfigUtil.SetBooleanValueAsync(SettingKey.IsDownloadCentral, v));
      this.IsDownloadLocal.SkipWhile(_ => !this.IsInitialized.Value).Where(_ => !this.IsBuildMasterData.Value).Subscribe(async v => await ConfigUtil.SetBooleanValueAsync(SettingKey.IsDownloadLocal, v));
      this.IsDownloadJrdb.SkipWhile(_ => !this.IsInitialized.Value).Where(_ => !this.IsBuildMasterData.Value).Subscribe(async v => await ConfigUtil.SetBooleanValueAsync(SettingKey.IsDownloadJrdb, v));
      this.IsRTDownloadCentralAfterThursdayOnly.SkipWhile(_ => !this.IsInitialized.Value).Subscribe(async v => await ConfigUtil.SetBooleanValueAsync(SettingKey.IsDownloadCentralOnThursdayAfterOnly, v));
      this.IsRTDownloadCentral.SkipWhile(_ => !this.IsInitialized.Value).Subscribe(async v => await ConfigUtil.SetBooleanValueAsync(SettingKey.IsRTDownloadCentral, v));
      this.IsRTDownloadLocal.SkipWhile(_ => !this.IsInitialized.Value).Subscribe(async v => await ConfigUtil.SetBooleanValueAsync(SettingKey.IsRTDownloadLocal, v));
      this.IsRTDownloadJrdb.SkipWhile(_ => !this.IsInitialized.Value).Subscribe(async v => await ConfigUtil.SetBooleanValueAsync(SettingKey.IsRTDownloadJrdb, v));
      this.IsDownloadBlod.SkipWhile(_ => !this.IsInitialized.Value).Subscribe(async v => await ConfigUtil.SetBooleanValueAsync(SettingKey.IsNotDownloadHorseBloods, v));
      this.IsDownloadSlop.SkipWhile(_ => !this.IsInitialized.Value).Subscribe(async v => await ConfigUtil.SetBooleanValueAsync(SettingKey.IsNotDownloadTrainings, v));
      this.IsBuildExtraData.SkipWhile(_ => !this.IsInitialized.Value).Subscribe(async v => await ConfigUtil.SetBooleanValueAsync(SettingKey.IsNotBuildExtraData, v));
    }

    public async Task InitializeAsync(MyContext db)
    {
      if (this._isInitializedObject) return;

      await this.LoadConfigsAsync(db);
      this.InitializeStartDate();

      this._isInitializedObject = true;
    }

    private async Task LoadConfigsAsync(MyContext db)
    {
      var central = await ConfigUtil.GetIntValueAsync(db, SettingKey.LastDownloadCentralDate);
      var local = await ConfigUtil.GetIntValueAsync(db, SettingKey.LastDownloadLocalDate);
      var jrdb = await ConfigUtil.GetIntValueAsync(db, SettingKey.LastDownloadJrdbDate);
      this.CentralDownloadedYear.Value = central / 100;
      this.CentralDownloadedMonth.Value = central % 100;
      this.LocalDownloadedYear.Value = local / 100;
      this.LocalDownloadedMonth.Value = local % 100;
      this.JrdbDownloadedYear.Value = jrdb / 100;
      this.JrdbDownloadedMonth.Value = jrdb % 100;

      this.IsDownloadBlod.Value = await ConfigUtil.GetBooleanValueAsync(db, SettingKey.IsNotDownloadHorseBloods);
      this.IsDownloadSlop.Value = await ConfigUtil.GetBooleanValueAsync(db, SettingKey.IsNotDownloadTrainings);
      this.IsBuildExtraData.Value = await ConfigUtil.GetBooleanValueAsync(db, SettingKey.IsNotBuildExtraData);

      logger.Info($"設定 {nameof(SettingKey.LastDownloadCentralDate)}: {central}, {nameof(SettingKey.LastDownloadLocalDate)}: {local}");

      this.IsDownloadCentral.Value = await ConfigUtil.GetBooleanValueAsync(db, SettingKey.IsDownloadCentral);
      this.IsDownloadLocal.Value = await ConfigUtil.GetBooleanValueAsync(db, SettingKey.IsDownloadLocal);
      this.IsDownloadJrdb.Value = await ConfigUtil.GetBooleanValueAsync(db, SettingKey.IsDownloadJrdb);
      this.IsRTDownloadCentral.Value = await ConfigUtil.GetBooleanValueAsync(db, SettingKey.IsRTDownloadCentral);
      this.IsRTDownloadLocal.Value = await ConfigUtil.GetBooleanValueAsync(db, SettingKey.IsRTDownloadLocal);
      this.IsRTDownloadJrdb.Value = await ConfigUtil.GetBooleanValueAsync(db, SettingKey.IsRTDownloadJrdb);
      this.IsRTDownloadCentralAfterThursdayOnly.Value = await ConfigUtil.GetBooleanValueAsync(db, SettingKey.IsDownloadCentralOnThursdayAfterOnly);

      logger.Info($"設定 {nameof(SettingKey.IsDownloadCentral)}: {IsDownloadCentral.Value}, {nameof(SettingKey.IsDownloadLocal)}: {IsDownloadLocal.Value}, {nameof(SettingKey.IsDownloadJrdb)}: {IsDownloadJrdb.Value} / {nameof(SettingKey.IsRTDownloadCentral)}: {IsRTDownloadCentral.Value}, {nameof(SettingKey.IsRTDownloadLocal)}: {IsRTDownloadLocal.Value}, {nameof(SettingKey.IsRTDownloadJrdb)}: {IsRTDownloadJrdb.Value}");
    }

    private void InitializeStartDate()
    {
      // アプリ起動時デフォルトで設定されるダウンロード開始年月を設定する
      var centralDownloadedYear = (this.IsDownloadCentral.Value && this.CentralDownloadedYear.Value != default) ? this.CentralDownloadedYear.Value : default;
      var centralDownloadedMonth = (this.IsDownloadCentral.Value && this.CentralDownloadedYear.Value != default) ? this.CentralDownloadedMonth.Value : default;
      var localDownloadedYear = (this.IsDownloadLocal.Value && this.LocalDownloadedYear.Value != default) ? this.LocalDownloadedYear.Value : default;
      var localDownloadedMonth = (this.IsDownloadLocal.Value && this.LocalDownloadedYear.Value != default) ? this.LocalDownloadedMonth.Value : default;
      var jrdbDownloadedYear = (this.IsDownloadJrdb.Value && this.JrdbDownloadedYear.Value != default) ? this.LocalDownloadedYear.Value : default;
      var jrdbDownloadedMonth = (this.IsDownloadJrdb.Value && this.JrdbDownloadedYear.Value != default) ? this.LocalDownloadedMonth.Value : default;
      logger.Debug($"保存されていたデータ: 中央競馬DL年月: {centralDownloadedYear * 100 + centralDownloadedMonth}, 地方競馬DL年月: {localDownloadedYear * 100 + localDownloadedMonth}");

      if (centralDownloadedYear != default && localDownloadedYear != default)
      {
        if (centralDownloadedYear < localDownloadedYear)
        {
          this.StartYear.Value = centralDownloadedYear;
          this.StartMonth.Value = centralDownloadedMonth;
        }
        else if (centralDownloadedYear > localDownloadedYear)
        {
          this.StartYear.Value = localDownloadedYear;
          this.StartMonth.Value = localDownloadedMonth;
        }
        else
        {
          this.StartYear.Value = centralDownloadedYear;
          this.StartMonth.Value = Math.Min(centralDownloadedMonth, localDownloadedMonth);
        }
      }
      else if (centralDownloadedYear != default)
      {
        this.StartYear.Value = centralDownloadedYear;
        this.StartMonth.Value = centralDownloadedMonth;
      }
      else if (localDownloadedYear != default)
      {
        this.StartYear.Value = localDownloadedYear;
        this.StartMonth.Value = localDownloadedMonth;
      }
      else
      {
        this.StartYear.Value = 2000;
        this.StartMonth.Value = 1;
      }
      logger.Debug($"画面に反映するデータ: 中央競馬DL年月: {centralDownloadedYear * 100 + centralDownloadedMonth}, 地方競馬DL年月: {localDownloadedYear * 100 + localDownloadedMonth}");
    }
  }
}
