using KmyKeiba.JVLink.Entities;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace KmyKeiba.Views.Parts
{
  public class RaceCourseConditionIcon : Border
  {
    private static Brush? _sunB;
    private static Brush? _rainB;

    public static readonly DependencyProperty ConditionProperty
      = DependencyProperty.Register(
       nameof(Condition),
       typeof(RaceCourseCondition),
       typeof(RaceCourseConditionIcon),
       new PropertyMetadata((sender, e) => (sender as RaceCourseConditionIcon)?.OnConditionChanged()));

    public RaceCourseCondition? Condition
    {
      get { return (RaceCourseCondition)GetValue(ConditionProperty); }
      set { SetValue(ConditionProperty, value); }
    }

    public RaceCourseConditionIcon()
    {
      var border = new Border
      {
        Background = Brushes.DeepSkyBlue,
        Height = 0,
        VerticalAlignment = VerticalAlignment.Bottom,
      };
      this.Child = border;

      this.IsVisibleChanged += (sender, e) =>
      {
        if (this.Visibility == Visibility.Visible)
        {
          this.OnConditionChanged();
        }
      };
    }

    private double _lastHeight;
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
      base.OnRenderSizeChanged(sizeInfo);

      if (this.ActualHeight != this._lastHeight)
      {
        this._lastHeight = this.ActualHeight;
        this.OnConditionChanged();
      }
    }

    private void OnConditionChanged()
    {
      var border = (Border)this.Child;

      switch (this.Condition)
      {
        case RaceCourseCondition.Standard:
          border.Height = this.ActualHeight * 0.1;
          border.Background = _sunB ??= this.TryFindResource("FineForeground") as Brush;
          break;
        case RaceCourseCondition.Good:
          border.Height = this.ActualHeight * 0.4;
          border.Background = Brushes.DeepSkyBlue;
          break;
        case RaceCourseCondition.Yielding:
          border.Height = this.ActualHeight * 0.7;
          border.Background = _rainB ??= this.TryFindResource("RainyForeground") as Brush;
          break;
        case RaceCourseCondition.Soft:
          border.Height = this.ActualHeight * 1;
          border.Background = Brushes.BlueViolet;
          break;
        default:
          border.Height = 0;
          break;
      }
    }
  }
}
