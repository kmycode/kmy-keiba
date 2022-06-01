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
  internal class NonMinusVisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is short sv)
      {
        return sv < 0 ? Visibility.Collapsed : Visibility.Visible;
      }
      if (value is ushort usv)
      {
        return usv < 0 ? Visibility.Collapsed : Visibility.Visible;
      }
      if (value is int iv)
      {
        return iv < 0 ? Visibility.Collapsed : Visibility.Visible;
      }
      if (value is uint uiv)
      {
        return uiv < 0 ? Visibility.Collapsed : Visibility.Visible;
      }
      throw new NotImplementedException();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
