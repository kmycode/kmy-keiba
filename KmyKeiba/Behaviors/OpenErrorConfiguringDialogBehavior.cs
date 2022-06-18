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
  class OpenErrorConfigringDialogBehavior : Behavior<MainWindow>
  {

    public static readonly DependencyProperty ControllerProperty
        = DependencyProperty.Register(
            nameof(Controller),
            typeof(OpenErrorConfiguringRequest),
            typeof(OpenErrorConfigringDialogBehavior),
            new PropertyMetadata(null, (sender, e) =>
            {
              if (sender is OpenErrorConfigringDialogBehavior view)
              {
                if (e.OldValue is OpenErrorConfiguringRequest old)
                {
                  old.Requested -= view.OnRequested;
                }
                if (e.NewValue is OpenErrorConfiguringRequest @new)
                {
                  @new.Requested += view.OnRequested;
                }
              }
            }));

    public OpenErrorConfiguringRequest? Controller
    {
      get { return (OpenErrorConfiguringRequest)GetValue(ControllerProperty); }
      set { SetValue(ControllerProperty, value); }
    }

    private void OnRequested(object? sender, OpenErrorConfiguringRequestEventArgs e)
    {
      this.Dispatcher.Invoke(() =>
      {
        if (this.AssociatedObject?.DataContext is MainViewModel viewModel)
        {
          viewModel.ErrorConfigringMessage.Value = e.Message;
          viewModel.CurrentDialog.Value = DialogType.ErrorConfigring;
        }
      });
    }
  }
}
