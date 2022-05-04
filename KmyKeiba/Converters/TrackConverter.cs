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
  class TrackConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      switch (value)
      {
        case TrackType type:
          {
            if (targetType == typeof(string))
            {
              return type switch
              {
                TrackType.Flat => "平地",
                TrackType.Steeplechase => "障害",
                _ => string.Empty,
              };
            }
            if (targetType == typeof(Brush))
            {
              return type switch
              {
                TrackType.Flat => Brushes.Green,
                TrackType.Steeplechase => Brushes.DarkOrange,
                _ => Brushes.Gray,
              };
            }
            if (targetType == typeof(Visibility))
            {
              return type != TrackType.Unknown ? Visibility.Visible : Visibility.Collapsed;
            }
            throw new NotSupportedException();
          }
        case TrackGround ground:
          {
            if (targetType == typeof(string))
            {
              return ground switch
              {
                TrackGround.Turf => "芝",
                TrackGround.Dirt => "ダート",
                TrackGround.Sand => "サンド",
                TrackGround.TurfToDirt => "芝→ダート",
                _ => string.Empty,
              };
            }
            if (targetType == typeof(Brush))
            {
              return ground switch
              {
                TrackGround.Turf => Brushes.Green,
                TrackGround.Dirt => Brushes.Brown,
                TrackGround.Sand => Brushes.DarkOrange,
                TrackGround.TurfToDirt => Brushes.Purple,
                _ => Brushes.Gray,
              };
            }
            if (targetType == typeof(Visibility))
            {
              return ground != TrackGround.Unknown ? Visibility.Visible : Visibility.Collapsed;
            }
            throw new NotSupportedException();
          }
        case TrackCornerDirection corner:
          {
            if (targetType == typeof(string))
            {
              return corner switch
              {
                TrackCornerDirection.Left => "左",
                TrackCornerDirection.Right => "右",
                TrackCornerDirection.Straight => "直線",
                _ => string.Empty,
              };
            }
            if (targetType == typeof(Brush))
            {
              return corner switch
              {
                TrackCornerDirection.Left => Brushes.DarkRed,
                TrackCornerDirection.Right => Brushes.DarkBlue,
                TrackCornerDirection.Straight => Brushes.Green,
                _ => Brushes.Gray,
              };
            }
            if (targetType == typeof(Visibility))
            {
              return corner != TrackCornerDirection.Unknown ? Visibility.Visible : Visibility.Collapsed;
            }
            throw new NotSupportedException();
          }
        case RaceCourseWeather wea:
          {
            if (targetType == typeof(string))
            {
              return wea switch
              {
                RaceCourseWeather.Fine => "晴",
                RaceCourseWeather.Rainy => "雨",
                RaceCourseWeather.Cloudy => "曇",
                RaceCourseWeather.Snow => "雪",
                RaceCourseWeather.LightSnow => "雪",
                _ => string.Empty,
              };
            }
            if (targetType == typeof(Brush))
            {
              return Brushes.Gray;
            }
            throw new NotSupportedException();
          }
        case RaceCourseCondition cod:
          {
            if (targetType == typeof(string))
            {
              return cod switch
              {
                RaceCourseCondition.Standard => "良",
                RaceCourseCondition.Good => "稍重",
                RaceCourseCondition.Yielding => "重",
                RaceCourseCondition.Soft => "不良",
                _ => string.Empty,
              };
            }
            if (targetType == typeof(Brush))
            {
              return Brushes.Gray;
            }
            throw new NotSupportedException();
          }
        case TrackOption opt:
          {
            if (targetType == typeof(string))
            {
              return opt switch
              {
                TrackOption.Inside => "内",
                TrackOption.Inside2 => "内2周",
                TrackOption.InsideToOutside => "内→外",
                TrackOption.Outside => "外",
                TrackOption.Outside2 => "外2周",
                TrackOption.OutsideToInside => "外→内",
                _ => string.Empty,
              };
            }
            if (targetType == typeof(Brush))
            {
              return opt switch
              {
                TrackOption.Inside => Brushes.Red,
                TrackOption.Inside2 => Brushes.Red,
                TrackOption.InsideToOutside => Brushes.Purple,
                TrackOption.Outside => Brushes.Blue,
                TrackOption.Outside2 => Brushes.Blue,
                TrackOption.OutsideToInside => Brushes.Purple,
                _ => Brushes.Gray,
              };
            }
            if (targetType == typeof(Visibility))
            {
              return opt != TrackOption.Unknown ? Visibility.Visible : Visibility.Collapsed;
            }
            throw new NotSupportedException();
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
