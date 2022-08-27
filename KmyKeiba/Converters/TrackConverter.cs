using KmyKeiba.Common;
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
              return type.ToLabelString();
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
              return ground.ToLabelString();
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
              return corner.ToLabelString();
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
              if (parameter?.ToString() == "Long")
              {
                return wea.ToLongLabelString();
              }
              return wea.ToLabelString();
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
              return cod.ToLabelString();
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
              return opt.ToLabelString();
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
        case RaceHorseSexRule sex:
          {
            if (targetType == typeof(Visibility))
            {
              var r = true;
              switch (sex)
              {
                case RaceHorseSexRule.Male:
                  r = parameter?.ToString() == "Male";
                  break;
                case RaceHorseSexRule.Female:
                case RaceHorseSexRule.A:
                case RaceHorseSexRule.B:
                  r = parameter?.ToString() == "Female";
                  break;
                case RaceHorseSexRule.MaleCastrated:
                  r = parameter?.ToString() == "Castrated" || parameter?.ToString() == "Male";
                  break;
                case RaceHorseSexRule.MaleFemale:
                  r = parameter?.ToString() == "Female" || parameter?.ToString() == "Male";
                  break;
              }
              return r ? Visibility.Visible : Visibility.Collapsed;
            }
            throw new NotSupportedException();
          }
        case RaceRiderWeightRule rider:
          {
            if (targetType == typeof(string))
            {
              return rider.ToLabelString();
            }
            throw new NotSupportedException();
          }
        case RaceHorseAreaRule area:
          {
            if (targetType == typeof(string))
            {
              return area.ToLabelString();
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
