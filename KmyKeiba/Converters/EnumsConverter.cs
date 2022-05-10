using KmyKeiba.JVLink.Entities;
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
  class EnumsConverter : IValueConverter
  {
    private static readonly Brush _maleBrush = Application.Current.TryFindResource("MaleForeground") as Brush ?? Brushes.Blue;
    private static readonly Brush _femaleBrush = Application.Current.TryFindResource("FemaleForeground") as Brush ?? Brushes.Red;
    private static readonly Brush _castratedBrush = Application.Current.TryFindResource("CastratedForeground") as Brush ?? Brushes.Green;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is HorseSex sex)
      {
        if (targetType == typeof(string))
        {
          return sex switch
          {
            HorseSex.Male => "牡",
            HorseSex.Female => "牝",
            HorseSex.Castrated => "騙",
            _ => "？",
          };
        }
        if (targetType == typeof(Brush))
        {
          return sex switch
          {
            HorseSex.Male => _maleBrush,
            HorseSex.Female => _femaleBrush,
            HorseSex.Castrated => _castratedBrush,
            _ => "？",
          };
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
