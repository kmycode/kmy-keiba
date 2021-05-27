using Keras;
using Keras.Layers;
using Keras.Models;
using Keras.Utils;
using KmyKeiba.Models.Data;
using Numpy;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Prompt.Models.Brains
{
  class KerasModel
  {
    private BaseModel? model = null;
    private int epochs = 0;

    public bool CanPredict => this.model != null;

    public void Training(float[,] data, float[] results)
    {
      //data = new float[,] { { 0, 0, 1 }, { 0, 1, 1 }, { 1, 0, 0 }, { 1, 1, 0 } };
      //results = new float[] { 0, 1, 1, 0 };

      //Load train data
      NDarray x = np.array(data);
      NDarray y = np.array(results);

      //Build sequential model
      if (this.model == null)
      {
        var model = new Sequential();
        this.model = model;
        model.Add(new Dense(32, activation: "relu", input_shape: new Shape(data.GetLength(1))));
        model.Add(new Dense(64, activation: "relu"));
        model.Add(new Dense(1, activation: "sigmoid"));
      }

      //Compile and train
      this.model.Compile(optimizer: "sgd", loss: "binary_crossentropy", metrics: new string[] { "accuracy" });
      this.model.Fit(x, y, batch_size: 2, initial_epoch: this.epochs, epochs: this.epochs + 1000, verbose: 1);
      this.epochs += 1000;

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
      var result = this.model.PredictOnBatch(np.array(data));
      return result.GetData<float>();
    }

    public void SaveFile(string fileName)
    {
      if (this.model == null)
      {
        return;
      }

      string json = this.model.ToJson();
      File.WriteAllText(fileName + ".json", json);
      this.model.SaveWeight(fileName + ".h5");

      File.WriteAllText(fileName + ".data", @$"epochs={this.epochs}");
    }

    public void LoadFile(string fileName)
    {
      var loaded_model = Sequential.ModelFromJson(File.ReadAllText(fileName + ".json"));
      loaded_model.LoadWeight(fileName + ".h5");
      this.model = loaded_model;

      var raw = File.ReadAllLines(fileName + ".data");
      var data = raw.Select((d) => d.Split("=")).ToDictionary((d) => d[0], (d) => d[1]);
      this.epochs = int.Parse(data["epochs"]);
    }

    public void Reset()
    {
      this.model = null;
      this.epochs = 0;
    }
  }
}
