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
using CefSharp.Wpf;
using KmyKeiba.Models.Script;
using CefSharp;

namespace KmyKeiba.Behaviors
{
  class BrowserControllerBehavior : Behavior<ChromiumWebBrowser>
  {
    private string? _requestedUrl;

    public static readonly DependencyProperty ControllerProperty
        = DependencyProperty.Register(
            nameof(Controller),
            typeof(BrowserController),
            typeof(BrowserControllerBehavior),
            new PropertyMetadata(null, (sender, e) =>
            {
              if (sender is BrowserControllerBehavior view)
              {
                if (e.OldValue is BrowserController old)
                {
                  old.NavigationRequested -= view.OnNavigationRequested;
                  old.UpdateRequested -= view.OnUpdateRequested;
                  old.UpdateHtmlRequested -= view.OnUpdateHtmlRequested;
                }
                if (e.NewValue is BrowserController @new)
                {
                  @new.NavigationRequested += view.OnNavigationRequested;
                  @new.UpdateRequested += view.OnUpdateRequested;
                  @new.UpdateHtmlRequested += view.OnUpdateHtmlRequested;
                  if (@new.LastUrl != null)
                  {
                    view.AssociatedObject?.LoadUrlAsync(@new.LastUrl);
                  }
                }
              }
            }));

    public BrowserController? Controller
    {
      get { return (BrowserController)GetValue(ControllerProperty); }
      set { SetValue(ControllerProperty, value); }
    }

    private void OnNavigationRequested(object? sender, NavigationRequestedEventArgs e)
    {
      if (this.AssociatedObject != null)
      {
        this.AssociatedObject.LoadUrlAsync(e.Url);
      }
      else
      {
        this._requestedUrl = e.Url;
      }
    }

    private void OnUpdateHtmlRequested(object? sender, UpdateHtmlRequestedEventArgs e)
    {
      if (this.AssociatedObject != null)
      {
        this.AssociatedObject.LoadHtml(e.Html, e.Url);
      }
    }

    private void OnUpdateRequested(object? sender, EventArgs e)
    {
      if (this.AssociatedObject != null)
      {
        this.AssociatedObject.ReloadCommand.Execute(null);
      }
    }

    protected override void OnAttached()
    {
      base.OnAttached();

      if (this._requestedUrl != null)
      {
        this.AssociatedObject.LoadUrlAsync(this._requestedUrl);
        this._requestedUrl = null;
      }
    }

    protected override void OnDetaching()
    {
      base.OnDetaching();
    }
  }
}
