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
    /// 参加条件
    /// </summary>
    public RaceSubject Subject { get; set; } = new();

    /// <summary>
    /// 馬の数
    /// </summary>
    public short HorsesCount { get; set; }

    /// <summary>
    /// 出走日時
    /// </summary>
    public DateTime StartTime { get; set; }

    public string Corner1Result { get; set; } = string.Empty;

    public short Corner1Position { get; set; }

    public short Corner1Number { get; set; }

    public TimeSpan Corner1LapTime { get; set; }

    public string Corner2Result { get; set; } = string.Empty;

    public short Corner2Position { get; set; }

    public short Corner2Number { get; set; }

    public TimeSpan Corner2LapTime { get; set; }

    public string Corner3Result { get; set; } = string.Empty;

    public short Corner3Position { get; set; }

    public short Corner3Number { get; set; }

    public TimeSpan Corner3LapTime { get; set; }

    public string Corner4Result { get; set; } = string.Empty;

    public short Corner4Position { get; set; }

    public short Corner4Number { get; set; }

    public TimeSpan Corner4LapTime { get; set; }

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
      short.TryParse(race.Kyori, out short distance);

      int.TryParse(race.TrackCD, out int track);
      var (trackType, trackGround, trackCornerDirection, trackOption) = GetTrackType(track);

      int.TryParse(race.TenkoBaba.TenkoCD, out int weather);

      // 地方競馬DATAは、盛岡の芝もダートとして配信し、SibaBabaCDには何も設定しないみたい
      int.TryParse((trackGround == TrackGround.Turf && course <= RaceCourse.CentralMaxValue) ? race.TenkoBaba.SibaBabaCD : race.TenkoBaba.DirtBabaCD, out int condition);

      short.TryParse(race.CornerInfo[0].Syukaisu, out var cnum1);
      short.TryParse(race.CornerInfo[0].Corner, out var cn1);
      short.TryParse(race.CornerInfo[1].Syukaisu, out var cnum2);
      short.TryParse(race.CornerInfo[1].Corner, out var cn2);
      short.TryParse(race.CornerInfo[2].Syukaisu, out var cnum3);
      short.TryParse(race.CornerInfo[2].Corner, out var cn3);
      short.TryParse(race.CornerInfo[3].Syukaisu, out var cnum4);
      short.TryParse(race.CornerInfo[3].Corner, out var cn4);

      int.TryParse(race.LapTime[0], out var lap1);
      int.TryParse(race.LapTime[1], out var lap2);
      int.TryParse(race.LapTime[2], out var lap3);
      int.TryParse(race.LapTime[3], out var lap4);

      var obj = new Race
      {
        LastModified = race.head.MakeDate.ToDateTime(),
        Key = race.id.ToRaceKey(),
        DataStatus = race.head.DataKubun.ToDataStatus(),
        Name = name,
        Name6Chars = name6,
        SubName = race.RaceInfo.Fukudai.Trim(),
        GradeId = gradeId,
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
        Subject = subject,
        HorsesCount = horsesCount,
        StartTime = startTime,
        Corner1Position = cn1,
        Corner1Number = cnum1,
        Corner1Result = race.CornerInfo[0].Jyuni.Trim(),
        Corner1LapTime = new TimeSpan(0, 0, 0, lap1 / 10, lap1 % 10 * 100),
        Corner2Position = cn2,
        Corner2Number = cnum2,
        Corner2Result = race.CornerInfo[1].Jyuni.Trim(),
        Corner2LapTime = new TimeSpan(0, 0, 0, lap2 / 10, lap1 % 10 * 100),
        Corner3Position = cn3,
        Corner3Number = cnum3,
        Corner3Result = race.CornerInfo[2].Jyuni.Trim(),
        Corner3LapTime = new TimeSpan(0, 0, 0, lap3 / 10, lap1 % 10 * 100),
        Corner4Position = cn4,
        Corner4Number = cnum4,
        Corner4Result = race.CornerInfo[3].Jyuni.Trim(),
        Corner4LapTime = new TimeSpan(0, 0, 0, lap4 / 10, lap1 % 10 * 100),
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

    [RaceCourseInfo(RaceCourseType.Local, "帯広(ば)")]
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
}
