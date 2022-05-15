using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis.Generic
{
  public class TrendAnalyzer : IDisposable
  {
    private readonly CompositeDisposable _disposables = new();
    private readonly List<Action> _postAnalysis = new();

    public ReactiveProperty<bool> IsLoaded { get; } = new();

    public ReactiveProperty<bool> IsAnalyzed { get; } = new();

    protected TrendAnalyzer()
    {
      // 選択されているメニューが変更された時の処理
      // NOTE: 今後の開発で、旧UIが不要と判断したら削除
      // https://github.com/kmycode/kmy-keiba/issues/5
      /*
      this.MenuItemsPrivate.ActiveKey.Subscribe(k =>
      {
        foreach (var parameter in this.Parameters)
        {
          parameter.IsActive.Value = parameter.Key.Equals(k);
        }
      }).AddTo(this._disposables);
      */

      this.IsAnalyzed.Subscribe(a =>
      {
        foreach (var action in this._postAnalysis)
        {
          action();
        }
        this._postAnalysis.Clear();
      });
    }

    public void PostAnalysis(Action action)
    {
      if (this.IsAnalyzed.Value)
      {
        action();
      }
      else
      {
        this._postAnalysis.Add(action);
      }
    }

    public async Task WaitAnalysisAsync()
    {
      while (!this.IsAnalyzed.Value)
      {
        await Task.Delay(10);
      }
    }

    public virtual void Dispose()
    {
      this._disposables.Dispose();

      // メモリリーク防止
      this._postAnalysis.Clear();
    }

    // NOTE: 今後の開発で、旧UIが不要と判断したら削除
    // https://github.com/kmycode/kmy-keiba/issues/5
    // ただし TrendAnalyzerMenuItemCollection クラスは
    // 「コレクションに入っている各アイテムのフラグがラジオボタンのチェック状態と連動する」
    // 「どのアイテムがラジオボタンによって選択されているか把握する、チェック変更のイベントを取得する」
    // という観点から再利用できる可能性がある
    #region Obsoletes

    /*
    [Obsolete]
    public IList MenuItems => this.MenuItemsPrivate;

    [Obsolete]
    protected TrendAnalyzerMenuItemCollection MenuItemsPrivate { get; } = new();

    [Obsolete]
    public ReactiveCollection<AnalysisParameter> Parameters { get; } = new();

    [Obsolete]
    protected class TrendAnalyzerMenuItem
    {
      public KEY Key { get; }

      public ReactiveProperty<bool> IsChecked { get; } = new();

      public ReactiveProperty<string> Value { get; } = new();

      public TrendAnalyzerMenuItem(KEY key)
      {
        this.Key = key;
      }
    }

    [Obsolete]
    protected class TrendAnalyzerMenuItemCollection : ReactiveCollection<TrendAnalyzerMenuItem>, IDisposable
    {
      public ReactiveProperty<KEY> ActiveKey { get; } = new();

      private readonly Dictionary<KEY, IDisposable> _itemEvents = new();

      public void AddValues(IEnumerable<(KEY Key, string Value)> pairs)
      {
        var existKeys = new List<KEY>();

        foreach (var item in pairs)
        {
          var exists = this.FirstOrDefault(i => i.Key.Equals(item.Key));
          if (exists != null)
          {
            exists.Value.Value = item.Value;
            existKeys.Add(item.Key);
          }
        }

        var newItems = new List<TrendAnalyzerMenuItem>();
        foreach (var item in pairs.Where(i => !existKeys.Any(ek => ek.Equals(i.Key))))
        {
          var newItem = new TrendAnalyzerMenuItem(item.Key)
          {
            Value = { Value = item.Value, },
          };
          newItems.Add(newItem);

          // アイテムのチェック状態の変更をつなぐ
          this._itemEvents[item.Key] = newItem.IsChecked
            .Where(c => c).Subscribe(_ =>
            {
            // チェックは１つだけ
            foreach (var oldItems in this.Where(i => !i.Key.Equals(item.Key)))
              {
                oldItems.IsChecked.Value = false;
              }

              this.ActiveKey.Value = item.Key;
            });
        }

        // Schedulerは非同期で実行されるので、要素の順番にこだわるのならInsertではなく一括追加する必要がある
        this.AddRangeOnScheduler(newItems);
      }

      public void Remove(KEY key)
      {
        var exists = this.FirstOrDefault(i => i.Key.Equals(key));
        if (exists != null)
        {
          this.RemoveOnScheduler(exists);
          this._itemEvents.Remove(key);
        }
      }

      public new void Dispose()
      {
        base.Dispose();

        foreach (var item in this._itemEvents)
        {
          item.Value.Dispose();
        }
        this._itemEvents.Clear();
      }
    }

    [Obsolete]
    public record class AnalysisParameter(KEY Key, string Name, string Value, string Comment, AnalysisParameterType Type)
    {
      public ReactiveProperty<bool> IsActive { get; } = new();
    }

    [Obsolete]
    public enum AnalysisParameterType
    {
      Unset,
      VeryHigh,
      High,
      Standard,
      Low,
      VeryLow,
    }

    */
    #endregion
  }
}
