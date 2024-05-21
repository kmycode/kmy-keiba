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
    public byte[] SingleOddsRaw { get; set; } = Array.Empty<byte>();
    public byte[] PlaceOddsRaw { get; set; } = Array.Empty<byte>();

    private short[]? _singleOddsCache;

    public short[] GetSingleOdds()
    {
      if (this._singleOddsCache == null)
      {
        var list = new short[this.SingleOddsRaw.Length / 2];
        for (var i = 0; i < this.SingleOddsRaw.Length / 2; i++)
        {
          var odds = this.SingleOddsRaw[i * 2] << 8 | this.SingleOddsRaw[i * 2 + 1];
          list[i] = (short)odds;
        }
        this._singleOddsCache = list;
      }

      return this._singleOddsCache;
    }

    private void SetOddsRaw(SingleAndDoubleWinOdds odds)
    {
      var entities = odds.Odds
        .OrderBy(o => o.HorseNumber)
        .ToArray();
      var raw = new byte[entities.Length * 2];
      var placeRaw = new byte[entities.Length * 4];
      for (var i = 0; i < entities.Length; i++)
      {
        raw[i * 2] = (byte)(entities[i].Odds >> 8 & 255);
        raw[i * 2 + 1] = (byte)(entities[i].Odds & 255);
        placeRaw[i * 4] = (byte)(entities[i].PlaceOddsMax >> 8 & 255);
        placeRaw[i * 4 + 1] = (byte)(entities[i].PlaceOddsMax & 255);
        placeRaw[i * 4 + 2] = (byte)(entities[i].PlaceOddsMin >> 8 & 255);
        placeRaw[i * 4 + 3] = (byte)(entities[i].PlaceOddsMin & 255);
      }
      this.SingleOddsRaw = raw;
      this.PlaceOddsRaw = placeRaw;
    }

    public override void SetEntity(SingleAndDoubleWinOdds race)
    {
      this.LastModified = race.LastModified;
      this.DataStatus = race.DataStatus;
      this.RaceKey = race.RaceKey;
      this.Time = race.Time;
      this.SetOddsRaw(race);
    }

    public override bool IsEquals(DataBase<SingleAndDoubleWinOdds> b)
    {
      var obj = (SingleOddsTimeline)b;
      return this.RaceKey == obj.RaceKey && this.Time == obj.Time;
    }
  }

  [Index(nameof(RaceKey))]
  [Index(nameof(IsCopied))]
  public class PlaceOddsData : DataBase<SingleAndDoubleWinOdds>
  {
    [StringLength(20)]
    public string RaceKey { get; set; } = string.Empty;
    public byte[] PlaceOddsRaw { get; set; } = Array.Empty<byte>();

    public bool IsCopied { get; set; }

    private OddsItem[]? _placeOddsCache;

    public OddsItem[] GetPlaceOdds()
    {
      if (this._placeOddsCache == null)
      {
        var list = new OddsItem[this.PlaceOddsRaw.Length / 4];
        for (var i = 0; i < this.PlaceOddsRaw.Length / 4; i++)
        {
          var max = this.PlaceOddsRaw[i * 4] << 8 | this.PlaceOddsRaw[i * 4 + 1];
          var min = this.PlaceOddsRaw[i * 4 + 2] << 8 | this.PlaceOddsRaw[i * 4 + 3];
          list[i] = new OddsItem((short)(i + 1), (short)min, (short)max);
        }
        this._placeOddsCache = list;
      }

      return this._placeOddsCache;
    }

    public readonly struct OddsItem(short horseNumber, short min, short max)
    {
      public short HorseNumber { get; } = horseNumber;

      public short Min { get; } = min;

      public short Max { get; } = max;
    }

    private void SetOddsRaw(SingleAndDoubleWinOdds odds)
    {
      var entities = odds.Odds
        .OrderBy(o => o.HorseNumber)
        .ToArray();
      var placeRaw = new byte[entities.Length * 4];
      for (var i = 0; i < entities.Length; i++)
      {
        placeRaw[i * 4] = (byte)(entities[i].PlaceOddsMax >> 8 & 255);
        placeRaw[i * 4 + 1] = (byte)(entities[i].PlaceOddsMax & 255);
        placeRaw[i * 4 + 2] = (byte)(entities[i].PlaceOddsMin >> 8 & 255);
        placeRaw[i * 4 + 3] = (byte)(entities[i].PlaceOddsMin & 255);
      }
      this.PlaceOddsRaw = placeRaw;
    }

    public override void SetEntity(SingleAndDoubleWinOdds race)
    {
      this.LastModified = race.LastModified;
      this.DataStatus = race.DataStatus;
      this.RaceKey = race.RaceKey;
      this.SetOddsRaw(race);
    }

    public override bool IsEquals(DataBase<SingleAndDoubleWinOdds> b)
    {
      var obj = (PlaceOddsData)b;
      return this.RaceKey == obj.RaceKey;
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
              // 枠連は同じ数字同士で買えることがある
              //continue;
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
      var len = this.Odds.Length;
      for (var f1 = 1; f1 <= this.FramesCount; f1++)
      {
        for (var f2 = f1; f2 <= this.FramesCount; f2++)
        {
          if (f1 == f2)
          {
            // 枠連は番号が同じになることがある
            // continue;
          }
          if (i + 1 >= len)
          {
            break;
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
      var len = this.OddsMax.Length;
      for (var f1 = 1; f1 <= this.HorsesCount; f1++)
      {
        for (var f2 = f1; f2 <= this.HorsesCount; f2++)
        {
          if (f1 == f2)
          {
            continue;
          }
          if (i + 1 >= len)
          {
            break;
          }
          var valueMax = (this.OddsMax[i] << 8) + this.OddsMax[i + 1];
          var valueMin = (this.OddsMin[i] << 8) + this.OddsMin[i + 1];
          i += 2;
          list.Add(new QuinellaPlaceOdds.OddsData
          {
            HorseNumber1 = (short)f1,
            HorseNumber2 = (short)f2,
            PlaceOddsMax = (ushort)valueMax,
            PlaceOddsMin = (ushort)valueMin,
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
      var len = this.Odds.Length;
      for (var f1 = 1; f1 <= this.HorsesCount; f1++)
      {
        for (var f2 = f1; f2 <= this.HorsesCount; f2++)
        {
          if (f1 == f2)
          {
            continue;
          }
          if (i + 2 >= len)
          {
            break;
          }
          var value = (this.Odds[i] << 16) + (this.Odds[i + 1] << 8) + this.Odds[i + 2];
          i += 3;
          list.Add(new QuinellaOdds.OddsData
          {
            HorseNumber1 = (short)f1,
            HorseNumber2 = (short)f2,
            Odds = (uint)value,
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
      var len = this.Odds.Length;
      for (var f1 = 1; f1 <= this.HorsesCount; f1++)
      {
        for (var f2 = 1; f2 <= this.HorsesCount; f2++)
        {
          if (f1 == f2)
          {
            continue;
          }
          if (i + 2 >= len)
          {
            break;
          }
          var value = (this.Odds[i] << 16) + (this.Odds[i + 1] << 8) + this.Odds[i + 2];
          i += 3;
          list.Add(new ExactaOdds.OddsData
          {
            HorseNumber1 = (short)f1,
            HorseNumber2 = (short)f2,
            Odds = (uint)value,
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
        this.HorsesCount = odds.Odds.Max(o => Math.Max(Math.Max(o.HorseNumber1, o.HorseNumber2), o.HorseNumber3));

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

    public IReadOnlyList<TrioOdds.OddsData> RestoreOdds()
    {
      var list = new List<TrioOdds.OddsData>();

      var i = 0;
      var len = this.Odds.Length;
      for (var f1 = 1; f1 <= this.HorsesCount; f1++)
      {
        for (var f2 = f1; f2 <= this.HorsesCount; f2++)
        {
          if (f1 == f2)
          {
            continue;
          }
          for (var f3 = f2; f3 <= this.HorsesCount; f3++)
          {
            if (f1 == f3 || f2 == f3)
            {
              continue;
            }
            if (i + 2 >= len)
            {
              break;
            }
            var value = (this.Odds[i] << 16) + (this.Odds[i + 1] << 8) + this.Odds[i + 2];
            i += 3;
            list.Add(new TrioOdds.OddsData
            {
              HorseNumber1 = (short)f1,
              HorseNumber2 = (short)f2,
              HorseNumber3 = (short)f3,
              Odds = (uint)value,
            });
          }
        }
      }

      return list;
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
        this.HorsesCount = odds.Odds.Max(o => Math.Max(Math.Max(o.HorseNumber1, o.HorseNumber2), o.HorseNumber3));

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

    public IReadOnlyList<TrifectaOdds.OddsData> RestoreOdds()
    {
      var list = new List<TrifectaOdds.OddsData>();

      var i = 0;
      var len = this.Odds.Length;
      for (var f1 = 1; f1 <= this.HorsesCount; f1++)
      {
        for (var f2 = 1; f2 <= this.HorsesCount; f2++)
        {
          if (f1 == f2)
          {
            continue;
          }
          for (var f3 = 1; f3 <= this.HorsesCount; f3++)
          {
            if (f1 == f3 || f2 == f3)
            {
              continue;
            }
            if (i + 3 >= len)
            {
              break;
            }
            var value = ((uint)this.Odds[i] << 24) + ((uint)this.Odds[i + 1] << 16) + ((uint)this.Odds[i + 2] << 8) + (uint)this.Odds[i + 3];
            i += 4;
            list.Add(new TrifectaOdds.OddsData
            {
              HorseNumber1 = (short)f1,
              HorseNumber2 = (short)f2,
              HorseNumber3 = (short)f3,
              Odds = value,
            });
          }
        }
      }

      return list;
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
