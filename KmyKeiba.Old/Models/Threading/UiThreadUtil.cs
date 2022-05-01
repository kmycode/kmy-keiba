using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace KmyKeiba.Models.Threading
{
  static class UiThreadUtil
  {
    public static Dispatcher? Dispatcher { get; set; }
  }
}
