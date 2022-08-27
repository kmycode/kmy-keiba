using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Common
{
  /// <summary>
  /// アプリのカラーテーマを管理
  /// </summary>
  internal static class ThemeUtil
  {
    private static ApplicationTheme _currentTheme = ApplicationTheme.Unset;

    public static ApplicationTheme Current
    {
      get => _currentTheme;
      set
      {
        if (_currentTheme != value)
        {
          _currentTheme = value;
          ViewMessages.ChangeTheme?.Invoke(value);
        }
      }
    }
  }

  internal enum ApplicationTheme
  {
    Unset,
    Classic,
    Dark,
  }

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
