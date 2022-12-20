using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Common
{
  public class OpenRaceRequest
  {
    public static OpenRaceRequest Default { get; } = new();

    public event EventHandler<OpenRaceRequestEventArgs>? Requested;
    public event EventHandler? RaceUpdated;

    public void Request(string key)
    {
      this.Requested?.Invoke(this, new OpenRaceRequestEventArgs(key));
    }

    public void Request(string key, string horseKey)
    {
      this.Requested?.Invoke(this, new OpenRaceRequestEventArgs(key, horseKey));
    }

    public void Request(string key, IReadOnlyList<string> horseKeys)
    {
      this.Requested?.Invoke(this, new OpenRaceRequestEventArgs(key, horseKeys));
    }

    public void Update()
    {
      this.RaceUpdated?.Invoke(this, EventArgs.Empty);
    }
  }

  public class OpenRaceRequestEventArgs : EventArgs
  {
    public string RaceKey { get; }

    public IReadOnlyList<string> HorseKeys { get; }

    public OpenRaceRequestEventArgs(string key)
    {
      this.RaceKey = key;
      this.HorseKeys = Array.Empty<string>();
    }

    public OpenRaceRequestEventArgs(string key, string horseKey)
    {
      this.RaceKey = key;
      this.HorseKeys = new[] { horseKey, };
    }

    public OpenRaceRequestEventArgs(string key, IReadOnlyList<string> horseKeys)
    {
      this.RaceKey = key;
      this.HorseKeys = horseKeys;
    }
  }
}
