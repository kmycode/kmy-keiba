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
            new PropertyMetadata(null));

    public FrameworkElement? TargetElement
    {
      get { return (FrameworkElement)GetValue(TargetElementProperty); }
      set { SetValue(TargetElementProperty, value); }
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
      while (!(element is ScrollViewer))
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
      if (this.GetScrollViewer(TargetElement) is ScrollViewer target)
      {
        target.ScrollToHorizontalOffset(e.HorizontalOffset);
        target.ScrollToVerticalOffset(e.VerticalOffset);
      }
    }
  }
}
