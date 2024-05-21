using KmyKeiba.Models.Race.Finder;
using KmyKeiba.Models.Setting;
using KmyKeiba.Views.Expansions;
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
  /// GeneralConfigView.xaml の相互作用ロジック
  /// </summary>
  public partial class GeneralConfigView : UserControl
  {
    public static readonly DependencyProperty AppGeneralConfigProperty
    = DependencyProperty.Register(
        nameof(GeneralConfig),
        typeof(AppGeneralConfig),
        typeof(GeneralConfigView),
        new PropertyMetadata(null));

    public AppGeneralConfig? GeneralConfig
    {
      get { return (AppGeneralConfig)GetValue(AppGeneralConfigProperty); }
      set { SetValue(AppGeneralConfigProperty, value); }
    }

    public GeneralConfigView()
    {
      InitializeComponent();
    }
  }
}
