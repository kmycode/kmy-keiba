using KmyKeiba.Models.Race.Finder;
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
  internal class CellTextAlignmentConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is CellTextAlignment v)
      {
        return v switch
        {
          CellTextAlignment.Center => TextAlignment.Center,
          CellTextAlignment.Right => TextAlignment.Right,
          CellTextAlignment.Left => TextAlignment.Left,
          _ => default,
        };
      }
      throw new NotImplementedException();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
