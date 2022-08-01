using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Common
{
  public class OpenErrorSavingMemoRequest
  {
    public static OpenErrorSavingMemoRequest Default { get; } = new();

    public void Request(string memo)
    {
      this.Requested?.Invoke(this, new OpenErrorSavingMemoRequestEventArgs(memo));
    }

    public event EventHandler<OpenErrorSavingMemoRequestEventArgs>? Requested;
  }

  public class OpenErrorSavingMemoRequestEventArgs : EventArgs
  {
    public string Memo { get; }

    public OpenErrorSavingMemoRequestEventArgs(string memo)
    {
      this.Memo = memo;
    }
  }

  public class OpenErrorConfiguringRequest
  {
    public static OpenErrorConfiguringRequest Default { get; } = new();

    public void Request(string message)
    {
      if (this.Requested != null)
      {
        this.Requested.Invoke(this, new OpenErrorConfiguringRequestEventArgs(message));
      }
      else
      {
        Task.Run(() =>
        {
          while (this.Requested == null)
          {
            Task.Delay(500).Wait();
          }
          ThreadUtil.InvokeOnUiThread(() =>
          {
            this.Requested.Invoke(this, new OpenErrorConfiguringRequestEventArgs(message));
          });
        });
      }
    }

    public event EventHandler<OpenErrorConfiguringRequestEventArgs>? Requested;
  }

  public class OpenErrorConfiguringRequestEventArgs : EventArgs
  {
    public string Message { get; }

    public OpenErrorConfiguringRequestEventArgs(string message)
    {
      this.Message = message;
    }
  }
}
