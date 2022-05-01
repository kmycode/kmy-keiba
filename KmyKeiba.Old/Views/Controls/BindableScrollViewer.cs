using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace KmyKeiba.Views.Controls
{
  class BindableScrollViewer : ScrollViewer
  {
    public static readonly DependencyProperty ScrollXProperty =
    DependencyProperty.Register(nameof(ScrollX),
                                typeof(double),
                                typeof(BindableScrollViewer),
                                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback((sender, e) =>
                                {
                                  if (sender is BindableScrollViewer view)
                                  {
                                    view.ScrollToHorizontalOffset(view.ScrollX);
                                  }
                                })));

    public double ScrollX
    {
      get { return (double)GetValue(ScrollXProperty); }
      set { SetValue(ScrollXProperty, value); }
    }

    public static readonly DependencyProperty ScrollYProperty =
    DependencyProperty.Register(nameof(ScrollY),
                                typeof(double),
                                typeof(BindableScrollViewer),
                                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback((sender, e) =>
                                {
                                  if (sender is BindableScrollViewer view)
                                  {
                                    view.ScrollToVerticalOffset(view.ScrollY);
                                  }
                                })));

    public double ScrollY
    {
      get { return (double)GetValue(ScrollYProperty); }
      set { SetValue(ScrollYProperty, value); }
    }

    public BindableScrollViewer()
    {
      this.Loaded += (sender, e) =>
      {
        this.ScrollToHorizontalOffset(this.ScrollX);
        this.ScrollToVerticalOffset(this.ScrollY);
      };
      this.ScrollChanged += (sender, e) =>
      {
        this.ScrollX = e.HorizontalOffset;
        this.ScrollY = e.VerticalOffset;
      };
    }
  }
}
