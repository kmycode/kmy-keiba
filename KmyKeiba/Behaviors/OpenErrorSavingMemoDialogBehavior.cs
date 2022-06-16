using KmyKeiba.Common;
using KmyKeiba.ViewModels;
using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KmyKeiba.Behaviors
{
  class OpenErrorSavingMemoDialogBehavior : Behavior<MainWindow>
  {

    public static readonly DependencyProperty ControllerProperty
        = DependencyProperty.Register(
            nameof(Controller),
            typeof(OpenErrorSavingMemoRequest),
            typeof(OpenErrorSavingMemoDialogBehavior),
            new PropertyMetadata(null, (sender, e) =>
            {
              if (sender is OpenErrorSavingMemoDialogBehavior view)
              {
                if (e.OldValue is OpenErrorSavingMemoRequest old)
                {
                  old.Requested -= view.OnRequested;
                }
                if (e.NewValue is OpenErrorSavingMemoRequest @new)
                {
                  @new.Requested += view.OnRequested;
                }
              }
            }));

    public OpenErrorSavingMemoRequest? Controller
    {
      get { return (OpenErrorSavingMemoRequest)GetValue(ControllerProperty); }
      set { SetValue(ControllerProperty, value); }
    }

    private void OnRequested(object? sender, OpenErrorSavingMemoRequestEventArgs e)
    {
      this.Dispatcher.Invoke(() =>
      {
        if (this.AssociatedObject?.DataContext is MainViewModel viewModel)
        {
          viewModel.ErrorSavingMemoText.Value = e.Memo;
          viewModel.CurrentDialog.Value = DialogType.ErrorSavingMemo;
        }
      });
    }
  }
}
