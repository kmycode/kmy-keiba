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
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Reflection;

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
      Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

      // var g = new KerasModel();
      // g.Training(new float[,] { { 0, 0, 0 }, { 1,0,0, } }, new float[] { 1, 0 });

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

      Func<string> addMenu;
      if (!isX64)
      {
        addMenu = () => @"
  update                       最新データ取得
  update <yyyymmdd-yyyymmdd>   指定期間データ取得
  update <yyyymmdd>            指定日以降データ取得
";
      }
      else
      {
        addMenu = () => $@"
  dl <load|save> <fileName without extensions>
  dl predict <racekey>
  dl <training|simulate> <yyyymmdd-yyyymmdd>
  dl retraining
  dl config <epochs|batch_size|activation|optimizer|loss|isreguressor> <value>
    ep:{ai!.Epochs}  ba:{ai!.BatchSize}  ac:{ai!.Activation}
    op:{ai!.Optimizer}  lo:{ai!.Loss} is:{ai!.IsKerasReguressor}
  dl reset
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
{addMenu()}
  ml rs                        脚質判定（機械学習）

  exit                         終了
====================================================

コマンドを入力してください...
");
        var input = Console.ReadLine() ?? string.Empty;

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
                case "simulate":
                  if (parameters.Length != 3)
                  {
                    Console.WriteLine("パラメータが不正です");
                    Task.Delay(3000).Wait();
                  }
                  else
                  {
                    SimulateRaceResults(parameters[2]);
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
                case "reset":
                  ai!.Reset();
                  break;
                case "retraining":
                  ReLearning();
                  break;
                case "config":
                  if (parameters.Length != 4)
                  {
                    Console.WriteLine("パラメータが不正です");
                    Task.Delay(3000).Wait();
                  }
                  else
                  {
                    SetConfig(parameters[2], parameters[3]);
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

    class DataLoader
    {
      public int TotalHorses { get; set; }

      public int Learned { get; set; }

      public float[,] Data { get; set; } = new float[0, 0];

      public float[] Results { get; set; } = new float[0];

      public async Task LearnAsync(DateTime f1, DateTime f2)
      {
        using (var db = new MyContext())
        {
          Expression<Func<RaceData, bool>> subject =
            mode == "地方" ? (r) => (short)r.Course >= 30 :
                              (r) => (short)r.Course < 30;

          var races = await db.Races!
            .Where((r) => r.StartTime >= f1 && r.StartTime < f2)
            .Where((r) => r.TrackType == TrackType.Flat)
            .Where(subject)
            .ToArrayAsync();
          var raceKeys = races.Select((r) => r.Key).ToArray();
          var allHorsesCount = await db.RaceHorses!.CountAsync((h) => raceKeys.Contains(h.RaceKey) && h.ResultOrder <= 8 && h.ResultOrder != 0);

          var shape = LearningData.GetShape();
          var row = 0;
          var data = new float[allHorsesCount, shape];
          var results = new float[allHorsesCount];
          this.Data = data;
          this.Results = results;

          this.TotalHorses = allHorsesCount;

          foreach (var race in races)
          {
            var sinceTime = race.StartTime.AddYears(-1);

            var horses = await db.RaceHorses!.Where((h) => h.ResultOrder <= 8 && h.ResultOrder != 0 && h.RaceKey == race.Key).ToArrayAsync();
            var horseNames = horses.Select((h) => h.Name);
            var allHorseHistories = (await db.RaceHorses!
              .Where((h) => horseNames.Contains(h.Name))
              .Join(db.Races!.Where((r) => r.StartTime < race.StartTime && r.StartTime >= sinceTime), (h) => h.RaceKey, (r) => r.Key, (h, r) => new { Horse = h, Race = r, })
              .OrderByDescending((h) => h.Race.StartTime)
              .ToArrayAsync());
            foreach (var horse in horses)
            {
              /*
              var history = await db.RaceHorses!
                .Where((h) => h.Name == horse.Name && h.ResultOrder != 0)
                .Join(db.Races!, (h) => h.RaceKey, (r) => r.Key, (h, r) => new { Race = r, Horse = h, })
                .Where((d) => d.Race.StartTime < race.StartTime)
                .OrderByDescending((d) => d.Race.StartTime)
                .Take(5)
                .ToArrayAsync();
              */
              var history = allHorseHistories.Where((h) => h.Horse.Name == horse.Name).ToArray();
              var rawData = LearningData.Create(
                race,
                horse,
                history.Select((h) => (h.Race, h.Horse)),
                allHorseHistories.Select((h) => (h.Race, h.Horse)));
              var raw = rawData.ToArray();
              for (var i = 0; i < shape; i++)
              {
                data[row, i] = raw[i];
              }
              results[row] = rawData.Result;
              row++;

              this.Learned++;
            }
          }

          /*
          var races = await db.Races!
            .Where((r) => r.StartTime >= from && r.StartTime < to)
            .Join(db.RaceHorses!, (h) => h.Key, (r) => r.RaceKey, (h, r) => new { Horse = r, Race = h, })
            .Where((d) => d.Horse.ResultOrder != 0);
          */
        }
      }
    }

    static void Learning(DateTime from, DateTime to)
    {
      if (ai == null)
      {
        return;
      }

      Console.WriteLine();
      Console.WriteLine();
      Console.WriteLine();
      Console.WriteLine("レースのロード中...");

      var splits = 8;
      var seconds = (to - from).TotalSeconds / splits;
      var d1 = from;
      var d2 = from.AddSeconds(seconds);
      var tasks = new List<Task>();
      var learnings = new List<DataLoader>();
      for (var m = 0; m < splits; m++)
      {
        var learning = new DataLoader();
        learnings.Add(learning);
        tasks.Add(learning.LearnAsync(d1, d2));
        d1 = d1.AddSeconds(seconds);
        d2 = d2.AddSeconds(seconds);
      }
      while (!tasks.All((t) => t.IsCompleted))
      {
        StepProgress(learnings.Sum((l) => l.Learned), learnings.Sum((l) => l.TotalHorses));
        Task.Delay(1000).Wait();
      }

      var data = new float[learnings.Sum((l) => l.Data.GetLength(0)), LearningData.GetShape()];
      var results = new float[learnings.Sum((l) => l.Results.Length)];
      var i = 0;
      foreach (var learning in learnings)
      {
        for (var j = 0; j < learning.Data.GetLength(0); j++)
        {
          for (var k = 0; k < learning.Data.GetLength(1); k++)
          {
            data[i, k] = learning.Data[j, k];
          }
          results[i] = learning.Results[j];
          i++;
        }
      }

      dataCache = data;
      resultsCache = results;
      ReLearning();
    }
    private static float[,]? dataCache;
    private static float[]? resultsCache;

    static void ReLearning()
    {
      if (dataCache == null || resultsCache == null)
      {
        return;
      }

      Console.WriteLine();
      Console.WriteLine("トレーニング中...");
      try
      {
        ai!.Reset();
        ai!.Training(dataCache, resultsCache);
      }
      catch (Exception ex)
      {
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine(ex.Message);
        Console.WriteLine(ex.StackTrace);
        return;
      }

      Console.WriteLine();
      Console.WriteLine();
      Console.WriteLine();
      Console.WriteLine("トレーニングが完了しました。損失値を確認してください");
      Console.ReadLine();
    }

    static void SetConfig(string name, string value)
    {
      if (ai == null)
      {
        return;
      }

      int.TryParse(value, out int intValue);

      // <epochs|batch_size|activation|optimizer|loss|isreguressor>
      switch (name)
      {
        case "epochs":
          ai.Epochs = intValue;
          break;
        case "batch_size":
          ai.BatchSize = intValue;
          break;
        case "activation":
          ai.Activation = value;
          break;
        case "optimizer":
          ai.Optimizer = value;
          break;
        case "loss":
          ai.Loss = value;
          break;
        case "isreguressor":
          ai.IsKerasReguressor = value == "true";
          break;
      }
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

      Console.WriteLine();
      Console.WriteLine();
      Console.WriteLine();
      Console.WriteLine("データ読み込み中...");

      List<(float[,] DataSet, RaceHorseData Horse)> dataSets = new();

      using (var db = new MyContext())
      {
        dataSets = GetPredictDataAsync(db, key).Result;
      }

      var predicted = new List<(float Prediction, RaceHorseData Horse)>();
      foreach (var dataSet in dataSets)
      {
        var prediction = ai.Predict(dataSet.DataSet);
        predicted.Add((prediction[0], dataSet.Horse));
      }
      foreach (var p in predicted.OrderByDescending((pp) => pp.Prediction))
      {
        Console.Write($"{p.Horse.Number} : {p.Horse.Name}\n");
        Console.Write($"{p.Prediction}\n\n");
      }

      Console.WriteLine();
      Console.WriteLine("--------------------------------");

      try
      {
        var items = Buyer.Select(predicted.Select((p) => new BuyCandidate { Horse = p.Horse, Prediction = p.Prediction, }));
        Console.WriteLine(string.Join("\n", items.Select((i) => i.Text).Where((i) => !string.IsNullOrEmpty(i))));
      }
      catch
      {
        Console.WriteLine("馬券購入処理でエラー発生しました");
      }

      Console.ReadLine();
    }

    static async Task<List<(float[,] DataSet, RaceHorseData Horse)>> GetPredictDataAsync(MyContext db, string raceKey)
    {
      var list = new List<(float[,] DataSet, RaceHorseData Horse)>();

      var race = await db.Races!.FirstOrDefaultAsync((r) => r.Key == raceKey);
      if (race != null)
      {
        var sinceTime = race.StartTime.AddYears(-1);

        var horses = await db.RaceHorses!
          .Where((h) => h.RaceKey == raceKey)
          .OrderBy((h) => h.Number)
          .ToArrayAsync();
        var horseNames = horses.Select((h) => h.Name);
        var allHorseHistories = (await db.RaceHorses!
          .Where((h) => horseNames.Contains(h.Name))
          .Join(db.Races!.Where((r) => r.StartTime < race.StartTime && r.StartTime >= sinceTime), (h) => h.RaceKey, (r) => r.Key, (h, r) => new { Horse = h, Race = r, })
          .OrderByDescending((h) => h.Race.StartTime)
          .ToArrayAsync());

        // var progress = 0;
        // StepProgress(0, horses.Length);

        foreach (var horse in horses)
        {
          var history = allHorseHistories.Where((h) => h.Horse.Name == horse.Name).ToArray();
          var rawData = LearningData.Create(
            race,
            horse,
            history.Select((h) => (h.Race, h.Horse)),
            allHorseHistories.Select((h) => (h.Race, h.Horse)));
          var raw = rawData.ToArray();

          var shape = LearningData.GetShape();
          var dataSet = new float[1, shape];
          for (var i = 0; i < shape; i++)
          {
            dataSet[0, i] = raw[i];
          }
          list.Add((dataSet, horse));

          // StepProgress(++progress, horses.Length);
        }
      }

      return list;
    }

    static void SimulateRaceResults(string parameter)
    {
      var date = parameter.Split("-");
      if (date.Length != 2)
      {
        Console.WriteLine("日付の形式が不正です");
        Task.Delay(3000).Wait();
        return;
      }

      DateTime startTime;
      DateTime endTime;
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
      try
      {
        endTime = DateTime.ParseExact(date[1], "yyyyMMdd", null, System.Globalization.DateTimeStyles.None);
        endTime = endTime.AddDays(1);
      }
      catch
      {
        Console.WriteLine("日付の形式が不正です");
        Task.Delay(3000).Wait();
        return;
      }

      SimulateRaceResults(startTime, endTime);
    }

    static void SimulateRaceResults(DateTime from, DateTime to)
    {
      var allCount = 0;
      var hit = 0;
      var hitAll = 0;
      var pay = 0;
      var income = 0;
      var data = new List<(List<(float[,] DataSet, RaceHorseData Horse)> DataSets, int HorsesCount, RefundData Refund)>();

      Expression <Func<RaceData, bool>> subject =
        mode == "地方" ? (r) => (short)r.Course >= 30 :
                          (r) => (short)r.Course < 30;

      Console.WriteLine("データベースから取得中...");
      var a = 0;
      var racesCount = 0;

      {
        var splits = 8;
        var seconds = (to - from).TotalSeconds / splits;
        var d1 = from;
        var d2 = from.AddSeconds(seconds);
        var tasks = new List<Task>();
        for (var m = 0; m < splits; m++)
        {
          tasks.Add(Task.Run(() =>
          {
            using (var db = new MyContext())
            {
              var races = db.Races!
                .Where((r) => r.StartTime >= d1 && r.StartTime < d2)
                .Join(db.Refunds!, (r) => r.Key, (r) => r.RaceKey, (r, rr) => new { Race = r, Refund = rr, })
                .Select((r) => new { r.Race.Key, r.Race.HorsesCount, r.Refund, }).ToArrayAsync().Result;
              racesCount += races.Length;

              foreach (var race in races)
              {
                var dataSets = GetPredictDataAsync(db, race.Key).Result;
                data.Add((dataSets, race.HorsesCount, race.Refund));

                Interlocked.Increment(ref a);
              }
            }
          }));
          d1 = d1.AddSeconds(seconds);
          d2 = d2.AddSeconds(seconds);
        }
        while (!tasks.All((t) => t.IsCompleted))
        {
          StepProgress(a, racesCount);
          Task.Delay(1000).Wait();
        }
      }

      racesCount = data.Count;
      a = 0;
      foreach (var dataSet in data)
      {
        var rs = new List<(float Prediction, RaceHorseData Horse)>();
        var dd = new float[dataSet.DataSets.Count, dataSet.DataSets.FirstOrDefault().DataSet?.GetLength(1) ?? 0];
        var i = 0;
        foreach (var d in dataSet.DataSets)
        {
          for (var j = 0; j < d.DataSet.GetLength(1); j++)
          {
            dd[i, j] = d.DataSet[0, j];
          }
          i++;
        }
        var prediction = ai!.Predict(dd);
        i = 0;
        foreach (var d in dataSet.DataSets)
        {
          rs.Add((prediction[i], d.Horse));
          i++;
        }
        var needOrder = (dataSet.HorsesCount > 7 ? 3 : 2);
        var hits = rs.OrderByDescending((r) => r.Prediction).Take(3).Count((r) => r.Horse.ResultOrder <= needOrder);
        allCount += needOrder;
        hit += hits;
        if (needOrder == hits)
        {
          hitAll++;
        }

        try
        {
          var buyItems = Buyer.Select(rs.Select((d) => new BuyCandidate { Horse = d.Horse, Prediction = d.Prediction, }));
          var buyResult = Buyer.Buy(dataSet.Refund, buyItems);
          pay += buyResult.Pay;
          income += buyResult.Income;
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
          Console.WriteLine(ex.StackTrace);
          Console.ReadLine();
          return;
        }

        StepProgress(++a, racesCount);
      }

      Console.WriteLine();
      Console.WriteLine();
      Console.WriteLine();
      Console.WriteLine($"        的中率: {hit} / {allCount} = {((float)hit / Math.Max(1, allCount)) * 100} %");
      Console.WriteLine($"複勝全員的中率: {hitAll} / {allCount} = {((float)hitAll / Math.Max(1, allCount)) * 100} %");
      Console.WriteLine($"        回収率: {income} / {pay} = {((float)income / Math.Max(1, pay)) * 100} %");
      Console.ReadLine();
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
