using KmyKeiba.JVLink.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  [Index(nameof(RaceKey), nameof(Key))]
  public class JrdbRaceHorseData : AppDataBase
  {
    public string RaceKey { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public JdbcRunningStyle RunningStyle { get; set; }

    public HorseDistanceAptitude DistanceAptitude { get; set; }

    public HorseClimb Climb { get; set; }

    public short IdmPoint { get; set; }

    public short RiderPoint { get; set; }

    public short InfoPoint { get; set; }

    public short TotalPoint { get; set; }

    public short BaseOdds { get; set; }

    public short BasePopular { get; set; }

    public short BasePlaceBetsOdds { get; set; }

    public short BasePlaceBetsPopular { get; set; }

    public short IdentificationMarkCount1 { get; set; }
    public short IdentificationMarkCount2 { get; set; }
    public short IdentificationMarkCount3 { get; set; }
    public short IdentificationMarkCount4 { get; set; }
    public short IdentificationMarkCount5 { get; set; }
    public short TotalMarkCount1 { get; set; }
    public short TotalMarkCount2 { get; set; }
    public short TotalMarkCount3 { get; set; }
    public short TotalMarkCount4 { get; set; }
    public short TotalMarkCount5 { get; set; }

    public short PopularPoint { get; set; }

    public short TrainingPoint { get; set; }

    public short StablePoint { get; set; }

    public TrainingArrow TrainingArrow { get; set; }

    public StableEvaluation StableEvaluation { get; set; }

    public short RiderTopRatioExpectPoint { get; set; }

    public short SpeedPoint { get; set; }

    public HorseHoof Hoof { get; set; }

    public TrackConditionHorseAptitude YieldingAptitude { get; set; }

    public ClassAptitude ClassAptitude { get; set; }

    public JdbcHorseMark TotalMark { get; set; }

    public JdbcHorseMark IdmMark { get; set; }

    public JdbcHorseMark InfoMark { get; set; }

    public JdbcHorseMark RiderMark { get; set; }

    public JdbcHorseMark StableMark { get; set; }

    public JdbcHorseMark TrainingMark { get; set; }

    public JdbcHorseMark SpeedMark { get; set; }

    public JdbcHorseMark TurfMark { get; set; }

    public JdbcHorseMark DirtMark { get; set; }

    public short RaceBefore3Point { get; set; }

    public short RaceBasePoint { get; set; }

    public short RaceAfter3Point { get; set; }

    public short RacePositionPoint { get; set; }

    public JdbcRacePace RacePace { get; set; }

    public JdbcRaceDevelopment RaceDevelopment { get; set; }

    public short MiddleOrder { get; set; }

    public short MiddleDiff { get; set; }

    public RacePosition MiddlePosition { get; set; }

    public short After3Order { get; set; }

    public short After3Diff { get; set; }

    public RacePosition After3Position { get; set; }

    public short GoalOrder { get; set; }

    public short GoalDiff { get; set; }

    public RacePosition GoalPosition { get; set; }

    public short BeforeWeight { get; set; }

    public short BeforeWeightDiff { get; set; }

    public short SpeedPointOrder { get; set; }

    public short LsPointOrder { get; set; }

    public short RaceBefore3PointOrder { get; set; }

    public short RaceBasePointOrder { get; set; }

    public short RaceAfter3PointOrder { get; set; }

    public short RacePositionPointOrder { get; set; }

    public short RiderExpectOdds { get; set; }

    public short RiderExpectPlaceBetsOdds { get; set; }

    public ShippingStatus Shipping { get; set; }

    public HorseNote Note1 { get; set; }
    public HorseNote Note2 { get; set; }
    public HorseNote Note3 { get; set; }

    public short RaceStartPoint { get; set; }

    public short RaceStartDelayPoint { get; set; }

    public short BigTicketPoint { get; set; }

    public JdbcHorseMark BigTicketMark { get; set; }

    public short LossClassSize { get; set; }

    public SpeedType SpeedType { get; set; }

    public RestReason RestReason { get; set; }

    public JdbcFlagGround GroundFlag { get; set; }

    public JdbcFlagDistance DistanceFlag { get; set; }

    public JdbcFlagClass ClassFlag { get; set; }

    public JdbcFlagChangeStable ChangeStableFlag { get; set; }

    public JdbcFlagCastration CastrationFlag { get; set; }

    public JdbcFlagChangeRider ChangeRiderFlag { get; set; }

    public string GrazingName { get; set; } = string.Empty;

    public GrazingRank GrazingRank { get; set; }

    public StableRank StableRank { get; set; }

    public async Task<bool> ReadStringAsync(MyContextBase db, string raw)
    {
      var bin = Encoding.GetEncoding(932).GetBytes(raw);

      string AsString(int startIndex, int length)
      {
        var binary = bin[startIndex..(startIndex + length)];
        return Encoding.GetEncoding(932).GetString(binary);
      }

      short.TryParse(AsString(0, 2), out var courseCode);
      short.TryParse(AsString(2, 2), out var year);
      year += 2000;
      short.TryParse(AsString(4, 1), out var kaiji);
      short.TryParse(AsString(5, 1), NumberStyles.HexNumber, null, out var nichiji);
      short.TryParse(AsString(6, 2), out var courseNumber);

      var course = RaceCourse.Unknown;
      if (courseCode <= 10)
      {
        course = (RaceCourse)courseCode;
      }
      else
      {
        course = courseCode switch
        {
          21 => RaceCourse.Asahikawa,
          22 => RaceCourse.SapporoLocal,
          23 => RaceCourse.Mombetsu,
          24 => RaceCourse.HakodateLocal,
          25 => RaceCourse.Morioka,
          26 => RaceCourse.Mizusawa,
          27 => RaceCourse.Kaminoyama,
          28 => RaceCourse.NiigataLocal,
          29 => RaceCourse.Sanjo,
          30 => RaceCourse.Ashikaka,
          31 => RaceCourse.Utsunomiya,
          32 => RaceCourse.Takasaki,
          33 => RaceCourse.Urawa,
          34 => RaceCourse.Funabashi,
          35 => RaceCourse.Oi,
          36 => RaceCourse.Kawazaki,
          37 => RaceCourse.Kanazawa,
          38 => RaceCourse.Kasamatsu,
          39 => RaceCourse.Nagoya,
          40 => RaceCourse.ChukyoLocal,
          41 => RaceCourse.Sonoda,
          42 => RaceCourse.Himeji,
          43 => RaceCourse.Masuda,
          44 => RaceCourse.Fukushima,
          45 => RaceCourse.Kochi,
          46 => RaceCourse.Saga,
          47 => RaceCourse.Arao,
          48 => RaceCourse.Nakatsu,
          61 => RaceCourse.Uk,
          62 => RaceCourse.Ireland,
          63 => RaceCourse.France,
          64 => RaceCourse.Italy,
          65 => RaceCourse.Germany,
          66 => RaceCourse.Usa,
          67 => RaceCourse.Canada,
          68 => RaceCourse.Arab,
          69 => RaceCourse.Australia,
          70 => RaceCourse.NewZealand,
          71 => RaceCourse.HongKong,
          72 => RaceCourse.Chile,
          73 => RaceCourse.Singapore,
          74 => RaceCourse.Sweden,
          75 => RaceCourse.Macau,
          76 => RaceCourse.Austria,
          77 => RaceCourse.Turkey,
          78 => RaceCourse.Qatar,
          79 => RaceCourse.Korea,
          _ => RaceCourse.Unknown,
        };
      }

      var yearString = year.ToString();
      var kaijiText = $"{kaiji:00}{nichiji:00}{courseNumber:00}";
      var raceData = await db.Races!.FirstOrDefaultAsync(r => r.Key.StartsWith(yearString) && r.Key.EndsWith(kaijiText) &&
        r.Nichiji == nichiji && r.CourseRaceNumber == courseNumber && r.Course == course);
      if (raceData == null)
      {
        return false;
      }

      this.RaceKey = raceData.Key;

      short.TryParse(AsString(8, 2), out var horseNumber);
      var horseData = await db.RaceHorses!.FirstOrDefaultAsync(rh => rh.RaceKey == this.RaceKey && rh.Number == horseNumber);
      if (horseData == null)
      {
        return false;
      }

      this.Key = horseData.Key;

      short.TryParse(AsString(54, 5).Trim().Replace(".", string.Empty), out var idm);
      short.TryParse(AsString(59, 5).Trim().Replace(".", string.Empty), out var riderPoint);
      short.TryParse(AsString(64, 5).Trim().Replace(".", string.Empty), out var infoPoint);
      short.TryParse(AsString(84, 5).Trim().Replace(".", string.Empty), out var totalPoint);
      this.IdmPoint = idm;
      this.RiderPoint = riderPoint;
      this.InfoPoint = infoPoint;
      this.TotalPoint = totalPoint;

      short.TryParse(AsString(89, 1), out var runningStyleCode);
      short.TryParse(AsString(90, 1), out var distanceAptitudeCode);
      short.TryParse(AsString(91, 1), out var climbCode);
      this.RunningStyle = (JdbcRunningStyle)runningStyleCode;
      this.DistanceAptitude = (HorseDistanceAptitude)distanceAptitudeCode;
      this.Climb = (HorseClimb)climbCode;

      short.TryParse(AsString(95, 5).Trim().Replace(".", string.Empty), out var baseOdds);
      short.TryParse(AsString(100, 2), out var basePopular);
      short.TryParse(AsString(102, 5).Trim().Replace(".", string.Empty), out var basePlaceOdds);
      short.TryParse(AsString(107, 2), out var basePlacePopular);
      this.BaseOdds = baseOdds;
      this.BasePopular = basePopular;
      this.BasePlaceBetsOdds = basePlaceOdds;
      this.BasePlaceBetsPopular = basePlacePopular;

      short.TryParse(AsString(109, 3).Trim(), out var mark11);
      short.TryParse(AsString(112, 3).Trim(), out var mark12);
      short.TryParse(AsString(115, 3).Trim(), out var mark13);
      short.TryParse(AsString(118, 3).Trim(), out var mark14);
      short.TryParse(AsString(121, 3).Trim(), out var mark15);
      short.TryParse(AsString(124, 3).Trim(), out var mark21);
      short.TryParse(AsString(127, 3).Trim(), out var mark22);
      short.TryParse(AsString(130, 3).Trim(), out var mark23);
      short.TryParse(AsString(133, 3).Trim(), out var mark24);
      short.TryParse(AsString(136, 3).Trim(), out var mark25);
      this.IdentificationMarkCount1 = mark11;
      this.IdentificationMarkCount2 = mark12;
      this.IdentificationMarkCount3 = mark13;
      this.IdentificationMarkCount4 = mark14;
      this.IdentificationMarkCount5 = mark15;
      this.TotalMarkCount1 = mark21;
      this.TotalMarkCount2 = mark22;
      this.TotalMarkCount3 = mark23;
      this.TotalMarkCount4 = mark24;
      this.TotalMarkCount5 = mark25;

      short.TryParse(AsString(139, 5).Trim().Replace(".", string.Empty), out var popularPoint);
      short.TryParse(AsString(144, 5).Trim().Replace(".", string.Empty), out var trainingPoint);
      short.TryParse(AsString(149, 5).Trim().Replace(".", string.Empty), out var housePoint);
      this.PopularPoint = popularPoint;
      this.TrainingPoint = trainingPoint;
      this.StablePoint = housePoint;

      if (bin.Length < 164)
      {
        this.Version = 1;
        return true;
      }

      short.TryParse(AsString(154, 1), out var trainingArrowCode);
      short.TryParse(AsString(155, 1), out var houseEvalutionCode);
      short.TryParse(AsString(156, 4).Trim().Replace(".", string.Empty), out var riderRentaiPoint);
      short.TryParse(AsString(160, 3).Trim(), out var runViolentryPoint);
      short.TryParse(AsString(163, 2), out var hoofCode);
      short.TryParse(AsString(165, 1), out var yieldingAptitudeCode);
      short.TryParse(AsString(166, 2), out var classAptitudeCode);
      this.TrainingArrow = (TrainingArrow)trainingArrowCode;
      this.StableEvaluation = (StableEvaluation)houseEvalutionCode;
      this.RiderTopRatioExpectPoint = riderRentaiPoint;
      this.SpeedPoint = runViolentryPoint;
      this.Hoof = (HorseHoof)hoofCode;
      this.YieldingAptitude = (TrackConditionHorseAptitude)yieldingAptitudeCode;
      this.ClassAptitude = (ClassAptitude)classAptitudeCode;

      if (bin.Length < 252)
      {
        this.Version = 3;
        return true;
      }

      if (bin.Length < 331)
      {
        this.Version = 4;
        return true;
      }

      short.TryParse(AsString(326, 1), out var totalMarkCode);
      short.TryParse(AsString(327, 1), out var idmMarkCode);
      short.TryParse(AsString(328, 1), out var infoMarkCode);
      short.TryParse(AsString(329, 1), out var riderMarkCode);
      short.TryParse(AsString(330, 1), out var houseMarkCode);
      short.TryParse(AsString(331, 1), out var trainingMarkCode);
      short.TryParse(AsString(332, 1), out var speedMarkCode);
      short.TryParse(AsString(333, 1), out var turfAptitudeCode);
      short.TryParse(AsString(334, 1), out var dirtAptitudeCode);
      this.TotalMark = (JdbcHorseMark)totalMarkCode;
      this.IdmMark = (JdbcHorseMark)idmMarkCode;
      this.InfoMark = (JdbcHorseMark)infoMarkCode;
      this.RiderMark = (JdbcHorseMark)riderMarkCode;
      this.StableMark = (JdbcHorseMark)houseMarkCode;
      this.TrainingMark = (JdbcHorseMark)trainingMarkCode;
      this.SpeedMark = (JdbcHorseMark)speedMarkCode;
      this.TurfMark = (JdbcHorseMark)turfAptitudeCode;
      this.DirtMark = (JdbcHorseMark)dirtAptitudeCode;

      if (bin.Length < 374)
      {
        this.Version = 5;
        return true;
      }

      short.TryParse(AsString(358, 5).Trim().Replace(".", string.Empty), out var before3hPoint);
      short.TryParse(AsString(363, 5).Trim().Replace(".", string.Empty), out var basePoint);
      short.TryParse(AsString(368, 5).Trim().Replace(".", string.Empty), out var after3hPoint);
      short.TryParse(AsString(373, 5).Trim().Replace(".", string.Empty), out var positionPoint);
      this.RaceBefore3Point = before3hPoint;
      this.RaceBasePoint = basePoint;
      this.RaceAfter3Point = after3hPoint;
      this.RacePositionPoint = positionPoint;

      var baseCode = AsString(378, 1);
      this.RacePace = baseCode switch
      {
        "H" => JdbcRacePace.Hard,
        "M" => JdbcRacePace.Middle,
        "S" => JdbcRacePace.Slow,
        _ => JdbcRacePace.Unknown,
      };

      short.TryParse(AsString(379, 2).Trim(), out var middleOrder);
      short.TryParse(AsString(381, 2).Trim(), out var middleDiff);
      short.TryParse(AsString(383, 1), out var middlePositionCode); // 内外
      short.TryParse(AsString(384, 2).Trim(), out var after3hOrder);
      short.TryParse(AsString(386, 2).Trim(), out var after3hDiff);
      short.TryParse(AsString(388, 1), out var after3hPositionCode); // 内外
      short.TryParse(AsString(389, 2).Trim(), out var goalOrder);
      short.TryParse(AsString(391, 2).Trim(), out var goalDiff);
      short.TryParse(AsString(393, 1), out var goalPositionCode); // 内外
      this.MiddleOrder = middleOrder;
      this.MiddleDiff = middleDiff;
      this.MiddlePosition = (RacePosition)middlePositionCode;
      this.After3Order = after3hOrder;
      this.After3Diff = after3hDiff;
      this.After3Position = (RacePosition)after3hPositionCode;
      this.GoalOrder = goalOrder;
      this.GoalDiff = goalDiff;
      this.GoalPosition = (RacePosition)goalPositionCode;

      var developmentCode = AsString(394, 1);
      this.RaceDevelopment = developmentCode switch
      {
        "<" => JdbcRaceDevelopment.SaveRunner,
        "@" => JdbcRaceDevelopment.After3Fastest,
        "*" => JdbcRaceDevelopment.After3Faster,
        "?" => JdbcRaceDevelopment.NotClear,
        "(" => JdbcRaceDevelopment.Others,
        _ => JdbcRaceDevelopment.Unknown,
      };

      if (bin.Length < 400)
      {
        this.Version = 6;
        return true;
      }

      short.TryParse(AsString(396, 3).Trim(), out var beforeWeight);
      short.TryParse(AsString(399, 3).Trim().Replace(" ", string.Empty), out var beforeWeightDiff);
      this.BeforeWeight = beforeWeight;
      this.BeforeWeightDiff = beforeWeightDiff;

      short.TryParse(AsString(448, 2).Trim(), out var speedPointOrder);
      short.TryParse(AsString(450, 2).Trim(), out var lsPointOrder);
      short.TryParse(AsString(452, 2).Trim(), out var before3hPointOrder);
      short.TryParse(AsString(454, 2).Trim(), out var basePointOrder);
      short.TryParse(AsString(456, 2).Trim(), out var after3hPointOrder);
      short.TryParse(AsString(458, 2).Trim(), out var positionPointOrder);
      this.SpeedPointOrder = speedPointOrder;
      this.LsPointOrder = lsPointOrder;
      this.RaceBefore3PointOrder = before3hPointOrder;
      this.RaceBasePointOrder = basePointOrder;
      this.RaceAfter3PointOrder = after3hPointOrder;
      this.RacePositionPointOrder = positionPointOrder;

      if (bin.Length < 465)
      {
        this.Version = 7;
        return true;
      }

      short.TryParse(AsString(460, 4).Trim().Replace(".", string.Empty), out var riderExpectOdds);
      short.TryParse(AsString(464, 4).Trim().Replace(".", string.Empty), out var riderExpectPlaceBetsRate);
      short.TryParse(AsString(468, 1), out var shippingCode);
      this.RiderExpectOdds = riderExpectOdds;
      this.RiderExpectPlaceBetsOdds = riderExpectPlaceBetsRate;
      this.Shipping = (ShippingStatus)shippingCode;

      if (bin.Length < 517)
      {
        this.Version = 8;
        return true;
      }

      // 走法データは現在採取していないとのこと
      // 体型はめんどいので省略（パドック見ればいいのでは？）

      short.TryParse(AsString(510, 3), out var horseNote1);
      short.TryParse(AsString(513, 3), out var horseNote2);
      short.TryParse(AsString(516, 3), out var horseNote3);
      this.Note1 = (HorseNote)horseNote1;
      this.Note2 = (HorseNote)horseNote2;
      this.Note3 = (HorseNote)horseNote3;

      short.TryParse(AsString(519, 4).Trim().Replace(".", string.Empty), out var horseStartPoint);
      short.TryParse(AsString(523, 4).Trim().Replace(".", string.Empty), out var horseStartDelayPoint);
      this.RaceStartPoint = horseStartPoint;
      this.RaceStartDelayPoint = horseStartDelayPoint;

      short.TryParse(AsString(534, 3).Trim(), out var bigTicketPoint);
      short.TryParse(AsString(537, 1), out var bigTicketMark);
      this.BigTicketPoint = bigTicketPoint;
      this.BigTicketMark = (JdbcHorseMark)bigTicketMark;

      if (bin.Length < 542)
      {
        this.Version = 9;
        return true;
      }

      short.TryParse(AsString(538, 1), out var lossClassSize);
      var speedTypeCode = AsString(539, 2);
      short.TryParse(AsString(541, 2), out var restReasonCode);
      this.LossClassSize = lossClassSize;
      this.RestReason = (RestReason)restReasonCode;
      this.SpeedType = speedTypeCode switch
      {
        "A1" => SpeedType.A1,
        "A2" => SpeedType.A2,
        "A3" => SpeedType.A3,
        "A4" => SpeedType.A4,
        "B1" => SpeedType.B1,
        "B2" => SpeedType.B2,
        _ => SpeedType.Unknown,
      };

      if (bin.Length < 570)
      {
        this.Version = 10;
        return true;
      }

      // フラグ
      short.TryParse(AsString(543, 1), out var trackTypeFlag);
      short.TryParse(AsString(544, 1), out var distanceFlag);
      short.TryParse(AsString(545, 1), out var classFlag);
      short.TryParse(AsString(546, 1), out var changeHouseFlag);
      short.TryParse(AsString(547, 1), out var castrationFlag);
      short.TryParse(AsString(548, 1), out var changeRiderFlag);
      this.GroundFlag = (JdbcFlagGround)trackTypeFlag;
      this.DistanceFlag = (JdbcFlagDistance)distanceFlag;
      this.ClassFlag = (JdbcFlagClass)classFlag;
      this.ChangeStableFlag = (JdbcFlagChangeStable)changeHouseFlag;
      this.CastrationFlag = (JdbcFlagCastration)castrationFlag;
      this.ChangeRiderFlag = (JdbcFlagChangeRider)changeRiderFlag;
  
      short.TryParse(AsString(559, 2).Trim(), out var currentHouseRaceNumber);
      DateTime.TryParseExact(AsString(561, 8), "yyyyMMdd", null, DateTimeStyles.None, out var houseInDate);
      short.TryParse(AsString(569, 3).Trim(), out var houseDays);

      var grazingName = AsString(572, 50).Trim();
      var grazingRankCode = AsString(622, 1);
      short.TryParse(AsString(623, 1), out var houseRankCode);
      this.GrazingName = grazingName;
      this.GrazingRank = (GrazingRank)(grazingRankCode switch
      {
        "A" => 1,
        "B" => 2,
        "C" => 3,
        "D" => 4,
        "E" => 5,
        _ => 0,
      });
      this.StableRank = (StableRank)houseRankCode;

      this.Version = 11;

      return true;

      // http://www.jrdb.com/program/Kyi/kyi_doc.txt 
    }
  }

  public enum HorseDistanceAptitude : short
  {
    Unknown = 0,
    Sprinter = 1,
    Middle = 2,
    Stayer = 3,
    Mylar = 5,
    All = 6,
  }

  public enum HorseClimb : short
  {
    Unknown = 0,
    AA = 1,
    A = 2,
    B = 3,
    C = 4,
    Difficult = 5,
  }

  public enum JdbcRunningStyle : short
  {
    Unknown = 0,
    FrontRunner = 1,
    Stalker = 2,
    Sotp = 3,
    SaveRunner = 4,
    GoodSotp = 5,  // 好位差し
    Freedom = 6,  // 自在
  }

  public enum TrainingArrow : short
  {
    Unknown = 0,

    /// <summary>
    /// デキ抜群
    /// </summary>
    Best = 1,

    /// <summary>
    /// 上昇
    /// </summary>
    Good = 2,

    /// <summary>
    /// 平行線
    /// </summary>
    Normal = 3,

    /// <summary>
    /// やや下降気味
    /// </summary>
    Worse = 4,

    /// <summary>
    /// デキ落ち
    /// </summary>
    Worst = 5,
  }

  public enum StableEvaluation : short
  {
    Unknown = 0,

    /// <summary>
    /// 超強気
    /// </summary>
    Best = 1,

    /// <summary>
    /// 強気
    /// </summary>
    Good = 2,

    /// <summary>
    /// 現状維持
    /// </summary>
    Normal = 3,

    /// <summary>
    /// 弱気
    /// </summary>
    Worse = 4,
  }

  public enum HorseHoof : short
  {
    Unknown = 0,
    /*
     * 01 大ベタ
02 中ベタ
03 小ベタ
04 細ベタ
05 大立
06 中立
07 小立
08 細立
09 大標準
10 中標準
11 小標準
12 細標準
17 大標起
18 中標起
19 小標起
20 細標起
21 大標ベ
22 中標ベ
23 小標ベ
24 細標ベ
     */
  }

  public enum TrackConditionHorseAptitude : short
  {
    Unknown = 0,

    /// <summary>
    /// 得意
    /// </summary>
    Good = 1,

    /// <summary>
    /// 普通
    /// </summary>
    Normal = 2,

    /// <summary>
    /// 苦手
    /// </summary>
    Bad = 3,
  }

  public enum ClassAptitude : short
  {
    Unknown = 0,

    // 今後のアプデで３つの変数に分けるべきか
    /*
     * 馬の能力をクラスで分けたもの。
芝ＯＰＡ　・・・　芝のオープン戦で勝つ能力のある馬。
芝ＯＰＢ　・・・　芝のオープン戦で好戦できる能力のある馬。
芝ＯＰＣ　・・・　芝のオープン戦で頭打ちの馬。
コードは以下のとおりです。
01 芝Ｇ１
02 芝Ｇ２
03 芝Ｇ３
04 芝ＯＰ A
05 芝ＯＰ B
06 芝ＯＰ C
07 芝３勝A
08 芝３勝B
09 芝３勝C
10 芝２勝A
11 芝２勝B
12 芝２勝C
13 芝１勝A
14 芝１勝B
15 芝１勝C
16 芝未 A
17 芝未 B
18 芝未 C
21 ダＧ１
22 ダＧ２
23 ダＧ３
24 ダＯＰ Ａ
25 ダＯＰ Ｂ
26 ダＯＰ Ｃ
27 ダ３勝Ａ
28 ダ３勝Ｂ
29 ダ３勝Ｃ
30 ダ２勝Ａ
31 ダ２勝Ｂ
32 ダ２勝Ｃ
33 ダ１勝Ａ
34 ダ１勝Ｂ
35 ダ１勝Ｃ
36 ダ未 Ａ
37 ダ未 Ｂ
38 ダ未 Ｃ
51 障Ｇ１
52 障Ｇ２
53 障Ｇ３
54 障ＯＰ Ａ
55 障ＯＰ Ｂ
56 障ＯＰ Ｃ
57 障１勝Ａ
58 障１勝Ｂ
59 障１勝Ｃ
60 障未 Ａ
61 障未 Ｂ
62 障未 Ｃ
     */
  }

  public enum JdbcHorseMark : short
  {
    Default = 0,
    DoubleCircle = 1,
    Circle = 2,
    FilledTriangle = 3,
    Attention = 4,
    Triangle1 = 5,
    Triangle2 = 6,
    Star = 9,
  }

  public enum JdbcRacePace : short
  {
    Unknown = 0,
    Hard = 1,
    Middle = 2,
    Slow = 3,
  }

  public enum JdbcRaceDevelopment : short
  {
    Unknown = 0,
    SaveRunner = 1,
    After3Fastest = 2,
    After3Faster = 3,
    NotClear = 4,
    Others = 5,
  }

  public enum RacePosition : short
  {
    Unknown = 0,
    MostInside = 1,
    Inside = 2,
    Middle = 3,
    Outside = 4,
    MostOutside = 5,
  }

  public enum ShippingStatus : short
  {
    Unknown = 0,
    Local = 1,
    Normal = 2,
    Expedition = 3,
    Staying = 4,
  }

  public enum HorseNote : short
  {
    Unknown = 0,
    // http://www.jrdb.com/program/tokki_code.txt
    // 多すぎ
  }

  public enum SpeedType : short
  {
    Unknown = 0,
    A1 = 1,
    A2 = 2,
    A3 = 3,
    A4 = 4,
    B1 = 1,
    B2 = 2,
  }

  public enum RestReason : short
  {
    Unknown = 0,
  }

  public enum JdbcFlagGround : short
  {
  }

  public enum JdbcFlagDistance : short
  {
  }

  public enum JdbcFlagClass : short
  {
  }

  public enum JdbcFlagChangeStable : short
  {
  }

  public enum JdbcFlagCastration : short
  {
  }

  public enum JdbcFlagChangeRider : short
  {
  }

  public enum GrazingRank : short
  {
  }

  public enum StableRank : short
  {
  }
}
