using KmyKeiba.JVLink.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace KmyKeiba.Converters
{
  class RaceClassBrushConverter : ConverterBase<RaceClass, Brush>
  {
    protected override Brush Convert(RaceClass value, object parameter)
    {
      return value switch
      {
        RaceClass.ClassA => Brushes.DarkRed,
        RaceClass.ClassB => Brushes.DarkBlue,
        RaceClass.ClassC => Brushes.Green,
        RaceClass.Money => Brushes.DarkGoldenrod,
        RaceClass.Age => Brushes.SkyBlue,
        _ => Brushes.LightGray,
      };
    }

    protected override RaceClass ConvertBack(Brush value, object parameter)
    {
      throw new NotImplementedException();
    }
  }
}
