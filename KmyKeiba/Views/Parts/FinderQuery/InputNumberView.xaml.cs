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
  /// InputNumberView.xaml の相互作用ロジック
  /// </summary>
  public partial class InputNumberView : UserControl
  {
    public static readonly DependencyProperty InputProperty
= DependencyProperty.Register(
 nameof(Input),
 typeof(FinderQueryNumberInput),
 typeof(InputNumberView),
 new PropertyMetadata(null));

    public FinderQueryNumberInput? Input
    {
      get { return (FinderQueryNumberInput)GetValue(InputProperty); }
      set { SetValue(InputProperty, value); }
    }

    public static readonly DependencyProperty HeaderProperty
= DependencyProperty.Register(
nameof(Header),
typeof(string),
typeof(InputNumberView),
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
typeof(InputNumberView),
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
 typeof(InputNumberView),
 new PropertyMetadata(false));

    public bool IsComparable
    {
      get { return (bool)GetValue(IsComparableProperty); }
      set { SetValue(IsComparableProperty, value); }
    }

    public static readonly DependencyProperty HasRaceProperty
= DependencyProperty.Register(
 nameof(HasRace),
 typeof(bool),
 typeof(InputNumberView),
 new PropertyMetadata(false));

    public bool HasRace
    {
      get { return (bool)GetValue(HasRaceProperty); }
      set { SetValue(HasRaceProperty, value); }
    }

    public static readonly DependencyProperty HasRaceHorseProperty
= DependencyProperty.Register(
 nameof(HasRaceHorse),
 typeof(bool),
 typeof(InputNumberView),
 new PropertyMetadata(false));

    public bool HasRaceHorse
    {
      get { return (bool)GetValue(HasRaceHorseProperty); }
      set { SetValue(HasRaceHorseProperty, value); }
    }

    public static readonly DependencyProperty CanCompareWithCurrentRaceProperty
= DependencyProperty.Register(
 nameof(CanCompareWithCurrentRace),
 typeof(bool),
 typeof(InputNumberView),
 new PropertyMetadata(true));

    public bool CanCompareWithCurrentRace
    {
      get { return (bool)GetValue(CanCompareWithCurrentRaceProperty); }
      set { SetValue(CanCompareWithCurrentRaceProperty, value); }
    }

    public Guid UniqueId { get; } = Guid.NewGuid();
    public Guid UniqueId2 { get; } = Guid.NewGuid();

    public InputNumberView()
    {
      InitializeComponent();
    }
  }
}
