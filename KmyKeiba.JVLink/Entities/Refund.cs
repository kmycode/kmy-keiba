using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  public class Refund : EntityBase
  {
    public string RaceKey { get; set; } = string.Empty;

    public List<Data1> Single { get; } = new();

    public List<Data1> Place { get; } = new();

    public List<DataF> FrameNumber { get; } = new();

    public List<Data2> Quinella { get; } = new();

    public List<Data2> QuinellaPlace { get; } = new();

    public List<Data2> Exacta { get; } = new();

    public List<Data3> Trio { get; } = new();

    public List<Data3> Trifecta { get; } = new();

    public struct Data1
    {
      public short HorseNumber { get; init; }

      public int Money { get; init; }
    }

    public struct DataF
    {
      public short FrameNumber1 { get; init; }
      public short FrameNumber2 { get; init; }

      public int Money { get; init; }
    }

    public struct Data2
    {
      public short HorseNumber1 { get; init; }

      public short HorseNumber2 { get; init; }

      public int Money { get; init; }
    }

    public struct Data3
    {
      public short HorseNumber1 { get; init; }

      public short HorseNumber2 { get; init; }

      public short HorseNumber3 { get; init; }

      public int Money { get; init; }
    }

    internal static Refund FromJV(JVData_Struct.JV_HR_PAY hr)
    {
      var obj = new Refund
      {
        DataStatus = hr.head.DataKubun.ToDataStatus(),
        LastModified = hr.head.MakeDate.ToDateTime(),
        RaceKey = hr.id.ToRaceKey(),
      };

      foreach (var h in hr.PayTansyo)
      {
        short.TryParse(h.Umaban, out short num1);
        int.TryParse(h.Pay, out int money);
        if (num1 == 0)
        {
          continue;
        }
        obj.Single.Add(new Data1
        {
          HorseNumber = num1,
          Money = money,
        });
      }

      foreach (var h in hr.PayFukusyo)
      {
        short.TryParse(h.Umaban, out short num1);
        int.TryParse(h.Pay, out int money);
        if (num1 == 0)
        {
          continue;
        }
        obj.Place.Add(new Data1
        {
          HorseNumber = num1,
          Money = money,
        });
      }

      foreach (var h in hr.PayWakuren)
      {
        short.TryParse(h.Umaban.Substring(0, 1), out short num1);
        short.TryParse(h.Umaban.Substring(1, 1), out short num2);
        int.TryParse(h.Pay, out int money);
        if (num1 == 0)
        {
          continue;
        }
        obj.FrameNumber.Add(new DataF
        {
          FrameNumber1 = num1,
          FrameNumber2 = num2,
          Money = money,
        });
      }

      foreach (var h in hr.PayUmaren)
      {
        short.TryParse(h.Kumi.Substring(0, 2), out short num1);
        short.TryParse(h.Kumi.Substring(2, 2), out short num2);
        int.TryParse(h.Pay, out int money);
        if (num1 == 0 || num2 == 0)
        {
          continue;
        }
        obj.Quinella.Add(new Data2
        {
          HorseNumber1 = num1,
          HorseNumber2 = num2,
          Money = money,
        });
      }

      foreach (var h in hr.PayUmatan)
      {
        short.TryParse(h.Kumi.Substring(0, 2), out short num1);
        short.TryParse(h.Kumi.Substring(2, 2), out short num2);
        int.TryParse(h.Pay, out int money);
        if (num1 == 0 || num2 == 0)
        {
          continue;
        }
        obj.Exacta.Add(new Data2
        {
          HorseNumber1 = num1,
          HorseNumber2 = num2,
          Money = money,
        });
      }

      foreach (var h in hr.PayWide)
      {
        short.TryParse(h.Kumi.Substring(0, 2), out short num1);
        short.TryParse(h.Kumi.Substring(2, 2), out short num2);
        int.TryParse(h.Pay, out int money);
        if (num1 == 0 || num2 == 0)
        {
          continue;
        }
        obj.QuinellaPlace.Add(new Data2
        {
          HorseNumber1 = num1,
          HorseNumber2 = num2,
          Money = money,
        });
      }

      foreach (var h in hr.PaySanrenpuku)
      {
        short.TryParse(h.Kumi.Substring(0, 2), out short num1);
        short.TryParse(h.Kumi.Substring(2, 2), out short num2);
        short.TryParse(h.Kumi.Substring(4, 2), out short num3);
        int.TryParse(h.Pay, out int money);
        if (num1 == 0 || num2 == 0)
        {
          continue;
        }
        obj.Trio.Add(new Data3
        {
          HorseNumber1 = num1,
          HorseNumber2 = num2,
          HorseNumber3 = num3,
          Money = money,
        });
      }

      foreach (var h in hr.PaySanrentan)
      {
        short.TryParse(h.Kumi.Substring(0, 2), out short num1);
        short.TryParse(h.Kumi.Substring(2, 2), out short num2);
        short.TryParse(h.Kumi.Substring(4, 2), out short num3);
        int.TryParse(h.Pay, out int money);
        if (num1 == 0 || num2 == 0)
        {
          continue;
        }
        obj.Trifecta.Add(new Data3
        {
          HorseNumber1 = num1,
          HorseNumber2 = num2,
          HorseNumber3 = num3,
          Money = money,
        });
      }

      return obj;
    }
  }
}
