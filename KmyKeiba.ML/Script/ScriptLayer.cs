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
    private readonly List<ScriptLayer> _profiles = new();

    public List<Action<KerasModel>> AfterTrainingActions { get; } = new();

    public List<Action<KerasModel>> BeforePredictionActions { get; } = new();

    public string ProfileName { get; init; } = string.Empty;

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

    [ScriptMember("epochs")]
    public int Epochs { get; set; } = 10;

    [ScriptMember("isContinuous")]
    public bool IsContinuous { get; set; }

    [ScriptMember("batchSize")]
    public int BatchSize { get; set; } = 2;

    [ScriptMember("verbose")]
    public int Verbose { get; set; } = 1;

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

    [ScriptMember("createProfile")]
    public ScriptLayer CreateProfile(string name)
    {
      var profile = new ScriptLayer
      {
        ProfileName = name,
      };
      this._profiles.Add(profile);
      return profile;
    }

    public ScriptLayer? GetProfile(string name)
    {
      if (string.IsNullOrEmpty(name))
      {
        return this;
      }
      return this._profiles.FirstOrDefault(p => p.ProfileName == name);
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

    [ScriptMember("activation")]
    public void Activation(string activation)
    {
      if (this._firstLayer == null)
      {
        this._firstLayer = shape => new Activation(activation, input_shape: new Shape(shape));
      }
      else
      {
        this._layers.Add(new Activation(activation));
      }
    }

    [ScriptMember("dropout")]
    public void Dropout(double rate, int? seed = null)
    {
      this._layers.Add(new Dropout(rate, seed: seed));
    }

    [ScriptMember("flatten")]
    public void Flatten(string format)
    {
      this._layers.Add(new Flatten(format));
    }

    [ScriptMember("activityRegularization")]
    public void ActivityRegularization(double l1, double l2)
    {
      if (this._firstLayer == null)
      {
        this._firstLayer = shape => new ActivityRegularization((float)l1, (float)l2, input_shape: new Shape(shape));
      }
      else
      {
        this._layers.Add(new ActivityRegularization((float)l1, (float)l2));
      }
    }

    [ScriptMember("masking")]
    public void Masking(double value)
    {
      this._layers.Add(new Masking((float)value));
    }

    [ScriptMember("batchNormalization")]
    public void BatchNormalization()
    {
      if (this._firstLayer == null)
      {
        this._firstLayer = shape => new BatchNormalization(input_shape: new Shape(shape));
      }
      else
      {
        this._layers.Add(new BatchNormalization());
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
