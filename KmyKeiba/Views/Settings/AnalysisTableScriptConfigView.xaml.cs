﻿using KmyKeiba.Models.Race.AnalysisTable;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace KmyKeiba.Views.Settings
{
  /// <summary>
  /// AnalysisTableScriptConfigView.xaml の相互作用ロジック
  /// </summary>
  public partial class AnalysisTableScriptConfigView : UserControl
  {
    public static readonly DependencyProperty AnalysisTableConfigProperty
    = DependencyProperty.Register(
        nameof(AnalysisTableConfig),
        typeof(AnalysisTableConfigModel),
        typeof(AnalysisTableScriptConfigView),
        new PropertyMetadata(null));

    public AnalysisTableConfigModel? AnalysisTableConfig
    {
      get { return (AnalysisTableConfigModel)GetValue(AnalysisTableConfigProperty); }
      set { SetValue(AnalysisTableConfigProperty, value); }
    }

    public AnalysisTableScriptConfigView()
    {
      InitializeComponent();
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
      Process.Start(new ProcessStartInfo
      {
        UseShellExecute = true,
        FileName = e.Uri.ToString(),
      });
    }
  }
}
