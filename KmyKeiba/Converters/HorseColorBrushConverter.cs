using KmyKeiba.JVLink.Entities;
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
  class HorseColorBrushConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is HorseBodyColor color)
      {
        return color switch
        {
          HorseBodyColor.Chestnut => Brushes.SaddleBrown,
          HorseBodyColor.DarkChestnut => Brushes.Brown,
          HorseBodyColor.Bay => Brushes.Olive,
          HorseBodyColor.DarkBay => Brushes.DarkOliveGreen,
          HorseBodyColor.Brown => Brushes.Brown,
          HorseBodyColor.Black => Brushes.Black,
          HorseBodyColor.Grey => Brushes.LightGray,
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
