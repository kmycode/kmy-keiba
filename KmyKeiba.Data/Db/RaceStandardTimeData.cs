using KmyKeiba.JVLink.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  [Index(nameof(Course))]
  public class RaceStandardTimeMasterData : MasterDataBase
  {
    public RaceCourse Course { get; set; }

    public DateTime SampleStartTime { get; set; }

    public DateTime SampleEndTime { get; set; }

    public int SampleCount { get; set; }

    public TrackCornerDirection CornerDirection { get; set; }

    public TrackOption TrackOption { get; set; }

    public TrackGround Ground { get; set; }

    public RaceCourseWeather Weather { get; set; }

    public RaceCourseCondition Condition { get; set; }

    public TrackType TrackType { get; set; }

    public short Distance { get; set; }

    public short DistanceMax { get; set; }

    public double Average { get; set; }

    public double Median { get; set; }

    public double Deviation { get; set; }
  }

  /// <summary>
  /// コース内の位置
  /// </summary>
  public enum CoursePosition : short
  {
    Unknown = 0,

    /// <summary>
    /// 指定なし
    /// </summary>
    NotClear = 1,

    /// <summary>
    /// 最後の直線
    /// </summary>
    LastLine = 2,

    /// <summary>
    /// 最後のコーナー
    /// </summary>
    Corner4 = 3,

    /// <summary>
    /// 最後から２番目のコーナー
    /// </summary>
    Corner3 = 4,

    /// <summary>
    /// 最後から３番目のコーナー
    /// </summary>
    Corner2 = 5,

    /// <summary>
    /// 最後から４番目のコーナー
    /// </summary>
    Corner1 = 6,

    /// <summary>
    /// 最初から
    /// </summary>
    First = 7,
  }
}
