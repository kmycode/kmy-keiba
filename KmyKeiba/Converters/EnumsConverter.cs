﻿using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
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

    private static readonly Brush _baseBrush = Application.Current.TryFindResource("BaseForeground") as Brush ?? Brushes.Red;
    private static readonly Brush _goodBrush = Application.Current.TryFindResource("GoodForeground") as Brush ?? Brushes.Red;
    private static readonly Brush _badBrush = Application.Current.TryFindResource("BadForeground") as Brush ?? Brushes.Blue;
    private static readonly Brush _unknownBrush = Application.Current.TryFindResource("SubForeground") as Brush ?? Brushes.Gray;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      #region JV-Link

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
      if (value is TicketType type)
      {
        return type switch
        {
          TicketType.Single => "単勝",
          TicketType.Place => "複勝",
          TicketType.FrameNumber => "枠連",
          TicketType.QuinellaPlace => "ワイド",
          TicketType.Quinella => "馬連",
          TicketType.Exacta => "馬単",
          TicketType.Trio => "三連複",
          TicketType.Trifecta => "三連単",
          _ => string.Empty,
        };
      }
      if (value is HorseBodyColor color)
      {
        return color switch
        {
          HorseBodyColor.Chestnut => "栗",
          HorseBodyColor.DarkChestnut => "栃栗",
          HorseBodyColor.Bay => "鹿",
          HorseBodyColor.DarkBay => "黒鹿",
          HorseBodyColor.Brown => "青鹿",
          HorseBodyColor.Black => "青",
          HorseBodyColor.Grey => "芦",
          _ => "不明",
        };
      }

      #endregion

      #region JRDB

      if (value is HorseClimb climb)
      {
        if (targetType == typeof(string))
        {
          if (parameter?.ToString() == "Description")
          {
            return climb.ToDescriptionString();
          }
          return climb.ToLabelString();
        }
      }

      #endregion

      if (value is RacePace pace)
      {
        if (targetType == typeof(string))
        {
          return pace.GetLabel() ?? string.Empty;
        }
        if (targetType == typeof(Brush))
        {
          return pace switch
          {
            RacePace.VeryLow => _badBrush,
            RacePace.Low => _badBrush,
            RacePace.Standard => _baseBrush,
            RacePace.High => _goodBrush,
            RacePace.VeryHigh => _goodBrush,
            RacePace.Unknown => _unknownBrush,
            _ => _baseBrush,
          };
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
