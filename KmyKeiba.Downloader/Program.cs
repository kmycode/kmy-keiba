﻿using KmyKeiba.Data.Db;
using KmyKeiba.Data.Wrappers;
using KmyKeiba.Downloader.Math;
using KmyKeiba.Downloader.Models;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.JVLink.Wrappers;
using Microsoft.EntityFrameworkCore;

namespace KmyKeiba.Downloader
{
  internal class Program
  {
    [STAThread]
    public static void Main(string[] args)
    {
      // マイグレーション
      Task.Run(async () =>
      {
        using var db = new MyContext();
        db.Database.SetCommandTimeout(1200);
        await db.Database.MigrateAsync();
      }).Wait();

      //StartSetPreviousRaceDays();
      //StartLoad();

      //StartRunningStyleTraining();
      //StartRunningStylePredicting();

      StartMakingStandardTimeMasterData();
    }

    private static void StartLoad()
    {
      var loader = new JVLinkLoader();

      var isLoaded = false;
      Task.Run(async () =>
      {
        await LoadAsync(loader);
        isLoaded = true;

        loader.Dispose();
      });

      while (!isLoaded)
      {
        Console.Write($"\rDWN [{loader.Downloaded.Value} / {loader.DownloadSize.Value}] LD [{loader.Loaded.Value} / {loader.LoadSize.Value}] ENT({loader.LoadEntityCount.Value}) SV [{loader.Saved.Value} / {loader.SaveSize.Value}] PC [{loader.Processed.Value} / {loader.ProcessSize.Value}]");
        Task.Delay(100).Wait();
      }
    }

    private static async Task LoadAsync(JVLinkLoader loader)
    {
      // 2005  2011-
      for (var year = 1990; year <= 2022; year++)
      {
        break;

        Console.WriteLine($"{year} 年");
        await loader.LoadAsync(JVLinkObject.Local,
          // JVLinkDataspec.Race | JVLinkDataspec.Blod | JVLinkDataspec.Diff | JVLinkDataspec.Slop | JVLinkDataspec.Toku,
          // JVLinkDataspec.Race | JVLinkDataspec.Blod | JVLinkDataspec.Diff | JVLinkDataspec.Slop | JVLinkDataspec.Toku,
          JVLinkDataspec.Race,
          JVLinkOpenOption.Setup,
          raceKey: null,
          startTime: new DateTime(year, 1, 1),
          endTime: new DateTime(year + 1, 1, 1),
          // loadSpecs: new string[] { "RA", "SE", "WH", "WE", "AV", "UM", "HN", "JC", "HC", "HR", });
          loadSpecs: new string[] { "O1", "O2", "O3", "O4", "O5", "O6", });
        Console.WriteLine();
        Console.WriteLine();
      }


      var startYear = 2017;
      var startMonth = 1;

      for (var year = startYear; year <= 2022; year++)
      {
        for (var month = 1; month <= 12; month++)
        {
          if (year == startYear && month < startMonth)
          {
            continue;
          }

          var start = new DateTime(year, month, 1);

          var dates = new DateTime[]
          {
            // start, start.AddDays(15),
            // start.AddDays(15), start.AddMonths(1),
            start, start.AddMonths(1),
          };
          for (var i = 0; i < dates.Length / 2; i++)
          {
            //if (year < 2021) continue;
            //if (year == 2021 && month < 10) continue;
            //if (year == 2021 && month == 10 && i < 1) continue;
            // if (year == 2017 && month <= 3) continue;

            Console.WriteLine($"{year} 年 {month} 月 {dates[i * 2].Day} 日");
            await loader.LoadAsync(JVLinkObject.Local,
              // JVLinkDataspec.Race | JVLinkDataspec.Blod | JVLinkDataspec.Diff | JVLinkDataspec.Slop | JVLinkDataspec.Toku,
              JVLinkDataspec.Race,
              JVLinkOpenOption.Setup,
              raceKey: null,
              startTime: dates[i * 2],
              endTime: dates[i * 2 + 1],
              loadSpecs: new string[] { /* "O1", "O2", "O3", */ "O4", "O5", "O6", });
              // loadSpecs: new string[] { "RA", "SE", "WH", "WE", "AV", "UM", "HN", "JC", "HC", "HR", });
            Console.WriteLine();
            Console.WriteLine();
          }
        }
      }
      /*
      await loader.LoadAsync(JVLinkObject.Central,
        JVLinkDataspec.Race | JVLinkDataspec.Blod | JVLinkDataspec.Diff,
        JVLinkOpenOption.Normal,
        raceKey: null,
        startTime: DateTime.Today.AddMonths(-1),
        endTime: null);
      */
    }

    private static void StartSetPreviousRaceDays()
    {
      Task.Run(async () => await SetPreviousRaceDays()).Wait();
    }

