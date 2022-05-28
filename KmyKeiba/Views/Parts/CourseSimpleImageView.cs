using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace KmyKeiba.Views.Parts
{
  public class CourseSimpleImageView : Border
  {
    private static readonly int size = 18;
    private static readonly PointCollection _left = new PointCollection(new[] { new Point(size, 0), new Point(size, size), new Point(0, size / 2), });
    private static readonly PointCollection _right = new PointCollection(new[] { new Point(0, 0), new Point(0, size), new Point(size, size / 2), });
    private static readonly PointCollection _straight = new PointCollection(new[] { new Point(0, size / 4), new Point(size, size / 4), new Point(size, size * 3 / 4), new Point(0, size * 3 / 4), });
    private static readonly PointCollection _steeplechases = new PointCollection(new[] { new Point(0, size * 6 / 7), new Point(size / 4, size / 7), new Point(size * 2 / 4, size * 6 / 7), new Point(size * 3 / 4, size / 7), new Point(size, size * 6 / 7), new Point(0, size * 6 / 7), });
    private static readonly Brush turf = (Application.Current.TryFindResource("TurfColor") as RHColor ?? new RHColor()).ToBrush();
    private static readonly Brush dirt = (Application.Current.TryFindResource("DirtColor") as RHColor ?? new RHColor()).ToBrush();

    public static readonly DependencyProperty RaceProperty
      = DependencyProperty.Register(
        nameof(Race),
        typeof(RaceData),
        typeof(CourseSimpleImageView),
        new PropertyMetadata(null, (sender, e) =>
        {
          if (sender is CourseSimpleImageView view)
          {
            view.Update();
          }
        }));

    public RaceData? Race
    {
      get { return (RaceData)GetValue(RaceProperty); }
      set { SetValue(RaceProperty, value); }
    }

    private void Update()
    {
      PointCollection points = _straight;
      if (this.Race?.TrackType == TrackType.Steeplechase)
      {
        points = _steeplechases;
      }
      else if (this.Race?.TrackCornerDirection == TrackCornerDirection.Left)
      {
        points = _left;
      }
      else if (this.Race?.TrackCornerDirection == TrackCornerDirection.Right)
      {
        points = _right;
      }

      this.Child = new Polygon
      {
        Points = new PointCollection(points),
        Fill = this.Race?.TrackGround == TrackGround.Turf ? turf : dirt,
      };
    }
  }
}
