using KmyKeiba.JVLink.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  [Index(nameof(RaceKey), nameof(Key))]
  public class RaceHorseAnalysisData : AnalysisDataBase
  {
    [StringLength(16)]
    public string Key { get; set; } = string.Empty;

    [StringLength(20)]
    public string RaceKey { get; set; } = string.Empty;

    /// <summary>
    /// 独自に算出した脚質
    /// </summary>
    public RunningStyle RunningStyle { get; set; }

    /// <summary>
    /// 脚質を試した結果
    /// </summary>
    public RunningStyleResult RunningStyleResult { get; set; }

    /// <summary>
    /// 失速のタイプ（最後に失速した場所）
    /// </summary>
    public CoursePosition StallPosition { get; set; }

    /// <summary>
    /// 失速のタイプ（一度失速して持ち直した場合、失速した場所）
    /// </summary>
    public CoursePosition CanceledStallPosition { get; set; }

    /// <summary>
    /// 失速のタイプ（一度失速して持ち直した場合、持ち直した場所）
    /// </summary>
    public CoursePosition CanceledStallRecoveredPosition { get; set; }

    /// <summary>
    /// スピード評価
    /// </summary>
    public short SpeedPoint { get; set; }

    /// <summary>
    /// 突破評価
    /// </summary>
    public short BreakthroughPoint { get; set; }

    /// <summary>
    /// 根性評価
    /// </summary>
    public short GutsPoint { get; set; }

    /// <summary>
    /// 直線の評価
    /// </summary>
    public short StraightPoint { get; set; }

    /// <summary>
    /// コーナーの評価
    /// </summary>
    public short CornerPoint { get; set; }

    /// <summary>
    /// 登り坂の評価
    /// </summary>
    public short UphillPoint { get; set; }

    /// <summary>
    /// 下り坂の評価
    /// </summary>
    public short DownhillPoint { get; set; }
  }

  public enum RunningStyleResult : short
  {
    Unknown = 0,

    /// <summary>
    /// 脚質成功
    /// </summary>
    Succeed = 1,

    /// <summary>
    /// 部分的に成功
    /// </summary>
    Partial = 2,

    /// <summary>
    /// 失敗
    /// </summary>
    Failed = 3,
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
