using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis.Generic
{
  public interface ICheckableItem
  {
    ReactiveProperty<bool> IsChecked { get; }
  }

  public class CheckableCollection<T> : ReactiveCollection<T>, IDisposable where T : class, ICheckableItem
  {
    public ReactiveProperty<T?> ActiveItem { get; } = new();

    private readonly Dictionary<T, IDisposable> _itemEvents = new();

    public CheckableCollection()
    {
      this.CollectionChanged += (_, e) =>
      {
        switch (e.Action)
        {
          case NotifyCollectionChangedAction.Add:
            var items = e.NewItems?.OfType<T>();
            if (items?.Any() == true)
            {
              foreach (var item in items)
              {
                if (item == null) continue;

                this._itemEvents.Add(item, item.IsChecked.Where(i => i).Subscribe(_ =>
                {
                  foreach (var it in this.Where(t => t != item))
                  {
                    it.IsChecked.Value = false;
                  }
                  this.ActiveItem.Value = item;
                }));
              }
            }
            break;
          case NotifyCollectionChangedAction.Remove:
            var items2 = e.OldItems as IEnumerable<T>;
            if (items2 != null)
            {
              foreach (var item in items2)
              {
                if (item == null) continue;

                this._itemEvents.TryGetValue(item, out var disable);
                if (disable != null)
                {
                  disable.Dispose();
                  this._itemEvents.Remove(item);
                }
              }
            }
            break;
          case NotifyCollectionChangedAction.Reset:
            foreach (var item in this._itemEvents)
            {
              item.Value.Dispose();
            }
            this._itemEvents.Clear();
            break;
        }
      };
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
}
