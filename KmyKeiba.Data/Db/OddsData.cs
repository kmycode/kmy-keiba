using KmyKeiba.JVLink.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  [Index(nameof(RaceKey))]
  public class SingleOddsTimeline : DataBase<SingleAndDoubleWinOdds>
  {
    [StringLength(20)]
    public string RaceKey { get; set; } = string.Empty;
    public DateTime Time { get; set; }
    public short Odds1 { get; set; }
    public short Odds2 { get; set; }
    public short Odds3 { get; set; }
    public short Odds4 { get; set; }
    public short Odds5 { get; set; }
    public short Odds6 { get; set; }
    public short Odds7 { get; set; }
    public short Odds8 { get; set; }
    public short Odds9 { get; set; }
    public short Odds10 { get; set; }
    public short Odds11 { get; set; }
    public short Odds12 { get; set; }
    public short Odds13 { get; set; }
    public short Odds14 { get; set; }
    public short Odds15 { get; set; }
    public short Odds16 { get; set; }
    public short Odds17 { get; set; }
    public short Odds18 { get; set; }
    public short Odds19 { get; set; }
    public short Odds20 { get; set; }
    public short Odds21 { get; set; }
    public short Odds22 { get; set; }
    public short Odds23 { get; set; }
    public short Odds24 { get; set; }
    public short Odds25 { get; set; }
    public short Odds26 { get; set; }
    public short Odds27 { get; set; }
    public short Odds28 { get; set; }

    public override void SetEntity(SingleAndDoubleWinOdds race)
    {
      this.LastModified = race.LastModified;
      this.DataStatus = race.DataStatus;
      this.RaceKey = race.RaceKey;
      this.Time = race.Time;
      this.Odds1 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 1).Odds;
      this.Odds2 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 2).Odds;
      this.Odds3 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 3).Odds;
      this.Odds4 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 4).Odds;
      this.Odds5 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 5).Odds;
      this.Odds6 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 6).Odds;
      this.Odds7 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 7).Odds;
      this.Odds8 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 8).Odds;
      this.Odds9 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 9).Odds;
      this.Odds10 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 10).Odds;
      this.Odds11 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 11).Odds;
      this.Odds12 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 12).Odds;
      this.Odds13 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 13).Odds;
      this.Odds14 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 14).Odds;
      this.Odds15 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 15).Odds;
      this.Odds16 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 16).Odds;
      this.Odds17 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 17).Odds;
      this.Odds18 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 18).Odds;
      this.Odds19 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 19).Odds;
      this.Odds20 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 20).Odds;
      this.Odds21 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 21).Odds;
      this.Odds22 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 22).Odds;
      this.Odds23 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 23).Odds;
      this.Odds24 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 24).Odds;
      this.Odds25 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 25).Odds;
      this.Odds26 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 26).Odds;
      this.Odds27 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 27).Odds;
      this.Odds28 = race.Odds.FirstOrDefault((r) => r.HorseNumber == 28).Odds;
    }

    public override bool IsEquals(DataBase<SingleAndDoubleWinOdds> b)
    {
      var obj = (SingleOddsTimeline)b;
      return this.RaceKey == obj.RaceKey && this.Time == obj.Time;
    }
  }

  [Index(nameof(RaceKey))]
  public class FrameNumberOddsData : DataBase<FrameNumberOdds>
  {
    [StringLength(20)]
    public string RaceKey { get; set; } = string.Empty;

    public short FramesCount { get; set; }

    public byte[] Odds { get; set; } = Array.Empty<byte>();

    public override void SetEntity(FrameNumberOdds odds)
    {
      this.LastModified = odds.LastModified;
      this.DataStatus = odds.DataStatus;
      this.RaceKey = odds.RaceKey;

      if (odds.Odds.Any())
      {
        this.FramesCount = odds.Odds.Max(o => Math.Max(o.Frame1, o.Frame2));

        var values = new List<uint>();
        for (var f1 = 1; f1 <= this.FramesCount; f1++)
        {
          for (var f2 = f1; f2 <= this.FramesCount; f2++)
          {
            if (f1 == f2)
            {
              continue;
            }
            var data = odds.Odds.FirstOrDefault(o => (o.Frame1 == f1 && o.Frame2 == f2) || (o.Frame1 == f2 && o.Frame2 == f1));
            values.Add((ushort)data.Odds);
          }
        }
        var binary = values.SelectMany(v => new byte[] { (byte)(v >> 8 & 255), (byte)(v & 255), }).ToArray();
        this.Odds = binary;
      }
    }

    public IReadOnlyList<FrameNumberOdds.OddsData> RestoreOdds()
    {
      var list = new List<FrameNumberOdds.OddsData>();

      var i = 0;
      for (var f1 = 1; f1 <= this.FramesCount; f1++)
      {
        for (var f2 = f1; f2 <= this.FramesCount; f2++)
        {
          if (f1 == f2)
          {
            continue;
          }
          var value = (this.Odds[i] << 8) + this.Odds[i + 1];
          i += 2;
          list.Add(new FrameNumberOdds.OddsData
          {
            Frame1 = (short)f1,
            Frame2 = (short)f2,
            Odds = (short)value,
          });
        }
      }

      return list;
    }

    public override bool IsEquals(DataBase<FrameNumberOdds> b)
    {
      var c = (FrameNumberOddsData)b;
      return this.RaceKey == c.RaceKey;
    }

    public override int GetHashCode()
    {
      return (this.RaceKey).GetHashCode();
    }
  }

  [Index(nameof(RaceKey))]
  public class QuinellaPlaceOddsData : DataBase<QuinellaPlaceOdds>
  {
    [StringLength(20)]
    public string RaceKey { get; set; } = string.Empty;

    public short HorsesCount { get; set; }

    public byte[] OddsMin { get; set; } = Array.Empty<byte>();

    public byte[] OddsMax { get; set; } = Array.Empty<byte>();

    public override void SetEntity(QuinellaPlaceOdds odds)
    {
      this.LastModified = odds.LastModified;
      this.DataStatus = odds.DataStatus;
      this.RaceKey = odds.RaceKey;

      if (odds.Odds.Any())
      {
        this.HorsesCount = odds.Odds.Max(o => Math.Max(o.HorseNumber1, o.HorseNumber2));

        var valuesMax = new List<uint>();
        var valuesMin = new List<uint>();
        for (var h1 = 1; h1 <= this.HorsesCount; h1++)
        {
          for (var h2 = h1; h2 <= this.HorsesCount; h2++)
          {
            if (h1 == h2)
            {
              continue;
            }
            var data = odds.Odds.FirstOrDefault(o => (o.HorseNumber1 == h1 && o.HorseNumber2 == h2) || (o.HorseNumber1 == h2 && o.HorseNumber2 == h1));
            valuesMax.Add((ushort)data.PlaceOddsMax);
            valuesMin.Add((ushort)data.PlaceOddsMin);
          }
        }
        var binaryMax = valuesMax.SelectMany(v => new byte[] { (byte)(v >> 8 & 255), (byte)(v & 255), }).ToArray();
        var binaryMin = valuesMin.SelectMany(v => new byte[] { (byte)(v >> 8 & 255), (byte)(v & 255), }).ToArray();
        this.OddsMax = binaryMax;
        this.OddsMin = binaryMin;
      }
    }

    public IReadOnlyList<QuinellaPlaceOdds.OddsData> RestoreOdds()
    {
      var list = new List<QuinellaPlaceOdds.OddsData>();

      var i = 0;
      for (var f1 = 1; f1 <= this.HorsesCount; f1++)
      {
        for (var f2 = f1; f2 <= this.HorsesCount; f2++)
        {
          if (f1 == f2)
          {
            continue;
          }
          var valueMax = (this.OddsMax[i] << 8) + this.OddsMax[i + 1];
          var valueMin = (this.OddsMin[i] << 8) + this.OddsMin[i + 1];
          i += 2;
          list.Add(new QuinellaPlaceOdds.OddsData
          {
            HorseNumber1 = (short)f1,
            HorseNumber2 = (short)f2,
            PlaceOddsMax = (short)valueMax,
            PlaceOddsMin = (short)valueMin,
          });
        }
      }

      return list;
    }

    public override bool IsEquals(DataBase<QuinellaPlaceOdds> b)
    {
      var c = (QuinellaPlaceOddsData)b;
      return this.RaceKey == c.RaceKey;
    }

    public override int GetHashCode()
    {
      return (this.RaceKey).GetHashCode();
    }
  }

  [Index(nameof(RaceKey))]
  public class QuinellaOddsData : DataBase<QuinellaOdds>
  {
    [StringLength(20)]
    public string RaceKey { get; set; } = string.Empty;

    public short HorsesCount { get; set; }

    public byte[] Odds { get; set; } = Array.Empty<byte>();

    public override void SetEntity(QuinellaOdds odds)
    {
      this.LastModified = odds.LastModified;
      this.DataStatus = odds.DataStatus;
      this.RaceKey = odds.RaceKey;

      if (odds.Odds.Any())
      {
        this.HorsesCount = odds.Odds.Max(o => Math.Max(o.HorseNumber1, o.HorseNumber2));

        var values = new List<uint>();
        for (var h1 = 1; h1 <= this.HorsesCount; h1++)
        {
          for (var h2 = h1; h2 <= this.HorsesCount; h2++)
          {
            if (h1 == h2)
            {
              continue;
            }
            var data = odds.Odds.FirstOrDefault(o => (o.HorseNumber1 == h1 && o.HorseNumber2 == h2) || (o.HorseNumber1 == h2 && o.HorseNumber2 == h1));
            values.Add((uint)data.Odds);
          }
        }
        var binary = values.SelectMany(v => new byte[] { (byte)(v >> 16 & 255), (byte)(v >> 8 & 255), (byte)(v & 255), }).ToArray();
        this.Odds = binary;
      }
    }

    public IReadOnlyList<QuinellaOdds.OddsData> RestoreOdds()
    {
      var list = new List<QuinellaOdds.OddsData>();

      var i = 0;
      for (var f1 = 1; f1 <= this.HorsesCount; f1++)
      {
        for (var f2 = f1; f2 <= this.HorsesCount; f2++)
        {
          if (f1 == f2)
          {
            continue;
          }
          var value = (this.Odds[i] << 16) + (this.Odds[i + 1] << 8) + this.Odds[i + 2];
          i += 3;
          list.Add(new QuinellaOdds.OddsData
          {
            HorseNumber1 = (short)f1,
            HorseNumber2 = (short)f2,
            Odds = (short)value,
          });
        }
      }

      return list;
    }

    public override bool IsEquals(DataBase<QuinellaOdds> b)
    {
      var c = (QuinellaOddsData)b;
      return this.RaceKey == c.RaceKey;
    }

    public override int GetHashCode()
    {
      return (this.RaceKey).GetHashCode();
    }
  }

  [Index(nameof(RaceKey))]
  public class ExactaOddsData : DataBase<ExactaOdds>
  {
    [StringLength(20)]
    public string RaceKey { get; set; } = string.Empty;

    public short HorsesCount { get; set; }

    public byte[] Odds { get; set; } = Array.Empty<byte>();

    public override void SetEntity(ExactaOdds odds)
    {
      this.LastModified = odds.LastModified;
      this.DataStatus = odds.DataStatus;
      this.RaceKey = odds.RaceKey;

      if (odds.Odds.Any())
      {
        this.HorsesCount = odds.Odds.Max(o => Math.Max(o.HorseNumber1, o.HorseNumber2));

        var values = new List<uint>();
        for (var h1 = 1; h1 <= this.HorsesCount; h1++)
        {
          for (var h2 = 1; h2 <= this.HorsesCount; h2++)
          {
            if (h1 == h2)
            {
              continue;
            }
            var data = odds.Odds.FirstOrDefault(o => o.HorseNumber1 == h1 && o.HorseNumber2 == h2);
            values.Add((uint)data.Odds);
          }
        }
        var binary = values.SelectMany(v => new byte[] { (byte)(v >> 16 & 255), (byte)(v >> 8 & 255), (byte)(v & 255), }).ToArray();
        this.Odds = binary;
      }
    }

    public IReadOnlyList<ExactaOdds.OddsData> RestoreOdds()
    {
      var list = new List<ExactaOdds.OddsData>();

      var i = 0;
      for (var f1 = 1; f1 <= this.HorsesCount; f1++)
      {
        for (var f2 = 1; f2 <= this.HorsesCount; f2++)
        {
          if (f1 == f2)
          {
            continue;
          }
          var value = (this.Odds[i] << 16) + (this.Odds[i + 1] << 8) + this.Odds[i + 2];
          i += 3;
          list.Add(new ExactaOdds.OddsData
          {
            HorseNumber1 = (short)f1,
            HorseNumber2 = (short)f2,
            Odds = (short)value,
          });
        }
      }

      return list;
    }

    public override bool IsEquals(DataBase<ExactaOdds> b)
    {
      var c = (ExactaOddsData)b;
      return this.RaceKey == c.RaceKey;
    }

    public override int GetHashCode()
    {
      return (this.RaceKey).GetHashCode();
    }
  }

  [Index(nameof(RaceKey))]
  public class TrioOddsData : DataBase<TrioOdds>
  {
    [StringLength(20)]
    public string RaceKey { get; set; } = string.Empty;

    public short HorsesCount { get; set; }

    public byte[] Odds { get; set; } = Array.Empty<byte>();

    public override void SetEntity(TrioOdds odds)
    {
      this.LastModified = odds.LastModified;
      this.DataStatus = odds.DataStatus;
      this.RaceKey = odds.RaceKey;

      if (odds.Odds.Any())
      {
        this.HorsesCount = odds.Odds.Max(o => Math.Max(o.HorseNumber1, o.HorseNumber2));

        var values = new List<uint>();
        for (var h1 = 1; h1 <= this.HorsesCount; h1++)
        {
          for (var h2 = h1; h2 <= this.HorsesCount; h2++)
          {
            if (h1 == h2)
            {
              continue;
            }
            for (var h3 = h2; h3 <= this.HorsesCount; h3++)
            {
              if (h2 == h3 || h1 == h3)
              {
                continue;
              }
              var data = odds.Odds.FirstOrDefault(o =>
                (o.HorseNumber1 == h1 && o.HorseNumber2 == h2 && o.HorseNumber3 == h3) ||
                (o.HorseNumber1 == h1 && o.HorseNumber2 == h3 && o.HorseNumber3 == h2) ||
                (o.HorseNumber1 == h2 && o.HorseNumber2 == h1 && o.HorseNumber3 == h3) ||
                (o.HorseNumber1 == h2 && o.HorseNumber2 == h3 && o.HorseNumber3 == h1) ||
                (o.HorseNumber1 == h3 && o.HorseNumber2 == h1 && o.HorseNumber3 == h2) ||
                (o.HorseNumber1 == h3 && o.HorseNumber2 == h2 && o.HorseNumber3 == h1));
              values.Add((uint)data.Odds);
            }
          }
        }
        var binary = values.SelectMany(v => new byte[] { (byte)(v >> 16 & 255), (byte)(v >> 8 & 255), (byte)(v & 255), }).ToArray();
        this.Odds = binary;
      }
    }

    public override bool IsEquals(DataBase<TrioOdds> b)
    {
      var c = (TrioOddsData)b;
      return this.RaceKey == c.RaceKey;
    }

    public override int GetHashCode()
    {
      return (this.RaceKey).GetHashCode();
    }
  }

  [Index(nameof(RaceKey))]
  public class TrifectaOddsData : DataBase<TrifectaOdds>
  {
    [StringLength(20)]
    public string RaceKey { get; set; } = string.Empty;

    public short HorsesCount { get; set; }

    public byte[] Odds { get; set; } = Array.Empty<byte>();

    public override void SetEntity(TrifectaOdds odds)
    {
      this.LastModified = odds.LastModified;
      this.DataStatus = odds.DataStatus;
      this.RaceKey = odds.RaceKey;

      if (odds.Odds.Any())
      {
        this.HorsesCount = odds.Odds.Max(o => Math.Max(o.HorseNumber1, o.HorseNumber2));

        var values = new List<uint>();
        for (var h1 = 1; h1 <= this.HorsesCount; h1++)
        {
          for (var h2 = 1; h2 <= this.HorsesCount; h2++)
          {
            if (h1 == h2)
            {
              continue;
            }
            for (var h3 = 1; h3 <= this.HorsesCount; h3++)
            {
              if (h2 == h3 || h1 == h3)
              {
                continue;
              }
              var data = odds.Odds.FirstOrDefault(o => o.HorseNumber1 == h1 && o.HorseNumber2 == h2 && o.HorseNumber3 == h3);
              values.Add((uint)data.Odds);
            }
          }
        }
        var binary = values.SelectMany(v => new byte[] { (byte)(v >> 24 & 255), (byte)(v >> 16 & 255), (byte)(v >> 8 & 255), (byte)(v & 255), }).ToArray();
        this.Odds = binary;
      }
    }

    public override bool IsEquals(DataBase<TrifectaOdds> b)
    {
      var c = (TrifectaOddsData)b;
      return this.RaceKey == c.RaceKey;
    }

    public override int GetHashCode()
    {
      return (this.RaceKey).GetHashCode();
    }
  }
}
