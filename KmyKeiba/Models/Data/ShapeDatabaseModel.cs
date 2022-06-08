using KmyKeiba.Data.Db;
using KmyKeiba.Data.Wrappers;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Analysis.Math;
using KmyKeiba.Models.Connection;
using KmyKeiba.Models.Race;
using KmyKeiba.Shared;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
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
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

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
      var mmlName = Constrants.RunningStyleTrainingFilePath;

      if (!isForce)
      {
        if (File.Exists(mmlName) && File.GetLastWriteTime(mmlName) > DateTime.Now.AddHours(-48))
        {
          return;
        }
      }

      var rs = new PredictRunningStyleModel();

      logger.Debug("トレーニング中...");
      rs.Training();

      logger.Debug("保存中...");
      rs.SaveFile(mmlName);
    }

    public static void StartRunningStylePredicting()
    {
      var mmlName = Constrants.RunningStyleTrainingFilePath;

      if (!File.Exists(mmlName))
      {
        return;
      }

      var rs = new PredictRunningStyleModel();

      logger.Debug("ロード中...");
      rs.OpenFile(mmlName);

      logger.Debug("予想中...");
      var done = -1;
      var sum = 0;
      while (done != 0)
      {
        done = rs.PredictAsync(10_0000).Result;
        sum += done;
        logger.Debug($"{sum} 完了");
      }
    }

    public static async Task MakeStandardTimeMasterDataAsync(int startYear, DownloadLink link = DownloadLink.Both, ReactiveProperty<bool>? isCanceled = null)
    {
      if (link == DownloadLink.Both)
      {
        await MakeStandardTimeMasterDataAsync(startYear, DownloadLink.Central);
        await MakeStandardTimeMasterDataAsync(startYear, DownloadLink.Local);
      }

      logger.Debug("基準タイムマスターデータ作成中...");

      try
      {
        using var db = new MyContext();

        await db.TryBeginTransactionAsync();

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

            logger.Debug($"[{year}] {course.GetName()}");

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

            if (isCanceled?.Value == true)
            {
              await db.CommitAsync();
              return;
            }
          }

          // コースごと
          await db.CommitAsync();
        }

        // キャッシュをクリア
        AnalysisUtil.ClearStandardTimeCaches();
      }
      catch (Exception ex)
      {
        logger.Error("基準タイム更新中にエラーが発生", ex);
      }
    }

    public static async Task SetPreviousRaceDaysAsync(DateOnly? date = null, ReactiveProperty<bool>? isCanceled = null, ReactiveProperty<int>? progress = null, ReactiveProperty<int>? progressMax = null)
    {
      var count = 0;
      var prevCommitCount = 0;

      progress ??= new();
      progressMax ??= new();
      progress.Value = progressMax.Value = 0;

      using var db = new MyContext();

      // 通常のクエリは３分まで
      db.Database.SetCommandTimeout(180);
      //await db.BeginTransactionAsync();

      var query = db.RaceHorses!
        .Where(rh => rh.PreviousRaceDays == 0)
        .Where(rh => rh.Key != "0000000000" && rh.Key != "" && rh.RaceKey != "");
      if (date != null)
      {
        var d = date.Value;
        var str = d.ToString("yyyyMMdd");
        query = query.Where(rh => rh.RaceKey.StartsWith(str));
      }
      progressMax.Value = await query.CountAsync();

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
          progress.Value = count;

          await db.SaveChangesAsync();

          if (isCanceled?.Value == true)
          {
            await db.CommitAsync();
            return;
          }

          if (count >= prevCommitCount + 10000)
          {
            await db.CommitAsync();
            prevCommitCount = count;
          }
          logger.Debug($"馬のInterval日数計算完了: {count} / {progressMax.Value}");

          targets = await allTargets.Take(96).ToArrayAsync();
        }
      }
      catch (Exception ex)
      {
        logger.Error("馬データ成型中にエラー", ex);
      }
    }

    public static async Task SetRiderWinRatesAsync(DateOnly? startMonth = null, ReactiveProperty<bool>? isCanceled = null)
    {
      DateOnly month;
      if (startMonth != null)
      {
        month = new DateOnly(startMonth.Value.Year, startMonth.Value.Month, 1);
      }
      else
      {
        month = new DateOnly(1986, 1, 1);
      }

      using var db = new MyContext();

      // 通常のクエリは３分まで
      db.Database.SetCommandTimeout(180);
      await db.TryBeginTransactionAsync();
      //await db.BeginTransactionAsync();

      var query = db.RaceHorses!
        .Where(rh => rh.ResultOrder > 0)
        .Where(rh => rh.Key != "0000000000" && rh.Key != "" && rh.RaceKey != "" && rh.RiderCode != "00000");

      var today = DateOnly.FromDateTime(DateTime.Today);
      var lastMonth = new DateOnly(today.Year, today.Month, 1).AddDays(-1);  // 先月末

      var distances = new short[] { 0, 1000, 1400, 1800, 2200, 2800, 10000, };

      try
      {
        while (month < lastMonth)
        {
          if (month.Month == 1)
          {
            // ２回目以降の処理を高速化するため、まず処理対象を年単位で確認する
            while (month < lastMonth)
            {
              var yearStr = month.ToString("yyyy");
              var yearAllTargets = query
                .Where(rh => rh.RaceKey.StartsWith(yearStr) && !rh.IsContainsRiderWinRate);

              if (yearAllTargets.Any())
              {
                break;
              }

              month = month.AddYears(1);
            }

            if (month >= lastMonth)
            {
              break;
            }
          }

          var monthStr = month.ToString("yyyyMM");

          // 指定したKeyのレースが存在しない場合があるので、Joinで存在確認する
          var allTargets = query
            .Where(rh => rh.RaceKey.StartsWith(monthStr) && !rh.IsContainsRiderWinRate)
            .Join(db.Races!, rh => rh.RaceKey, r => r.Key, (rh, r) => rh);
          var allRiderCodes = allTargets
            .GroupBy(rh => rh.RiderCode)
            .Select(g => g.Key);

          while (allTargets.Any())
          {
            var targets = await allRiderCodes.Take(96).ToArrayAsync();
            var targetHorses = await query
              .Where(rh => rh.RaceKey.StartsWith(monthStr))
              .Where(rh => targets.Contains(rh.RiderCode))
              .Join(db.Races!, rh => rh.RaceKey, r => r.Key, (rh, r) => new { rh.Id, rh.RiderCode, rh.ResultOrder, r.Distance, r.TrackType, r.TrackGround, Horse = rh, })
              .ToArrayAsync();

            foreach (var riderCode in targets)
            {
              var riderGrades = targetHorses
                .Where(rh => rh.RiderCode == riderCode)
                .ToArray();
              var existsMasterData = await db.RiderWinRates!.Where(r => r.Year == month.Year && r.Month == month.Month && r.RiderCode == riderCode).ToArrayAsync();

              for (var di = 0; di < distances.Length - 1; di++)
              {
                var distanceMin = distances[di];
                var distanceMax = distances[di + 1];

                var distanceGrades = riderGrades.Where(r => r.Distance >= distanceMin && r.Distance < distanceMax);
                if (!distanceGrades.Any())
                {
                  continue;
                }

                var data = existsMasterData.FirstOrDefault(r => r.Distance == distanceMin && r.DistanceMax == distanceMax);
                if (data == null)
                {
                  data = new RiderWinRateMasterData
                  {
                    Year = (short)month.Year,
                    Month = (short)month.Month,
                    RiderCode = riderCode,
                    Distance = distanceMin,
                    DistanceMax = distanceMax,
                  };
                  await db.RiderWinRates!.AddAsync(data);
                }

                var grades = distanceGrades.Where(r => r.TrackType == TrackType.Flat && r.TrackGround == TrackGround.Turf);
                data.AllTurfCount = (short)grades.Count();
                data.FirstTurfCount = (short)grades.Count(r => r.ResultOrder == 1);
                data.SecondTurfCount = (short)grades.Count(r => r.ResultOrder == 2);
                data.ThirdTurfCount = (short)grades.Count(r => r.ResultOrder == 3);

                grades = distanceGrades.Where(r => r.TrackType == TrackType.Flat && r.TrackGround == TrackGround.Dirt);
                data.AllDirtCount = (short)grades.Count();
                data.FirstDirtCount = (short)grades.Count(r => r.ResultOrder == 1);
                data.SecondDirtCount = (short)grades.Count(r => r.ResultOrder == 2);
                data.ThirdDirtCount = (short)grades.Count(r => r.ResultOrder == 3);

                grades = distanceGrades.Where(r => r.TrackType == TrackType.Steeplechase && r.TrackGround == TrackGround.Turf);
                data.AllTurfSteepsCount = (short)grades.Count();
                data.FirstTurfSteepsCount = (short)grades.Count(r => r.ResultOrder == 1);
                data.SecondTurfSteepsCount = (short)grades.Count(r => r.ResultOrder == 2);
                data.ThirdTurfSteepsCount = (short)grades.Count(r => r.ResultOrder == 3);

                grades = distanceGrades.Where(r => r.TrackType == TrackType.Steeplechase && r.TrackGround == TrackGround.Dirt);
                data.AllDirtSteepsCount = (short)grades.Count();
                data.FirstDirtSteepsCount = (short)grades.Count(r => r.ResultOrder == 1);
                data.SecondDirtSteepsCount = (short)grades.Count(r => r.ResultOrder == 2);
                data.ThirdDirtSteepsCount = (short)grades.Count(r => r.ResultOrder == 3);
              }

              foreach (var horse in riderGrades)
              {
                horse.Horse.IsContainsRiderWinRate = true;
              }
            }

            await db.SaveChangesAsync();

            if (isCanceled?.Value == true)
            {
              await db.CommitAsync();
              return;
            }
          }

          month = month.AddMonths(1);
          await db.CommitAsync();
        }
      }
      catch (Exception ex)
      {
        logger.Error("騎手勝率計算中にエラー", ex);
      }

      AnalysisUtil.ClearRiderWinRateCaches();
    }

    public static async Task SetRaceSubjectDisplayInfosAsync(DateOnly? startMonth = null, bool isForce = false, ReactiveProperty<bool>? isCanceled = null)
    {
      DateTime month;
      if (startMonth != null)
      {
        month = new DateTime(startMonth.Value.Year, startMonth.Value.Month, 1);
      }
      else
      {
        month = new DateTime(1986, 1, 1);
      }

      using var db = new MyContext();
      await db.TryBeginTransactionAsync();

      var races = db.Races!.Where(r => r.StartTime >= month && r.Course >= RaceCourse.LocalMinValue);
      if (!isForce)
      {
        races = races.Where(r => r.SubjectDisplayInfo == string.Empty);
      }
      races = races.OrderBy(r => r.StartTime);

      var count = 0;

      try
      {
        foreach (var race in races)
        {
          var subject = new RaceSubjectInfo(race);
          race.SubjectDisplayInfo = $"{subject.Subject.DisplayClass}/{subject.Subject.SecondaryClass ?? string.Empty}/{subject.Subject.ClassName}";
          count++;

          if (count > 10000)
          {
            await db.SaveChangesAsync();
            await db.CommitAsync();
            count = 0;

            if (isCanceled?.Value == true)
            {
              return;
            }
          }
        }
        await db.SaveChangesAsync();
        await db.CommitAsync();
      }
      catch (Exception ex)
      {
        logger.Error("レースの条件解析中にエラー", ex);
      }
    }
  }
}
