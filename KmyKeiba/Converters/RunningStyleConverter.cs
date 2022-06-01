using KmyKeiba.JVLink.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace KmyKeiba.Converters
{
  class RunningStyleConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is RunningStyle style)
      {
        return style switch
        {
          RunningStyle.FrontRunner => "逃",
          RunningStyle.SaveRunner => "追",
          RunningStyle.Sotp => "差",
          RunningStyle.Stalker => "先",
          _ => "？",
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
