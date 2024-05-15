using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;

namespace KmyKeiba.Models.Connection.Connector
{
  public interface IConnector
  {
    DownloadLink Link { get; }

    ReactiveProperty<bool> IsAvailable { get; }
    ReactiveProperty<bool> IsRTAvailable { get; }

    ReactiveProperty<bool> IsRunning { get; }

    ReactiveProperty<bool> IsSaving { get; }

    ReactiveProperty<ConnectorErrorInfo?> Error { get; }

    Task DownloadAsync(DateOnly start, DateOnly end);

    Task DownloadAsync(DateOnly start) => this.DownloadAsync(start, DateOnly.FromDateTime(DateTime.Today));

    Task DownloadRTAsync(DateOnly start, DateOnly end);

    Task DownloadRTAsync(DateOnly day) => this.DownloadRTAsync(day, DateOnly.FromDateTime(DateTime.Today));
  }

  internal static class Connectors
  {
    public static CentralConnector Central { get; } = new();

    public static LocalConnector Local { get; } = new();

    public static JrdbConnector Jrdb { get; } = new();
  }

  public class ConnectorCollection : ReadOnlyCollection<IConnector>, IConnector
  {
    public ReactiveProperty<IConnector?> ActiveConnector { get; } = new();
    public ReactiveProperty<IConnector?> RTActiveConnector { get; } = new();
    public DownloadLink Link => DownloadLink.None;
    public ReactiveProperty<bool> IsAvailable => throw new NotSupportedException();
    public ReactiveProperty<bool> IsRTAvailable => throw new NotSupportedException();
    public ReactiveProperty<bool> IsRunning => throw new NotSupportedException();
    public ReactiveProperty<bool> IsSaving => throw new NotSupportedException();
    public ReactiveProperty<ConnectorErrorInfo?> Error => throw new NotSupportedException();

    public ConnectorCollection(IList<IConnector> list) : base(list)
    {
    }

    public static ConnectorCollection GenerateDefaults() => new ConnectorCollection([
      Connectors.Central,
      Connectors.Local,
      Connectors.Jrdb,
    ]);

    public async Task DownloadAsync(DateOnly start, DateOnly end)
    {
      foreach (var item in this.Where(i => i.IsAvailable.Value))
      {
        this.ActiveConnector.Value = item;

        await item.DownloadAsync(start, end);
      }

      this.ActiveConnector.Value = null;
    }

    public async Task DownloadRTAsync(DateOnly start, DateOnly end)
    {
      foreach (var item in this.Where(i => i.IsRTAvailable.Value))
      {
        this.RTActiveConnector.Value = item;

        await item.DownloadRTAsync(start, end);
      }

      this.RTActiveConnector.Value = null;
    }

    public Task DownloadAsync(DateOnly start) => this.DownloadAsync(start, DateOnly.FromDateTime(DateTime.Today));

    public async Task DownloadPreviousDayResultsAsync(DateOnly start)
    {
      foreach (var item in this.Where(i => i.IsRTAvailable.Value && i.Link == DownloadLink.Central))
      {
        this.RTActiveConnector.Value = item;

        await item.DownloadRTAsync(start, DateOnly.FromDateTime(DateTime.Today));
      }

      this.RTActiveConnector.Value = null;
    }
  }

  public readonly record struct ConnectorErrorInfo(string Message);
}
