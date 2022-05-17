using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
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
    private ReactiveCollection<SingleTicket> _singles { get; } = new();
    public ReactiveCollection<SingleTicket> Singles { get; } = new();

    private ReactiveProperty<OddsBlock<FrameNumberOdds.OddsData>> _frames { get; } = new();
    public ReactiveProperty<OddsBlock<FrameNumberOdds.OddsData>> Frames { get; } = new();

    private ReactiveProperty<OddsBlock<QuinellaPlaceOdds.OddsData>> _quinellaPlaces { get; } = new();
    public ReactiveProperty<OddsBlock<QuinellaPlaceOdds.OddsData>> QuinellaPlaces { get; } = new();

    private ReactiveProperty<OddsBlock<QuinellaOdds.OddsData>> _quinellas { get; } = new();
    public ReactiveProperty<OddsBlock<QuinellaOdds.OddsData>> Quinellas { get; } = new();

    private ReactiveProperty<OddsBlock<ExactaOdds.OddsData>> _exactas { get; } = new();
    public ReactiveProperty<OddsBlock<ExactaOdds.OddsData>> Exactas { get; } = new();

    public OddsInfo(IReadOnlyList<RaceHorseData> horses, FrameNumberOddsData? frame, QuinellaPlaceOddsData? quinellaPlace, QuinellaOddsData? quinella, ExactaOddsData? exacta)
    {
      var singles = horses.OrderBy(h => h.Number).Select(h => new SingleTicket
      {
        Number1 = h.Number,
        Odds = h.Odds,
        PlaceOddsMax = h.PlaceOddsMax,
        PlaceOddsMin = h.PlaceOddsMin,
      }).ToArray();
      AddRange(this._singles, singles);
      AddRange(this.Singles, singles);

      if (frame != null)
      {
        var block = OddsBlock.Create(frame);
        this._frames.Value = this.Frames.Value = block;
      }
      if (quinellaPlace != null)
      {
        var block = OddsBlock.Create(quinellaPlace);
        this._quinellaPlaces.Value = this.QuinellaPlaces.Value = block;
      }
      if (quinella != null)
      {
        var block = OddsBlock.Create(quinella);
        this._quinellas.Value = this.Quinellas.Value = block;
      }
      if (exacta != null)
      {
        var block = OddsBlock.Create(exacta);
        this._exactas.Value = this.Exactas.Value = block;
      }
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

  public class OddsBlock<T>
  {
    public IReadOnlyList<short> Numbers { get; }

    public IReadOnlyList<OddsBlockColumn<T>> Columns { get; init; } = Array.Empty<OddsBlockColumn<T>>();

    public OddsBlock(int numbers)
    {
      this.Numbers = Enumerable.Range(1, numbers).Select(n => (short)n).ToArray();
    }
  }

  public static class OddsBlock
  {
    private static ValueComparation OddsRange(short odds, short badMax, short goodMin)
    {
      return odds >= goodMin ? ValueComparation.Good :
        odds <= badMax ? ValueComparation.Bad : ValueComparation.Standard;
    }

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
          rows.Add(new OddsItem<FrameNumberOdds.OddsData>(d, OddsRange(d.Odds, 20, 500)));
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
          rows.Add(new OddsItem<QuinellaPlaceOdds.OddsData>(d, OddsRange(d.PlaceOddsMax, 20, 500)));
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
          rows.Add(new OddsItem<QuinellaOdds.OddsData>(d, OddsRange(d.Odds, 20, 500)));
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
          rows.Add(new OddsItem<ExactaOdds.OddsData>(d, OddsRange(d.Odds, 20, 500)));
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
  }

  public class OddsBlockColumn<T>
  {
    public short Number { get; init; }

    public IReadOnlyList<OddsItem<T>> Odds { get; init; } = Array.Empty<OddsItem<T>>();
  }
}
