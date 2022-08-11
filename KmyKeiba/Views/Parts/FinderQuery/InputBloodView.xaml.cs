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
  /// InputBloodView.xaml の相互作用ロジック
  /// </summary>
  public partial class InputBloodView : UserControl
  {
    public static readonly DependencyProperty InputProperty
= DependencyProperty.Register(
 nameof(Input),
 typeof(FinderQueryBloodRelationInput),
 typeof(InputBloodView),
 new PropertyMetadata(null));

    public FinderQueryBloodRelationInput? Input
    {
      get { return (FinderQueryBloodRelationInput)GetValue(InputProperty); }
      set { SetValue(InputProperty, value); }
    }

    public Guid UniqueId { get; } = Guid.NewGuid();

    public InputBloodView()
    {
      InitializeComponent();
    }
  }
}
