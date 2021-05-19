using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace KmyKeiba.Converters
{
  abstract class ConverterBase<T, O> : IValueConverter
  {
    protected abstract O Convert(T value, object parameter);

    protected abstract T ConvertBack(O value, object parameter);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is T t && targetType == typeof(O))
      {
        return this.Convert(t, parameter)!;
      }
      throw new NotImplementedException();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is O t && targetType == typeof(T))
      {
        return this.ConvertBack(t, parameter)!;
      }
      throw new NotImplementedException();
    }
  }
}
