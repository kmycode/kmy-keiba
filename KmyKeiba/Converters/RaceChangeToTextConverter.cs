using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Race;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace KmyKeiba.Converters
{
  class RaceChangeToTextConverter : IValueConverter
  {
    private static readonly EnumsConverter conv = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is RaceChangeInfo data)
      {
        var horse = data.Horse ?? new RaceHorseData();
        return data.Data.ChangeType switch
        {
          RaceChangeType.AbnormalResult => $"{horse.Name} : {this.GetString(horse.AbnormalResult, culture)}",
          RaceChangeType.Course => $"コース変更",
          RaceChangeType.HorseWeight => "馬体重発表",
          RaceChangeType.Rider => $"{horse.Name} : 騎手変更 / {horse.RiderName}",
          RaceChangeType.StartTime => "開始時刻変更",
          RaceChangeType.TrackWeatherCondition => "天気・馬場変更",
          _ => string.Empty,
        };
      }
      throw new NotImplementedException();
    }

    private string GetString(object obj, CultureInfo culture)
    {
      return conv.Convert(obj, typeof(string), new object(), culture).ToString()!;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
