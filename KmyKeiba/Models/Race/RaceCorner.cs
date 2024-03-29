﻿using KmyKeiba.Data.Db;
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
    public IEnumerable<Group> Groups
    {
      get => this.Image.Groups ?? Array.Empty<Group>();
      set
      {
        if (this.Groups != value)
        {
          this.Image.Groups = value;
        }
      }
    }

    public RaceHorsePassingOrderImage Image { get; } = new();

    public int Number { get; set; }

    public int Position { get; set; }

    private RaceCorner()
    {
    }

    public static RaceCorner FromString(string orderString)
    {
      var instance = new Builder(orderString).Build();
      return instance;
    }

    public static IReadOnlyList<Group> GetGroupListFromResult(IEnumerable<RaceHorseData> horses)
    {
      var groups = new List<Group>();
      var currentNumbers = new List<int>();
      var currentGroup = new MutableGroup
      {
        HorseNumbers = currentNumbers,
      };

      var x = 0;
      var lastGroupX = 0;

      void AddGroup()
      {
        if (groups == null || currentGroup == null || !currentNumbers.Any())
        {
          return;
        }

        if (currentGroup.HorseNumbers.Count() == 1)
        {
          currentGroup.TopHorseNumber = default;
        }

        groups.Add(currentGroup.ToImmutable());
        currentNumbers = new();
        lastGroupX = x;
      }

      foreach (var horse in horses.OrderBy(h => h.ResultOrder))
      {
        x += Math.Max(horse.ResultLength1, Math.Max(horse.ResultLength2, horse.ResultLength3));
        if (x >= lastGroupX + 100)
        {
          var d = x - lastGroupX;

          if (currentNumbers.Any())
          {
            AddGroup();
          }

          currentGroup = new MutableGroup
          {
            AheadSpace = d >= 500 ? Group.AheadSpaceType.Large : d >= 200 ? Group.AheadSpaceType.Small : Group.AheadSpaceType.None,
            TopHorseNumber = horse.Number,
            HorseNumbers = currentNumbers,
          };
        }

        currentNumbers.Add(horse.Number);
        if (currentGroup.TopHorseNumber == default)
        {
          currentGroup.TopHorseNumber = horse.Number;
        }
      }

      AddGroup();
      return groups;
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
