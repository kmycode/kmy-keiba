using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Common
{
  public class OpenDialogRequest
  {
    public event EventHandler<OpenDialogRequestEventArgs>? Requested;

    public void Request(string name)
    {
      this.Requested?.Invoke(this, new OpenDialogRequestEventArgs(name));
    }
  }

  public class OpenDialogRequestEventArgs : EventArgs
  {
    public string DialogName { get; }

    public OpenDialogRequestEventArgs(string name)
    {
      this.DialogName = name;
    }
  }
}
