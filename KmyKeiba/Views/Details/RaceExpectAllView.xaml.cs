using CefSharp;
using CefSharp.SchemeHandler;
using CefSharp.Wpf;
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
  /// RaceExpectAllView.xaml の相互作用ロジック
  /// </summary>
  public partial class RaceExpectAllView : UserControl
  {
    public static readonly DependencyProperty RaceProperty
    = DependencyProperty.Register(
        nameof(Race),
        typeof(RaceInfo),
        typeof(RaceExpectAllView),
        new PropertyMetadata(null));

    public RaceInfo? Race
    {
      get { return (RaceInfo)GetValue(RaceProperty); }
      set { SetValue(RaceProperty, value); }
    }

    public Guid UniqueId { get; } = Guid.NewGuid();

    public Guid UniqueId2 { get; } = Guid.NewGuid();

    public Guid UniqueId3 { get; } = Guid.NewGuid();

    public Guid UniqueId4 { get; } = Guid.NewGuid();

    public RaceExpectAllView()
    {
      var settings = new CefSettings();
      settings.RegisterScheme(new CefCustomScheme
      {
        SchemeName = "localfolder",
        DomainName = "cefsharp",
        SchemeHandlerFactory = new FolderSchemeHandlerFactory(
          rootFolder: System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "script"),
          hostName: "cefsharp"
        ),
      });
      Cef.Initialize(settings);

      InitializeComponent();
    }
  }
}
