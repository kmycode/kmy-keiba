using KmyKeiba.Data.Db;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Connection
{
  internal class DownloaderModel
  {
    public ReactiveProperty<bool> IsInitializationError { get; } = new();

    public ReactiveProperty<bool> IsError { get; } = new();

    public ReactiveProperty<string> ErrorMessage { get; } = new();

    public ReactiveProperty<bool> IsInitialized { get; } = new();

    public ReactiveProperty<int> StartYear { get; } = new();

    public ReactiveProperty<int> StartMonth { get; } = new();

    public IReadOnlyList<int> StartYearSelection { get; }

    public IReadOnlyList<int> StartMonthSelection { get; }

    public ReactiveProperty<DownloadingType> DownloadingType { get; } = new();

    public ReactiveProperty<DownloadLink> DownloadingLink { get; } = new();

    public ReactiveProperty<int> DownloadingYear { get; } = new();

    public ReactiveProperty<int> DownloadingMonth { get; } = new();

    public ReactiveProperty<LoadingProcessValue> LoadingProcess { get; } = new();

    public DownloaderModel()
    {
      this.StartYearSelection = Enumerable.Range(1986, DateTime.Now.Year - 1986 + 1).ToArray();
      this.StartMonthSelection = Enumerable.Range(1, 12).ToArray();
    }

    public async Task<bool> InitializeAsync()
    {
      var downloader = DownloaderConnector.Default;
      var isFirst = !downloader.IsExistsDatabase;

      try
      {
        await downloader.InitializeAsync();
        this.IsInitialized.Value = true;
      }
      catch (DownloaderCommandException ex)
      {
        // TODO: logs
        this.IsInitializationError.Value = true;
        this.ErrorMessage.Value = ex.Message;
        isFirst = false;
      }
      catch
      {
        // TODO: logs
      }

      return isFirst;
    }

    public async Task DownloadAsync(DownloadLink link)
    {
      if (link == DownloadLink.Both)
      {
        await this.DownloadAsync(DownloadLink.Central);
        await this.DownloadAsync(DownloadLink.Local);
        return;
      }

      this.IsError.Value = false;

      var downloader = DownloaderConnector.Default;
      var linkName = link == DownloadLink.Central ? "central" : "local";

      try
      {
        this.DownloadingLink.Value = link;
        this.DownloadingType.Value = Connection.DownloadingType.Race;
        await downloader.DownloadAsync(linkName, "race", this.StartYear.Value, this.StartMonth.Value, this.OnDownloadProgress);
        this.DownloadingType.Value = Connection.DownloadingType.Odds;
        await downloader.DownloadAsync(linkName, "odds", this.StartYear.Value, this.StartMonth.Value, this.OnDownloadProgress);
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
    }

    private Task OnDownloadProgress(DownloaderTaskData task)
    {
      var p = task.Parameter.Split(',');
      int.TryParse(p[0], out var year);
      int.TryParse(p[1], out var month);

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

      return Task.CompletedTask;
    }
  }

  enum DownloadLink
  {
    Central,
    Local,
    Both,
  }

  enum DownloadingType
  {
    Race,
    Odds,
  }

  enum LoadingProcessValue
  {
    Unknown,
    Opening,
    Downloading,
    Loading,
    Writing,
    Processing,
    Closing,
  }
}
