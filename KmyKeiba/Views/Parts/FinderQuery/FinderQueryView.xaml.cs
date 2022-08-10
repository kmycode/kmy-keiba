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
  /// FinderQueryView.xaml の相互作用ロジック
  /// </summary>
  public partial class FinderQueryView : UserControl
  {
    public static readonly DependencyProperty FinderModelProperty
= DependencyProperty.Register(
 nameof(FinderModel),
 typeof(FinderModel),
 typeof(FinderQueryView),
 new PropertyMetadata(null));

    public FinderModel? FinderModel
    {
      get { return (FinderModel)GetValue(FinderModelProperty); }
      set { SetValue(FinderModelProperty, value); }
    }

    public Guid UniqueId { get; } = Guid.NewGuid();
    public Guid UniqueId2 { get; } = Guid.NewGuid();
    public Guid UniqueId3 { get; } = Guid.NewGuid();
    public Guid UniqueId4 { get; } = Guid.NewGuid();

    public FinderQueryView()
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
            System.Diagnostics.Debug.WriteLine("- changed");
          }
          else
          {
            lbi.IsSelected = false;
            lbi.Focus();
            listbox?.SelectedItems.Remove(lbi);
            System.Diagnostics.Debug.WriteLine("- changed");
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
