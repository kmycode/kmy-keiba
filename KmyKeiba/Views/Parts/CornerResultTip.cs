using KmyKeiba.Models.Analysis;
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
  public class CornerResultTip : StackPanel
  {
    private static readonly Brush _arrowBrush = Application.Current.TryFindResource("ThinSubForeground") as Brush ?? Brushes.Gray;
    private static readonly Brush _goodBrush = Application.Current.TryFindResource("GoodBackground") as Brush ?? Brushes.Gray;
    private static readonly Brush _badBrush = Application.Current.TryFindResource("BadBackground") as Brush ?? Brushes.Gray;
    private static readonly Brush _baseBrush = Application.Current.TryFindResource("BaseBackground") as Brush ?? Brushes.Gray;

    public static readonly DependencyProperty CornerGradesProperty
  = DependencyProperty.Register(
      nameof(CornerGrades),
      typeof(IEnumerable<RaceHorseCornerGrade>),
      typeof(CornerResultTip),
      new PropertyMetadata(Enumerable.Empty<RaceHorseCornerGrade>(), (sender, e) =>
      {
        if (sender is CornerResultTip view)
        {
          view.Update();
        }
      }));

    public IEnumerable<RaceHorseCornerGrade>? CornerGrades
    {
      get { return (IEnumerable<RaceHorseCornerGrade>)GetValue(CornerGradesProperty); }
      set { SetValue(CornerGradesProperty, value); }
    }

    public static readonly DependencyProperty WithResultProperty
  = DependencyProperty.Register(
      nameof(WithResult),
      typeof(bool),
      typeof(CornerResultTip),
      new PropertyMetadata(true, (sender, e) =>
      {
        if (sender is CornerResultTip view)
        {
          view.Update();
        }
      }));

    public bool WithResult
    {
      get { return (bool)GetValue(WithResultProperty); }
      set { SetValue(WithResultProperty, value); }
    }

    public static readonly DependencyProperty FontSizeProperty
      = DependencyProperty.Register(
        nameof(FontSize),
        typeof(double),
        typeof(CornerResultTip),
        new PropertyMetadata(16.0, (sender, e) =>
        {
          if (sender is CornerResultTip view)
          {
            var val = (double)e.NewValue;
            foreach (var ui in view.Children.OfType<Border>().Select(b => b.Child).OfType<TextBlock>())
            {
              ui.FontSize = val;
            }
          }
        }));

    public double FontSize
    {
      get { return (double)GetValue(FontSizeProperty); }
      set { SetValue(FontSizeProperty, value); }
    }

    public static readonly DependencyProperty PartWidthProperty
      = DependencyProperty.Register(
        nameof(PartWidth),
        typeof(double),
        typeof(CornerResultTip),
        new PropertyMetadata(28.0, (sender, e) =>
        {
          if (sender is CornerResultTip view)
          {
            var val = (double)e.NewValue;
            foreach (var ui in view.Children.OfType<Border>())
            {
              ui.Width = val;
            }
          }
        }));

    public double PartWidth
    {
      get { return (double)GetValue(PartWidthProperty); }
      set { SetValue(PartWidthProperty, value); }
    }

    public static readonly DependencyProperty PartHeightProperty
      = DependencyProperty.Register(
        nameof(PartHeight),
        typeof(double),
        typeof(CornerResultTip),
        new PropertyMetadata(36.0, (sender, e) =>
        {
          if (sender is CornerResultTip view)
          {
            var val = (double)e.NewValue;
            foreach (var ui in view.Children.OfType<Border>())
            {
              ui.Height = val;
            }
          }
        }));

    public double PartHeight
    {
      get { return (double)GetValue(PartHeightProperty); }
      set { SetValue(PartHeightProperty, value); }
    }

    public static readonly DependencyProperty PartPaddingProperty
      = DependencyProperty.Register(
        nameof(PartPadding),
        typeof(double),
        typeof(CornerResultTip),
        new PropertyMetadata(2.0, (sender, e) =>
        {
          if (sender is CornerResultTip view)
          {
            var val = (double)e.NewValue;
            var thickness = new Thickness(val, 0, val, 0);
            foreach (var ui in view.Children.OfType<Polygon>())
            {
              ui.Margin = thickness;
            }
          }
        }));

    public double PartPadding
    {
      get { return (double)GetValue(PartPaddingProperty); }
      set { SetValue(PartPaddingProperty, value); }
    }

    public CornerResultTip()
    {
      this.Orientation = Orientation.Horizontal;
    }

    private void Update()
    {
      this.Children.Clear();

      if (this.CornerGrades == null)
      {
        return;
      }

      var isFirst = true;
      foreach (var grade in this.CornerGrades)
      {
        if (!isFirst)
        {
          this.Children.Add(this.CreateArrow());
        }
        else
        {
          isFirst = false;
        }

        if (!this.WithResult && grade.IsResult)
        {
          if (this.Children.Count > 0)
          {
            // 最後の矢印を消す
            this.Children.RemoveAt(this.Children.Count - 1);
          }
          continue;
        }

        var text = new Border
        {
          Width = this.PartWidth,
          Height = this.PartHeight,
          Child = new TextBlock
          {
            Text = grade.Order.ToString(),
            TextAlignment = TextAlignment.Center,
            FontSize = this.FontSize,
          },
          Background = grade.Type == CornerGradeType.Bad ? _badBrush : grade.Type == CornerGradeType.Good ? _goodBrush : _baseBrush,
        };
        this.Children.Add(text);
      }
    }

    private FrameworkElement CreateArrow()
    {
      var arrow = new Polygon
      {
        Points =
        {
          new Point(0, 0),
          new Point(0, 10),
          new Point(5, 5),
        },
        Fill = _arrowBrush,
        StrokeThickness = 0,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(this.PartPadding, 0, this.PartPadding, 0),
      };

      return arrow;
    }
  }
}
