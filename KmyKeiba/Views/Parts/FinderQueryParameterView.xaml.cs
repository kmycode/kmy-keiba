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
  /// FinderQueryParameterView.xaml の相互作用ロジック
  /// </summary>
  public partial class FinderQueryParameterView : UserControl
  {
    public static readonly DependencyProperty ParameterProperty
= DependencyProperty.Register(
 nameof(Parameter),
 typeof(FinderQueryParameterItem),
 typeof(FinderQueryParameterView),
 new PropertyMetadata(null));

    public FinderQueryParameterItem? Parameter
    {
      get { return (FinderQueryParameterItem)GetValue(ParameterProperty); }
      set { SetValue(ParameterProperty, value); }
    }

    public FinderQueryParameterView()
    {
      InitializeComponent();
    }

    private void InputPopup_Opened(object sender, EventArgs e)
    {
      if (sender is Popup popup && popup.Child is Border border)
      {
        if (this.Parameter?.Category is IListBoxInputCategory listbox)
        {
          if (border.Child is InputListBoxView view)
          {
            view.Input = listbox;
            view.HasRace = true;
            view.HasRaceHorse = true;
            view.Header = this.Parameter.Header;
          }
          else
          {
            border.Child = new InputListBoxView
            {
              Input = listbox,
              HasRace = true,
              HasRaceHorse = true,
              Header = this.Parameter.Header,
            };
          }
          popup.Width = 110;
          popup.Height = 400;
        }
        else if (this.Parameter?.Category is NumberInputCategoryBase num)
        {
          if (border.Child is InputNumberView view)
          {
            view.Input = num.Input;
            view.HasRace = true;
            view.HasRaceHorse = true;
            view.Header = this.Parameter.Header;
          }
          else
          {
            border.Child = new InputNumberView
            {
              Input = num.Input,
              HasRace = true,
              HasRaceHorse = true,
              Header = this.Parameter.Header,
            };
          }
          popup.Width = 450;
          popup.Height = 100;
        }
      }
    }
  }
}
