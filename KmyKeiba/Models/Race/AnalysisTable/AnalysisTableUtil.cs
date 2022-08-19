using KmyKeiba.Data.Db;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.AnalysisTable
{
  static class AnalysisTableUtil
  {
    private static bool _isInitialized;

    public static ReactiveCollection<AnalysisTableWeight> Weights { get; } = new();

    public static List<AnalysisTableData> TableConfigs { get; } = new();

    public static List<AnalysisTableRowData> TableRowConfigs { get; } = new();

    internal static async Task InitializeAsync(MyContext db)
    {
      if (!_isInitialized)
      {
        Weights.ToCollectionChanged().Subscribe(ev =>
        {
          if (ev.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
          {
            if (ev.Values != null || ev.Value != null)
            {
              foreach (var item in ev.Values ?? new[] { ev.Value })
              {
                item.Dispose();
              }
            }
          }
        });

        var weights = await db.AnalysisTableWeights!.ToArrayAsync();
        foreach (var weight in weights)
        {
          Weights.Add(new AnalysisTableWeight(weight));
        }

        var tables = await db.AnalysisTables!.ToArrayAsync();
        foreach (var table in tables)
        {
          TableConfigs.Add(table);
        }

        var rows = await db.AnalysisTableRows!.ToArrayAsync();
        foreach (var row in rows)
        {
          TableRowConfigs.Add(row);
        }

#if DEBUG
        // TODO: test data
        TableConfigs.Add(new AnalysisTableData
        {
          Id = 1000,
          Name = "テスト",
          Order = 1000,
        });

        using var finder = new Finder.FinderModel(null, null, null);
        finder.Input.Number.IsSetCurrentRaceHorseValue.Value = true;
        TableRowConfigs.Add(new AnalysisTableRowData
        {
          Id = 2000,
          Name = "わーい",
          Order = 2000,
          Output = AnalysisTableRowOutputType.PlaceBetsRate,
          FinderConfig = finder.Input.Serialize(false),
          TableId = 1000,
          BaseWeight = 1.0,
        });
#endif

        _isInitialized = true;
      }
    }
  }
}
