using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.ViewEvents
{
  class FileDialogCaller
  {
    public event EventHandler<FileDialogCalledEventArgs>? Called;

    public void Call(FileDialogCalledEventArgs ev)
    {
      this.Called?.Invoke(this, ev);
    }
  }

  class FileDialogCalledEventArgs : EventArgs
  {
    public FileDialogType Type { get; set; }

    public string Filter { get; set; } = string.Empty;

    public Action<string>? OnCompleted { get; set; }
  }

  enum FileDialogType
  {
    Open,
    Save,
  }
}
