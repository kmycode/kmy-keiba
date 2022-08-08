using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  public class RaceHorse : EntityBase
  {
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 出場するレースID
    /// </summary>
    public string RaceKey { get; set; } = string.Empty;

    /// <summary>
    /// 名前
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 血統登録番号
    /// </summary>
    public string Code => this.Key;

    /// <summary>
    /// 年齢
    /// </summary>
    public short Age { get; set; }

    /// <summary>
    /// 性別
    /// </summary>
    public HorseSex Sex { get; set; }

    public HorseType Type { get; set; }

    public HorseBodyColor Color { get; set; }

    /// <summary>
    /// 番号
    /// </summary>
    public short Number { get; set; }

    /// <summary>
    /// 枠番
    /// </summary>
    public short FrameNumber { get; set; }

    /// <summary>
    /// 競馬場
    /// </summary>
    public RaceCourse Course { get; set; }

    /// <summary>
    /// 着順
    /// </summary>
    public short ResultOrder { get; set; }

    /// <summary>
    /// 入線順位
    /// </summary>
    public short GoalOrder { get; set; }

    /// <summary>
    /// １着とのタイム差（１着の場合は２着との差）
    /// </summary>
    public short TimeDifference { get; set; }

    /// <summary>
    /// 異常結果
    /// </summary>
    public RaceAbnormality AbnormalResult { get; set; }

    /// <summary>
    /// 着差
    /// </summary>
    public short ResultLength1 { get; set; }

    public short ResultLength2 { get; set; }

    public short ResultLength3 { get; set; }

    /// <summary>
    /// 人気
    /// </summary>
    public short Popular { get; set; }

    /// <summary>
    /// 走破タイム
    /// </summary>
    public TimeSpan ResultTime { get; set; }

    public short ResultTimeValue { get; set; }

    public short FirstCornerOrder { get; set; }

    public short SecondCornerOrder { get; set; }

    public short ThirdCornerOrder { get; set; }

    public short FourthCornerOrder { get; set; }

    /// <summary>
    /// 騎手コード
    /// </summary>
    public string RiderCode { get; set; } = string.Empty;

    /// <summary>
    /// 騎手の名前
    /// </summary>
    public string RiderName { get; set; } = string.Empty;

    /// <summary>
    /// 斤量
    /// </summary>
    public short RiderWeight { get; set; }

    public string TrainerCode { get; set; } = string.Empty;

    public string TrainerName { get; set; } = string.Empty;

    public string OwnerCode { get; set; } = string.Empty;

    public string OwnerName { get; set; } = string.Empty;

    /// <summary>
    /// ブリンカー使用しているか
    /// </summary>
    public bool IsBlinkers { get; set; }

    /// <summary>
    /// 体重
    /// </summary>
    public short Weight { get; set; }

    /// <summary>
    /// 体重の増減
    /// </summary>
    public short WeightDiff { get; set; }

    /// <summary>
    /// 単勝オッズ
    /// </summary>
    public short Odds { get; set; }

    /// <summary>
    /// 後３ハロンタイム
    /// </summary>
    public TimeSpan AfterThirdHalongTime { get; set; }

    public short AfterThirdHalongTimeValue { get; set; }

    /// <summary>
    /// 脚質
    /// </summary>
    public RunningStyle RunningStyle { get; set; }

    /// <summary>
    /// 勝負服の模様
    /// </summary>
    public string UniformFormat { get; set; } = string.Empty;

    internal RaceHorse()
    {
    }

    public static RaceHorse FromJV(JVData_Struct.JV_SE_RACE_UMA uma)
    {
      short.TryParse(uma.Barei.Trim(), out short age);
      short.TryParse(uma.SexCD.Trim(), out short sex);
      short.TryParse(uma.Umaban.Trim(), out short num);
      short.TryParse(uma.UmaKigoCD, out short tc);
      short.TryParse(uma.Wakuban.Trim(), out short wakuNum);
      short.TryParse(uma.KakuteiJyuni.Trim(), out short result);
      short.TryParse(uma.NyusenJyuni.Trim(), out short result2);
      short.TryParse(uma.Ninki.Trim(), out short pop);
      short.TryParse(uma.Jyuni1c.Trim(), out short corner1);
      short.TryParse(uma.Jyuni2c.Trim(), out short corner2);
      short.TryParse(uma.Jyuni3c.Trim(), out short corner3);
      short.TryParse(uma.Jyuni4c.Trim(), out short corner4);
      short.TryParse(uma.Odds.Trim(), out short odds);
      int.TryParse(uma.IJyoCD.Trim(), out int abnormal);
      int.TryParse(uma.Futan.Trim(), out int riderWeight);
      short.TryParse(uma.BaTaijyu.Trim(), out short weight);
      short.TryParse(uma.ZogenSa.Trim(), out short weightDiff);
      int.TryParse(uma.KyakusituKubun.Trim(), out int runningStyle);
      int.TryParse(uma.id.JyoCD.Trim(), out int course);
      short.TryParse(uma.KeiroCD, out var color);
      short.TryParse(uma.TimeDiff, out var timeDiff);

      int.TryParse(uma.Time.Substring(0, 1), out int timeMinutes);
      int.TryParse(uma.Time.Substring(1, 2), out int timeSeconds);
      int.TryParse(uma.Time.Substring(3, 1), out int timeMilliSeconds);

      TimeSpan halongTime;
      if (int.TryParse(uma.HaronTimeL3, out int halongTime10))
      {
        halongTime = new TimeSpan(days: 0, hours: 0, minutes: 0, seconds: halongTime10 / 10, milliseconds: halongTime10 % 10 * 100);
      }
      else
      {
        halongTime = default;
      }

      short GetLength(string len)
      {
        switch (len)
        {
          case "A  ":
            return (short)HorseLength.Head;
          case "D  ":
            return (short)HorseLength.Same;
          case "H  ":
            return (short)HorseLength.Nose;
          case "K  ":
            return (short)HorseLength.Neck;
          case "Z  ":
            return 10 * 100;
          case "T  ":
            return 15 * 100;
          case "   ":
            return (short)HorseLength.Unknown;
        }
        if (int.TryParse(len, out var num))
        {
          int.TryParse(len[0].ToString(), out var a);
          int.TryParse(len[1].ToString(), out var b);
          int.TryParse(len[2].ToString(), out var c);
          if (c > 0)
          {
            return (short)((a + b / (float)c) * 100);
          }
          return (short)(a * 100);
        }
        return (short)HorseLength.Unknown;
      }

      var horse = new RaceHorse
      {
        LastModified = uma.head.MakeDate.ToDateTime(),
        DataStatus = uma.head.DataKubun.ToDataStatus(),
        Key = uma.KettoNum,
        RaceKey = uma.id.ToRaceKey(),
        Age = age,
        Sex = (HorseSex)sex,
        Type = Horse.ToHorseType(tc),
        Color = (HorseBodyColor)color,
        Course = (RaceCourse)course,
        Name = uma.Bamei.Trim(),
        Number = num,
        FrameNumber = wakuNum,
        ResultOrder = result,
        GoalOrder = result2,
        TimeDifference = timeDiff,
        ResultLength1 = GetLength(uma.ChakusaCD),
        ResultLength2 = GetLength(uma.ChakusaCDP),
        ResultLength3 = GetLength(uma.ChakusaCDPP),
        AbnormalResult = (RaceAbnormality)abnormal,
        Popular = pop,
        FirstCornerOrder = corner1,
        SecondCornerOrder = corner2,
        ThirdCornerOrder = corner3,
        FourthCornerOrder = corner4,
        ResultTime = new TimeSpan(0, 0, timeMinutes, timeSeconds, timeMilliSeconds * 100),
        ResultTimeValue = (short)(timeMinutes * 600 + timeSeconds * 10 + timeMilliSeconds),
        RiderCode = uma.KisyuCode.Trim(),
        RiderName = uma.KisyuRyakusyo.Trim(),
        RiderWeight = (short)riderWeight,
        TrainerCode = uma.ChokyosiCode,
        TrainerName = uma.ChokyosiRyakusyo,
        OwnerCode = uma.BanusiCode.Trim(),
        OwnerName = uma.BanusiName.Trim(),
        Weight = weight,
        WeightDiff = (short)(uma.ZogenFugo == "+" ? weightDiff : -weightDiff),
        IsBlinkers = uma.Blinker == "1",
        Odds = odds,
        AfterThirdHalongTime = halongTime,
        AfterThirdHalongTimeValue = (short)halongTime10,
        RunningStyle = (RunningStyle)runningStyle,
        UniformFormat = uma.Fukusyoku.Trim(),
      };
      return horse;
    }

    public override int GetHashCode() => this.Name.GetHashCode();
  }

  class RaceAbnormalityInfoAttribute : Attribute
  {
    public string Label { get; }

    public RaceAbnormalityInfoAttribute(string label)
    {
      this.Label = label;
    }
  }

  public enum RaceAbnormality : short
  {
    [RaceAbnormalityInfo("")]
    Unknown = 0,

    [RaceAbnormalityInfo("取消")]
    Scratched = 1,

    [RaceAbnormalityInfo("発除")]
    ExcludedByStarters = 2,

    [RaceAbnormalityInfo("競除")]
    ExcludedByStewards = 3,

    [RaceAbnormalityInfo("中止")]
    FailToFinish = 4,

    [RaceAbnormalityInfo("失格")]
    Disqualified = 5,

    [RaceAbnormalityInfo("再騎")]
    Remount = 6,

    [RaceAbnormalityInfo("降着")]
    DisqualifiedAndPlaced = 7,
  }

  public enum HorseLength : short
  {
    Unknown = 0,

    /// <summary>
    /// あたま
    /// </summary>
    Head = 1,

    /// <summary>
    /// 同着
    /// </summary>
    Same = 2,

    /// <summary>
    /// はな
    /// </summary>
    Nose = 3,

    /// <summary>
    /// くび
    /// </summary>
    Neck = 4,
  }

  public enum HorseSex : short
  {
    Unknown,
    Male = 1,
    Female = 2,
    Castrated = 3,
  }

  public enum RunningStyle : short
  {
    Unknown,

    /// <summary>
    /// 逃げ
    /// </summary>
    FrontRunner = 1,

    /// <summary>
    /// 先行
    /// </summary>
    Stalker = 2,

    /// <summary>
    /// 差し
    /// </summary>
    Sotp = 3,

    /// <summary>
    /// 追込
    /// </summary>
    SaveRunner = 4,

    /// <summary>
    /// 不明（追い込み失敗、型なし）（独自分析用の値）
    /// </summary>
    NotClear = 101,
  }
}
