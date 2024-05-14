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
  public class Race : EntityBase
  {
    public string Key { get; set; } = string.Empty;

    public short Nichiji { get; set; }

    /// <summary>
    /// レースの名前
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// レースの名前（6文字）
    /// </summary>
    public string Name6Chars { get; set; } = string.Empty;

    /// <summary>
    /// レースの副題
    /// </summary>
    public string SubName { get; set; } = string.Empty;

    /// <summary>
    /// 特別競走番号
    /// </summary>
    public short GradeId { get; set; }

    /// <summary>
    /// 競馬場
    /// </summary>
    public RaceCourse Course { get; set; }

    /// <summary>
    /// トラックコード（外部指数の内部変数置換に使う）
    /// </summary>
    public short TrackCode { get; set; }

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
    /// ばんえいの水分率
    /// </summary>
    public short BaneiMoisture { get; set; }

    public RaceRiderWeightRule RiderWeight { get; set; }

    public RaceHorseAreaRule Area { get; set; }

    public RaceHorseSexRule Sex { get; set; }

    public RaceCrossRaceRule Cross { get; set; }

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
    /// 参加条件
    /// </summary>
    public RaceSubject Subject { get; set; } = new();

    /// <summary>
    /// 馬の数
    /// </summary>
    public short HorsesCount { get; set; }

    /// <summary>
    /// 入選数
    /// </summary>
    public short ResultHorsesCount { get; set; }

    /// <summary>
    /// 出走日時
    /// </summary>
    public DateTime StartTime { get; set; }

    public string Corner1Result { get; set; } = string.Empty;

    public short Corner1Position { get; set; }

    public short Corner1Number { get; set; }

    public string Corner2Result { get; set; } = string.Empty;

    public short Corner2Position { get; set; }

    public short Corner2Number { get; set; }

    public string Corner3Result { get; set; } = string.Empty;

    public short Corner3Position { get; set; }

    public short Corner3Number { get; set; }

    public string Corner4Result { get; set; } = string.Empty;

    public short Corner4Position { get; set; }

    public short Corner4Number { get; set; }

    public short[] LapTimes { get; set; } = Array.Empty<short>();

    public int PrizeMoney1 { get; set; }

    public int PrizeMoney2 { get; set; }

    public int PrizeMoney3 { get; set; }

    public int PrizeMoney4 { get; set; }

    public int PrizeMoney5 { get; set; }

    public int PrizeMoney6 { get; set; }

    public int PrizeMoney7 { get; set; }

    public int ExtraPrizeMoney1 { get; set; }

    public int ExtraPrizeMoney2 { get; set; }

    public int ExtraPrizeMoney3 { get; set; }

    public int ExtraPrizeMoney4 { get; set; }

    public int ExtraPrizeMoney5 { get; set; }

    public short BeforeHaronTime3 { get; set; }

    public short BeforeHaronTime4 { get; set; }

    public short AfterHaronTime3 { get; set; }

    public short AfterHaronTime4 { get; set; }

    public short SteeplechaseMileTime { get; set; }

    internal Race()
    {
    }

    internal static (TrackType, TrackGround, TrackCornerDirection, TrackOption) GetTrackType(int track)
    {
      var trackGround = TrackGround.Unknown;
      var trackOption = TrackOption.Unknown;
      var trackType = TrackType.Unknown;
      var trackCornerDirection = TrackCornerDirection.Unknown;
      trackType = track >= 10 && track <= 29 ? TrackType.Flat :
        track >= 51 && track <= 59 ? TrackType.Steeplechase :
        TrackType.Unknown;
      trackGround = ((track >= 10 && track <= 22) || track == 51 || (track >= 53 && track <= 59)) ? TrackGround.Turf :
        ((track >= 23 && track <= 26) || track == 29) ? TrackGround.Dirt :
        (track == 27 || track == 28) ? TrackGround.Sand :
        track == 52 ? TrackGround.TurfToDirt : TrackGround.Unknown;
      trackCornerDirection = track == 10 || track == 29 ? TrackCornerDirection.Straight :
        ((track >= 11 && track <= 16) || track == 23 || track == 25 || track == 27 || track == 53) ? TrackCornerDirection.Left :
        ((track >= 17 && track <= 22) || track == 24 || track == 26 || track == 28) ? TrackCornerDirection.Right :
        TrackCornerDirection.Unknown;
      trackOption = track == 12 || track == 18 || track == 26 || track == 55 ? TrackOption.Outside :
        track == 13 || track == 19 || track == 57 ? TrackOption.InsideToOutside :
        track == 14 || track == 20 || track == 56 ? TrackOption.OutsideToInside :
        track == 15 || track == 21 || track == 58 ? TrackOption.Inside2 :
        track == 16 || track == 22 || track == 59 ? TrackOption.Outside2 :
        TrackOption.Unknown;
      return (trackType, trackGround, trackCornerDirection, trackOption);
    }

    public static Race FromJV(JVData_Struct.JV_RA_RACE race)
    {
      var name = race.RaceInfo.Hondai.Trim();
      if (string.IsNullOrEmpty(name))
      {
        name = race.JyokenName.Trim();
      }

      var name6 = race.RaceInfo.Ryakusyo6.Trim();
      if (string.IsNullOrEmpty(name6))
      {
        name6 = new Regex(@"(\s|　)[\s　]+").Replace(name6, "　");
      }

      var startTime = DateTime.ParseExact($"{race.id.Year}{race.id.MonthDay}{race.HassoTime}", "yyyyMMddHHmm", null);
      short.TryParse(race.id.Nichiji, out var nichiji);

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

      var subject = RaceSubject.FromJV(race);

      short.TryParse(race.RaceInfo.TokuNum, out var gradeId);
      short.TryParse(race.id.RaceNum, out short courseRaceNum);
      short.TryParse(race.TorokuTosu, out short horsesCount);
      short.TryParse(race.NyusenTosu, out short horsesCount2);
      short.TryParse(race.Kyori, out short distance);

      int.TryParse(race.TrackCD, out int track);
      var (trackType, trackGround, trackCornerDirection, trackOption) = GetTrackType(track);

      int.TryParse(race.TenkoBaba.TenkoCD, out int weather);

      // 地方競馬DATAは、盛岡の芝もダートとして配信し、SibaBabaCDには何も設定しないみたい
      int.TryParse((trackGround == TrackGround.Turf && course <= RaceCourse.CentralMaxValue) ? race.TenkoBaba.SibaBabaCD : race.TenkoBaba.DirtBabaCD, out int condition);

      short moisture = default;
      if (course == RaceCourse.ObihiroBannei)
      {
        // ばんえいの水分率
        try
        {
          moisture = Convert.ToInt16(race.TenkoBaba.SibaBabaCD + race.TenkoBaba.DirtBabaCD, 16);
        }
        catch { }
      }

      short.TryParse(race.JyokenInfo.JyuryoCD, out var riderWeight);
      var kigo1 = (RaceHorseAreaRule)(short)(race.JyokenInfo.KigoCD.ToLower()[0] - 'a' + 1);
      RaceHorseSexRule kigo2;
      {
        var k = race.JyokenInfo.KigoCD.ToLower()[1];
        if (k == 'a')
        {
          kigo2 = RaceHorseSexRule.A;
        }
        else if (k == 'b')
        {
          kigo2 = RaceHorseSexRule.B;
        }
        else
        {
          kigo2 = (RaceHorseSexRule)(short)(k - '0');
        }
      }
      RaceCrossRaceRule kigo3;
      var kigo3Char = race.JyokenInfo.KigoCD.ToLower()[2];
      if (kigo3Char <= '9')
      {
        kigo3 = (RaceCrossRaceRule)(short)(kigo3Char - '0');
      }
      else
      {
        kigo3 = (RaceCrossRaceRule)(short)(kigo3Char - 'a' + 1 + 9);
      }

      short.TryParse(race.CornerInfo[0].Syukaisu, out var cnum1);
      short.TryParse(race.CornerInfo[0].Corner, out var cn1);
      short.TryParse(race.CornerInfo[1].Syukaisu, out var cnum2);
      short.TryParse(race.CornerInfo[1].Corner, out var cn2);
      short.TryParse(race.CornerInfo[2].Syukaisu, out var cnum3);
      short.TryParse(race.CornerInfo[2].Corner, out var cn3);
      short.TryParse(race.CornerInfo[3].Syukaisu, out var cnum4);
      short.TryParse(race.CornerInfo[3].Corner, out var cn4);

      short.TryParse(race.HaronTimeS3, out var haronb3);
      short.TryParse(race.HaronTimeS4, out var haronb4);
      short.TryParse(race.HaronTimeL3, out var haron3);
      short.TryParse(race.HaronTimeL4, out var haron4);
      short.TryParse(race.SyogaiMileTime, out var mile);

      int.TryParse(race.Honsyokin[0], out var money1);
      int.TryParse(race.Honsyokin[1], out var money2);
      int.TryParse(race.Honsyokin[2], out var money3);
      int.TryParse(race.Honsyokin[3], out var money4);
      int.TryParse(race.Honsyokin[4], out var money5);
      int.TryParse(race.Honsyokin[5], out var money6);
      int.TryParse(race.Honsyokin[6], out var money7);
      int.TryParse(race.Fukasyokin[0], out var emoney1);
      int.TryParse(race.Fukasyokin[1], out var emoney2);
      int.TryParse(race.Fukasyokin[2], out var emoney3);
      int.TryParse(race.Fukasyokin[3], out var emoney4);
      int.TryParse(race.Fukasyokin[4], out var emoney5);

      var obj = new Race
      {
        LastModified = race.head.MakeDate.ToDateTime(),
        Key = race.id.ToRaceKey(),
        Nichiji = nichiji,
        DataStatus = race.head.DataKubun.ToDataStatus(),
        Name = name,
        Name6Chars = name6,
        SubName = race.RaceInfo.Fukudai.Trim(),
        GradeId = gradeId,
        Course = course,
        CourseType = race.CourseKubunCD.Trim(),
        TrackCode = (short)track,
        TrackGround = trackGround,
        TrackType = trackType,
        TrackCornerDirection = trackCornerDirection,
        TrackOption = trackOption,
        TrackWeather = (RaceCourseWeather)weather,
        TrackCondition = (RaceCourseCondition)condition,
        BaneiMoisture = moisture,
        RiderWeight = (RaceRiderWeightRule)riderWeight,
        Area = kigo1,
        Sex = kigo2,
        Cross = kigo3,
        Distance = distance,
        CourseRaceNumber = courseRaceNum,
        Subject = subject,
        HorsesCount = horsesCount,
        ResultHorsesCount = horsesCount2,
        StartTime = startTime,
        Corner1Position = cn1,
        Corner1Number = cnum1,
        Corner1Result = race.CornerInfo[0].Jyuni.Trim(),
        Corner2Position = cn2,
        Corner2Number = cnum2,
        Corner2Result = race.CornerInfo[1].Jyuni.Trim(),
        Corner3Position = cn3,
        Corner3Number = cnum3,
        Corner3Result = race.CornerInfo[2].Jyuni.Trim(),
        Corner4Position = cn4,
        Corner4Number = cnum4,
        Corner4Result = race.CornerInfo[3].Jyuni.Trim(),
        LapTimes = race.LapTime.Select(lt =>
        {
          short.TryParse(lt, out var s);
          return s;
        }).Where(lt => lt != default).ToArray(),
        PrizeMoney1 = money1,
        PrizeMoney2 = money2,
        PrizeMoney3 = money3,
        PrizeMoney4 = money4,
        PrizeMoney5 = money5,
        PrizeMoney6 = money6,
        PrizeMoney7 = money7,
        ExtraPrizeMoney1 = emoney1,
        ExtraPrizeMoney2 = emoney2,
        ExtraPrizeMoney3 = emoney3,
        ExtraPrizeMoney4 = emoney4,
        ExtraPrizeMoney5 = emoney5,
        BeforeHaronTime3 = haronb3,
        BeforeHaronTime4 = haronb4,
        AfterHaronTime3 = haron3,
        AfterHaronTime4 = haron4,
        SteeplechaseMileTime = mile,
      };
      return obj;
    }

    public override int GetHashCode() => this.Key.GetHashCode();
  }

  public enum RaceDataStatus : short
  {
    Unknown = -1,

    /// <summary>
    /// 削除
    /// </summary>
    Delete = 0,

    /// <summary>
    /// 出走馬名表
    /// </summary>
    Horses = 1,

    /// <summary>
    /// 出走表
    /// </summary>
    Horses2 = 2,

    /// <summary>
    /// 速報成績（3着まで確定）
    /// </summary>
    PreliminaryGrade3 = 3,

    /// <summary>
    /// 速報成績（５着まで確定）
    /// </summary>
    PreliminaryGrade5 = 4,

    /// <summary>
    /// 速報成績（全員確定）
    /// </summary>
    PreliminaryGrade = 5,

    /// <summary>
    /// 速報成績
    /// </summary>
    PreliminaryGradeFull = 6,

    /// <summary>
    /// 成績
    /// </summary>
    Grade = 7,

    /// <summary>
    /// 中止
    /// </summary>
    Canceled = 9,

    /// <summary>
    /// 地方競馬
    /// </summary>
    Local = 101,

    /// <summary>
    /// 海外
    /// </summary>
    Foreign = 102,
  }

  public enum OddsDataStatus : short
  {
    /// <summary>
    /// 中間
    /// </summary>
    RealTime = 1,

    /// <summary>
    /// 前日最終
    /// </summary>
    PreviousDay = 2,

    /// <summary>
    /// 最終
    /// </summary>
    Last = 3,

    /// <summary>
    /// 確定
    /// </summary>
    Determined = 4,

    /// <summary>
    /// 確定（月曜日）
    /// </summary>
    DeterminedInMonday = 5,

    /// <summary>
    /// 中止
    /// </summary>
    Canceled = 9,

    /// <summary>
    /// 削除
    /// </summary>
    Deleted = 0,
  }

  class RaceCourseInfoAttribute : Attribute
  {
    public string Name { get; }

    public RaceCourseType Type { get; }

    public string Key { get; set; } = string.Empty;

    public RaceCourseInfoAttribute(RaceCourseType type, string name)
    {
      this.Type = type;
      this.Name = name;
    }
  }

  public enum RaceCourseType : short
  {
    Unknown = 0,
    Central = 1,
    Local = 2,
    Foreign = 3,
  }

  public enum RaceCourse : short
  {
    [RaceCourseInfo(RaceCourseType.Unknown, "全て")]
    All = -1,

    [RaceCourseInfo(RaceCourseType.Central, "不明")]
    Unknown = 0,

    [RaceCourseInfo(RaceCourseType.Central, "札幌")]
    Sapporo = 1,

    [RaceCourseInfo(RaceCourseType.Central, "函館")]
    Hakodate = 2,

    [RaceCourseInfo(RaceCourseType.Central, "福島")]
    Fukushima = 3,

    [RaceCourseInfo(RaceCourseType.Central, "新潟")]
    Niigata = 4,

    [RaceCourseInfo(RaceCourseType.Central, "東京")]
    Tokyo = 5,

    [RaceCourseInfo(RaceCourseType.Central, "中山")]
    Nakayama = 6,

    [RaceCourseInfo(RaceCourseType.Central, "中京")]
    Chukyo = 7,

    [RaceCourseInfo(RaceCourseType.Central, "京都")]
    Kyoto = 8,

    [RaceCourseInfo(RaceCourseType.Central, "阪神")]
    Hanshin = 9,

    [RaceCourseInfo(RaceCourseType.Central, "小倉")]
    Kokura = 10,

    [RaceCourseInfo(RaceCourseType.Local, "門別")]
    Mombetsu = 30,

    [RaceCourseInfo(RaceCourseType.Local, "北見")]
    Kitami = 31,

    [RaceCourseInfo(RaceCourseType.Local, "岩見沢")]
    Iwamizawa = 32,

    [RaceCourseInfo(RaceCourseType.Local, "帯広")]
    Obihiro = 33,

    [RaceCourseInfo(RaceCourseType.Local, "旭川")]
    Asahikawa = 34,

    [RaceCourseInfo(RaceCourseType.Local, "盛岡")]
    Morioka = 35,

    [RaceCourseInfo(RaceCourseType.Local, "水沢")]
    Mizusawa = 36,

    [RaceCourseInfo(RaceCourseType.Local, "上山")]
    Kaminoyama = 37,

    [RaceCourseInfo(RaceCourseType.Local, "三条")]
    Sanjo = 38,

    [RaceCourseInfo(RaceCourseType.Local, "足利")]
    Ashikaka = 39,

    [RaceCourseInfo(RaceCourseType.Local, "宇都宮")]
    Utsunomiya = 40,

    [RaceCourseInfo(RaceCourseType.Local, "高崎")]
    Takasaki = 41,

    [RaceCourseInfo(RaceCourseType.Local, "浦和")]
    Urawa = 42,

    [RaceCourseInfo(RaceCourseType.Local, "船橋")]
    Funabashi = 43,

    [RaceCourseInfo(RaceCourseType.Local, "大井")]
    Oi = 44,

    [RaceCourseInfo(RaceCourseType.Local, "川崎")]
    Kawazaki = 45,

    [RaceCourseInfo(RaceCourseType.Local, "金沢")]
    Kanazawa = 46,

    [RaceCourseInfo(RaceCourseType.Local, "笠松")]
    Kasamatsu = 47,

    [RaceCourseInfo(RaceCourseType.Local, "名古屋")]
    Nagoya = 48,

    [RaceCourseInfo(RaceCourseType.Local, "紀三井寺")]
    Kimidera = 49,

    [RaceCourseInfo(RaceCourseType.Local, "園田")]
    Sonoda = 50,

    [RaceCourseInfo(RaceCourseType.Local, "姫路")]
    Himeji = 51,

    [RaceCourseInfo(RaceCourseType.Local, "益田")]
    Masuda = 52,

    [RaceCourseInfo(RaceCourseType.Local, "福山")]
    Fukuyama = 53,

    [RaceCourseInfo(RaceCourseType.Local, "高知")]
    Kochi = 54,

    [RaceCourseInfo(RaceCourseType.Local, "佐賀")]
    Saga = 55,

    [RaceCourseInfo(RaceCourseType.Local, "荒尾")]
    Arao = 56,

    [RaceCourseInfo(RaceCourseType.Local, "中津")]
    Nakatsu = 57,

    [RaceCourseInfo(RaceCourseType.Local, "札幌(地)")]
    SapporoLocal = 58,

    [RaceCourseInfo(RaceCourseType.Local, "函館(地)")]
    HakodateLocal = 59,

    [RaceCourseInfo(RaceCourseType.Local, "新潟(地)")]
    NiigataLocal = 60,

    [RaceCourseInfo(RaceCourseType.Local, "中京(地)")]
    ChukyoLocal = 61,

    [RaceCourseInfo(RaceCourseType.Local, "帯広ば")]
    ObihiroBannei = 83,

    [RaceCourseInfo(RaceCourseType.Foreign, "その他の外国", Key = "A0")]
    Foreign = 1000,

    [RaceCourseInfo(RaceCourseType.Foreign, "日本", Key = "A2")]
    Japan = 1001,

    [RaceCourseInfo(RaceCourseType.Foreign, "アメリカ", Key = "A4")]
    Usa = 1002,

    [RaceCourseInfo(RaceCourseType.Foreign, "イギリス", Key = "A6")]
    Uk = 1003,

    [RaceCourseInfo(RaceCourseType.Foreign, "フランス", Key = "A8")]
    France = 1004,

    [RaceCourseInfo(RaceCourseType.Foreign, "インド", Key = "B0")]
    India = 1005,

    [RaceCourseInfo(RaceCourseType.Foreign, "アイルランド", Key = "B2")]
    Ireland = 1006,

    [RaceCourseInfo(RaceCourseType.Foreign, "ニュージーランド", Key = "B4")]
    NewZealand = 2000,

    [RaceCourseInfo(RaceCourseType.Foreign, "オーストラリア", Key = "B6")]
    Australia = 1007,

    [RaceCourseInfo(RaceCourseType.Foreign, "カナダ", Key = "B8")]
    Canada = 1008,

    [RaceCourseInfo(RaceCourseType.Foreign, "イタリア", Key = "C0")]
    Italy = 1009,

    [RaceCourseInfo(RaceCourseType.Foreign, "ドイツ", Key = "C2")]
    Germany = 1010,

    [RaceCourseInfo(RaceCourseType.Foreign, "オマーン", Key = "C5")]
    Oman = 1011,

    [RaceCourseInfo(RaceCourseType.Foreign, "イラク", Key = "C6")]
    Iraq = 1012,

    [RaceCourseInfo(RaceCourseType.Foreign, "アラブ首長国連邦", Key = "C7")]
    Arab = 1013,

    [RaceCourseInfo(RaceCourseType.Foreign, "シリア", Key = "C8")]
    Syria = 1014,

    [RaceCourseInfo(RaceCourseType.Foreign, "スウェーデン", Key = "D0")]
    Sweden = 1015,

    [RaceCourseInfo(RaceCourseType.Foreign, "ハンガリー", Key = "D2")]
    Hungary = 1016,

    [RaceCourseInfo(RaceCourseType.Foreign, "ポルトガル", Key = "D4")]
    Portugal = 1017,

    [RaceCourseInfo(RaceCourseType.Foreign, "ロシア", Key = "D6")]
    Russia = 1018,

    [RaceCourseInfo(RaceCourseType.Foreign, "ウルグアイ", Key = "D8")]
    Uruguay = 1019,

    [RaceCourseInfo(RaceCourseType.Foreign, "ペルー", Key = "E0")]
    Peru = 1020,

    [RaceCourseInfo(RaceCourseType.Foreign, "アルゼンチン", Key = "E2")]
    Argentina = 1021,

    [RaceCourseInfo(RaceCourseType.Foreign, "ブラジル", Key = "E4")]
    Brazil = 1022,

    [RaceCourseInfo(RaceCourseType.Foreign, "ベルギー", Key = "E6")]
    Belgium = 1023,

    [RaceCourseInfo(RaceCourseType.Foreign, "トルコ", Key = "E8")]
    Turkey = 1024,

    [RaceCourseInfo(RaceCourseType.Foreign, "韓国", Key = "F0")]
    Korea = 1025,

    [RaceCourseInfo(RaceCourseType.Foreign, "中国", Key = "F1")]
    China = 1026,

    [RaceCourseInfo(RaceCourseType.Foreign, "チリ", Key = "F2")]
    Chile = 1027,

    [RaceCourseInfo(RaceCourseType.Foreign, "パナマ", Key = "F8")]
    Panama = 1028,

    [RaceCourseInfo(RaceCourseType.Foreign, "香港", Key = "G0")]
    HongKong = 1029,

    [RaceCourseInfo(RaceCourseType.Foreign, "スペイン", Key = "G2")]
    Spain = 1030,
    
    [RaceCourseInfo(RaceCourseType.Foreign, "民主ドイツ", Key = "H0")]
    WestGermany = 1031,

    [RaceCourseInfo(RaceCourseType.Foreign, "南アフリカ", Key = "H2")]
    SouthAfrica = 1032,

    [RaceCourseInfo(RaceCourseType.Foreign, "スイス", Key = "H4")]
    Swizerland = 1033,

    [RaceCourseInfo(RaceCourseType.Foreign, "モナコ", Key = "H6")]
    Monaco = 1034,

    [RaceCourseInfo(RaceCourseType.Foreign, "フィリピン", Key = "H8")]
    Philippines = 1035,

    [RaceCourseInfo(RaceCourseType.Foreign, "プエルトリコ", Key = "I0")]
    PuertoRico = 1036,

    [RaceCourseInfo(RaceCourseType.Foreign, "コロンビア", Key = "I2")]
    Columbia = 1037,

    [RaceCourseInfo(RaceCourseType.Foreign, "チェコスロバキア", Key = "I4")]
    Czechoslovakia = 1038,

    [RaceCourseInfo(RaceCourseType.Foreign, "チェコ", Key = "I6")]
    Czech = 1039,

    [RaceCourseInfo(RaceCourseType.Foreign, "スロバキア", Key = "I8")]
    Slovakia = 1040,

    [RaceCourseInfo(RaceCourseType.Foreign, "エクアドル", Key = "J0")]
    Ecuador = 1041,

    [RaceCourseInfo(RaceCourseType.Foreign, "ギリシャ", Key = "J2")]
    Greece = 1042,

    [RaceCourseInfo(RaceCourseType.Foreign, "マレーシア", Key = "J4")]
    Malaysia = 1043,

    [RaceCourseInfo(RaceCourseType.Foreign, "メキシコ", Key = "J6")]
    Mexico = 1044,

    [RaceCourseInfo(RaceCourseType.Foreign, "モロッコ", Key = "J8")]
    Morocco = 1045,

    [RaceCourseInfo(RaceCourseType.Foreign, "パキスタン", Key = "K0")]
    Pakistan = 1046,

    [RaceCourseInfo(RaceCourseType.Foreign, "ポーランド", Key = "K2")]
    Poland = 1047,

    [RaceCourseInfo(RaceCourseType.Foreign, "パラグアイ", Key = "K4")]
    Paraguay = 1048,

    [RaceCourseInfo(RaceCourseType.Foreign, "サウジアラビア", Key = "K6")]
    SaudiArabia = 1049,

    [RaceCourseInfo(RaceCourseType.Foreign, "キプロス", Key = "K8")]
    Cyprus = 1050,

    [RaceCourseInfo(RaceCourseType.Foreign, "タイ", Key = "L0")]
    Thailand = 1051,

    [RaceCourseInfo(RaceCourseType.Foreign, "ウクライナ", Key = "L2")]
    Ukraine = 1052,

    [RaceCourseInfo(RaceCourseType.Foreign, "ベネズエラ", Key = "L4")]
    Venezuela = 1053,

    [RaceCourseInfo(RaceCourseType.Foreign, "ユーゴスラビア", Key = "L6")]
    Yugoslavia = 1054,

    [RaceCourseInfo(RaceCourseType.Foreign, "デンマーク", Key = "L8")]
    Denmark = 1055,

    [RaceCourseInfo(RaceCourseType.Foreign, "シンガポール", Key = "M0")]
    Singapore = 1056,

    [RaceCourseInfo(RaceCourseType.Foreign, "マカオ", Key = "M2")]
    Macau = 1057,

    [RaceCourseInfo(RaceCourseType.Foreign, "オーストリア", Key = "M4")]
    Austria = 1058,

    [RaceCourseInfo(RaceCourseType.Foreign, "ヨルダン", Key = "M6")]
    Jordan = 1059,

    [RaceCourseInfo(RaceCourseType.Foreign, "カタール", Key = "M8")]
    Qatar = 1060,

    [RaceCourseInfo(RaceCourseType.Foreign, "東ドイツ", Key = "N0")]
    EastGermany = 1061,

    [RaceCourseInfo(RaceCourseType.Foreign, "バーレーン", Key = "N2")]
    Bahrain = 1062,

    LocalMinValue = 29,  // ほんとは30だけど便宜上
    CentralMaxValue = 28,
  }

  public enum TrackGround : short
  {
    Unknown = 0,
    Turf = 1,
    Dirt = 2,
    TurfToDirt = 3,
    Sand = 4,
  }

  public enum TrackCornerDirection : short
  {
    Unknown = 0,
    Left = 1,
    Right = 2,
    Straight = 3,
  }

  public enum TrackOption : short
  {
    Unknown = 0,
    Outside = 1,
    Inside = 2,
    OutsideToInside = 3,
    InsideToOutside = 4,
    Outside2 = 5,
    Inside2 = 6,
  }

  public enum TrackType : short
  {
    Unknown = 0,
    Flat = 1,
    Steeplechase = 2,
  }

  public enum RaceCourseWeather : short
  {
    Unknown = 0,
    Fine = 1,
    Cloudy = 2,
    Rainy = 3,
    Drizzle = 4,
    Snow = 5,
    LightSnow = 6,
  }

  public enum RaceCourseCondition : short
  {
    Unknown = 0,
    Standard = 1,
    Good = 2,
    Yielding = 3,
    Soft = 4,
  }

  public enum RaceRiderWeightRule : short
  {
    Unset = 0,

    /// <summary>
    /// ハンデ（実績などで重量を決定）
    /// </summary>
    Handicap = 1,

    /// <summary>
    /// 別定（レースごとに負担を決める基準がある）
    /// </summary>
    SpecialWeight = 2,

    /// <summary>
    /// 馬齢（性別や年齢で重量を決定）
    /// </summary>
    WeightForAge = 3,

    /// <summary>
    /// 定量（別定、かつ性別や年齢で重量を決定）
    /// </summary>
    SpecialWeightForAge = 4,
  }

  public enum RaceHorseAreaRule : short
  {
    Unknown = 0,

    /// <summary>
    /// 混合（A）
    /// </summary>
    Mixed = 1,

    /// <summary>
    /// 父（B）
    /// </summary>
    Father = 2,

    /// <summary>
    /// 市（C）
    /// </summary>
    Market = 3,

    /// <summary>
    /// 抽（D）
    /// </summary>
    Lottery = 4,

    /// <summary>
    /// 「抽」（E）
    /// </summary>
    Lottery2 = 5,

    /// <summary>
    /// 市抽（F）
    /// </summary>
    MarketLottery = 6,

    /// <summary>
    /// 抽・関西（G）
    /// </summary>
    LotteryWest = 7,

    /// <summary>
    /// 抽・関東（H）
    /// </summary>
    LotteryEast = 8,

    /// <summary>
    /// 「抽」・関西（I）
    /// </summary>
    Lottery2West = 9,

    /// <summary>
    /// 「抽」・関東（J）
    /// </summary>
    Lottery2East = 10,

    MarketLotteryWest = 11,

    MarketLotteryEast = 12,

    Kyushu = 13,

    International = 14,

    O = 15,

    /// <summary>
    /// 兵庫（一部に佐賀などを含む）リミテッド
    /// </summary>
    LimitedHyogo = 16,

    Q = 17,
    R = 18,
    S = 19,
    T = 20,
    U = 21,
    V = 22,
    W = 23,

    /// <summary>
    /// JRA認定競走（X）地方競馬のみに設定
    /// </summary>
    JraCertificated = 24,

    /// <summary>
    /// 南関東リミテッド
    /// </summary>
    LimitedSouthKanto = 25,

    Z = 26,
  }

  public enum RaceHorseSexRule : short
  {
    Unknown = 0,

    Male = 1,
    Female = 2,
    MaleCastrated = 3,
    MaleFemale = 4,

    A = 11,
    B = 12,
  }

  public enum RaceCrossRaceRule : short
  {
    Unknown = 0,

    /// <summary>
    /// 指定
    /// </summary>
    Specificated = 1,

    /// <summary>
    /// 見習い騎手
    /// </summary>
    BeginnerRider = 2,

    /// <summary>
    /// 「指定」
    /// </summary>
    Specificated2 = 3,

    /// <summary>
    /// 特別指定
    /// </summary>
    Special = 4,
  }
}