    private static async Task SetPreviousRaceDays()
    {
      var count = 0;

      using (var db = new MyContext())
      {
        // 通常のクエリは３分まで
        db.Database.SetCommandTimeout(180);

        var allTargets = db.RaceHorses!
          .Where(rh => rh.PreviousRaceDays == 0)
          .Where(rh => rh.Key != "0000000000")
          .GroupBy(rh => rh.Key)
          .Select(g => g.Key);

        var targets = await allTargets.Take(96).ToArrayAsync();

        while (targets.Any())
        {
          var targetHorses = await db.RaceHorses!.Where(rh => targets.Contains(rh.Key)).ToArrayAsync();

          foreach (var horse in targets)
          {
            var races = targetHorses.Where(rh => rh.Key == horse).OrderBy(rh => rh.RaceKey);
            var beforeRaceDh = DateTime.MinValue;
            var isFirst = true;

            foreach (var race in races)
            {
              var y = race.RaceKey.Substring(0, 4);
              var m = race.RaceKey.Substring(4, 2);
              var d = race.RaceKey.Substring(6, 2);
              int.TryParse(y, out var year);
              int.TryParse(m, out var month);
              int.TryParse(d, out var day);
              var dh = new DateTime(year, month, day);

              if (!isFirst)
              {
                race.PreviousRaceDays = (short)(dh - beforeRaceDh).TotalDays;
              }
              else
              {
                race.PreviousRaceDays = -1;
                isFirst = false;
              }

              beforeRaceDh = dh;
              count++;
            }
          }

          await db.SaveChangesAsync();
          Console.Write($"\r完了: {count}");

          targets = await allTargets.Take(96).ToArrayAsync();
        }
      }
    }

    private static void StartRunningStyleTraining()
    {
      var rs = new PredictRunningStyleModel();

      Console.WriteLine("トレーニング中...");
      rs.Training();

      Console.WriteLine("保存中...");
      rs.SaveFile("runningstyleml.mml");
    }

    private static void StartRunningStylePredicting()
    {
      var rs = new PredictRunningStyleModel();

      Console.WriteLine("ロード中...");
      rs.OpenFile("runningstyleml.mml");

      Console.WriteLine("予想中...");
      var done = -1;
      var sum = 0;
      while (done != 0)
      {
        done = rs.Predict(10_0000);
        sum += done;
        Console.WriteLine($"{sum} 完了");
      }
    }

    private static void StartMakingStandardTimeMasterData()
    {
      Console.WriteLine("マスターデータ作成中...");
      Task.Run(async () =>
      {
        try
        {
          using var db = new MyContext();

          var grounds = new[] { TrackGround.Turf, TrackGround.Dirt, TrackGround.TurfToDirt, };
          var types = new[] { TrackType.Flat, TrackType.Steeplechase, };
          var conditions = new[] { RaceCourseCondition.Unknown, RaceCourseCondition.Standard, RaceCourseCondition.Good, RaceCourseCondition.Soft, RaceCourseCondition.Yielding, };
          var distances = new[] { 0, 1000, 1400, 1800, 2200, 2800, 10000, };

          for (var i = 2; i < 99; i++)
          {
            var course = (RaceCourse)i;
            if (string.IsNullOrEmpty(course.GetName()))
            {
              continue;
            }

            for (var year = 1990; year < 2022 - 1; year++)
            {
              var totalSampels = 0;

              var startTime = new DateTime(year, 1, 1);
              var endTime = new DateTime(year + 2, 1, 1);
              
              // 中央競馬のデータに地方重賞出場する地方馬の過去成績がいくつか紛れてる
              // それが１つでもあると、中央からしばらく後に地方のデータ取得した後に支障が発生したりする
              // 重複集計をなくすには、このメソッドの引数に範囲となる年を指定するのがいいかも
              // if (await db.RaceStandardTimes!.AnyAsync(rt => rt.Course == course && rt.SampleStartTime == startTime && rt.SampleEndTime == endTime))
              // {
              //   continue;
              // }

              foreach (var ground in grounds)
              {
                foreach (var type in types)
                {
                  foreach (var condition in conditions)
                  {
                    for (var di = 0; di < distances.Length - 1; di++)
                    {
                      // 障害に距離は必要ない
                      if (di != 0 && type == TrackType.Steeplechase)
                      {
                        continue;
                      }

                      var distanceMin = distances[di];
                      var distanceMax = distances[di + 1];
                      Console.WriteLine($"[{year}] {course.GetName()} {ground} {type} {condition} {distanceMin}m");

                      var timesQuery = db.Races!
                        .Where(r => r.TrackGround == ground && r.TrackType == type &&
                                    r.Course == course && r.StartTime >= startTime && r.StartTime < endTime);
                      if (condition != RaceCourseCondition.Unknown)
                      {
                        timesQuery = timesQuery.Where(r => r.TrackCondition == condition);
                      }
                      if (type != TrackType.Steeplechase)
                      {
                        timesQuery = timesQuery.Where(r => r.Distance >= distanceMin && r.Distance < distanceMax);
                      }
                      
                      var times = await timesQuery.Join(db.RaceHorses!.Where(rh => rh.ResultOrder == 1), r => r.Key, rh => rh.RaceKey, (r, rh) => new { rh.ResultTime, r.Distance })
                        .ToArrayAsync();
                      var arr = times.Where(t => t.Distance > 0).Select(t => t.ResultTime.TotalSeconds / t.Distance).ToArray();
                      var statistic = new StatisticSingleArray
                      {
                        Values = arr,
                      };

                      var data = new RaceStandardTimeMasterData
                      {
                        Course = course,
                        Ground = ground,
                        TrackType = type,
                        Condition = condition,
                        SampleStartTime = startTime,
                        SampleEndTime = endTime,
                        SampleCount = times.Length,
                        Distance = (short)distanceMin,
                        DistanceMax = (short)distanceMax,
                        Average = statistic.Average,
                        Median = statistic.Median,
                        Deviation = statistic.Deviation,
                      };
                      await db.RaceStandardTimes!.AddAsync(data);

                      totalSampels += data.SampleCount;
                      if (totalSampels >= 10000)
                      {
                        await db.SaveChangesAsync();
                        totalSampels = 0;
                      }
                    }
                  }
                }
              }

              await db.SaveChangesAsync();
            }
          }
        }
        catch
        {

        }
      }).Wait();
    }
  }
}