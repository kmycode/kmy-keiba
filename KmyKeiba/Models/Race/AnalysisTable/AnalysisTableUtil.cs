using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis.Generic;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race.Finder;
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

    public static ReactiveCollection<ValueDelimiter> Delimiters { get; } = new();

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

        var delimiters = await db.Delimiters!.ToArrayAsync();
        var delimiterRows = await db.DelimiterRows!.ToArrayAsync();
        foreach (var delimiter in delimiters
          .GroupJoin(delimiterRows, d => d.Id, r => r.DelimiterId, (d, r) => new { Delimiter = d, Rows = r, }))
        {
          var obj = new ValueDelimiter(delimiter.Delimiter);

          foreach (var row in delimiter.Rows.OrderBy(r => r.Order))
          {
            obj.Rows.Add(new ValueDelimiterRow(row));
          }

          Delimiters.Add(obj);
        }

        var weights = await db.AnalysisTableWeights!.ToArrayAsync();
        var weightRows = await db.AnalysisTableWeightRows!.ToArrayAsync();
        foreach (var weight in weights
          .GroupJoin(weightRows, w => w.Id, r => r.WeightId, (w, r) => new { Weight = w, Rows = r, }))
        {
          var obj = new AnalysisTableWeight(weight.Weight);

          foreach (var row in weight.Rows.OrderBy(r => r.Order))
          {
            obj.Rows.Add(new AnalysisTableWeightRow(row));
          }

          Weights.Add(obj);
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

        _isInitialized = true;
      }
    }

    internal static void UpdateParameters(FinderModel finder, IList<FinderQueryParameterItem> list)
    {
      var removes = new List<FinderQueryParameterItem>();
      var index = 0;
      var parameters = finder.Input.ToParameters();
      foreach (var parameter in list)
      {
        if (!parameters.Contains(parameter))
        {
          removes.Add(parameter);
        }
      }
      foreach (var item in removes)
      {
        list.Remove(item);
      }

      foreach (var parameter in parameters)
      {
        if (!list.Contains(parameter))
        {
          list.Insert(index++, parameter);
        }
        else
        {
          index++;
        }
      }
    }
  }
}
