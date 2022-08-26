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
  /// AnalysisTableCellDetail.xaml の相互作用ロジック
  /// </summary>
  public partial class AnalysisTableCellDetail : UserControl
  {
    public static readonly DependencyProperty CellProperty
    = DependencyProperty.Register(
        nameof(Cell),
        typeof(AnalysisTableCell),
        typeof(AnalysisTableCellDetail),
        new PropertyMetadata(null));

    public AnalysisTableCell? Cell
    {
      get { return (AnalysisTableCell)GetValue(CellProperty); }
      set { SetValue(CellProperty, value); }
    }

    public static readonly DependencyProperty MyDataContextProperty
    = DependencyProperty.Register(
        nameof(MyDataContext),
        typeof(object),
        typeof(AnalysisTableCellDetail),
        new PropertyMetadata(null));

    public object? MyDataContext
    {
      get { return GetValue(MyDataContextProperty); }
      set { SetValue(MyDataContextProperty, value); }
    }

    public AnalysisTableCellDetail()
    {
      InitializeComponent();
    }
  }
}
