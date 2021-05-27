using Keras;
using Keras.Layers;
using Keras.Models;
using KmyKeiba.Models.Data;
using Numpy;
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
    private Sequential? model = null;

    public bool CanPredict => this.model != null;

    public void Training(float[,] data, float[] results)
    {
      //Load train data
      NDarray x = np.array(new float[,] { { 0, 0, 1 }, { 0, 1, 1 }, { 1, 0, 0 }, { 1, 1, 0 } });
      NDarray y = np.array(new float[] { 0, 1, 1, 0 });

      //Build sequential model
      this.model = new Sequential();
      this.model.Add(new Dense(32, activation: "relu", input_shape: new Shape(3)));
      this.model.Add(new Dense(64, activation: "relu"));
      this.model.Add(new Dense(1, activation: "sigmoid"));

      //Compile and train
      this.model.Compile(optimizer: "sgd", loss: "binary_crossentropy", metrics: new string[] { "accuracy" });
      this.model.Fit(x, y, batch_size: 2, epochs: 1000, verbose: 1);
    }

    public float[] Predict(float[,] data)
    {
      if (this.model == null)
      {
        return Array.Empty<float>();
      }

      // var result = this.model.Predict(np.array(new float[,] { { 1, 1, 0 } }));
      var result = this.model.PredictOnBatch(np.array(new float[,] { { 1, 1, 0 } }));
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
    }

    public void LoadFile(string fileName)
    {
      var loaded_model = Sequential.ModelFromJson(File.ReadAllText(fileName + ".json"));
      loaded_model.LoadWeight(fileName + ".h5");
      this.model = loaded_model as Sequential;
    }
  }
}
