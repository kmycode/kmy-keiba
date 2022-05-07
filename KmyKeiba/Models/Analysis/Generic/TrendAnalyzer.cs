using Reactive.Bindings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis.Generic
{
  public class TrendAnalyzer<KEY> where KEY : Enum, IComparable
  {
    public IList MenuItems => this.MenuItemsPrivate;

    protected TrendAnalyzerMenuItemCollection MenuItemsPrivate { get; } = new();

    protected class TrendAnalyzerMenuItem
    {
      public KEY Key { get; }

      public ReactiveProperty<string> Value { get; } = new();

      public TrendAnalyzerMenuItem(KEY key)
      {
        this.Key = key;
      }
    }

    protected class TrendAnalyzerMenuItemCollection : ReactiveCollection<TrendAnalyzerMenuItem>
    {
      public void SetValue(KEY key, string value)
      {
        var exists = this.FirstOrDefault(i => i.Key.Equals(key));
        if (exists != null)
        {
          exists.Value.Value = value;
        }

        // 順序も考慮した位置に挿入する
        var i = 0;
        for (; i < this.Count; i++)
        {
          if (this[i].Key.CompareTo(key) > 0)
          {
            break;
          }
        }
        try
        {
          this.InsertOnScheduler(i, new TrendAnalyzerMenuItem(key)
          {
            Value = { Value = value, },
          });
        }
        catch
        {

        }
      }

      public void Remove(KEY key)
      {
        var exists = this.FirstOrDefault(i => i.Key.Equals(key));
        if (exists != null)
        {
          this.Remove(exists);
        }
      }
    }
  }
}
