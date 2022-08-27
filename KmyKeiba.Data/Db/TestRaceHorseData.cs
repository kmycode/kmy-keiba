using KmyKeiba.JVLink.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  public class TestRaceHorseData : DataBase<TestRaceHorse>
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

    public override void SetEntity(TestRaceHorse race)
    {
      this.PassDate = race.PassDate;
      this.Key = race.Key;
      this.RaceKey = race.RaceKey;
      this.Name = race.Name;
      this.Age = race.Age;
      this.Color = race.Color;
      this.Sex = race.Sex;
      this.Number = race.Number;
      this.Course = race.Course;
      this.ResultOrder = race.ResultOrder;
      this.AbnormalResult = race.AbnormalResult;
      this.ResultLength1 = race.ResultLength1;
      this.ResultLength2 = race.ResultLength2;
      this.ResultLength3 = race.ResultLength3;
      this.RiderCode = race.RiderCode;
      this.RiderName = race.RiderName;
      this.TrainerCode = race.TrainerCode;
      this.TrainerName = race.TrainerName;
      this.Weight = race.Weight;
      this.WeightDiff = race.WeightDiff;
      this.RiderWeight = race.RiderWeight;
      this.TestType = race.TestType;
      this.TestResult = race.TestResult;
      this.FailedType = race.FailedType;
      this.AfterThirdHalongTimeValue = race.AfterThirdHalongTimeValue;
    }

    public override bool IsEquals(DataBase<TestRaceHorse> b)
    {
      var c = (TestRaceHorseData)b;
      return this.Key == c.Key && this.RaceKey == c.RaceKey;
    }

    public override int GetHashCode()
    {
      return (this.Key + this.RaceKey).GetHashCode();
    }
  }
}
