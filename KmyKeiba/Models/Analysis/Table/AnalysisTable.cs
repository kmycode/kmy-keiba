using KmyKeiba.Models.Analysis.Generic;
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
using KmyKeiba.Common;

namespace KmyKeiba.Models.Analysis.Table
{
  public class AnalysisTableList : ReactiveCollection<AnalysisTable>, IDisposable
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

    public new void Dispose()
    {
      base.Dispose();
      this._disposables.Dispose();
      foreach (var table in this)
      {
        table.Dispose();
      }
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
    private readonly CompositeDisposable _disposables = new();

    public string Name { get; }

    public ObservableCollection<RaceHorseAnalyzer> Horses { get; }

    public ObservableCollection<AnalysisTableRow> Rows { get; } = new();

    public ReactiveProperty<bool> IsActive { get; } = new();

    public ReactiveProperty<bool> CanLoadAll { get; } = new(false);

    public AnalysisTable(string name, IReadOnlyList<RaceHorseAnalyzer> horses)
    {
      this.Name = name;
      this.Horses = new ObservableCollection<RaceHorseAnalyzer>(horses);
      this.Rows.ObserveElementObservableProperty(row => row.IsAnalyzed)
        .Subscribe(_ => this.CanLoadAll.Value = this.Rows.Any(r => !r.IsAnalyzed.Value))
        .AddTo(this._disposables);
    }

    public async Task LoadAllAsync()
    {
      if (!this.CanLoadAll.Value)
      {
        return;
      }
      this.CanLoadAll.Value = false;

      foreach (var row in this.Rows)
      {
        await row.LoadAsync();
      }
    }

    public void Dispose()
    {
      this._disposables.Dispose();
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

      try
      {
        await Task.WhenAll(this.Cells.Select(c => c.LoadAsync()));
      }
      catch (TaskCanceledException)
      {
        // TODO: logs
        return;
      }
      catch (ObjectDisposedException)
      {
        return;
      }

      if (this.Cells.Any())
      {
        if (this.Cells.Count() >= 4 && !this.Cells.All(c => c.ComparationValue.Value == float.MinValue))
        {
          var max = this.Cells.OrderByDescending(c => c.ComparationValue.Value).ElementAtOrDefault(2)?.ComparationValue.Value ?? default;
          var min = this.Cells.OrderBy(c => c.ComparationValue.Value).ElementAtOrDefault(2)?.ComparationValue.Value ?? default;
          foreach (var cell in this.Cells)
          {
            cell.Comparation.Value = AnalysisUtil.CompareValue(cell.ComparationValue.Value, max, min);
          }
        }
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
    int SampleSize { set; }

    Func<RaceHorseAnalyzer, bool> SampleFilter { set; }

    ReactiveProperty<string> Value { get; }

    ReactiveProperty<string> SubValue { get; }

    ReactiveProperty<float> ComparationValue { get; }

    ReactiveProperty<ValueComparation> Comparation { get; }

    ReactiveProperty<bool> HasComparationValue { get; }

    ReactiveCollection<RaceHorseAnalyzer> Samples { get; }

    Task LoadAsync();
  }

  public abstract class AnalysisTableCell<S, A> : IAnalysisTableCell
    where A : TrendAnalyzer where S : ITrendAnalysisSelector<A>
  {
    private readonly RaceHorseAnalyzer _horse;

    private readonly Func<RaceHorseAnalyzer, S> _selector;

    private readonly string _keys;

    public int SampleSize { get; set; } = 10;

    public Func<RaceHorseAnalyzer, bool> SampleFilter { get; set; } = _ => true;

    public ReactiveProperty<A?> Analyzer { get; } = new();

    public ReactiveProperty<string> Value { get; } = new();

    public ReactiveProperty<string> SubValue { get; } = new();

    public ReactiveProperty<float> ComparationValue { get; } = new(float.MinValue);

    public ReactiveProperty<bool> HasComparationValue { get; } = new(true);

    public ReactiveProperty<ValueComparation> Comparation { get; } = new();

    public ReactiveCollection<RaceHorseAnalyzer> Samples { get; } = new();

    public bool IsSamplesEnabled => this.Samples.Any();

    public AnalysisTableCell(RaceHorseAnalyzer horse, Func<RaceHorseAnalyzer, S> selector, string keys)
    {
      this._horse = horse;
      this._selector = selector;
      this._keys = keys;
    }

    public async Task LoadAsync()
    {
      if (this._horse.IsDisposed)
      {
        throw new TaskCanceledException("horse");
      }

      var count = ApplicationConfiguration.Current.Value.AnalysisTableSourceSize;
      if (typeof(S) == typeof(RaceWinnerHorseTrendAnalysisSelector))
      {
        count = ApplicationConfiguration.Current.Value.AnalysisTableRaceHorseSourceSize;
      }

      var selector = this._selector(this._horse);
      var analyzer = selector.BeginLoad(this._keys, count, 0, false);
      await analyzer.WaitAnalysisAsync();

      this.Analyzer.Value = analyzer;

      this.AfterLoad(analyzer);

      if (analyzer is RaceHorseTrendAnalyzerBase rha)
      {
        ThreadUtil.InvokeOnUiThread(() =>
        {
          foreach (var sample in rha.Source
            .Where(this.SampleFilter)
            .Where(s => s.Data.ResultOrder > 0)
            .Take(this.SampleSize))
          {
            this.Samples.Add(sample);
          }
        });
      }
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
