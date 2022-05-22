using KmyKeiba.JVLink.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  [Index(nameof(RaceKey), nameof(Key), nameof(RiderCode), nameof(TrainerCode), nameof(Course))]
  public class RaceHorseData : DataBase<RaceHorse>
  {
    [StringLength(16)]
    public string Key { get; set; } = string.Empty;

    [StringLength(20)]
    public string RaceKey { get; set; } = string.Empty;

    [StringLength(72)]
    public string Name { get; set; } = string.Empty;

    public short Age { get; set; }

    public HorseSex Sex { get; set; }

    public HorseType Type { get; set; }

    public HorseBodyColor Color { get; set; }

    [Column("CourseCode")]
    public RaceCourse Course { get; set; }

    /// <summary>
    /// マーク
    /// </summary>
    public RaceHorseMark Mark { get; set; }

    /// <summary>
    /// 番号
    /// </summary>
    public short Number { get; set; }

    /// <summary>
    /// 枠版
    /// </summary>
    public short FrameNumber { get; set; }

    /// <summary>
    /// 着順
    /// </summary>
    public short ResultOrder { get; set; }

    /// <summary>
    /// 着差
    /// </summary>
    public short ResultLength1 { get; set; }

    public short ResultLength2 { get; set; }

    public short ResultLength3 { get; set; }

    /// <summary>
    /// 異常結果
    /// </summary>
    public RaceAbnormality AbnormalResult { get; set; }

    /// <summary>
    /// 人気
    /// </summary>
    public short Popular { get; set; }

    /// <summary>
    /// 走破タイム
    /// </summary>
    public TimeSpan ResultTime { get; set; }

    public short FirstCornerOrder { get; set; }

    public short SecondCornerOrder { get; set; }

    public short ThirdCornerOrder { get; set; }

    public short FourthCornerOrder { get; set; }

    /// <summary>
    /// 騎手コード
    /// </summary>
    [StringLength(8)]
    public string RiderCode { get; set; } = string.Empty;

    /// <summary>
    /// 騎手の名前
    /// </summary>
    [StringLength(16)]
    public string RiderName { get; set; } = string.Empty;

    /// <summary>
    /// 斤量
    /// </summary>
    public short RiderWeight { get; set; }

    [StringLength(8)]
    public string TrainerCode { get; set; } = string.Empty;

    [StringLength(16)]
    public string TrainerName { get; set; } = string.Empty;

    [StringLength(8)]
    public string OwnerCode { get; set; } = string.Empty;

    [StringLength(128)]
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
    /// 最低複勝オッズ
    /// </summary>
    public short PlaceOddsMin { get; set; }

    /// <summary>
    /// 最大複勝オッズ
    /// </summary>
    public short PlaceOddsMax { get; set; }

    /// <summary>
    /// 後３ハロンタイム
    /// </summary>
    public TimeSpan AfterThirdHalongTime { get; set; }

    /// <summary>
    /// 後３ハロンタイムの順位
    /// </summary>
    public short AfterThirdHalongTimeOrder { get; set; }

    /// <summary>
    /// 脚質
    /// </summary>
    public RunningStyle RunningStyle { get; set; }

    /// <summary>
    /// 脚質を手動で設定したか
    /// </summary>
    public bool IsRunningStyleSetManually { get; set; }

    /// <summary>
    /// 前回のレースは何日前か
    /// </summary>
    public short PreviousRaceDays { get; set; }

    /// <summary>
    /// 勝負服の模様
    /// </summary>
    [StringLength(120)]
    public string UniformFormat { get; set; } = string.Empty;

    /// <summary>
    /// 勝負服の画像
    /// </summary>
    [MaxLength(8000), Column(TypeName = "VARBINARY(8000)")]
    public byte[] UniformFormatData { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// メモ
    /// </summary>
    public string? Memo { get; set; }

    public override void SetEntity(RaceHorse entity)
    {
      this.LastModified = entity.LastModified;
      this.DataStatus = entity.DataStatus;
      this.Key = entity.Key;
      this.Name = entity.Name;
      this.Age = entity.Age;
      this.Sex = entity.Sex;
      this.Type = entity.Type;
      this.Color = entity.Color;
      this.Number = entity.Number;
      this.Popular = entity.Popular;
      this.RaceKey = entity.RaceKey;
      this.Course = entity.Course;
      this.ResultOrder = entity.ResultOrder;
      this.ResultTime = entity.ResultTime;
      this.ResultLength1 = entity.ResultLength1;
      this.ResultLength2 = entity.ResultLength2;
      this.ResultLength3 = entity.ResultLength3;
      this.FrameNumber = entity.FrameNumber;
      this.FirstCornerOrder = entity.FirstCornerOrder;
      this.SecondCornerOrder = entity.SecondCornerOrder;
      this.ThirdCornerOrder = entity.ThirdCornerOrder;
      this.FourthCornerOrder = entity.FourthCornerOrder;
      this.RiderCode = entity.RiderCode;
      this.RiderName = entity.RiderName;
      this.RiderWeight = entity.RiderWeight;
      this.TrainerCode = entity.TrainerCode;
      this.TrainerName = entity.TrainerName;
      this.OwnerCode = entity.OwnerCode;
      this.OwnerName = entity.OwnerName;
      this.Weight = entity.Weight;
      this.WeightDiff = entity.WeightDiff;
      this.Odds = entity.Odds;
      this.AfterThirdHalongTime = entity.AfterThirdHalongTime;
      this.AbnormalResult = entity.AbnormalResult;

      if (!this.IsRunningStyleSetManually)
      {
        this.RunningStyle = entity.RunningStyle;
      }

      if (this.UniformFormat != entity.UniformFormat)
      {
        this.UniformFormat = entity.UniformFormat;
        this.UniformFormatData = new byte[0];
      }
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

  public enum RaceHorseMark : short
  {
    Default = 0,
    DoubleCircle = 1,
    Circle = 2,
    FilledTriangle = 3,
    Triangle = 4,
    Star = 5,
    Deleted = 6,
  }
}
