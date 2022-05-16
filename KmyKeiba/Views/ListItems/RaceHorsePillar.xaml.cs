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
        typeof(RaceHorseAnalyzer),
        typeof(RaceHorsePillar),
        new PropertyMetadata(null));

    public RaceHorseAnalyzer? Horse
    {
      get { return (RaceHorseAnalyzer)GetValue(HorseProperty); }
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

    public static readonly DependencyProperty IsAllRacesProperty
    = DependencyProperty.Register(
        nameof(IsAllRaces),
        typeof(bool),
        typeof(RaceHorsePillar),
        new PropertyMetadata(false, (sender, e) =>
        {
          if (sender is RaceHorsePillar view)
          {
            var binding = new Binding(view.IsAllRaces ? "Horse.History.BeforeRaces" : "Horse.History.BeforeFiveRaces")
            {
              Source = view,
            };
            view.History.SetBinding(ItemsControl.ItemsSourceProperty, binding);
          }
        }));

    public bool IsAllRaces
    {
      get { return (bool)GetValue(IsAllRacesProperty); }
      set { SetValue(IsAllRacesProperty, value); }
    }

    public RaceHorsePillar()
    {
      InitializeComponent();
    }
  }
}
