using KmyKeiba.Common;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race;
using KmyKeiba.Models.Script.NodeJSCompat;
using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;
using Reactive.Bindings;
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

namespace KmyKeiba.Models.Script
{
  public class ScriptManager
  {
    private int _outputNum = 0;

    public static JsonSerializerOptions JsonOptions { get; } = new JsonSerializerOptions
    {
      Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
      NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
    };

    public RaceInfo Race { get; }

    public BrowserController Controller { get; } = new();

    public ReactiveProperty<bool> IsError { get; } = new();

    public ReactiveProperty<string> ErrorMessage { get; } = new();

    public ReactiveProperty<ScriptSuggestion?> Suggestion { get; } = new();

    public ScriptManager(RaceInfo race)
    {
      this.Race = race;
    }

    public async Task UpdateAsync() => await this.ExecuteAsync();

    public async Task ExecuteAsync()
    {
      this.Suggestion.Value = null;
      this.IsError.Value = false;

      var result = await ExecuteAsync(this.Race);

      if (result.IsError)
      {
        this.IsError.Value = true;
        this.ErrorMessage.Value = result.ErrorMessage;
      }
      else
      {
        this._outputNum++;
        if (this._outputNum == 1)
        {
          File.WriteAllText($"script/output-{this._outputNum}.html", result.Html.ToString());
          this.Controller.Navigate($"localfolder://cefsharp/output-{this._outputNum}.html");
        }
        else
        {
          this.Controller.UpdateHtml(result.Html.ToString());
        }
        this.Suggestion.Value = result.Suggestion;
      }
    }

    public static async Task<ScriptResult> ExecuteAsync(RaceInfo race)
    {
      using var engine = new ScriptEngineWrapper();
      return await engine.ExecuteAsync(race);
    }

    public async Task ApproveMarksAsync()
    {
      if (this.Suggestion.Value == null || !this.Suggestion.Value.HasMarks.Value)
      {
        return;
      }

      using var db = new MyContext();

      foreach (var horse in this.Race.Horses)
      {
        var mark = this.Suggestion.Value.Marks.FirstOrDefault(m => m.HorseNumber == horse.Data.Number);
        horse.ChangeHorseMark(db, mark.Mark);
      }

      await db.SaveChangesAsync();

      this.Suggestion.Value.Marks.Clear();
      this.Suggestion.Value.HasMarks.Value = false;
    }

    public async Task ApproveTicketsAsync()
    {
      await this.ApproveTicketsAsync(false);
    }

    public async Task ApproveReplacingTicketsAsync()
    {
      await this.ApproveTicketsAsync(true);
    }

    private async Task ApproveTicketsAsync(bool isReplace)
    {
      if (this.Suggestion.Value == null || !this.Suggestion.Value.HasTickets.Value || this.Race.Tickets.Value == null)
      {
        return;
      }

      if (isReplace)
      {
        await this.Race.Tickets.Value.ClearTicketsAsync();
      }
      await this.Race.Tickets.Value.BuyAsync(this.Suggestion.Value.Tickets);

      this.Suggestion.Value.Tickets.Clear();
      this.Suggestion.Value.HasTickets.Value = false;
    }
  }

  public class ScriptResult
  {
    public bool IsError { get; init; }

    public string ErrorMessage { get; init; } = string.Empty;

    public ScriptSuggestion Suggestion { get; }

    public ScriptHtml Html { get; }

    public ScriptResult(ScriptHtml html, ScriptSuggestion suggestion)
    {
      this.Html = html;
      this.Suggestion = suggestion;
    }
  }

  public class ScriptEngineWrapper : IDisposable
  {
    private static readonly string _backgroundColor = ResourceUtil.TryGetResource<RHColor>("BrowserBackgroundColor")?.ToHTMLColor() ?? "white";
    private static readonly string _foregroundColor = ResourceUtil.TryGetResource<RHColor>("BrowserForegroundColor")?.ToHTMLColor() ?? "white";

    protected ScriptObjectContainer<ScriptHtml> HtmlContainer { get; } = new();

