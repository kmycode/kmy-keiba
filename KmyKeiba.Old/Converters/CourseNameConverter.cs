using KmyKeiba.Data.Wrappers;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.JVLink.Wrappers;
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
  class CourseNameConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is RaceCourse course)
      {
        if (parameter?.ToString() == "Type")
        {
          return course.GetCourseType() switch
          {
            RaceCourseType.Central => "中央",
            RaceCourseType.Local => "地方",
            RaceCourseType.Foreign => "海外",
            _ => "不明",
          };
        }
        if (targetType == typeof(Brush))
        {
          return course.GetCourseType() switch
          {
            RaceCourseType.Central => Brushes.Green,
            RaceCourseType.Local => Brushes.DarkOrange,
            RaceCourseType.Foreign => Brushes.Blue,
            _ => Brushes.Gray,
          };
        }
        return course.GetName();
      }
      throw new NotImplementedException();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
