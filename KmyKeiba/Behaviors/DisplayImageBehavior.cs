using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KmyKeiba.ViewModels;
using KmyKeiba.Models.Image;
using SkiaSharp.Views.WPF;
using System.Windows.Media;

namespace KmyKeiba.Behaviors
{
  class DisplayImageBehavior : Behavior<SKElement>
  {
    public static readonly DependencyProperty ImageProperty
        = DependencyProperty.Register(
            nameof(Image),
            typeof(DisplayImage),
            typeof(DisplayImageBehavior),
            new PropertyMetadata(null, (sender, e) =>
            {
              if (sender is DisplayImageBehavior view)
              {
                view.OnImageChanged();

                if (e.OldValue is DisplayImage old)
                {
                  old.Updated -= view.OnImageChanged;
                }
                if (e.NewValue is DisplayImage @new)
                {
                  @new.Updated += view.OnImageChanged;
                }
              }
            }));

    public DisplayImage? Image
    {
      get { return (DisplayImage)GetValue(ImageProperty); }
      set { SetValue(ImageProperty, value); }
    }

    protected override void OnAttached()
    {
      base.OnAttached();

      // これがないとなぜか描画更新されない（StackPanelとかで）
      this.AssociatedObject.MinWidth = 1;
      this.AssociatedObject.MinHeight = 1;

      this.AssociatedObject.PaintSurface += this.AssociatedObject_PaintSurface;
      this.OnImageChanged();
    }

    protected override void OnDetaching()
    {
      base.OnDetaching();

      this.AssociatedObject.PaintSurface -= this.AssociatedObject_PaintSurface;
    }

    private void OnImageChanged()
    {
      // PaintSurfaceイベントに着火
      this.AssociatedObject?.InvalidateVisual();
    }

    private void OnImageChanged(object? sender, EventArgs e)
    {
      this.OnImageChanged();
    }

    private void AssociatedObject_PaintSurface(object? sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
    {
      if (this.Image != null && this.Image.Width > 0 && this.Image.Height > 0)
      {
        this.AssociatedObject.Width = this.Image.Width;
        this.AssociatedObject.Height = this.Image.Height;
      }

      this.Image?.OnPaint(e.Surface);
    }
  }
}
