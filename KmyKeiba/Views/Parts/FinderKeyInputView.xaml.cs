using KmyKeiba.Models.Race.Finder;
using KmyKeiba.Views.Parts.FinderQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

    public static readonly DependencyProperty IsGenerateDefaultPopupProperty
= DependencyProperty.Register(
 nameof(IsGenerateDefaultPopup),
 typeof(bool),
 typeof(FinderKeyInputView),
 new PropertyMetadata(false, (sender, e) =>
 {
   if (e.NewValue is bool b && b && sender is FinderKeyInputView view)
   {
     view.GeneratePopup();
   }
 }));

    public bool IsGenerateDefaultPopup
    {
      get { return (bool)GetValue(IsGenerateDefaultPopupProperty); }
      set { SetValue(IsGenerateDefaultPopupProperty, value); }
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

    public static readonly DependencyProperty CanCompareWithCurrentRaceProperty
= DependencyProperty.Register(
 nameof(CanCompareWithCurrentRace),
 typeof(bool),
 typeof(FinderKeyInputView),
 new PropertyMetadata(true));

    public bool CanCompareWithCurrentRace
    {
      get { return (bool)GetValue(CanCompareWithCurrentRaceProperty); }
      set { SetValue(CanCompareWithCurrentRaceProperty, value); }
    }

    public static readonly DependencyProperty IsShowFinderButtonOnlyProperty
= DependencyProperty.Register(
 nameof(IsShowFinderButtonOnly),
 typeof(bool),
 typeof(FinderKeyInputView),
 new PropertyMetadata(false));

    public bool IsShowFinderButtonOnly
    {
      get { return (bool)GetValue(IsShowFinderButtonOnlyProperty); }
      set { SetValue(IsShowFinderButtonOnlyProperty, value); }
    }

    public static readonly DependencyProperty IsEnumerableProperty
= DependencyProperty.Register(
 nameof(IsEnumerable),
 typeof(bool),
 typeof(FinderKeyInputView),
 new PropertyMetadata(false));

    public bool IsEnumerable
    {
      get { return (bool)GetValue(IsEnumerableProperty); }
      set { SetValue(IsEnumerableProperty, value); }
    }

    public FinderKeyInputView()
    {
      InitializeComponent();
      this.SearchButton.Click += (sender, e) =>
      {
        this.FinderModel?.BeginLoad();
      };
    }

    private void GeneratePopup()
    {
      var popup = this.QueryPopup;

      var border = (Border)popup.Child;
      if (border.Child == null)
      {
        border.Child = new FinderQueryView
        {
          FinderModel = this.FinderModel,
          IsSubView = this.IsSubView,
          IsSubViewSameRaceHorse = this.IsSubViewSameRaceHorse,
          IsSubViewBeforeRace = this.IsSubViewBeforeRace,
          IsEnumerable = this.IsEnumerable,
        };
      }
      else
      {
        var view = (FinderQueryView)border.Child;
        view.FinderModel = this.FinderModel;
        view.IsSubView = this.IsSubView;
        view.IsSubViewBeforeRace = this.IsSubViewBeforeRace;
        view.IsSubViewSameRaceHorse = this.IsSubViewSameRaceHorse;
        view.IsEnumerable = this.IsEnumerable;
      }
    }

    private void ImePopup_Opened(object sender, EventArgs e)
    {
      this.GeneratePopup();
    }

    private void ImePopup_Closed(object sender, EventArgs e)
    {

    }
  }
}
