﻿using KmyKeiba.Models.Analysis.Generic;
using KmyKeiba.Models.Race;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using Reactive.Bindings.Extensions;

namespace KmyKeiba.Models.Analysis.Table
{
  public class AnalysisTableList : ReactiveCollection<AnalysisTable>
  {
    private readonly CompositeDisposable _disposables = new();

    public ReactiveProperty<AnalysisTable> ActiveTable { get; } = new();

    public ReactiveProperty<bool> HasItems { get; } = new();

    public AnalysisTableList()
    {
      this.ObserveAddChanged().Subscribe(item =>
      {
        item.IsActive.Where(value => value).Subscribe(value =>
        {
          this.ActiveTable.Value = item;
        }).AddTo(this._disposables);

        this.HasItems.Value = true;
      }).AddTo(this._disposables);
    }

    // 以下はRemoveItemObservable対応してないので保険で

    public new void Remove(AnalysisTable item)
    {
      throw new NotSupportedException();
    }

    public new void RemoveAt(int index)
    {
      throw new NotSupportedException();
    }

    public new void RemoveItem(int index)
    {
      throw new NotSupportedException();
    }

    public new void Clear()
    {
      throw new NotSupportedException();
    }
  }

  public class AnalysisTable
  {
    public string Name { get; }

    public ObservableCollection<RaceHorseAnalyzer> Horses { get; }

    public ObservableCollection<AnalysisTableRow> Rows { get; } = new();

    public ReactiveProperty<bool> IsActive { get; } = new();

    public AnalysisTable(string name, IReadOnlyList<RaceHorseAnalyzer> horses)
    {
      this.Name = name;
      this.Horses = new ObservableCollection<RaceHorseAnalyzer>(horses);
    }
  }

  public class AnalysisTableRow
  {
    private readonly RaceInfo _race;

    public string Name { get; }

    public ObservableCollection<IAnalysisTableCell> Cells { get; }

    public ReactiveProperty<bool> IsAnalyzed { get; } = new();

    public AnalysisTableRow(string name, RaceInfo race, Func<RaceInfo, RaceHorseAnalyzer, IAnalysisTableCell> cell)
    {
      this.Name = name;
      this._race = race;

      this.Cells = new();
      foreach (var horse in race.Horses)
      {
        this.Cells.Add(cell(this._race, horse));
      };
    }

    public async Task LoadAsync()
    {
      if (this.IsAnalyzed.Value)
      {
        return;
      }
      this.IsAnalyzed.Value = true;

      foreach (var cell in this.Cells)
      {
        await cell.LoadAsync();
      }
    }

    public async Task<AnalysisTableRow> WithLoadAsync()
    {
      await this.LoadAsync();
      return this;
    }
  }

  public interface IAnalysisTableCell
  {
    ReactiveProperty<string> Value { get; }

    ReactiveProperty<ValueComparation> Comparation { get; }

    Task LoadAsync();
  }

  public abstract class AnalysisTableCell<S, A> : IAnalysisTableCell
    where A : TrendAnalyzer where S : ITrendAnalysisSelector<A>
  {
    private readonly RaceHorseAnalyzer _horse;

    private readonly Func<RaceHorseAnalyzer, S> _selector;

    private readonly string _keys;

    public ReactiveProperty<A?> Analyzer { get; } = new();

    public ReactiveProperty<string> Value { get; } = new();

    public ReactiveProperty<ValueComparation> Comparation { get; } = new();

    public AnalysisTableCell(RaceHorseAnalyzer horse, Func<RaceHorseAnalyzer, S> selector, string keys)
    {
      this._horse = horse;
      this._selector = selector;
      this._keys = keys;
    }

    public async Task LoadAsync()
    {
      var selector = this._selector(this._horse);
      var analyzer = selector.BeginLoadByScript(this._keys, 1000, 0, false);
      await analyzer.WaitAnalysisAsync();

      this.Analyzer.Value = analyzer;

      this.AfterLoad(analyzer);
    }

    protected abstract void AfterLoad(A analyzer);
  }

  public class LambdaAnalysisTableCell<S, A> : AnalysisTableCell<S, A>
    where A : TrendAnalyzer where S : ITrendAnalysisSelector<A>
  {
    private Action<A, AnalysisTableCell<S, A>> _afterLoad;

    public LambdaAnalysisTableCell(RaceHorseAnalyzer horse, Func<RaceHorseAnalyzer, S> selector, string keys, Action<A, AnalysisTableCell<S, A>> afterLoad) : base(horse, selector, keys)
    {
      this._afterLoad = afterLoad;
    }

    protected override void AfterLoad(A analyzer)
    {
      this._afterLoad(analyzer, this);
    }
  }
}
