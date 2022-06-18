using KmyKeiba.Models.Analysis.Table;
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

namespace KmyKeiba.Views.Details
{
  /// <summary>
  /// AnalysisTableGroupView.xaml の相互作用ロジック
  /// </summary>
  public partial class AnalysisTableGroupView : UserControl
  {
    public static readonly DependencyProperty TableGroupProperty
    = DependencyProperty.Register(
        nameof(TableGroup),
        typeof(AnalysisTableList),
        typeof(AnalysisTableGroupView),
        new PropertyMetadata(null));

    public AnalysisTableList? TableGroup
    {
      get { return (AnalysisTableList)GetValue(TableGroupProperty); }
      set { SetValue(TableGroupProperty, value); }
    }

    public Guid UniqueId { get; } = Guid.NewGuid();

    public AnalysisTableGroupView()
    {
      InitializeComponent();
    }
  }
}
