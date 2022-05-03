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

    protected void Invalidate()
    {
      this.Updated?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? Updated;
  }

  internal static class SKUtils
  {
    public static void DrawRectWithBorder(this SKCanvas canvas, float x, float y, float w, float h, SKPaint border, SKPaint fill)
    {
      var oldBorderStroke = border.IsStroke;
      var oldFillStroke = fill.IsStroke;
      border.IsStroke = true;
      fill.IsStroke = false;

      canvas.DrawRect(x, y, w, h, fill);
      canvas.DrawRect(x, y, w - border.StrokeWidth, h - border.StrokeWidth, border);

      border.IsStroke = oldBorderStroke;
      fill.IsStroke = oldFillStroke;
    }
  }
}
