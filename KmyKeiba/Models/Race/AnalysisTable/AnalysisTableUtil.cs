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

    public static List<AnalysisTableRowOutputItem> RowOutputItems { get; } = new();

    public static List<AnalysisTableRowOutputItem> RowOutputSubItems { get; } = new();

    public static List<AnalysisTableRowOutputItem> RowOutputJrdbItems { get; } = new();

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

        RowOutputItems.Add(new AnalysisTableRowOutputItem
        {
          Label = "複勝率",
          OutputType = AnalysisTableRowOutputType.PlaceBetsRate,
        });
        RowOutputItems.Add(new AnalysisTableRowOutputItem
        {
          Label = "勝率",
          OutputType = AnalysisTableRowOutputType.WinRate,
        });
        RowOutputItems.Add(new AnalysisTableRowOutputItem
        {
          Label = "単勝回収率",
          OutputType = AnalysisTableRowOutputType.RecoveryRate,
        });
        RowOutputItems.Add(new AnalysisTableRowOutputItem
        {
          Label = "持ちタイム",
          OutputType = AnalysisTableRowOutputType.ShortestTime,
        });
        RowOutputItems.Add(new AnalysisTableRowOutputItem
        {
          Label = "タイム偏差値",
          OutputType = AnalysisTableRowOutputType.Time,
          CanApplyWeight = false,
        });
        RowOutputItems.Add(new AnalysisTableRowOutputItem
        {
          Label = "後３ハロンタイム偏差値",
          OutputType = AnalysisTableRowOutputType.A3HTime,
          CanApplyWeight = false,
        });
        RowOutputItems.Add(new AnalysisTableRowOutputItem
        {
          Label = "Until後３ハロンタイム偏差値",
          OutputType = AnalysisTableRowOutputType.UA3HTime,
          CanApplyWeight = false,
        });
        RowOutputItems.Add(new AnalysisTableRowOutputItem
        {
          Label = "ブーリアン",
          OutputType = AnalysisTableRowOutputType.Binary,
        });
        RowOutputItems.Add(new AnalysisTableRowOutputItem
        {
          Label = "現在のレースを重みで評価",
          OutputType = AnalysisTableRowOutputType.FixedValue,
        });
        RowOutputItems.Add(new AnalysisTableRowOutputItem
        {
          Label = "過去レース一覧を重みで評価",
          OutputType = AnalysisTableRowOutputType.FixedValuePerPastRace,
        });
        RowOutputItems.Add(new AnalysisTableRowOutputItem
        {
          Label = "拡張メモ",
          OutputType = AnalysisTableRowOutputType.ExpansionMemo,
          CanApplyWeight = false,
        });
        RowOutputItems.Add(new AnalysisTableRowOutputItem
        {
          Label = "外部指数",
          OutputType = AnalysisTableRowOutputType.ExternalNumber,
          CanApplyWeight = false,
        });
        RowOutputItems.Add(new AnalysisTableRowOutputItem
        {
          Label = "その他（JRDB）",
          OutputType = AnalysisTableRowOutputType.JrdbValues,
        });
        RowOutputItems.Add(new AnalysisTableRowOutputItem
        {
          Label = "その他（馬の値）",
          OutputType = AnalysisTableRowOutputType.HorseValues,
        });

        RowOutputSubItems.Add(new AnalysisTableRowOutputItem
        {
          Label = "斤量",
          OutputType = AnalysisTableRowOutputType.RiderWeight,
        });
        RowOutputSubItems.Add(new AnalysisTableRowOutputItem
        {
          Label = "斤量から55を引いた値",
          OutputType = AnalysisTableRowOutputType.RiderWeightDiff,
        });
        RowOutputSubItems.Add(new AnalysisTableRowOutputItem
        {
          Label = "年齢",
          OutputType = AnalysisTableRowOutputType.Age,
        });
        RowOutputSubItems.Add(new AnalysisTableRowOutputItem
        {
          Label = "単勝オッズ",
          OutputType = AnalysisTableRowOutputType.Odds,
        });

        RowOutputJrdbItems.Add(new AnalysisTableRowOutputItem
        {
          Label = "騎手指数",
          OutputType = AnalysisTableRowOutputType.RiderPoint,
        });
        RowOutputJrdbItems.Add(new AnalysisTableRowOutputItem
        {
          Label = "情報指数",
          OutputType = AnalysisTableRowOutputType.InfoPoint,
        });
        RowOutputJrdbItems.Add(new AnalysisTableRowOutputItem
        {
          Label = "総合指数",
          OutputType = AnalysisTableRowOutputType.TotalPoint,
        });

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

  public class AnalysisTableRowOutputItem
  {
    public string Label { get; init; } = string.Empty;

    public bool CanApplyWeight { get; init; } = true;   // BaseWeightは全項目に適用可能

    public AnalysisTableRowOutputType OutputType { get; init; }
  }
}
