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
}
