using KmyKeiba.Data.Wrappers;
using KmyKeiba.JVLink.Wrappers;
using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  public class RaceSubject
  {
    public string Name { get; init; } = string.Empty;

    public string RaceName { get; init; } = string.Empty;

    public int Money { get; private set; }

    public RaceMoneySubject MoneySubject { get; private set; }

    public bool IsNewHorses { get; private set; }

    public bool IsNotWon { get; private set; }

    public bool IsOpen { get; private set; }

    public int Age { get; private set; }

    public bool IsLocal { get; set; }

    public RaceGrade Grade { get; set; }

    public List<SubjectTypeItem> AgeSubjects { get; } = new();

    public struct SubjectTypeItem
    {
      public int Age { get; init; }

      public RaceSubjectType Type { get; init; }
    }

    public RaceSubjectType NotAgeSubjectType { get; set; }

    public RaceClass MaxClass =>
      this.Items.Any() ? this.Items.Max((i) => i.Class) :
      this.Money > 0 ? RaceClass.Money :
      this.Age > 0 ? RaceClass.Age :
      RaceClass.Unknown;

    public object DisplayClass =>
      this.Grade != RaceGrade.Unknown && this.Grade != RaceGrade.Others ? this.Grade :
      this.AgeSubjects.Any() ? RaceClass.Age :
      this.MaxClass != RaceClass.Unknown ? this.MaxClass : RaceSubjectType.Open;

    public object? SecondaryClass
    {
      get
      {
        var maxClass = this.MaxClass;
        var displayClass = this.DisplayClass;

        if (this.Grade == RaceGrade.Grade1 || this.Grade == RaceGrade.Grade2 || this.Grade == RaceGrade.Grade3 ||
          this.Grade == RaceGrade.LocalGrade1 || this.Grade == RaceGrade.LocalGrade2 || this.Grade == RaceGrade.LocalGrade3)
        {
          return null;
        }

        // 地方競馬
        if (maxClass != RaceClass.Unknown || this.IsLocal)
        {
          // 重賞or特別レースでは未勝利や新馬のような情報を表示する余裕がない
          // でも高知がたまに特別レースという名の新馬戦をぶちこんでくるので、特別レースは扱いを分ける
          if (displayClass is RaceGrade &&
            displayClass is not RaceGrade.LocalNonGradeSpecial &&
            displayClass is not RaceGrade.NonGradeSpecial &&
            displayClass is not RaceGrade.Listed &&
            displayClass is not RaceGrade.LocalListed)
          {
            if (maxClass == RaceClass.Age)
            {
              // 年齢制限特別レースで年齢制限の表示ぷっちゃけいらない
              return null;
            }
            return maxClass;
          }

          if (this.IsNotWon)
          {
            return RaceSubjectType.Maiden;
          }
          if (this.IsNewHorses)
          {
            return RaceSubjectType.NewComer;
          }
          if (this.IsOpen)
          {
            return RaceSubjectType.Open;
          }

          // 上の条件分岐で漏れた分（特別レース）
          if (displayClass is RaceGrade)
          {
            if (maxClass == RaceClass.Age)
            {
              return null;
            }
            return maxClass;
          }

          if (displayClass is RaceClass.Age && maxClass == RaceClass.Money)
          {
            return RaceClass.Money;
          }

          return null;
        }
        // 中央競馬
        else
        {
          // 特別レースの条件（中央競馬）
          if ((displayClass is RaceGrade.NonGradeSpecial || displayClass is RaceGrade.Listed || displayClass is RaceClass.Age) && this.AgeSubjects.Any())
          {
            return this.AgeSubjects.Max(s => s.Type);
          }
        }

        return null;
      }
    }
    public RaceClass[] AllClasses => this.Items.Any() ? this.Items.Select(i => i.Class).ToArray() :
      this.Money > 0 ? new[] { RaceClass.Money, } :
      this.Age > 0 ? new[] { RaceClass.Age, } : Array.Empty<RaceClass>();

    public string ClassName
    {
      get
      {
        if (this.DisplayClass is RaceGrade grade)
        {
          return grade.GetLabel();
        }

        if (this.DisplayClass is RaceClass cls)
        {
          if (cls == RaceClass.Age && this.AgeSubjects.Any())
          {
            var age = this.AgeSubjects.Min((s) => s.Age);
            if (age <= 5)
            {
              return age + "歳";
            }
            return "最若";
          }

          if (cls != RaceClass.Age)
          {
            var maxClass = cls;
            var max = this.Items.FirstOrDefault((i) => i.Class == maxClass);
            var className = max?.Class.GetAttribute();

            if (className != null)
            {
              if (max != null && max.Level > 0)
              {
                return className.Name + max.Level;
              }
              return className.Name;
            }
          }
        }

        if (this.Grade != RaceGrade.Unknown && this.Grade != RaceGrade.Others &&
          (string.IsNullOrEmpty(this.Name) || this.Grade != RaceGrade.NonGradeSpecial))
        {
          return this.Grade.GetLabel();
        }

        if (!this.Items.Any())
        {
          if (this.Money > 0)
          {
            return "金" + ((double)this.Money / 10000);
          }
          if (this.Age > 0)
          {
            return this.Age + "歳";
          }
          return "OP";
        }

        return "OP";
      }
    }

    public string DisplayName
    {
      get
      {
        if (!string.IsNullOrEmpty(this.Name))
        {
          return this.Name;
        }

        if (this.AgeSubjects.Any())
        {
          var ages = string.Join(',', this.AgeSubjects.Select(s => s.Age + "歳"));
          var subjects = this.AgeSubjects.First().Type.GetLabel();
          return $"{ages} - {subjects}";
        }

        return string.Empty;
      }
    }

    public List<DataItem> Items { get; } = new();

    public class DataItem
    {
      public RaceClass Class { get; init; }

      public int Level { get; init; }

      public int Group { get; init; }

      public int Age { get; init; }

      public RaceMoneySubject ClassSubject { get; init; }

      public bool IsDefault =>
        this.Class != RaceClass.Unknown ||
        this.Level != 0 ||
        this.Group != 0 ||
        this.Age != 0 ||
        this.ClassSubject != RaceMoneySubject.Unknown;
    }

    internal RaceSubject()
    {
    }

    public static RaceSubject Parse(string text, string raceName)
    {
      var subject = new RaceSubject
      {
        RaceName = raceName,
        Name = text,
      };

#pragma warning disable CA1416 // プラットフォームの互換性を検証
      text = Microsoft.VisualBasic.Strings.StrConv(text, Microsoft.VisualBasic.VbStrConv.Narrow)!;
      raceName = Microsoft.VisualBasic.Strings.StrConv(raceName, Microsoft.VisualBasic.VbStrConv.Narrow)!;
#pragma warning restore CA1416 // プラットフォームの互換性を検証

      static RaceClass StringToClass(string text)
      {
        return text switch
        {
          "A" => RaceClass.ClassA,
          "B" => RaceClass.ClassB,
          "C" => RaceClass.ClassC,
          "D" => RaceClass.ClassD,
          _ => RaceClass.Unknown,
        };
      }

      var reg = new Regex(@"(((?<age>\d)歳\s*)*)(?<class>[A-Z])-*(?<number>\d*)\s*-*(?<group>[一二三四五六七八九\d]*)\s*((?<classsub>以上|未満|以下)*)");
      var match = reg.Match(text);
      for (; match.Success; match = match.NextMatch())
      {
        var cls = StringToClass(match.Groups["class"].Value);

        var grp = match.Groups["group"].Value switch
        {
          "一" => 1,
          "二" => 2,
          "三" => 3,
          "四" => 4,
          "五" => 5,
          "六" => 6,
          "七" => 7,
          "八" => 8,
          "九" => 9,
          _ => 0,
        };
        if (grp == 0 && int.TryParse(match.Groups["group"].Value, out int grpNum))
        {
          grp = grpNum;
        }

        int.TryParse(match.Groups["number"].Value, out int num);
        int.TryParse(match.Groups["age"].Value, out int age);

        var classSubject = match.Groups["classsub"].Value switch
        {
          "以上" => RaceMoneySubject.More,
          "以下" => RaceMoneySubject.Less,
          "未満" => RaceMoneySubject.LessThan,
          _ => RaceMoneySubject.Unknown,
        };

        var item = new DataItem
        {
          Class = cls,
          Level = num,
          Group = grp,
          Age = age,
          ClassSubject = classSubject,
        };
        if (item.IsDefault)
        {
          subject.Items.Add(item);
        }
      }

      reg = new Regex(@"(?<money>[\d\.]+)万((?<moneysub>以上|未満|以下)*)");
      match = reg.Match(text);
      if (match.Success)
      {
        if (match.Groups["moneysub"].Value == "以上")
        {
          // 「以上」としか書かれてないことがある（大井など）
          var nextMatch = match.NextMatch();
          if (nextMatch.Success)
          {
            match = match.NextMatch();
          }
        }

        if (match.Success)
        {
          double.TryParse(match.Groups["money"].Value, out double money);
          subject.Money = (int)(money * 10000);

          subject.MoneySubject = match.Groups["moneysub"].Value switch
          {
            "以上" => RaceMoneySubject.More,
            "以下" => RaceMoneySubject.Less,
            "未満" => RaceMoneySubject.LessThan,
            _ => RaceMoneySubject.Less,
          };
        }
      }

      bool TryMatchAge(Regex reg, string str)
      {
        match = reg.Match(str);
        if (match.Success)
        {
          int.TryParse(match.Groups["age"].Value, out int age);
          subject!.Age = age;
          return true;
        }
        return false;
      }

      if (text.Contains("新馬") || raceName.Contains("新馬") || text.Contains("初出走") || raceName.Contains("初出走"))
      {
        subject.IsNewHorses = true;

        reg = new Regex(@"(?<age>\d)歳");

        if (!TryMatchAge(reg, text))
        {
          TryMatchAge(reg, raceName);
        }
      }
      else if (text.Contains("未勝利") || text.Contains("認未勝") || raceName.Contains("未勝利") || raceName.Contains("認未勝"))
      {
        subject.IsNotWon = true;

        reg = new Regex(@"(?<age>\d)歳");

        if (!TryMatchAge(reg, text))
        {
          TryMatchAge(reg, raceName);
        }
      }
      else if (text.Contains("OP") || raceName.Contains("OP"))
      {
        subject.IsOpen = true;

        reg = new Regex(@"(?<age>\d)歳");

        if (!TryMatchAge(reg, text))
        {
          TryMatchAge(reg, raceName);
        }
      }
      else if (!subject.Items.Any())
      {
        reg = new Regex(@"(?<age>\d+)歳");

        if (!TryMatchAge(reg, text))
        {
          TryMatchAge(reg, raceName);
        }
      }

      if (!subject.Items.Any())
      {
        // 笠松でこんなタイトルがある
        // マッチングを「?級」と一般化するのは、協賛レースに引っ掛かる可能性もあり避ける
        reg = new Regex(@"(?<class>[A-Z])級(ｻﾊﾞｲﾊﾞﾙ|ｾﾚｸｼｮﾝ)");

        match = reg.Match(raceName);
        if (match.Success)
        {
          var cls = StringToClass(match.Groups["class"].Value);
          subject.Items.Add(new DataItem
          {
            Class = cls,
          });
        }
      }

      return subject;
    }

    internal static RaceSubject FromJV(JVData_Struct.JV_RA_RACE race)
    {
      // Parseメソッドとは二重処理になってないはず
      // ここでは、JVやNVから来たレースデータをEnumに直してる程度

      var subject = Parse(race.JyokenName.Trim(), race.RaceInfo.Hondai.Trim());

      var grade = Enum
        .GetValues<RaceGrade>()
        .Select((v) => new { Value = v, Attribute = v.GetAttribute(), })
        .FirstOrDefault((a) => race.GradeCD == a?.Attribute?.Code);
      subject.Grade = grade?.Value ?? RaceGrade.Unknown;

      var startTime = DateTime.ParseExact($"{race.id.Year}{race.id.MonthDay}{race.HassoTime}", "yyyyMMddHHmm", null);

      void AddSubjectType(int index)
      {
        if (int.TryParse(race.JyokenInfo.JyokenCD[index], out int codeNum))
        {
          var age = index + 2;
          var code = (RaceSubjectType)codeNum;

          if (code == RaceSubjectType.Win1 || code == RaceSubjectType.Win2 || code == RaceSubjectType.Win3)
          {
            if (startTime.Year < 2006 || (startTime.Year == 2006 && startTime.Month < 6))
            {
              code = (RaceSubjectType)(codeNum + 1000);
            }
          }

          if (code != RaceSubjectType.Unknown)
          {
            subject!.AgeSubjects.Add(new SubjectTypeItem
            {
              Age = age,
              Type = code,
            });
          }
        }
      }

      AddSubjectType(0);
      AddSubjectType(1);
      AddSubjectType(2);
      AddSubjectType(3);
      AddSubjectType(4);

      return subject;
    }

    public override string ToString()
    {
      if (!string.IsNullOrWhiteSpace(this.Name))
      {
        return this.Name;
      }

      if (this.Grade != RaceGrade.Unknown && this.Grade != RaceGrade.Others)
      {
        return this.Grade.GetLabel();
      }

      if (this.AgeSubjects.Any())
      {
        var text = string.Empty;
        foreach (var grp in this.AgeSubjects.GroupBy((s) => s.Type))
        {
          var minAge = grp.Min((i) => i.Age);
          var maxAge = grp.Max((i) => i.Age);

          string age = string.Empty;
          if (minAge == maxAge)
          {
            age = minAge == 5 ? "5歳以上" : minAge == 6 ? "最若" : (minAge + "歳");
          }
          else if (maxAge == 5 || maxAge == 6)
          {
            age = minAge + "歳";
          }
          else
          {
            age = minAge + "歳～" + maxAge + "歳";
          }

          var type = grp.Key.GetLabel();
          text += $"{age}{type}　";
        }
        return text;
      }

      return base.ToString() ?? string.Empty;
    }
  }

  public class RaceClassInfoAttribute : Attribute
  {
    public string Name { get; }

    public RaceClassInfoAttribute(string name)
    {
      this.Name = name;
    }
  }

  public enum RaceClass
  {
    Unknown,

    [RaceClassInfo("A")]
    ClassA = 999,

    [RaceClassInfo("B")]
    ClassB = 998,

    [RaceClassInfo("C")]
    ClassC = 997,

    [RaceClassInfo("D")]
    ClassD = 996,

    Money = 10,

    Age = 20,
  }

  public enum RaceMoneySubject
  {
    Unknown,

    /// <summary>
    /// 以上
    /// </summary>
    More,

    /// <summary>
    /// 以下
    /// </summary>
    Less,

    /// <summary>
    /// 未満
    /// </summary>
    LessThan,
  }

  class RaceGradeInfoAttribute : Attribute
  {
    public string Code { get; }

    public string Label { get; }

    public RaceGradeType Type { get; }

    public RaceGradeInfoAttribute(string code, RaceGradeType type, string label)
    {
      this.Code = code;
      this.Type = type;
      this.Label = label;
    }
  }

  public enum RaceGradeType
  {
    Unknown = 0,
    Grade = 1,
    NoNamedGrade = 2,
    NonGradeSpecial = 3,
    Steeplechase = 4,
    Listed = 5,
    Others = 6,
  }

  public enum RaceGrade
  {
    [RaceGradeInfo("", RaceGradeType.Unknown, "不明")]
    Unknown = 0,

    [RaceGradeInfo("A", RaceGradeType.Grade, "G1")]
    Grade1 = 1,

    [RaceGradeInfo("B", RaceGradeType.Grade, "G2")]
    Grade2 = 2,

    [RaceGradeInfo("C", RaceGradeType.Grade, "G3")]
    Grade3 = 3,

    [RaceGradeInfo("D", RaceGradeType.NoNamedGrade, "G")]
    NoNamedGrade = 4,

    [RaceGradeInfo("E", RaceGradeType.NonGradeSpecial, "特別")]
    NonGradeSpecial = 5,

    [RaceGradeInfo("F", RaceGradeType.Steeplechase, "Jpn1")]
    Steeplechase1 = 6,

    [RaceGradeInfo("G", RaceGradeType.Steeplechase, "Jpn2")]
    Steeplechase2 = 7,

    [RaceGradeInfo("H", RaceGradeType.Steeplechase, "Jpn3")]
    Steeplechase3 = 8,

    [RaceGradeInfo("L", RaceGradeType.Listed, "L")]
    Listed = 9,

    [RaceGradeInfo(" ", RaceGradeType.Others, "")]
    Others = 10,

    [RaceGradeInfo("Jpn1", RaceGradeType.Grade, "Jpn1")]
    LocalGrade1 = 101,

    [RaceGradeInfo("Jpn2", RaceGradeType.Grade, "Jpn2")]
    LocalGrade2 = 102,

    [RaceGradeInfo("Jpn3", RaceGradeType.Grade, "Jpn3")]
    LocalGrade3 = 103,

    [RaceGradeInfo("JpnD", RaceGradeType.NoNamedGrade, "G")]
    LocalNoNamedGrade = 104,

    [RaceGradeInfo("JpnE", RaceGradeType.NonGradeSpecial, "特別")]
    LocalNonGradeSpecial = 105,

    [RaceGradeInfo("JpnL", RaceGradeType.Listed, "L")]
    LocalListed = 109,
  }

  class RaceSubjectTypeInfoAttribute : Attribute
  {
    public string Label { get; }

    public RaceSubjectKind Kind { get; }

    public int Money { get; }

    public int Win { get; }

    public RaceSubjectTypeInfoAttribute(string label, RaceSubjectKind kind, int money = 0)
    {
      this.Label = label;
      this.Kind = kind;
      if (kind == RaceSubjectKind.MoneyLess)
      {
        this.Money = money;
      }
      else if (kind == RaceSubjectKind.Win)
      {
        this.Win = money;
      }
    }
  }

  public enum RaceSubjectKind
  {
    Unknown = 0,
    MoneyLess = 1,
    Win = 2,
    NewComer = 3,
    Unraced = 4,
    Maiden = 5,
    Open = 6,
  }

  public enum RaceSubjectType
  {
    [RaceSubjectTypeInfo("不明", RaceSubjectKind.Unknown)]
    Unknown = 0,

    [RaceSubjectTypeInfo("100万以下", RaceSubjectKind.MoneyLess, 100)]
    MoneyLess100 = 1,

    [RaceSubjectTypeInfo("200万以下", RaceSubjectKind.MoneyLess, 200)]
    MoneyLess200 = 2,

    [RaceSubjectTypeInfo("300万以下", RaceSubjectKind.MoneyLess, 300)]
    MoneyLess300 = 3,

    [RaceSubjectTypeInfo("500万以下", RaceSubjectKind.MoneyLess, 500)]
    MoneyLess500 = 1005,

    [RaceSubjectTypeInfo("1000万以下", RaceSubjectKind.MoneyLess, 1000)]
    MoneyLess1000 = 1010,

    [RaceSubjectTypeInfo("1600万以下", RaceSubjectKind.MoneyLess, 1600)]
    MoneyLess1600 = 1016,

    [RaceSubjectTypeInfo("1勝クラス", RaceSubjectKind.Win, 1)]
    Win1 = 5,

    [RaceSubjectTypeInfo("2勝クラス", RaceSubjectKind.Win, 2)]
    Win2 = 10,

    [RaceSubjectTypeInfo("3勝クラス", RaceSubjectKind.Win, 3)]
    Win3 = 16,

    [RaceSubjectTypeInfo("9900万以下", RaceSubjectKind.MoneyLess, 9900)]
    MoneyLess9900 = 99,

    [RaceSubjectTypeInfo("1億以下", RaceSubjectKind.MoneyLess, 10000)]
    MoneyLess10000 = 100,

    [RaceSubjectTypeInfo("新馬", RaceSubjectKind.NewComer)]
    NewComer = 701,

    [RaceSubjectTypeInfo("未出走", RaceSubjectKind.Unraced)]
    Unraced = 702,

    [RaceSubjectTypeInfo("未勝利", RaceSubjectKind.Maiden)]
    Maiden = 703,

    [RaceSubjectTypeInfo("オープン", RaceSubjectKind.Open)]
    Open = 999,
  }
}
