﻿using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Data;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Connection
{
  internal class DownloaderModel : IDisposable
  {
    private readonly CompositeDisposable _disposables = new();

    public ReactiveProperty<bool> IsInitializationError { get; } = new();

    public ReactiveProperty<bool> IsError { get; } = new();

    public ReactiveProperty<bool> IsRTError { get; } = new();

    public ReactiveProperty<bool> IsBusy => DownloaderConnector.Default.IsBusy;

    public ReactiveProperty<bool> IsRTBusy => DownloaderConnector.Default.IsRTBusy;

    public ReactiveProperty<bool> IsDownloading { get; } = new();

    public ReactiveProperty<bool> IsProcessing { get; } = new();

    public ReactiveProperty<bool> IsRTDownloading { get; } = new();

    public ReactiveProperty<bool> IsRTProcessing { get; } = new();

    public ReactiveProperty<string> ErrorMessage { get; } = new();

    public ReactiveProperty<string> RTErrorMessage { get; } = new();

    public ReactiveProperty<bool> IsInitialized { get; } = new();

    public ReactiveProperty<int> StartYear { get; } = new(1986);

    public ReactiveProperty<int> StartMonth { get; } = new(1);

    public IReadOnlyList<int> StartYearSelection { get; }

    public IReadOnlyList<int> StartMonthSelection { get; }

    public ReactiveProperty<DownloadingType> DownloadingType { get; } = new();

    public ReactiveProperty<DownloadLink> DownloadingLink { get; } = new();

    public ReactiveProperty<DownloadLink> RTDownloadingLink { get; } = new();

    public ReactiveProperty<int> DownloadingYear { get; } = new();

    public ReactiveProperty<int> DownloadingMonth { get; } = new();

    public ReactiveProperty<int> CentralDownloadedYear { get; } = new();

    public ReactiveProperty<int> CentralDownloadedMonth { get; } = new();

    public ReactiveProperty<int> LocalDownloadedYear { get; } = new();

    public ReactiveProperty<int> LocalDownloadedMonth { get; } = new();

    public ReactiveProperty<bool> IsDownloadCentral { get; } = new();

    public ReactiveProperty<bool> IsDownloadLocal { get; } = new();

    public ReactiveProperty<LoadingProcessValue> LoadingProcess { get; } = new();

    public ReactiveProperty<LoadingProcessValue> RTLoadingProcess { get; } = new();

    public ReactiveProperty<DownloadingDataspec> RTDownloadingDataspec { get; } = new();

    public ReactiveProperty<ProcessingStep> ProcessingStep { get; } = new();

    public ReactiveProperty<ProcessingStep> RTProcessingStep { get; } = new();

    public ReactiveProperty<DownloadMode> Mode { get; } = new();

    public DownloaderModel()
    {
      this.StartYearSelection = Enumerable.Range(1986, DateTime.Now.Year - 1986 + 1).ToArray();
      this.StartMonthSelection = Enumerable.Range(1, 12).ToArray();

      // 設定を保存
      this.IsDownloadCentral.Where(_ => this.IsInitialized.Value).Subscribe(async v => await ConfigUtil.SetIntValueAsync(SettingKey.IsDownloadCentral, v ? 1 : 0)).AddTo(this._disposables);
      this.IsDownloadLocal.Where(_ => this.IsInitialized.Value).Subscribe(async v => await ConfigUtil.SetIntValueAsync(SettingKey.IsDownloadLocal, v ? 1 : 0)).AddTo(this._disposables);
    }

    public async Task<bool> InitializeAsync()
    {
      var downloader = DownloaderConnector.Default;
      var isFirst = !downloader.IsExistsDatabase;

      try
      {
        await downloader.InitializeAsync();
        this.IsInitialized.Value = true;

        using var db = new MyContext();
        var central = await ConfigUtil.GetIntValueAsync(db, SettingKey.LastDownloadCentralDate);
        var local = await ConfigUtil.GetIntValueAsync(db, SettingKey.LastDownloadLocalDate);
        this.CentralDownloadedYear.Value = central / 100;
        this.CentralDownloadedMonth.Value = central % 100;
        this.LocalDownloadedYear.Value = local / 100;
        this.LocalDownloadedMonth.Value = local % 100;

        var isCentral = await ConfigUtil.GetIntValueAsync(db, SettingKey.IsDownloadCentral);
        var isLocal = await ConfigUtil.GetIntValueAsync(db, SettingKey.IsDownloadLocal);
        this.IsDownloadCentral.Value = isCentral != 0;
        this.IsDownloadLocal.Value = isLocal != 0;

        // アプリ起動時デフォルトで設定されるダウンロード開始年月を設定する
        var centralDownloadedYear = (this.IsDownloadCentral.Value && this.CentralDownloadedYear.Value != default) ? this.CentralDownloadedYear.Value : default;
        var centralDownloadedMonth = (this.IsDownloadCentral.Value && this.CentralDownloadedYear.Value != default) ? this.CentralDownloadedMonth.Value : default ;
        var localDownloadedYear = (this.IsDownloadLocal.Value && this.LocalDownloadedYear.Value != default) ? this.LocalDownloadedYear.Value : default;
        var localDownloadedMonth = (this.IsDownloadLocal.Value && this.LocalDownloadedYear.Value != default) ? this.LocalDownloadedMonth.Value : default;
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

        // 最新情報をダウンロードする
        this.StartDownloadLoop();
      }
      catch (DownloaderCommandException ex)
      {
        // TODO: logs
        this.IsInitializationError.Value = true;
        this.ErrorMessage.Value = !string.IsNullOrEmpty(ex.Message) ? ex.Message : ex.Error.GetErrorText();
        isFirst = false;
      }
      catch
      {
        // TODO: logs
      }

      return isFirst;
    }

    private void StartDownloadLoop()
    {
      Task.Run(async () =>
      {
        while (true)
        {
          var now = DateTime.Now;

          try
          {
            var today = new DateOnly(now.Year, now.Month, now.Day);
            if (this.IsDownloadCentral.Value)
            {
              await this.DownloadRTAsync(DownloadLink.Central, today);
            }
            if (this.IsDownloadLocal.Value)
            {
              await this.DownloadRTAsync(DownloadLink.Local, today);
            }
          }
          catch
          {
            // TODO: logs
          }

          while ((DateTime.Now - now).TotalMinutes < 5)
          {
            // ５分に１回ずつ更新する
            await Task.Delay(1000);
          }
        }
      });
    }

    public async Task DownloadAsync()
    {
      var link = (DownloadLink)0;
      if (this.IsDownloadCentral.Value) link |= DownloadLink.Central;
      if (this.IsDownloadLocal.Value) link |= DownloadLink.Local;

      await this.DownloadAsync(link);
    }

    private async Task DownloadAsync(DownloadLink link)
    {
      if (link == DownloadLink.Both)
      {
        await this.DownloadAsync(DownloadLink.Central);
        await this.DownloadAsync(DownloadLink.Local);
        return;
      }

      this.IsError.Value = false;
      this.IsDownloading.Value = true;

      var downloader = DownloaderConnector.Default;
      var linkName = link == DownloadLink.Central ? "central" : "local";

      var startYear = this.StartYear.Value;
      var startMonth = this.StartMonth.Value;
      if (this.Mode.Value == DownloadMode.Continuous)
      {
        using var db = new MyContext();

      }

      try
      {
        this.DownloadingLink.Value = link;
        var isContinue = await downloader.DownloadAsync(linkName, "race", startYear, startMonth, this.OnDownloadProgress);
        this.RacesUpdated?.Invoke(this, EventArgs.Empty);
        if (isContinue)
        {
          // this.DownloadingType.Value = Connection.DownloadingType.Odds;
          // await downloader.DownloadAsync(linkName, "odds", startYear, startMonth, this.OnDownloadProgress);
        }

        this.ProcessingStep.Value = Connection.ProcessingStep.InvalidData;
        this.IsProcessing.Value = true;
        await ShapeDatabaseModel.RemoveInvalidDataAsync();
        this.ProcessingStep.Value = Connection.ProcessingStep.RunningStyle;
        if (link == DownloadLink.Central)
        {
          ShapeDatabaseModel.TrainRunningStyle(isForce: true);
        }
        ShapeDatabaseModel.StartRunningStylePredicting();
        this.ProcessingStep.Value = Connection.ProcessingStep.StandardTime;
        await ShapeDatabaseModel.MakeStandardTimeMasterDataAsync(startYear - 2, link);
        this.ProcessingStep.Value = Connection.ProcessingStep.PreviousRaceDays;
        await ShapeDatabaseModel.SetPreviousRaceDaysAsync();
        this.RacesUpdated?.Invoke(this, EventArgs.Empty);
      }
      catch (DownloaderCommandException ex)
      {
        this.ErrorMessage.Value = ex.Error.GetErrorText();
        this.IsError.Value = true;
      }
      catch (Exception ex)
      {
        this.ErrorMessage.Value = ex.Message;
        this.IsError.Value = true;
      }
      finally
      {
        this.IsDownloading.Value = false;
        this.IsProcessing.Value = false;
      }
    }

    private async Task DownloadRTAsync(DownloadLink link, DateOnly date)
    {
      if (link == DownloadLink.Both)
      {
        await this.DownloadRTAsync(DownloadLink.Central, date);
        await this.DownloadRTAsync(DownloadLink.Local, date);
        return;
      }

      this.IsRTError.Value = false;
      this.IsRTDownloading.Value = true;
      this.RTDownloadingLink.Value = link;

      var downloader = DownloaderConnector.Default;
      var linkName = link == DownloadLink.Central ? "central" : "local";

      try
      {
        await downloader.DownloadRTAsync(linkName, date, this.OnRTDownloadProgress);
        this.RacesUpdated?.Invoke(this, EventArgs.Empty);

        this.RTProcessingStep.Value = Connection.ProcessingStep.InvalidData;
        this.IsRTProcessing.Value = true;
        await ShapeDatabaseModel.RemoveInvalidDataAsync();
        this.RTProcessingStep.Value = Connection.ProcessingStep.RunningStyle;
        ShapeDatabaseModel.StartRunningStylePredicting();
        this.RTProcessingStep.Value = Connection.ProcessingStep.PreviousRaceDays;
        await ShapeDatabaseModel.SetPreviousRaceDaysAsync();
        this.RacesUpdated?.Invoke(this, EventArgs.Empty);
      }
      catch (DownloaderCommandException ex)
      {
        this.RTErrorMessage.Value = ex.Error.GetErrorText();
        this.IsRTError.Value = true;
      }
      catch (Exception ex)
      {
        this.RTErrorMessage.Value = ex.Message;
        this.IsRTError.Value = true;
      }
      finally
      {
        this.IsRTDownloading.Value = false;
        this.IsRTProcessing.Value = false;
      }
    }

    private async Task OnDownloadProgress(DownloaderTaskData task)
    {
      var p = task.Parameter.Split(',');
      int.TryParse(p[0], out var year);
      int.TryParse(p[1], out var month);
      var mode = p[3];

      this.DownloadingYear.Value = year;
      this.DownloadingMonth.Value = month;

      this.LoadingProcess.Value = task.Result switch
      {
        "opening" => LoadingProcessValue.Opening,
        "downloading" => LoadingProcessValue.Downloading,
        "loading" => LoadingProcessValue.Loading,
        "writing" => LoadingProcessValue.Writing,
        "processing" => LoadingProcessValue.Processing,
        "closing" => LoadingProcessValue.Closing,
        _ => LoadingProcessValue.Unknown,
      };
      this.DownloadingType.Value = mode switch
      {
        "race" => Connection.DownloadingType.Race,
        "odds" => Connection.DownloadingType.Odds,
        _ => default,
      };

      // 現在ダウンロード中の年月を保存する
      if (task.Parameter.Contains("central") && (this.CentralDownloadedMonth.Value != month || this.CentralDownloadedYear.Value != year))
      {
        this.CentralDownloadedMonth.Value = month;
        this.CentralDownloadedYear.Value = year;
        using var db = new MyContext();
        await ConfigUtil.SetIntValueAsync(db, SettingKey.LastDownloadCentralDate, year * 100 + month);
      }
      if (task.Parameter.Contains("local") && (this.LocalDownloadedMonth.Value != month || this.LocalDownloadedYear.Value != year))
      {
        this.LocalDownloadedMonth.Value = month;
        this.LocalDownloadedYear.Value = year;
        using var db = new MyContext();
        await ConfigUtil.SetIntValueAsync(db, SettingKey.LastDownloadLocalDate, year * 100 + month);
      }
    }

    private Task OnRTDownloadProgress(DownloaderTaskData task)
    {
      var p = task.Parameter.Split(',');

      this.RTDownloadingDataspec.Value = p[2] switch
      {
        "1" => DownloadingDataspec.RB12,
        "2" => DownloadingDataspec.RB15,
        "3" => DownloadingDataspec.RB30,
        "4" => DownloadingDataspec.RB11,
        "5" => DownloadingDataspec.RB14,
        _ => DownloadingDataspec.Unknown,
      };

      this.RTLoadingProcess.Value = task.Result switch
      {
        "opening" => LoadingProcessValue.Opening,
        "downloading" => LoadingProcessValue.Downloading,
        "loading" => LoadingProcessValue.Loading,
        "writing" => LoadingProcessValue.Writing,
        "processing" => LoadingProcessValue.Processing,
        "closing" => LoadingProcessValue.Closing,
        _ => LoadingProcessValue.Unknown,
      };

      return Task.CompletedTask;
    }

    public async Task CancelDownloadAsync()
    {
      await DownloaderConnector.Default.CancelCurrentTaskAsync();
    }

    public void SetMode(string mode)
    {
      this.Mode.Value = mode == "Continuous" ? DownloadMode.Continuous : DownloadMode.WithStartDate;
    }

    public async Task OpenJvlinkConfigAsync()
    {
      this.IsError.Value = false;

      try
      {
        await DownloaderConnector.Default.OpenJvlinkConfigAsync();
      }
      catch (Exception ex)
      {
        this.IsError.Value = true;
        this.ErrorMessage.Value = ex.Message;
      }
    }

    public async Task OpenNvlinkConfigAsync()
    {
      this.IsError.Value = false;

      try
      {
        await DownloaderConnector.Default.OpenNvlinkConfigAsync();
      }
      catch (Exception ex)
      {
        this.IsError.Value = true;
        this.ErrorMessage.Value = ex.Message;
      }
    }

    public void Dispose()
    {
      this._disposables.Dispose();
      DownloaderConnector.Default.Dispose();
    }

    public event EventHandler? RacesUpdated;
  }

  [Flags]
  enum DownloadLink
  {
    [Label("中央")]
    Central = 0b01,

    [Label("地方")]
    Local = 0b10,

    Both = 0b11,
  }

  enum DownloadingType
  {
    [Label("レース")]
    Race,

    [Label("オッズ")]
    Odds,
  }

  enum LoadingProcessValue
  {
    Unknown,

    [Label("接続オープン中")]
    Opening,

    [Label("ダウンロード中")]
    Downloading,

    [Label("データ読み込み中")]
    Loading,

    [Label("データ保存中")]
    Writing,

    [Label("後処理中")]
    Processing,

    [Label("接続クローズ中")]
    Closing,
  }

  enum DownloadMode
  {
    Continuous,
    WithStartDate,
  }

  enum DownloadingDataspec
  {
    Unknown,

    [Label("レース結果")]
    RB12,

    [Label("レース情報")]
    RB15,

    [Label("オッズ")]
    RB30,

    [Label("馬体重")]
    RB11,

    [Label("変更情報")]
    RB14,
  }

  enum ProcessingStep
  {
    Unknown,

    [Label("不正なデータを処理中")]
    InvalidData,

    [Label("脚質を計算中")]
    RunningStyle,

    [Label("基準タイムを計算中")]
    StandardTime,

    [Label("馬データの成型中")]
    PreviousRaceDays,
  }
}