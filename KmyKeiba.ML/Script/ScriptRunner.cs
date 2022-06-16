using KmyKeiba.Shared;
using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace KmyKeiba.ML.Script
{
  public class ScriptRunner
  {
    public static JsonSerializerOptions JsonOptions { get; } = new JsonSerializerOptions
    {
      Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
      NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
    };

    public bool IsError { get; set; }

    public bool IsExecuting { get; set; }

    public bool IsCompleted { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;

    public async Task<ScriptResult> ExecuteAsync(string fileName)
    {
      this.IsError = false;
      this.IsCompleted = false;
      this.IsExecuting = true;

      using var engine = new ScriptEngineWrapper();
      var result = await engine.ExecuteAsync(fileName);

      if (result.IsError)
      {
        this.IsError = true;
        this.ErrorMessage = result.ErrorMessage;
      }
      else
      {
        this.IsCompleted = true;
      }

      this.IsExecuting = false;
      return result;
    }
  }

  public class ScriptResult
  {
    public bool IsError { get; init; }

    public string ErrorMessage { get; init; } = string.Empty;

    public ScriptLayer? Layer { get; init; }
  }

  public class ScriptEngineWrapper : IDisposable
  {
    protected V8ScriptEngine Engine { get; }

    protected ScriptLayer Layer { get; } = new();

    public ScriptEngineWrapper()
    {
      this.Engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDynamicModuleImports |
        V8ScriptEngineFlags.EnableTaskPromiseConversion |
        V8ScriptEngineFlags.EnableValueTaskPromiseConversion |
        V8ScriptEngineFlags.EnableDateTimeConversion);

      this.Engine.DocumentSettings.AccessFlags |= DocumentAccessFlags.EnableAllLoading | DocumentAccessFlags.EnforceRelativePrefix;
#if DEBUG
      this.Engine.DocumentSettings.SearchPath = Path.Combine(Directory.GetCurrentDirectory(), "script");
#else
      this.Engine.DocumentSettings.SearchPath = Constrants.ScriptDir;
#endif

      this.Engine.AddHostObject("keras", this.Layer);
      this.Engine.AddHostObject("__hostFuncs", new HostFunctions());
    }

    protected virtual object Execute(string fileName)
    {
      DocumentLoader.Default.DiscardCachedDocuments();

      var script = File.ReadAllText(fileName);
      this.Engine.Script.OnInit = this.Engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard, }, script);
      return this.Engine.Invoke("OnInit");
    }

    public async Task<ScriptResult> ExecuteAsync(string fileName)
    {
      try
      {
        var result = this.Execute(fileName);

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
      }
      catch (Exception ex)
      {
        var isError = true;
        string errorMessage;

        if (ex is ScriptEngineException sex)
        {
          errorMessage = sex.ErrorDetails;
        }
        else
        {
          errorMessage = ex.Message;
        }

        return new ScriptResult()
        {
          IsError = isError,
          ErrorMessage = errorMessage,
        };
      }

      return new ScriptResult()
      {
        Layer = this.Layer,
      };
    }

    public virtual void Dispose()
    {
      this.Engine.Dispose();
    }
  }

  [NoDefaultScriptAccess]
  public class ScriptObjectContainer<T> where T : class
  {
    [ScriptMember("item")]
    public T? Item { get; private set; }

    public void SetItem(T race)
    {
      this.Item = race;
    }
  }

  [NoDefaultScriptAccess]
  public class ScriptBulkConfig
  {
    [ScriptMember("isCentral")]
    public bool IsCentral { get; set; } = true;

    [ScriptMember("isLocal")]
    public bool IsLocal { get; set; } = true;

    [ScriptMember("isBanei")]
    public bool IsBanei { get; set; } = true;
  }
}
