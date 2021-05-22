using KmyKeiba.JVLink.Entities;
using KmyKeiba.JVLink.Wrappers;
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
  class AbnormalityResultConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is RaceAbnormality ab)
      {
        if (targetType == typeof(string))
        {
          return ab.GetLabel();
        }
        if (targetType == typeof(Visibility))
        {
          if (parameter?.ToString() == "Reverse")
          {
            return ab != RaceAbnormality.Unknown ? Visibility.Collapsed : Visibility.Visible;
          }
          return ab == RaceAbnormality.Unknown ? Visibility.Collapsed : Visibility.Visible;
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
