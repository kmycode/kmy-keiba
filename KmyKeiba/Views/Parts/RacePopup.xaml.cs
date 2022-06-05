using KmyKeiba.Models.Analysis;
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
  /// RacePopup.xaml の相互作用ロジック
  /// </summary>
  public partial class RacePopup : UserControl
  {
    public static readonly DependencyProperty RaceProperty
 = DependencyProperty.Register(
     nameof(Race),
     typeof(RaceAnalyzer),
     typeof(RacePopup),
     new PropertyMetadata(null));

    public RaceAnalyzer? Race
    {
      get { return (RaceAnalyzer)GetValue(RaceProperty); }
      set { SetValue(RaceProperty, value); }
    }

    public RacePopup()
    {
      InitializeComponent();
    }
  }
}
