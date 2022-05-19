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
  class EnumsConverter : IValueConverter
  {
    private static readonly Brush _maleBrush = Application.Current.TryFindResource("MaleForeground") as Brush ?? Brushes.Blue;
    private static readonly Brush _femaleBrush = Application.Current.TryFindResource("FemaleForeground") as Brush ?? Brushes.Red;
    private static readonly Brush _castratedBrush = Application.Current.TryFindResource("CastratedForeground") as Brush ?? Brushes.Green;
    private static readonly Brush _miuraBrush = Application.Current.TryFindResource("TCMiuraForeground") as Brush ?? Brushes.Blue;
    private static readonly Brush _rittoBrush = Application.Current.TryFindResource("TCRittoForeground") as Brush ?? Brushes.Blue;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is HorseSex sex)
      {
        if (targetType == typeof(string))
        {
          return sex switch
          {
            HorseSex.Male => "牡",
            HorseSex.Female => "牝",
            HorseSex.Castrated => "騙",
            _ => "？",
          };
        }
        if (targetType == typeof(Brush))
        {
          return sex switch
          {
            HorseSex.Male => _maleBrush,
            HorseSex.Female => _femaleBrush,
            HorseSex.Castrated => _castratedBrush,
            _ => "？",
          };
        }
      }
      if (value is TrainingCenter trc)
      {
        if (targetType == typeof(string) || targetType == typeof(object))
        {
          return trc switch
          {
            TrainingCenter.Miura => "美浦",
            TrainingCenter.Ritto => "栗東",
            _ => string.Empty,
          };
        }
        if (targetType == typeof(Brush))
        {
          return trc switch
          {
            TrainingCenter.Miura => _miuraBrush,
            TrainingCenter.Ritto => _rittoBrush,
            _ => Brushes.Transparent,
          };
        }
      }
      if (value is RaceAbnormality ab)
      {
        return ab switch
        {
          RaceAbnormality.Scratched => "出走取消",
          RaceAbnormality.ExcludedByStarters => "発走除外",
          RaceAbnormality.ExcludedByStewards => "競走除外",
          RaceAbnormality.FailToFinish => "競走中止",
          RaceAbnormality.Disqualified => "失格",
          RaceAbnormality.Remount => "再騎乗",
          RaceAbnormality.DisqualifiedAndPlaced => "降着",
          _ => string.Empty,
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
