using KmyKeiba.Common;
using KmyKeiba.Models.Data;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis.Generic
{
  public interface ITrendAnalysisSelector
  {
    public IEnumerable Filters { get; }

    public string Name { get; }

    void BeginLoad();
  }

  public abstract class TrendAnalysisSelector<KEY, A> : ITrendAnalysisSelector, IDisposable
    where A : TrendAnalyzer where KEY : Enum, IComparable
  {
    private readonly CompositeDisposable _disposables = new();

    public abstract string Name { get; }

    public IEnumerable Filters => this.Keys;

    public TrendAnalysisFilterItemCollection<KEY> Keys { get; }

    public TrendAnalysisFilterItemCollection<KEY> IgnoreKeys { get; }

    protected Dictionary<IEnumerable<KEY>, A> Analyzers { get; } = new(new TrendAnalysisFilterItemCollection<KEY>.Comparer());

    public ReactiveProperty<A?> CurrentAnalyzer { get; } = new();

    protected virtual bool IsAutoLoad => false;

    public TrendAnalysisSelector()
    {
      this.Keys = new TrendAnalysisFilterItemCollection<KEY>().AddTo(this._disposables);
      this.IgnoreKeys = new TrendAnalysisFilterItemCollection<KEY>().AddTo(this._disposables);
      this.Initialize();
    }

    public TrendAnalysisSelector(IEnumerable<KEY> keys)
    {
      var type = typeof(KEY);
      var ignoreKeys = keys
        .Where(k => type.GetField(k.ToString())!
                       .GetCustomAttributes(true)
                       .OfType<IgnoreKeyAttribute>()
                       .Any());

      this.Keys = new TrendAnalysisFilterItemCollection<KEY>(keys.Except(ignoreKeys)).AddTo(this._disposables);
      this.IgnoreKeys = new TrendAnalysisFilterItemCollection<KEY>(ignoreKeys).AddTo(this._disposables);
      this.Initialize();
    }

    public TrendAnalysisSelector(Type enumType) : this((IEnumerable<KEY>)enumType.GetEnumValues())
    {
    }

    private void Initialize()
    {
      this.Keys.ChangedItemObservable
        .Subscribe(i =>
        {
          this.TryUpdateExistingAnalyzer();
        }).AddTo(this._disposables);
      this.IgnoreKeys.ChangedItemObservable
        .Subscribe(i =>
        {
          this.TryUpdateExistingAnalyzer();
        }).AddTo(this._disposables);
    }

    protected void OnFinishedInitialization()
    {
      // デフォルトのアナライザ
      ThreadUtil.InvokeOnUiThread(() =>
      {
        this.TryUpdateExistingAnalyzer();
      });
    }

    private void TryUpdateExistingAnalyzer()
    {
      var keys = this.Keys.GetActiveKeys().Concat(this.IgnoreKeys.GetActiveKeys()).ToArray();
      this.CurrentAnalyzer.Value = this.GetExistingAnalyzer(keys);

      if (this.IsAutoLoad)
      {
        this.BeginLoad(keys);
      }
    }

    private A GetExistingAnalyzer(IReadOnlyList<KEY> keys)
    {
      //var existsAnalyzer = this.Analyzers.FirstOrDefault(a => a.Key.SequenceEqual(keys)).Value;
      this.Analyzers.TryGetValue(keys, out var existsAnalyzer);
      if (existsAnalyzer != null)
      {
        return existsAnalyzer;
      }

      var analyzer = this.GenerateAnalyzer().AddTo(this._disposables);
      this.Analyzers[keys] = analyzer;
      return analyzer;
    }

    public void BeginLoad()
    {
      var currentKeys = this.Keys.GetActiveKeys().Concat(this.IgnoreKeys.GetActiveKeys()).ToArray();

      this.CurrentAnalyzer.Value = this.BeginLoad(currentKeys, 300, 0);
    }

    public A BeginLoad(IReadOnlyList<KEY> keys, int count, int offset, bool isLoadSameHorses = true)
    {
      // ここはUIスレッドでなければならない（ReactiveCollectionなどにスレッドが伝播しないので）
      var analyzer = this.GetExistingAnalyzer(keys);

      if (analyzer.IsLoaded.Value || analyzer.IsLoading.Value)
      {
        return analyzer;
      }
      analyzer.IsLoading.Value = true;

      _ = Task.Run(async () =>
      {
        // 非同期で解析を開始
        try
        {
          using var db = new MyContext();
          await this.InitializeAnalyzerAsync(db, keys, analyzer, count, offset, isLoadSameHorses);
        }
        catch
        {
          // TODO Log
        }
      });

      return analyzer;
    }

    public A BeginLoad(params KEY[] keys)
    {
      return this.BeginLoad(keys, 300, 0);
    }

    public A BeginLoad(string scriptParams, int count, int offset, bool isLoadSameHorses = true)
    {
      var pairs = Enum.GetValues(typeof(KEY)).OfType<KEY>().Select(k =>
        new { Key = k, ScriptParameter = typeof(KEY).GetField(k.ToString())!.GetCustomAttributes(true).OfType<ScriptParameterKeyAttribute>().FirstOrDefault()?.Key ?? string.Empty, })
        .Where(d => !string.IsNullOrEmpty(d.ScriptParameter));

      var keys = new List<KEY>();
      foreach (var param in scriptParams.Split('|'))
      {
        var key = pairs.FirstOrDefault(p => p.ScriptParameter == param);
        if (key != null)
        {
          keys.Add(key.Key);
        }
      }

      return this.BeginLoad(keys, count, offset, isLoadSameHorses);
    }

    public void CopyFrom(TrendAnalysisSelector<KEY, A> selector)
    {
      this.Analyzers.Clear();

      foreach (var item in selector.Analyzers)
      {
        this.Analyzers.Add(item.Key, item.Value);
      }

      if (this.CurrentAnalyzer.Value != null)
      {
        this.CurrentAnalyzer.Value = null;
        this.TryUpdateExistingAnalyzer();
      }
    }

    protected abstract A GenerateAnalyzer();

    protected virtual Task InitializeAnalyzerAsync(MyContext db, IEnumerable<KEY> keys, A analyzer, int count, int offset, bool isLoadSameHorses)
    {
      return Task.CompletedTask;
    }

    public virtual void Dispose() => this._disposables.Dispose();
  }

  public class TrendAnalysisFilterItemCollection<KEY> : MultipleCheckableCollection<TrendAnalysisFilterItem<KEY>>, IEquatable<TrendAnalysisFilterItemCollection<KEY>>
  {
    public TrendAnalysisFilterItemCollection() : base()
    {
    }

    public TrendAnalysisFilterItemCollection(IEnumerable<KEY> keys) : this()
    {
      foreach (var key in keys)
      {
        var groupName = key!.GetType().GetField(key.ToString()!)!.GetCustomAttributes(true).OfType<GroupNameAttribute>().FirstOrDefault();
        this.Add(new TrendAnalysisFilterItem<KEY>(key, groupName?.GroupName));
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

    public void RemoveKey(KEY key)
    {
      var i = -1;
      var isHit = false;
      foreach (var item in this)
      {
        i++;
        if (item.Key!.Equals(key))
        {
          isHit = true;
          break;
        }
      }

      if (isHit)
      {
        this.RemoveAt(i);
      }
    }

    public IReadOnlyList<KEY> GetActiveKeys()
    {
      return this.Where(i => i.IsChecked.Value).Select(i => i.Key).OrderBy(k => k).ToArray();
    }

    public override bool Equals(object? obj)
    {
      if (obj is TrendAnalysisFilterItemCollection<KEY> collection)
      {
        return this.GetActiveKeys().SequenceEqual(collection.GetActiveKeys());
      }
      if (obj is IEnumerable<KEY> keys)
      {
        return this.GetActiveKeys().SequenceEqual(keys);
      }
      return base.Equals(obj);
    }

    public override int GetHashCode()
    {
      if (!this.Any())
      {
        return 0;
      }
      return this.GetActiveKeys().Sum(i => i!.GetHashCode() % 789);
    }

    public bool Equals(TrendAnalysisFilterItemCollection<KEY>? other) => this.Equals((object?)other);

    public class Comparer : IEqualityComparer<IEnumerable<KEY>>
    {
      public bool Equals(IEnumerable<KEY>? x, IEnumerable<KEY>? y)
      {
        if (x != null && y != null)
        {
          return x.OrderBy(a => a).SequenceEqual(y.OrderBy(b => b));
        }
        return false;
      }

      public int GetHashCode([DisallowNull] IEnumerable<KEY> obj)
      {
        if (!obj.Any())
        {
          return 0;
        }
        return obj.Sum(i => i!.GetHashCode() % 789);
      }
    }
  }

  [AttributeUsage(AttributeTargets.Field)]
  internal class GroupNameAttribute : Attribute
  {
    public string GroupName { get; }

    public GroupNameAttribute(string name)
    {
      this.GroupName = name;
    }
  }

  [AttributeUsage(AttributeTargets.Field)]
  internal class IgnoreKeyAttribute : Attribute
  {
  }

  [AttributeUsage(AttributeTargets.Field)]
  internal class ScriptParameterKeyAttribute : Attribute
  {
    public string Key { get; }

    public ScriptParameterKeyAttribute(string key)
    {
      this.Key = key;
    }
  }

  public record class TrendAnalysisFilterItem<KEY>(KEY Key, string? GroupName) : IMultipleCheckableItem
  {
    public ReactiveProperty<bool> IsChecked { get; } = new();
  }
}
