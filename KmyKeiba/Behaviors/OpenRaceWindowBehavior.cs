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
using KmyKeiba.Views.Main;

namespace KmyKeiba.Behaviors
{
  class OpenRaceWindowBehavior : Behavior<MainWindow>
  {
    private readonly List<OpenRaceRequestEventArgs> _stockEvents = new();
    private readonly List<WeakReference<RaceWindow>> _windows = new();

    public static readonly DependencyProperty ControllerProperty
        = DependencyProperty.Register(
            nameof(Controller),
            typeof(OpenRaceRequest),
            typeof(OpenRaceWindowBehavior),
            new PropertyMetadata(null, (sender, e) =>
            {
              if (sender is OpenRaceWindowBehavior view)
              {
                if (e.OldValue is OpenRaceRequest old)
                {
                  old.Requested -= view.OnRequested;
                }
                if (e.NewValue is OpenRaceRequest @new)
                {
                  @new.Requested += view.OnRequested;
                }
              }
            }));

    public OpenRaceRequest? Controller
    {
      get { return (OpenRaceRequest)GetValue(ControllerProperty); }
      set { SetValue(ControllerProperty, value); }
    }

    protected override void OnAttached()
    {
      base.OnAttached();

      this.AssociatedObject.Closing += (sender, e) => this.OnMainWindowClosing();

      var stocks = this._stockEvents.ToArray();
      this._stockEvents.Clear();
      foreach (var stock in stocks)
      {
        this.OpenRaceWindow(stock);
      }
    }

    private void OnRequested(object? sender, OpenRaceRequestEventArgs e)
    {
      if (this.AssociatedObject != null)
      {
        this.OpenRaceWindow(e);
      }
      else
      {
        this._stockEvents.Add(e);
      }
    }

    private void OpenRaceWindow(OpenRaceRequestEventArgs e)
    {
      var window = new RaceWindow
      {
        DataContext = new RaceWindowViewModel(e.RaceKey),
      };
      window.Show();

      this._windows.Add(new WeakReference<RaceWindow>(window));
    }

    private void OnMainWindowClosing()
    {
      foreach (var window in this._windows)
      {
        window.TryGetTarget(out var win);
        if (win != null)
        {
          try
          {
            win.Close();
          }
          catch (Exception ex)
          {
            // TODO: logger
          }
        }
      }
    }
  }
}
