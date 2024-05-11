using KmyKeiba.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace KmyKeiba.Converters
{
  internal class MoneyLabelConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is int iv) return ValueUtil.ToMoneyLabel(iv);
      if (value is short sv) return ValueUtil.ToMoneyLabel(sv);
      if (value is long lv) return ValueUtil.ToMoneyLabel(lv);
      throw new NotImplementedException();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
