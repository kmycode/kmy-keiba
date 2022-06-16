using Keras;
using Keras.Layers;
using Keras.Models;
using KmyKeiba.ML.Brain;
using Microsoft.ClearScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KmyKeiba.ML.Script
{
  [NoDefaultScriptAccess]
  public class ScriptLayer
  {
    public List<Action<KerasModel>> AfterTrainingActions { get; } = new();

    public List<Action<KerasModel>> BeforePredictionActions { get; } = new();

    [ScriptMember("layers")]
    public LayerList Layers { get; } = new();

    [ScriptMember("name")]
    public string Name { get; set; } = "__unnamed";

    [ScriptMember("optimizer")]
    public string Optimizer { get; set; } = "sgd";

    [ScriptMember("loss")]
    public string Loss { get; set; } = "binary_crossentropy";

    [ScriptMember("type")]
    public string Type { get; set; } = "binary";

    [ScriptMember("dotFileName")]
    public string DotFileName { get; set; } = string.Empty;

    public string[] Labels { get; set; } = Array.Empty<string>();

    public int ShapeLength { get; set; }

    [ScriptMember("setLabels")]
    public void SetLabels(string jsonArray)
    {
      var array = JsonSerializer.Deserialize<string[]>(jsonArray, ScriptRunner.JsonOptions) ?? Array.Empty<string>();
      this.Labels = array;
    }

    public Sequential ToModel()
    {
      var model = new Sequential();
      this.Layers.SetModel(model, this.ShapeLength);

      this.AfterTrainingActions.Add(m => m.SaveFile());
      this.BeforePredictionActions.Add(m => m.LoadFile());

      dynamic optimizer = this.Optimizer;
      if (this.Optimizer == "radam")
      {
        optimizer = new KerasObjectWrapper(OptimizerManager.Radam!.RAdam());
      }

      model.Compile(optimizer: optimizer, loss: this.Loss, metrics: new string[] { "accuracy" });
      return model;
    }
  }

  [NoDefaultScriptAccess]
  public class LayerList
  {
    private Func<int, BaseLayer>? _firstLayer;
    private readonly List<BaseLayer> _layers = new();

    [ScriptMember("dense")]
    public void Dense(int units, string activation)
    {
      if (this._firstLayer == null)
      {
        this._firstLayer = shape => new Dense(units, activation: activation, input_shape: new Shape(shape));
      }
      else
      {
        this._layers.Add(new Dense(units, activation: activation));
      }
    }

    public void SetModel(Sequential model, int shapeLength)
    {
      if (this._firstLayer != null)
      {
        model.Add(this._firstLayer(shapeLength));
      }
      foreach (var layer in this._layers)
      {
        model.Add(layer);
      }
    }
  }
}
