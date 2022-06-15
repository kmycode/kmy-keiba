using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KmyKeiba.Views.Main
{
    /// <summary>
    /// WindowMenu.xaml の相互作用ロジック
    /// </summary>
  public partial class WindowMenu : UserControl
  {

    public WindowMenu()
    {
      InitializeComponent();

      this.LayoutUpdated += this.WindowMenu_LayoutUpdated;
      this.NormalButton.Visibility = Visibility.Collapsed;
    }

    private void WindowMenu_LayoutUpdated(object? sender, EventArgs e)
    {
      try
      {
        this.GetWindow().StateChanged += (sender, e) =>
        {
          var state = ((Window)sender!).WindowState;
          if (state == WindowState.Maximized)
          {
            this.MaximumButton.Visibility = Visibility.Collapsed;
            this.NormalButton.Visibility = Visibility.Visible;
          }
          else if (state == WindowState.Normal)
          {
            this.MaximumButton.Visibility = Visibility.Visible;
            this.NormalButton.Visibility = Visibility.Collapsed;
          }
        };
        this.LayoutUpdated -= this.WindowMenu_LayoutUpdated;
      }
      catch { }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
      this.GetWindow().Close();
    }

    private Window GetWindow()
    {
      var element = (DependencyObject)this;
      while (element != null)
      {
        element = VisualTreeHelper.GetParent(element);
        if (element is Window window)
        {
          return window;
        }
      }
      throw new InvalidOperationException();
    }

    private void MaxButton_Click(object sender, RoutedEventArgs e)
    {
      this.GetWindow().WindowState = WindowState.Maximized;
    }

    private void NormalButton_Click(object sender, RoutedEventArgs e)
    {
      this.GetWindow().WindowState = WindowState.Normal;
    }

    private void MinButton_Click(object sender, RoutedEventArgs e)
    {
      this.GetWindow().WindowState = WindowState.Minimized;
    }
  }
}
