using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Common
{
  internal static class EnumUtil
  {
    public static RaceHorseMark ToHorseMark(string num)
    {
      short.TryParse(num, out var markss);
      var mark = (RaceHorseMark)markss;

      return mark;
    }

    public static RaceHorseMark ToAppMark(this JdbcHorseMark mark)
    {
      return mark switch
      {
        JdbcHorseMark.DoubleCircle => RaceHorseMark.DoubleCircle,
        JdbcHorseMark.Circle => RaceHorseMark.Circle,
        JdbcHorseMark.FilledTriangle => RaceHorseMark.FilledTriangle,
        JdbcHorseMark.Triangle1 => RaceHorseMark.Triangle,
        JdbcHorseMark.Triangle2 => RaceHorseMark.Check,
        JdbcHorseMark.Star => RaceHorseMark.Star,
        JdbcHorseMark.Attention => RaceHorseMark.Note,
        _ => RaceHorseMark.Default,
      };
    }

    public static string ToLabelString(this RunningStyle style)
    {
      return style switch
      {
        RunningStyle.FrontRunner => "逃げ",
        RunningStyle.Stalker => "先行",
        RunningStyle.Sotp => "差し",
        RunningStyle.SaveRunner => "追込",
        RunningStyle.NotClear => "不明",
        _ => string.Empty,
      };
    }

    public static string ToLabelString(this RaceRiderWeightRule rule)
    {
      return rule switch
      {
        RaceRiderWeightRule.Handicap => "ハンデ",
        RaceRiderWeightRule.SpecialWeight => "別定",
        RaceRiderWeightRule.WeightForAge => "馬齢",
        RaceRiderWeightRule.SpecialWeightForAge => "定量",
        _ => "なし",
      };
    }

    public static string ToLabelString(this RaceHorseAreaRule area)
    {
      return area switch
      {
        RaceHorseAreaRule.Unknown => "限定なし",
        RaceHorseAreaRule.Mixed => "混合",
        RaceHorseAreaRule.Father => "（父）",
        RaceHorseAreaRule.Market => "（市）",
        RaceHorseAreaRule.Lottery => "（抽）",
        RaceHorseAreaRule.Lottery2 => "「抽」",
        RaceHorseAreaRule.MarketLottery => "（市）（抽）",
        RaceHorseAreaRule.LotteryWest => "（抽）関西",
        RaceHorseAreaRule.LotteryEast => "（抽）関東",
        RaceHorseAreaRule.Lottery2West => "「抽」関西",
        RaceHorseAreaRule.Lottery2East => "「抽」関東",
        RaceHorseAreaRule.MarketLotteryWest => "（市）（抽）関西",
        RaceHorseAreaRule.MarketLotteryEast => "（市）（抽）関東",
        RaceHorseAreaRule.Kyushu => "九州",
        RaceHorseAreaRule.International => "国際",
        RaceHorseAreaRule.LimitedHyogo => "兵庫など",
        RaceHorseAreaRule.LimitedSouthKanto => "南関東",
        RaceHorseAreaRule.JraCertificated => "JRA認定",
        _ => string.Empty,
      };
    }

    public static string ToLabelString(this TrackType type)
    {
      return type switch
      {
        TrackType.Flat => "平地",
        TrackType.Steeplechase => "障害",
        _ => string.Empty,
      };
    }

    public static string ToLabelString(this TrackGround ground)
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

    public static string ToLabelString(this TrackCornerDirection direction)
    {
      return direction switch
      {
        TrackCornerDirection.Left => "左",
        TrackCornerDirection.Right => "右",
        TrackCornerDirection.Straight => "直線",
        _ => string.Empty,
      };
    }

    public static string ToLabelString(this RaceCourseWeather wea)
    {
      return wea switch
      {
        RaceCourseWeather.Fine => "晴",
        RaceCourseWeather.Rainy => "雨",
        RaceCourseWeather.Cloudy => "曇",
        RaceCourseWeather.Drizzle => "小",
        RaceCourseWeather.Snow => "雪",
        RaceCourseWeather.LightSnow => "雪",
        _ => string.Empty,
      };
    }

    public static string ToLongLabelString(this RaceCourseWeather wea)
    {
      return wea switch
      {
        RaceCourseWeather.Fine => "晴",
        RaceCourseWeather.Rainy => "雨",
        RaceCourseWeather.Cloudy => "曇",
        RaceCourseWeather.Drizzle => "小雨",
        RaceCourseWeather.Snow => "雪",
        RaceCourseWeather.LightSnow => "小雪",
        _ => string.Empty,
      };
    }

    public static string ToLabelString(this RaceCourseCondition condition)
    {
      return condition switch
      {
        RaceCourseCondition.Standard => "良",
        RaceCourseCondition.Good => "稍",
        RaceCourseCondition.Yielding => "重",
        RaceCourseCondition.Soft => "不",
        _ => string.Empty,
      };
    }

    public static string ToLabelString(this TrackOption opt)
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
  }

  public static class ValueUtil
  {
    public static string ToMoneyLabel(long value)
    {
      if (value >= 1000_0000_0000)
      {
        return ((double)value / 1_0000_0000_0000).ToString("F2") + "兆";
      }
      else if (value >= 1000_0000)
      {
        return ((double)value / 1_0000_0000).ToString("F2") + "億";
      }
      else if (value >= 1_0000)
      {
        return ((double)value / 1_0000).ToString("F2") + "万";
      }
      else
      {
        return value.ToString();
      }
    }
  }
}