    protected V8ScriptEngine Engine { get; }

    protected ScriptObjectContainer<ScriptCurrentRace> RaceContainer { get; } = new();

    protected ScriptObjectContainer<ScriptSuggestion> SuggestionContainer { get; } = new();

    public ScriptEngineWrapper()
    {
      this.Engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDynamicModuleImports |
        V8ScriptEngineFlags.EnableTaskPromiseConversion |
        V8ScriptEngineFlags.EnableValueTaskPromiseConversion |
        V8ScriptEngineFlags.EnableDateTimeConversion);

      this.Engine.DocumentSettings.AccessFlags |= DocumentAccessFlags.EnableAllLoading | DocumentAccessFlags.EnforceRelativePrefix;
      this.Engine.DocumentSettings.SearchPath = Path.Combine(Directory.GetCurrentDirectory(), "script");

      this.Engine.AddHostObject("__currentRace", this.RaceContainer);
      this.Engine.AddHostObject("__suggestion", this.SuggestionContainer);
      this.Engine.AddHostObject("__html", this.HtmlContainer);
      this.Engine.AddHostObject("__fs", new NodeJSFileSystem());
      this.Engine.AddHostObject("__hostFuncs", new HostFunctions());
    }

    protected virtual object Execute(RaceInfo race)
    {
      var script = File.ReadAllText("script/index.js");
      this.Engine.Script.OnInit = this.Engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard, }, script);
      return this.Engine.Invoke("OnInit");
    }

    public async Task<ScriptResult> ExecuteAsync(RaceInfo race)
    {
      var isError = false;
      var errorMessage = string.Empty;

      this.RaceContainer.SetItem(new ScriptCurrentRace(race));
      this.SuggestionContainer.SetItem(new ScriptSuggestion(race.Data.Key));
      this.HtmlContainer.SetItem(new ScriptHtml());

      try
      {
        var result = this.Execute(race);
        var htmlObj = this.HtmlContainer.Item!;

        string text = string.Empty;
        if (result is string str)
        {
          text = str;
        }
        else if (result is Task<object> task)
        {
          text = (await task)?.ToString() ?? string.Empty;
        }
        else if (result is Task<string> task2)
        {
          text = await task2 ?? string.Empty;
        }
        else
        {
          text = result?.ToString() ?? string.Empty;
        }

        // HTML出力
        htmlObj.Body = text;
        htmlObj.DefaultHead = $@"
    <style>
      body {{ background-color: {_backgroundColor}; color: {_foregroundColor}; font-size: 16px; }}
    </style>
    <meta charset=""utf8""/>";
      }
      catch (Exception ex)
      {
        isError = true;
        if (ex is ScriptEngineException sex)
        {
          errorMessage = sex.ErrorDetails;
        }
        else
        {
          errorMessage = ex.Message;
        }
      }

      return new ScriptResult(this.HtmlContainer.Item!, this.SuggestionContainer.Item!)
      {
        IsError = isError,
        ErrorMessage = errorMessage,
      };
    }

    public void Dispose()
    {
      this.Engine.Dispose();
    }
  }

  public class CompiledScriptEngineWrapper : ScriptEngineWrapper
  {
    private V8Script? _compiled;

    protected override object Execute(RaceInfo race)
    {
      if (this._compiled == null)
      {
        this.Compile();
      }

      return this.Engine.Invoke("OnInit");
    }

    private void Compile()
    {
      var script = File.ReadAllText("script/index.js");
      this._compiled = this.Engine.Compile(new DocumentInfo { Category = ModuleCategory.Standard, }, script);
      this.Engine.Script.OnInit = this.Engine.Evaluate(this._compiled);
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
  public class ScriptHtml
  {
    [ScriptMember("head")]
    public string Head { get; set; } = string.Empty;

    public string DefaultHead { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public override string ToString()
    {
      return $@"<html lang=""ja"">
  <head>
{this.DefaultHead}
{this.Head}
  </head>
  <body>
{this.Body}
  </body>
</html>";
    }
  }
}
