using KmyKeiba.JVLink.Wrappers;
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

    public int Money { get; private set; }

    public RaceMoneySubject MoneySubject { get; private set; }

    public bool IsNewHorses { get; private set; }

    public bool IsNotWon { get; private set; }

    public int Age { get; private set; }

    public RaceClass MaxClass =>
      this.Items.Any() ? this.Items.Max((i) => i.Class) :
      this.Money > 0 ? RaceClass.Money :
      this.Age > 0 ? RaceClass.Age :
      RaceClass.Unknown;

    public string ClassName
    {
      get
      {
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
          return string.Empty;
        }

        var maxClass = this.MaxClass;
        var max = this.Items.First((i) => i.Class == maxClass);
        var className = max.Class.GetAttribute();

        if (className != null)
        {
          if (max.Level > 0)
          {
            return className.Name + max.Level;
          }
          return className.Name;
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
    }

    internal RaceSubject()
    {
    }

    public static RaceSubject Parse(string text)
    {
      var subject = new RaceSubject
      {
        Name = text,
      };

#pragma warning disable CA1416 // プラットフォームの互換性を検証
      text = Microsoft.VisualBasic.Strings.StrConv(text, Microsoft.VisualBasic.VbStrConv.Narrow)!;
#pragma warning restore CA1416 // プラットフォームの互換性を検証

      var reg = new Regex(@"(((?<age>\d)歳\s*)*)(?<class>[A-Z])-*(?<number>\d*)\s*-*(?<group>[一二三四五六七八九\d]*)\s*((?<classsub>以上|未満|以下)*)");
      var match = reg.Match(text);
      for (; match.Success; match = match.NextMatch())
      {
        var cls = match.Groups["class"].Value switch
        {
          "A" => RaceClass.ClassA,
          "B" => RaceClass.ClassB,
          "C" => RaceClass.ClassC,
          "D" => RaceClass.ClassD,
          _ => RaceClass.Unknown,
        };

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
        subject.Items.Add(item);
      }

      reg = new Regex(@"(?<money>[\d\.]+)万((?<moneysub>以上|未満|以下)*)");
      match = reg.Match(text);
      if (match.Success)
      {
        if (match.Groups["moneysub"].Value == "以上")
        {
          match = match.NextMatch();
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

      if (text.Contains("新馬"))
      {
        subject.IsNewHorses = true;

        reg = new Regex(@"(?<age>\d)歳");
        match = reg.Match(text);
        if (match.Success)
        {
          int.TryParse(match.Groups["age"].Value, out int age);
          subject.Age = age;
        }
      }
      else if (text.Contains("未勝利") || text.Contains("認未勝"))
      {
        subject.IsNotWon = true;

        reg = new Regex(@"(?<age>\d)歳");
        match = reg.Match(text);
        if (match.Success)
        {
          int.TryParse(match.Groups["age"].Value, out int age);
          subject.Age = age;
        }
      }
      else if (!subject.Items.Any())
      {
        reg = new Regex(@"(?<age>\d)歳");
        match = reg.Match(text);
        if (match.Success)
        {
          int.TryParse(match.Groups["age"].Value, out int age);
          subject.Age = age;
        }
      }

      return subject;
    }
  }

  class RaceClassInfoAttribute : Attribute
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
}
