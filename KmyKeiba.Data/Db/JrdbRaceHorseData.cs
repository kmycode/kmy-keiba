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

      var raceData = await db.Races!.FirstOrDefaultAsync(r => r.StartTime.Year == year && r.Kaiji == kaiji &&
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

      short.TryParse(AsString(109, 3).Trim().Replace(".", string.Empty), out var mark11);
      short.TryParse(AsString(112, 3).Trim().Replace(".", string.Empty), out var mark12);
      short.TryParse(AsString(115, 3).Trim().Replace(".", string.Empty), out var mark13);
      short.TryParse(AsString(118, 3).Trim().Replace(".", string.Empty), out var mark14);
      short.TryParse(AsString(121, 3).Trim().Replace(".", string.Empty), out var mark15);
      short.TryParse(AsString(124, 3).Trim().Replace(".", string.Empty), out var mark21);
      short.TryParse(AsString(127, 3).Trim().Replace(".", string.Empty), out var mark22);
      short.TryParse(AsString(130, 3).Trim().Replace(".", string.Empty), out var mark23);
      short.TryParse(AsString(133, 3).Trim().Replace(".", string.Empty), out var mark24);
      short.TryParse(AsString(136, 3).Trim().Replace(".", string.Empty), out var mark25);

      short.TryParse(AsString(139, 5).Trim().Replace(".", string.Empty), out var popularPoint);
      short.TryParse(AsString(144, 5).Trim().Replace(".", string.Empty), out var trainingPoint);
      short.TryParse(AsString(149, 5).Trim().Replace(".", string.Empty), out var housePoint);

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
      short.TryParse(AsString(165, 1), out var gradeAptitudeCode);
      short.TryParse(AsString(166, 2), out var classAptitudeCode);

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

      if (bin.Length < 374)
      {
        this.Version = 5;
        return true;
      }

      short.TryParse(AsString(358, 5).Trim().Replace(".", string.Empty), out var before3hPoint);
      short.TryParse(AsString(363, 5).Trim().Replace(".", string.Empty), out var basePoint);
      short.TryParse(AsString(368, 5).Trim().Replace(".", string.Empty), out var after3hPoint);
      short.TryParse(AsString(373, 5).Trim().Replace(".", string.Empty), out var positionPoint);

      var baseCode = AsString(378, 1);

      short.TryParse(AsString(379, 2).Trim(), out var middleOrder);
      short.TryParse(AsString(381, 2).Trim(), out var middleDiff);
      short.TryParse(AsString(383, 1), out var middlePositionCode); // 内外
      short.TryParse(AsString(384, 2).Trim(), out var after3hOrder);
      short.TryParse(AsString(386, 2).Trim(), out var after3hDiff);
      short.TryParse(AsString(388, 1), out var after3hPositionCode); // 内外
      short.TryParse(AsString(389, 2).Trim(), out var goalOrder);
      short.TryParse(AsString(391, 2).Trim(), out var goalDiff);
      short.TryParse(AsString(393, 1), out var goalPositionCode); // 内外

      var developmentCode = AsString(394, 1);

      if (bin.Length < 400)
      {
        this.Version = 6;
        return true;
      }

      short.TryParse(AsString(396, 3).Trim(), out var beforeWeight);
      short.TryParse(AsString(399, 3).Trim().Replace(" ", string.Empty), out var beforeWeightDiff);

      short.TryParse(AsString(448, 2).Trim(), out var speedPointOrder);
      short.TryParse(AsString(450, 2).Trim(), out var lsPointOrder);
      short.TryParse(AsString(452, 2).Trim(), out var before3hPointOrder);
      short.TryParse(AsString(454, 2).Trim(), out var basePointOrder);
      short.TryParse(AsString(456, 2).Trim(), out var after3hPointOrder);
      short.TryParse(AsString(458, 2).Trim(), out var positionPointOrder);

      if (bin.Length < 465)
      {
        this.Version = 7;
        return true;
      }

      short.TryParse(AsString(460, 4).Trim().Replace(".", string.Empty), out var riderExpectOdds);
      short.TryParse(AsString(464, 4).Trim().Replace(".", string.Empty), out var riderExpectPlaceBetsRate);
      short.TryParse(AsString(468, 1), out var shippingCode);

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

      short.TryParse(AsString(519, 4).Trim().Replace(".", string.Empty), out var horseStartPoint);
      short.TryParse(AsString(523, 4).Trim().Replace(".", string.Empty), out var horseStartDelayPoint);

      short.TryParse(AsString(534, 3).Trim(), out var bigTicketPoint);
      short.TryParse(AsString(537, 1), out var bigTicketMark);

      if (bin.Length < 542)
      {
        this.Version = 9;
        return true;
      }

      short.TryParse(AsString(538, 1), out var lossClassSize);
      var speedTypeCode = AsString(539, 2);
      short.TryParse(AsString(541, 2), out var restReasonCode);

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

      short.TryParse(AsString(559, 2).Trim(), out var currentHouseRaceNumber);
      DateTime.TryParseExact(AsString(561, 8), "yyyyMMdd", null, DateTimeStyles.None, out var houseInDate);
      short.TryParse(AsString(569, 3).Trim(), out var houseDays);

      var grazingName = AsString(572, 50);
      var grazingRankCode = AsString(622, 1);
      var houseRankCode = AsString(623, 1);

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
}
