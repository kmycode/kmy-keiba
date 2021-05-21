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

    public override void SetEntity(RaceHorse entity)
    {
      this.LastModified = entity.LastModified;
      this.Name = entity.Name;
      this.Number = entity.Number;
      this.Popular = entity.Popular;
      this.RaceKey = entity.RaceKey;
      this.ResultOrder = entity.ResultOrder;
      this.FrameNumber = entity.FrameNumber;
      this.FirstCornerOrder = entity.FirstCornerOrder;
      this.SecondCornerOrder = entity.SecondCornerOrder;
      this.ThirdCornerOrder = entity.ThirdCornerOrder;
      this.FourthCornerOrder = entity.FourthCornerOrder;
      this.RiderCode = entity.RiderCode;
      this.RiderName = entity.RiderName;
      this.Odds = entity.Odds;
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
