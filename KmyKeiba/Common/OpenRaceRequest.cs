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

    public void Request(string key)
    {
      this.Requested?.Invoke(this, new OpenRaceRequestEventArgs(key));
    }
  }

  public class OpenRaceRequestEventArgs : EventArgs
  {
    public string RaceKey { get; }

    public OpenRaceRequestEventArgs(string key)
    {
      this.RaceKey = key;
    }
  }
}
