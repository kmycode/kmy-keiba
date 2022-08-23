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
  public class JrdbRaceHorseData : AppDataBase
  {
    public string RaceKey { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public JdbcRunningStyle RunningStyle { get; set; }

    public HorseDistanceAptitude DistanceAptitude { get; set; }

    public HorseClimb Climb { get; set; }

    public async Task<bool> ReadStringAsync(MyContextBase db, string raw)
    {
      // 【警告】
      // よく考えたらSubStringは全角文字も１つとしてカウントしてしまうので
      // 以下のコードはJDBCには使えない。修正の必要あり

      short.TryParse(raw[0..2], out var courseCode);
      short.TryParse(raw.AsSpan(2, 2), out var year);
      year += 2000;
      short.TryParse(raw.AsSpan(4, 1), out var kaiji);
      short.TryParse(raw.AsSpan(5, 1), NumberStyles.HexNumber, null, out var nichiji);
      short.TryParse(raw.AsSpan(6, 2), out var courseNumber);

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

      var raceData = await db.Races!.FirstOrDefaultAsync(r => r.StartTime.Year == year && r.Kaiji == kaiji &&
        r.Nichiji == nichiji && r.CourseRaceNumber == courseNumber && r.Course == course);
      if (raceData == null)
      {
        return false;
      }

      this.RaceKey = raceData.Key;

      short.TryParse(raw.AsSpan(8, 2), out var horseNumber);
      var horseData = await db.RaceHorses!.FirstOrDefaultAsync(rh => rh.RaceKey == this.RaceKey && rh.Number == horseNumber);
      if (horseData == null)
      {
        return false;
      }

      this.Key = horseData.Key;

      short.TryParse(raw.Substring(54, 5).Trim().Replace(".", string.Empty), out var idm);
      short.TryParse(raw.Substring(59, 5).Trim().Replace(".", string.Empty), out var riderPoint);
      short.TryParse(raw.Substring(64, 5).Trim().Replace(".", string.Empty), out var infoPoint);
      short.TryParse(raw.Substring(84, 5).Trim().Replace(".", string.Empty), out var totalPoint);

      short.TryParse(raw.AsSpan(89, 1), out var runningStyleCode);
      short.TryParse(raw.AsSpan(90, 1), out var distanceAptitudeCode);
      short.TryParse(raw.AsSpan(91, 1), out var climbCode);
      this.RunningStyle = (JdbcRunningStyle)runningStyleCode;
      this.DistanceAptitude = (HorseDistanceAptitude)distanceAptitudeCode;
      this.Climb = (HorseClimb)climbCode;

      short.TryParse(raw.Substring(95, 5).Trim().Replace(".", string.Empty), out var baseOdds);
      short.TryParse(raw.AsSpan(100, 2), out var basePopular);
      short.TryParse(raw.Substring(102, 5).Trim().Replace(".", string.Empty), out var basePlaceOdds);
      short.TryParse(raw.AsSpan(107, 2), out var basePlacePopular);

      short.TryParse(raw.Substring(109, 3).Trim().Replace(".", string.Empty), out var mark11);
      short.TryParse(raw.Substring(112, 3).Trim().Replace(".", string.Empty), out var mark12);
      short.TryParse(raw.Substring(115, 3).Trim().Replace(".", string.Empty), out var mark13);
      short.TryParse(raw.Substring(118, 3).Trim().Replace(".", string.Empty), out var mark14);
      short.TryParse(raw.Substring(121, 3).Trim().Replace(".", string.Empty), out var mark15);
      short.TryParse(raw.Substring(124, 3).Trim().Replace(".", string.Empty), out var mark21);
      short.TryParse(raw.Substring(127, 3).Trim().Replace(".", string.Empty), out var mark22);
      short.TryParse(raw.Substring(130, 3).Trim().Replace(".", string.Empty), out var mark23);
      short.TryParse(raw.Substring(133, 3).Trim().Replace(".", string.Empty), out var mark24);
      short.TryParse(raw.Substring(136, 3).Trim().Replace(".", string.Empty), out var mark25);

      short.TryParse(raw.Substring(139, 5).Trim().Replace(".", string.Empty), out var popularPoint);
      short.TryParse(raw.Substring(144, 5).Trim().Replace(".", string.Empty), out var trainingPoint);
      short.TryParse(raw.Substring(149, 5).Trim().Replace(".", string.Empty), out var housePoint);

      if (raw.Length < 164)
      {
        this.Version = 1;
        return true;
      }

      short.TryParse(raw.AsSpan(154, 1), out var trainingArrowCode);
      short.TryParse(raw.AsSpan(155, 1), out var houseEvalutionCode);
      short.TryParse(raw.Substring(156, 4).Trim().Replace(".", string.Empty), out var riderRentaiPoint);
      short.TryParse(raw.AsSpan(160, 3).Trim(), out var runViolentryPoint);
      short.TryParse(raw.AsSpan(163, 2), out var hoofCode);
      short.TryParse(raw.AsSpan(165, 1), out var gradeAptitudeCode);
      short.TryParse(raw.AsSpan(166, 2), out var classAptitudeCode);

      if (raw.Length < 252)
      {
        this.Version = 3;
        return true;
      }

      if (raw.Length < 331)
      {
        this.Version = 4;
        return true;
      }

      short.TryParse(raw.AsSpan(326, 1), out var totalMarkCode);
      short.TryParse(raw.AsSpan(327, 1), out var idmMarkCode);
      short.TryParse(raw.AsSpan(328, 1), out var infoMarkCode);
      short.TryParse(raw.AsSpan(329, 1), out var riderMarkCode);
      short.TryParse(raw.AsSpan(330, 1), out var houseMarkCode);
      short.TryParse(raw.AsSpan(331, 1), out var trainingMarkCode);
      short.TryParse(raw.AsSpan(332, 1), out var speedMarkCode);
      short.TryParse(raw.AsSpan(333, 1), out var turfAptitudeCode);
      short.TryParse(raw.AsSpan(334, 1), out var dirtAptitudeCode);

      if (raw.Length < 374)
      {
        this.Version = 5;
        return true;
      }

      short.TryParse(raw.Substring(358, 5).Trim().Replace(".", string.Empty), out var before3hPoint);
      short.TryParse(raw.Substring(363, 5).Trim().Replace(".", string.Empty), out var basePoint);
      short.TryParse(raw.Substring(368, 5).Trim().Replace(".", string.Empty), out var after3hPoint);
      short.TryParse(raw.Substring(373, 5).Trim().Replace(".", string.Empty), out var positionPoint);

      var baseCode = raw.Substring(378, 1);

      short.TryParse(raw.Substring(379, 2).Trim(), out var middleOrder);
      short.TryParse(raw.Substring(381, 2).Trim(), out var middleDiff);
      short.TryParse(raw.Substring(383, 1), out var middlePositionCode); // 内外
      short.TryParse(raw.Substring(384, 2).Trim(), out var after3hOrder);
      short.TryParse(raw.Substring(386, 2).Trim(), out var after3hDiff);
      short.TryParse(raw.Substring(388, 1), out var after3hPositionCode); // 内外
      short.TryParse(raw.Substring(389, 2).Trim(), out var goalOrder);
      short.TryParse(raw.Substring(391, 2).Trim(), out var goalDiff);
      short.TryParse(raw.Substring(393, 1), out var goalPositionCode); // 内外

      var developmentCode = raw.Substring(394, 1);

      if (raw.Length < 400)
      {
        this.Version = 6;
        return true;
      }

      short.TryParse(raw.Substring(396, 3).Trim(), out var beforeWeight);
      short.TryParse(raw.Substring(399, 3).Trim().Replace(" ", string.Empty), out var beforeWeightDiff);

      short.TryParse(raw.Substring(448, 2).Trim(), out var speedPointOrder);
      short.TryParse(raw.Substring(450, 2).Trim(), out var lsPointOrder);
      short.TryParse(raw.Substring(452, 2).Trim(), out var before3hPointOrder);
      short.TryParse(raw.Substring(454, 2).Trim(), out var basePointOrder);
      short.TryParse(raw.Substring(456, 2).Trim(), out var after3hPointOrder);
      short.TryParse(raw.Substring(458, 2).Trim(), out var positionPointOrder);

      if (raw.Length < 465)
      {
        this.Version = 7;
        return true;
      }

      short.TryParse(raw.Substring(460, 4).Trim().Replace(".", string.Empty), out var riderExpectOdds);
      short.TryParse(raw.Substring(464, 4).Trim().Replace(".", string.Empty), out var riderExpectPlaceBetsRate);
      short.TryParse(raw.Substring(468, 1), out var shippingCode);

      if (raw.Length < 517)
      {
        this.Version = 8;
        return true;
      }

      // 走法データは現在採取していないとのこと
      // 体型はめんどいので省略（パドック見ればいいのでは？）

      short.TryParse(raw.Substring(510, 3), out var horseNote1);
      short.TryParse(raw.Substring(513, 3), out var horseNote2);
      short.TryParse(raw.Substring(516, 3), out var horseNote3);

      short.TryParse(raw.Substring(519, 4).Trim().Replace(".", string.Empty), out var horseStartPoint);
      short.TryParse(raw.Substring(523, 4).Trim().Replace(".", string.Empty), out var horseStartDelayPoint);

      short.TryParse(raw.Substring(534, 3).Trim(), out var bigTicketPoint);
      short.TryParse(raw.Substring(537, 1), out var bigTicketMark);

      if (raw.Length < 542)
      {
        this.Version = 9;
        return true;
      }

      short.TryParse(raw.Substring(538, 1), out var lossClassSize);
      var speedTypeCode = raw.Substring(539, 2);
      short.TryParse(raw.Substring(541, 2), out var restReasonCode);

      if (raw.Length < 570)
      {
        this.Version = 10;
        return true;
      }

      // フラグ
      short.TryParse(raw.Substring(543, 1), out var trackTypeFlag);
      short.TryParse(raw.Substring(544, 1), out var distanceFlag);
      short.TryParse(raw.Substring(545, 1), out var classFlag);
      short.TryParse(raw.Substring(546, 1), out var changeHouseFlag);
      short.TryParse(raw.Substring(547, 1), out var castrationFlag);
      short.TryParse(raw.Substring(548, 1), out var changeRiderFlag);

      short.TryParse(raw.Substring(559, 2).Trim(), out var currentHouseRaceNumber);
      DateTime.TryParseExact(raw.Substring(561, 8), "yyyyMMdd", null, DateTimeStyles.None, out var houseInDate);
      short.TryParse(raw.Substring(569, 3).Trim(), out var houseDays);

      var grazingName = raw.Substring(572, 50);
      var grazingRankCode = raw.Substring(622, 1);
      var houseRankCode = raw.Substring(623, 1);

      throw new NotImplementedException();

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
}
