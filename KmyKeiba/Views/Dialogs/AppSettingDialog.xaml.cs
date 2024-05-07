using KmyKeiba.Models.Script;
using KmyKeiba.Models.Setting;
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

namespace KmyKeiba.Views.Dialogs
{
  /// <summary>
  /// AppSettingDialog.xaml の相互作用ロジック
  /// </summary>
  public partial class AppSettingDialog : UserControl
  {
    public static readonly DependencyProperty AppSettingsProperty
    = DependencyProperty.Register(
        nameof(AppSettings),
        typeof(AppSettingsModel),
        typeof(AppSettingDialog),
        new PropertyMetadata(null));

    public AppSettingsModel? AppSettings
    {
      get { return (AppSettingsModel)GetValue(AppSettingsProperty); }
      set { SetValue(AppSettingsProperty, value); }
    }

    public Guid UniqueId { get; } = Guid.NewGuid();

    public AppSettingDialog()
    {
      InitializeComponent();
    }
  }
}
