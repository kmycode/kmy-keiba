using KmyKeiba.Data.Db;
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
  class MemoColorBrushConverter : DependencyObject, IValueConverter
  {
    public static readonly DependencyProperty DefaultBrushProperty
        = DependencyProperty.Register(
            nameof(DefaultBrush),
            typeof(Brush),
            typeof(MemoColorBrushConverter),
            new PropertyMetadata(null));

    public Brush? DefaultBrush
    {
      get { return (Brush)GetValue(DefaultBrushProperty); }
      set { SetValue(DefaultBrushProperty, value); }
    }

    public static readonly DependencyProperty GoodBrushProperty
        = DependencyProperty.Register(
            nameof(GoodBrush),
            typeof(Brush),
            typeof(MemoColorBrushConverter),
            new PropertyMetadata(null));

    public Brush? GoodBrush
    {
      get { return (Brush)GetValue(GoodBrushProperty); }
      set { SetValue(GoodBrushProperty, value); }
    }

    public static readonly DependencyProperty BadBrushProperty
        = DependencyProperty.Register(
            nameof(BadBrush),
            typeof(Brush),
            typeof(MemoColorBrushConverter),
            new PropertyMetadata(null));

    public Brush? BadBrush
    {
      get { return (Brush)GetValue(BadBrushProperty); }
      set { SetValue(BadBrushProperty, value); }
    }

    public static readonly DependencyProperty WarningBrushProperty
        = DependencyProperty.Register(
            nameof(WarningBrush),
            typeof(Brush),
            typeof(MemoColorBrushConverter),
            new PropertyMetadata(null));

    public Brush? WarningBrush
    {
      get { return (Brush)GetValue(WarningBrushProperty); }
      set { SetValue(WarningBrushProperty, value); }
    }

    public static readonly DependencyProperty NegativeBrushProperty
        = DependencyProperty.Register(
            nameof(NegativeBrush),
            typeof(Brush),
            typeof(MemoColorBrushConverter),
            new PropertyMetadata(null));

    public Brush? NegativeBrush
    {
      get { return (Brush)GetValue(NegativeBrushProperty); }
      set { SetValue(NegativeBrushProperty, value); }
    }

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is MemoColor c)
      {
        return c switch
        {
          MemoColor.Good => this.GoodBrush,
          MemoColor.Bad => this.BadBrush,
          MemoColor.Warning => this.WarningBrush,
          MemoColor.Negative => this.NegativeBrush,
          _ => this.DefaultBrush,
        };
      }
      return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }
}
