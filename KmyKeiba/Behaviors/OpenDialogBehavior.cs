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
using KmyKeiba.Common;

namespace KmyKeiba.Behaviors
{
  class OpenDialogBehavior : Behavior<MainWindow>
  {
    private readonly List<OpenDialogRequestEventArgs> _stockEvents = new();

    public static readonly DependencyProperty ControllerProperty
        = DependencyProperty.Register(
            nameof(Controller),
            typeof(OpenDialogRequest),
            typeof(OpenDialogBehavior),
            new PropertyMetadata(null, (sender, e) =>
            {
              if (sender is OpenDialogBehavior view)
              {
                if (e.OldValue is OpenDialogRequest old)
                {
                  old.Requested -= view.OnRequested;
                }
                if (e.NewValue is OpenDialogRequest @new)
                {
                  @new.Requested += view.OnRequested;
                }
              }
            }));

    public OpenDialogRequest? Controller
    {
      get { return (OpenDialogRequest)GetValue(ControllerProperty); }
      set { SetValue(ControllerProperty, value); }
    }

    protected override void OnAttached()
    {
      base.OnAttached();

      var stocks = this._stockEvents.ToArray();
      this._stockEvents.Clear();
      foreach (var stock in stocks)
      {
        this.OpenDialog(stock);
      }
    }

    private void OnRequested(object? sender, OpenDialogRequestEventArgs e)
    {
      if (this.AssociatedObject != null)
      {
        this.OpenDialog(e);
      }
      else
      {
        this._stockEvents.Add(e);
      }
    }

    private void OpenDialog(OpenDialogRequestEventArgs e)
    {
      // TODO: open dialogs
    }
  }
}
