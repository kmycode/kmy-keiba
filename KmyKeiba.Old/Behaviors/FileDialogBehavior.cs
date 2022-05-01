using Microsoft.Win32;
using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KmyKeiba.ViewModels;
using KmyKeiba.ViewEvents;

namespace KmyKeiba.Behaviors
{
  class FileDialogBehavior : Behavior<FrameworkElement>
  {
    public static readonly DependencyProperty CallerProperty
        = DependencyProperty.Register(
            nameof(Caller),
            typeof(FileDialogCaller),
            typeof(FileDialogBehavior),
            new PropertyMetadata(null, (sender, e) =>
            {
              if (sender is FileDialogBehavior view)
              {
                view.OnCallerChanged(e.OldValue as FileDialogCaller);
              }
            }));

    public FileDialogCaller? Caller
    {
      get { return (FileDialogCaller)GetValue(CallerProperty); }
      set { SetValue(CallerProperty, value); }
    }

    protected override void OnAttached()
    {
      base.OnAttached();
      this.OnCallerChanged();
    }

    private void OnCallerChanged(FileDialogCaller? old = null)
    {
      if (old != null)
      {
        old.Called -= this.OnCalled;
      }

      if (this.Caller != null)
      {
        this.Caller.Called += this.OnCalled;
      }
    }

    private void OnCalled(object? sender, FileDialogCalledEventArgs e)
    {
      if (e.Type == FileDialogType.Open)
      {
        var dialog = new Microsoft.Win32.OpenFileDialog();
        dialog.Filter = e.Filter;
        dialog.RestoreDirectory = true;
        if (dialog.ShowDialog(Application.Current.MainWindow) == true)
        {
          e.OnCompleted?.Invoke(dialog.FileName);
        }
      }
      else
      {
        var dialog = new Microsoft.Win32.SaveFileDialog();
        dialog.Filter = e.Filter;
        dialog.RestoreDirectory = true;
        if (dialog.ShowDialog(Application.Current.MainWindow) == true)
        {
          e.OnCompleted?.Invoke(dialog.FileName);
        }
      }
    }
  }
}
