using KmyKeiba.Common;
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
  /// HorseMarkButton.xaml の相互作用ロジック
  /// </summary>
  public partial class HorseMarkButton : UserControl
  {
    public static readonly DependencyProperty HorseProperty
    = DependencyProperty.Register(
        nameof(Horse),
        typeof(IHorseMarkSetter),
        typeof(HorseMarkButton),
        new PropertyMetadata(null));

    public IHorseMarkSetter? Horse
    {
      get { return (IHorseMarkSetter)GetValue(HorseProperty); }
      set { SetValue(HorseProperty, value); }
    }

    public HorseMarkButton()
    {
      InitializeComponent();
    }
  }
}
