using KmyKeiba.Models.Race;
using KmyKeiba.Common;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KmyKeiba.JVLink.Entities;

namespace KmyKeiba.Models.Image
{
  /// <summary>
  /// 脚質
  /// </summary>
  public class RunningStyleImage : DisplayImage
  {
    private SKBitmap? _bitmap;

    private static readonly SKBitmap _frontRunner;
    private static readonly SKBitmap _stalker;
    private static readonly SKBitmap _sotp;
    private static readonly SKBitmap _saveRunner;
    private static readonly SKBitmap _unknown;

    public override float Width => 44;
    public override float Height => 14;

    public RunningStyle RunningStyle
    {
      get => this._runningStyle;
      set
      {
        if (this._runningStyle != value)
        {
          this._runningStyle = value;
          this._bitmap = value switch
          {
            RunningStyle.FrontRunner => _frontRunner,
            RunningStyle.Stalker => _stalker,
            RunningStyle.Sotp => _sotp,
            RunningStyle.SaveRunner => _saveRunner,
            _ => _unknown,
          };
          this.Invalidate();
        }
      }
    }
    private RunningStyle _runningStyle;

    static RunningStyleImage()
    {
      var runningStyleDisabledColor = ResourceUtil.TryGetResource<RHColor>("RunningStyleDisabledColor")?.ToSKColor()
        ?? new SKColor(48, 48, 48);
      var frontRunnerColor = ResourceUtil.TryGetResource<RHColor>("FrontRunnerColor")?.ToSKColor()
        ?? new SKColor(128, 0, 0);
      var stalkerRunnerColor = ResourceUtil.TryGetResource<RHColor>("StalkerRunnerColor")?.ToSKColor()
        ?? new SKColor(128, 0, 0);
      var sotpRunnerColor = ResourceUtil.TryGetResource<RHColor>("SotpRunnerColor")?.ToSKColor()
        ?? new SKColor(128, 0, 0);
      var saveRunnerColor = ResourceUtil.TryGetResource<RHColor>("SaveRunnerColor")?.ToSKColor()
        ?? new SKColor(128, 0, 0);

      const int width = 11;
      const int height = 14;
      const int padding = 1;

      SKBitmap DrawRunningStyle(RunningStyle runningStyle)
      {
        var bitmap = new SKBitmap(width * 4, height);
        using var canvas = new SKCanvas(bitmap);

        void DrawMark(float x, RunningStyle style)
        {
          var path = new SKPath();
          path.MoveTo(x + width - padding, padding);
          path.LineTo(x + padding, height / 2);
          path.LineTo(x + width - padding, height - padding);
          path.LineTo(x + width - padding, padding);

          canvas?.DrawPath(path, new SKPaint
          {
            IsStroke = false,
            Color = (style != runningStyle) ? runningStyleDisabledColor :
                    style switch
                    {
                      RunningStyle.FrontRunner => frontRunnerColor,
                      RunningStyle.Stalker => stalkerRunnerColor,
                      RunningStyle.Sotp => sotpRunnerColor,
                      RunningStyle.SaveRunner => saveRunnerColor,
                      _ => runningStyleDisabledColor,
                    },
          });
        }

        DrawMark(width * 0, RunningStyle.FrontRunner);
        DrawMark(width * 1, RunningStyle.Stalker);
        DrawMark(width * 2, RunningStyle.Sotp);
        DrawMark(width * 3, RunningStyle.SaveRunner);

        return bitmap;
      }

      _frontRunner = DrawRunningStyle(RunningStyle.FrontRunner);
      _stalker = DrawRunningStyle(RunningStyle.Stalker);
      _sotp = DrawRunningStyle(RunningStyle.Sotp);
      _saveRunner = DrawRunningStyle(RunningStyle.SaveRunner);
      _unknown = DrawRunningStyle(RunningStyle.Unknown);
    }

    public override void OnPaint(SKSurface surface)
    {
      var canvas = surface.Canvas;

      if (this._bitmap != null)
      {
        canvas.DrawBitmap(this._bitmap, 0, 0);
      }
    }
  }
}
