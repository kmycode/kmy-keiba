using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Image
{
  internal class RaceHorsePassingOrderImage
  {
    private SKBitmap? _bitmap;

    public RaceHorsePassingOrderImage()
    {
      this.UpdateBitmap();
    }

    private void UpdateBitmap()
    {
      var width = 400;
      var height = 100;

      var bitmap = new SKBitmap(width, height);
      using var canvas = new SKCanvas(bitmap);

      canvas.DrawRect(0, 0, 400, 100, new SKPaint
      {
        Color = new SKColor(255, 255, 230),
        IsStroke = false,
      });
      canvas.DrawRect(0, 0, 400 - 1, 100 - 1, new SKPaint
      {
        Color = new SKColor(0, 128, 0),
        StrokeWidth = 1,
        IsStroke = true,
      });

      canvas.DrawRect(20, 20, 30, 30, new SKPaint
      {
        Color = SKColors.White,
        IsStroke = false,
      });
      canvas.DrawRect(20, 20, 30, 30, new SKPaint
      {
        Color = SKColors.Black,
        StrokeWidth = 1,
        IsStroke = true,
      });
      canvas.DrawText("10", 35, 45, new SKPaint
      {
        Color = SKColors.Black,
        TextSize = 22,
        TextAlign = SKTextAlign.Center,
      });

      this._bitmap = bitmap;
    }

    public void OnPaint(SKSurface surface)
    {
      var canvas = surface.Canvas;

      if (this._bitmap != null)
      {
        canvas.DrawBitmap(this._bitmap, 0, 0);
      }
    }
  }
}
