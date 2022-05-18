using KmyKeiba.Common;
using KmyKeiba.Models.Race;
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
  public class ScriptManager
  {
    private static readonly string _backgroundColor = ResourceUtil.TryGetResource<RHColor>("BrowserBackgroundColor")?.ToHTMLColor() ?? "white";
    private static readonly string _foregroundColor = ResourceUtil.TryGetResource<RHColor>("BrowserForegroundColor")?.ToHTMLColor() ?? "white";
    private int _outputNum = 0;

    public RaceInfo Race { get; }

    public BrowserController Controller { get; } = new();

    public ScriptManager(RaceInfo race)
    {
      this.Race = race;
    }

    public void Update() => this.Execute();

    public void Execute()
    {
      using (var engine = new V8ScriptEngine())
      {
        try
        {
          engine.DocumentSettings.AccessFlags |= DocumentAccessFlags.EnableAllLoading | DocumentAccessFlags.EnforceRelativePrefix;
          engine.DocumentSettings.SearchPath = Path.Combine(Directory.GetCurrentDirectory(), "script");

          var script = File.ReadAllText("script/index.js");
          engine.Script.OnInit = engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard, }, script);
          var result = engine.Invoke("OnInit");

          // HTML出力
          this._outputNum++;
          var html = $@"<html lang=""ja"">
  <head>
    <style>
      body {{ background-color: {_backgroundColor}; color: {_foregroundColor}; font-size: 16px; }}
    </style>
  </head>
  <body>
{result}
  </body>
</html>";

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
        catch
        {

        }
      }
    }
  }
}
