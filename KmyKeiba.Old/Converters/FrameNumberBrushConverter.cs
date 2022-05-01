using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace KmyKeiba.Converters
{
  class FrameNumberBrushConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is int num)
      {
        if (parameter?.ToString() == "Foreground")
        {
          return num switch
          {
            1 => Brushes.Black,
            2 => Brushes.White,
            3 => Brushes.White,
            4 => Brushes.White,
            5 => Brushes.Black,
            6 => Brushes.White,
            7 => Brushes.Black,
            8 => Brushes.Black,
            _ => Brushes.Black,
          };
        }
        return num switch
        {
          1 => Brushes.White,
          2 => Brushes.Black,
          3 => Brushes.Red,
          4 => Brushes.Blue,
          5 => Brushes.Yellow,
          6 => Brushes.Green,
          7 => Brushes.Orange,
          8 => Brushes.Pink,
          _ => Brushes.Transparent,
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
