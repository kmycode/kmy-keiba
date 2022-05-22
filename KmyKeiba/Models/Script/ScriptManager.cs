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
    private static readonly string _backgroundColor = ResourceUtil.TryGetResource<RHColor>("BrowserBackgroundColor")?.ToHTMLColor() ?? "white";
    private static readonly string _foregroundColor = ResourceUtil.TryGetResource<RHColor>("BrowserForegroundColor")?.ToHTMLColor() ?? "white";
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
      var suggestion = new ScriptSuggestion(this.Race.Data.Key);
      var htmlObj = new ScriptHtml();
      this.Suggestion.Value = null;

      using var engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDynamicModuleImports |
        V8ScriptEngineFlags.EnableTaskPromiseConversion |
        V8ScriptEngineFlags.EnableValueTaskPromiseConversion |
        V8ScriptEngineFlags.EnableDateTimeConversion);

      try
      {
        this.IsError.Value = false;

        engine.DocumentSettings.AccessFlags |= DocumentAccessFlags.EnableAllLoading | DocumentAccessFlags.EnforceRelativePrefix;
        engine.DocumentSettings.SearchPath = Path.Combine(Directory.GetCurrentDirectory(), "script");

        engine.AddHostObject("__currentRace", new ScriptCurrentRace(this.Race));
        engine.AddHostObject("__suggestion", suggestion);
        engine.AddHostObject("__html", htmlObj);
        engine.AddHostObject("__fs", new NodeJSFileSystem());
        engine.AddHostObject("__hostFuncs", new HostFunctions());

        var script = File.ReadAllText("script/index.js");
        engine.Script.OnInit = engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard, }, script);
        var result = engine.Invoke("OnInit");

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
        this._outputNum++;
        htmlObj.Body = text;
        htmlObj.DefaultHead = $@"
    <style>
      body {{ background-color: {_backgroundColor}; color: {_foregroundColor}; font-size: 16px; }}
    </style>
    <meta charset=""utf8""/>";
        var html = htmlObj.ToString();

        if (this._outputNum == 1)
        {
          File.WriteAllText($"script/output-{this._outputNum}.html", html);
          this.Controller.Navigate($"localfolder://cefsharp/output-{this._outputNum}.html");
        }
        else
        {
          this.Controller.UpdateHtml(html);
        }
      }
      catch (Exception ex)
      {
        this.IsError.Value = true;
        if (ex is ScriptEngineException sex)
        {
          this.ErrorMessage.Value = sex.ErrorDetails;
        }
        else
        {
          this.ErrorMessage.Value = ex.Message;
        }
      }

      // 提案
      this.Suggestion.Value = suggestion;
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
