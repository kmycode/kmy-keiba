using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace KmyKeiba.Converters
{
  class ResultLengthConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is short v)
      {
        if (v == 1) return "アタマ";
        if (v == 2) return "同着";
        if (v == 3) return "ハナ";
        if (v == 4) return "クビ";
        if (v == 1500) return "大差";

        var dsize = v % 100;
        var hsize = v / 100;

        var hsizeStr = hsize > 0 ? hsize.ToString() : String.Empty;

        return dsize switch
        {
          0 => hsize.ToString(),
          25 => hsizeStr + " 1/4",
          33 => hsizeStr + " 1/3",
          50 => hsizeStr + " 1/2",
          66 => hsizeStr + " 2/3",
          75 => hsizeStr + " 3/4",
          _ => hsizeStr + " " + dsize,
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
