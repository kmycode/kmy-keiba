using KmyKeiba.Models.Race;
using KmyKeiba.Models.Race.AnalysisTable;
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
  /// AnalysisTableAggregateView.xaml の相互作用ロジック
  /// </summary>
  public partial class AnalysisTableAggregateView : UserControl
  {
    public static readonly DependencyProperty AnalysisTableProperty
    = DependencyProperty.Register(
        nameof(AnalysisTable),
        typeof(AnalysisTableModel),
        typeof(AnalysisTableAggregateView),
        new PropertyMetadata(null));

    public AnalysisTableModel? AnalysisTable
    {
      get { return (AnalysisTableModel)GetValue(AnalysisTableProperty); }
      set { SetValue(AnalysisTableProperty, value); }
    }

    public AnalysisTableAggregateView()
    {
      InitializeComponent();
    }
  }
}
