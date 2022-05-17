using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Analysis.Generic;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race
{
  public class OddsInfo
  {
    private OddsList<SingleTicket> _singles { get; } = new();
    public ReactiveProperty<OddsList<SingleTicket>?> Singles { get; } = new();

    private OddsBlock<FrameNumberOdds.OddsData>? _frames { get; }
    public ReactiveProperty<OddsBlock<FrameNumberOdds.OddsData>?> Frames { get; } = new();

    private OddsBlock<QuinellaPlaceOdds.OddsData>? _quinellaPlaces { get; }
    public ReactiveProperty<OddsBlock<QuinellaPlaceOdds.OddsData>?> QuinellaPlaces { get; } = new();

    private OddsBlock<QuinellaOdds.OddsData>? _quinellas { get; }
    public ReactiveProperty<OddsBlock<QuinellaOdds.OddsData>?> Quinellas { get; } = new();

    private OddsBlock<ExactaOdds.OddsData>? _exactas { get; }
    public ReactiveProperty<OddsBlock<ExactaOdds.OddsData>?> Exactas { get; } = new();

    private OddsBlockGroup<TrioOdds.OddsData>? _trios { get; } = new();
    public ReactiveProperty<OddsBlockGroup<TrioOdds.OddsData>?> Trios { get; } = new();

    private OddsBlockGroup<TrifectaOdds.OddsData>? _trifectas { get; } = new();
    public ReactiveProperty<OddsBlockGroup<TrifectaOdds.OddsData>?> Trifectas { get; } = new();

    public CheckableCollection<OddsFilterItem> Filters { get; } = new();

    public OddsInfo(IReadOnlyList<RaceHorseData> horses, FrameNumberOddsData? frame, QuinellaPlaceOddsData? quinellaPlace, QuinellaOddsData? quinella, ExactaOddsData? exacta, TrioOddsData? trio, TrifectaOddsData? trifecta)
    {
      var singles = horses.OrderBy(h => h.Number).Select(h => new SingleTicket
      {
        Number1 = h.Number,
        Odds = h.Odds,
        PlaceOddsMax = h.PlaceOddsMax,
        PlaceOddsMin = h.PlaceOddsMin,
      }).ToArray();
      AddRange(this._singles, singles);
      this.Singles.Value = this._singles;

      if (frame != null)
      {
        var block = OddsBlock.Create(frame);
        this._frames = this.Frames.Value = block;
      }
      if (quinellaPlace != null)
      {
        var block = OddsBlock.Create(quinellaPlace);
        this._quinellaPlaces = this.QuinellaPlaces.Value = block;
      }
      if (quinella != null)
      {
        var block = OddsBlock.Create(quinella);
        this._quinellas = this.Quinellas.Value = block;
      }
      if (exacta != null)
      {
        var block = OddsBlock.Create(exacta);
        this._exactas = this.Exactas.Value = block;
      }
      if (trio != null)
      {
        var block = OddsBlock.Create(trio);
        this._trios = this.Trios.Value = block;
      }
      if (trifecta != null)
      {
        var block = OddsBlock.Create(trifecta);
        this._trifectas = this.Trifectas.Value = block;
      }

      // フィルター
      var horseFrames = horses.Select(h => new { h.Number, h.FrameNumber, }).ToArray();
      this.Filters = new MultipleCheckableCollection<OddsFilterItem>();
      AddRange(this.Filters, horses.OrderBy(h => h.Number).Select(h => new OddsFilterItem
      {
        Number = h.Number,
        IsChecked = { Value = true, },
      }));
      this.Filters.ChangedItemObservable.Subscribe(h =>
      {
        this.Singles.Value = this._singles.Filtering(this.Filters, t => t.Number1);
        this.Frames.Value = this._frames?.Filtering(this.Filters, o =>
        {
          return horseFrames
            .Where(hf => hf.FrameNumber == o.Frame1 || hf.FrameNumber == o.Frame2)
            .Select(hf => hf.FrameNumber);
        });
        this.QuinellaPlaces.Value = this._quinellaPlaces?.Filtering(this.Filters, o => new[] { o.HorseNumber1, o.HorseNumber2, });
        this.Quinellas.Value = this._quinellas?.Filtering(this.Filters, o => new[] { o.HorseNumber1, o.HorseNumber2, });
        this.Exactas.Value = this._exactas?.Filtering(this.Filters, o => new[] { o.HorseNumber1, o.HorseNumber2, });
        this.Trios.Value = this._trios?.Filtering(this.Filters, o => new[] { o.HorseNumber1, o.HorseNumber2, o.HorseNumber3, });
        this.Trifectas.Value = this._trifectas?.Filtering(this.Filters, o => new[] { o.HorseNumber1, o.HorseNumber2, o.HorseNumber3, });
      });
    }

    private static void AddRange<T>(ReactiveCollection<T> collection, IEnumerable<T> items)
    {
      foreach (var item in items)
      {
        collection.Add(item);
      }
    }
  }

  public struct SingleTicket
  {
    public short Number1 { get; init; }

    public short Odds { get; init; }

    public short PlaceOddsMax { get; init; }

    public short PlaceOddsMin { get; init; }

    public ValueComparation Comparation { get; init; }
  }

  public struct OddsItem<T>
  {
    public T Data { get; }

    public ValueComparation Comparation { get; }

    public OddsItem(T data, ValueComparation comparation)
    {
      this.Data = data;
      this.Comparation = comparation;
    }
  }

  public class OddsList<T> : ReactiveCollection<T>
  {
    public OddsList<T> Filtering(IReadOnlyList<OddsFilterItem> filters, Func<T, short> horseNumber)
    {
      var items = this.Where(i => filters.Where(f => f.IsChecked.Value).Any(f => f.Number != horseNumber(i)));
      var list = new OddsList<T>();
      foreach (var item in items)
      {
        list.Add(item);
      }
      return list;
    }
  }

  public class OddsBlock<T>
  {
    public IReadOnlyList<short> Numbers { get; private init; } = Array.Empty<short>();

    public short NumberInGroup { get; init; }

    public IReadOnlyList<OddsBlockColumn<T>> Columns { get; init; } = Array.Empty<OddsBlockColumn<T>>();

    private OddsBlock()
    {
    }

    public OddsBlock(int numbers)
    {
      this.Numbers = Enumerable.Range(1, numbers).Select(n => (short)n).ToArray();
    }

    public OddsBlock(int numbers, int startNumber)
    {
      if (numbers - startNumber + 1 > 0)
      {
        this.Numbers = Enumerable.Range(startNumber, numbers - startNumber + 1).Select(n => (short)n).ToArray();
      }
      else
      {
        this.Numbers = Array.Empty<short>();
      }
    }

    public OddsBlock<T> Filtering(IReadOnlyList<OddsFilterItem> filters, Func<T, IEnumerable<short>> itemNumbers)
    {
      var items = this.Columns
        .Where(c => filters.Where(f => f.IsChecked.Value).Any(f => f.Number == c.Number))
        .Select(c => c.Filtering(filters, itemNumbers))
        .ToArray();
      var numbers = filters.OrderBy(f => f.Number).Where(f => f.IsChecked.Value).Select(f => f.Number).ToArray();
      return new OddsBlock<T>()
      {
        Numbers = Numbers,
        NumberInGroup = this.NumberInGroup,
        Columns = items,
      };
    }
  }

  public static class OddsBlock
  {
    private static ValueComparation OddsRange(uint odds, short badMax, short goodMin)
    {
      return odds >= goodMin ? ValueComparation.Good :
        odds <= badMax ? ValueComparation.Bad : ValueComparation.Standard;
    }

    #region Creations

    public static OddsBlock<FrameNumberOdds.OddsData> Create(FrameNumberOddsData data)
    {
      var odds = data.RestoreOdds();
      var columns = new List<OddsBlockColumn<FrameNumberOdds.OddsData>>();
      for (var f1 = 1; f1 <= data.FramesCount; f1++)
      {
        var rows = new List<OddsItem<FrameNumberOdds.OddsData>>();
        for (var f2 = 1; f2 <= data.FramesCount; f2++)
        {
          var d = odds.FirstOrDefault(o => o.Frame1 == f1 && o.Frame2 == f2);
          rows.Add(new OddsItem<FrameNumberOdds.OddsData>(d, OddsRange((uint)d.Odds, 20, 300)));
        }
        var column = new OddsBlockColumn<FrameNumberOdds.OddsData>
        {
          Number = (short)f1,
          Odds = rows,
        };
        columns.Add(column);
      }

      return new OddsBlock<FrameNumberOdds.OddsData>(data.FramesCount)
      {
        Columns = columns,
      };
    }

    public static OddsBlock<QuinellaPlaceOdds.OddsData> Create(QuinellaPlaceOddsData data)
    {
      var odds = data.RestoreOdds();
      var columns = new List<OddsBlockColumn<QuinellaPlaceOdds.OddsData>>();
      for (var f1 = 1; f1 <= data.HorsesCount; f1++)
      {
        var rows = new List<OddsItem<QuinellaPlaceOdds.OddsData>>();
        for (var f2 = 1; f2 <= data.HorsesCount; f2++)
        {
          var d = odds.FirstOrDefault(o => o.HorseNumber1 == f1 && o.HorseNumber2 == f2);
          rows.Add(new OddsItem<QuinellaPlaceOdds.OddsData>(d, OddsRange(d.PlaceOddsMax, 100, 300)));
        }
        var column = new OddsBlockColumn<QuinellaPlaceOdds.OddsData>
        {
          Number = (short)f1,
          Odds = rows,
        };
        columns.Add(column);
      }

      return new OddsBlock<QuinellaPlaceOdds.OddsData>(data.HorsesCount)
      {
        Columns = columns,
      };
    }

    public static OddsBlock<QuinellaOdds.OddsData> Create(QuinellaOddsData data)
    {
      var odds = data.RestoreOdds();
      var columns = new List<OddsBlockColumn<QuinellaOdds.OddsData>>();
      for (var f1 = 1; f1 <= data.HorsesCount; f1++)
      {
        var rows = new List<OddsItem<QuinellaOdds.OddsData>>();
        for (var f2 = 1; f2 <= data.HorsesCount; f2++)
        {
          var d = odds.FirstOrDefault(o => o.HorseNumber1 == f1 && o.HorseNumber2 == f2);
          rows.Add(new OddsItem<QuinellaOdds.OddsData>(d, OddsRange(d.Odds, 200, 1000)));
        }
        var column = new OddsBlockColumn<QuinellaOdds.OddsData>
        {
          Number = (short)f1,
          Odds = rows,
        };
        columns.Add(column);
      }

      return new OddsBlock<QuinellaOdds.OddsData>(data.HorsesCount)
      {
        Columns = columns,
      };
    }

    public static OddsBlock<ExactaOdds.OddsData> Create(ExactaOddsData data)
    {
      var odds = data.RestoreOdds();
      var columns = new List<OddsBlockColumn<ExactaOdds.OddsData>>();
      for (var f1 = 1; f1 <= data.HorsesCount; f1++)
      {
        var rows = new List<OddsItem<ExactaOdds.OddsData>>();
        for (var f2 = 1; f2 <= data.HorsesCount; f2++)
        {
          var d = odds.FirstOrDefault(o => o.HorseNumber1 == f1 && o.HorseNumber2 == f2);
          rows.Add(new OddsItem<ExactaOdds.OddsData>(d, OddsRange((uint)d.Odds, 400, 1500)));
        }
        var column = new OddsBlockColumn<ExactaOdds.OddsData>
        {
          Number = (short)f1,
          Odds = rows,
        };
        columns.Add(column);
      }

      return new OddsBlock<ExactaOdds.OddsData>(data.HorsesCount)
      {
        Columns = columns,
      };
    }

    public static OddsBlockGroup<TrioOdds.OddsData> Create(TrioOddsData data)
    {
      var odds = data.RestoreOdds();
      var blocks = new List<OddsBlock<TrioOdds.OddsData>>();
      for (var f1 = 1; f1 <= data.HorsesCount - 2; f1++)
      {
        var columns = new List<OddsBlockColumn<TrioOdds.OddsData>>();
        for (var f2 = f1 + 1; f2 <= data.HorsesCount - 1; f2++)
        {
          var rows = new List<OddsItem<TrioOdds.OddsData>>();
          for (var f3 = f1 + 2; f3 <= data.HorsesCount; f3++)
          {
            var d = odds.FirstOrDefault(o => o.HorseNumber1 == f1 && o.HorseNumber2 == f2 && o.HorseNumber3 == f3);
            rows.Add(new OddsItem<TrioOdds.OddsData>(d, OddsRange((uint)d.Odds, 500, 5000)));
          }
          var column = new OddsBlockColumn<TrioOdds.OddsData>
          {
            Number = (short)f2,
            Odds = rows,
          };
          columns.Add(column);
        }
        var block = new OddsBlock<TrioOdds.OddsData>(data.HorsesCount, f1 + 2)
        {
          NumberInGroup = (short)f1,
          Columns = columns,
        };
        blocks.Add(block);
      }

      return new OddsBlockGroup<TrioOdds.OddsData>()
      {
        Blocks = blocks,
      };
    }

    public static OddsBlockGroup<TrifectaOdds.OddsData> Create(TrifectaOddsData data)
    {
      var odds = data.RestoreOdds();
      var blocks = new List<OddsBlock<TrifectaOdds.OddsData>>();
      for (var f1 = 1; f1 <= data.HorsesCount; f1++)
      {
        var columns = new List<OddsBlockColumn<TrifectaOdds.OddsData>>();
        for (var f2 = 1; f2 <= data.HorsesCount; f2++)
        {
          var rows = new List<OddsItem<TrifectaOdds.OddsData>>();
          for (var f3 = 1; f3 <= data.HorsesCount; f3++)
          {
            var d = odds.FirstOrDefault(o => o.HorseNumber1 == f1 && o.HorseNumber2 == f2 && o.HorseNumber3 == f3);
            rows.Add(new OddsItem<TrifectaOdds.OddsData>(d, OddsRange((uint)d.Odds, 1000, 10000)));
          }
          var column = new OddsBlockColumn<TrifectaOdds.OddsData>
          {
            Number = (short)f2,
            Odds = rows,
          };
          columns.Add(column);
        }
        var block = new OddsBlock<TrifectaOdds.OddsData>(data.HorsesCount)
        {
          NumberInGroup = (short)f1,
          Columns = columns,
        };
        blocks.Add(block);
      }

      return new OddsBlockGroup<TrifectaOdds.OddsData>()
      {
        Blocks = blocks,
      };
    }

    #endregion
  }

  public class OddsBlockColumn<T>
  {
    public short Number { get; init; }

    public IReadOnlyList<OddsItem<T>> Odds { get; init; } = Array.Empty<OddsItem<T>>();

    public OddsBlockColumn<T> Filtering(IReadOnlyList<OddsFilterItem> filters, Func<T, IEnumerable<short>> itemNumbers)
    {
      var items = this.Odds
        .Where(o => !filters.Where(f => !f.IsChecked.Value).Any(f => itemNumbers(o.Data).Contains(f.Number)))
        .ToArray();
      return new OddsBlockColumn<T>
      {
        Number = this.Number,
        Odds = items,
      };
    }
  }

  public class OddsBlockGroup<T>
  {
    public IReadOnlyList<OddsBlock<T>> Blocks { get; init; } = Array.Empty<OddsBlock<T>>();

    public OddsBlockGroup<T> Filtering(IReadOnlyList<OddsFilterItem> filters, Func<T, IEnumerable<short>> itemNumbers)
    {
      var items = this.Blocks
        .Where(b => filters.Where(f => f.IsChecked.Value).Any(f => f.Number == b.NumberInGroup))
        .Select(b => b.Filtering(filters, itemNumbers))
        .ToArray();
      return new OddsBlockGroup<T>
      {
        Blocks = items,
      };
    }
  }

  public class OddsFilterItem : IMultipleCheckableItem
  {
    public short Number { get; init; }

    public ReactiveProperty<bool> IsChecked { get; } = new();

    public string? GroupName { get; }
  }
}
