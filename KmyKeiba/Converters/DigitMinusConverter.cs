using KmyKeiba.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace KmyKeiba.Converters
{
  class DigitMinusConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var order = 0;
      if (value is short os)
      {
        order = os;
      }
      else if (value is int oi)
      {
        order = oi;
      }
      else
      {
        throw new NotSupportedException();
      }

      return (order / 10) + "." + (order % 10);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }
}
