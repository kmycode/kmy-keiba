using KmyKeiba.Models.Data;
using Reactive.Bindings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis.Generic
{
  public interface ITrendAnalysisSelector
  {
    public IEnumerable Filters { get; }

    void BeginLoad();
  }

  public abstract class TrendAnalysisSelector<KEY, KEY2, A> : ITrendAnalysisSelector
    where A : TrendAnalyzer<KEY2> where KEY : Enum, IComparable where KEY2 : Enum, IComparable
  {
    private readonly MyContext _db;

    public IEnumerable Filters => this.Keys;

    protected TrendAnalysisFilterItemCollection<KEY> Keys { get; }

    protected Dictionary<TrendAnalysisFilterItemCollection<KEY>, A> Analyzers { get; } = new();

    public ReactiveProperty<A?> CurrentAnalyzer { get; } = new();

    public TrendAnalysisSelector(MyContext db)
    {
      this._db = db;
      this.Keys = new TrendAnalysisFilterItemCollection<KEY>();
    }

    public TrendAnalysisSelector(MyContext db, IEnumerable<KEY> keys)
    {
      this._db = db;
      this.Keys = new TrendAnalysisFilterItemCollection<KEY>(keys);
    }

    public TrendAnalysisSelector(MyContext db, Type enumType) : this(db, (IEnumerable<KEY>)enumType.GetEnumValues())
    {
    }

    public void BeginLoad()
    {
      var db = this._db;

      // なぜかTrendAnalysisFilterItemCollectionクラスのEqualsメソッドが呼び出されない。なぜ？
      //this.Analyzers.TryGetValue(this.Keys, out var existsAnalyzer);
      foreach (var pair in this.Analyzers)
      {
        if (pair.Key.Equals(this.Keys))
        {
          this.CurrentAnalyzer.Value = pair.Value;
          return;
        }
      }

      // ここはUIスレッドでなければならない（ReactiveCollectionなどにスレッドが伝播しないので）
      var analyzer = this.GenerateAnalyzer();
      this.Analyzers[this.Keys.Clone()] = analyzer;
      this.CurrentAnalyzer.Value = analyzer;

      Task.Run(async () =>
      {
        // 非同期で解析を開始
        await this.InitializeAnalyzerAsync(db, analyzer);
      });
    }

    protected abstract A GenerateAnalyzer();

    protected virtual Task InitializeAnalyzerAsync(MyContext db, A analyzer)
    {
      return Task.CompletedTask;
    }
  }

  public class TrendAnalysisFilterItemCollection<KEY> : ObservableCollection<TrendAnalysisFilterItem<KEY>>, IEquatable<TrendAnalysisFilterItemCollection<KEY>>
  {
    public TrendAnalysisFilterItemCollection() : base()
    {
    }

    public TrendAnalysisFilterItemCollection(IEnumerable<KEY> keys) : this()
    {
      foreach (var key in keys)
      {
        this.Add(new TrendAnalysisFilterItem<KEY>(key));
      }
    }

    public bool IsChecked(KEY key)
    {
      var item = this.Where(i => i.Key?.Equals(key) ?? false);
      if (item.Any())
      {
        return item.First().IsChecked.Value;
      }
      return false;
    }

    public TrendAnalysisFilterItemCollection<KEY> Clone()
    {
      var collection = new TrendAnalysisFilterItemCollection<KEY>();

      foreach (var item in this)
      {
        var newItem = new TrendAnalysisFilterItem<KEY>(item.Key)
        {
          IsChecked = { Value = item.IsChecked.Value, },
        };
        collection.Add(newItem);
      }

      return collection;
    }

    public override bool Equals(object? obj)
    {
      if (obj is TrendAnalysisFilterItemCollection<KEY> collection)
      {
        var result = this.GroupJoin(collection,
          self => self.Key,
          col => col.Key,
          (self, col) => col.Any() && col.First().IsChecked.Value == self.IsChecked.Value);
        return result.All(r => r);
      }
      return base.Equals(obj);
    }

    public override int GetHashCode()
    {
      if (!this.Any())
      {
        return 0;
      }
      return this.Sum(i => i.GetHashCode() % 789);
    }

    public bool Equals(TrendAnalysisFilterItemCollection<KEY>? other) => this.Equals((object?)other);
  }

  public record struct TrendAnalysisFilterItem<KEY>(KEY Key)
  {
    public ReactiveProperty<bool> IsChecked { get; } = new();
  }
}
