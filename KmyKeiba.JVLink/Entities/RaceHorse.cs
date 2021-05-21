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
    /// <summary>
    /// 出場するレースID
    /// </summary>
    public string RaceKey { get; set; } = string.Empty;

    /// <summary>
    /// 名前
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 番号
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// 枠番
    /// </summary>
    public int FrameNumber { get; set; }

    /// <summary>
    /// 着順
    /// </summary>
    public int ResultOrder { get; set; }

    /// <summary>
    /// 人気
    /// </summary>
    public int Popular { get; set; }

    /// <summary>
    /// 走破タイム
    /// </summary>
    public TimeSpan ResultTime { get; set; }

    public int FirstCornerOrder { get; set; }

    public int SecondCornerOrder { get; set; }

    public int ThirdCornerOrder { get; set; }

    public int FourthCornerOrder { get; set; }

    /// <summary>
    /// 騎手コード
    /// </summary>
    public string RiderCode { get; set; } = string.Empty;

    /// <summary>
    /// 騎手の名前
    /// </summary>
    public string RiderName { get; set; } = string.Empty;

    /// <summary>
    /// 単勝オッズ
    /// </summary>
    public double Odds { get; set; }

    /// <summary>
    /// 後３ハロンタイム
    /// </summary>
    public TimeSpan AfterThirdHalongTime { get; set; }

    internal RaceHorse()
    {
    }

    internal static RaceHorse FromJV(JVData_Struct.JV_SE_RACE_UMA uma)
    {
      int.TryParse(uma.Umaban.Trim(), out int num);
      int.TryParse(uma.Wakuban.Trim(), out int wakuNum);
      int.TryParse(uma.KakuteiJyuni.Trim(), out int result);
      int.TryParse(uma.Ninki.Trim(), out int pop);
      int.TryParse(uma.Jyuni1c.Trim(), out int corner1);
      int.TryParse(uma.Jyuni2c.Trim(), out int corner2);
      int.TryParse(uma.Jyuni3c.Trim(), out int corner3);
      int.TryParse(uma.Jyuni4c.Trim(), out int corner4);
      double.TryParse(uma.Odds.Trim(), out double odds);

      int.TryParse(uma.Time.Substring(0, 1), out int timeMinutes);
      int.TryParse(uma.Time.Substring(1, 2), out int timeSeconds);
      int.TryParse(uma.Time.Substring(3, 1), out int timeMilliSeconds);

      TimeSpan halongTime;
      if (int.TryParse(uma.HaronTimeL3, out int halongTime10))
      {
        halongTime = TimeSpan.FromSeconds((float)halongTime10 / 10);
      }
      else
      {
        halongTime = default;
      }

      var horse = new RaceHorse
      {
        LastModified = uma.head.MakeDate.ToDateTime(),
        RaceKey = uma.id.ToRaceKey(),
        Name = uma.Bamei.Trim(),
        Number = num,
        FrameNumber = wakuNum,
        ResultOrder = result,
        Popular = pop,
        FirstCornerOrder = corner1,
        SecondCornerOrder = corner2,
        ThirdCornerOrder = corner3,
        FourthCornerOrder = corner4,
        ResultTime = new TimeSpan(0, 0, timeMinutes, timeSeconds, timeMilliSeconds * 100),
        RiderCode = uma.KisyuCode,
        RiderName = uma.KisyuRyakusyo.Trim(),
        Odds = odds,
        AfterThirdHalongTime = halongTime,
      };
      return horse;
    }

    public override int GetHashCode() => this.Name.GetHashCode();
  }
}
