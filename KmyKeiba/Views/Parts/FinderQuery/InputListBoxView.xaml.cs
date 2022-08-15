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
  /// InputListBoxView.xaml の相互作用ロジック
  /// </summary>
  public partial class InputListBoxView : UserControl
  {
    public static readonly DependencyProperty InputProperty
= DependencyProperty.Register(
nameof(Input),
typeof(IListBoxInputCategory),
typeof(InputListBoxView),
new PropertyMetadata(null));

    public IListBoxInputCategory? Input
    {
      get { return (IListBoxInputCategory)GetValue(InputProperty); }
      set { SetValue(InputProperty, value); }
    }

    public static readonly DependencyProperty HeaderProperty
= DependencyProperty.Register(
nameof(Header),
typeof(string),
typeof(InputListBoxView),
new PropertyMetadata(null));

    public string? Header
    {
      get { return (string)GetValue(HeaderProperty); }
      set { SetValue(HeaderProperty, value); }
    }

    public static readonly DependencyProperty CommentProperty
= DependencyProperty.Register(
nameof(Comment),
typeof(string),
typeof(InputListBoxView),
new PropertyMetadata(null));

    public string? Comment
    {
      get { return (string)GetValue(CommentProperty); }
      set { SetValue(CommentProperty, value); }
    }

    public static readonly DependencyProperty IsComparableProperty
= DependencyProperty.Register(
 nameof(IsComparable),
 typeof(bool),
 typeof(InputListBoxView),
 new PropertyMetadata(false));

    public bool IsComparable
    {
      get { return (bool)GetValue(IsComparableProperty); }
      set { SetValue(IsComparableProperty, value); }
    }

    public Guid UniqueId { get; } = Guid.NewGuid();

    public InputListBoxView()
    {
      InitializeComponent();
    }

    private void ListBoxItem_MouseEnter(object sender, MouseEventArgs e)
    {
      System.Diagnostics.Debug.WriteLine(e.LeftButton);
      if (e.LeftButton == MouseButtonState.Pressed)
      {
        if (sender is ListBoxItem lbi)
        {
          var listbox = this.GetListBox(lbi);
          if (!lbi.IsSelected)
          {
            lbi.IsSelected = true;
            lbi.Focus();
            listbox?.SelectedItems.Add(lbi);
          }
          else
          {
            lbi.IsSelected = false;
            lbi.Focus();
            listbox?.SelectedItems.Remove(lbi);
          }
        }
      }
    }

    private ListBox? GetListBox(ListBoxItem item)
    {
      var element = (DependencyObject)item;
      while (element != null && element is not ListBox)
      {
        element = VisualTreeHelper.GetParent(element);
      }
      return element as ListBox;
    }
  }
}
