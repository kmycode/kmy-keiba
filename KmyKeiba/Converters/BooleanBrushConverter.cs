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
  class BooleanBrushConverter : DependencyObject, IValueConverter
  {
    public static readonly DependencyProperty TrueBrushProperty
        = DependencyProperty.Register(
            nameof(TrueBrush),
            typeof(Brush),
            typeof(BooleanBrushConverter),
            new PropertyMetadata(null));

    public Brush? TrueBrush
    {
      get { return (Brush)GetValue(TrueBrushProperty); }
      set { SetValue(TrueBrushProperty, value); }
    }

    public static readonly DependencyProperty FalseBrushProperty
        = DependencyProperty.Register(
            nameof(FalseBrush),
            typeof(Brush),
            typeof(BooleanBrushConverter),
            new PropertyMetadata(null));

    public Brush? FalseBrush
    {
      get { return (Brush)GetValue(FalseBrushProperty); }
      set { SetValue(FalseBrushProperty, value); }
    }

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is bool b)
      {
        return b ? this.TrueBrush : this.FalseBrush;
      }
      return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }
}
