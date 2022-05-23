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

namespace KmyKeiba.Views.Details
{
  /// <summary>
  /// RaceCornerView.xaml の相互作用ロジック
  /// </summary>
  public partial class RaceCornerView : UserControl
  {
    public static readonly DependencyProperty RaceCornerProperty
    = DependencyProperty.Register(
        nameof(RaceCorner),
        typeof(RaceCorner),
        typeof(RaceCornerView),
        new PropertyMetadata(null));

    public RaceCorner? RaceCorner
    {
      get { return (RaceCorner)GetValue(RaceCornerProperty); }
      set { SetValue(RaceCornerProperty, value); }
    }

    public RaceCornerView()
    {
      InitializeComponent();
    }
  }
}
