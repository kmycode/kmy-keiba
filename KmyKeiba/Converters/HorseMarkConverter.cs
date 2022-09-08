using KmyKeiba.Data.Db;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace KmyKeiba.Converters
{
  class HorseMarkConverter : IValueConverter
  {
    public ControlTemplate? MarkStar { get; set; } = null;
    public ControlTemplate? MarkDefault { get; set; } = null;
    public ControlTemplate? MarkTriangle { get; set; } = null;
    public ControlTemplate? MarkFilledTriangle { get; set; } = null;
    public ControlTemplate? MarkDoubleCircle { get; set; } = null;
    public ControlTemplate? MarkCircle { get; set; } = null;
    public ControlTemplate? MarkDeleted { get; set; } = null;
    public ControlTemplate? MarkNote { get; set; } = null;
    public ControlTemplate? MarkCheck { get; set; } = null;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is RaceHorseMark mark)
      {
        return mark switch
        {
          RaceHorseMark.Default => this.MarkDefault!,
          RaceHorseMark.DoubleCircle => this.MarkDoubleCircle!,
          RaceHorseMark.Circle => this.MarkCircle!,
          RaceHorseMark.Triangle => this.MarkTriangle!,
          RaceHorseMark.FilledTriangle => this.MarkFilledTriangle!,
          RaceHorseMark.Star => this.MarkStar!,
          RaceHorseMark.Deleted => this.MarkDeleted!,
          RaceHorseMark.Check => this.MarkCheck!,
          RaceHorseMark.Note => this.MarkNote!,
          _ => this.MarkDefault!,
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
