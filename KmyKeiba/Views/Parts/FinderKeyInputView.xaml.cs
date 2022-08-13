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

namespace KmyKeiba.Views.Parts
{
    /// <summary>
    /// FinderKeyInputView.xaml の相互作用ロジック
    /// </summary>
    public partial class FinderKeyInputView : UserControl
    {
    public static readonly DependencyProperty FinderModelProperty
= DependencyProperty.Register(
 nameof(FinderModel),
 typeof(FinderModel),
 typeof(FinderKeyInputView),
 new PropertyMetadata(null));

    public FinderModel? FinderModel
    {
      get { return (FinderModel)GetValue(FinderModelProperty); }
      set { SetValue(FinderModelProperty, value); }
    }

    public static readonly DependencyProperty IsSubViewProperty
= DependencyProperty.Register(
 nameof(IsSubView),
 typeof(bool),
 typeof(FinderKeyInputView),
 new PropertyMetadata(false));

    public bool IsSubView
    {
      get { return (bool)GetValue(IsSubViewProperty); }
      set { SetValue(IsSubViewProperty, value); }
    }

    public static readonly DependencyProperty IsSubViewSameRaceHorseProperty
= DependencyProperty.Register(
 nameof(IsSubViewSameRaceHorse),
 typeof(bool),
 typeof(FinderKeyInputView),
 new PropertyMetadata(false));

    public bool IsSubViewSameRaceHorse
    {
      get { return (bool)GetValue(IsSubViewSameRaceHorseProperty); }
      set { SetValue(IsSubViewSameRaceHorseProperty, value); }
    }

    public static readonly DependencyProperty IsSubViewBeforeRaceProperty
= DependencyProperty.Register(
 nameof(IsSubViewBeforeRace),
 typeof(bool),
 typeof(FinderKeyInputView),
 new PropertyMetadata(false));

    public bool IsSubViewBeforeRace
    {
      get { return (bool)GetValue(IsSubViewBeforeRaceProperty); }
      set { SetValue(IsSubViewBeforeRaceProperty, value); }
    }

    public FinderKeyInputView()
    {
      InitializeComponent();
      this.SearchButton.Click += (sender, e) =>
      {
        this.FinderModel?.BeginLoad();
      };
    }
  }
}
