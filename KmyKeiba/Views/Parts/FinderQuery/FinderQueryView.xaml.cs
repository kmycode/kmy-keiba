﻿using KmyKeiba.Models.Race.Finder;
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

    public static readonly DependencyProperty IsSubViewProperty
= DependencyProperty.Register(
 nameof(IsSubView),
 typeof(bool),
 typeof(FinderQueryView),
 new PropertyMetadata(false, (sender, e) =>
 {
   if (sender is FinderQueryView view && e.NewValue is bool b)
   {
     if (b)
     {
       view.S_Horse.IsChecked = true;
     }
     else
     {
       view.S_RaceSubject.IsChecked = true;
     }
   }
 }));

    public bool IsSubView
    {
      get { return (bool)GetValue(IsSubViewProperty); }
      set { SetValue(IsSubViewProperty, value); }
    }

    public static readonly DependencyProperty IsSubViewSameRaceHorseProperty
= DependencyProperty.Register(
 nameof(IsSubViewSameRaceHorse),
 typeof(bool),
 typeof(FinderQueryView),
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
 typeof(FinderQueryView),
 new PropertyMetadata(false));

    public bool IsSubViewBeforeRace
    {
      get { return (bool)GetValue(IsSubViewBeforeRaceProperty); }
      set { SetValue(IsSubViewBeforeRaceProperty, value); }
    }

    public static readonly DependencyProperty IsEnumerableProperty
= DependencyProperty.Register(
 nameof(IsEnumerable),
 typeof(bool),
 typeof(FinderQueryView),
 new PropertyMetadata(false));

    public bool IsEnumerable
    {
      get { return (bool)GetValue(IsEnumerableProperty); }
      set { SetValue(IsEnumerableProperty, value); }
    }

    public Guid UniqueId { get; } = Guid.NewGuid();
    public Guid UniqueId2 { get; } = Guid.NewGuid();
    public Guid UniqueId3 { get; } = Guid.NewGuid();
    public Guid UniqueId4 { get; } = Guid.NewGuid();
    public Guid UniqueId5 { get; } = Guid.NewGuid();
    public Guid UniqueId6 { get; } = Guid.NewGuid();
    public Guid UniqueId7 { get; } = Guid.NewGuid();

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
          if (listbox?.SelectionMode != SelectionMode.Multiple)
          {
            return;
          }

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
