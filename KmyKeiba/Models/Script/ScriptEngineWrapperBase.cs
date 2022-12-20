using KmyKeiba.Models.Script.NodeJSCompat;
using KmyKeiba.Shared;
using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Script
{
  public abstract class FileScriptEngine : IDisposable
  {
    protected V8ScriptEngine Engine { get; }

    protected string FileName { get; }

    public FileScriptEngine(string fileName)
    {
      this.FileName = fileName;

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

      this.Engine.AddHostObject("__fs", new NodeJSFileSystem());
      this.Engine.AddHostObject("__hostFuncs", new HostFunctions());
    }

    public bool IsScriptExists()
    {
      return File.Exists(Path.Combine(Constrants.ScriptDir, this.FileName));
    }

    public object Execute()
    {
      DocumentLoader.Default.DiscardCachedDocuments();

      var script = File.ReadAllText(Path.Combine(Constrants.ScriptDir, this.FileName));
      this.Engine.Script.OnInit = this.Engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard, }, script);
      return this.Engine.Invoke("OnInit");
    }

    protected V8Script Compile()
    {
      DocumentLoader.Default.DiscardCachedDocuments();

      var script = File.ReadAllText(Path.Combine(Constrants.ScriptDir, this.FileName));
      var compiled = this.Engine.Compile(new DocumentInfo { Category = ModuleCategory.Standard, }, script);
      this.Engine.Script.OnInit = this.Engine.Evaluate(compiled);

      return compiled;
    }

    protected object Execute(V8Script compiled)
    {
      return this.Engine.Invoke("OnInit");
    }

    public virtual void Dispose()
    {
      this.Engine.Dispose();
    }
  }

  public class StringScriptEngine : IDisposable
  {
    protected V8ScriptEngine Engine { get; }

    public StringScriptEngine()
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
    }

    public void AddHostObject(string name, object obj)
    {
      this.Engine.AddHostObject(name, obj);
    }

    public object Execute(string script)
    {
      // DocumentLoader.Default.DiscardCachedDocuments();

      return this.Engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard, }, script);
    }

    public virtual void Dispose()
    {
      this.Engine.Dispose();
    }
  }
}
