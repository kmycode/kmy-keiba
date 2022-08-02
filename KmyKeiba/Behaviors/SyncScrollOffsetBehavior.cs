using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace KmyKeiba.Behaviors
{
  internal class SyncScrollOffsetBehavior : Behavior<FrameworkElement>
  {
    public static readonly DependencyProperty TargetElementProperty
        = DependencyProperty.Register(
            nameof(TargetElement),
            typeof(FrameworkElement),
            typeof(SyncScrollOffsetBehavior),
            new PropertyMetadata(null, (sender, e) =>
            {
              if (sender is SyncScrollOffsetBehavior view)
              {
                if (e.OldValue is FrameworkElement old)
                {
                  var scroll = view.GetScrollViewer(old);
                  if (scroll != null)
                  {
                    scroll.ScrollChanged -= view.Scroll_ScrollChangedAndBack;
                  }
                }
                if (e.NewValue is FrameworkElement @new)
                {
                  var scroll = view.GetScrollViewer(@new);
                  if (scroll != null)
                  {
                    scroll.ScrollChanged += view.Scroll_ScrollChangedAndBack;
                  }
                }
              }
            }));

    public FrameworkElement? TargetElement
    {
      get { return (FrameworkElement)GetValue(TargetElementProperty); }
      set { SetValue(TargetElementProperty, value); }
    }

    public static readonly DependencyProperty RowHeightProperty
        = DependencyProperty.Register(
            nameof(RowHeight),
            typeof(double),
            typeof(SyncScrollOffsetBehavior),
            new PropertyMetadata(1.0));

    public double RowHeight
    {
      get { return (double)GetValue(RowHeightProperty); }
      set { SetValue(RowHeightProperty, value); }
    }

    protected override void OnAttached()
    {
      base.OnAttached();
      this.StartBindingOffset();
    }

    protected override void OnDetaching()
    {
      this.ReleaseBindingOffset();
      base.OnDetaching();
    }

    private async void StartBindingOffset()
    {
      ScrollViewer? scroll = null;
      while (scroll == null)
      {
        scroll = this.GetScrollViewer(this.AssociatedObject) as ScrollViewer;
        await Task.Delay(1000);
      }

      scroll.ScrollChanged += Scroll_ScrollChanged;
    }

    private void ReleaseBindingOffset()
    {
      if (this.GetScrollViewer(this.AssociatedObject) is ScrollViewer scroll)
      {
        scroll.ScrollChanged -= Scroll_ScrollChanged;
      }
    }

    private ScrollViewer? GetScrollViewer(DependencyObject? element)
    {
      while (element is not ScrollViewer)
      {
        var childrenCount = VisualTreeHelper.GetChildrenCount(element);
        if (childrenCount == 1)
        {
          element = VisualTreeHelper.GetChild(element, 0);
        }
        else
        {
          break;
        }
      }

      return element as ScrollViewer;
    }

    private void Scroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
      if (this.GetScrollViewer(this.TargetElement) is ScrollViewer target)
      {
        target.ScrollToHorizontalOffset(e.HorizontalOffset);
        target.ScrollToVerticalOffset(e.VerticalOffset * this.RowHeight);
      }
    }

    private void Scroll_ScrollChangedAndBack(object sender, ScrollChangedEventArgs e)
    {
      if (this.GetScrollViewer(this.AssociatedObject) is ScrollViewer self &&
        this.GetScrollViewer(this.TargetElement) is ScrollViewer target &&
        (Math.Abs(target.HorizontalOffset - self.HorizontalOffset) > 2 || Math.Abs(target.VerticalOffset - self.VerticalOffset) > 2))
      {
        target.ScrollToHorizontalOffset(self.HorizontalOffset);
        target.ScrollToVerticalOffset(self.VerticalOffset * this.RowHeight);
      }
    }
  }
}
