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
  public class WoodtipTrainingDirectionIcon : Border
  {
    private static readonly int size = 18;
    private static readonly PointCollection _left = new PointCollection(new[] { new Point(size, 0), new Point(size, size), new Point(0, size / 2), });
    private static readonly PointCollection _right = new PointCollection(new[] { new Point(0, 0), new Point(0, size), new Point(size, size / 2), });
    private static readonly Brush woodtip = (Application.Current.TryFindResource("WoodtipColor") as RHColor ?? new RHColor()).ToBrush();

    public static readonly DependencyProperty DirectionProperty
      = DependencyProperty.Register(
        nameof(Direction),
        typeof(WoodtipDirection),
        typeof(WoodtipTrainingDirectionIcon),
        new PropertyMetadata(default(WoodtipDirection), (sender, e) =>
        {
          if (sender is WoodtipTrainingDirectionIcon view)
          {
            view.Update();
          }
        }));

    public WoodtipDirection Direction
    {
      get { return (WoodtipDirection)GetValue(DirectionProperty); }
      set { SetValue(DirectionProperty, value); }
    }

    public WoodtipTrainingDirectionIcon()
    {
      this.Update();
    }

    private void Update()
    {
      this.Child = new Polygon
      {
        Points = new PointCollection(this.Direction switch {
          WoodtipDirection.Left => _left,
          WoodtipDirection.Right => _right,
          _ => _left,
        }),
        Fill = woodtip,
      };
    }
  }
}
