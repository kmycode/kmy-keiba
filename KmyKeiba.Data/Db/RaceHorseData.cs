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
  [Index(nameof(RaceKey), nameof(Key))]
  [Index(nameof(RiderCode))]
  [Index(nameof(TrainerCode))]
  [Index(nameof(Key), nameof(RaceCount), nameof(RaceCountWithinRunning), nameof(RaceCountWithinRunningCompletely), nameof(RaceCountAfterLastRest))]
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
    /// 入線順位
    /// </summary>
    public short GoalOrder { get; set; }

    /// <summary>
    /// １着とのタイム差（１着の場合は２着との差）
    /// </summary>
    public short TimeDifference { get; set; }

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

    public short ResultTimeValue { get; set; }

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

    public short AfterThirdHalongTimeValue { get; set; }

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
    /// この馬にとって何度目のレースか
    /// </summary>
    public short RaceCount { get; set; }

    /// <summary>
    ///  競走除外とかでそもそも走らなかったレースを除く回数
    ///  走らなかったレースでは-2が設定される
    /// </summary>
    public short RaceCountWithinRunning { get; set; }

    /// <summary>
    ///  とにかく失敗したレースを除く回数
    ///  走らなかったレースでは-2が設定される
    /// </summary>
    public short RaceCountWithinRunningCompletely { get; set; }

    /// <summary>
    /// 一定日以上レース間隔のあいた直後のレースを 1 として、休養後のレース回数をカウントしていく
    /// </summary>
    public short RaceCountAfterLastRest { get; set; }

    /// <summary>
    /// 騎手の勝率マスターデータにこのデータは含まれているか
    /// </summary>
    public bool IsContainsRiderWinRate { get; set; }

    /// <summary>
    /// 勝負服の模様
    /// </summary>
    [StringLength(120)]
    public string UniformFormat { get; set; } = string.Empty;

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
      this.RaceKey = entity.RaceKey;
      this.Course = entity.Course;
      this.ResultOrder = entity.ResultOrder;
      this.GoalOrder = entity.GoalOrder;
      this.TimeDifference = entity.TimeDifference;
      this.ResultTime = entity.ResultTime;
      this.ResultTimeValue = entity.ResultTimeValue;
      this.ResultLength1 = entity.ResultLength1;
      this.ResultLength2 = entity.ResultLength2;
      this.ResultLength3 = entity.ResultLength3;
      this.FrameNumber = entity.FrameNumber;
      this.FirstCornerOrder = entity.FirstCornerOrder;
      this.SecondCornerOrder = entity.SecondCornerOrder;
      this.ThirdCornerOrder = entity.ThirdCornerOrder;
      this.FourthCornerOrder = entity.FourthCornerOrder;
      this.TrainerCode = entity.TrainerCode;
      this.TrainerName = entity.TrainerName;
      this.OwnerCode = entity.OwnerCode;
      this.OwnerName = entity.OwnerName;
      this.IsBlinkers = entity.IsBlinkers;
      this.AfterThirdHalongTime = entity.AfterThirdHalongTime;
      this.AfterThirdHalongTimeValue = entity.AfterThirdHalongTimeValue;
      this.AbnormalResult = entity.AbnormalResult;
      this.UniformFormat = entity.UniformFormat;

      if (this.CanSetOdds(entity.Odds))
      {
        this.Odds = entity.Odds;
        this.Popular = entity.Popular;
        this.RiderCode = entity.RiderCode;
        this.RiderName = entity.RiderName;
        this.RiderWeight = entity.RiderWeight;
        this.Weight = entity.Weight;
        this.WeightDiff = entity.WeightDiff;
      }

      if (!this.IsRunningStyleSetManually)
      {
        this.RunningStyle = entity.RunningStyle;
      }
    }

    public bool CanSetOdds(short odds)
    {
      if (this.Odds == default || odds != default) return true;

      if (this.AbnormalResult == RaceAbnormality.Scratched || this.AbnormalResult == RaceAbnormality.ExcludedByStarters)
      {
        return true;
      }
      return false;
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
