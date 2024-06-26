﻿using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Race;
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

namespace KmyKeiba.Views.Details
{
  /// <summary>
  /// RaceExpectHorseView.xaml の相互作用ロジック
  /// </summary>
  public partial class RaceExpectHorseView : UserControl
  {
    public static readonly DependencyProperty RaceProperty
    = DependencyProperty.Register(
        nameof(Race),
        typeof(RaceInfo),
        typeof(RaceExpectHorseView),
        new PropertyMetadata(null));

    public RaceInfo? Race
    {
      get { return (RaceInfo)GetValue(RaceProperty); }
      set { SetValue(RaceProperty, value); }
    }

    public static readonly DependencyProperty RaceHorseProperty
    = DependencyProperty.Register(
        nameof(RaceHorse),
        typeof(RaceHorseAnalyzer),
        typeof(RaceExpectHorseView),
        new PropertyMetadata((sender, e) => (sender as RaceExpectHorseView)?.OnHorseChanged()));

    public RaceHorseAnalyzer? RaceHorse
    {
      get { return (RaceHorseAnalyzer)GetValue(RaceHorseProperty); }
      set { SetValue(RaceHorseProperty, value); }
    }

    public Guid UniqueId { get; } = Guid.NewGuid();

    public Guid UniqueId2 { get; } = Guid.NewGuid();

    public Guid UniqueId3 { get; } = Guid.NewGuid();

    public RaceExpectHorseView()
    {
      InitializeComponent();

      this.S_Finder.Checked += (sender, e) => this.OnHorseChanged();
    }

    private void OnHorseChanged()
    {
      if (this.RaceHorse == null)
      {
        this.FinderKeyInput.FinderModel = null;
        this.FinderRowList.FinderModel = null;
        return;
      }

      if (this.S_Finder.IsChecked == true)
      {
        this.FinderKeyInput.FinderModel = this.RaceHorse.FinderModel.Value;
        this.FinderRowList.FinderModel = this.RaceHorse.FinderModel.Value;
      }
    }
  }
}
