using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Image
{
  /// <summary>
  /// 画面に表示する画像の基底クラス
  /// </summary>
  public abstract class DisplayImage
  {
    public abstract float Width { get; }

    public abstract float Height { get; }

    public abstract void OnPaint(SKSurface surface);
  }
}
