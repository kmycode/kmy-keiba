using KmyKeiba.Models.Race.Finder;
using KmyKeiba.Models.Setting;
using KmyKeiba.Views.Dialogs;
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

namespace KmyKeiba.Views.Settings
{
  /// <summary>
  /// FinderConfigView.xaml の相互作用ロジック
  /// </summary>
  public partial class FinderConfigView : UserControl
  {
    public static readonly DependencyProperty FinderConfigProperty
    = DependencyProperty.Register(
        nameof(FinderConfig),
        typeof(FinderConfigModel),
        typeof(FinderConfigView),
        new PropertyMetadata(null));

    public FinderConfigModel? FinderConfig
    {
      get { return (FinderConfigModel)GetValue(FinderConfigProperty); }
      set { SetValue(FinderConfigProperty, value); }
    }

    public FinderConfigView()
    {
      InitializeComponent();
    }
  }
}
