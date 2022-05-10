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

namespace KmyKeiba.Views.ListItems
{
  /// <summary>
  /// RaceHorsePillar.xaml の相互作用ロジック
  /// </summary>
  public partial class RaceHorsePillar : UserControl
  {
    public static readonly DependencyProperty HorseProperty
    = DependencyProperty.Register(
        nameof(Horse),
        typeof(RaceHorseAnalysisData),
        typeof(RaceHorsePillar),
        new PropertyMetadata(null));

    public RaceHorseAnalysisData? Horse
    {
      get { return (RaceHorseAnalysisData)GetValue(HorseProperty); }
      set { SetValue(HorseProperty, value); }
    }

    public static readonly DependencyProperty IsResultProperty
    = DependencyProperty.Register(
        nameof(IsResult),
        typeof(bool),
        typeof(RaceHorsePillar),
        new PropertyMetadata(false));

    public bool IsResult
    {
      get { return (bool)GetValue(IsResultProperty); }
      set { SetValue(IsResultProperty, value); }
    }

    public RaceHorsePillar()
    {
      InitializeComponent();
    }
  }
}
