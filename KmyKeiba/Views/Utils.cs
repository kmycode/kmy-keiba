using KmyKeiba.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace KmyKeiba.Views
{
  internal static class Utils
  {
    public static Brush ToBrush(this RHColor color)
    {
      return new SolidColorBrush(Color.FromRgb(color.Red, color.Green, color.Blue));
    }
  }
}
