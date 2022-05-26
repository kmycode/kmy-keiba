using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMath = System.Math;

namespace KmyKeiba.Models.Data
{
  class ClusteringModel
  {
    private readonly MLContext ml;
    private ITransformer? model;
    private DataViewSchema? schema;
    private PredictionEngine<InputData, ClusterPrediction>? predictor;

    public bool CanSave => this.schema != null && this.model != null;

    public ClusteringModel()
    {
      this.ml = new(seed: 0);
    }

    public void LoadFile(string fileName)
    {
      this.model = this.ml.Model.Load(fileName, out DataViewSchema schema);
      this.schema = schema;
      this.predictor = this.ml.Model.CreatePredictionEngine<InputData, ClusterPrediction>(this.model);
    }

    public void SaveFile(string fileName)
    {
      if (!this.CanSave)
      {
        return;
      }

      this.ml.Model.Save(this.model, this.schema, fileName);
    }

    public void Training(IEnumerable<RaceHorseDataInput> data)
    {
      var dataView = this.ml.Data.LoadFromEnumerable(data.Select((d) => InputData.FromData(d)));

      var pipeline = this.ml.Transforms.Conversion.MapValueToKey("Label", "RunningStyle")
        .Append(this.ml.Transforms.Concatenate("Features", "FirstCornerOrder", "SecondCornerOrder", "ThirdCornerOrder", "FourthCornerOrder"))
        .AppendCacheCheckpoint(this.ml)
        .Append(this.ml.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))
        .Append(this.ml.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

      this.model = pipeline.Fit(dataView);
      this.schema = dataView.Schema;
      this.predictor = this.ml.Model.CreatePredictionEngine<InputData, ClusterPrediction>(this.model);
    }

    public uint Predict(RaceHorseDataInput data)
    {
      if (!this.CanSave || this.predictor == null)
      {
        return 0;
      }

      var prediction = this.predictor.Predict(InputData.FromData(data));

      return (uint)SMath.Round(prediction.PredictedClusterId);
    }

    public IEnumerable<uint> Predict(IEnumerable<RaceHorseDataInput> data)
    {
      if (!this.CanSave || this.predictor == null)
      {
        yield break;
      }

      foreach (var d in data)
      {
        var prediction = this.predictor.Predict(InputData.FromData(d));
        if (prediction != null)
        {
          yield return (uint)SMath.Round(prediction.PredictedClusterId);
        }
      }
    }

    private class InputData
    {
      [LoadColumn(0)]
      public float FirstCornerOrder;

      [LoadColumn(1)]
      public float SecondCornerOrder;

      [LoadColumn(2)]
      public float ThirdCornerOrder;

      [LoadColumn(3)]
      public float FourthCornerOrder;

      [LoadColumn(4)]
      public float ResultOrder;

      public float RunningStyle;

      public static InputData FromData(RaceHorseDataInput d)
      {
        return new InputData
        {
          FirstCornerOrder = (d.RaceHorse.FirstCornerOrder / (float)d.Race.HorsesCount),
          SecondCornerOrder = (d.RaceHorse.SecondCornerOrder / (float)d.Race.HorsesCount),
          ThirdCornerOrder = (d.RaceHorse.ThirdCornerOrder / (float)d.Race.HorsesCount),
          FourthCornerOrder = (d.RaceHorse.FourthCornerOrder / (float)d.Race.HorsesCount),
          ResultOrder = (d.RaceHorse.ResultOrder / (float)d.Race.HorsesCount),
          RunningStyle = (short)d.RaceHorse.RunningStyle,
        };
      }
    }

    private class ClusterPrediction
    {
      [ColumnName("PredictedLabel")]
      public float PredictedClusterId = 0;

      [ColumnName("Score")]
      public float[] Distances = new float[0];
    }

    public class RaceHorseDataInput
    {
      public RaceData Race { get; set; } = new();

      public RaceHorseData RaceHorse { get; set; } = new();
    }
  }
}
