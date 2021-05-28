using Keras;
using Keras.Callbacks;
using Keras.Layers;
using Keras.Models;
using Keras.Utils;
using KmyKeiba.Models.Data;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Numpy;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Prompt.Models.Brains
{
  class KerasModel
  {
    private BaseModel? model = null;
    private int epochs = 0;

    public bool CanPredict => this.model != null;

    public int Epochs { get; set; } = 200;
    public int BatchSize { get; set; } = 2;
    public string Activation { get; set; } = "relu";
    public string Optimizer { get; set; } = "adam";
    public string Loss { get; set; } = "binary_crossentropy";
    public bool IsKerasReguressor { get; set; } = false;

    public void Training(float[,] data, float[] results)
    {
      // data = new float[,] { { 0, 0, 1 }, { 0, 1, 1 }, { 1, 0, 0 }, { 1, 1, 0 } };
      // results = new float[] { 0, 1, 1, 0 };

      //Load train data
      NDarray x = np.array(data);
      NDarray y = np.array(results);

      //Build sequential model
      if (this.model == null)
      {
        var layers = new List<BaseLayer>();
        string[] lines;

        try
        {
          lines = File.ReadAllLines("scripts\\layer-config.txt");

          try
          {
            var options = ScriptOptions.Default.AddReferences(Assembly.GetAssembly(typeof(Keras.Keras)));
            foreach (var line in lines.Where((l) => !string.IsNullOrEmpty(l)))
            {
              var replaced = line.Replace("<shape>", data.GetLength(1).ToString());
              var script = CSharpScript.Create(replaced, options);
              var result = script.RunAsync().Result.ReturnValue;
              if (result is BaseLayer layer)
              {
                layers.Add(layer);
              }
              else
              {
                return;
              }
            }
          }
          catch (Exception ex)
          {
            return;
          }
        }
        catch
        {
        }

        var model = new Sequential();
        this.model = model;

        if (!layers.Any())
        {
          model.Add(new Dense(32, activation: "relu", input_shape: new Shape(data.GetLength(1))));
          model.Add(new Dense(64, activation: this.Activation));
          model.Add(new Dense(1, activation: "sigmoid"));
        }
        else
        {
          foreach (var layer in layers)
          {
            model.Add(layer);
          }
        }

        this.model.Compile(optimizer: this.Optimizer, loss: this.Loss, metrics: new string[] { "accuracy" });
      }


      //Compile and train
      if (!this.IsKerasReguressor)
      {
        this.model.Fit(x, y, batch_size: this.BatchSize,
          initial_epoch: this.epochs, epochs: this.epochs + this.Epochs, verbose: 1);
      }
      else
      {
        var estimator = new KerasReguressor(this.model, initial_epoch: this.epochs,
          epochs: this.Epochs, batch_size: this.BatchSize);
        estimator.Fit(x, y);
        estimator.Dispose();
      }
      this.epochs += this.Epochs;

      /*
      var score = this.model.Evaluate();
      score[0] 損失値（小さいほどよい）
      score[1] 正答率（大きいほどよい）
      */
    }

    public float[] Predict(float[,] data)
    {
      if (this.model == null)
      {
        return Array.Empty<float>();
      }

      // data = new float[,] { { 1, 1, 0 } };

      // var result = this.model.Predict(np.array(new float[,] { { 1, 1, 0 } }));
      var result = this.model.Predict(np.array(data), batch_size: 1, verbose: 0);
      return result.GetData<float>();
    }

    public void SaveFile(string fileName)
    {
      if (this.model == null)
      {
        return;
      }

      // string json = this.model.ToJson();
      // File.WriteAllText(fileName + ".json", json);
      // this.model.SaveWeight(fileName + ".h5");
      this.model.Save(fileName);

      File.WriteAllText(fileName + ".data", @$"epochs={this.epochs}");
    }

    public void LoadFile(string fileName)
    {
      var loaded_model = Sequential.LoadModel(fileName);
      // var loaded_model = Sequential.ModelFromJson(File.ReadAllText(fileName + ".json"));
      // loaded_model.LoadWeight(fileName + ".h5");
      this.model = loaded_model;

      var raw = File.ReadAllLines(fileName + ".data");
      var data = raw.Select((d) => d.Split("=")).ToDictionary((d) => d[0], (d) => d[1]);
      this.epochs = int.Parse(data["epochs"]);
    }

    public void Reset()
    {
      // this.model?.Dispose();
      this.model = null;
      this.epochs = 0;
    }
  }

  class KerasReguressor : Base
  {
    private dynamic estimator;

    public KerasReguressor(BaseModel model, int initial_epoch = 0, int epochs = 32, int batch_size = 2)
    {
      dynamic module = Py.Import("keras.wrappers.scikit_learn");

      var args = new Dictionary<string, object>
      {
        ["epochs"] = 1000,
        ["batch_size"] = 2
      };
      var dele = (Func<dynamic>)(() => model.ToPython());
      this.estimator = module.KerasRegressor(build_fn: dele.ToPython(), initial_epoch: initial_epoch, epochs: epochs, batch_size: batch_size);

      var field = typeof(KerasReguressor).GetField("PyInstance", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.SetField | System.Reflection.BindingFlags.GetField);
      field!.SetValue(this, this.estimator);
    }

    public History Fit(NDarray x, NDarray y)
    {
      var args = new Dictionary<string, object>
      {
        ["x"] = x,
        ["y"] = y,
      };
      var history = InvokeMethod("fit", args);
      return new History(history);
    }

    public new void Dispose()
    {
      try
      {
        // (this.estimator as PyObject)?.Dispose();
      }
      catch
      {

      }
    }
  }
}
