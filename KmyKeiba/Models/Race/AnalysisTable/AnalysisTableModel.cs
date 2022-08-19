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
    private readonly List<RaceFinder> _finders = new();
    private readonly RaceData _race;
    private readonly IReadOnlyList<RaceHorseAnalyzer> _horses;

    public ReactiveCollection<AnalysisTableWeight> Weights => AnalysisTableUtil.Weights;

    public CheckableCollection<AnalysisTableSurface> Tables { get; } = new();

    public ReactiveProperty<AnalysisTableSurface?> ActiveTable => this.Tables.ActiveItem;

    public AnalysisTableConfigModel Config => AnalysisTableConfigModel.Instance;

    public AnalysisTableModel(RaceData race, IReadOnlyList<RaceHorseAnalyzer> horses, AnalysisTableCache? cache)
    {
      if (cache == null)
      {
        foreach (var horse in horses)
        {
          this._finders.Add(new RaceFinder(race, horse.Data));
        }
      }
      else
      {
        foreach (var finder in cache.Finders.Join(horses, f => f.RaceHorse?.Key, h => h.Data.Key, (f, h) => RaceFinder.CopyFrom(f, race, h.Data)))
        {
          this._finders.Add(finder);
        }
      }

      this._race = race;
      this._horses = horses;

      this.InitializeTables();

      Task.Run(() => this.AnalysisTableCacheOnly());
    }

    private void InitializeTables()
    {
      var checkedTableId = this.ActiveTable.Value?.Data.Id;

      foreach (var table in AnalysisTableUtil.TableConfigs.OrderBy(t => t.Order))
      {
        this.Tables.Add(new AnalysisTableSurface(this._race, table, this._horses));
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

    public async Task ReloadTablesAsync()
    {
      using var db = new MyContext();

      this.ClearTables();
      this.InitializeTables();

      foreach (var table in this.Tables)
      {
        await table.AnalysisAsync(db, this._finders, this.Weights, false);
      }
    }

    public async Task AnalysisTablesAsync()
    {
      using var db = new MyContext();

      foreach (var table in this.Tables)
      {
        await table.AnalysisAsync(db, this._finders, this.Weights, false);
      }
    }

    public async Task AnalysisTableAsync(AnalysisTableSurface table)
    {
      using var db = new MyContext();
      await table.AnalysisAsync(db, this._finders, this.Weights, false);
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
      ThreadUtil.InvokeOnUiThread(() =>
      {
        this.Tables.Clear();
      });
      foreach (var table in this.Tables)
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
