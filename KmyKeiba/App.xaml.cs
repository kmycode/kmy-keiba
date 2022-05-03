﻿using KmyKeiba.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace KmyKeiba
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    public App()
    {
      // Dependency Injections
      ViewMessages.TryGetResource = (key) => this.TryFindResource(key);
      ViewMessages.ChangeTheme = (theme) => this.ChangeTheme(theme);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);
    }

    private void ChangeTheme(ApplicationTheme theme)
    {
      var name = theme == ApplicationTheme.Classic ? "Resources/ThemeClassic.xaml" :
        theme == ApplicationTheme.Dark ? "Resources/ThemeDark.xaml" : "Resources/ThemeClassic.xaml";

      ResourceDictionary dic = new ResourceDictionary
      {
        Source = new Uri(name, UriKind.Relative),
      };
      this.Resources.MergedDictionaries.Clear();
      this.Resources.MergedDictionaries.Add(dic);
    }
  }
}
