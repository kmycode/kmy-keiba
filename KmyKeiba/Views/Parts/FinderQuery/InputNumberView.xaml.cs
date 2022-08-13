using KmyKeiba.Models.Race.Finder;
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

namespace KmyKeiba.Views.Parts.FinderQuery
{
  /// <summary>
  /// InputNumberView.xaml の相互作用ロジック
  /// </summary>
  public partial class InputNumberView : UserControl
  {
    public static readonly DependencyProperty InputProperty
= DependencyProperty.Register(
 nameof(Input),
 typeof(FinderQueryNumberInput),
 typeof(InputNumberView),
 new PropertyMetadata(null));

    public FinderQueryNumberInput? Input
    {
      get { return (FinderQueryNumberInput)GetValue(InputProperty); }
      set { SetValue(InputProperty, value); }
    }

    public static readonly DependencyProperty IsComparableProperty
= DependencyProperty.Register(
 nameof(IsComparable),
 typeof(bool),
 typeof(InputNumberView),
 new PropertyMetadata(false));

    public bool IsComparable
    {
      get { return (bool)GetValue(IsComparableProperty); }
      set { SetValue(IsComparableProperty, value); }
    }

    public Guid UniqueId { get; } = Guid.NewGuid();
    public Guid UniqueId2 { get; } = Guid.NewGuid();

    public InputNumberView()
    {
      InitializeComponent();
    }
  }
}
