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
using System.Diagnostics.CodeAnalysis;
using KmyKeiba.Data.Wrappers;

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
    const int MAX_RACE_ORDER = 12;

    static Program()
    {
      link = JVLinkObject.Central;
      mode = "中央";
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
  dl <training|simulate|cache> <yyyymmdd-yyyymmdd>
  dl <retraining|resimulate>
  dl config <epochs|batch_size|loss|isreguressor> <value>
    ep:{ai!.Epochs}  ba:{ai!.BatchSize}
    lo:{ai!.Loss}  is:{ai!.IsKerasReguressor}
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
          case "updateforce":
          case "updatetoday":
          case "updateyearodds":
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
                  if (parameters.Length < 3)
                  {
                    Console.WriteLine("パラメータが不正です");
                    Task.Delay(3000).Wait();
                  }
                  else
                  {
                    SimulateRaceResults(parameters[2], parameters.Length >= 4 ? parameters[3] : string.Empty);
                  }
                  break;
                case "cache":
                case "training":
                  if (parameters.Length != 3)
                  {
                    Console.WriteLine("パラメータが不正です");
                    Task.Delay(3000).Wait();
                  }
                  else
                  {
                    if (parameters[1] == "cache")
                    {
                      Learning(parameters[2], true);
                    }
                    else
                    {
                      Learning(parameters[2], false);
                    }
                  }
                  break;
                case "reset":
                  ai!.Reset();
                  break;
                case "retraining":
                  ReLearning();
                  break;
                case "resimulate":
                  ResimulateRaceResults();
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
          case "simulate":
            if (parameters.Length <= 1)
            {
              SimulateAsukaPointResults("20201101-20201130");
            }
            else
            {
              SimulateAsukaPointResults(parameters[1]);
            }
            break;
          case "predict":
            if (parameters.Length <= 1)
            {
              Console.WriteLine("パラメータが不正です");
              Task.Delay(3000).Wait();
            }
            else
            {
              PredictAsukaPointResult(parameters[1]);
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
            Expression<Func<RaceData, bool>> force =
              parameters[0] == "updateforce" ? (r) => true :
              parameters[0] == "updatetoday" ? (r) => true :
              parameters[0] == "updateyearodds" ? (r) => true :
                                               (r) => (short)r.DataStatus < (short)RaceDataStatus.PreliminaryGradeFull;
            var upto = DateTime.Today.AddDays(-3);
            if (parameters[0] == "updatetoday")
            {
              upto = DateTime.Today;
            }
            if (parameters[0] == "updateyearodds")
            {
              upto = DateTime.Today.AddYears(-1);
            }

            planKeys = await db.Races!
              .Where((r) => r.StartTime >= DateTime.Now)
              .Where(force)
              .Where(subject)
              .Select((r) => r.Key)
              .ToArrayAsync();
            startKeys = await db.Races!
              .Where((r) => r.StartTime < DateTime.Now && r.StartTime >= upto)
              .Where(force)
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

          await loader.LoadAsync(link, JVLinkDataspec.RB14, JVLinkOpenOption.RealTime, DateTime.Today.ToString("yyyyMMdd"), null, null);
          if (parameters[0] != "updateyearodds")
          {
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
              await loader.LoadAsync(link, JVLinkDataspec.RB41, JVLinkOpenOption.RealTime, key, null, null);
              StepProgress(++processed, processSize);
            }
          }
          else
          {
            foreach (var key in planKeys.Concat(startKeys))
            {
              await loader.LoadAsync(link, JVLinkDataspec.RB41, JVLinkOpenOption.RealTime, key, null, null);
              processed += 4;
              StepProgress(processed, processSize);
            }
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
                JVLinkDataspec.Race | JVLinkDataspec.Slop,
                day >= DateTime.Today.AddMonths(-1) ? JVLinkOpenOption.Normal : JVLinkOpenOption.Setup,
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
            Console.WriteLine($"[{race.Key}] {race.Course.GetName()} ({(short)race.Course}){race.CourseRaceNumber} {race.TrackWeather} {race.TrackCondition} {race.Name}");
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

    static void Learning(string dateFormat, bool isCache)
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

      Learning(startTime, endTime ?? DateTime.Today, isCache);
    }

    class DataLoader
    {
      public int TotalHorses { get; set; }

      public int Learned { get; set; }

      public float[,] Data { get; set; } = new float[0, 0];

      public float[] Results { get; set; } = new float[0];

      public async Task LearnAsync(DateTime f1, DateTime f2, bool isCache)
      {
        using (var db = new MyContext())
        {
          Expression<Func<RaceData, bool>> subject =
            mode == "地方" ? (r) => (short)r.Course >= 30 :
                              (r) => (short)r.Course < 30;
          Expression<Func<RaceHorseData, bool>> subject2 =
            mode == "地方" ? (r) => (short)r.Course >= 30 :
                              (r) => (short)r.Course < 30;

          var maxOrder = isCache ? 100 : MAX_RACE_ORDER;

          var races = await db.Races!
            .Where((r) => r.StartTime >= f1 && r.StartTime < f2)
            .Where((r) => r.TrackType == TrackType.Flat)
            .Where(subject)
            .Where((r) => r.Course != RaceCourse.ObihiroBannei)
            .ToArrayAsync();
          races = races.Distinct(new RaceComparer()).ToArray();
          var raceKeys = races.Select((r) => r.Key).ToArray();
          var allHorsesCount = await db.RaceHorses!.CountAsync((h) => raceKeys.Contains(h.RaceKey) && h.ResultOrder <= maxOrder && h.ResultOrder != 0);

          var shape = LearningData.GetShape();
          var row = 0;
          var data = new float[allHorsesCount, shape];
          var results = new float[allHorsesCount];
          var newCacheCount = 0;
          this.Data = data;
          this.Results = results;

          this.TotalHorses = allHorsesCount;

          foreach (var race in races)
          {
            var sinceTime = race.StartTime.AddYears(-1);

            var allHorses = await db.RaceHorses!.Where((h) => h.ResultOrder != 0 && h.RaceKey == race.Key).ToArrayAsync();
            var horses = allHorses.Where((h) => h.ResultOrder <= maxOrder);
            var horseNames = horses.Select((h) => h.Name);
            var allHorseHistories = (await db.RaceHorses!
              .Where((h) => horseNames.Contains(h.Name))
              .Join(db.Races!.Where((r) => r.StartTime < race.StartTime && r.StartTime >= sinceTime), (h) => h.RaceKey, (r) => r.Key, (h, r) => new { Horse = h, Race = r, })
              .OrderByDescending((h) => h.Race.StartTime)
              .Take(1000)
              .ToArrayAsync());
            IEnumerable<RaceHorseData> historyOrder = Enumerable.Empty<RaceHorseData>();
            var caches = await db.LearningDataCaches!
              .Where((c) => c.RaceKey == race.Key && c.CacheVersion == LearningData.VERSION).ToArrayAsync();
            if (!horses.All((h) => caches.Any((c) => h.Name == c.HorseName)))
            {
              try
              {
                historyOrder = allHorseHistories
                  .Select((h) => h.Horse)
                  .GroupBy((h) => h.Name)
                  .Select((h) => h.First())
                  .OrderBy((h) => h, new LearningData.HorseHistoryResultComparer(allHorseHistories.Select((hh) => hh.Horse)))
                  .ToArray();
              }
              catch { }

              var rawList = new List<(LearningData Data, RaceHorseData Horse)>();

              foreach (var horse in horses)
              {
                var horseRiders = allHorseHistories
                  .Where((h) => h.Horse.Name == horse.Name)
                  .Select((h) => h.Horse.RiderCode)
                  .ToArray();
                var history = allHorseHistories.Where((h) => h.Horse.Name == horse.Name).ToArray();
                var rawData = await LearningData.CreateAsync(
                  db,
                  race,
                  horse,
                  history.Select((h) => (h.Race, h.Horse)),
                  allHorseHistories.Select((h) => (h.Race, h.Horse)),
                  historyOrder);

                rawList.Add((rawData, horse));

                this.Learned++;
              }

              LearningData.Merge(rawList.Select((r) => r.Data));

              foreach (var rawData in rawList)
              {
                var oldCache = caches.FirstOrDefault((c) => c.HorseName == rawData.Horse.Name && c.CacheVersion == LearningData.VERSION);
                if (oldCache != null)
                {
                  db.LearningDataCaches!.Remove(oldCache);
                }

                var cache = new LearningDataCache
                {
                  RaceKey = race.Key,
                  HorseName = rawData.Horse.Name,
                  CacheVersion = LearningData.VERSION,
                };

                cache.Cache = rawData.Data.ToCacheString();
                await db.LearningDataCaches!.AddAsync(cache);

                if (newCacheCount++ % 100 == 0)
                {
                  await db.SaveChangesAsync();
                }

                float[] raw = rawData.Data.ToArray();
                float result = rawData.Data.Result;

                try
                {
                  for (var i = 0; i < shape; i++)
                  {
                    data[row, i] = raw[i];
                  }
                  results[row] = result;
                  row++;
                }
                catch (Exception ex)
                {
                }
              }
            }
            else
            {
              foreach (var horse in horses)
              {
                var cache = caches.FirstOrDefault((c) => c.HorseName == horse.Name && c.CacheVersion == LearningData.VERSION);
                float[] raw;
                float result;

                if (cache != null)
                {
                  var cacheData = cache.Cache.Split(",");
                  raw = new float[cacheData.Length - 1];
                  for (var i = 0; i < raw.Length; i++)
                  {
                    if (float.TryParse(cacheData[i], out float f))
                    {
                      raw[i] = f;
                    }
                    else if (int.TryParse(cacheData[i], out int ii))
                    {
                      raw[i] = i;
                    }
                    else if (double.TryParse(cacheData[i], out double d))
                    {
                      raw[i] = (float)d;
                    }
                  }

                  float.TryParse(cacheData.Last(), out float ff);
                  result = ff;
                }
                else
                {
                  continue;
                }

                try
                {
                  for (var i = 0; i < shape; i++)
                  {
                    data[row, i] = raw[i];
                  }
                  results[row] = result;
                  row++;
                }
                catch (Exception ex)
                {
                }

                this.Learned++;
              }
            }
          }

          /*
          var races = await db.Races!
            .Where((r) => r.StartTime >= from && r.StartTime < to)
            .Join(db.RaceHorses!, (h) => h.Key, (r) => r.RaceKey, (h, r) => new { Horse = r, Race = h, })
            .Where((d) => d.Horse.ResultOrder != 0);
          */

          await db.SaveChangesAsync();
        }
      }

      private class RaceComparer : IEqualityComparer<RaceData>
      {
        public bool Equals(RaceData? x, RaceData? y)
        {
          if (x != null && y != null)
          {
            return x.Key == y.Key;
          }
          return false;
        }

        public int GetHashCode([DisallowNull] RaceData obj)
        {
          return obj.Key.GetHashCode();
        }
      }
    }

    static void Learning(DateTime from, DateTime to, bool isCache)
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
        tasks.Add(learning.LearnAsync(d1, d2, isCache));
        d1 = d1.AddSeconds(seconds);
        d2 = d2.AddSeconds(seconds);
      }
      while (!tasks.All((t) => t.IsCompleted))
      {
        StepProgress(learnings.Sum((l) => l.Learned), learnings.Sum((l) => l.TotalHorses));
        Task.Delay(1000).Wait();
      }

      if (isCache)
      {
        return;
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
      var tree = new TreeAnalyticsModel(8);

      using (var db = new MyContext())
      {
        if (key.Length == 4)
        {
          int.TryParse(key.Substring(0, 2), out int course);
          int.TryParse(key.Substring(2, 2), out int r);
          var race = db.Races!
            .FirstOrDefault((rr) => rr.StartTime.Date == DateTime.Today && (short)rr.Course == course && rr.CourseRaceNumber == r);
          if (race != null)
          {
            key = race.Key;
          }
        }

        dataSets = GetPredictDataAsync(db, key, false).Result;
      }

      var predicted = new List<(float Prediction, RaceHorseData Horse)>();
      foreach (var dataSet in dataSets)
      {
        var prediction = ai.Predict(dataSet.DataSet);
        predicted.Add((prediction[0], dataSet.Horse));
        tree.AddData(dataSet.DataSet, prediction);
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
        Buyer.UpdateScript();
        var items = Buyer.Select(predicted.Select((p) => new BuyCandidate { Horse = p.Horse, Prediction = p.Prediction, }));
        Console.WriteLine(string.Join("\n", items.Select((i) => i.Text).Where((i) => !string.IsNullOrEmpty(i))));
      }
      catch
      {
        Console.WriteLine("馬券購入処理でエラー発生しました");
      }

      tree.Fit();
      tree.ExportAsDot("predict.dot", LearningData.GetFieldNames());
      Console.ReadLine();
    }

    static async Task<List<(float[,] DataSet, RaceHorseData Horse)>> GetPredictDataAsync(MyContext db, string raceKey, bool isUseCache = true)
    {
      var list = new List<(float[,] DataSet, RaceHorseData Horse)>();
      Expression<Func<RaceHorseData, bool>> subject2 =
        mode == "地方" ? (r) => (short)r.Course >= 30 :
                          (r) => (short)r.Course < 30;

      var race = await db.Races!.FirstOrDefaultAsync((r) => r.Key == raceKey);
      if (race != null)
      {
        var sinceTime = race.StartTime.AddYears(-1);

        var horses = await db.RaceHorses!
          .Where((h) => h.RaceKey == raceKey && h.AbnormalResult == RaceAbnormality.Unknown)
          .OrderBy((h) => h.Number)
          .ToArrayAsync();
        var horseNames = horses.Select((h) => h.Name);
        var allHorseHistories = (await db.RaceHorses!
          .Where((h) => horseNames.Contains(h.Name) && h.ResultOrder != 0)
          .Join(db.Races!.Where((r) => r.StartTime < race.StartTime && r.StartTime >= sinceTime), (h) => h.RaceKey, (r) => r.Key, (h, r) => new { Horse = h, Race = r, })
          .OrderByDescending((h) => h.Race.StartTime)
          .ToArrayAsync());
        IEnumerable<RaceHorseData> historyOrder = Enumerable.Empty<RaceHorseData>();
        var caches = await db.LearningDataCaches!
          .Where((c) => c.RaceKey == race.Key && c.CacheVersion == LearningData.VERSION).ToArrayAsync();
        if (!horses.All((h) => caches.Any((c) => h.Name == c.HorseName)))
        {
          try
          {
            historyOrder = allHorseHistories
              .Select((h) => h.Horse)
              .GroupBy((h) => h.Name)
              .Select((h) => h.First())
              .OrderBy((h) => h, new LearningData.HorseHistoryResultComparer(allHorseHistories.Select((hh) => hh.Horse)))
              .ToArray();
          }
          catch { }

          var rawList = new List<(LearningData Data, RaceHorseData Horse)>();

          foreach (var horse in horses)
          {
            var horseRiders = allHorseHistories
              .Where((h) => h.Horse.Name == horse.Name)
              .Select((h) => h.Horse.RiderCode)
              .ToArray();
            var history = allHorseHistories.Where((h) => h.Horse.Name == horse.Name).ToArray();
            var rawData = await LearningData.CreateAsync(
              db,
              race,
              horse,
              history.Select((h) => (h.Race, h.Horse)),
              allHorseHistories.Select((h) => (h.Race, h.Horse)),
              historyOrder);

            rawList.Add((rawData, horse));
          }

          LearningData.Merge(rawList.Select((r) => r.Data));

          foreach (var rawData in rawList)
          {
            var oldCache = caches.FirstOrDefault((c) => c.HorseName == rawData.Horse.Name && c.CacheVersion == LearningData.VERSION);
            if (oldCache != null)
            {
              db.LearningDataCaches!.Remove(oldCache);
            }

            var cache = new LearningDataCache
            {
              RaceKey = race.Key,
              HorseName = rawData.Horse.Name,
              CacheVersion = LearningData.VERSION,
            };

            cache.Cache = rawData.Data.ToCacheString();
            await db.LearningDataCaches!.AddAsync(cache);

            float[] raw = rawData.Data.ToArray();
            var shape = LearningData.GetShape();
            var dataSet = new float[1, shape];
            for (var i = 0; i < shape; i++)
            {
              dataSet[0, i] = raw[i];
            }

            list.Add((dataSet, rawData.Horse));
          }
        }
        else
        {
          foreach (var horse in horses)
          {
            var cache = caches.FirstOrDefault((c) => c.HorseName == horse.Name && c.CacheVersion == LearningData.VERSION);
            float[] raw;
            float result;

            if (cache != null)
            {
              var cacheData = cache.Cache.Split(",");
              raw = new float[cacheData.Length - 1];
              for (var i = 0; i < raw.Length; i++)
              {
                if (float.TryParse(cacheData[i], out float f))
                {
                  raw[i] = f;
                }
                else if (int.TryParse(cacheData[i], out int ii))
                {
                  raw[i] = i;
                }
                else if (double.TryParse(cacheData[i], out double d))
                {
                  raw[i] = (float)d;
                }
              }

              float.TryParse(cacheData.Last(), out float ff);
              result = ff;
            }
            else
            {
              continue;
            }

            var shape = LearningData.GetShape();
            var dataSet = new float[1, shape];
            for (var i = 0; i < shape; i++)
            {
              dataSet[0, i] = raw[i];
            }

            list.Add((dataSet, horse));
          }
        }
      }

      await db.SaveChangesAsync();
      return list;
    }

    static void SimulateRaceResults(string parameter, string option)
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

      if (option == "s")
      {
        SimulateSpeedValues(startTime, endTime);
      }
      else
      {
        SimulateRaceResults(startTime, endTime, option == "g");
      }
    }

    private static List<(List<(float[,] DataSet, RaceHorseData Horse)> DataSets, int HorsesCount, RefundData Refund)>? simulateDataCache;

    static void SimulateRaceResults(DateTime from, DateTime to, bool isGonly)
    {
      var data = new List<(List<(float[,] DataSet, RaceHorseData Horse)> DataSets, int HorsesCount, RefundData Refund)>();

      Expression<Func<RaceData, bool>> subject =
        mode == "地方" ? (r) => (short)r.Course >= 30 :
                          (r) => (short)r.Course < 30;
      Expression<Func<RaceData, bool>> gonly =
        isGonly ? (r) => r.Grade != RaceGrade.Unknown && r.Grade != RaceGrade.Others :
                  (r) => true;

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
          var dd1 = d1;
          var dd2 = d2;
          tasks.Add(Task.Run(() =>
          {
            using (var db = new MyContext())
            {
              var races = db.Races!
                .Where((r) => r.StartTime >= dd1 && r.StartTime < dd2)
                .Where(subject)
                .Where(gonly)
                .Where((r) => r.Course != RaceCourse.ObihiroBannei)
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

      simulateDataCache = data;

      ResimulateRaceResults();
    }

    private class RaceWinCountData
    {
      public int Popular { get; set; }
      public int[] RacesCount { get; } = new int[12];
      public int[] OrderCount { get; } = new int[12];
    }

    static void ResimulateRaceResults()
    {
      if (simulateDataCache == null)
      {
        return;
      }

      try
      {
        Buyer.UpdateScript();
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        Console.WriteLine(ex.StackTrace);
        Console.ReadLine();
        return;
      }

      var placeCounts = new List<RaceWinCountData>();
      var winCounts = new List<RaceWinCountData>();
      for (var i = 0; i < 12; i++)
      {
        placeCounts.Add(new() { Popular = i + 1, });
        winCounts.Add(new() { Popular = i + 1, });
      }

      var buyTypeResult = new Dictionary<BuyType, (int Pay, int Income, int Unit)>();
      var allCount = 0;
      var hit = 0;
      var hitAll = 0;
      var winHit = 0;
      var secondHit = 0;
      var rentaiHit = 0;
      var topFukuHit = 0;
      var pay = 0;
      var income = 0;

      var data = simulateDataCache;
      var tree = new TreeAnalyticsModel(6);
      var racesCount = data.Count;
      var a = 0;
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
        tree.AddData(dd, prediction);
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

        var top = rs.OrderByDescending((r) => r.Prediction).FirstOrDefault();
        if (top.Horse?.ResultOrder == 1)
        {
          winHit++;
        }
        if (top.Horse?.ResultOrder <= 2)
        {
          rentaiHit++;
        }
        if (top.Horse?.ResultOrder <= needOrder)
        {
          topFukuHit++;
        }

        var second = rs.OrderByDescending((r) => r.Prediction).ElementAtOrDefault(1);
        if (top.Horse?.ResultOrder == 1 || second.Horse?.ResultOrder == 1)
        {
          secondHit++;
        }

        var reals = rs.OrderByDescending((r) => r.Prediction).Select((r, i) => new { PredictedOrder = i + 1, r.Horse.Popular, r.Horse.ResultOrder, });
        for (var j = 0; j < 12 && j < rs.Count; j++) {
          var order = reals.FirstOrDefault((r) => r.PredictedOrder == j + 1);
          var real = placeCounts.FirstOrDefault((w) => w.Popular == order?.Popular);
          var real2 = winCounts.FirstOrDefault((w) => w.Popular == order?.Popular);
          if (real != null && order != null)
          {
            real.RacesCount[j]++;
            if (order.ResultOrder <= needOrder)
            {
              real.OrderCount[j]++;
            }
          }
          if (real2 != null && order != null)
          {
            real2.RacesCount[j]++;
            if (order.ResultOrder <= 1)
            {
              real2.OrderCount[j]++;
            }
          }
        }

        try
        {
          var buyItems = Buyer.Select(rs.Select((d) => new BuyCandidate { Horse = d.Horse, Prediction = d.Prediction, })).ToArray();
          var buyResult = Buyer.Buy(dataSet.Refund, buyItems);
          pay += buyResult.Pay;
          income += buyResult.Income;

          foreach (var buyType in buyItems.GroupBy((i) => i.Type))
          {
            if (buyTypeResult.ContainsKey(buyType.Key))
            {
              var old = buyTypeResult[buyType.Key];
              buyTypeResult[buyType.Key] = (old.Pay + buyType.Sum((i) => i.Pay), old.Income + buyType.Sum((i) => i.Income), old.Unit + buyType.Sum((i) => i.Unit));
            }
            else
            {
              buyTypeResult[buyType.Key] = (buyType.Sum((i) => i.Pay), buyType.Sum((i) => i.Income), buyType.Sum((i) => i.Unit));
            }
          }
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

      tree.Fit();
      tree.ExportAsDot("simulate.dot", LearningData.GetFieldNames());

      Console.WriteLine();
      Console.WriteLine();
      Console.WriteLine();
      foreach (var buy in buyTypeResult)
      {
        Console.WriteLine($"{buy.Key} ({buy.Value.Unit})  : {buy.Value.Income} / {buy.Value.Pay} = {((float)buy.Value.Income / Math.Max(1, buy.Value.Pay) * 100)} %");
      }
      Console.WriteLine();
      Console.WriteLine($"１着的中率          : {winHit} / {racesCount} = {((float)winHit / Math.Max(1, racesCount)) * 100} %");
      Console.WriteLine($"12予想の勝率        : {secondHit} / {racesCount} = {((float)secondHit / Math.Max(1, racesCount)) * 100} %");
      Console.WriteLine($"予想１位連対率      : {rentaiHit} / {racesCount} = {((float)rentaiHit / Math.Max(1, racesCount)) * 100} %");
      Console.WriteLine($"予想１位複勝率      : {topFukuHit} / {racesCount} = {((float)topFukuHit / Math.Max(1, racesCount)) * 100} %");
      Console.WriteLine($"複勝的中率（馬）    : {hit} / {allCount} = {((float)hit / Math.Max(1, allCount)) * 100} %");
      Console.WriteLine($"複勝全員的中率      : {hitAll} / {allCount} = {((float)hitAll / Math.Max(1, allCount)) * 100} %");
      Console.WriteLine($"回収率              : {income} / {pay} = {((float)income / Math.Max(1, pay)) * 100} %");
      Console.WriteLine();
      Console.WriteLine("人気　予想１位勝率　予想２位勝率　予想３位勝率");
      foreach (var real in placeCounts)
      {
        Console.WriteLine($"{real.Popular}　{(string.Join("　", real.OrderCount.Select((_, i) => $"{(float)real.OrderCount[i] / Math.Max(real.RacesCount[i], 1):f3}")))}");
      }
      Console.WriteLine();
      foreach (var real in winCounts)
      {
        Console.WriteLine($"{real.Popular}　{(string.Join("　", real.OrderCount.Select((_, i) => $"{(float)real.OrderCount[i] / Math.Max(real.RacesCount[i], 1):f3}")))}");
      }
      Console.ReadLine();
    }

    static void SimulateAsukaPointResults(string parameter)
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

      SimulateAsukaPointResults(startTime, endTime);
    }

    class AsukaPointLoader
    {
      public int RacesCount { get; private set; }

      public int LoadedCount { get; private set; }

      public List<AsukaPointRace> Results { get; } = new();

      public async Task SimulateAsync(DateTime from, DateTime to)
      {
        using (var db = new MyContext())
        {
          var races = db.Races!
            .Where((r) => r.StartTime >= from && r.StartTime < to);
          if (mode == "地方")
          {
            races = races.Where((r) => (short)r.Course >= 30);
          }
          else
          {
            races = races.Where((r) => (short)r.Course < 30);
          }
          var rs = await races.ToArrayAsync();
          this.RacesCount = rs.Length;

          foreach (var race in rs)
          {
            var points = await AsukaPointRace.CreateAsync(db, race);
            this.Results.Add(points);

            this.LoadedCount++;
          }
        }
      }
    }

    static void SimulateAsukaPointResults(DateTime from, DateTime to)
    {
      try
      {
        Buyer.UpdateScript();
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        Console.WriteLine(ex.StackTrace);
        Console.ReadLine();
        return;
      }

      var placeCounts = new List<RaceWinCountData>();
      var winCounts = new List<RaceWinCountData>();
      for (var i = 0; i < 12; i++)
      {
        placeCounts.Add(new() { Popular = i + 1, });
        winCounts.Add(new() { Popular = i + 1, });
      }

      var buyTypeResult = new Dictionary<BuyType, (int Pay, int Income, int Unit)>();
      var allCount = 0;
      var hit = 0;
      var hitAll = 0;
      var hitWide = 0;
      var winHit = 0;
      var secondHit = 0;
      var rentaiHit = 0;
      var topFukuHit = 0;
      var pay = 0;
      var income = 0;

      var largeDiffCount = 0;
      var largeDiffWinCount = 0;
      var largeDiffAllCount = 0;
      var largeDiffAllWinCount = 0;

      var splits = 8;
      var seconds = (to - from).TotalSeconds / splits;
      var d1 = from;
      var d2 = from.AddSeconds(seconds);
      var tasks = new List<Task>();
      var objs = new List<AsukaPointLoader>();
      for (var m = 0; m < splits; m++)
      {
        var loader = new AsukaPointLoader();
        objs.Add(loader);
        tasks.Add(loader.SimulateAsync(d1, d2));
        d1 = d1.AddSeconds(seconds);
        d2 = d2.AddSeconds(seconds);
      }
      while (!tasks.All((t) => t.IsCompleted))
      {
        StepProgress(objs.Sum((l) => l.LoadedCount), objs.Sum((l) => l.RacesCount));
        Task.Delay(1000).Wait();
      }
      var exs = tasks.Where((t) => t.Exception != null).Select((t) => t.Exception);
      if (exs.Any())
      {
        return;
      }

      var data = objs.SelectMany((o) => o.Results).ToArray();
      var racesCount = data.Count();
      var a = 0;
      foreach (var dataSet in data)
      {
        var needOrder = (dataSet.Race.HorsesCount > 7 ? 3 : 2);
        var hits = dataSet.Horses.OrderByDescending((r) => r.Prediction).Take(3).Count((r) => r.Horse.ResultOrder <= needOrder);
        allCount += needOrder;
        hit += hits;

        var isLargeDiff = false;
        var prediction1 = dataSet.Horses.OrderByDescending((r) => r.Prediction).ElementAtOrDefault(0);
        var prediction2 = dataSet.Horses.OrderByDescending((r) => r.Prediction).ElementAtOrDefault(1);
        var prediction3 = dataSet.Horses.OrderByDescending((r) => r.Prediction).ElementAtOrDefault(2);
        var prediction4 = dataSet.Horses.OrderByDescending((r) => r.Prediction).ElementAtOrDefault(3);
        var prediction6 = dataSet.Horses.OrderByDescending((r) => r.Prediction).ElementAtOrDefault(5);
        if (prediction4 != null)
        {
          if (prediction2!.Prediction / prediction1!.Prediction < 0.85f)
          {
            isLargeDiff = true;
            largeDiffCount++;
            if (prediction1!.Horse.ResultOrder <= needOrder)
            {
              largeDiffWinCount++;
            }
          }
          else if (prediction3!.Prediction / prediction2!.Prediction < 0.88f)
          {
            isLargeDiff = true;
            largeDiffCount++;
            if (prediction1!.Horse.ResultOrder <= needOrder)
            {
              largeDiffWinCount++;
            }
          }
          else if (needOrder >= 3 && prediction4!.Prediction / prediction3!.Prediction < 0.9f)
          {
            isLargeDiff = true;
            largeDiffCount++;
            if (prediction1!.Horse.ResultOrder <= needOrder)
            {
              largeDiffWinCount++;
            }
          }
        }

        var isLargeDiffAll = false;
        if (prediction6 != null && prediction6.Prediction / prediction1!.Prediction < 0.65f)
        {
          isLargeDiffAll = true;
          largeDiffAllCount++;
        }

        if (dataSet.Horses.OrderByDescending((r) => r.Prediction).Take(4).Count((r) => r.Horse.ResultOrder <= 3) == 3)
        {
          hitAll++;
          if (isLargeDiffAll)
          {
            largeDiffAllWinCount++;
          }
        }

        if (dataSet.Horses.OrderByDescending((r) => r.Prediction).Take(4).Count((r) => r.Horse.ResultOrder <= 3) >= 2)
        {
          hitWide++;
        }

        var top = dataSet.Horses.OrderByDescending((r) => r.Prediction).FirstOrDefault();
        if (top?.Horse?.ResultOrder == 1)
        {
          winHit++;
        }
        if (top?.Horse?.ResultOrder <= 2)
        {
          rentaiHit++;
        }
        if (top?.Horse?.ResultOrder <= needOrder)
        {
          topFukuHit++;
        }

        var second = dataSet.Horses.OrderByDescending((r) => r.Prediction).ElementAtOrDefault(1);
        if (top?.Horse?.ResultOrder == 1 || second?.Horse?.ResultOrder == 1)
        {
          secondHit++;
        }

        var reals = dataSet.Horses.OrderByDescending((r) => r.Prediction).Select((r, i) => new { PredictedOrder = i + 1, r.Horse.Popular, r.Horse.ResultOrder, });
        for (var j = 0; j < 12 && j < dataSet.Horses.Count; j++)
        {
          var order = reals.FirstOrDefault((r) => r.PredictedOrder == j + 1);
          var real = placeCounts.FirstOrDefault((w) => w.Popular == order?.Popular);
          var real2 = winCounts.FirstOrDefault((w) => w.Popular == order?.Popular);
          if (real != null && order != null)
          {
            real.RacesCount[j]++;
            if (order.ResultOrder <= needOrder)
            {
              real.OrderCount[j]++;
            }
          }
          if (real2 != null && order != null)
          {
            real2.RacesCount[j]++;
            if (order.ResultOrder <= 1)
            {
              real2.OrderCount[j]++;
            }
          }
        }

        try
        {
          var buyItems = Buyer.Select(dataSet.Horses.Select((d) => new BuyCandidate { Horse = d.Horse, Prediction = d.Prediction, }), isLargeDiff, isLargeDiffAll).ToArray();
          var buyResult = Buyer.Buy(dataSet.Refund ?? new(), buyItems);
          pay += buyResult.Pay;
          income += buyResult.Income;

          foreach (var buyType in buyItems.GroupBy((i) => i.Type))
          {
            if (buyTypeResult.ContainsKey(buyType.Key))
            {
              var old = buyTypeResult[buyType.Key];
              buyTypeResult[buyType.Key] = (old.Pay + buyType.Sum((i) => i.Pay), old.Income + buyType.Sum((i) => i.Income), old.Unit + buyType.Sum((i) => i.Unit));
            }
            else
            {
              buyTypeResult[buyType.Key] = (buyType.Sum((i) => i.Pay), buyType.Sum((i) => i.Income), buyType.Sum((i) => i.Unit));
            }
          }
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
      foreach (var buy in buyTypeResult)
      {
        Console.WriteLine($"{buy.Key} ({buy.Value.Unit})  : {buy.Value.Income} / {buy.Value.Pay} = {((float)buy.Value.Income / Math.Max(1, buy.Value.Pay) * 100)} %");
      }
      Console.WriteLine();
      Console.WriteLine($"１着的中率          : {winHit} / {racesCount} = {((float)winHit / Math.Max(1, racesCount)) * 100} %");
      Console.WriteLine($"12予想の勝率        : {secondHit} / {racesCount} = {((float)secondHit / Math.Max(1, racesCount)) * 100} %");
      Console.WriteLine($"予想１位連対率      : {rentaiHit} / {racesCount} = {((float)rentaiHit / Math.Max(1, racesCount)) * 100} %");
      Console.WriteLine($"予想１位複勝率      : {topFukuHit} / {racesCount} = {((float)topFukuHit / Math.Max(1, racesCount)) * 100} %");
      Console.WriteLine($"予想１位複勝率(大差): {largeDiffWinCount} / {largeDiffCount} = {((float)largeDiffWinCount / Math.Max(1, largeDiffCount)) * 100} %");
      Console.WriteLine($"複勝的中率（馬）    : {hit} / {allCount} = {((float)hit / Math.Max(1, allCount)) * 100} %");
      Console.WriteLine($"３連全員的中率      : {hitAll} / {allCount} = {((float)hitAll / Math.Max(1, allCount)) * 100} %");
      Console.WriteLine($"３連全員的中率(大差): {largeDiffAllWinCount} / {largeDiffAllCount} = {((float)largeDiffAllWinCount / Math.Max(1, largeDiffAllCount)) * 100} %");
      Console.WriteLine($"ワイド的中率        : {hitWide} / {allCount} = {((float)hitWide / Math.Max(1, allCount)) * 100} %");
      Console.WriteLine($"回収率              : {income} / {pay} = {((float)income / Math.Max(1, pay)) * 100} %");
      Console.WriteLine();
      Console.WriteLine("人気　予想１位勝率　予想２位勝率　予想３位勝率");
      foreach (var real in placeCounts)
      {
        Console.WriteLine($"{real.Popular}　{(string.Join("　", real.OrderCount.Select((_, i) => real.RacesCount[i] == 0 ? "-.---" : $"{(float)real.OrderCount[i] / real.RacesCount[i]:f3}")))}");
      }
      Console.WriteLine();
      foreach (var real in winCounts)
      {
        Console.WriteLine($"{real.Popular}　{(string.Join("　", real.OrderCount.Select((_, i) => real.RacesCount[i] == 0 ? "-.---" : $"{(float)real.OrderCount[i] / real.RacesCount[i]:f3}")))}");
      }
      Console.ReadLine();
    }

    static void PredictAsukaPointResult(string key)
    {
      Console.WriteLine();
      Console.WriteLine();
      Console.WriteLine();
      Console.WriteLine("データ読み込み中...");

      AsukaPointRace data;

      using (var db = new MyContext())
      {
        if (key.Length == 4)
        {
          int.TryParse(key.Substring(0, 2), out int course);
          int.TryParse(key.Substring(2, 2), out int r);
          var race = db.Races!
            .FirstOrDefault((rr) => rr.StartTime.Date == DateTime.Today && (short)rr.Course == course && rr.CourseRaceNumber == r);
          if (race != null)
          {
            key = race.Key;
          }
        }

        var rr = db.Races!.FirstOrDefault((r) => r.Key == key);
        if (rr == null)
        {
          Console.WriteLine();
          Console.WriteLine("レースが見つかりませんでした");
          Task.Delay(3000).Wait();
          return;
        }

        data = AsukaPointRace.CreateAsync(db, rr).Result;
      }

      foreach (var p in data.Horses.OrderByDescending((pp) => pp.Prediction))
      {
        Console.Write($"{p.Horse.Number} : {p.Horse.Name}\n");
        Console.Write($"{p.Prediction}\n\n");
      }

      Console.WriteLine();
      Console.WriteLine("--------------------------------");

      try
      {
        Buyer.UpdateScript();
        var items = Buyer.Select(data.Horses.Select((p) => new BuyCandidate { Horse = p.Horse, Prediction = p.Prediction, }));
        Console.WriteLine(string.Join("\n", items.Select((i) => i.Text).Where((i) => !string.IsNullOrEmpty(i))));
      }
      catch
      {
        Console.WriteLine("馬券購入処理でエラー発生しました");
      }

      Console.ReadLine();
    }

    static void SimulateSpeedValues(DateTime from, DateTime to)
    {
      var hit = 0;
      var hit3 = 0;

      Expression<Func<RaceData, bool>> subject =
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
          var dd1 = d1;
          var dd2 = d2;
          tasks.Add(Task.Run(async () =>
          {
            using (var db = new MyContext())
            {
              var races = await db.Races!
                .Where((r) => r.StartTime >= dd1 && r.StartTime < dd2)
                .Where(subject)
                .ToArrayAsync();
              racesCount += races.Length;

              foreach (var race in races)
              {
                var horses = await db.RaceHorses!
                  .Where((h) => h.RaceKey == race.Key && h.ResultOrder != 0)
                  .ToArrayAsync();
                var horseSpeeds = new List<(RaceHorseData Horse, double Speed)>();

                foreach (var horse in horses)
                {
                  var oldRaces = await db.RaceHorses!
                    .Where((h) => h.Name == horse.Name && h.ResultOrder != 0)
                    .Join(db.Races!, (h) => h.RaceKey, (r) => r.Key, (h, r) => new { Horse = h, Race = r, })
                    .Where((r) => r.Race.StartTime < race.StartTime)
                    .OrderByDescending((r) => r.Race.StartTime)
                    .Take(5)
                    .ToArrayAsync();
                  if (oldRaces.Any())
                  {
                    var speedSum = 0f;
                    foreach (var or in oldRaces)
                    {
                      var pastLimit = or.Race.StartTime.AddMonths(-18);
                      var sameCourseInfo = await db.RaceHorses!
                        .Join(db.Races!, (h) => h.RaceKey, (r) => r.Key, (h, r) => new { Horse = h, Race = r, })
                        .Where((r) => r.Race.StartTime < or.Race.StartTime && r.Race.StartTime >= pastLimit)
                        .Where((r) => r.Race.Course == or.Race.Course && r.Race.CourseType == or.Race.CourseType && r.Race.Distance == or.Race.Distance && r.Race.TrackGround == or.Race.TrackGround)
                        .Where((r) => r.Horse.ResultOrder != 0 && r.Horse.ResultOrder <= 3 &&
                          ((short)r.Race.Course >= 30 ||
                            (r.Race.SubjectAge2 == RaceSubjectType.Win3 || r.Race.SubjectAge3 == RaceSubjectType.Win3 || r.Race.SubjectAge4 == RaceSubjectType.Win3 || r.Race.SubjectAge5 == RaceSubjectType.Win3 || r.Race.SubjectAgeYounger == RaceSubjectType.Win3) ||
                            (r.Race.SubjectAge2 == RaceSubjectType.Win2 || r.Race.SubjectAge3 == RaceSubjectType.Win2 || r.Race.SubjectAge4 == RaceSubjectType.Win2 || r.Race.SubjectAge5 == RaceSubjectType.Win2 || r.Race.SubjectAgeYounger == RaceSubjectType.Win2)))
                        .Select((r) => r.Horse.ResultTime)
                        .ToArrayAsync();
                      if (sameCourseInfo.Any())
                      {
                        var sameCourseBaseSpeed = sameCourseInfo.Average((c) => c.TotalSeconds);
                        var thisSpeed = (sameCourseBaseSpeed * 10 - or.Horse.ResultTime.TotalSeconds * 10) *
                          (1 / (sameCourseBaseSpeed * 10) * 1000) + (or.Horse.RiderWeight - 55) * 2 +
                          ((short)or.Race.TrackCondition - 1) * 8 + 80;

                        speedSum += (float)thisSpeed;
                      }
                    }
                    var speed = speedSum / oldRaces.Length;
                    horseSpeeds.Add((horse, speed));
                  }
                }

                if (horseSpeeds.Any())
                {
                  var max = horseSpeeds.OrderByDescending((s) => s.Speed).First();
                  if (max.Horse.ResultOrder == 1)
                  {
                    hit++;
                  }
                  if (max.Horse.ResultOrder <= 3)
                  {
                    hit3++;
                  }
                }

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

      Console.WriteLine();
      Console.WriteLine($"１着的中率          : {hit} / {racesCount} = {((float)hit / Math.Max(1, racesCount)) * 100} %");
      Console.WriteLine($"３着以内的中率      : {hit3} / {racesCount} = {((float)hit3 / Math.Max(1, racesCount)) * 100} %");
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
