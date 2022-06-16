using Keras.Callbacks;
using Keras.Models;
using Keras.Utils;
using Kmykeiba.ML;
using KmyKeiba.ML.Script;
using KmyKeiba.Prompt.Models.Brains;
using KmyKeiba.Shared;
using Numpy;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.ML.Brain
{
  public class KerasModel
  {
    private BaseModel _model;
    private readonly ScriptLayer _layer;

    private int _epochs;

    private KerasModel(ScriptLayer layer)
    {
      this._model = layer.ToModel();
      this._layer = layer;
    }

    public void SaveFile() => this.SaveFile(Path.Combine(Constrants.MLDir, this._layer.Name));

    public void SaveFile(string fileName)
    {
      this._model.Save(fileName);
      File.WriteAllText(fileName + "/epochs.data", @$"epochs={this._epochs}");
    }

    public void LoadFile() => this.LoadFile(Path.Combine(Constrants.MLDir, this._layer.Name));

    public void LoadFile(string fileName)
    {
      var customObjects = new Dictionary<string, PyObject>();
      if (OptimizerManager.Radam != null)
      {
        customObjects.Add("RAdam", (PyObject)OptimizerManager.Radam.RAdam());
      }

      var loaded_model = Sequential.LoadModel(fileName, customObjects);
      this._model = loaded_model;

      var raw = File.ReadAllLines(fileName + "/epochs.data");
      var data = raw.Select((d) => d.Split("=")).ToDictionary((d) => d[0], (d) => d[1]);
      this._epochs = int.Parse(data["epochs"]);
    }

    private void Training(float[,] source, float[] results)
    {
      NDarray x = np.array(source);
      NDarray y = np.array(results);

      if (this._layer.Type == "reguressor")
      {
        var estimator = new KerasReguressor(this._model, epochs: this._layer.Epochs, batch_size: this._layer.BatchSize);
        estimator.Fit(x, y);
      }
      else
      {
        this._model.Fit(x, y, batch_size: this._layer.BatchSize, epochs: this._layer.Epochs, verbose: this._layer.Verbose, callbacks: new Callback[]
        {
          new EarlyStopping(monitor: "loss"),
        });
      }
    }

    public float[] Predict(float[,] data)
    {
      if (this._model == null)
      {
        return Array.Empty<float>();
      }

      var result = this._model.Predict(np.array(data), batch_size: 1, verbose: 0);
      var resultArray = result.GetData<float>();

      if (!string.IsNullOrEmpty(this._layer.DotFileName) && this._layer.Labels.Any())
      {
        var tree = new TreeAnalyticsModel(8);
        tree.AddData(data, resultArray);
        tree.Fit();
        tree.ExportAsDot(Path.Combine(Constrants.MLDir, this._layer.DotFileName), this._layer.Labels);
      }

      return resultArray;
    }

    public static async Task FromSourceAsync()
    {
      var rawFile = Path.Combine(Constrants.MLDir, "source.txt");
      var rawResultFile = Path.Combine(Constrants.MLDir, "results.txt");
      var layerFile = Path.Combine(Constrants.ScriptDir, "mlconfigure.js");

      if (!File.Exists(rawFile))
      {
        Program.Error("No raw file");
      }
      if (!File.Exists(rawResultFile))
      {
        Program.Error("No raw result file");
      }
      if (!File.Exists(layerFile))
      {
        Program.Error("No layer script");
      }

      var script = new ScriptRunner();
      var result = await script.ExecuteAsync(layerFile);

      if (result.Layer == null || result.IsError)
      {
        Program.Error("Script Error: " + result.ErrorMessage);
      }

      var raws = File.ReadLines(rawFile);
      var source = RawToSource(raws.ToArray());
      var array = SourceToArray(source);
      var resultRaws = File.ReadLines(rawResultFile);
      var results = ResultsToArray(resultRaws.ToArray());

      var layer = result.Layer!;
      layer.ShapeLength = array.GetLength(1);
      var model = new KerasModel(layer);

      model.Training(array, results);

      foreach (var action in result.Layer!.AfterTrainingActions)
      {
        action(model);
      }
    }

    public static async Task PredictAsync()
    {
      var rawFile = Path.Combine(Constrants.MLDir, "predicts.txt");
      var layerFile = Path.Combine(Constrants.ScriptDir, "mlconfigure.js");

      if (!File.Exists(rawFile))
      {
        Program.Error("No raw file");
      }
      if (!File.Exists(layerFile))
      {
        Program.Error("No layer script");
      }

      var script = new ScriptRunner();
      var result = await script.ExecuteAsync(layerFile);

      if (result.Layer == null || result.IsError)
      {
        Program.Error("Script Error: " + result.ErrorMessage);
      }

      var model = new KerasModel(result.Layer!);

      var raws = File.ReadLines(rawFile);
      var source = RawToSource(raws.ToArray());
      var array = SourceToArray(source);

      foreach (var action in result.Layer!.BeforePredictionActions)
      {
        action(model);
      }

      var results = model.Predict(array);

      File.WriteAllLines(Path.Combine(Constrants.MLDir, "predictresults.txt"), results.Select(r => r.ToString()));
    }

    private static List<float[]> RawToSource(string[] rawText)
    {
      var source = new List<float[]>();
      var raws = rawText;
      foreach (var line in raws.Where(r => !string.IsNullOrEmpty(r)))
      {
        var columns = line.Split(',');
        var row = columns.Where(r => !string.IsNullOrEmpty(r)).Select(c =>
        {
          float.TryParse(c, out var value);
          return value;
        }).ToArray();
        source.Add(row);
      }

      return source;
    }

    private static float[,] SourceToArray(List<float[]> source)
    {
      if (!source.Any())
      {
        return new float[0, 0];
      }

      var array = new float[source.Count, source[0].Length];
      for (var i = 0; i < source.Count; i++)
      {
        for (var j = 0; j < source[i].Length; j++)
        {
          array[i, j] = source[i][j];
        }
      }

      return array;
    }

    private static float[] ResultsToArray(string[] results)
    {
      return results.Where(r => !string.IsNullOrEmpty(r)).Select(r =>
      {
        float.TryParse(r, out var value);
        return value;
      }).ToArray();
    }
  }
}
