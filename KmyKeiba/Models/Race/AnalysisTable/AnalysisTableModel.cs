using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Analysis.Generic;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race.Finder;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.AnalysisTable
{
  public class AnalysisTableModel : IDisposable
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    private readonly List<RaceFinder> _finders = new();
    private readonly RaceData _race;
    private readonly IReadOnlyList<RaceHorseAnalyzer> _horses;

    public ReactiveCollection<AnalysisTableWeight> Weights => AnalysisTableUtil.Weights;

    public CheckableCollection<AnalysisTableSurface> Tables { get; } = new();

    public ReactiveProperty<AnalysisTableSurface?> ActiveTable => this.Tables.ActiveItem;

    public AnalysisTableAggregater Aggregate { get; }

    public AnalysisTableConfigModel Config => AnalysisTableConfigModel.Instance;

    public AnalysisTableModel(RaceData race, IReadOnlyList<RaceHorseAnalyzer> horses, AnalysisTableCache? cache)
    {
      if (cache == null)
      {
        foreach (var horse in horses)
        {
          this._finders.Add(new RaceFinder(horse));
        }
      }
      else
      {
        foreach (var finder in cache.Finders.Join(horses, f => f.RaceHorse?.Key, h => h.Data.Key, (f, h) => RaceFinder.CopyFrom(f, null, null, h)))
        {
          this._finders.Add(finder);
        }
      }

      this._race = race;
      this._horses = horses;

      this.Aggregate = new AnalysisTableAggregater(this, horses);
      this.InitializeTables();

      if (cache != null)
      {
        Task.Run(() => this.AnalysisTableCacheOnly());
      }
    }

    private void InitializeTables()
    {
      var oldTables = this.Tables.ToList();
      this.Tables.Clear();

      var checkedTableId = this.ActiveTable.Value?.Data.Id;

      foreach (var table in AnalysisTableUtil.TableConfigs.OrderBy(t => t.Order))
      {
        var old = oldTables.FirstOrDefault(t => t.Data.Id == table.Id);
        if (old == null)
        {
          this.Tables.Add(new AnalysisTableSurface(this._race, table, this._horses));
        }
        else
        {
          old.UpdateRows();
          this.Tables.Add(old);
          oldTables.Remove(old);
        }
      }
      foreach (var old in oldTables)
      {
        old.Dispose();
      }

      if (checkedTableId != default)
      {
        var checkedTable = this.Tables.FirstOrDefault(t => t.Data.Id == checkedTableId);
        if (checkedTable != null)
        {
          checkedTable.IsChecked.Value = true;
        }
      }
      else if (this.Tables.Any())
      {
        this.Tables.First().IsChecked.Value = true;
      }
    }

    private void AnalysisTableCacheOnly()
    {
      using var db = new MyContext();

      foreach (var table in this.Tables)
      {
        _ = table.AnalysisAsync(db, this._finders, this.Weights, true);
      }
    }

    public void ReloadTables()
    {
      //this.ClearTables();
      this.InitializeTables();
    }

    public async Task AnalysisTablesAsync()
    {
      using var db = new MyContext();

      foreach (var table in this.Tables)
      {
        await table.AnalysisAsync(db, this._finders, this.Weights, false);
      }
    }

    public async Task AnalysisTableWithReloadAsync(AnalysisTableSurface table)
    {
      table.UpdateRows();
      await this.AnalysisTableAsync(table);
    }

    public async Task AnalysisTableAsync(AnalysisTableSurface table, bool isBulk = false, AggregateRaceFinder? aggregateFinder = null)
    {
      //this.ReloadTables();
      var newTable = this.Tables.FirstOrDefault(t => t.Data.Id == table.Data.Id);
      if (newTable == null)
      {
        return;
      }

      using var db = new MyContext();
      await newTable.AnalysisAsync(db, this._finders, this.Weights, false, isBulk, aggregateFinder);
    }

    public AnalysisTableCache ToCache()
    {
      return new AnalysisTableCache(this._finders);
    }

    public void Dispose()
    {
      foreach (var table in this.Tables)
      {
        table.Dispose();
      }
      this.Tables.Dispose();
    }

    private void ClearTables()
    {
      var tables = this.Tables.ToArray();
      ThreadUtil.InvokeOnUiThread(() =>
      {
        this.Tables.Clear();
      });
      foreach (var table in tables)
      {
        table.Dispose();
      }
    }
  }

  public class AnalysisTableCache
  {
    public IReadOnlyList<RaceFinder> Finders { get; }

    public AnalysisTableCache(IReadOnlyList<RaceFinder> finders)
    {
      this.Finders = finders;
    }
  }
}
