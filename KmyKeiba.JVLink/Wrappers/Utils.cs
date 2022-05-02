using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Wrappers
{
  internal class LambdaComparer<T> : IComparer<T> where T : class
  {
    private Func<T?, T?, int> _predicate;

    public LambdaComparer(Func<T?, T?, int> predicate)
    {
      this._predicate = predicate;
    }

    public int Compare(T? x, T? y)
    {
      return this._predicate(x, y);
    }
  }

  internal class ComparableComparer<T> : IComparer<T>
  {
    private Func<T?, IComparable?> _predicate;

    public ComparableComparer(Func<T?, IComparable?> predicate)
    {
      this._predicate = predicate;
    }

    public int Compare(T? x, T? y)
    {
      var xx = this._predicate(x);
      var yy = this._predicate(y);
      if (xx == null && yy != null)
      {
        return -1;
      }
      return xx?.CompareTo(yy) ?? 0;
    }
  }
}
