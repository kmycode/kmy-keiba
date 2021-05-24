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
  class RaceClassBrushConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return value switch
      {
        RaceClass.ClassA => Brushes.DarkRed,
        RaceClass.ClassB => Brushes.DarkBlue,
        RaceClass.ClassC => Brushes.Green,
        RaceClass.ClassD => Brushes.Gray,
        RaceClass.Money => Brushes.DarkGoldenrod,
        RaceClass.Age => Brushes.SkyBlue,
        RaceGrade.Grade1 => Brushes.DarkRed,
        RaceGrade.Grade2 => Brushes.DarkBlue,
        RaceGrade.Grade3 => Brushes.Green,
        RaceGrade.LocalGrade1 => Brushes.DeepPink,
        RaceGrade.LocalGrade2 => Brushes.DeepSkyBlue,
        RaceGrade.LocalGrade3 => Brushes.Lime,
        RaceGrade.Steeplechase1 => Brushes.DarkRed,
        RaceGrade.Steeplechase2 => Brushes.DarkBlue,
        RaceGrade.Steeplechase3 => Brushes.Green,
        RaceGrade.NoNamedGrade => Brushes.Gray,
        RaceGrade.NonGradeSpecial => Brushes.DarkGoldenrod,
        RaceGrade.Listed => Brushes.Gray,
        _ => Brushes.LightGray,
      };
      throw new NotImplementedException();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
