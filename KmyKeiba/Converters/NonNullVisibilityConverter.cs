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
  internal class NonNullVisibilityConverter : IValueConverter
  {
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
      var collapsed = parameter?.ToString() == "Hidden" ? Visibility.Hidden : Visibility.Collapsed;

      if (value is string str)
      {
        return string.IsNullOrEmpty(str) ? collapsed : Visibility.Visible;
      }
      return value == null ? collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
