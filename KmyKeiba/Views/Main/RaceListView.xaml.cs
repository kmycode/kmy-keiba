using KmyKeiba.Models.RList;
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

namespace KmyKeiba.Views.Main
{
  /// <summary>
  /// RaceListView.xaml の相互作用ロジック
  /// </summary>
  public partial class RaceListView : UserControl
  {
    public static readonly DependencyProperty RaceListProperty
    = DependencyProperty.Register(
        nameof(RaceList),
        typeof(RaceList),
        typeof(RaceListView),
        new PropertyMetadata(null));

    public RaceList? RaceList
    {
      get { return (RaceList)GetValue(RaceListProperty); }
      set { SetValue(RaceListProperty, value); }
    }

    public RaceListView()
    {
      InitializeComponent();
    }
  }
}
