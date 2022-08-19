using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Analysis.Generic;
using KmyKeiba.Models.Data;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.AnalysisTable
{
  public class AnalysisTableConfigModel
  {
    public static AnalysisTableConfigModel Instance => _instance ??= new AnalysisTableConfigModel();
    private static AnalysisTableConfigModel? _instance;

    public CheckableCollection<AnalysisTableSurface> Tables { get; } = new();

    public ReactiveProperty<AnalysisTableSurface?> ActiveTable => this.Tables.ActiveItem;

    private AnalysisTableConfigModel()
    {
      this.InitializeTables();
    }

    private void InitializeTables()
    {
      foreach (var table in AnalysisTableUtil.TableConfigs.OrderBy(t => t.Order))
      {
        this.Tables.Add(new AnalysisTableSurface(new RaceData(), table, Array.Empty<RaceHorseAnalyzer>()));
      }

      if (this.Tables.Any())
      {
        this.Tables.First().IsChecked.Value = true;
      }
    }

    public async Task AddTableAsync()
    {
      using var db = new MyContext();

      var data = new AnalysisTableData();
      await db.AnalysisTables!.AddAsync(data);
      await db.SaveChangesAsync();

      data.Order = data.Id;
      await db.SaveChangesAsync();

      AnalysisTableUtil.TableConfigs.Add(data);
      ThreadUtil.InvokeOnUiThread(() =>
      {
        this.Tables.Add(new AnalysisTableSurface(new RaceData(), data, Array.Empty<RaceHorseAnalyzer>()));
      });
    }

    public async Task RemoveTableAsync(AnalysisTableSurface table)
    {
      if (!this.Tables.Contains(table))
      {
        return;
      }

      ThreadUtil.InvokeOnUiThread(() =>
      {
        this.Tables.Remove(table);
      });

      using var db = new MyContext();

      db.AnalysisTables!.Remove(table.Data);
      await db.SaveChangesAsync();
      AnalysisTableUtil.TableConfigs.Remove(table.Data);
    }

    public async Task AddTableRowAsync(AnalysisTableSurface table)
    {
      using var db = new MyContext();

      var row = new AnalysisTableRowData
      {
        TableId = table.Data.Id,
      };
      await db.AnalysisTableRows!.AddAsync(row);
      await db.SaveChangesAsync();

      row.Order = row.Id;
      await db.SaveChangesAsync();

      AnalysisTableUtil.TableRowConfigs.Add(row);
    }

    public async Task RemoveTableRowAsync(AnalysisTableRow row)
    {
      var table = this.Tables.FirstOrDefault(t => t.Data.Id == row.Data.TableId);
      if (table == null || !table.Rows.Contains(row))
      {
        return;
      }

      ThreadUtil.InvokeOnUiThread(() =>
      {
        table.Rows.Remove(row);
      });

      using var db = new MyContext();

      db.AnalysisTableRows!.Remove(row.Data);
      await db.SaveChangesAsync();
      AnalysisTableUtil.TableRowConfigs.Remove(row.Data);
    }
  }
}
