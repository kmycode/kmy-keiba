using KmyKeiba.JVLink.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  [Index(nameof(RaceKey), nameof(Key))]
  public class RaceHorseExtraData : AppDataBase
  {
    public string RaceKey { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public short MiningTime { get; set; }

    public short MiningTimeDiffLonger { get; set; }

    public short MiningTimeDiffShorter { get; set; }

    public short MiningMatchScore { get; set; }

    public short Pci { get; set; }

    public short Pci3 { get; set; }

    public short Rpci { get; set; }

    public short After3HaronOrder { get; set; }

    public short Before3HaronTimeFixed { get; set; }

    public short BaseTime { get; set; }

    public short BaseTimeAs3Haron { get; set; }

    public short CornerOrderDiff2 { get; set; }

    public short CornerOrderDiff3 { get; set; }

    public short CornerOrderDiff4 { get; set; }

    public short CornerOrderDiffGoal { get; set; }

    public short CornerInsideCount { get; set; }

    public short CornerMiddleCount { get; set; }

    public short CornerOutsideCount { get; set; }

    public short CornerAloneCount { get; set; }

    public TrainingType TrainingType { get; set; }

    public TrainingCourseType TrainingCourseType { get; set; }

    public bool IsTrainingSlop { get; set; }
    public bool IsTrainingWood { get; set; }
    public bool IsTrainingDirt { get; set; }
    public bool IsTrainingTurf { get; set; }
    public bool IsTrainingPool { get; set; }
    public bool IsTrainingSteeplechase { get; set; }
    public bool IsTrainingPolyTruck { get; set; }

    public TrainingDistanceType TrainingDistance { get; set; }

    public TrainingEmphasis TrainingEmphasis { get; set; }

    public short TrainingCatchupPoint { get; set; }

    public short TrainingFinishPoint { get; set; }

    public TrainingSizeEvaluation TrainingSizeEvaluation { get; set; }

    public PointDiffType TrainingCatchupPointDiffType { get; set; }

    public string TrainingComment { get; set; } = string.Empty;

    public DateTime TrainingCommentDate { get; set; }

    public TrainingEvaluation TrainingEvaluation { get; set; }

    public short LastWeekTrainingTimePoint { get; set; }

    public TrainingCourseType LastWeekTrainingCourseType { get; set; }

    public short CatchupTrainingWeekday { get; set; }

    public DateTime CatchupTrainingDate { get; set; }

    public TrainingCourse CatchupTrainingCourse { get; set; }

    public TrainingRunningType CatchupRunningType { get; set; }

    public FollowingStatus CatchupFollowingStatus { get; set; }

    public TrainingpRiderType CatchupRiderType { get; set; }

    public short CatchupHarons { get; set; }

    public short CatchupBefore3hTime { get; set; }
    public short CatchupBaseTime { get; set; }
    public short CatchupAfter3hTime { get; set; }
    public short CatchupBefore3hTimePoint { get; set; }
    public short CatchupBaseTimePoint { get; set; }
    public short CatchupAfter3hTimePoint { get; set; }
    public short CatchupPoint { get; set; }

    public TrainingAbreastResult CatchupAbreastResult { get; set; }

    public TrainingRunningType CatchupAbreastRunningType { get; set; }

    public short CatchupAbreastAge { get; set; }

    public JrdbRaceClass CatchupAbreastClass { get; set; }

    public void SetData(string key, string raceKey, short number, MiningTime? time = null, MiningMatch? match = null)
    {
      this.Key = key;
      this.RaceKey = raceKey;

      if (time != null)
      {
        var item = time.Items.FirstOrDefault(i => i.HorseNumber == number);
        if (item.MiningTime != default)
        {
          this.MiningTime = item.MiningTime;
          this.MiningTimeDiffShorter = item.MiningTimeDiffShorter;
          this.MiningTimeDiffLonger = item.MiningTimeDiffLonger;
        }
      }
      if (match != null)
      {
        var item = match.Items.FirstOrDefault(i => i.HorseNumber == number);
        if (item.MiningScore != default)
        {
          this.MiningMatchScore = item.MiningScore;
        }
      }
    }

    public void SetJrdbCybData(string raw)
    {
      var bin = Encoding.GetEncoding(932).GetBytes(raw);

      string AsString(int startIndex, int length)
      {
        var binary = bin![startIndex..(startIndex + length)];
        return Encoding.GetEncoding(932).GetString(binary);
      }

      short.TryParse(AsString(10, 2), out var trainingType);
      short.TryParse(AsString(12, 1), out var trainingCourseType);
      this.TrainingType = (TrainingType)trainingType;
      this.TrainingCourseType = (TrainingCourseType)trainingCourseType;

      var isSlop = AsString(14, 1) != "0";
      var isWood = AsString(16, 1) != "0";
      var isDirt = AsString(18, 1) != "0";
      var isTurf = AsString(20, 1) != "0";
      var isPool = AsString(22, 1) != "0";
      var isSteep = AsString(24, 1) != "0";
      var isPoli = AsString(26, 1) != "0";
      this.IsTrainingSlop = isSlop;
      this.IsTrainingWood = isWood;
      this.IsTrainingDirt = isDirt;
      this.IsTrainingTurf = isTurf;
      this.IsTrainingPool = isPool;
      this.IsTrainingSteeplechase = isSteep;
      this.IsTrainingPolyTruck = isPoli;

      short.TryParse(AsString(27, 1), out var distanceType);
      short.TryParse(AsString(28, 1), out var trainingEmphasis);
      short.TryParse(AsString(29, 3).TrimStart(), out var catchupPoint);
      short.TryParse(AsString(32, 3).TrimStart(), out var finishPoint);
      this.TrainingDistance = (TrainingDistanceType)distanceType;
      this.TrainingEmphasis = (TrainingEmphasis)trainingEmphasis;
      this.TrainingCatchupPoint = catchupPoint;
      this.TrainingFinishPoint = finishPoint;

      var trainingSizeEvaluation = AsString(35, 1);
      short.TryParse(AsString(36, 1), out var catchupPointDiffType);
      this.TrainingSizeEvaluation = trainingSizeEvaluation switch
      {
        "A" => TrainingSizeEvaluation.Many,
        "B" => TrainingSizeEvaluation.Normal,
        "C" => TrainingSizeEvaluation.Few,
        "D" => TrainingSizeEvaluation.VeryFew,
        _ => TrainingSizeEvaluation.Unknown,
      };
      this.TrainingCatchupPointDiffType = (PointDiffType)catchupPointDiffType;

      var trainingComment = AsString(37, 40);
      DateTime.TryParseExact(AsString(77, 8), "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var trainingCommentDate);
      this.TrainingComment = trainingComment;
      this.TrainingCommentDate = trainingCommentDate;

      short.TryParse(AsString(85, 1), out var trainingEvaluation);
      short.TryParse(AsString(86, 3).TrimStart(), out var lastWeekTrainingTimePoint);
      short.TryParse(AsString(89, 2).TrimStart(), out var lastWeekTrainingCourseType);
      this.TrainingEvaluation = (TrainingEvaluation)trainingEvaluation;
      this.LastWeekTrainingTimePoint = lastWeekTrainingTimePoint;
      this.LastWeekTrainingCourseType = (TrainingCourseType)lastWeekTrainingCourseType;
    }

    public void SetJrdbChaData(string raw)
    {
      var bin = Encoding.GetEncoding(932).GetBytes(raw);

      string AsString(int startIndex, int length)
      {
        var binary = bin![startIndex..(startIndex + length)];
        return Encoding.GetEncoding(932).GetString(binary);
      }

      var weekday = AsString(10, 2) switch
      {
        "日" => 1,
        "月" => 2,
        "火" => 3,
        "水" => 4,
        "木" => 5,
        "金" => 6,
        "土" => 7,
        _ => 0,
      };
      DateTime.TryParseExact(AsString(12, 8), "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var trainingDate);
      this.CatchupTrainingWeekday = (short)weekday;
      this.CatchupTrainingDate = trainingDate;

      var trainingCourseRaw = AsString(21, 2);
      if (trainingCourseRaw == "A1")
      {
        this.CatchupTrainingCourse = (TrainingCourse)101;
      }
      else if (trainingCourseRaw == "B1")
      {
        this.CatchupTrainingCourse = (TrainingCourse)102;
      }
      else
      {
        short.TryParse(trainingCourseRaw, out var trainingCourse);
        this.CatchupTrainingCourse = (TrainingCourse)trainingCourse;
      }

      short.TryParse(AsString(23, 1), out var catchupType);
      short.TryParse(AsString(24, 2), out var followStatus);
      short.TryParse(AsString(26, 1), out var trainingRiderType);
      short.TryParse(AsString(27, 1), out var trainingHarons);
      this.CatchupRunningType = (TrainingRunningType)catchupType;
      this.CatchupFollowingStatus = (FollowingStatus)followStatus;
      this.CatchupRiderType = (TrainingpRiderType)trainingRiderType;
      this.CatchupHarons = trainingHarons;

      short.TryParse(AsString(28, 3).TrimStart(), out var before3fTime);
      short.TryParse(AsString(31, 3).TrimStart(), out var baseTime);
      short.TryParse(AsString(34, 3).TrimStart(), out var after3fTime);
      short.TryParse(AsString(37, 3).TrimStart(), out var before3fTimePoint);
      short.TryParse(AsString(40, 3).TrimStart(), out var baseTimePoint);
      short.TryParse(AsString(43, 3).TrimStart(), out var after3fTimePoint);
      short.TryParse(AsString(46, 3).TrimStart(), out var catchupPoint);
      this.CatchupBefore3hTime = before3fTime;
      this.CatchupBaseTime = baseTime;
      this.CatchupAfter3hTime = after3fTime;
      this.CatchupBefore3hTimePoint = before3fTimePoint;
      this.CatchupBaseTimePoint = baseTimePoint;
      this.CatchupAfter3hTimePoint = after3fTimePoint;
      this.CatchupPoint = catchupPoint;

      short.TryParse(AsString(49, 1), out var abreastResultType);
      short.TryParse(AsString(50, 1), out var catchupAbreastResultType);
      short.TryParse(AsString(51, 2).TrimStart(), out var abreastAge);
      this.CatchupAbreastResult = (TrainingAbreastResult)abreastResultType;
      this.CatchupAbreastRunningType = (TrainingRunningType)catchupAbreastResultType;
      this.CatchupAbreastAge = abreastAge;

      var abreastClassType = AsString(53, 2);
      this.CatchupAbreastClass = abreastClassType switch
      {
        "04" => JrdbRaceClass.Win1,
        "05" => JrdbRaceClass.Win1_2,
        "08" => JrdbRaceClass.Win2,
        "09" => JrdbRaceClass.Win2_2,
        "10" => JrdbRaceClass.Win2_3,
        "15" => JrdbRaceClass.Win3,
        "16" => JrdbRaceClass.Win3_2,
        "A1" => JrdbRaceClass.NewHorse,
        "A2" => JrdbRaceClass.Unraced,
        "A3" => JrdbRaceClass.Maiden,
        "OP" => JrdbRaceClass.Open,
        _ => JrdbRaceClass.Unknown,
      };
    }
  }

  public enum TrainingType : short
  {
    Unknown = 0,
    Sparta = 1,
    NormalMore = 2,
    Norikomi = 3,
    FullAverage = 4,
    Normal = 5,
    UmanariAverage = 6,
    Quick = 7,
    NormalLess = 8,
    Light = 9,
  }

  public enum TrainingDistanceType : short
  {
    Unknown = 0,
    Long = 1,
    Normal = 2,
    Short = 3,
    Double = 4,
    Other = 5,
  }

  public enum TrainingCourse : short
  {
    /*
     * 01      美浦坂路        美坂
02      南Ｗ            南Ｗ
03      南Ｄ            南Ｄ
04      南芝            南芝
05      南Ａ            南Ａ
06      北Ｂ            北Ｂ
07      北Ｃ            北Ｃ
08      美浦障害芝      美障
09      美浦プール      美プ
10      南ポリトラック  南Ｐ
11      栗東坂路        栗坂
12      ＣＷ            ＣＷ
13      ＤＷ            ＤＷ
14      栗Ｂ            栗Ｂ
15      栗Ｅ            栗Ｅ
16      栗芝            栗芝
17      栗ポリトラック  栗Ｐ
18      栗東障害        栗障
19      栗東プール      栗プ
21      札幌ダ          札ダ
22      札幌芝          札芝
23      函館ダ          函ダ
24      函館芝          函芝
25      函館Ｗ          函Ｗ
26      福島芝          福芝
27      福島ダ          福ダ
28      新潟芝          新芝
29      新潟ダ          新ダ
30      東京芝          東芝
31      東京ダ          東ダ
32      中山芝          中芝
33      中山ダ          中ダ
34      中京芝          名芝
35      中京ダ          名ダ
36      京都芝          京芝
37      京都ダ          京ダ
38      阪神芝          阪芝
39      阪神ダ          阪ダ
40      小倉芝          小芝
41      小倉ダ          小ダ
42      福島障害        福障
43      新潟障害        新障
44      東京障害        東障
45      中山障害        中障
46      中京障害        名障
47      京都障害        京障
48      阪神障害        阪障
49      小倉障害        小障
50      地方競馬        地方
61      障害試験        障試
62      北障害          北障
68      美障害ダ        美障
70      北A             北Ａ
81      美ゲート        美ゲ
82      栗ゲート        栗ゲ
88      牧場            牧場
93      白井ダ          白井
A1      連闘            連闘    -> 101
B1      その他          他      -> 102
     */
  }

  public enum TrainingCourseType : short
  {
    Unknown = 0,
    Slop = 1,
    Course = 2,
    SlopAndCourse = 3,
    Steeplechase = 4,
    SteeplechaseAndOthers = 5,
  }

  public enum TrainingEmphasis : short
  {
    Unknown = 0,
    Before3f = 1,
    Base = 2,
    After3f = 3,
    Middle = 4,
  }

  public enum TrainingSizeEvaluation : short
  {
    Unknown = 0,
    Many = 1,
    Normal = 2,
    Few = 3,
    VeryFew = 4,
  }

  public enum TrainingEvaluation : short
  {
    Unknown = 0,
    Good = 1,
    Middle = 2,
    Bad = 3,
  }

  public enum PointDiffType : short
  {
    Unknown = 0,
    PlusPlus = 1,
    Plus = 2,
    Middle = 3,
    Minus = 4,
  }

  public enum TrainingRunningType : short
  {
    Unknown = 0,
    Full = 1,
    Strong = 2,
    Umanari = 3,
  }

  public enum FollowingStatus : short
  {
    /*01      流す
02      余力あり
03      終い抑え
04      一杯
05      バテる
06      伸びる
07      テンのみ
08      鋭く伸び
09      強目
10      終い重点
11      ８分追い
12      追って伸
13      向正面
14      ゲート
15      障害練習
16      中間軽め
17      キリ
21      引っ張る
22      掛かる
23      掛リバテ
24      テン掛る
25      掛り一杯
26      ササル
27      ヨレル
28      バカつく
29      手間取る
99      その他
     */
  }

  public enum TrainingpRiderType : short
  {
    Unknown = 0,
    Assistant = 1,
    Trainer = 2,
    Rider = 3,
    TrainingRider = 4,
    Apprentice = 5,
  }

  public enum TrainingAbreastResult : short
  {
    Unknown = 0,
    Win = 1,
    Draw = 2,
    Lose = 3,
  }

  public enum JrdbRaceClass : short
  {
    Unknown = 0,
    Win1 = 4,
    Win1_2 = 5,
    Win2 = 8,
    Win2_2 = 9,
    Win2_3 = 10,
    Win3 = 15,
    Win3_2 = 16,
    NewHorse = 101,
    Unraced = 102,
    Maiden = 103,
    Open = 104,
  }
}
