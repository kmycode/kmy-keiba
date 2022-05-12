using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Data.Wrappers;
using KmyKeiba.JVLink.Entities;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Image
{
  /// <summary>
  /// レースのコースの概形
  /// </summary>
  public class RaceCourseSummaryImage : DisplayImage
  {
    private SKBitmap? _bitmap;

    public RaceData? Race
    {
      get => this._race;
      set
      {
        if (this._race != value)
        {
          this._race = value;
          this.UpdateBitmap();
        }
      }
    }
    private RaceData? _race;

    public override float Width => 300;
    public override float Height => 200;

    private void UpdateBitmap()
    {
      if (this.Race == null)
      {
        return;
      }

      var bitmap = new SKBitmap((int)this.Width, (int)this.Height);
      using var canvas = new SKCanvas(bitmap);

      var turfColor = ResourceUtil.TryGetResource<RHColor>("TurfColor")?.ToSKColor()
        ?? new SKColor(0, 128, 0);
      var dirtColor = ResourceUtil.TryGetResource<RHColor>("DirtColor")?.ToSKColor()
        ?? new SKColor(132, 132, 0);
      var baseTextColor = ResourceUtil.TryGetResource<RHColor>("BaseTextColor")?.ToSKColor()
        ?? new SKColor(16, 16, 16);
      var subTextColor = ResourceUtil.TryGetResource<RHColor>("SubTextColor")?.ToSKColor()
        ?? new SKColor(99, 99, 99);
      var uphillColor = ResourceUtil.TryGetResource<RHColor>("UphillColor")?.ToSKColor()
        ?? SKColors.Red;
      var downhillColor = ResourceUtil.TryGetResource<RHColor>("DownhillColor")?.ToSKColor()
        ?? SKColors.Blue;

      var strokeWidth = 8;
      var turf = new SKPaint
      {
        StrokeWidth = strokeWidth,
        Color = turfColor,
        IsStroke = true,
      };
      var dirt = turf.Clone();
      dirt.Color = dirtColor;

      var slopUp = new SKPaint
      {
        IsStroke = false,
        Color = uphillColor,
      };
      var slopDown = slopUp.Clone();
      slopDown.Color = downhillColor;

      void DrawTrack(float x, float y, float width, float height, SKPaint paint)
      {
        canvas.DrawArc(new SKRect(x + strokeWidth, y + strokeWidth, x + height - strokeWidth, y + height - strokeWidth),
                       90, 180, false, paint);
        canvas.DrawArc(new SKRect(x + width - height - strokeWidth, y + strokeWidth, x + width - strokeWidth, y + height - strokeWidth),
                       270, 180, false, paint);
        canvas.DrawLine(x + height / 2, y + strokeWidth, x + width - height / 2, y + strokeWidth, paint);
        canvas.DrawLine(x + height / 2, y + height - strokeWidth, x + width - height / 2, y + height - strokeWidth, paint);
      }

      void DrawSlop(float x, float y, TrackSlope slope)
      {
        if (slope == TrackSlope.Flat)
        {
          return;
        }

        var size = 20;

        if (slope == TrackSlope.LargeDownhill)
        {
          size *= 2;
          slope = TrackSlope.Downhill;
        }
        else if (slope == TrackSlope.LargeUphill)
        {
          size *= 2;
          slope = TrackSlope.Uphill;
        }
        else if (slope == TrackSlope.DownToUphill)
        {
          DrawSlop(x - size / 2, y, TrackSlope.Downhill);
          DrawSlop(x + size / 2, y, TrackSlope.Uphill);
          return;
        }
        else if (slope == TrackSlope.UpToDownhill)
        {
          DrawSlop(x - size / 2, y, TrackSlope.Uphill);
          DrawSlop(x + size / 2, y, TrackSlope.Downhill);
          return;
        }

        var path = new SKPath();
        if (slope == TrackSlope.Uphill)
        {
          path.MoveTo(x, y - size / 2);
          path.LineTo(x + size / 2, y + size / 2);
          path.LineTo(x - size / 2, y + size / 2);
          path.LineTo(x, y - size / 2);
        }
        else
        {
          path.MoveTo(x, y + size / 2);
          path.LineTo(x + size / 2, y - size / 2);
          path.LineTo(x - size / 2, y - size / 2);
          path.LineTo(x, y + size / 2);
        }
        canvas!.DrawPath(path, slope == TrackSlope.Uphill ? slopUp : slopDown);
      }

      (float x, float y) CalcSlopPosition(float x, float y, float width, float height, CoursePosition pos)
      {
        var sq = (float)(1 - MathF.Sqrt(2) / 2);

        return pos switch
        {
          CoursePosition.Corner2 => (x + height / 2 * sq, y + height / 2 * 0.3f),
          CoursePosition.Corner1 => (x + height / 2 * sq, y + height - (height / 2 * 0.3f)),
          CoursePosition.Corner3 => (x + width - (height / 2 * sq), y + height / 2 * 0.3f),
          CoursePosition.Corner4 => (x + width - (height / 2 * sq), y + height - (height / 2 * sq)),
          CoursePosition.First => (x + width / 2, strokeWidth),
          CoursePosition.LastLine => (x + width / 2, height - strokeWidth),
          _ => default,
        };
      }

      void DrawSlopWithPosition(float x, float y, float width, float height, CoursePosition pos, TrackSlope slope)
      {
        var point = CalcSlopPosition(x, y, width, height, pos);

        if (this.Race?.TrackCornerDirection == TrackCornerDirection.Right)
        {
          // 右回りで２・３コーナー間の直線の場合、矢印が進行方向と逆の順番になり紛らわしくなる
          if (pos == CoursePosition.First)
          {
            if (slope == TrackSlope.UpToDownhill)
            {
              slope = TrackSlope.DownToUphill;
            }
            else if (slope == TrackSlope.DownToUphill)
            {
              slope = TrackSlope.UpToDownhill;
            }
          }
        }

        DrawSlop(point.x, point.y, slope);
      }

      SKPaint? outTrack, inTrack;

      if (this.Race.TrackGround == TrackGround.Turf)
      {
        (outTrack, inTrack) = (turf, null);
        if (this.Race.TrackOption == TrackOption.InsideToOutside || this.Race.TrackOption == TrackOption.OutsideToInside)
        {
          inTrack = turf;
        }
      }
      else if (this.Race.TrackGround == TrackGround.TurfToDirt)
      {
        if (this.Race.TrackOption == TrackOption.OutsideToInside)
        {
          (outTrack, inTrack) = (turf, dirt);
        }
        else
        {
          (outTrack, inTrack) = (dirt, turf);
        }
      }
      else
      {
        (outTrack, inTrack) = (dirt, null);
        if (this.Race.TrackOption == TrackOption.InsideToOutside || this.Race.TrackOption == TrackOption.OutsideToInside)
        {
          inTrack = dirt;
        }
      }

      var courseInfos = RaceCourses.TryGetCourses(this.Race);
      if (this.Race.TrackCornerDirection != TrackCornerDirection.Straight)
      {
        DrawTrack(0, 0, this.Width, this.Height, outTrack);

        // 坂を描画
        if (courseInfos.Any())
        {
          var info = courseInfos.FirstOrDefault(c => inTrack == null || (c.Option == TrackOption.Outside || c.Option == TrackOption.Unknown));
          if (info != null)
          {
            DrawSlopWithPosition(0, 0, this.Width, this.Height, CoursePosition.Corner1, info.Corner1Slope);
            DrawSlopWithPosition(0, 0, this.Width, this.Height, CoursePosition.Corner2, info.Corner2Slope);
            DrawSlopWithPosition(0, 0, this.Width, this.Height, CoursePosition.Corner3, info.Corner3Slope);
            DrawSlopWithPosition(0, 0, this.Width, this.Height, CoursePosition.Corner4, info.Corner4Slope);
            DrawSlopWithPosition(0, 0, this.Width, this.Height, CoursePosition.First, info.Corner23LineSlope);
            DrawSlopWithPosition(0, 0, this.Width, this.Height, CoursePosition.LastLine, info.LastLineSlope);
          }
        }

        // 外と内を両方使う場合、内側のトラック
        if (inTrack != null)
        {
          DrawTrack(20, 20, this.Width - 40, this.Height - 40, inTrack);
          if (courseInfos.Any())
          {
            var info = courseInfos.FirstOrDefault(c => c.Option == TrackOption.Inside);
            if (info != null)
            {
              DrawSlopWithPosition(20, 20, this.Width - 40, this.Height - 40, CoursePosition.Corner1, info.Corner1Slope);
              DrawSlopWithPosition(20, 20, this.Width - 40, this.Height - 40, CoursePosition.Corner2, info.Corner2Slope);
              DrawSlopWithPosition(20, 20, this.Width - 40, this.Height - 40, CoursePosition.Corner3, info.Corner3Slope);
              DrawSlopWithPosition(20, 20, this.Width - 40, this.Height - 40, CoursePosition.Corner4, info.Corner4Slope);
              DrawSlopWithPosition(20, 20, this.Width - 40, this.Height - 40, CoursePosition.First, info.Corner23LineSlope);
              DrawSlopWithPosition(20, 20, this.Width - 40, this.Height - 40, CoursePosition.LastLine, info.LastLineSlope);
            }
          }
        }
      }
      else
      {
        // 直線レース
        canvas.DrawLine(0, this.Height - strokeWidth / 2, this.Width, this.Height - strokeWidth / 2, outTrack);
        if (courseInfos.Any())
        {
          var info = courseInfos.FirstOrDefault(c => c.Direction == TrackCornerDirection.Straight);
          if (info != null)
          {
            DrawSlopWithPosition(20, 20, this.Width - 40, this.Height - 40, CoursePosition.LastLine, info.LastLineSlope);
          }
        }
      }

      // レースの長さを描画
      var textPaint = new SKPaint
      {
        TextSize = 18,
        Color = baseTextColor,
      };
      var text = this.Race.Distance + "m";
      canvas.DrawText(this.Race.Distance + "m", this.Width / 2 - textPaint.MeasureText(text) / 2, this.Height / 2 - 9, textPaint);

      // 矢印
      var arrowX = this.Width / 2 - 50;
      var arrowTopBottomX = this.Width / 2 - 50;
      var arrowTopX = this.Width / 2 - 60;
      var arrowY = this.Height - strokeWidth * 8;
      if (this.Race.TrackCornerDirection == TrackCornerDirection.Right)
      {
        arrowTopBottomX = this.Width / 2 + 50;
        arrowTopX = this.Width / 2 + 60;
      }
      canvas.DrawLine(arrowX, arrowY, arrowX + 100, arrowY, new SKPaint
      {
        StrokeWidth = 5,
        Color = subTextColor,
      });

      var path = new SKPath();
      path.MoveTo(arrowTopBottomX, arrowY - 12);
      path.LineTo(arrowTopBottomX, arrowY + 12);
      path.LineTo(arrowTopX, arrowY);
      path.LineTo(arrowTopBottomX, arrowY - 12);
      canvas.DrawPath(path, new SKPaint
      {
        IsStroke = false,
        Color = subTextColor,
      });

      this._bitmap = bitmap;
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
