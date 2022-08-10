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
  /// InputStringView.xaml の相互作用ロジック
  /// </summary>
  public partial class InputStringView : UserControl
  {
    public static readonly DependencyProperty InputProperty
= DependencyProperty.Register(
 nameof(Input),
 typeof(FinderQueryStringInput),
 typeof(InputStringView),
 new PropertyMetadata(null));

    public FinderQueryStringInput? Input
    {
      get { return (FinderQueryStringInput)GetValue(InputProperty); }
      set { SetValue(InputProperty, value); }
    }

    public Guid UniqueId { get; } = Guid.NewGuid();

    public InputStringView()
    {
      InitializeComponent();
    }
  }
}
