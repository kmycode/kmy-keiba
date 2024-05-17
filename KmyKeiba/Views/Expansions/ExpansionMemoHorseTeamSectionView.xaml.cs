using KmyKeiba.Models.Race;
using KmyKeiba.Models.Race.Memo;
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

namespace KmyKeiba.Views.Expansions
{
  /// <summary>
  /// ExpansionMemoHorseTeamSectionView.xaml の相互作用ロジック
  /// </summary>
  public partial class ExpansionMemoHorseTeamSectionView : UserControl
  {
    public static readonly DependencyProperty SectionProperty
    = DependencyProperty.Register(
        nameof(Section),
        typeof(HorseTeamSection),
        typeof(ExpansionMemoHorseTeamSectionView),
        new PropertyMetadata(null));

    public HorseTeamSection? Section
    {
      get { return (HorseTeamSection)GetValue(SectionProperty); }
      set { SetValue(SectionProperty, value); }
    }

    public ExpansionMemoHorseTeamSectionView()
    {
      InitializeComponent();
    }
  }
}
