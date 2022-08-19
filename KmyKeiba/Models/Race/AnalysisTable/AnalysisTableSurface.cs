using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Analysis.Generic;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race.Finder;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.AnalysisTable
{
  public class AnalysisTableSurface : IDisposable, ICheckableItem
  {
    private readonly CompositeDisposable _disposables = new();

    public ReactiveProperty<bool> IsChecked { get; } = new();

    public AnalysisTableData Data { get; }

    public RaceData Race { get; }

    public ReactiveProperty<string> Name { get; } = new();

    public ReactiveCollection<AnalysisTableRow> Rows { get; } = new();

    public ReactiveProperty<bool> CanLoadAll { get; } = new(true);

    public AnalysisTableSurface(RaceData race, AnalysisTableData data, IReadOnlyList<RaceHorseAnalyzer> horses)
    {
      this.Race = race;
      this.Data = data;
      this.Name.Value = data.Name;

      this.Name
        .Skip(1).Subscribe(async _ =>
        {
          using var db = new MyContext();
          db.AnalysisTables!.Attach(this.Data);
          this.Data.Name = this.Name.Value;
          await db.SaveChangesAsync();
        }).AddTo(this._disposables);

      foreach (var row in AnalysisTableUtil.TableRowConfigs.Where(r => r.TableId == data.Id).OrderBy(r => r.Order))
      {
        this.Rows.Add(new AnalysisTableRow(row, horses));
      }
    }

    public async Task AnalysisAsync(MyContext db, IReadOnlyList<RaceFinder> finders, IReadOnlyList<AnalysisTableWeight> weights, bool isCacheOnly)
    {
      foreach (var row in this.Rows)
      {
        await row.LoadAsync(this.Race, finders, weights, isCacheOnly);
      }
      this.CanLoadAll.Value = this.Rows.Any(r => !r.IsLoaded.Value);
    }

    public void Dispose()
    {
      this._disposables.Dispose();
      foreach (var row in this.Rows)
      {
        row.Dispose();
      }
    }
  }
}
