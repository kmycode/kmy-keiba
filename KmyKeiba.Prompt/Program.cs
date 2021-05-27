using KmyKeiba.Models.Data;
using System;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms;
using Microsoft.ML.Data;
using Keras.Layers;
using Keras;
using Keras.Models;
using Numpy;
using System.IO;
using Python.Runtime;
using KmyKeiba.JVLink.Wrappers;
using System.Threading.Tasks;
using KmyKeiba.Prompt.Logics;
using System.Collections;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Logics.Logics;
using KmyKeiba.Prompt.Models.Brains;
using System.Threading;

namespace KmyKeiba.Prompt
{
  class Program
  {
#if X64
    static bool isX64 = true;
    static KerasModel? ai = new KerasModel();
#else
    static bool isX64 = false;
    static KerasModel? ai = null;
#endif
    static JVLinkObject link;
    static JVLinkLoader loader = new();
    static PredictRunningStyleModel runningStyle = new();
    static string mode;

    static Program()
    {
      link = JVLinkObject.Local;
      mode = "地方";
    }

    static void Main(string[] args)
    {
      using (var db = new MyContext())
      {
        db.Database.Migrate();
      }

      Menu();
    }

    static void Menu()
    {
      Console.Clear();
      var isExit = false;

      var addMenu = "";
      if (!isX64)
      {
        addMenu = @"
  update                       最新データ取得
  update <yyyymmdd-yyyymmdd>   指定期間データ取得
  update <yyyymmdd>            指定日以降データ取得
";
      }
      else
      {
        addMenu = @"
  dl <load|save> <fileName without extensions>
  dl predict <racekey>         予測
  dl training <yyyymmdd-yyyymmdd> トレーニング
";
      }

      while (!isExit)
      {
        Console.Clear();
        Console.WriteLine($@"
競馬予想【{mode}】{(isX64 ? "64bit" : "32bit")}

====================================================
  mode <central|local>         中央地方の切替
  config                       JVLink/UmaConn 設定

  races                        今日のレース一覧
  race <key>                   レース情報表示
{addMenu}
  ml rs                        脚質判定（機械学習）

  exit                         終了
====================================================

コマンドを入力してください...
");
        var input = Console.ReadLine();

        var parameters = input.Split(" ");
        var command = parameters[0];

        switch (command)
        {
          case "mode":
            {
              if (parameters.Length != 2)
              {
                Console.WriteLine("パラメータが不正です");
                Task.Delay(3000).Wait();
              }
              switch (parameters[1])
              {
                case "central":
                  link = JVLinkObject.Central;
                  mode = "中央";
                  break;
                case "local":
                  link = JVLinkObject.Local;
                  mode = "地方";
                  break;
                default:
                  Console.WriteLine("パラメータが不正です");
                  Task.Delay(3000).Wait();
                  break;
              }
            }
            break;
          case "config":
            link.OpenConfigWindow();
            break;
          case "update":
            if (isX64)
            {
              Console.WriteLine("64bit では実行できません");
              Task.Delay(3000).Wait();
            }
            else
            {
              Update(parameters);
            }
            break;
          case "races":
            ShowRaces(DateTime.Today, DateTime.Today.AddDays(1));
            break;
          case "race":
            if (parameters.Length != 2)
            {
              Console.WriteLine("パラメータが不正です");
              Task.Delay(3000).Wait();
            }
            else
            {
              ShowRaceDetail(parameters[1]);
            }
            break;
          case "ml":
            if (parameters.Length < 2)
            {
              Console.WriteLine("パラメータが不正です");
              Task.Delay(3000).Wait();
            }
            else if (parameters[1] == "rs")
            {
              PredictRunningStyles(parameters.Length >= 3 ? parameters[2] : string.Empty,
                                   parameters.Length >= 4 ? parameters[3] : string.Empty);
            }
            break;
          case "dl":
            if (!isX64)
            {
              Console.WriteLine("32bit では実行できません");
              Task.Delay(3000).Wait();
            }
            else if (parameters.Length < 2)
            {
              Console.WriteLine("パラメータが不正です");
              Task.Delay(3000).Wait();
            }
            else
            {
              switch (parameters[1])
              {
                case "load":
                  if (parameters.Length != 3)
                  {
                    Console.WriteLine("パラメータが不正です");
                    Task.Delay(3000).Wait();
                  }
                  else
                  {
                    LoadLearningData(parameters[2]);
                  }
                  break;
                case "save":
                  if (parameters.Length != 3)
                  {
                    Console.WriteLine("パラメータが不正です");
                    Task.Delay(3000).Wait();
                  }
                  else
                  {
                    SaveLearningData(parameters[2]);
                  }
                  break;
                case "predict":
                  if (parameters.Length != 3)
                  {
                    Console.WriteLine("パラメータが不正です");
                    Task.Delay(3000).Wait();
                  }
                  else
                  {
                    PredictRaceResult(parameters[2]);
                  }
                  break;
                case "training":
                  if (parameters.Length != 3)
                  {
                    Console.WriteLine("パラメータが不正です");
                    Task.Delay(3000).Wait();
                  }
                  else
                  {
                    Learning(parameters[2]);
                  }
                  break;
                default:
                  Console.WriteLine("パラメータが不正です");
                  Task.Delay(3000).Wait();
                  break;
              }
            }
            break;
          case "exit":
            isExit = true;
            break;
        }
      }
    }

    static void Update(string[] parameters)
    {
      if (parameters.Length > 3)
      {
        Console.WriteLine("パラメータが不正です");
        Task.Delay(3000).Wait();
        return;
      }

      if (parameters.Length == 1)
      {
        // 最新データ
        Task.Run(async () =>
        {
          IEnumerable<string> planKeys;
          IEnumerable<string> startKeys;
          using (var db = new MyContext())
          {
            Expression<Func<RaceData, bool>> subject =
              mode == "地方" ? (r) => (short)r.Course >= 30 :
                               (r) => (short)r.Course < 30;
            planKeys = await db.Races!
              .Where((r) => r.StartTime >= DateTime.Now)
              .Where((r) => (short)r.DataStatus < (short)RaceDataStatus.PreliminaryGrade)
              .Where(subject)
              .Select((r) => r.Key)
              .ToArrayAsync();
            startKeys = await db.Races!
              .Where((r) => r.StartTime < DateTime.Now)
              .Where((r) => (short)r.DataStatus < (short)RaceDataStatus.PreliminaryGrade)
              .Where(subject)
              .Select((r) => r.Key)
              .ToArrayAsync();
          }

          var processSize = startKeys.Concat(planKeys).Count() * 4;
          var processed = 0;
          if (processSize == 0)
          {
            Console.WriteLine("今日のレースデータはありません");
            Task.Delay(3000).Wait();
            return;
          }

          Console.WriteLine();
          Console.WriteLine();

          foreach (var key in planKeys.Concat(startKeys))
          {
            if (startKeys.Contains(key))
            {
              await loader.LoadAsync(link, JVLinkDataspec.RB12, JVLinkOpenOption.RealTime, key, null, null);
            }
            else
            {
              await loader.LoadAsync(link, JVLinkDataspec.RB15, JVLinkOpenOption.RealTime, key, null, null);
            }
            StepProgress(++processed, processSize);

            await loader.LoadAsync(link, JVLinkDataspec.RB30, JVLinkOpenOption.RealTime, key, null, null);
            StepProgress(++processed, processSize);
            await loader.LoadAsync(link, JVLinkDataspec.RB11, JVLinkOpenOption.RealTime, key, null, null);
            StepProgress(++processed, processSize);
            await loader.LoadAsync(link, JVLinkDataspec.RB14, JVLinkOpenOption.RealTime, key, null, null);
            StepProgress(++processed, processSize);
          }
        }).Wait();

        return;
      }

      if (parameters.Length >= 2)
      {
        // 指定日データ
        Task.Run(async () =>
        {
          var date = parameters[1].Split("-");
          if (date.Length > 2)
          {
            Console.WriteLine("日付の形式が不正です");
            Task.Delay(3000).Wait();
            return;
          }

          DateTime startTime;
          DateTime? endTime = null;
          try
          {
            startTime = DateTime.ParseExact(date[0], "yyyyMMdd", null, System.Globalization.DateTimeStyles.None);
          }
          catch
          {
            Console.WriteLine("日付の形式が不正です");
            Task.Delay(3000).Wait();
            return;
          }
          if (date.Length == 2)
          {
            try
            {
              endTime = DateTime.ParseExact(date[1], "yyyyMMdd", null, System.Globalization.DateTimeStyles.None);
              endTime = endTime.Value.AddDays(1);
            }
            catch
            {
              Console.WriteLine("日付の形式が不正です");
              Task.Delay(3000).Wait();
              return;
            }
          }

          var types = Enumerable.Empty<string>().ToArray();
          if (parameters.Length == 3)
          {
            types = parameters[2].ToUpper().Split(",");
          }

          Console.WriteLine();
          Console.WriteLine();

          var day = startTime;
          // while (day < endTime)
          try
          {
            var task = Task.Run(async () =>
            {
              await loader.LoadAsync(link,
                JVLinkDataspec.Race,
                day >= DateTime.Today.AddYears(-1) ? JVLinkOpenOption.Normal : JVLinkOpenOption.Setup,
                null,
                day,
                endTime,
                types);
                //new string[] { "RA", "SE", });
                //new string[] { "HR", "O1", "O2", });
                //new string[] { "O1", });
                //new string[] { "O2", });
                //new string[] { "O3", });
                //new string[] { "O4", });
                //new string[] { "O5", });
                //new string[] { "O6", });
            });

            Console.WriteLine("ダウンロード\n");
            while (!task.IsCompleted && !task.IsCanceled)
            {
              Console.Clear();

              StepProgress(loader.Downloaded.Value, loader.DownloadSize.Value);
              Console.WriteLine();
              StepProgress(loader.Loaded.Value, loader.LoadSize.Value);
              Console.WriteLine();
              StepProgress(loader.Saved.Value, loader.SaveSize.Value);
              Console.WriteLine();
              StepProgress(loader.Processed.Value, loader.ProcessSize.Value);
              Console.WriteLine();

              Console.WriteLine();
              Console.WriteLine($"Entities: {loader.LoadEntityCount.Value}    {string.Join(",", types)}");
              Console.WriteLine(day.ToString("yyyy/MM/dd") + "-" + endTime?.ToString("yyyy/MM/dd"));

              await Task.Delay(1000);
            }

            day = day.AddDays(1);
          }
          catch (Exception ex)
          {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            Task.Delay(600_000).Wait();
          }
        }).Wait();
      }
    }

    static void ShowRaces(DateTime from, DateTime to)
    {
      Task.Run(async () =>
      {
        Expression<Func<RaceData, bool>> subject =
          mode == "地方" ? (r) => (short)r.Course >= 30 :
                           (r) => (short)r.Course < 30;

        using (var db = new MyContext())
        {
          var races = await db.Races!
            .Where(subject)
            .Where((r) => r.StartTime >= from && r.StartTime < to)
            .OrderBy((r) => r.Key)
            .ToArrayAsync();

          Console.WriteLine();
          Console.WriteLine();
          foreach (var race in races)
          {
            Console.WriteLine($"[{race.Key}] {race.Course.GetName()} {race.CourseRaceNumber} {race.Name}");
          }
        }
      }).Wait();

      Console.ReadLine();
    }

    static void ShowRaceDetail(string key)
    {
      Task.Run(async () =>
      {
        Expression<Func<RaceData, bool>> subject =
          mode == "地方" ? (r) => (short)r.Course >= 30 :
                           (r) => (short)r.Course < 30;

        using (var db = new MyContext())
        {
          var race = await db.Races!.FirstOrDefaultAsync((r) => r.Key == key);
          if (race != null)
          {
            var horses = await db.RaceHorses!.Where((h) => h.RaceKey == key).OrderBy((h) => h.Number).ToArrayAsync();
            Console.WriteLine($@"

[{race.Key}] {race.Course.GetName()} {race.CourseRaceNumber} {race.Name}");
            foreach (var horse in horses)
            {
              Console.WriteLine($@"{horse.Number} : {horse.Name}");
            }

            Console.ReadLine();
          }
          else
          {
            Console.WriteLine("レースはありません");
            Task.Delay(3000).Wait();
          }
        }
      }).Wait();
    }

    static void LoadLearningData(string fileName)
    {
      if (ai == null)
      {
        return;
      }

      Console.WriteLine("ファイルロード中...");
      Task.Run(() =>
      {
        try
        {
          ai.LoadFile(fileName);
        }
        catch (Exception ex)
        {
          Console.WriteLine("読み込めませんでした");
          Console.WriteLine(ex.Message);
          Console.WriteLine(ex.StackTrace);
          Console.ReadLine();
        }
      }).Wait();
    }

    static void SaveLearningData(string fileName)
    {
      if (ai == null)
      {
        return;
      }

      if (!ai.CanPredict)
      {
        Console.WriteLine("データがありません");
        Console.ReadLine();
        return;
      }

      Console.WriteLine("ファイル保存中...");
      Task.Run(() =>
      {
        try
        {
          ai.SaveFile(fileName);
        }
        catch (Exception ex)
        {
          Console.WriteLine("保存できませんでした");
          Console.WriteLine(ex.Message);
          Console.WriteLine(ex.StackTrace);
          Console.ReadLine();
        }
      }).Wait();
    }

    static void Learning(string dateFormat)
    {
      var date = dateFormat.Split("-");

      DateTime startTime;
      DateTime? endTime = null;
      try
      {
        startTime = DateTime.ParseExact(date[0], "yyyyMMdd", null, System.Globalization.DateTimeStyles.None);
      }
      catch
      {
        Console.WriteLine("日付の形式が不正です");
        Task.Delay(3000).Wait();
        return;
      }
      if (date.Length == 2)
      {
        try
        {
          endTime = DateTime.ParseExact(date[1], "yyyyMMdd", null, System.Globalization.DateTimeStyles.None);
          endTime = endTime.Value.AddDays(1);
        }
        catch
        {
          Console.WriteLine("日付の形式が不正です");
          Task.Delay(3000).Wait();
          return;
        }
      }

      Learning(startTime, endTime ?? DateTime.Today);
    }

    static void Learning(DateTime from, DateTime to)
    {
      if (ai == null)
      {
        return;
      }

      var data = new float[0, 0];
      var results = new float[0];

      Task.Run(async () =>
      {
        using (var db = new MyContext())
        {
          /*
          var races = await db.Races!
            .Where((r) => r.StartTime >= from && r.StartTime < to)
            .Join(db.RaceHorses!, (h) => h.Key, (r) => r.RaceKey, (h, r) => new { Horse = r, Race = h, })
            .Where((d) => d.Horse.ResultOrder != 0)
            .ToArrayAsync();
          */
        }
      }).Wait();

      ai.Training(data, results);
    }

    static void PredictRunningStyles(string fileName, string saveFileName)
    {
      Console.WriteLine();
      Console.WriteLine();

      Task.Run(() =>
      {
        if (fileName == "_" || fileName == string.Empty)
        {
          Console.WriteLine("データベースからロード中...");
          runningStyle.Training();
        }
        else
        {
          Console.WriteLine("ファイルからロード中...");
          runningStyle.OpenFile(fileName);
        }

        if (!runningStyle.CanPredict.Value)
        {
          Console.WriteLine("エラー発生");
          Task.Delay(3000).Wait();
          return;
        }

        Console.WriteLine("予測中...");
        runningStyle.Predict();

        if (!string.IsNullOrEmpty(saveFileName))
        {
          Console.WriteLine("保存中...");
          runningStyle.SaveFile(saveFileName);
        }
      }).Wait();
    }

    static void PredictRaceResult(string key)
    {
      if (ai == null)
      {
        return;
      }
      if (!ai.CanPredict)
      {
        Console.WriteLine("学習していません");
        Task.Delay(3000).Wait();
        return;
      }

      var data = new float[0, 0];

      Task.Run(async () =>
      {
        Expression<Func<RaceData, bool>> subject =
          mode == "地方" ? (r) => (short)r.Course >= 30 :
                           (r) => (short)r.Course < 30;

        using (var db = new MyContext())
        {
          var race = await db.Races!.FirstOrDefaultAsync((r) => r.Key == key);
          if (race != null)
          {
            var horses = await db.RaceHorses!.Where((h) => h.RaceKey == key).OrderBy((h) => h.Number).ToArrayAsync();
          }
          else
          {
            Console.WriteLine("レースはありません");
            Task.Delay(3000).Wait();
          }
        }
      }).Wait();

      Console.WriteLine();
      Console.WriteLine();
      Console.WriteLine();
      Console.WriteLine(ai.Predict(data)[0]);
      Console.ReadLine();
    }

    static void SimulateRaceResults(DateTime from, DateTime to)
    {
      Task.Run(async () =>
      {
        var allCount = 0;
        var processed = 0;
        var hit = 0;

        Expression<Func<RaceData, bool>> subject =
          mode == "地方" ? (r) => (short)r.Course >= 30 :
                           (r) => (short)r.Course < 30;

        using (var db = new MyContext())
        {
          Console.WriteLine("データベースから取得中...");
          var races = await db.Races!
            .Where((r) => r.StartTime >= from && r.StartTime < to)
            .Join(db.RaceHorses!, (r) => r.Key, (h) => h.RaceKey, (r, h) => new { Race = r, Horse = h, })
            .ToArrayAsync();
          allCount = races.Length;

          foreach (var race in races)
          {
            StepProgress(++processed, allCount);
          }
        }

        Console.WriteLine($"的中率: {hit} / {allCount} = {((float)hit / Math.Max(1, allCount)) / 100} %");
      }).Wait();
    }

    static void StepProgress(int processed, int processSize)
    {
      var num = (int)((float)processed / Math.Max(1, processSize) * 32);
      Console.Write($"\r[{string.Concat(Enumerable.Repeat("=", num))}{string.Concat(Enumerable.Repeat(" ", 32 - num))}] ({processed}/{processSize})        ");
    }


    private void LoadData()
    {
      using (var db = new MyContext())
      {
        var races = db.Races!
          .OrderByDescending((h) => h.StartTime)
          .Take(100)
          .Join(db.RaceHorses!, (h) => h.Key, (r) => r.RaceKey, (h, r) => new { Horse = r, Race = h, })
          .ToArray();
      }
    }

    private void DlTest()
    {
      //Load train data
      NDarray x = np.array(new float[,] { { 0, 0, 1 }, { 0, 1, 1 }, { 1, 0, 0 }, { 1, 1, 0 } });
      NDarray y = np.array(new float[] { 0, 1, 1, 0 });

      //Build sequential model
      var model = new Sequential();
      model.Add(new Dense(32, activation: "relu", input_shape: new Shape(3)));
      model.Add(new Dense(64, activation: "relu"));
      model.Add(new Dense(1, activation: "sigmoid"));

      //Compile and train
      model.Compile(optimizer: "sgd", loss: "binary_crossentropy", metrics: new string[] { "accuracy" });
      model.Fit(x, y, batch_size: 2, epochs: 1000, verbose: 1);

      var result = model.Predict(np.array(new float[,] { { 1, 1, 0 } }));

      /*
      //Save model and weights
      string json = model.ToJson();
      File.WriteAllText("model.json", json);
      model.SaveWeight("model.h5");

      //Load model and weight
      var loaded_model = Sequential.ModelFromJson(File.ReadAllText("model.json"));
      loaded_model.LoadWeight("model.h5");
      */
    }

    private class TestInput
    {
      public float Value1;

      public float Value2;

      public float Result;
    }

    public class TestResult
    {
      [ColumnName("Score")]
      public float Result;
    }

    private void MlTest()
    {
      //コンテキストの生成
      MLContext mlContext = new MLContext(seed: 1);
      //データのロード
      IDataView data = mlContext.Data.LoadFromEnumerable<TestInput>(new TestInput[]
      {
        new TestInput()
        {
          Value1 = 1,
          Value2 = 0,
          Result = 500,
        },
        new TestInput()
        {
          Value1 = 2,
          Value2 = 0,
          Result = 800,
        },
        new TestInput()
        {
          Value1 = 2,
          Value2 = 1,
          Result = 700,
        },
        new TestInput()
        {
          Value1 = 2,
          Value2 = 2,
          Result = 600,
        },
        new TestInput()
        {
          Value1 = 3,
          Value2 = 1,
          Result = 1000,
        },
        new TestInput()
        {
          Value1 = 3,
          Value2 = 2,
          Result = 900,
        },
        new TestInput()
        {
          Value1 = 4,
          Value2 = 1,
          Result = 1300,
        },
        new TestInput()
        {
          Value1 = 4,
          Value2 = 2,
          Result = 1200,
        },
      });

      //学習データとテスト（訓練用）データに分割
      var split = mlContext.Data.TrainTestSplit(data, testFraction: 0.25, seed: 0);

      var dataProcessPipeline = mlContext.Transforms.Concatenate(
  outputColumnName: "Features",
  nameof(TestInput.Value1),
  nameof(TestInput.Value2));

      //学習アルゴリズムの定義
      // https://docs.microsoft.com/ja-jp/dotnet/machine-learning/how-to-choose-an-ml-net-algorithm
      var trainer = mlContext.Regression.Trainers.Sdca(labelColumnName: "Result", featureColumnName: "Features");
      //var trainer = mlContext.Regression.Trainers.FastForest(labelColumnName: "Label", featureColumnName: "Features");

      //学習アルゴリズムをパイプラインに設定
      var trainingPipeline = dataProcessPipeline.Append(trainer);

      //学習データを用いて学習モデルを生成
      var trainedModel = trainingPipeline.Fit(split.TrainSet);

      //テストデータによるモデルの評価
      //生成した学習モデルにテストデータを設定
      IDataView predictions = trainedModel.Transform(split.TestSet);
      //学習モデルの評価
      var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: "Result", scoreColumnName: "Score");

      Console.WriteLine($"*   損失関数(LossFn): {metrics.LossFunction}");
      Console.WriteLine($"*   決定係数(R2 Score): {metrics.RSquared}");
      Console.WriteLine($"*   平均絶対誤差(Absolute loss): {metrics.MeanAbsoluteError}");
      Console.WriteLine($"*   平均二乗誤差(Squared loss): {metrics.MeanSquaredError}");
      Console.WriteLine($"*   平均二乗誤差平方根(RMS loss): {metrics.RootMeanSquaredError}");

      // 予測
      var predictor = mlContext.Model.CreatePredictionEngine<TestInput, TestResult>(trainedModel);
      var prediction = predictor.Predict(new TestInput
      {
        Value1 = 7,
        Value2 = 2,
      });

      Console.WriteLine(prediction.Result);
    }

    private void SaveLoadTest()
    {
      /*
      string modelFilePath = $@".\Data\model{DateTimeOffset.Now:yyyyMMddHmmss}.zip";
      //学習モデルをファイルに保存
      mlContext.Model.Save(trainedModel, split.TrainSet.Schema, modelFilePath);

      //学習モデルのロード
      ITransformer model = mlContext.Model.Load(modelFilePath, out DataViewSchema inputSchema);
      //推論エンジンの生成
      var predictionEngine = mlContext.Model.CreatePredictionEngine<WineQualityData, WineQualityPrediction>(model);

      //各説明変数を定義したオブジェクトを生成
      WineQualityData wineQualityData = new WineQualityData()
      {
        //TODO: 各属性の設定
        //FixedAcidity = ....
      };

      // 推論の実行
      WineQualityPrediction predictionResult = predictionEngine.Predict(wineQualityData);
      */
    }
  }
}
