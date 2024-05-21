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
using static KmyKeiba.Models.Analysis.RaceHorseBloodModel;

namespace KmyKeiba.Views.Parts
{
    /// <summary>
    /// HorseBloodParentPanel.xaml の相互作用ロジック
    /// </summary>
    public partial class HorseBloodParentPanel : UserControl
  {
    public static readonly DependencyProperty ItemProperty
= DependencyProperty.Register(
 nameof(Item),
 typeof(IBloodCheckableItem),
 typeof(HorseBloodParentPanel),
 new PropertyMetadata(null));

    public IBloodCheckableItem? Item
    {
      get { return (IBloodCheckableItem)GetValue(ItemProperty); }
      set { SetValue(ItemProperty, value); }
    }

    public HorseBloodParentPanel()
        {
            InitializeComponent();
        }
    }
}
