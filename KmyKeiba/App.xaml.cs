using KmyKeiba.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prism.Ioc;
using Prism.Unity;
using System.Configuration;
using System.IO;
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
      return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
  }
}
