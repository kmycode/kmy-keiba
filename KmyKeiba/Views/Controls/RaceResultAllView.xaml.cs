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
  /// RaceResultAllView.xaml の相互作用ロジック
  /// </summary>
  public partial class RaceResultAllView : UserControl
  {
    public static readonly DependencyProperty RaceProperty
    = DependencyProperty.Register(
        nameof(Race),
        typeof(RaceInfo),
        typeof(RaceResultAllView),
        new PropertyMetadata(null));

    public RaceInfo? Race
    {
      get { return (RaceInfo)GetValue(RaceProperty); }
      set { SetValue(RaceProperty, value); }
    }

    public Guid UniqueId3 { get; } = Guid.NewGuid();

    public Guid UniqueId4 { get; } = Guid.NewGuid();

    public Guid UniqueId5 { get; } = Guid.NewGuid();

    public RaceResultAllView()
    {
      InitializeComponent();
    }
  }
}
