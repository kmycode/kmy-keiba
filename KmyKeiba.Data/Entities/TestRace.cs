using KmyKeiba.Data.Wrappers;
using KmyKeiba.JVLink.Wrappers;
using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  public class TestRace : EntityBase
  {
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 競馬場
    /// </summary>
    public RaceCourse Course { get; set; }

    /// <summary>
    /// 競馬場の地面
    /// </summary>
    public TrackGround TrackGround { get; set; }

    /// <summary>
    /// 競馬場のコーナーの向き
    /// </summary>
    public TrackCornerDirection TrackCornerDirection { get; set; }

    /// <summary>
    /// 競馬の種類
    /// </summary>
    public TrackType TrackType { get; set; }

    /// <summary>
    /// 競馬場のその他の条件
    /// </summary>
    public TrackOption TrackOption { get; set; }

    /// <summary>
    /// 天気
    /// </summary>
    public RaceCourseWeather TrackWeather { get; set; }

    /// <summary>
    /// 馬場の状態
    /// </summary>
    public RaceCourseCondition TrackCondition { get; set; }

    /// <summary>
    /// 距離
    /// </summary>
    public short Distance { get; set; }

    /// <summary>
    /// 競馬場の名前
    /// </summary>
    public string CourseName => this._courseName ??= this.Course.GetAttribute()?.Name ?? string.Empty;
    private string? _courseName = null;

    /// <summary>
    /// 競馬場内のコース番号
    /// </summary>
    public short CourseRaceNumber { get; set; }

    /// <summary>
    /// コースの種類
    /// </summary>
    public string CourseType { get; set; } = string.Empty;

    /// <summary>
    /// 馬の数
    /// </summary>
    public short HorsesCount { get; set; }

    /// <summary>
    /// 出走日時
    /// </summary>
    public DateTime StartTime { get; set; }

    internal TestRace()
    {
    }

    public static TestRace FromJV(JVData_Struct.JV_NR_NOSI_RACE race)
    {
      var startTime = DateTime.ParseExact($"{race.id.Year}{race.id.MonthDay}{race.HassoTime}", "yyyyMMddHHmm", null);

      var course = RaceCourse.Unknown;
      if (int.TryParse(race.id.JyoCD, out int courseNum))
      {
        course = (RaceCourse)courseNum;
      }
      else
      {
        course = Enum
          .GetValues<RaceCourse>()
          .FirstOrDefault((rc) => rc.GetAttribute()?.Key == race.id.JyoCD);
      }

      short.TryParse(race.id.RaceNum, out short courseRaceNum);
      short.TryParse(race.TorokuTosu, out short horsesCount);
      short.TryParse(race.Kyori, out short distance);

      int.TryParse(race.TrackCD, out int track);
      var (trackType, trackGround, trackCornerDirection, trackOption) = Race.GetTrackType(track);

      int.TryParse(race.TenkoBaba.TenkoCD, out int weather);

      // 地方競馬DATAは、盛岡の芝もダートとして配信し、SibaBabaCDには何も設定しないみたい
      int.TryParse((trackGround == TrackGround.Turf && course <= RaceCourse.CentralMaxValue) ? race.TenkoBaba.SibaBabaCD : race.TenkoBaba.DirtBabaCD, out int condition);

      var obj = new TestRace
      {
        LastModified = race.head.MakeDate.ToDateTime(),
        DataStatus = race.head.DataKubun.ToDataStatus(),
        Key = race.id.ToTestRaceKey(),
        Course = course,
        CourseType = race.CourseKubunCD.Trim(),
        TrackGround = trackGround,
        TrackType = trackType,
        TrackCornerDirection = trackCornerDirection,
        TrackOption = trackOption,
        TrackWeather = (RaceCourseWeather)weather,
        TrackCondition = (RaceCourseCondition)condition,
        Distance = distance,
        CourseRaceNumber = courseRaceNum,
        HorsesCount = horsesCount,
        StartTime = startTime,
      };
      return obj;
    }

    public override int GetHashCode() => this.Key.GetHashCode();
  }
}
