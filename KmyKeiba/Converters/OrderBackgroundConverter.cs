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
  class OrderBackgroundConverter : IValueConverter
  {
    private static readonly Brush firstOrder = Application.Current.TryFindResource("FirstOrderBackground") as Brush ?? Brushes.Transparent;
    private static readonly Brush secondOrder = Application.Current.TryFindResource("SecondOrderBackground") as Brush ?? Brushes.Transparent;
    private static readonly Brush thirdOrder = Application.Current.TryFindResource("ThirdOrderBackground") as Brush ?? Brushes.Transparent;

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

      return order switch
      {
        1 => firstOrder,
        2 => secondOrder,
        3 => thirdOrder,
        _ => Brushes.Transparent,
      };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }
}
