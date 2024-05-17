using KmyKeiba.Models.Race.Memo;
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

namespace KmyKeiba.Views.Expansions
{
  /// <summary>
  /// ExpansionMemoLabelConfigView.xaml の相互作用ロジック
  /// </summary>
  public partial class ExpansionMemoLabelConfigView : UserControl
  {
    public static readonly DependencyProperty LabelConfigProperty
    = DependencyProperty.Register(
        nameof(LabelConfig),
        typeof(PointLabelModel),
        typeof(ExpansionMemoLabelConfigView),
        new PropertyMetadata(null));

    public PointLabelModel? LabelConfig
    {
      get { return (PointLabelModel)GetValue(LabelConfigProperty); }
      set { SetValue(LabelConfigProperty, value); }
    }

    public Guid UniqueId5 { get; } = Guid.NewGuid();

    public ExpansionMemoLabelConfigView()
    {
      InitializeComponent();
    }
  }
}
