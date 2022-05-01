using KmyKeiba.Models.Data;
using KmyKeiba.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prism.Ioc;
using Prism.Unity;
using System.Configuration;
using System.IO;
using System.Text;
using System.Windows;

namespace KmyKeiba
{
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
  public partial class App
  {
    protected override Window CreateShell()
    {
      log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4net.config"));
      Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

      // var context = new MyContext();
      // context.Database.Migrate();
      // context.Dispose();

      return Container.Resolve<MainWindow>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
      containerRegistry.RegisterDialog<LoadJVLinkDialog>();
      containerRegistry.RegisterDialog<PredictRunningStyleDialog>();
    }
  }
}
