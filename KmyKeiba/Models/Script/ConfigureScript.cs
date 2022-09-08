using KmyKeiba.Common;
using KmyKeiba.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Script
{
  internal static class ConfigureScript
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public static async Task RunAsync()
    {
      try
      {
        using var engine = new ConfigureScriptEngineWrapper();
        if (!engine.IsScriptExists())
        {
          var sampleFile = Path.Combine(Constrants.ScriptDir, "configure_sample.js");
          if (File.Exists(sampleFile))
          {
            logger.Info("構成スクリプトファイルが見つからないため、サンプルスクリプトをコピーしました");
            File.Copy(sampleFile, Path.Combine(Constrants.ScriptDir, "configure.js"));
          }
          else
          {
            logger.Info("構成スクリプトファイルが見つからないため、構成はスキップされました");
            return;
          }
        }

        logger.Info("構成スクリプトを実行します");
        var result = engine.Execute();

        if (result is Task<object> task)
        {
          await task;
        }
        else if (result is Task task2)
        {
          await task2;
        }
        else if (result is Action task3)
        {
          task3();
        }
        else if (result is Func<object> task4)
        {
          task4();
        }

        engine.Configuration.SetConfiguration();
      }
      catch (Exception ex)
      {
        logger.Warn("構成スクリプト実行でエラーが発生しました", ex);
        OpenErrorConfiguringRequest.Default.Request(ex.Message);
      }
    }
  }

  internal class ConfigureScriptEngineWrapper : FileScriptEngine
  {
    public ScriptConfiguration Configuration { get; } = new();

    public ConfigureScriptEngineWrapper() : base("configure.js")
    {
      this.Engine.AddHostObject("appconfig", this.Configuration);
    }
  }
}
