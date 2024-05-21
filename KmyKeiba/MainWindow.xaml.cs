using CefSharp.Wpf;
using KmyKeiba.Models.Image;
using KmyKeiba.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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

namespace KmyKeiba
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();

      this.DataContext = new MainViewModel();

      this.Closing += (_, _) => ((MainViewModel)this.DataContext).OnApplicationExit();
    }

    // https://stackoverflow.com/questions/18113597/wpf-handedness-with-popups

    private static readonly FieldInfo? _menuDropAlignmentField;

    static MainWindow()
    {
      _menuDropAlignmentField = typeof(SystemParameters).GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);
      System.Diagnostics.Debug.Assert(_menuDropAlignmentField != null);

      EnsureStandardPopupAlignment();
      SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
    }

    private static void SystemParameters_StaticPropertyChanged(object? sender, PropertyChangedEventArgs e)
      => EnsureStandardPopupAlignment();

    private static void EnsureStandardPopupAlignment()
    {
      if (SystemParameters.MenuDropAlignment)
      {
        _menuDropAlignmentField?.SetValue(null, false);
      }
    }
  }
}
