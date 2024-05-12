using KmyKeiba.Models.Race.Finder;
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

namespace KmyKeiba.Views.Parts
{
  /// <summary>
  /// FinderRowListView.xaml の相互作用ロジック
  /// </summary>
  public partial class FinderRowListView : UserControl
  {
    public static readonly DependencyProperty FinderModelProperty
  = DependencyProperty.Register(
     nameof(FinderModel),
     typeof(FinderModel),
     typeof(FinderRowListView),
     new PropertyMetadata((sender, e) => (sender as FinderRowListView)?.OnFinderModelChanged()));

    public FinderModel? FinderModel
    {
      get { return (FinderModel)GetValue(FinderModelProperty); }
      set { SetValue(FinderModelProperty, value); }
    }

    public Guid UniqueId { get; } = Guid.NewGuid();

    public FinderRowListView()
    {
      InitializeComponent();
    }

    private void OnFinderModelChanged()
    {
      this.FinderModel?.OnRendered();
    }
  }
}
