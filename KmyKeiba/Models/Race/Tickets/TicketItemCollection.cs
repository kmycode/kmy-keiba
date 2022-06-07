using KmyKeiba.Models.Analysis.Generic;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.Tickets
{
  public class TicketItemCollection : ObservableItemCollection<TicketItem>, IDisposable
  {
    private readonly CompositeDisposable _disposables = new();
    private readonly Dictionary<TicketItem, List<IDisposable>> _disposableItems = new();

    public TicketItemCollection()
    {
      this.NewItemObservable.Subscribe(i =>
      {
        this._disposableItems.TryGetValue(i, out var list);
        if (list == null)
        {
          list = new();
          this._disposableItems.Add(i, list);
        }

        var d = i.Count.Subscribe(c =>
        {
          this.TicketCountChanged?.Invoke(this, EventArgs.Empty);
        });
        list.Add(d);
      }).AddTo(this._disposables);

      this.OldItemObservable.Subscribe(i =>
      {
        this._disposableItems.TryGetValue(i, out var list);
        if (list != null)
        {
          foreach (var disposable in list)
          {
            disposable.Dispose();
          }
          this._disposableItems.Remove(i);
        }
      });
    }

    public new void Dispose()
    {
      base.Dispose();
      this._disposables.Dispose();
      foreach (var disposable in this._disposableItems.SelectMany(i => i.Value))
      {
        disposable.Dispose();
      }
    }

    public event EventHandler? TicketCountChanged;
  }
}
