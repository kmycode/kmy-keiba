using KmyKeiba.JVLink.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Data
{
  public class RaceHorseData : DataBase<RaceHorse>
  {
    public string RaceKey { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 番号
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// 枠版
    /// </summary>
    public int FrameNumber { get; set; }

    /// <summary>
    /// 着順
    /// </summary>
    public int ResultOrder { get; set; }

    /// <summary>
    /// 異常結果
    /// </summary>
    public RaceAbnormality AbnormalResult { get; set; }

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
    /// 斤量
    /// </summary>
    public float RiderWeight { get; set; }

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
    public float Odds { get; set; }

    /// <summary>
    /// 後３ハロンタイム
    /// </summary>
    public TimeSpan AfterThirdHalongTime { get; set; }

    /// <summary>
    /// 後３ハロンタイムの順位
    /// </summary>
    public int AfterThirdHalongTimeOrder { get; set; }

    /// <summary>
    /// 脚質
    /// </summary>
    public RunningStyle RunningStyle { get; set; }

    public override void SetEntity(RaceHorse entity)
    {
      this.LastModified = entity.LastModified;
      this.DataStatus = entity.DataStatus;
      this.Name = entity.Name;
      this.Number = entity.Number;
      this.Popular = entity.Popular;
      this.RaceKey = entity.RaceKey;
      this.ResultOrder = entity.ResultOrder;
      this.ResultTime = entity.ResultTime;
      this.FrameNumber = entity.FrameNumber;
      this.FirstCornerOrder = entity.FirstCornerOrder;
      this.SecondCornerOrder = entity.SecondCornerOrder;
      this.ThirdCornerOrder = entity.ThirdCornerOrder;
      this.FourthCornerOrder = entity.FourthCornerOrder;
      this.RiderCode = entity.RiderCode;
      this.RiderName = entity.RiderName;
      this.RiderWeight = entity.RiderWeight;
      this.Weight = entity.Weight;
      this.WeightDiff = entity.WeightDiff;
      this.Odds = entity.Odds;
      this.AfterThirdHalongTime = entity.AfterThirdHalongTime;
      this.RunningStyle = entity.RunningStyle;
      this.AbnormalResult = entity.AbnormalResult;
    }

    public override bool IsEquals(DataBase<RaceHorse> b)
    {
      var c = (RaceHorseData)b;
      return this.Name == c.Name && this.RaceKey == c.RaceKey;
    }

    public override int GetHashCode()
    {
      return this.Name.GetHashCode() + this.RaceKey.GetHashCode();
    }
  }
}
