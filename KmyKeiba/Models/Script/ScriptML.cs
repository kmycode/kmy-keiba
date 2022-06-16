using KmyKeiba.Shared;
using Microsoft.ClearScript;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Script
{
  [NoDefaultScriptAccess]
  public class ScriptML
  {
    private readonly List<float[]> _rows = new();
    private readonly List<float> _results = new();
    private readonly List<ScriptML> _merged = new();

    public bool HasTrainingData => this._merged.SelectMany(m => m._rows).Any();

    public ScriptML()
    {
      this._merged.Add(this);
    }

    [ScriptMember("addRow")]
    public void AddRow(string array, double result)
    {
      var arr = JsonSerializer.Deserialize<float[]>(array, ScriptManager.JsonOptions);
      if (arr != null)
      {
        this._rows.Add(arr);
        this._results.Add((float)result);
      }
    }

    public void Merge(ScriptML other)
    {
      this._merged.Add(other);
    }

    public void SaveTrainingFile(string fileName, string resultFileName)
    {
      if (!this._merged.SelectMany(m => m._rows).Any())
      {
        return;
      }

      var text = new StringBuilder();
      foreach (var row in this._merged.SelectMany(m => m._rows))
      {
        foreach (var column in row)
        {
          text.Append(column).Append(',');
        }
        text.AppendLine();
      }

      File.WriteAllText(fileName, text.ToString());
      File.WriteAllText(resultFileName, string.Join("\n", this._merged.SelectMany(m => m._results)));
    }
  }

  [NoDefaultScriptAccess]
  public class ScriptMLPrediction
  {
    private readonly List<float[]> _rows = new();

    [ScriptMember("addRow")]
    public void AddRow(string array)
    {
      var arr = JsonSerializer.Deserialize<float[]>(array, ScriptManager.JsonOptions);
      if (arr != null)
      {
        this._rows.Add(arr);
      }
    }

    [ScriptMember("predictAsync")]
    public async Task<string> PredictAsync(bool isConsole)
    {
      var resultFile = Path.Combine(Constrants.MLDir, "predictresults.txt");
      var errorFile = Path.Combine(Constrants.MLDir, "error.txt");
      File.Delete(resultFile);
      File.Delete(errorFile);

      File.WriteAllLines(Path.Combine(Constrants.MLDir, "predicts.txt"), this._rows.Select(r => string.Join(',', r)));
      await Process.Start(new ProcessStartInfo
      {
        Arguments = "predict",
        FileName = "./KmyKeiba.ML.exe",
        CreateNoWindow = !isConsole,
      })!.WaitForExitAsync();

      if (File.Exists(errorFile))
      {
        throw new Exception("機械学習利用中にエラーが発生しました: " + File.ReadAllText(errorFile));
      }
      else if (File.Exists(resultFile))
      {
        var results = File.ReadAllLines(resultFile).Select(r =>
        {
          double.TryParse(r, out var value);
          return value;
        });
        return JsonSerializer.Serialize(results, ScriptManager.JsonOptions);
      }

      return "[]";
    }
  }
}
