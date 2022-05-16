using KmyKeiba.Models.Analysis.Generic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace KmyKeiba.Views.Controls
{
  /// <summary>
  /// TrendAnalyzerView.xaml の相互作用ロジック
  /// </summary>
  public partial class TrendAnalyzerView : UserControl
  {
    public static readonly DependencyProperty SelectorProperty
    = DependencyProperty.Register(
        nameof(Selector),
        typeof(ITrendAnalysisSelector),
        typeof(TrendAnalyzerView),
        new PropertyMetadata(null));

    public ITrendAnalysisSelector? Selector
    {
      get { return (ITrendAnalysisSelector)GetValue(SelectorProperty); }
      set { SetValue(SelectorProperty, value); }
    }

    public static readonly DependencyProperty MenuContentProperty
    = DependencyProperty.Register(
        nameof(MenuContent),
        typeof(object),
        typeof(TrendAnalyzerView),
        new PropertyMetadata(null));

    public object? MenuContent
    {
      get { return GetValue(MenuContentProperty); }
      set { SetValue(MenuContentProperty, value); }
    }

    public Guid UniqueId { get; } = Guid.NewGuid();

    public TrendAnalyzerView()
    {
      InitializeComponent();
    }

    private void AnalysisButton_Click(object sender, RoutedEventArgs e)
    {
      this.Selector?.BeginLoad();
    }
  }
}
