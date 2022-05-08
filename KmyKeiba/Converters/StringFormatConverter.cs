using KmyKeiba.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace KmyKeiba.Converters
{
  class StringFormatConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (parameter is string format)
      {
        switch (value)
        {
          case TimeSpan ts:
            return ts.ToString(format);
          case float fl:
            return fl.ToString(format);
          case double dl:
            return dl.ToString(format);
        }
      }
      return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }
}
