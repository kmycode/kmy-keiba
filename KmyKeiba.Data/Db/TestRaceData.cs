using KmyKeiba.JVLink.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  public class TestRaceData : DataBase<TestRace>
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
    /// 競馬場内のコース番号
    /// </summary>
    public short CourseRaceNumber { get; set; }

    /// <summary>
    /// 馬の数
    /// </summary>
    public short HorsesCount { get; set; }

    /// <summary>
    /// 出走日時
    /// </summary>
    public DateTime StartTime { get; set; }

    public override void SetEntity(TestRace race)
    {
      this.Key = race.Key;
      this.Course = race.Course;
      this.TrackCornerDirection = race.TrackCornerDirection;
      this.TrackOption = race.TrackOption;
      this.TrackType = race.TrackType;
      this.TrackGround = race.TrackGround;
      this.TrackCondition = race.TrackCondition;
      this.Distance = race.Distance;
      this.CourseRaceNumber = race.CourseRaceNumber;
      this.HorsesCount = race.HorsesCount;
      this.StartTime = race.StartTime;
    }

    public override bool IsEquals(DataBase<TestRace> b)
    {
      var c = (TestRaceData)b;
      return this.Key == c.Key;
    }

    public override int GetHashCode()
    {
      return this.Key.GetHashCode();
    }
  }
}
