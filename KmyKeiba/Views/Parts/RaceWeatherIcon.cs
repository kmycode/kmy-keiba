using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Race.Finder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace KmyKeiba.Views.Parts
{
  public class RaceWeatherIcon : Border
  {
    private static Geometry? _sun;
    private static Geometry? _cloud;
    private static Geometry? _rain;
    private static Geometry? _rainHeavy;
    private static Geometry? _snow;
    private static Geometry? _snowHeavy;
    private static Brush? _sunB;
    private static Brush? _cloudB;
    private static Brush? _rainB;
    private static Brush? _rainHeavyB;
    private static Brush? _snowB;
    private static Brush? _snowHeavyB;

    public static readonly DependencyProperty WeatherProperty
      = DependencyProperty.Register(
       nameof(Weather),
       typeof(RaceCourseWeather),
       typeof(RaceWeatherIcon),
       new PropertyMetadata((sender, e) => (sender as RaceWeatherIcon)?.OnWeatherChanged()));

    public RaceCourseWeather? Weather
    {
      get { return (RaceCourseWeather)GetValue(WeatherProperty); }
      set { SetValue(WeatherProperty, value); }
    }

    public RaceWeatherIcon()
    {
      this.Child = new Path();
    }

    private void OnWeatherChanged()
    {
      var path = (Path)this.Child;

      switch (this.Weather)
      {
        case RaceCourseWeather.Fine:
          path.Data = _sun ??= this.TryFindResource("IconSun") as Geometry;
          path.Fill = _sunB ??= this.TryFindResource("FineForeground") as Brush;
          break;
        case RaceCourseWeather.Cloudy:
          path.Data = _cloud ??= this.TryFindResource("IconCloud") as Geometry;
          path.Fill = _cloudB ??= this.TryFindResource("CloudyForeground") as Brush;
          break;
        case RaceCourseWeather.Rainy:
          path.Data = _rain ??= this.TryFindResource("IconRain") as Geometry;
          path.Fill = _rainB ??= this.TryFindResource("RainyForeground") as Brush;
          break;
        case RaceCourseWeather.Drizzle:
          path.Data = _rainHeavy ??= this.TryFindResource("IconRainHeavy") as Geometry;
          path.Fill = _rainHeavyB ??= this.TryFindResource("DrizzleForeground") as Brush;
          break;
        case RaceCourseWeather.Snow:
          path.Data = _snow ??= this.TryFindResource("IconSnow") as Geometry;
          path.Fill = _snowB ??= this.TryFindResource("LightSnowForeground") as Brush;
          break;
        case RaceCourseWeather.LightSnow:
          path.Data = _snowHeavy ??= this.TryFindResource("IconSnowHeavy") as Geometry;
          path.Fill = _snowHeavyB ??= this.TryFindResource("SnowForeground") as Brush;
          break;
        default:
          path.Data = null;
          break;
      }
    }
  }
}
