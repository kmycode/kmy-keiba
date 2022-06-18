using KmyKeiba.Common;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race;
using KmyKeiba.Models.Script.NodeJSCompat;
using KmyKeiba.Shared;
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
    private static readonly Dictionary<string, ScriptManager> _instances = new();
    private static int _outputNum = 0;

    public static JsonSerializerOptions JsonOptions { get; } = new JsonSerializerOptions
    {
      Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
      NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
    };

    private WeakReference<RaceInfo> RaceReference { get; set; }

    public RaceInfo? Race
    {
      get
      {
        this.RaceReference.TryGetTarget(out var race);
        return race;
      }
      set
      {
        if (value != null)
        {
          this.RaceReference = new WeakReference<RaceInfo>(value);
        }
      }
    }

    public BrowserController Controller { get; } = new();

    public ReactiveProperty<bool> IsError { get; } = new();

    public ReactiveProperty<bool> IsExecuting { get; } = new();

    public ReactiveProperty<bool> IsCompleted { get; } = new();

    public ReactiveProperty<string> ErrorMessage { get; } = new();

    public ReactiveProperty<ScriptSuggestion?> Suggestion { get; } = new();

    public ReactiveProperty<ReactiveProperty<int>?> Progress { get; } = new();

    public ReactiveProperty<ReactiveProperty<int>?> ProgressMax { get; } = new();

    public static void Initialize()
    {
      if (!Directory.Exists(Constrants.MLDir))
      {
        Directory.CreateDirectory(Constrants.MLDir);
      }
    }

    private ScriptManager(RaceInfo race)
    {
      this.RaceReference = new WeakReference<RaceInfo>(race);
    }

    public static ScriptManager GetInstance(RaceInfo race)
    {
      _instances.TryGetValue(race.Data.Key, out var exists);
      if (exists != null)
      {
        exists.Race = race;
        return exists;
      }

      var instance = new ScriptManager(race);
      _instances.Add(race.Data.Key, instance);
      return instance;
    }

    public async Task UpdateAsync() => await this.ExecuteAsync();

    public async Task ExecuteAsync()
    {
      var race = this.Race;
      if (race == null)
      {
        return;
      }

      this.Suggestion.Value = null;
      this.IsError.Value = false;
      this.IsCompleted.Value = false;
      this.IsExecuting.Value = true;

      var result = await this.ExecuteAsync(race);

      if (result.IsError)
      {
        this.IsError.Value = true;
        this.ErrorMessage.Value = result.ErrorMessage;
      }
      else
      {
        _outputNum++;
        //if (_outputNum == 1)
        {
          var tmpDir = Path.Combine(Constrants.ScriptDir, "tmp");
          if (!Directory.Exists(tmpDir))
          {
            Directory.CreateDirectory(tmpDir);
          }
          File.WriteAllText(Path.Combine(tmpDir, $"output-{_outputNum}.html"), result.Html.ToString());
          this.Controller.Navigate($"localfolder://cefsharp/tmp/output-{_outputNum}.html");
        }
        /*
        else
        {
          this.Controller.UpdateHtml(result.Html.ToString());
        }
        */
        this.Suggestion.Value = result.Suggestion;
        this.IsCompleted.Value = true;
      }

      this.IsExecuting.Value = false;
    }

    private async Task<ScriptResult> ExecuteAsync(RaceInfo race)
    {
      using var engine = new ScriptEngineWrapper();
      this.ProgressMax.Value = engine.Html.ProgressMaxObservable;
      this.Progress.Value = engine.Html.ProgressObservable;

      var result = await engine.ExecuteAsync(race);

      this.ProgressMax.Value = null;
      this.Progress.Value = null;

      return result;
    }

    public async Task ApproveMarksAsync()
    {
      if (this.Suggestion.Value == null || !this.Suggestion.Value.HasMarks.Value)
      {
        return;
      }

      var race = this.Race;
      if (race == null)
      {
        return;
      }

      using var db = new MyContext();

      foreach (var horse in race.Horses)
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
      var race = this.Race;

      if (this.Suggestion.Value == null || !this.Suggestion.Value.HasTickets.Value || race?.Tickets.Value == null)
      {
        return;
      }

      if (isReplace)
      {
        await race.Tickets.Value.ClearTicketsAsync();
      }
      await race.Tickets.Value.BuyAsync(this.Suggestion.Value.Tickets);

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

  public class ScriptEngineWrapper : ScriptEngineWrapperBase
  {
    private static readonly string _backgroundColor = ResourceUtil.TryGetResource<RHColor>("BrowserBackgroundColor")?.ToHTMLColor() ?? "white";
    private static readonly string _foregroundColor = ResourceUtil.TryGetResource<RHColor>("BrowserForegroundColor")?.ToHTMLColor() ?? "white";

    protected ScriptObjectContainer<ScriptHtml> HtmlContainer { get; } = new();

    public ScriptHtml Html => this.HtmlContainer.Item!;

    protected ScriptObjectContainer<ScriptCurrentRace> RaceContainer { get; } = new();

    protected ScriptObjectContainer<ScriptCurrentRace> RaceContainerWithResults { get; } = new();

    public bool IsReadResults => this.RaceContainerWithResults.IsRead;

    protected ScriptObjectContainer<ScriptSuggestion> SuggestionContainer { get; } = new();

    protected ScriptObjectContainer<ScriptBulkConfig> BulkConfigContainer { get; } = new();

    protected ScriptObjectContainer<ScriptML> MLContainer { get; } = new();

    protected ScriptObjectContainer<ScriptMLPrediction> MLPredictionContainer { get; } = new();

    public ScriptBulkConfig BulkConfig => this.BulkConfigContainer.Item!;

    public ScriptML ML => this.MLContainer.Item!;

    public ScriptEngineWrapper() : base("index.js")
    {
      this.Engine.AddHostObject("__currentRace", this.RaceContainer);
      this.Engine.AddHostObject("__currentRaceWithResults", this.RaceContainerWithResults);
      this.Engine.AddHostObject("__suggestion", this.SuggestionContainer);
      this.Engine.AddHostObject("__html", this.HtmlContainer);
      this.Engine.AddHostObject("__bulk", this.BulkConfigContainer);
      this.Engine.AddHostObject("__ml", this.MLContainer);
      this.Engine.AddHostObject("__mlp", this.MLPredictionContainer);

      this.HtmlContainer.SetItem(new ScriptHtml());
      this.BulkConfigContainer.SetItem(new ScriptBulkConfig());
      this.MLContainer.SetItem(new ScriptML());
      this.MLPredictionContainer.SetItem(new ScriptMLPrediction());
    }

    protected virtual object Execute(RaceInfo race)
    {
      return this.Execute();
    }

    public async Task<ScriptResult> ExecuteAsync(RaceInfo race)
    {
      var isError = false;
      var errorMessage = string.Empty;

      var currentRace = new ScriptCurrentRace(race);
      this.RaceContainer.SetItem(currentRace);
      this.RaceContainerWithResults.SetItem(currentRace);
      this.SuggestionContainer.SetItem(new ScriptSuggestion(race.Data.Key));

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

      var data = new ScriptResult(this.HtmlContainer.Item!, this.SuggestionContainer.Item!)
      {
        IsError = isError,
        ErrorMessage = errorMessage,
      };

      this.HtmlContainer.SetItem(new ScriptHtml());

      return data;
    }
  }

  public class CompiledScriptEngineWrapper : ScriptEngineWrapper
  {
    private V8Script? _compiled;

    protected override object Execute(RaceInfo race)
    {
      if (this._compiled == null)
      {
        this._compiled = this.Compile();
      }

      this.RaceContainerWithResults.SetItem(new ScriptCurrentRaceWithResults(race));
      return this.Execute(this._compiled!);
    }

    public override void Dispose()
    {
      base.Dispose();
      this._compiled?.Dispose();
    }
  }

  [NoDefaultScriptAccess]
  public class ScriptObjectContainer<T> where T : class
  {
    [ScriptMember("item")]
    public T? Item
    {
      get
      {
        this.IsRead = true;
        return this._item;
      }
      private set
      {
        this._item = value;
      }
    }
    private T? _item;

    public bool IsRead { get; private set; }

    public void SetItem(T race)
    {
      this.Item = race;
      this.IsRead = false;
    }
  }

  [NoDefaultScriptAccess]
  public class ScriptHtml
  {
    [ScriptMember("head")]
    public string Head { get; set; } = string.Empty;

    public string DefaultHead { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    [ScriptMember("progress")]
    public int Progress
    {
      get => this.ProgressObservable.Value;
      set => this.ProgressObservable.Value = value;
    }
    public ReactiveProperty<int> ProgressObservable { get; } = new();

    [ScriptMember("progressMax")]
    public int ProgressMax
    {
      get => this.ProgressMaxObservable.Value;
      set => this.ProgressMaxObservable.Value = value;
    }
    public ReactiveProperty<int> ProgressMaxObservable { get; } = new();

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

  [NoDefaultScriptAccess]
  public class ScriptBulkConfig
  {
    [ScriptMember("isCentral")]
    public bool IsCentral { get; set; } = true;

    [ScriptMember("isLocal")]
    public bool IsLocal { get; set; } = true;

    [ScriptMember("isBanei")]
    public bool IsBanei { get; set; } = true;

    [ScriptMember("isBulk")]
    public bool IsBulk => this._isBulk;

    private bool _isBulk;
    public void SetBulk(bool value)
    {
      this._isBulk = value;
    }
  }
}
