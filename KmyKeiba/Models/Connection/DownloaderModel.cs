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

    public ReactiveProperty<string> ErrorMessage { get; } = new();

    public ReactiveProperty<bool> IsInitialized { get; } = new();

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
      }
      catch
      {
        // TODO: logs
      }

      return isFirst;
    }
  }
}
