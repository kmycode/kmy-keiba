using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace KmyKeiba.Converters
{
  internal class EqualConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (targetType == typeof(Visibility))
      {
        return value.ToString() == parameter?.ToString() ? Visibility.Visible : Visibility.Collapsed;
      }
      return value.ToString() == parameter?.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
