using KmyKeiba.Models.Race;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Image
{
  /// <summary>
  /// コーナー通過順位
  /// </summary>
  public class RaceHorsePassingOrderImage : DisplayImage
  {
    private SKBitmap? _bitmap;

    public RaceCorner? Order
    {
      get => this._order;
      set
      {
        if (this._order != value)
        {
          this._order = value;
          this.UpdateBitmap();
        }
      }
    }
    private RaceCorner? _order;

    private float _width;
    private float _height;

    public override float Width => this._width;
    public override float Height => this._height;

    public RaceHorsePassingOrderImage()
    {
      this.UpdateBitmap();
    }

    private void UpdateBitmap()
    {
      if (this.Order == null || !this.Order.Groups.Any())
      {
        return;
      }

      const int SmallSpaceSize = 20;
      const int LargeSpaceSize = 50;
      const int HorseNumberSize = 30;
      const int HorseNumberMargin = 16;
      const int HorseNumberMarginVertical = 5;
      const int GroupTopHorseMargin = 8;

      var width = this.Order.Groups
        .Where(g => g.AheadSpace != RaceCorner.Group.AheadSpaceType.Retired)
        .Select(g => HorseNumberSize + HorseNumberMargin +
                         (g.TopHorseNumber != 0 ? GroupTopHorseMargin : 0) +
                         (g.AheadSpace == RaceCorner.Group.AheadSpaceType.Small ? SmallSpaceSize :
                          g.AheadSpace == RaceCorner.Group.AheadSpaceType.Large ? LargeSpaceSize : 0))
        .Sum() + HorseNumberMargin;
      var height = this.Order.Groups.Max(g => g.HorseNumbers.Count) * (HorseNumberSize + HorseNumberMarginVertical) + HorseNumberMargin * 2;

      this._width = width;
      this._height = height;

      var bitmap = new SKBitmap(width, height);
      using var canvas = new SKCanvas(bitmap);

      void DrawHorseNumber(int num, float x, float y)
      {
        if (canvas == null)
        {
          return;
        }

        canvas.DrawRectWithBorder(x, y, HorseNumberSize, HorseNumberSize,
          new SKPaint
          {
            Color = SKColors.Black,
            StrokeWidth = 1,
          },
          new SKPaint
          {
            Color = SKColors.White,
          });
        canvas.DrawText(num.ToString(), x + HorseNumberSize / 2, y + HorseNumberSize - 6, new SKPaint
        {
          Color = SKColors.Black,
          TextSize = 22,
          TextAlign = SKTextAlign.Center,
        });
      }

      canvas.DrawRectWithBorder(0, 0, width, height,
        new SKPaint
        {
          Color = new SKColor(0, 128, 0),
          StrokeWidth = 1,
        },
        new SKPaint
        {
          Color = new SKColor(255, 255, 230),
        });

      var groupHorsesMax = this.Order.Groups.Max(g => g.HorseNumbers.Count);

      var x = HorseNumberMargin;
      foreach (var group in this.Order.Groups.Where(g => g.AheadSpace != RaceCorner.Group.AheadSpaceType.Retired))
      {
        var y = HorseNumberMargin;

        // 垂直方向に中央揃え
        if (group.HorseNumbers.Count < groupHorsesMax)
        {
          y += (HorseNumberSize + HorseNumberMarginVertical) * (groupHorsesMax - group.HorseNumbers.Count) / 2;
        }

        // 前のグループとの間のスペース
        if (group.AheadSpace == RaceCorner.Group.AheadSpaceType.Small)
        {
          x += SmallSpaceSize;
        }
        if (group.AheadSpace == RaceCorner.Group.AheadSpaceType.Large)
        {
          x += LargeSpaceSize;
        }

        // 番号を描画
        if (group.TopHorseNumber != 0)
        {
          x += GroupTopHorseMargin;
        }
        foreach (var number in group.HorseNumbers)
        {
          var xx = x;
          if (group.TopHorseNumber == number)
          {
            xx -= GroupTopHorseMargin;
          }

          DrawHorseNumber(number, xx, y);

          y += HorseNumberMarginVertical + HorseNumberSize;
        }

        x += HorseNumberMargin + HorseNumberSize;
      }

      this._bitmap = bitmap;

      this.Invalidate();
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
