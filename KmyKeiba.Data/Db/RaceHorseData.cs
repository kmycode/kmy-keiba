using KmyKeiba.JVLink.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update;
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

    public HorseExtraDataState ExtraDataState { get; set; }

    public short ExtraDataVersion { get; set; }

    public override void SetEntity(RaceHorse entity)
    {
      if (this.LastModified != entity.LastModified)
        this.LastModified = entity.LastModified;
      if (this.DataStatus != entity.DataStatus)
        this.DataStatus = entity.DataStatus;
      if (this.Key != entity.Key)
        this.Key = entity.Key;
      if (this.Name != entity.Name)
        this.Name = entity.Name;
      if (this.Age != entity.Age)
        this.Age = entity.Age;
      if (this.Sex != entity.Sex)
        this.Sex = entity.Sex;
      if (this.Type != entity.Type)
        this.Type = entity.Type;
      if (this.Color != entity.Color)
        this.Color = entity.Color;
      if (this.Number != entity.Number)
        this.Number = entity.Number;
      if (this.RaceKey != entity.RaceKey)
        this.RaceKey = entity.RaceKey;
      if (this.Course != entity.Course)
        this.Course = entity.Course;
      if (this.ResultOrder != entity.ResultOrder)
        this.ResultOrder = entity.ResultOrder;
      if (this.GoalOrder != entity.GoalOrder)
        this.GoalOrder = entity.GoalOrder;
      if (this.TimeDifference != entity.TimeDifference)
        this.TimeDifference = entity.TimeDifference;
      if (this.ResultTime != entity.ResultTime)
        this.ResultTime = entity.ResultTime;
      if (this.ResultTimeValue != entity.ResultTimeValue)
        this.ResultTimeValue = entity.ResultTimeValue;
      if (this.ResultLength1 != entity.ResultLength1)
        this.ResultLength1 = entity.ResultLength1;
      if (this.ResultLength2 != entity.ResultLength2)
        this.ResultLength2 = entity.ResultLength2;
      if (this.ResultLength3 != entity.ResultLength3)
        this.ResultLength3 = entity.ResultLength3;
      if (this.FrameNumber != entity.FrameNumber)
        this.FrameNumber = entity.FrameNumber;
      if (this.FirstCornerOrder != entity.FirstCornerOrder)
        this.FirstCornerOrder = entity.FirstCornerOrder;
      if (this.SecondCornerOrder != entity.SecondCornerOrder)
        this.SecondCornerOrder = entity.SecondCornerOrder;
      if (this.ThirdCornerOrder != entity.ThirdCornerOrder)
        this.ThirdCornerOrder = entity.ThirdCornerOrder;
      if (this.FourthCornerOrder != entity.FourthCornerOrder)
        this.FourthCornerOrder = entity.FourthCornerOrder;
      if (this.TrainerCode != entity.TrainerCode)
        this.TrainerCode = entity.TrainerCode;
      if (this.TrainerName != entity.TrainerName)
        this.TrainerName = entity.TrainerName;
      if (this.OwnerCode != entity.OwnerCode)
        this.OwnerCode = entity.OwnerCode;
      if (this.OwnerName != entity.OwnerName)
        this.OwnerName = entity.OwnerName;
      if (this.IsBlinkers != entity.IsBlinkers)
        this.IsBlinkers = entity.IsBlinkers;
      if (this.AfterThirdHalongTime != entity.AfterThirdHalongTime)
        this.AfterThirdHalongTime = entity.AfterThirdHalongTime;
      if (this.AfterThirdHalongTimeValue != entity.AfterThirdHalongTimeValue)
        this.AfterThirdHalongTimeValue = entity.AfterThirdHalongTimeValue;
      if (this.AbnormalResult != entity.AbnormalResult)
        this.AbnormalResult = entity.AbnormalResult;
      if (this.UniformFormat != entity.UniformFormat)
        this.UniformFormat = entity.UniformFormat;

      if (this.CanSetOdds(entity.Odds))
      {
        if (this.Odds != entity.Odds)
          this.Odds = entity.Odds;
        if (this.Popular != entity.Popular)
          this.Popular = entity.Popular;
        if (this.RiderCode != entity.RiderCode)
          this.RiderCode = entity.RiderCode;
        if (this.RiderName != entity.RiderName)
          this.RiderName = entity.RiderName;
        if (this.RiderWeight != entity.RiderWeight)
          this.RiderWeight = entity.RiderWeight;
        if (this.Weight != entity.Weight)
          this.Weight = entity.Weight;
        if (this.WeightDiff != entity.WeightDiff)
          this.WeightDiff = entity.WeightDiff;
      }

      if (!this.IsRunningStyleSetManually)
      {
        if (this.RunningStyle != entity.RunningStyle)
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
    Check = 7,
    Note = 8,
  }

  public enum HorseExtraDataState : short
  {
    Unset = 0,
    UntilRace = 1,
    AfterRace = 2,
    Ignored = 3,
  }
}
