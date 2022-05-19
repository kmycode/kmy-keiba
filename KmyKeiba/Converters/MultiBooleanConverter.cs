﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace KmyKeiba.Converters
{
  class MultiBooleanConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      var result = false;
      if (parameter?.ToString() == "Or") result = values.OfType<bool>().Any((v) => v);
      else result = values.OfType<bool>().All((v) => v);
      if (targetType == typeof(Visibility))
      {
        return result ? Visibility.Visible : Visibility.Collapsed;
      }
      return result;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
