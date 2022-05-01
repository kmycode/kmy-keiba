using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Prompt.Models.Brains
{
  public class BuyerScriptData
  {
    public List<BuyCandidate> Data { get; set; } = new();

    public bool IsLargeDiff { get; set; }

    public bool IsLargeDiffAll { get; set; }
  }

  class Buyer
  {
    private static Script<object>? script;

    public static void UpdateScript()
    {
      try
      {
        var text = File.ReadAllText("scripts/buyer.txt");
        var options = ScriptOptions.Default.AddReferences(Assembly.GetAssembly(typeof(Buyer)));
        script = CSharpScript.Create(text, options, typeof(BuyerScriptData));
      }
      catch (Exception ex)
      {
        throw new Exception("", ex);
      }
    }

    public static IEnumerable<BuyItem> Select(IEnumerable<BuyCandidate> data, bool isLargeDiff = false, bool isLargeDiffAll = false)
    {
      try
      {
        if (script == null)
        {
          UpdateScript();
        }

        var result = script!.RunAsync(new BuyerScriptData { Data = data.ToList(), IsLargeDiff = isLargeDiff, IsLargeDiffAll = isLargeDiffAll, }).Result.ReturnValue;

        if (result is IEnumerable<BuyItem> items)
        {
          return items;
        }
      }
      catch (Exception ex)
      {
        throw new Exception("", ex);
      }

      var count = data.Count() <= 7 ? 2 : 3;
      return data
        .OrderByDescending((d) => d.Prediction)
        .Take(count)
        .Select((d) => new BuyItem
        {
          Number1 = d.Horse.Number,
          Type = BuyType.Place,
        });
    }

    public static (int Pay, int Income) Buy(RefundData refund, IEnumerable<BuyItem> items)
    {
      var pay = items.Sum((i) => i.Unit) * 100;
      var income = 0;

      void Calc1(BuyItem item, int[,] numbers)
      {
        for (var i = 0; i < numbers.GetLength(0); i++)
        {
          if (numbers[i, 0] == item.Number1)
          {
            item.Income += numbers[i, 1] * item.Unit;
            income += item.Income;
          }
        }
      }

      void Calc2(BuyItem item, int[,] numbers)
      {
        for (var i = 0; i < numbers.GetLength(0); i++)
        {
          if ((numbers[i, 0] == item.Number1 || numbers[i, 0] == item.Number2) &&
            (numbers[i, 1] == item.Number1 || numbers[i, 1] == item.Number2))
          {
            if (item.Income > 0)
            {

            }
            item.Income += numbers[i, 2] * item.Unit;
            income += item.Income;
          }
        }
      }

      void Calc2Ordered(BuyItem item, int[,] numbers)
      {
        for (var i = 0; i < numbers.GetLength(0); i++)
        {
          if (numbers[i, 0] == item.Number1 && numbers[i, 1] == item.Number2)
          {
            item.Income += numbers[i, 2] * item.Unit;
            income += item.Income;
          }
        }
      }

      void Calc2Wide(BuyItem item, int[,] numbers)
      {
        for (var i = 0; i < numbers.GetLength(0); i++)
        {
          var c = 0;
          if (numbers[i, 0] == item.Number1 || numbers[i, 0] == item.Number2)
          {
            c++;
          }
          if (numbers[i, 1] == item.Number1 || numbers[i, 1] == item.Number2)
          {
            c++;
          }
          if (numbers[i, 2] == item.Number1 || numbers[i, 2] == item.Number2)
          {
            c++;
          }
          if (c >= 2)
          {
            item.Income += numbers[i, 2] * item.Unit;
            income += item.Income;
          }
        }
      }

      void Calc3(BuyItem item, int[,] numbers)
      {
        for (var i = 0; i < numbers.GetLength(0); i++)
        {
          if ((numbers[i, 0] == item.Number1 || numbers[i, 0] == item.Number2 || numbers[i, 0] == item.Number3) &&
            (numbers[i, 1] == item.Number1 || numbers[i, 1] == item.Number2 || numbers[i, 1] == item.Number3) &&
            (numbers[i, 2] == item.Number1 || numbers[i, 2] == item.Number2 || numbers[i, 2] == item.Number3))
          {
            if (item.Income > 0)
            {

            }
            item.Income += numbers[i, 3] * item.Unit;
            income += item.Income;
          }
        }
      }

      void Calc3Ordered(BuyItem item, int[,] numbers)
      {
        for (var i = 0; i < numbers.GetLength(0); i++)
        {
          if (numbers[i, 0] == item.Number1 && numbers[i, 1] == item.Number2 && numbers[i, 2] == item.Number3)
          {
            item.Income += numbers[i, 3] * item.Unit;
            income += item.Income;
          }
        }
      }

      foreach (var item in items)
      {
        switch (item.Type)
        {
          case BuyType.Single:
            {
              var numbers = new int[,]
              {
                { refund.SingleNumber1, refund.SingleNumber1Money, },
                { refund.SingleNumber2, refund.SingleNumber2Money, },
                { refund.SingleNumber3, refund.SingleNumber3Money, },
              };
              Calc1(item, numbers);
            }
            break;
          case BuyType.Place:
            {
              var numbers = new int[,] {
                { refund.PlaceNumber1, refund.PlaceNumber1Money, },
                { refund.PlaceNumber2, refund.PlaceNumber2Money, },
                { refund.PlaceNumber3, refund.PlaceNumber3Money, },
                { refund.PlaceNumber4, refund.PlaceNumber4Money, },
                { refund.PlaceNumber5, refund.PlaceNumber5Money, },
              };
              Calc1(item, numbers);
            }
            break;
          case BuyType.Frame:
            {
              var numbers = new int[,]
              {
                { refund.Frame1Number1, refund.Frame2Number1, refund.FrameNumber1Money, },
                { refund.Frame1Number2, refund.Frame2Number2, refund.FrameNumber2Money, },
                { refund.Frame1Number3, refund.Frame2Number3, refund.FrameNumber3Money, },
              };
              Calc2(item, numbers);
            }
            break;
          case BuyType.Exacta:
            {
              var numbers = new int[,] {
                { refund.Exacta1Number1, refund.Exacta2Number1, refund.ExactaNumber1Money, },
                { refund.Exacta1Number2, refund.Exacta2Number2, refund.ExactaNumber2Money, },
                { refund.Exacta1Number3, refund.Exacta2Number3, refund.ExactaNumber3Money, },
              };
              Calc2Ordered(item, numbers);
            }
            break;
          case BuyType.Quinella:
            {
              var numbers = new int[,] {
                { refund.Quinella1Number1, refund.Quinella2Number1, refund.QuinellaNumber1Money, },
                { refund.Quinella1Number2, refund.Quinella2Number2, refund.QuinellaNumber2Money, },
                { refund.Quinella1Number3, refund.Quinella2Number3, refund.QuinellaNumber3Money, },
              };
              Calc2(item, numbers);
            }
            break;
          case BuyType.QuinellaPlace:
            {
              var numbers = new int[,] {
                { refund.QuinellaPlace1Number1, refund.QuinellaPlace2Number1, refund.QuinellaPlaceNumber1Money, },
                { refund.QuinellaPlace1Number2, refund.QuinellaPlace2Number2, refund.QuinellaPlaceNumber2Money, },
                { refund.QuinellaPlace1Number3, refund.QuinellaPlace2Number3, refund.QuinellaPlaceNumber3Money, },
                { refund.QuinellaPlace1Number4, refund.QuinellaPlace2Number4, refund.QuinellaPlaceNumber4Money, },
                { refund.QuinellaPlace1Number5, refund.QuinellaPlace2Number5, refund.QuinellaPlaceNumber5Money, },
                { refund.QuinellaPlace1Number6, refund.QuinellaPlace2Number6, refund.QuinellaPlaceNumber6Money, },
                { refund.QuinellaPlace1Number7, refund.QuinellaPlace2Number7, refund.QuinellaPlaceNumber7Money, },
              };
              Calc2Wide(item, numbers);
            }
            break;
          case BuyType.Trifecta:
            {
              var numbers = new int[,] {
                { refund.Trifecta1Number1, refund.Trifecta2Number1, refund.Trifecta3Number1, refund.TrifectaNumber1Money, },
                { refund.Trifecta1Number2, refund.Trifecta2Number2, refund.Trifecta3Number2, refund.TrifectaNumber2Money, },
                { refund.Trifecta1Number3, refund.Trifecta2Number3, refund.Trifecta3Number3, refund.TrifectaNumber3Money, },
                { refund.Trifecta1Number4, refund.Trifecta2Number4, refund.Trifecta3Number4, refund.TrifectaNumber4Money, },
                { refund.Trifecta1Number5, refund.Trifecta2Number5, refund.Trifecta3Number5, refund.TrifectaNumber5Money, },
                { refund.Trifecta1Number6, refund.Trifecta2Number6, refund.Trifecta3Number6, refund.TrifectaNumber6Money, },
              };
              Calc3Ordered(item, numbers);
            }
            break;
          case BuyType.Trio:
            {
              var numbers = new int[,] {
                { refund.Trio1Number1, refund.Trio2Number1, refund.Trio3Number1, refund.TrioNumber1Money, },
                { refund.Trio1Number2, refund.Trio2Number2, refund.Trio3Number2, refund.TrioNumber2Money, },
                { refund.Trio1Number3, refund.Trio2Number3, refund.Trio3Number3, refund.TrioNumber3Money, },
              };
              Calc3(item, numbers);
            }
            break;
        }
      }

      return (pay, income);
    }
  }

  public class BuyCandidate
  {
    public RaceHorseData Horse { get; set; } = new RaceHorseData();

    public float Prediction { get; set; }
  }

  public class BuyItem
  {
    public int Unit { get; set; } = 1;

    public BuyType Type { get; set; }

    public int Number1 { get; set; }

    public int Number2 { get; set; }

    public int Number3 { get; set; }

    internal string Text { get; set; } = string.Empty;

    internal int Pay => this.Unit * 100;

    internal int Income { get; set; }

    internal void SetText(string text)
    {
      this.Text = text;
    }

    public static IEnumerable<BuyItem> Box2(BuyType type, int unit, bool isOrdered, params int[] numbers)
    {
      numbers = numbers.Where((n) => n != 0).ToArray();

      if (numbers.Length < 2)
      {
        return Enumerable.Empty<BuyItem>();
      }

      if (numbers.Length > 2)
      {
        var list = new List<BuyItem>();
        for (var x = 0; x < numbers.Length - 1; x++)
        {
          for (var y = x + 1; y < numbers.Length; y++)
          {
            list.AddRange(Box2(type, unit, isOrdered, numbers[x], numbers[y]));
          }
        }
        return list;
      }

      var result = new List<BuyItem>
      {
        new BuyItem
        {
          Unit = unit,
          Type = type,
          Number1 = numbers[0],
          Number2 = numbers[1],
        },
      };
      if (isOrdered)
      {
        result.Add(new BuyItem
        {
          Unit = unit,
          Type = type,
          Number1 = numbers[1],
          Number2 = numbers[0],
        });
      }
      return result;
    }

    public static IEnumerable<BuyItem> Nagashi2(BuyType type, int unit, bool isOrdered, int jiku, params int[] numbers)
    {
      return Box2(type, unit, isOrdered, numbers).Where((i) => i.Number1 == jiku || i.Number2 == jiku);
    }

    public static IEnumerable<BuyItem> Formation2(BuyType type, int unit, bool isOrdered, int[] jiku1, int[] jiku2)
    {
      return Box3(type, unit, isOrdered, jiku1.Concat(jiku2).Distinct().ToArray())
        .Where((i) => jiku1.Contains(i.Number1) || jiku1.Contains(i.Number2))
        .Where((i) => jiku2.Length == 0 || (jiku2.Contains(i.Number1) || jiku2.Contains(i.Number2)));
    }

    public static IEnumerable<BuyItem> Box3(BuyType type, int unit, bool isOrdered, params int[] numbers)
    {
      numbers = numbers.Where((n) => n != 0).ToArray();

      if (numbers.Length < 3)
      {
        return Enumerable.Empty<BuyItem>();
      }

      if (numbers.Length > 3)
      {
        var list = new List<BuyItem>();
        for (var x = 0; x < numbers.Length - 2; x++)
        {
          for (var y = x + 1; y < numbers.Length - 1; y++)
          {
            for (var z = y + 1; z < numbers.Length; z++)
            {
              list.AddRange(Box3(type, unit, isOrdered, numbers[x], numbers[y], numbers[z]));
            }
          }
        }
        return list;
      }

      var result = new List<BuyItem>
      {
        new BuyItem
        {
          Unit = unit,
          Type = type,
          Number1 = numbers[0],
          Number2 = numbers[1],
          Number3 = numbers[2],
        },
      };
      if (isOrdered)
      {
        result.Add(new BuyItem
        {
          Unit = unit,
          Type = type,
          Number1 = numbers[0],
          Number2 = numbers[2],
          Number3 = numbers[1],
        });
        result.Add(new BuyItem
        {
          Unit = unit,
          Type = type,
          Number1 = numbers[1],
          Number2 = numbers[0],
          Number3 = numbers[2],
        });
        result.Add(new BuyItem
        {
          Unit = unit,
          Type = type,
          Number1 = numbers[1],
          Number2 = numbers[2],
          Number3 = numbers[1],
        });
        result.Add(new BuyItem
        {
          Unit = unit,
          Type = type,
          Number1 = numbers[2],
          Number2 = numbers[0],
          Number3 = numbers[1],
        });
        result.Add(new BuyItem
        {
          Unit = unit,
          Type = type,
          Number1 = numbers[2],
          Number2 = numbers[1],
          Number3 = numbers[0],
        });
      }
      return result;
    }

    public static IEnumerable<BuyItem> Nagashi3(BuyType type, int unit, bool isOrdered, int jiku1, int jiku2, params int[] numbers)
    {
      return Box3(type, unit, isOrdered, numbers)
        .Where((i) => i.Number1 == jiku1 || i.Number2 == jiku1 || i.Number3 == jiku1)
        .Where((i) => jiku2 == 0 || (i.Number1 == jiku2 || i.Number2 == jiku2 || i.Number3 == jiku2));
    }

    public static IEnumerable<BuyItem> Formation3(BuyType type, int unit, bool isOrdered, int[] jiku1, int[] jiku2, int[] jiku3)
    {
      return Box3(type, unit, isOrdered, jiku1.Concat(jiku2).Concat(jiku3).Distinct().ToArray())
        .Where((i) => jiku1.Contains(i.Number1) || jiku1.Contains(i.Number2) || jiku1.Contains(i.Number3))
        .Where((i) => jiku2.Length == 0 || (jiku2.Contains(i.Number1) || jiku2.Contains(i.Number2) || jiku2.Contains(i.Number3)))
        .Where((i) => jiku3.Length == 0 || (jiku3.Contains(i.Number1) || jiku3.Contains(i.Number2) || jiku3.Contains(i.Number3)));
    }

    public static IEnumerable<BuyItem> Single(int unit, params int[] numbers)
    {
      foreach (var num in numbers.Where((n) => n != 0))
      {
        yield return new BuyItem
        {
          Unit = unit,
          Type = BuyType.Single,
          Number1 = num,
          Text = $"単勝　{num}",
        };
      }
    }

    public static IEnumerable<BuyItem> Place(int unit, params int[] numbers)
    {
      foreach (var num in numbers.Where((n) => n != 0))
      {
        yield return new BuyItem
        {
          Unit = unit,
          Type = BuyType.Place,
          Number1 = num,
          Text = $"複勝　{num}",
        };
      }
    }

    public static IEnumerable<BuyItem> FrameBox(int unit, params int[] numbers)
    {
      var r = Box2(BuyType.Frame, unit, false, numbers);
      r.FirstOrDefault()?.SetText($"枠連BOX　{string.Join(",", numbers)}");
      return r;
    }

    public static IEnumerable<BuyItem> FrameNagashi(int unit, int jiku, params int[] numbers)
    {
      var r = Nagashi2(BuyType.Frame, unit, false, jiku, numbers);
      r.FirstOrDefault()?.SetText($"枠連流し　軸{jiku}　{string.Join(",", numbers)}");
      return r;
    }

    public static IEnumerable<BuyItem> FrameFormation(int unit, int[] jiku1, int[] jiku2)
    {
      var r = Formation2(BuyType.Frame, unit, false, jiku1, jiku2);
      r.FirstOrDefault()?.SetText($"枠連フォメ　軸{string.Join(",", jiku1)}　軸{string.Join(",", jiku2)}");
      return r;
    }

    public static IEnumerable<BuyItem> Exacta(int unit, int number1, int number2)
    {
      yield return new BuyItem
      {
        Unit = unit,
        Type = BuyType.Exacta,
        Number1 = number1,
        Number2 = number2,
        Text = $"馬単　{number1},{number2}",
      };
    }

    public static IEnumerable<BuyItem> ExactaBox(int unit, params int[] numbers)
    {
      var r = Box2(BuyType.Exacta, unit, true, numbers);
      r.FirstOrDefault()?.SetText($"馬単BOX　{string.Join(",", numbers)}");
      return r;
    }

    public static IEnumerable<BuyItem> ExactaNagashi(int unit, int jiku, params int[] numbers)
    {
      var r = Nagashi2(BuyType.Exacta, unit, true, jiku, numbers);
      r.FirstOrDefault()?.SetText($"馬単流し　軸{jiku}　{string.Join(",", numbers)}");
      return r;
    }

    public static IEnumerable<BuyItem> ExactaFormation(int unit, int[] jiku1, int[] jiku2)
    {
      var r = Formation2(BuyType.Quinella, unit, true, jiku1, jiku2);
      r.FirstOrDefault()?.SetText($"馬単フォメ　軸{string.Join(",", jiku1)}　軸{string.Join(",", jiku2)}");
      return r;
    }

    public static IEnumerable<BuyItem> Quinella(int unit, int number1, int number2)
    {
      yield return new BuyItem
      {
        Unit = unit,
        Type = BuyType.Quinella,
        Number1 = number1,
        Number2 = number2,
        Text = $"馬連　{number1},{number2}",
      };
    }

    public static IEnumerable<BuyItem> QuinellaBox(int unit, params int[] numbers)
    {
      var r = Box2(BuyType.Quinella, unit, false, numbers);
      r.FirstOrDefault()?.SetText($"馬連BOX　{string.Join(",", numbers)}");
      return r;
    }

    public static IEnumerable<BuyItem> QuinellaNagashi(int unit, int jiku, params int[] numbers)
    {
      var r = Nagashi2(BuyType.Quinella, unit, false, jiku, numbers);
      r.FirstOrDefault()?.SetText($"馬連流し　軸{jiku}　{string.Join(",", numbers)}");
      return r;
    }

    public static IEnumerable<BuyItem> QuinellaFormation(int unit, int[] jiku1, int[] jiku2)
    {
      var r = Formation2(BuyType.Quinella, unit, false, jiku1, jiku2);
      r.FirstOrDefault()?.SetText($"馬連フォメ　軸{string.Join(",", jiku1)}　軸{string.Join(",", jiku2)}");
      return r;
    }

    public static IEnumerable<BuyItem> QuinellaPlace(int unit, int number1, int number2)
    {
      yield return new BuyItem
      {
        Unit = unit,
        Type = BuyType.QuinellaPlace,
        Number1 = number1,
        Number2 = number2,
        Text = $"ワイド　{number1},{number2}",
      };
    }

    public static IEnumerable<BuyItem> QuinellaPlaceBox(int unit, params int[] numbers)
    {
      var r = Box2(BuyType.QuinellaPlace, unit, false, numbers);
      r.FirstOrDefault()?.SetText($"ワイドBOX　{string.Join(",", numbers)}");
      return r;
    }

    public static IEnumerable<BuyItem> QuinellaPlaceNagashi(int unit, int jiku1, params int[] numbers)
    {
      var r = Nagashi2(BuyType.QuinellaPlace, unit, false, jiku1, numbers);
      r.FirstOrDefault()?.SetText($"ワイド流し　軸{jiku1}　{string.Join(",", numbers)}");
      return r;
    }

    public static IEnumerable<BuyItem> QuinellaPlaceFormation(int unit, int[] jiku1, int[] jiku2)
    {
      var r = Formation2(BuyType.QuinellaPlace, unit, false, jiku1, jiku2);
      r.FirstOrDefault()?.SetText($"ワイドフォメ　軸{string.Join(",", jiku1)}　軸{string.Join(",", jiku2)}");
      return r;
    }

    public static IEnumerable<BuyItem> Trifecta(int unit, int number1, int number2, int number3)
    {
      yield return new BuyItem
      {
        Unit = unit,
        Type = BuyType.Trifecta,
        Number1 = number1,
        Number2 = number2,
        Number3 = number3,
        Text = $"３連単　{number1},{number2},{number3}",
      };
    }

    public static IEnumerable<BuyItem> TrifectaBox(int unit, params int[] numbers)
    {
      var r = Box3(BuyType.Trifecta, unit, true, numbers);
      r.FirstOrDefault()?.SetText($"３連単BOX　{string.Join(",", numbers)}");
      return r;
    }

    public static IEnumerable<BuyItem> TrifectaNagashi(int unit, int jiku1, int jiku2, params int[] numbers)
    {
      var r = Nagashi3(BuyType.Trifecta, unit, true, jiku1, jiku2, numbers);
      r.FirstOrDefault()?.SetText($"３連単流し　軸{jiku1},{jiku2}　{string.Join(",", numbers)}");
      return r;
    }

    public static IEnumerable<BuyItem> TrifectaFormation(int unit, int[] jiku1, int[] jiku2, int[] jiku3)
    {
      var r = Formation3(BuyType.Trifecta, unit, true, jiku1, jiku2, jiku3);
      r.FirstOrDefault()?.SetText($"３連単フォメ　軸{string.Join(",", jiku1)}　軸{string.Join(",", jiku2)}　軸{string.Join(",", jiku3)}");
      return r;
    }

    public static IEnumerable<BuyItem> Trio(int unit, int number1, int number2, int number3)
    {
      yield return new BuyItem
      {
        Unit = unit,
        Type = BuyType.Trio,
        Number1 = number1,
        Number2 = number2,
        Number3 = number3,
        Text = $"３連複　{number1},{number2},{number3}",
      };
    }

    public static IEnumerable<BuyItem> TrioBox(int unit, params int[] numbers)
    {
      var r = Box3(BuyType.Trio, unit, false, numbers);
      r.FirstOrDefault()?.SetText($"３連複BOX　{string.Join(",", numbers)}");
      return r;
    }

    public static IEnumerable<BuyItem> TrioNagashi(int unit, int jiku1, int jiku2, params int[] numbers)
    {
      var r = Nagashi3(BuyType.Trio, unit, false, jiku1, jiku2, numbers);
      r.FirstOrDefault()?.SetText($"３連複流し　軸{jiku1},{jiku2}　{string.Join(",", numbers)}");
      return r;
    }

    public static IEnumerable<BuyItem> TrioFormation(int unit, int[] jiku1, int[] jiku2, int[] jiku3)
    {
      var r = Formation3(BuyType.Trio, unit, false, jiku1, jiku2, jiku3);
      r.FirstOrDefault()?.SetText($"３連複フォメ　軸{string.Join(",", jiku1)}　軸{string.Join(",", jiku2)}　軸{string.Join(",", jiku3)}");
      return r;
    }
  }

  public enum BuyType
  {
    Single,
    Place,
    Frame,

    /// <summary>
    /// 馬単
    /// </summary>
    Exacta,

    /// <summary>
    /// 馬連
    /// </summary>
    Quinella,

    /// <summary>
    /// ワイド
    /// </summary>
    QuinellaPlace,

    /// <summary>
    /// 3連単
    /// </summary>
    Trifecta,

    /// <summary>
    /// ３連複
    /// </summary>
    Trio,
  }
}
