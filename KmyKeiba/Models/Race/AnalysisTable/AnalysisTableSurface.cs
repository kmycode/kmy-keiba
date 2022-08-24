﻿using KmyKeiba.Data.Db;
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
    private readonly Dictionary<AnalysisTableRow, IDisposable> _disposableEvents = new();

    public ReactiveProperty<bool> IsChecked { get; } = new();

    public AnalysisTableData Data { get; }

    public RaceData Race { get; }

    public ReactiveProperty<string> Name { get; } = new();

    public ReactiveCollection<AnalysisTableRow> Rows { get; } = new();

    public ReactiveCollection<AnalysisTableRow> ParentRowSelections { get; } = new();

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

      this.Rows.CollectionChangedAsObservable().Subscribe(ev =>
      {
        if (ev.OldItems != null)
        {
          foreach (var item in ev.OldItems.OfType<AnalysisTableRow>())
          {
            if (this._disposableEvents.TryGetValue(item, out var dispose))
            {
              dispose.Dispose();
              this._disposableEvents.Remove(item);
            }
          }
        }
        if (ev.NewItems != null)
        {
          foreach (var item in ev.NewItems.OfType<AnalysisTableRow>())
          {
            if (!this._disposableEvents.ContainsKey(item))
            {
              this._disposableEvents[item] = item.Output.Subscribe(_ => this.UpdateParentRows());
            }
          }
        }
      });

      foreach (var row in AnalysisTableUtil.TableRowConfigs.Where(r => r.TableId == data.Id).OrderBy(r => r.Order))
      {
        var item = new AnalysisTableRow(row, this, horses);
        this.Rows.Add(item);
      }
      this.UpdateParentRows();
    }

    public async Task AnalysisAsync(MyContext db, IReadOnlyList<RaceFinder> finders, IReadOnlyList<AnalysisTableWeight> weights, bool isCacheOnly)
    {
      foreach (var row in this.Rows.OrderBy(r => r.Data.Output == AnalysisTableRowOutputType.Binary ? 0 : 1))
      {
        await row.LoadAsync(this.Race, finders, weights, isCacheOnly);
      }
      this.CanLoadAll.Value = this.Rows.Any(r => !r.IsLoaded.Value);
    }

    private void UpdateParentRows()
    {
      this.ParentRowSelections.Clear();
      foreach (var row in this.Rows.Where(r => r.Output.Value == AnalysisTableRowOutputType.Binary))
      {
        this.ParentRowSelections.Add(row);
      }
      foreach (var row in this.Rows)
      {
        row.OnParentListUpdated();
      }
    }

    public void Dispose()
    {
      this._disposables.Dispose();
      foreach (var row in this.Rows)
      {
        row.Dispose();
      }
      foreach (var item in this._disposableEvents)
      {
        item.Value.Dispose();
      }
    }
  }
}