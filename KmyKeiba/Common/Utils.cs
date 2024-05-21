using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Common
{
  internal static class ThreadUtil
  {
    public static void InvokeOnUiThread(Action action)
    {
      ViewMessages.InvokeUiThread?.Invoke(action);
    }
  }

  public class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
  {
    readonly IEnumerable<TElement> elements;

    public Grouping(TKey key, IEnumerable<TElement> elements)
    {
      this.Key = key;
      this.elements = elements;
    }

    public TKey Key { get; private set; }

    public IEnumerator<TElement> GetEnumerator()
    {
      return this.elements.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return ((Grouping<TKey, TElement>)this).GetEnumerator();
    }
  }
}
