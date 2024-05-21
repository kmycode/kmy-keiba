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
  internal class ScrollSizeBehavior : Behavior<FrameworkElement>
  {
    protected override void OnAttached()
    {
      base.OnAttached();

      this.AssociatedObject.PreviewMouseWheel += this.AssociatedObject_PreviewMouseWheel;
    }

    protected override void OnDetaching()
    {
      this.AssociatedObject.PreviewMouseWheel -= this.AssociatedObject_PreviewMouseWheel;

      base.OnDetaching();
    }

    private void AssociatedObject_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
    {
      var scrollViewer = this.AssociatedObject as ScrollViewer;
      scrollViewer ??= VisualTreeHelper.GetChild(this.AssociatedObject, 0) as ScrollViewer;
      if (scrollViewer == null) return;

      scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + (e.Delta > 0 ? -1 : 1));
    }
  }
}
