using KmyKeiba.Common;
using KmyKeiba.Shared;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace KmyKeiba
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    private readonly Mutex _mutex;

    public App()
    {
      Directory.CreateDirectory(Constrants.AppDataDir);

      var selfPath = Assembly.GetEntryAssembly()?.Location ?? string.Empty;
      var selfPathDir = Path.GetDirectoryName(selfPath) ?? "./";
      log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(Path.Combine(selfPathDir, "log4net.config")));

      logger.Info("================================");
      logger.Info("==                            ==");
      logger.Info("==            開始            ==");
      logger.Info("==                            ==");
      logger.Info("================================");
      logger.Info($"Version: {Constrants.ApplicationVersion}");

#if !DEBUG
      var rootLogger = ((Hierarchy)logger.Logger.Repository).Root;
      if (File.Exists(Constrants.DebugFilePath))
      {
        rootLogger.Level = log4net.Core.Level.All;
        logger.Info("ログレベル: All (デバッグファイルが見つかりました)");
      }
      else
      {
        rootLogger.Level = log4net.Core.Level.Info;
        logger.Info("ログレベル: Info");
      }
#else
      logger.Info("ログレベル: All");
#endif

      //ミューテックスの所有権を要求する
      this._mutex = new Mutex(false, "KMY_Keiba");
      try
      {
        if (!this._mutex.WaitOne(0, false))
        {
          logger.Fatal("アプリの多重起動が検出されました。終了します");
          MessageBox.Show("本アプリは多重起動に対応していません。\n処理を終了します。");
          Application.Current.Shutdown(-1);
          return;
        }
      }
      catch (AbandonedMutexException)
      {
        logger.Warn("前回のアプリが強制終了したようです。無視して処理を継続します");
      }
      this.DispatcherUnhandledException += App_DispatcherUnhandledException;

      // Dependency Injections
      ViewMessages.TryGetResource = (key) => this.TryFindResource(key);
      ViewMessages.ChangeTheme = (theme) => this.ChangeTheme(theme);
      ViewMessages.InvokeUiThread = (action) => this.Dispatcher.Invoke(action);
      
      // Shift-JISを扱う
      Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

      logger.Info("アプリ初期化完了");
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
      logger.Fatal("ハンドルされていないエラーが発生: ", e.Exception);

#if !DEBUG
      MessageBox.Show("内部エラーが発生しました。\nアプリを終了します。");
#endif
      try
      {
        this._mutex.ReleaseMutex();
        this._mutex.Dispose();
      }
      catch { }

      Environment.Exit(-1);
    }

    protected override void OnExit(ExitEventArgs e)
    {
      logger.Info("アプリ終了");
      try
      {
        this._mutex.ReleaseMutex();
        this._mutex.Dispose();
      }
      catch (Exception ex)
      {
        logger.Warn("Mutexの解放でエラーが発生しました", ex);
      }

      base.OnExit(e);
    }

    private void ChangeTheme(ApplicationTheme theme)
    {
      logger.Info($"テーマを変更します: {theme}");

      var name = theme == ApplicationTheme.Classic ? "Resources/ThemeClassic.xaml" :
        theme == ApplicationTheme.Dark ? "Resources/ThemeDark.xaml" : "Resources/ThemeClassic.xaml";

      ResourceDictionary dic = new ResourceDictionary
      {
        Source = new Uri(name, UriKind.Relative),
      };
      // this.Resources.MergedDictionaries.RemoveAt(0);
      this.Resources.MergedDictionaries.Add(dic);

      logger.Info("テーマ変更完了");
    }
  }
}
