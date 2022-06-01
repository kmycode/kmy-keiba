using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Image;
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
  /// RunningStyleTip.xaml の相互作用ロジック
  /// </summary>
  public partial class RunningStyleTip : UserControl
  {
    public static readonly DependencyProperty RunningStyleProperty
      = DependencyProperty.Register(
          nameof(RunningStyle),
          typeof(RunningStyle),
          typeof(RunningStyleTip),
          new PropertyMetadata(KmyKeiba.JVLink.Entities.RunningStyle.Unknown, (sender, e) =>
          {
            if (sender is RunningStyleTip view)
            {
              view.Image.RunningStyle = (RunningStyle)e.NewValue;
            }
          }));

    public RunningStyle? RunningStyle
    {
      get { return (RunningStyle)GetValue(RunningStyleProperty); }
      set { SetValue(RunningStyleProperty, value); }
    }

    public RunningStyleImage Image { get; } = new();

    public RunningStyleTip()
    {
      InitializeComponent();
    }
  }
}
