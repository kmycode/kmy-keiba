using KmyKeiba.Data.DataObjects;
using KmyKeiba.Data.Db;
using KmyKeiba.Views;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using static KmyKeiba.ViewModels.MainWindowViewModel;

namespace KmyKeiba.Converters
{
  class MarkHorseCommandConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      var horse = values.OfType<RaceHorseDataObject>().FirstOrDefault();
      var markStr = values.OfType<string>().FirstOrDefault();
      if (horse != null && markStr != null)
      {
        if (int.TryParse(markStr, out int markInt))
        {
          return new MarkHorseCommandParameter
          {
            Horse = horse,
            Mark = (RaceHorseMark)markInt,
          };
        }
      }
      return new MarkHorseCommandParameter();
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
