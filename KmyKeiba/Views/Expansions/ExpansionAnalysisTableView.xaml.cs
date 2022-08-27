using KmyKeiba.Models.Race;
using KmyKeiba.Models.Race.AnalysisTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
  /// ExpansionAnalysisTableView.xaml の相互作用ロジック
  /// </summary>
  public partial class ExpansionAnalysisTableView : UserControl
  {
    public static readonly DependencyProperty RaceProperty
    = DependencyProperty.Register(
        nameof(Race),
        typeof(RaceInfo),
        typeof(ExpansionAnalysisTableView),
        new PropertyMetadata(null));

    public RaceInfo? Race
    {
      get { return (RaceInfo)GetValue(RaceProperty); }
      set { SetValue(RaceProperty, value); }
    }

    public static readonly DependencyProperty AnalysisTableProperty
    = DependencyProperty.Register(
        nameof(AnalysisTable),
        typeof(AnalysisTableModel),
        typeof(ExpansionAnalysisTableView),
        new PropertyMetadata(null));

    public AnalysisTableModel? AnalysisTable
    {
      get { return (AnalysisTableModel)GetValue(AnalysisTableProperty); }
      set { SetValue(AnalysisTableProperty, value); }
    }

    public Guid UniqueId3 { get; } = Guid.NewGuid();

    public ExpansionAnalysisTableView()
    {
      InitializeComponent();
    }

    private void ImePopup_Opened(object sender, EventArgs e)
    {
      if (sender is Popup popup)
      {
        if (popup.Child is Border border)
        {
          if (border.Child == null)
          {
            var view = new AnalysisTableCellDetail
            {
              MyDataContext = this.DataContext,
            };
            var cell = new Binding();
            view.SetBinding(AnalysisTableCellDetail.CellProperty, cell);
            border.Child = view;
          }
        }
      }
    }
  }
}
