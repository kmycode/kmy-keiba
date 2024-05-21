using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Common
{
  /// <summary>
  /// Viewより依存の注入をおこない、モデルからViewのロジックを呼び出すクラス
  /// </summary>
  internal static class ViewMessages
  {
    public static Func<string, object?>? TryGetResource { get; set; }

    public static Action<Action>? InvokeUiThread { get; set; }
  }
}
