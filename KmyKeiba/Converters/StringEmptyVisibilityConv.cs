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
  class StringEmptyVisibilityConv : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is string str)
      {
        if (parameter?.ToString() == "Negative")
        {
          return !string.IsNullOrEmpty(str) ? Visibility.Visible : Visibility.Collapsed;
        }
        return string.IsNullOrEmpty(str) ? Visibility.Visible : Visibility.Collapsed;
      }
      return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
