using KmyKeiba.Data.Db;
using KmyKeiba.Data.Wrappers;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Analysis.Math;
using KmyKeiba.Models.Connection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Data
{
  internal class ShapeDatabaseModel
  {
    // TODO: Console.Write to logs

    public static async Task RemoveInvalidDataAsync()
    {
      using var db = new MyContext();

      {
        var targets = db.RaceHorses!.Where(rh => rh.RaceKey == "" || rh.Key == "");
        db.RaceHorses!.RemoveRange(targets);
        await db.SaveChangesAsync();
      }
    }

    public static void TrainRunningStyle(bool isForce = false)
    {
      StartRunningStyleTraining(isForce);
      StartRunningStylePredicting();
    }

    public static void StartRunningStyleTraining(bool isForce = false)
    {
      if (!isForce)
      {
        if (File.Exists("runningstyleml.mml") && File.GetLastWriteTime("runningstyleml.mml") > DateTime.Now.AddHours(-48))
        {
          return;
        }
      }

      var rs = new PredictRunningStyleModel();

      Console.WriteLine("トレーニング中...");
      rs.Training();

      Console.WriteLine("保存中...");
      rs.SaveFile("runningstyleml.mml");
    }

    public static void StartRunningStylePredicting()
    {
      if (!File.Exists("runningstyleml.mml"))
      {
        return;
      }

      var rs = new PredictRunningStyleModel();

      Console.WriteLine("ロード中...");
      rs.OpenFile("runningstyleml.mml");

      Console.WriteLine("予想中...");
      var done = -1;
      var sum = 0;
      while (done != 0)
      {
        done = rs.PredictAsync(10_0000).Result;
        sum += done;
        Console.WriteLine($"{sum} 完了");
      }
    }

    public static async Task MakeStandardTimeMasterDataAsync(int startYear, DownloadLink link = DownloadLink.Both)
    {
      if (link == DownloadLink.Both)
      {
        await MakeStandardTimeMasterDataAsync(startYear, DownloadLink.Central);
        await MakeStandardTimeMasterDataAsync(startYear, DownloadLink.Local);
      }

      Console.WriteLine("マスターデータ作成中...");

      try
      {
        using var db = new MyContext();

        var tryCount = 0;
        var isSucceed = false;
        while (!isSucceed)
        {
          try
          {
            await db.BeginTransactionAsync();
            isSucceed = true;
          }
          catch (SqliteException ex) when (ex.SqliteErrorCode == 5)
          {
            // TODO: logs
            tryCount++;
            if (tryCount >= 30 * 60)
            {
              throw new Exception(ex.Message, ex);
            }
            await Task.Delay(1000);
          }
        }

        var grounds = new[] { TrackGround.Turf, TrackGround.Dirt, TrackGround.TurfToDirt, };
        var types = new[] { TrackType.Flat, TrackType.Steeplechase, };
        var conditions = new[] { RaceCourseCondition.Unknown, RaceCourseCondition.Standard, RaceCourseCondition.Good, RaceCourseCondition.Soft, RaceCourseCondition.Yielding, };
        var distances = new[] { 0, 1000, 1400, 1800, 2200, 2800, 10000, };

        for (var i = 1; i < 99; i++)
        {
          var course = (RaceCourse)i;
          if (string.IsNullOrEmpty(course.GetName()))
          {
            continue;
          }
          if (link.HasFlag(DownloadLink.Central))
          {
            if (course > RaceCourse.CentralMaxValue)
            {
              continue;
            }
          }
          if (link.HasFlag(DownloadLink.Local))
          {
            if (course < RaceCourse.LocalMinValue)
            {
              continue;
            }
          }

          for (var year = startYear; year < DateTime.Today.Year - 1; year++)
          {
            var totalSampels = 0;

            var startTime = new DateTime(year, 1, 1);
            var endTime = new DateTime(year + 2, 1, 1);

            Console.WriteLine($"[{year}] {course.GetName()}");

            // 中央競馬のデータに地方重賞出場する地方馬の過去成績がいくつか紛れてる
            // それが１つでもあると、中央からしばらく後に地方のデータ取得した後に支障が発生したりする
            // 重複集計をなくすには、このメソッドの引数に範囲となる年を指定するのがいいかも
            // if (await db.RaceStandardTimes!.AnyAsync(rt => rt.Course == course && rt.SampleStartTime == startTime && rt.SampleEndTime == endTime))
            // {
            //   continue;
            // }

            var targets = await db.Races!
              .Where(r => r.Course == course && r.StartTime >= startTime && r.StartTime < endTime && r.Distance > 0)
              //.Join(db.RaceHorses!.Where(rh => rh.ResultOrder == 1), r => r.Key, rh => rh.RaceKey, (r, rh) => new { rh.ResultTime, r.Distance, rh.AfterThirdHalongTime, r.TrackGround, r.TrackType, r.TrackCondition, })
              .Select((r) => new { r.Key, r.Distance, r.TrackGround, r.TrackType, r.TrackCondition, })
              .ToArrayAsync();
            if (targets.Length == 0)
            {
              continue;
            }

            var keys = targets.Select(r => r.Key).ToArray();
            var allHorses = await db.RaceHorses!
              .Where(rh => keys.Contains(rh.RaceKey) && rh.ResultOrder > 0)
              .Select(rh => new { rh.ResultTime, rh.RaceKey, rh.AfterThirdHalongTime, })
              .ToArrayAsync();

            var exists = await db.RaceStandardTimes!
              .Where(st => st.Course == course && st.SampleStartTime == startTime && st.SampleEndTime == endTime)
              .ToArrayAsync();

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

                    var timesQuery = targets
                      .Where(r => r.TrackGround == ground && r.TrackType == type);
                    if (condition != RaceCourseCondition.Unknown)
                    {
                      timesQuery = timesQuery.Where(r => r.TrackCondition == condition);
                    }
                    if (type != TrackType.Steeplechase)
                    {
                      timesQuery = timesQuery.Where(r => r.Distance >= distanceMin && r.Distance < distanceMax);
                    }

                    var times = timesQuery
                      .GroupJoin(allHorses, t => t.Key, h => h.RaceKey, (t, hs) => new { Race = t, Horses = hs, })
                      .ToArray();

                    var arr = times.SelectMany(t => t.Horses.Select(h => h.ResultTime.TotalSeconds / t.Race.Distance)).ToArray();
                    var arr2 = times.SelectMany(t => t.Horses.Select(h => h.AfterThirdHalongTime.TotalSeconds)).ToArray();
                    var arr3 = times.Where(t => t.Race.Distance >= 800).SelectMany(t => t.Horses
                      .Select(h => (h.ResultTime.TotalSeconds - h.AfterThirdHalongTime.TotalSeconds) / (t.Race.Distance - 600))).ToArray();
                    var statistic = new StatisticSingleArray
                    {
                      Values = arr,
                    };
                    var statistic2 = new StatisticSingleArray
                    {
                      Values = arr2,
                    };
                    var statistic3 = new StatisticSingleArray
                    {
                      Values = arr3,
                    };

                    RaceStandardTimeMasterData data;

                    var old = exists.FirstOrDefault(st => st.Ground == ground &&
                      st.TrackType == type && st.Condition == condition &&
                      st.Distance == (short)distanceMin && st.DistanceMax == (short)distanceMax);
                    if (old != null)
                    {
                      data = old;
                      data.SampleCount = times.Length;
                      data.Average = statistic.Average;
                      data.Median = statistic.Median;
                      data.Deviation = statistic.Deviation;
                      data.A3FAverage = statistic2.Average;
                      data.A3FMedian = statistic2.Median;
                      data.A3FDeviation = statistic2.Deviation;
                      data.UntilA3FAverage = statistic3.Average;
                      data.UntilA3FMedian = statistic3.Median;
                      data.UntilA3FDeviation = statistic3.Deviation;
                    }
                    else
                    {
                      data = new RaceStandardTimeMasterData
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
                        A3FAverage = statistic2.Average,
                        A3FMedian = statistic2.Median,
                        A3FDeviation = statistic2.Deviation,
                        UntilA3FAverage = statistic3.Average,
                        UntilA3FMedian = statistic3.Median,
                        UntilA3FDeviation = statistic3.Deviation,
                      };
                      await db.RaceStandardTimes!.AddAsync(data);
                    }

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

            // 毎年
            await db.SaveChangesAsync();
          }

          // コースごと
          await db.CommitAsync();
        }

        // キャッシュをクリア
        AnalysisUtil.ClearStandardTimeCaches();
      }
      catch
      {
        // TODO: logs
      }
    }

    public static async Task SetPreviousRaceDaysAsync(DateOnly? date = null)
    {
      var count = 0;
      var prevCommitCount = 0;

      using var db = new MyContext();

      // 通常のクエリは３分まで
      db.Database.SetCommandTimeout(180);
      await db.BeginTransactionAsync();

      var query = db.RaceHorses!
        .Where(rh => rh.PreviousRaceDays == 0)
        .Where(rh => rh.Key != "0000000000" && rh.Key != "" && rh.RaceKey != "");
      if (date != null)
      {
        var d = date.Value;
        var str = d.ToString("yyyyMMdd");
        query = query.Where(rh => rh.RaceKey.StartsWith(str));
      }

      var allTargets = query
        .GroupBy(rh => rh.Key)
        .Select(g => g.Key);

      var targets = await allTargets.Take(96).ToArrayAsync();

      try
      {
        while (targets.Any())
        {
          var targetHorses = await db.RaceHorses!
            .Where(rh => targets.Contains(rh.Key))
            .Select(rh => new { rh.Id, rh.Key, rh.RaceKey, })
            .ToArrayAsync();

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
              _ = int.TryParse(y, out var year);
              _ = int.TryParse(m, out var month);
              _ = int.TryParse(d, out var day);
              var dh = new DateTime(year, month, day);

              var attach = new RaceHorseData { Id = race.Id, };
              db.RaceHorses!.Attach(attach);
              if (!isFirst)
              {
                attach.PreviousRaceDays = System.Math.Max((short)(dh - beforeRaceDh).TotalDays, (short)1);
              }
              else
              {
                attach.PreviousRaceDays = -1;
                isFirst = false;
              }

              beforeRaceDh = dh;
              count++;
            }
          }

          await db.SaveChangesAsync();
          if (count >= prevCommitCount + 10000)
          {
            await db.CommitAsync();
            prevCommitCount = count;
          }
          Console.Write($"\r完了: {count}");

          targets = await allTargets.Take(96).ToArrayAsync();
        }
      }
      catch
      {
        // TODO: logs
      }
    }
  }
}
