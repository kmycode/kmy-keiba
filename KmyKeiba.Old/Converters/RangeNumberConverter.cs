using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace KmyKeiba.Converters
{
  class RangeNumberConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      double? val = null;
      if (value is double v)
      {
        val = v;
      }
      if (value is float vv)
      {
        val = vv;
      }
      if (val != null && int.TryParse(parameter?.ToString(), out int max))
      {
        var r = max * val;
        if (targetType == typeof(double))
        {
          return r;
        }
        if (targetType == typeof(float))
        {
          return (float)r;
        }
        if (targetType == typeof(int))
        {
          return (int)r;
        }
      }
      throw new NotImplementedException();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
