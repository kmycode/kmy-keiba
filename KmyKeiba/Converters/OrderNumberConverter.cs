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
  class OrderNumberConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      int? num = null;
      if (value is int numi)
      {
        num = numi;
      }
      if (value is short nums)
      {
        num = nums;
      }

      if (num != null)
      {
        if (targetType == typeof(Brush))
        {
          if (parameter?.ToString() == "Thin")
          {
            Color GetThinColor(Color c)
            {
              // Colorはstructなので、呼び出し元の値は変更されない
              c.A = 64;

              return c;
            }

            return num switch
            {
              1 => new SolidColorBrush(GetThinColor(Brushes.Gold.Color)),
              2 => new SolidColorBrush(GetThinColor(Brushes.LightSkyBlue.Color)),
              3 => new SolidColorBrush(GetThinColor(Brushes.LightPink.Color)),
              _ => Brushes.Transparent,
            };
          }
          return num switch
          {
            1 => Brushes.Gold,
            2 => Brushes.LightSkyBlue,
            3 => Brushes.LightPink,
            _ => Brushes.LightGray,
          };
        }
        if (targetType == typeof(FontWeight))
        {
          return num switch
          {
            1 => FontWeights.Bold,
            2 => FontWeights.Bold,
            3 => FontWeights.Bold,
            _ => FontWeights.Normal,
          };
        }
        if (targetType == typeof(Visibility))
        {
          return num == 0 ? Visibility.Hidden : Visibility.Visible;
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
