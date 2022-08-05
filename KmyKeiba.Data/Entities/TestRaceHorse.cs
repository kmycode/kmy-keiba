using KmyKeiba.JVLink.Entities;
using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  public class TestRaceHorse : EntityBase
  {
    public string Key { get; set; } = string.Empty;

    public DateTime PassDate { get; set; }

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

    public HorseBodyColor Color { get; set; }

    /// <summary>
    /// 番号
    /// </summary>
    public short Number { get; set; }

    /// <summary>
    /// 競馬場
    /// </summary>
    public RaceCourse Course { get; set; }

    /// <summary>
    /// 着順
    /// </summary>
    public short ResultOrder { get; set; }

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

    /// <summary>
    /// 体重
    /// </summary>
    public short Weight { get; set; }

    /// <summary>
    /// 体重の増減
    /// </summary>
    public short WeightDiff { get; set; }

    public TestRaceType TestType { get; set; }

    public TestRaceResult TestResult { get; set; }

    public TestRaceFailedType FailedType { get; set; }

    public short AfterThirdHalongTimeValue { get; set; }

    internal TestRaceHorse()
    {
    }

    public static TestRaceHorse FromJV(JVData_Struct.JV_NS_NOSI_UMA uma)
    {
      short.TryParse(uma.Barei.Trim(), out short age);
      short.TryParse(uma.SexCD.Trim(), out short sex);
      short.TryParse(uma.Umaban.Trim(), out short num);
      short.TryParse(uma.KakuteiJyuni.Trim(), out short result);
      short.TryParse(uma.Jyuni1c.Trim(), out short corner1);
      short.TryParse(uma.Jyuni2c.Trim(), out short corner2);
      short.TryParse(uma.Jyuni3c.Trim(), out short corner3);
      short.TryParse(uma.Jyuni4c.Trim(), out short corner4);
      int.TryParse(uma.IJyoCD.Trim(), out int abnormal);
      int.TryParse(uma.Futan.Trim(), out int riderWeight);
      short.TryParse(uma.BaTaijyu.Trim(), out short weight);
      short.TryParse(uma.ZogenSa.Trim(), out short weightDiff);
      int.TryParse(uma.id.JyoCD.Trim(), out int course);
      short.TryParse(uma.KeiroCD, out var color);

      short.TryParse(uma.NouryokuSyuruiCD, out var testType);
      short.TryParse(uma.GohiCD, out var testResult);
      short.TryParse(uma.RiyuCD, out var testFailedType);

      int.TryParse(uma.Time.Substring(0, 1), out int timeMinutes);
      int.TryParse(uma.Time.Substring(1, 2), out int timeSeconds);
      int.TryParse(uma.Time.Substring(3, 1), out int timeMilliSeconds);
      short.TryParse(uma.HaronTimeL3, out short halongTime10);

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

      var horse = new TestRaceHorse
      {
        LastModified = uma.head.MakeDate.ToDateTime(),
        DataStatus = uma.head.DataKubun.ToDataStatus(),
        Key = uma.KettoNum,
        RaceKey = uma.id.ToTestRaceKey(),
        Age = age,
        Sex = (HorseSex)sex,
        Color = (HorseBodyColor)color,
        Course = (RaceCourse)course,
        Name = uma.Bamei.Trim(),
        Number = num,
        ResultOrder = result,
        ResultLength1 = GetLength(uma.ChakusaCD),
        ResultLength2 = GetLength(uma.ChakusaCDP),
        ResultLength3 = GetLength(uma.ChakusaCDPP),
        AbnormalResult = (RaceAbnormality)abnormal,
        PassDate = uma.GohiNengappi.ToDateTime(),
        FirstCornerOrder = corner1,
        SecondCornerOrder = corner2,
        ThirdCornerOrder = corner3,
        FourthCornerOrder = corner4,
        ResultTimeValue = (short)(timeMinutes * 600 + timeSeconds * 10 + timeMilliSeconds),
        TestType = (TestRaceType)testType,
        TestResult = (TestRaceResult)testResult,
        FailedType = (TestRaceFailedType)testFailedType,
        RiderCode = uma.KisyuCode.Trim(),
        RiderName = uma.KisyuRyakusyo.Trim(),
        RiderWeight = (short)riderWeight,
        TrainerCode = uma.ChokyosiCode,
        TrainerName = uma.ChokyosiRyakusyo,
        Weight = weight,
        WeightDiff = (short)(uma.ZogenFugo == "+" ? weightDiff : -weightDiff),
        AfterThirdHalongTimeValue = (short)halongTime10,
      };
      return horse;
    }

    public override int GetHashCode() => this.Name.GetHashCode();
  }

  public enum TestRaceType : short
  {
  }

  public enum TestRaceResult : short
  {
    Unknown = 0,
    Passed = 1,
    Failure = 2,

    /// <summary>
    /// 不参加
    /// </summary>
    NonParticipation = 3,
  }

  public enum TestRaceFailedType : short
  {
    Unknown = 0,

    /// <summary>
    /// 発走調教不良
    /// </summary>
    BadStart = 1,

    /// <summary>
    /// 不明
    /// </summary>
    Unset = 2,

    /// <summary>
    /// タイムオーバー
    /// </summary>
    TimeOver = 3,
  }
}
