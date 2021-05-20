using KmyKeiba.JVLink.Wrappers;
using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  public class Race
  {
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// レースの名前
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// レースの副題
    /// </summary>
    public string SubName { get; set; } = string.Empty;

    /// <summary>
    /// 競馬場
    /// </summary>
    public RaceCourse Course { get; set; }

    /// <summary>
    /// 競馬場の名前
    /// </summary>
    public string CourseName => this._courseName ??= this.Course.GetAttribute()?.Name ?? string.Empty;
    private string? _courseName = null;

    /// <summary>
    /// 競馬場内のコース番号
    /// </summary>
    public int CourseRaceNumber { get; set; }

    /// <summary>
    /// 参加条件
    /// </summary>
    public RaceSubject Subject { get; set; } = new();

    /// <summary>
    /// 馬の数
    /// </summary>
    public int HorsesCount { get; set; }

    /// <summary>
    /// 出走日時
    /// </summary>
    public DateTime StartTime { get; set; }

    internal Race()
    {
    }

    internal static Race FromJV(JVData_Struct.JV_RA_RACE race)
    {
      var name = race.RaceInfo.Hondai.Trim();
      if (string.IsNullOrEmpty(name))
      {
        name = race.JyokenName.Trim();
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

      int.TryParse(race.id.RaceNum, out int courseRaceNum);
      int.TryParse(race.TorokuTosu, out int horsesCount);

      var obj = new Race
      {
        Key = race.id.ToRaceKey(),
        Name = name,
        SubName = race.RaceInfo.Fukudai.Trim(),
        Course = course,
        CourseRaceNumber = courseRaceNum,
        Subject = RaceSubject.Parse(race.JyokenName.Trim()),
        HorsesCount = horsesCount,
        StartTime = startTime,
      };
      return obj;
    }

    public override int GetHashCode() => this.Key.GetHashCode();
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

  public enum RaceCourseType
  {
    Unknown = 0,
    Central = 1,
    Local = 2,
    Foreign = 3,
  }

  public enum RaceCourse
  {
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

    /*
エクアドル
ギリシャ
マレーシア
メキシコ
モロッコ
パキスタン
ポーランド
パラグアイ
サウジアラビア
キプロス
タイ
ウクライナ
ベネズエラ
ユーゴスラビア
デンマーク
シンガポール
マカオ
オーストリア
ヨルダン
カタール
東ドイツ
バーレーン
    */

  }
}
