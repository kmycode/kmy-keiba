using KmyKeiba.Models.Race.ExNumber;
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
    /// ExternalNumberConfigView.xaml の相互作用ロジック
    /// </summary>
    public partial class ExternalNumberConfigView : UserControl
  {
    public static readonly DependencyProperty ExternalNumberProperty
    = DependencyProperty.Register(
        nameof(ExternalNumber),
        typeof(ExternalNumberConfigModel),
        typeof(ExternalNumberConfigView),
        new PropertyMetadata(null));

    public ExternalNumberConfigModel? ExternalNumber
    {
      get { return (ExternalNumberConfigModel)GetValue(ExternalNumberProperty); }
      set { SetValue(ExternalNumberProperty, value); }
    }

    public Guid UniqueId1 { get; } = Guid.NewGuid();
    public Guid UniqueId2 { get; } = Guid.NewGuid();
    public Guid UniqueId3 { get; } = Guid.NewGuid();
    public Guid UniqueId5 { get; } = Guid.NewGuid();

    public ExternalNumberConfigView()
        {
            InitializeComponent();
        }
    }
}
