using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis.Generic
{
  public class ObservableItemCollection<T> : ReactiveCollection<T>
  {
    public Subject<T> NewItemObservable { get; } = new();

    public Subject<T> OldItemObservable { get; } = new();

    public ObservableItemCollection()
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
                this.NewItemObservable.OnNext(item);
              }
            }
            break;
          case NotifyCollectionChangedAction.Remove:
            if (e.OldItems is IEnumerable<T> items2)
            {
              foreach (var item in items2)
              {
                if (item == null) continue;
                this.OldItemObservable.OnNext(item);
              }
            }
            break;
          case NotifyCollectionChangedAction.Reset:
            if (e.OldItems is IEnumerable<T> items3)
            {
              foreach (var item in items3)
              {
                if (item == null) continue;
                this.OldItemObservable.OnNext(item);
              }
            }
            break;
        }
      };
    }
  }
}
