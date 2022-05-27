using KmyKeiba.Common;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Common;
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
  class ValueComparationBackgroundConverter : IValueConverter
  {
    private static readonly Brush goodBrush = Application.Current.TryFindResource("GoodBackground") as Brush ?? Brushes.Red;
    private static readonly Brush badBrush = Application.Current.TryFindResource("BadBackground") as Brush ?? Brushes.Blue;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is ValueComparation comp)
      {
        return comp switch
        {
          ValueComparation.Good => goodBrush,
          ValueComparation.Bad => badBrush,
          _ => Brushes.Transparent,
        };
      }
      if (value is StatusFeeling fel)
      {
        return fel switch
        {
          StatusFeeling.Good => goodBrush,
          StatusFeeling.Bad => badBrush,
          _ => Brushes.Transparent,
        };
      }
      return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }

  class ValueComparationForegroundConverter : IValueConverter
  {
    private static readonly Brush baseBrush = Application.Current.TryFindResource("BaseForeground") as Brush ?? Brushes.Red;
    private static readonly Brush goodBrush = Application.Current.TryFindResource("GoodForeground") as Brush ?? Brushes.Red;
    private static readonly Brush badBrush = Application.Current.TryFindResource("BadForeground") as Brush ?? Brushes.Blue;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is ValueComparation comp)
      {
        return comp switch
        {
          ValueComparation.Good => goodBrush,
          ValueComparation.Bad => badBrush,
          _ => baseBrush,
        };
      }
      if (value is StatusFeeling fel)
      {
        return fel switch
        {
          StatusFeeling.Good => goodBrush,
          StatusFeeling.Bad => badBrush,
          _ => baseBrush,
        };
      }
      return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }
}
