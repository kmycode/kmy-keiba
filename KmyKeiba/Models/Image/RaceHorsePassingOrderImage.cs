using KmyKeiba.Models.Race;
using KmyKeiba.Common;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KmyKeiba.Models.Image
{
  /// <summary>
  /// コーナー通過順位
  /// </summary>
  public class RaceHorsePassingOrderImage : DisplayImage
  {
    private SKBitmap? _bitmap;

    public IEnumerable<RaceCorner.Group>? Groups
    {
      get => this._groups;
      set
      {
        if (this._groups != value)
        {
          this._groups = value;
          this.UpdateBitmap();
        }
      }
    }
    private IEnumerable<RaceCorner.Group>? _groups;

    private short _firstHorse;
    private short _secondHorse;
    private short _thirdHorse;

    private float _width;
    private float _height;

    public override float Width => this._width;
    public override float Height => this._height;

    public RaceHorsePassingOrderImage()
    {
      this.UpdateBitmap();
    }

    public void SetOrders(short firstHorse, short secondHorse, short thirdHorse)
    {
      this._firstHorse = firstHorse;
      this._secondHorse = secondHorse;
      this._thirdHorse = thirdHorse;
      this.UpdateBitmap();
    }

    private void UpdateBitmap()
    {
      if (this.Groups == null || !this.Groups.Any())
      {
        return;
      }

      var horceNumberPlateBackground = ResourceUtil.TryGetResource<RHColor>("HorseNumberPlateBackground")?.ToSKColor()
        ?? new SKColor(255, 255, 230);
      var horceNumberPlateBorder = ResourceUtil.TryGetResource<RHColor>("HorseNumberPlateBorder")?.ToSKColor()
        ?? new SKColor(0, 128, 0);
      var horceNumberPlateItemBackground = ResourceUtil.TryGetResource<RHColor>("HorseNumberPlateItemBackground")?.ToSKColor()
        ?? SKColors.White;
      var horceNumberPlateItemBorder = ResourceUtil.TryGetResource<RHColor>("HorseNumberPlateItemBorder")?.ToSKColor()
        ?? SKColors.Black;
      var horceNumberPlateItemForeground = ResourceUtil.TryGetResource<RHColor>("HorseNumberPlateItemForeground")?.ToSKColor()
        ?? SKColors.Black;

      var horceNumberPlateItemBackgroundFirst = ResourceUtil.TryGetResource<RHColor>("HorseNumberPlateItemBackgroundFirst")?.ToSKColor()
        ?? new SKColor(255, 255, 230);
      var horceNumberPlateItemBackgroundSecond = ResourceUtil.TryGetResource<RHColor>("HorseNumberPlateItemBackgroundSecond")?.ToSKColor()
        ?? new SKColor(255, 255, 230);
      var horceNumberPlateItemBackgroundThird = ResourceUtil.TryGetResource<RHColor>("HorseNumberPlateItemBackgroundThird")?.ToSKColor()
        ?? new SKColor(255, 255, 230);

      const int SmallSpaceSize = 40;
      const int LargeSpaceSize = 100;
      const int HorseNumberSize = 30;
      const int HorseNumberMargin = 16;
      const int HorseNumberMarginVertical = 5;
      const int GroupTopHorseMargin = 8;

      var width = this.Groups
        .Where(g => g.AheadSpace != RaceCorner.Group.AheadSpaceType.Retired)
        .Select(g => HorseNumberSize + HorseNumberMargin +
                         (g.TopHorseNumber != 0 ? GroupTopHorseMargin : 0) +
                         (g.AheadSpace == RaceCorner.Group.AheadSpaceType.Small ? SmallSpaceSize :
                          g.AheadSpace == RaceCorner.Group.AheadSpaceType.Large ? LargeSpaceSize : 0))
        .Sum() + HorseNumberMargin;
      var height = this.Groups.Max(g => g.HorseNumbers.Count) * (HorseNumberSize + HorseNumberMarginVertical) + HorseNumberMargin * 2;

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

        var color = num == this._firstHorse ? horceNumberPlateItemBackgroundFirst :
          num == this._secondHorse ? horceNumberPlateItemBackgroundSecond :
          num == this._thirdHorse ? horceNumberPlateItemBackgroundThird :
          horceNumberPlateItemBackground;

        canvas.DrawRectWithBorder(x, y, HorseNumberSize, HorseNumberSize,
          new SKPaint
          {
            Color = horceNumberPlateItemBorder,
            StrokeWidth = 1,
          },
          new SKPaint
          {
            Color = color,
          });
        canvas.DrawText(num.ToString(), x + HorseNumberSize / 2, y + HorseNumberSize - 6, new SKPaint
        {
          Color = horceNumberPlateItemForeground,
          TextSize = 22,
          TextAlign = SKTextAlign.Center,
        });
      }

      canvas.DrawRectWithBorder(0, 0, width, height,
        new SKPaint
        {
          Color = horceNumberPlateBorder,
          StrokeWidth = 1,
        },
        new SKPaint
        {
          Color = horceNumberPlateBackground,
        });

      var groupHorsesMax = this.Groups.Max(g => g.HorseNumbers.Count);

      var x = HorseNumberMargin;
      foreach (var group in this.Groups.Where(g => g.AheadSpace != RaceCorner.Group.AheadSpaceType.Retired))
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
        canvas.Clear();
        canvas.DrawBitmap(this._bitmap, 0, 0);
      }
    }
  }
}
