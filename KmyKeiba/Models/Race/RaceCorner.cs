using KmyKeiba.Models.Image;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race
{
  /// <summary>
  /// 馬のコーナー順位を管理するクラス
  /// </summary>
  public sealed class RaceCorner
  {
    public IReadOnlyList<Group> Groups { get; init; } = Array.Empty<Group>();

    public RaceHorsePassingOrderImage Image { get; } = new();

    public int Number { get; set; }

    public int Position { get; set; }

    public TimeSpan LapTime { get; set; }

    private RaceCorner()
    {
    }

    public static RaceCorner FromString(string orderString)
    {
      var instance = new Builder(orderString).Build();
      instance.Image.Order = instance;
      return instance;
    }

    public sealed class Group
    {
      public AheadSpaceType AheadSpace { get; init; }

      public IReadOnlyList<int> HorseNumbers { get; init; } = Array.Empty<int>();

      public int TopHorseNumber { get; init; }

      public enum AheadSpaceType
      {
        None,
        Large,
        Small,

        /// <summary>
        /// このコーナーを通過しなかった
        /// </summary>
        Retired,
      }
    }

    private class MutableGroup
    {
      public Group.AheadSpaceType AheadSpace { get; set; }

      public IReadOnlyList<int> HorseNumbers { get; set; } = Array.Empty<int>();

      public int TopHorseNumber { get; set; }

      public Group ToImmutable()
      {
        return new Group
        {
          AheadSpace = this.AheadSpace,
          HorseNumbers = this.HorseNumbers,
          TopHorseNumber = this.TopHorseNumber,
        };
      }
    }

    private class Builder
    {
      private readonly string text;

      public Builder(string text)
      {
        this.text = text;
      }

      public RaceCorner Build()
      {
        var tokens = this.Parse(text.Trim());
        var groups = this.GetGroups(tokens);

        return new()
        {
          Groups = groups,
        };
      }

      private List<string> Parse(string text)
      {
        var list = new List<string>();

        var currentNumText = string.Empty;
        var currentSpaceCount = 0;
        var isAfterAsterisk = false;

        foreach (var c in text)
        {
          if (c >= '0' && c <= '9')
          {
            currentNumText += c;
            currentSpaceCount = 0;
          }
          else
          {
            currentSpaceCount = 0;

            // 数字を処理
            if (!string.IsNullOrEmpty(currentNumText))
            {
              if (isAfterAsterisk)
              {
                currentNumText = '*' + currentNumText;
              }
              list.Add(currentNumText);
              currentNumText = string.Empty;
            }
            isAfterAsterisk = false;

            if (c == ' ')
            {
              currentSpaceCount++;
              isAfterAsterisk = false;

              if (currentSpaceCount >= 3)
              {
                list.Add("   ");
                currentSpaceCount = 0;
              }
            }
            else if (c == '*')
            {
              isAfterAsterisk = true;
            }
            else
            {
              list.Add(c.ToString());
            }
          }
        }

        if (!string.IsNullOrEmpty(currentNumText))
        {
          list.Add(currentNumText);
        }

        return list;
      }

      private List<Group> GetGroups(List<string> tokens)
      {
        var groups = new List<Group>();
        var isInGroup = false;
        var currentGroupHorses = new List<int>();
        var currentGroup = new MutableGroup();
        currentGroup.HorseNumbers = currentGroupHorses;

        foreach (var token in tokens)
        {
          if (string.IsNullOrEmpty(token))
          {
            continue;
          }

          void UpdateGroup()
          {
            groups.Add(currentGroup.ToImmutable());

            currentGroup = new();
            currentGroupHorses = new();
            currentGroup.HorseNumbers = currentGroupHorses;
          }

          void AddHorseNumber(int num, bool isAsta = false)
          {
            currentGroupHorses.Add(num);
            if (!isInGroup)
            {
              UpdateGroup();
            }
            else
            {
              if (isAsta)
              {
                currentGroup.TopHorseNumber = num;
              }
            }
          }
          if (int.TryParse(token, out var num))
          {
            AddHorseNumber(num);
          }
          else if (token[0] == '*' && token.Length > 1 && int.TryParse(token.AsSpan(1), out var num2))
          {
            AddHorseNumber(num2, true);
          }

          else if (token == "(")
          {
            isInGroup = true;
          }
          else if (token == ")")
          {
            isInGroup = false;
            UpdateGroup();
          }
          else if (token == "=")
          {
            currentGroup.AheadSpace = Group.AheadSpaceType.Large;
          }
          else if (token == "-")
          {
            currentGroup.AheadSpace = Group.AheadSpaceType.Small;
          }
          else if (token == "   ")
          {
            currentGroup.AheadSpace = Group.AheadSpaceType.Retired;
          }
        }

        return groups;
      }
    }
  }
}
