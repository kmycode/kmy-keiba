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
    private static ApplicationTheme _currentTheme = ApplicationTheme.Classic;

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
    Classic,
    Dark,
  }
}
