using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Analysis.Generic;
using KmyKeiba.Models.Race;
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

namespace KmyKeiba.Views.Controls
{
  /// <summary>
  /// TrendAnalyzerAnalysisView.xaml の相互作用ロジック
  /// </summary>
  public partial class TrendAnalyzerAnalysisView : UserControl
  {
    public static readonly DependencyProperty RaceProperty
    = DependencyProperty.Register(
        nameof(Race),
        typeof(RaceInfo),
        typeof(TrendAnalyzerAnalysisView),
        new PropertyMetadata(null));

    public RaceInfo? Race
    {
      get { return (RaceInfo)GetValue(RaceProperty); }
      set { SetValue(RaceProperty, value); }
    }

    public static readonly DependencyProperty RaceHorseProperty
    = DependencyProperty.Register(
        nameof(RaceHorse),
        typeof(RaceHorseAnalyzer),
        typeof(TrendAnalyzerAnalysisView),
        new PropertyMetadata(null));

    public RaceHorseAnalyzer? RaceHorse
    {
      get { return (RaceHorseAnalyzer)GetValue(RaceHorseProperty); }
      set { SetValue(RaceHorseProperty, value); }
    }

    public static readonly DependencyProperty TrendAnalyzersProperty
    = DependencyProperty.Register(
        nameof(TrendAnalyzers),
        typeof(ITrendAnalysisSelector),
        typeof(TrendAnalyzerAnalysisView),
        new PropertyMetadata(null));

    public ITrendAnalysisSelector? TrendAnalyzers
    {
      get { return (ITrendAnalysisSelector)GetValue(TrendAnalyzersProperty); }
      set { SetValue(TrendAnalyzersProperty, value); }
    }

    public static readonly DependencyProperty MenuContentProperty
    = DependencyProperty.Register(
        nameof(MenuContent),
        typeof(object),
        typeof(TrendAnalyzerAnalysisView),
        new PropertyMetadata(null));

    public object? MenuContent
    {
      get { return GetValue(MenuContentProperty); }
      set { SetValue(MenuContentProperty, value); }
    }

    public TrendAnalyzerAnalysisView()
    {
      InitializeComponent();
    }
  }
}
