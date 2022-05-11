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
  /// RaceResultOrderGradeRow.xaml の相互作用ロジック
  /// </summary>
  public partial class RaceResultOrderGradeRow : UserControl
  {
    public static readonly DependencyProperty GradeProperty
      = DependencyProperty.Register(
          nameof(Grade),
          typeof(ResultOrderGradeMap),
          typeof(RaceResultOrderGradeRow),
          new PropertyMetadata(default(ResultOrderGradeMap)));

    public ResultOrderGradeMap Grade
    {
      get { return (ResultOrderGradeMap)GetValue(GradeProperty); }
      set { SetValue(GradeProperty, value); }
    }

    public static readonly DependencyProperty HeaderProperty
      = DependencyProperty.Register(
          nameof(Header),
          typeof(string),
          typeof(RaceResultOrderGradeRow),
          new PropertyMetadata(string.Empty));

    public string? Header
    {
      get { return (string)GetValue(HeaderProperty); }
      set { SetValue(HeaderProperty, value); }
    }

    public RaceResultOrderGradeRow()
    {
      InitializeComponent();
    }
  }
}
