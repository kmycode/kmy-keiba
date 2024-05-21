using CefSharp;
using CefSharp.SchemeHandler;
using CefSharp.Wpf;
using KmyKeiba.Models.Race;
using KmyKeiba.Shared;
using System;
using System.Collections.Generic;
using System.IO;
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
  /// RaceExpectAllView.xaml の相互作用ロジック
  /// </summary>
  public partial class RaceExpectAllView : UserControl
  {
    public static readonly DependencyProperty RaceProperty
    = DependencyProperty.Register(
        nameof(Race),
        typeof(RaceInfo),
        typeof(RaceExpectAllView),
        new PropertyMetadata((sender, e) => ((RaceExpectAllView)sender).OnConditionChanged()));

    public RaceInfo? Race
    {
      get { return (RaceInfo)GetValue(RaceProperty); }
      set { SetValue(RaceProperty, value); }
    }

    public Guid UniqueId { get; } = Guid.NewGuid();

    public Guid UniqueId2 { get; } = Guid.NewGuid();

    public Guid UniqueId3 { get; } = Guid.NewGuid();

    public Guid UniqueId4 { get; } = Guid.NewGuid();

    static RaceExpectAllView()
    {
      if (!Directory.Exists(Constrants.ScriptDir))
      {
        Directory.CreateDirectory(Constrants.ScriptDir);
      }
      File.WriteAllText(System.IO.Path.Combine(Constrants.ScriptDir, "dummy.html"), string.Empty);

      var settings = new CefSettings();
      settings.RegisterScheme(new CefCustomScheme
      {
        SchemeName = "localfolder",
        DomainName = "cefsharp",
        SchemeHandlerFactory = new FolderSchemeHandlerFactory(
          rootFolder: Constrants.ScriptDir,
          hostName: "cefsharp"
        ),
      });
      Cef.Initialize(settings);
    }

    public RaceExpectAllView()
    {
      InitializeComponent();

      this.Browser.LoadUrl("localfolder://cefsharp/dummy.html");

      this.S_HorseList.Checked += this.RadioButton_Checked;
      this.S_Odds.Checked += this.RadioButton_Checked;
      this.IsVisibleChanged += this.RaceExpectAllView_IsVisibleChanged;
    }

    private void UpdateHeavyBindings()
    {
      if (this.Race == null)
      {
        return;
      }

      if (this.S_HorseList.IsChecked == true && this.RaceHorsePillarList.ItemsSource != this.Race.Horses)
      {
        this.RaceHorsePillarList.ItemsSource = this.Race.Horses;
      }
    }

    private void RadioButton_Checked(object sender, RoutedEventArgs e)
    {
      this.UpdateHeavyBindings();
    }

    private void RaceExpectAllView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      this.UpdateHeavyBindings();
    }

    private void OnConditionChanged()
    {
      if (this.Race == null)
      {
        this.RaceHorsePillarList.ItemsSource = null;
        return;
      }

      if (this.Visibility == Visibility.Visible)
      {
        this.UpdateHeavyBindings();
      }
    }
  }
}
