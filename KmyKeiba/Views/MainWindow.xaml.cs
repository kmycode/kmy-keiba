using KmyKeiba.Models.Threading;
using System.Windows;

namespace KmyKeiba.Views
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      UiThreadUtil.Dispatcher = this.Dispatcher;
      InitializeComponent();
    }
  }
}
