﻿using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
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
    IEnumerable Filters { get; }

    string Name { get; }

    ReactiveProperty<bool> IsError { get; }

    ReactiveProperty<int> SizeMax { get; }

    ReactiveProperty<string> SizeMaxInput { get; }

    ReactiveProperty<bool> CanAnalysis { get; }

    void BeginLoad();
  }

  public interface ITrendAnalysisSelector<A> : ITrendAnalysisSelector where A : TrendAnalyzer
  {
    A BeginLoad(string scriptParams, int count, int offset, bool isLoadSameHorses = true);

    A BeginLoadByScript(string scriptParams, int count, int offset, bool isLoadSameHorses = true);

    void CopyFrom(ITrendAnalysisSelector<A> selector);

    void CopyFrom(ITrendAnalysisSelector<A> selector, RaceUpdateType updateType);
  }

  internal static class TrendAnalysisSelector
  {
    public static ReactiveProperty<int> SizeMax { get; } = new(1000);
    public static ReactiveProperty<string> SizeMaxInput { get; } = new(SizeMax.Value.ToString());

    static TrendAnalysisSelector()
    {
      SizeMaxInput.Subscribe(s =>
      {
        int.TryParse(s, out var sizeMax);
        SizeMax.Value = sizeMax;
      });
    }
  }

  public abstract class TrendAnalysisSelector<KEY, A> : ITrendAnalysisSelector<A>, IDisposable
    where A : TrendAnalyzer where KEY : Enum, IComparable
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    private readonly CompositeDisposable _disposables = new();

    public abstract string Name { get; }

    public IEnumerable Filters => this.Keys;

    public TrendAnalysisFilterItemCollection<KEY> Keys { get; }

    public TrendAnalysisFilterItemCollection<KEY> IgnoreKeys { get; }

    protected Dictionary<IEnumerable<KEY>, A> Analyzers { get; } = new(new TrendAnalysisFilterItemCollection<KEY>.Comparer());

    public ReactiveProperty<A?> CurrentAnalyzer { get; } = new();

    public ReactiveProperty<bool> IsError { get; } = new();

    public ReactiveProperty<int> SizeMax => TrendAnalysisSelector.SizeMax;

    public ReactiveProperty<string> SizeMaxInput => TrendAnalysisSelector.SizeMaxInput;

    public ReactiveProperty<bool> CanAnalysis { get; }

    public ReactiveProperty<bool> IsSizeChanged { get; }

    protected virtual bool IsAutoLoad => false;

    public abstract RaceData Race { get; }

    public TrendAnalysisSelector(): this(Enumerable.Empty<KEY>())
    {
    }

    public TrendAnalysisSelector(IEnumerable<KEY> keys)
    {
      this.CanAnalysis = this.SizeMax
        .Select(s => s > 0)
        .ToReactiveProperty()
        .AddTo(this._disposables);
      this.IsSizeChanged = this.SizeMax
        .CombineLatest(this.CurrentAnalyzer, (sizeMax, current) => (current?.SizeMax ?? sizeMax) < sizeMax)
        .ToReactiveProperty()
        .AddTo(this._disposables);

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
      // 傾向画面開いたときに最初に表示される、デフォルトのアナライザ
      ThreadUtil.InvokeOnUiThread(() =>
      {
        this.TryUpdateExistingAnalyzer();
      });
    }

    private void TryUpdateExistingAnalyzer()
    {
      var keys = this.Keys.GetActiveKeys().Concat(this.IgnoreKeys.GetActiveKeys()).ToArray();
      this.CurrentAnalyzer.Value = this.GetExistingAnalyzer(keys, this.SizeMax.Value);

      if (this.IsAutoLoad)
      {
        this.BeginLoad(keys);
      }
    }

    private A GetExistingAnalyzer(IReadOnlyList<KEY> keys, int count, bool isSandbox = false)
    {
      if (!isSandbox)
      {
        //var existsAnalyzer = this.Analyzers.FirstOrDefault(a => a.Key.SequenceEqual(keys)).Value;
        this.Analyzers.TryGetValue(keys, out var existsAnalyzer);
        if (existsAnalyzer != null)
        {
          if (existsAnalyzer.SizeMax >= this.SizeMax.Value)
          {
            return existsAnalyzer;
          }
          else
          {
            // 取得件数を変更して最初から取り直す
            this.Analyzers.Remove(keys);
            this._disposables.Remove(existsAnalyzer);
            existsAnalyzer.Dispose();
          }
        }

        var analyzer = this.GenerateAnalyzer(count).AddTo(this._disposables);
        this.Analyzers[keys] = analyzer;
        return analyzer;
      }
      else
      {
        // スクリプト用のサンドボックス
        var analyzer = this.GenerateAnalyzer(count).AddTo(this._disposables);
        return analyzer;
      }
    }

    public void BeginLoad()
    {
      var currentKeys = this.Keys.GetActiveKeys().Concat(this.IgnoreKeys.GetActiveKeys()).ToArray();

      this.CurrentAnalyzer.Value = this.BeginLoad(currentKeys, this.SizeMax.Value, 0);
    }

    public A BeginLoad(IReadOnlyList<KEY> keys, int count, int offset, bool isLoadSameHorses = true, bool isSandbox = false)
    {
      this.IsError.Value = false;

      // ここはUIスレッドでなければならない（ReactiveCollectionなどにスレッドが伝播しないので）
      var analyzer = this.GetExistingAnalyzer(keys, count, isSandbox);

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
        catch (Exception ex)
        {
          logger.Error($"解析中にエラー　キー: {string.Join(',', keys)}, アナライザ: {analyzer.GetType().Name}, count: {count}, offset: {offset}, isLoadSameHorses: {isLoadSameHorses}", ex);
          this.IsError.Value = true;
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
      return this.BeginLoadWithScriptParameters(scriptParams, count, offset, isLoadSameHorses, isSandbox: false);
    }

    public A BeginLoadByScript(string scriptParams, int count, int offset, bool isLoadSameHorses = true)
    {
      return this.BeginLoadWithScriptParameters(scriptParams, count, offset, isLoadSameHorses,
        isSandbox: offset != default);
    }

    private A BeginLoadWithScriptParameters(string scriptParams, int count, int offset, bool isLoadSameHorses = true, bool isSandbox = true)
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

      return this.BeginLoad(keys, count, offset, isLoadSameHorses, isSandbox: isSandbox);
    }

    void ITrendAnalysisSelector<A>.CopyFrom(ITrendAnalysisSelector<A> selector)
      => this.CopyFrom((TrendAnalysisSelector<KEY, A>)selector, RaceUpdateType.None);

    void ITrendAnalysisSelector<A>.CopyFrom(ITrendAnalysisSelector<A> selector, RaceUpdateType updateType)
      => this.CopyFrom((TrendAnalysisSelector<KEY, A>)selector, RaceUpdateType.None);

    public void CopyFrom(TrendAnalysisSelector<KEY, A> selector)
      => this.CopyFrom(selector, RaceUpdateType.None);

    public void CopyFrom(TrendAnalysisSelector<KEY, A> selector, RaceUpdateType updateType)
    {
      this.Analyzers.Clear();
      var updateTypes = Enum.GetValues(typeof(RaceUpdateType)).OfType<RaceUpdateType>().Skip(1).ToArray();

      foreach (var item in selector.Analyzers)
      {
        if (item.Key
          .Any(k => k.GetType()
            .GetField(k.ToString())!
            .GetCustomAttributes(true)
            .OfType<NotCacheKeyUntilRaceAttribute>()
            .Any()
          ))
        {
          if (this.Race.DataStatus < RaceDataStatus.PreliminaryGradeFull)
          {
            continue;
          }
        }

        var keyUpdateType = item.Key
          .SelectMany(k => k.GetType()
            .GetField(k.ToString())!
            .GetCustomAttributes(true)
            .OfType<NotCacheAttribute>()
            .Select(a => a.UpdateType)
          )
          .Aggregate(RaceUpdateType.None, (a, b) => a | b);
        if (updateTypes.Any(t => updateType.HasFlag(t) && keyUpdateType.HasFlag(t)))
        {
          continue;
        }

        this.Analyzers[item.Key] = item.Value;
      }

      foreach (var key in this.Keys
        .Concat(this.IgnoreKeys)
        .Join(selector.Keys.Concat(selector.IgnoreKeys), k => k.Key, k => k.Key, (n, o) => new { New = n, Old = o, }))
      {
        key.New.IsChecked.Value = key.Old.IsChecked.Value;
      }

      if (this.CurrentAnalyzer.Value != null)
      {
        this.CurrentAnalyzer.Value = null;
        this.TryUpdateExistingAnalyzer();
      }
    }

    protected abstract A GenerateAnalyzer(int sizeMax);

    protected virtual Task InitializeAnalyzerAsync(MyContext db, IEnumerable<KEY> keys, A analyzer, int sizeMax, int offset, bool isLoadSameHorses)
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

    public bool SetChecked(KEY key, bool isChecked)
    {
      var item = this.Where(i => i.Key?.Equals(key) ?? false);
      if (item.Any())
      {
        item.First().IsChecked.Value = isChecked;
        return true;
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

  internal class NotCacheKeyUntilRaceAttribute : Attribute
  {
  }

  internal class NotCacheAttribute : Attribute
  {
    public RaceUpdateType UpdateType { get; }

    public NotCacheAttribute(RaceUpdateType updateType)
    {
      this.UpdateType = updateType;
    }
  }

  public record class TrendAnalysisFilterItem<KEY>(KEY Key, string? GroupName) : IMultipleCheckableItem
  {
    public ReactiveProperty<bool> IsChecked { get; } = new();
  }

  [Flags]
  public enum RaceUpdateType
  {
    None = 0,
    Weather = 1,
    Condition = 2,
    Distance = 4,
    Ground = 8,
    Odds = 16,
    Option = 32,
    CornerDirection = 64,
    Subject = 128,
    All = int.MaxValue,
  }
}
