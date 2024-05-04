using KmyKeiba.Common;
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
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.WebUI;

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

        var targets2 = db.Horses!.Where(h => h.CentralFlag == 0 && (h.Belongs == HorseBelongs.Ritto || h.Belongs == HorseBelongs.Miho));
        db.Horses!.RemoveRange(targets2);
        await db.SaveChangesAsync();
      }
    }

    public static void TrainRunningStyle(bool isForce = false)
    {
      StartRunningStyleTraining(isForce);
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
      var count = rs.Training();

      // トレーニングデータが一定数に満たなければ、インストーラ付属のファイルを使う
      if (count > 500)
      {
        logger.Debug("保存中...");
        rs.SaveFile(mmlName);
      }
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

    public static async Task MakeStandardTimeMasterDataAsync(int startYear, DownloadLink link = DownloadLink.Both, ReactiveProperty<bool>? isCanceled = null, ReactiveProperty<int>? progress = null, ReactiveProperty<int>? progressMax = null)
    {
      if (link == DownloadLink.Both)
      {
        await MakeStandardTimeMasterDataAsync(startYear, DownloadLink.Central, isCanceled, progress, progressMax);
        await MakeStandardTimeMasterDataAsync(startYear, DownloadLink.Local, isCanceled, progress, progressMax);
      }

      logger.Debug("基準タイムマスターデータ作成中...");

      progress ??= new();
      progressMax ??= new();
      progress.Value = progressMax.Value = 0;

      try
      {
        using var db = new MyContext();

        await db.TryBeginTransactionAsync();

        var grounds = new[] { TrackGround.Turf, TrackGround.Dirt, TrackGround.TurfToDirt, };
        var types = new[] { TrackType.Flat, TrackType.Steeplechase, };
        var conditions = new[] { RaceCourseCondition.Unknown, RaceCourseCondition.Standard, RaceCourseCondition.Good, RaceCourseCondition.Soft, RaceCourseCondition.Yielding, };
        var distances = new[] { 0, 1000, 1400, 1800, 2200, 2800, 10000, };

        progressMax.Value = 98;

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
              .Select((r) => new { r.Key, r.Distance, r.TrackGround, r.TrackType, r.TrackCondition, r.AfterHaronTime3, })
              .ToArrayAsync();
            if (targets.Length == 0)
            {
              continue;
            }

            var keys = targets.Select(r => r.Key).ToArray();
            var allHorses = await db.RaceHorses!
              .Where(rh => keys.Contains(rh.RaceKey) && rh.ResultOrder > 0)
              .Select(rh => new { rh.ResultTime, rh.RaceKey, rh.AfterThirdHalongTime, rh.ResultOrder, rh.ResultTimeValue, rh.AfterThirdHalongTimeValue, })
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

                    var topHorses = times
                      .Select(t => new { t.Race, TopHorses = t.Horses.Where(h => h.ResultOrder >= 1 && h.ResultOrder <= 3), })
                      .Where(t => t.TopHorses.Count() >= 3 && t.TopHorses.Any(h => h.ResultOrder == 1));
                    var pcis = times.SelectMany(t => t.Horses.Select(h => AnalysisUtil.CalcPci(t.Race.Distance, h.ResultTimeValue, h.AfterThirdHalongTimeValue)));
                    var pci3s = topHorses.Select(t => t.TopHorses.Select(h => AnalysisUtil.CalcPci(t.Race.Distance, h.ResultTimeValue, h.AfterThirdHalongTimeValue)).Average());
                    var rpcis = topHorses.Select(t => AnalysisUtil.CalcRpci(t.Race.Distance, t.Race.AfterHaronTime3, t.TopHorses.First(h => h.ResultOrder == 1).ResultTimeValue, t.Race.AfterHaronTime3));
                    var statisticPci = new StatisticSingleArray
                    {
                      Values = pcis.ToArray(),
                    };
                    var statisticPci3 = new StatisticSingleArray
                    {
                      Values = pci3s.ToArray(),
                    };
                    var statisticRpci = new StatisticSingleArray
                    {
                      Values = rpcis.ToArray(),
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
                      data.PciAverage = statisticPci.Average;
                      data.PciMedian = statisticPci.Median;
                      data.PciDeviation = statisticPci.Deviation;
                      data.Pci3Average = statisticPci3.Average;
                      data.Pci3Median = statisticPci3.Median;
                      data.Pci3Deviation = statisticPci3.Deviation;
                      data.RpciAverage = statisticRpci.Average;
                      data.RpciMedian = statisticRpci.Median;
                      data.RpciDeviation = statisticRpci.Deviation;
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
                        PciAverage = statisticPci.Average,
                        PciMedian = statisticPci.Median,
                        PciDeviation = statisticPci.Deviation,
                        Pci3Average = statisticPci3.Average,
                        Pci3Median = statisticPci3.Median,
                        Pci3Deviation = statisticPci3.Deviation,
                        RpciAverage = statisticRpci.Average,
                        RpciMedian = statisticRpci.Median,
                        RpciDeviation = statisticRpci.Deviation,
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

          progress.Value = i;
        }

        // キャッシュをクリア
        AnalysisUtil.ClearStandardTimeCaches();
      }
      catch (Exception ex)
      {
        logger.Error("基準タイム更新中にエラーが発生", ex);
      }
    }

    public static async Task SetHorseExtraDataAsync(DateOnly? date = null, ReactiveProperty<bool>? isCanceled = null, ReactiveProperty<int>? progress = null, ReactiveProperty<int>? progressMax = null)
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
        .Where(rh => rh.PreviousRaceDays == 0 || rh.RaceCount == 0)
        .Where(rh => rh.Key != "0000000000" && rh.Key != "" && rh.RaceKey != "");
      if (date != null)
      {
        var d = date.Value;
        var str = d.ToString("yyyyMMdd");
        query = query.Where(rh => rh.RaceKey.StartsWith(str));
      }

      if (!(await query.AnyAsync()))
      {
        return;
      }

      var allTargets = query
        .Select(g => g.Key);
      progressMax.Value = await allTargets.GroupBy(t => t).CountAsync();
      var targets = await allTargets.Take(1024).ToArrayAsync();
      var skipSize = 0;

      var completedHorses = new Dictionary<string, bool>();

      try
      {
        while (targets.Any())
        {
          targets = targets.GroupBy(k => k).Select(g => g.Key).Where(t => !completedHorses.ContainsKey(t)).ToArray();

          if (targets.Any())
          {
            var targetHorses = await db.RaceHorses!
              .Where(rh => targets.Contains(rh.Key))
              .Select(rh => new { rh.Id, rh.Key, rh.RaceKey, rh.AbnormalResult, })
              .ToArrayAsync();

            foreach (var horse in targets)
            {
              var races = targetHorses.Where(rh => rh.Key == horse).OrderBy(rh => rh.RaceKey);
              var beforeRaceDh = DateTime.MinValue;
              var isFirst = true;
              var isRested = false;

              var raceCount = 1;
              var raceCountWithinRunning = 0;
              var raceCountCompletely = 0;
              var raceCountAfterRest = 1;
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
                if (attach.PreviousRaceDays >= 90)
                {
                  raceCountAfterRest = 1;
                  isRested = true;
                }

                attach.RaceCount = (short)raceCount;

                attach.RaceCountWithinRunning = -2;
                attach.RaceCountWithinRunningCompletely = -2;
                if (race.AbnormalResult == RaceAbnormality.Unknown || race.AbnormalResult > RaceAbnormality.ExcludedByStewards)
                {
                  // とりあえず走った
                  raceCountWithinRunning++;
                  attach.RaceCountWithinRunning = (short)raceCountWithinRunning;

                  if (isRested)
                  {
                    attach.RaceCountAfterLastRest = (short)raceCountAfterRest;
                    raceCountAfterRest++;
                  }
                  else
                  {
                    attach.RaceCountAfterLastRest = -2;
                  }

                  // ちゃんとゴールできた
                  if (race.AbnormalResult == RaceAbnormality.Unknown)
                  {
                    raceCountCompletely++;
                    attach.RaceCountWithinRunningCompletely = (short)raceCountCompletely;
                  }
                }

                beforeRaceDh = dh;
                count++;
                raceCount++;
              }

              completedHorses[horse] = true;
            }
            progress.Value += targets.Length;

            await db.SaveChangesAsync();

            if (isCanceled?.Value == true)
            {
              await db.CommitAsync();
              return;
            }

            if (count >= prevCommitCount + 10000)
            {
              await db.CommitAsync();
              db.ChangeTracker.Clear();
              prevCommitCount = count;
            }
            logger.Debug($"馬のInterval日数計算完了: {count} / {progressMax.Value}");
          }
          else
          {
            skipSize += 1024;
          }

          targets = await allTargets.Skip(skipSize).Take(1024).ToArrayAsync();
        }
      }
      catch (Exception ex)
      {
        logger.Error("馬データ成型中にエラー", ex);
      }
    }

    public static async Task SetHorseExtraTableDataAsync(DateOnly? startDate = null, ReactiveProperty<bool>? isCanceled = null, ReactiveProperty<int>? progress = null, ReactiveProperty<int>? progressMax = null)
    {
      short currentDataVersion = 1;

      try
      {
        using var db = new MyContext();
        var startTime = startDate == null ? DateTime.MinValue : startDate.Value.ToDateTime(TimeOnly.MinValue);

        var horses = db.RaceHorses!
          .Where(rh => rh.ExtraDataVersion < currentDataVersion || (rh.ExtraDataState != HorseExtraDataState.Ignored && rh.ExtraDataState != HorseExtraDataState.AfterRace))
          .Join(db.Races!.Where(r => r.StartTime >= startTime), rh => rh.RaceKey, r => r.Key, (rh, r) => new { RaceHorse = rh, Race = r, })
          .Where(rh => rh.Race.DataStatus <= RaceDataStatus.Canceled)
          .OrderByDescending(rh => rh.Race.StartTime);

        var notyetRaces = horses
          .Where(h => h.RaceHorse.ExtraDataVersion < currentDataVersion || h.RaceHorse.ExtraDataState < HorseExtraDataState.UntilRace)
          .Where(h => h.Race.DataStatus < RaceDataStatus.PreliminaryGradeFull);
        var finishedRaces = horses
          .Where(h => h.RaceHorse.ExtraDataVersion < currentDataVersion || h.RaceHorse.ExtraDataState < HorseExtraDataState.AfterRace)
          .Where(h => h.Race.DataStatus >= RaceDataStatus.PreliminaryGradeFull);

        if (!await notyetRaces.AnyAsync() && !await finishedRaces.AnyAsync())
        {
          return;
        }

        await db.BeginTransactionAsync();

        progress ??= new ReactiveProperty<int>();
        progressMax ??= new ReactiveProperty<int>();
        isCanceled ??= new ReactiveProperty<bool>();

        progress.Value = 0;
        progressMax.Value = await notyetRaces.CountAsync() + await finishedRaces.CountAsync();

        var pciCaches = new Dictionary<string, (short, short)>();

        var lastCommit = 0;
        var state = HorseExtraDataState.UntilRace;
        while (true)
        {
          var buffer = state == HorseExtraDataState.UntilRace ?
            await notyetRaces.Take(96).ToArrayAsync() :
            await finishedRaces.Take(96).ToArrayAsync();
          if (!buffer.Any())
          {
            if (state < HorseExtraDataState.AfterRace)
            {
              state = HorseExtraDataState.AfterRace;
              continue;
            }
            break;
          }

          foreach (var horse in buffer)
          {
            var data = await db.RaceHorseExtras!.FirstOrDefaultAsync(e => e.RaceKey == horse.RaceHorse.RaceKey && e.Key == horse.RaceHorse.Key);
            if (data == null)
            {
              data = new RaceHorseExtraData
              {
                RaceKey = horse.RaceHorse.RaceKey,
                Key = horse.RaceHorse.Key,
              };
              await db.RaceHorseExtras!.AddAsync(data);
            }
            if (horse.RaceHorse.ExtraDataVersion < currentDataVersion)
            {
              horse.RaceHorse.ExtraDataState = HorseExtraDataState.Unset;
            }

            if (horse.RaceHorse.ExtraDataState < HorseExtraDataState.UntilRace)
            {
            }
            if (horse.RaceHorse.ExtraDataState < HorseExtraDataState.AfterRace &&
              horse.Race.DataStatus >= RaceDataStatus.PreliminaryGradeFull)
            {
              if (horse.Race.DataStatus <= RaceDataStatus.Grade)
              {
                var sameRaceHorses = await db.RaceHorses!
                  .Where(rh => rh.RaceKey == horse.Race.Key && rh.ResultOrder > 0)
                  .Select(rh => new { rh.ResultOrder, rh.AfterThirdHalongTimeValue, rh.ResultTimeValue, })
                  .ToArrayAsync();

                // PCI
                try
                {
                  var pci = (short)(AnalysisUtil.CalcPci(horse.Race, horse.RaceHorse) * 100);
                  if (pci == default) pci = -1;
                  data.Pci = pci;
                }
                catch { }

                // PCI3, RPCI
                if (pciCaches.TryGetValue(horse.Race.Key, out var pciCache))
                {
                  data.Pci3 = pciCache.Item1;
                  data.Rpci = pciCache.Item2;
                }
                else
                {
                  double pci3 = default;
                  double rpci = default;

                  if (sameRaceHorses.Any(h => h.ResultOrder <= 3))
                  {
                    pci3 = sameRaceHorses.Where(h => h.ResultOrder <= 3)
                      .Select(h => AnalysisUtil.CalcPci(horse.Race.Distance, h.ResultTimeValue, h.AfterThirdHalongTimeValue) * 100)
                      .Average();
                  }
                  if (sameRaceHorses.Any(h => h.ResultOrder == 1))
                  {
                    var top = sameRaceHorses.First(h => h.ResultOrder == 1);
                    rpci = AnalysisUtil.CalcRpci(horse.Race.Distance, horse.Race.AfterHaronTime3, top.ResultTimeValue, top.AfterThirdHalongTimeValue) * 100;
                  }

                  try
                  {
                    var pci3Value = (short)pci3;
                    var rpciValue = (short)rpci;

                    data.Pci3 = pci3Value;
                    data.Rpci = rpciValue;

                    pciCaches[horse.Race.Key] = (pci3Value, rpciValue);
                  }
                  catch { }
                }

                // 後3ハロンタイム順位
                {
                  var a3hsorted = sameRaceHorses.Where(h => h.AfterThirdHalongTimeValue > 0).OrderBy(h => h.AfterThirdHalongTimeValue);
                  var a3horder = 0;
                  var a3hhit = false;
                  foreach (var item in a3hsorted)
                  {
                    a3horder++;
                    if (item.ResultOrder == horse.RaceHorse.ResultOrder)
                    {
                      a3hhit = true;
                      break;
                    }
                  }
                  if (!a3hhit)
                  {
                    a3horder = -1;
                  }

                  data.After3HaronOrder = (short)a3horder;
                }

                // 調整済前3ハロンタイム
                var b3hFixed = AnalysisUtil.NormalizeB3FTime(horse.Race);
                data.Before3HaronTimeFixed = b3hFixed;

                if (horse.Race.Distance > 600)
                {
                  // ベース部分タイム
                  var baseTime = horse.RaceHorse.ResultTimeValue - horse.RaceHorse.AfterThirdHalongTimeValue;
                  data.BaseTime = (short)baseTime;

                  // ベース部分タイムの3ハロン換算
                  var baseTime3h = (short)((baseTime / (float)(horse.Race.Distance - 600)) * 600);
                  data.BaseTimeAs3Haron = baseTime3h;
                }

                // コーナー順位１２／２３／３４／４結の差
                if (horse.RaceHorse.FirstCornerOrder > 0)
                  data.CornerOrderDiff2 = (short)(horse.RaceHorse.SecondCornerOrder - horse.RaceHorse.FirstCornerOrder);
                if (horse.RaceHorse.SecondCornerOrder > 0)
                  data.CornerOrderDiff3 = (short)(horse.RaceHorse.ThirdCornerOrder - horse.RaceHorse.SecondCornerOrder);
                if (horse.RaceHorse.ThirdCornerOrder > 0)
                  data.CornerOrderDiff4 = (short)(horse.RaceHorse.FourthCornerOrder - horse.RaceHorse.ThirdCornerOrder);
                if (horse.RaceHorse.FourthCornerOrder > 0)
                  data.CornerOrderDiffGoal = (short)(horse.RaceHorse.ResultOrder - horse.RaceHorse.FourthCornerOrder);

                // コーナーで内・中・外に位置した回数
                {
                  var number = horse.RaceHorse.Number;
                  var inside = 0;
                  var center = 0;
                  var outside = 0;
                  var one = 0;
                  foreach (var corner in new string[]
                  {
                    horse.Race.Corner1Result,
                    horse.Race.Corner2Result,
                    horse.Race.Corner3Result,
                    horse.Race.Corner4Result,
                  }.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => RaceCorner.FromString(c)))
                  {
                    var group = corner.Groups.FirstOrDefault(g => g.HorseNumbers.Contains(number));
                    if (group != null)
                    {
                      if (group.HorseNumbers.Count == 1)
                      {
                        one++;
                      }
                      else
                      {
                        var index = group.HorseNumbers.ToList().IndexOf(number);
                        if (index == 0)
                        {
                          inside++;
                        }
                        else if (index == group.HorseNumbers.Count - 1)
                        {
                          outside++;
                        }
                        else
                        {
                          center++;
                        }
                      }
                    }
                  }
                  data.CornerInsideCount = (short)inside;
                  data.CornerMiddleCount = (short)center;
                  data.CornerOutsideCount = (short)outside;
                  data.CornerAloneCount = (short)one;
                }
              }
            }

            if (horse.Race.DataStatus < RaceDataStatus.PreliminaryGradeFull)
            {
              horse.RaceHorse.ExtraDataState = HorseExtraDataState.UntilRace;
            }
            else if (horse.Race.DataStatus <= RaceDataStatus.Grade)
            {
              horse.RaceHorse.ExtraDataState = HorseExtraDataState.AfterRace;
            }
            else
            {
              // 中止、地方、外国など
              horse.RaceHorse.ExtraDataState = HorseExtraDataState.Ignored;
            }
            horse.RaceHorse.ExtraDataVersion = currentDataVersion;
          }

          progress.Value += buffer.Length;

          await db.SaveChangesAsync();
          if (lastCommit + 10000 < progress.Value)
          {
            await db.CommitAsync();
            db.ChangeTracker.Clear();
            lastCommit = progress.Value;
          }

          if (isCanceled.Value)
          {
            await db.CommitAsync();
            return;
          }
        }

        await db.SaveChangesAsync();
        await db.CommitAsync();
      }
      catch (Exception ex)
      {
        logger.Error("拡張データ作成でエラー", ex);
      }
    }

    public static async Task ResetHorseExtraTableDataAsync()
    {
      using var db = new MyContext();
      try
      {
        var targets = db.RaceHorses!.Where(rh => rh.ExtraDataVersion == 0);
        await db.Database.ExecuteSqlRawAsync("UPDATE RaceHorses SET ExtraDataVersion = 0;");
      }
      catch (Exception ex)
      {
        logger.Error("拡張情報リセットでエラー", ex);
      }
    }

    public static async Task SetRiderWinRatesAsync(DateOnly? startMonth = null, ReactiveProperty<bool>? isCanceled = null, ReactiveProperty<int>? progress = null, ReactiveProperty<int>? progressMax = null)
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

      progress ??= new();
      progressMax ??= new();
      progress.Value = progressMax.Value = 0;

      using var db = new MyContext();

      // 通常のクエリは３分まで
      db.Database.SetCommandTimeout(180);
      await db.TryBeginTransactionAsync();
      //await db.BeginTransactionAsync();

      // 指定したKeyのレースが存在しない場合があるので、Joinで存在確認する
      var query = db.RaceHorses!
        .Where(rh => rh.ResultOrder > 0)
        .Where(rh => rh.Key != "0000000000" && rh.Key != "" && rh.RaceKey != "" && rh.RiderCode != "00000")
        .Join(db.Races!, rh => rh.RaceKey, r => r.Key, (rh, r) => rh);

      var today = DateOnly.FromDateTime(DateTime.Today);
      var lastMonth = new DateOnly(today.Year, today.Month, 1).AddDays(-1);  // 先月末。今月のデータはとらない

      // 今月のデータはとらないことに留意して、利用するデータの存在を確認する
      var lastMonthStarts = lastMonth.AddDays(1).ToString("yyyyMM");
      progressMax.Value = await query.CountAsync(rh => !rh.IsContainsRiderWinRate && !rh.RaceKey.StartsWith(lastMonthStarts));
      if (progressMax.Value <= 0)
      {
        return;
      }

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

              if (await yearAllTargets.AnyAsync())
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

          var allTargets = query
            .Where(rh => rh.RaceKey.StartsWith(monthStr) && !rh.IsContainsRiderWinRate);
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

              var changedCount = 0;
              foreach (var horse in riderGrades)
              {
                if (!horse.Horse.IsContainsRiderWinRate)
                {
                  changedCount++;
                  horse.Horse.IsContainsRiderWinRate = true;
                }
              }
              progress.Value += changedCount;
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
          var cls1 = subject.Subject.DisplayClass.ToString()?.ToLower() ?? string.Empty;
          var cls2 = subject.Subject.SecondaryClass?.ToString()?.ToLower() ?? string.Empty;
          race.SubjectDisplayInfo = $"{cls1}/{cls2}/{subject.Subject.ClassName}";
          race.SubjectInfo1 = cls1;
          race.SubjectInfo2 = cls2;
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

    public static async Task MigrateFrom250Async(ReactiveProperty<bool>? isCanceled = null, ReactiveProperty<int>? progress = null, ReactiveProperty<int>? progressMax = null)
    {
      try
      {
        using var db = new MyContext();
        await db.TryBeginTransactionAsync();

        var races = db.Races!.Where(r => r.SubjectDisplayInfo != string.Empty && r.SubjectInfo1 == string.Empty);

        if (!(await races.AnyAsync()))
        {
          return;
        }

        if (progressMax != null)
        {
          progressMax.Value = await races.CountAsync();
        }
        if (progress != null)
        {
          progress.Value = 0;
        }

        var i = 0;
        foreach (var race in races)
        {
          race.SubjectDisplayInfo = string.Empty;
          i++;

          if (i >= 10000)
          {
            await db.SaveChangesAsync();
            await db.CommitAsync();

            if (isCanceled?.Value == true)
            {
              return;
            }

            if (progress != null)
            {
              progress.Value += i;
            }
            i = 0;
          }
        }

        await db.SaveChangesAsync();
        await db.CommitAsync();
      }
      catch (Exception ex)
      {
        logger.Error("2.5.0からのマイグレーション中にエラー", ex);
      }
    }

    public static async Task MigrateFrom322Async(ReactiveProperty<bool>? isCanceled = null, ReactiveProperty<int>? progress = null, ReactiveProperty<int>? progressMax = null)
    {
      using var db = new MyContext();
      await db.TryBeginTransactionAsync();

      var targets = db.Horses!
        .GroupBy(h => h.Code, (h, hs) => new { Key = h, Count = hs.Count(), IsCentral = hs.Any(h => h.CentralFlag != 0), IsLocal = hs.Any(h => h.CentralFlag == 0), })
        .Where(h => h.Count >= 2);

      try
      {
        if (!await targets.AnyAsync())
        {
          return;
        }

        var keys = await targets.Select(t => t.Key).ToArrayAsync();

        if (progressMax != null)
        {
          progressMax.Value = keys.Length;
        }
        if (progress != null)
        {
          progress.Value = 0;
        }

        var count = 0;
        foreach (var key in keys)
        {
          var horses = await db.Horses!.Where(h => h.Code == key).ToArrayAsync();

          HorseData? central = null;
          HorseData? local = null;

          foreach (var horse in horses)
          {
            if (horse.IsCentral)
            {
              if (central == null && horse.Belongs != HorseBelongs.Local)
              {
                central = horse;
              }
              else
              {
                db.Horses!.Remove(horse);
              }
            }
            else if (!horse.IsCentral)
            {
              if (local == null && horse.Belongs == HorseBelongs.Local)
              {
                local = horse;
              }
              else
              {
                db.Horses!.Remove(horse);
              }
            }
          }

          count++;

          if (count >= 100)
          {
            await db.SaveChangesAsync();
            await db.CommitAsync();

            if (progress != null)
            {
              progress.Value += count;
            }
            count = 0;
          }
        }

        await db.SaveChangesAsync();
        await db.CommitAsync();
      }
      catch (Exception ex)
      {
        logger.Error("3.2.2からのマイグレーションでエラー", ex);
      }
    }

    public static async Task MigrateFrom430Async(ReactiveProperty<bool>? isCanceled = null, ReactiveProperty<int>? progress = null, ReactiveProperty<int>? progressMax = null)
    {
      using var db = new MyContext();
      await db.TryBeginTransactionAsync();

      var targets = db.RaceHorseExtras!
        .Where(e => e.Rpci == 0 || e.Pci3 == 0)
        .Join(db.RaceHorses!, e => new { e.Key, e.RaceKey, }, rh => new { rh.Key, rh.RaceKey, }, (e, rh) => new { Extra = e, RaceHorse = rh, })
        .Where(d => d.RaceHorse.ExtraDataVersion >= 1 && d.RaceHorse.ExtraDataState == HorseExtraDataState.AfterRace &&
          (d.RaceHorse.Course <= RaceCourse.CentralMaxValue || d.RaceHorse.Course == RaceCourse.Urawa || d.RaceHorse.Course == RaceCourse.Oi || d.RaceHorse.Course == RaceCourse.Kawazaki || d.RaceHorse.Course == RaceCourse.Funabashi));

      try
      {
        if (!await targets.AnyAsync())
        {
          return;
        }

        var count = 0;

        foreach (var target in targets)
        {
          target.RaceHorse.ExtraDataState = HorseExtraDataState.Unset;
          target.RaceHorse.ExtraDataVersion = 0;

          count++;
          if (count >= 10000)
          {
            await db.SaveChangesAsync();
            await db.CommitAsync();
            db.ChangeTracker.Clear();
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
        logger.Error("4.3.0からのマイグレーションでエラー", ex);
      }
    }

    public static async Task MigrateFrom500Async(ReactiveProperty<bool>? isCanceled = null, ReactiveProperty<int>? progress = null, ReactiveProperty<int>? progressMax = null)
    {
      using var db = new MyContext();
      await db.TryBeginTransactionAsync();

      var count = 0;

      async Task<bool> TrySaveAsync()
      {
        count++;
        if (count >= 10000)
        {
          await db.SaveChangesAsync();
          await db.CommitAsync();
          db.ChangeTracker.Clear();
          count = 0;

          if (isCanceled?.Value == true)
          {
            return false;
          }
        }

        return true;
      }

      try
      {
        foreach (var target in db.Horses!.FromSql($"SELECT * FROM Horses WHERE length(FatherBreedingCode) = 8"))
        {
          target.OwnerCode = $"{target.OwnerCode}00";
          target.FatherBreedingCode = $"{target.FatherBreedingCode.Substring(0, 3)}00{target.FatherBreedingCode.Substring(3, 5)}";
          target.MotherBreedingCode = $"{target.MotherBreedingCode.Substring(0, 3)}00{target.MotherBreedingCode.Substring(3, 5)}";
          target.FFBreedingCode = $"{target.FFBreedingCode.Substring(0, 3)}00{target.FFBreedingCode.Substring(3, 5)}";
          target.FMBreedingCode = $"{target.FMBreedingCode.Substring(0, 3)}00{target.FMBreedingCode.Substring(3, 5)}";
          target.FFFBreedingCode = $"{target.FFFBreedingCode.Substring(0, 3)}00{target.FFFBreedingCode.Substring(3, 5)}";
          target.FFMBreedingCode = $"{target.FFMBreedingCode.Substring(0, 3)}00{target.FFMBreedingCode.Substring(3, 5)}";
          target.FMFBreedingCode = $"{target.FMFBreedingCode.Substring(0, 3)}00{target.FMFBreedingCode.Substring(3, 5)}";
          target.FMMBreedingCode = $"{target.FMMBreedingCode.Substring(0, 3)}00{target.FMMBreedingCode.Substring(3, 5)}";
          target.MFFBreedingCode = $"{target.MFFBreedingCode.Substring(0, 3)}00{target.MFFBreedingCode.Substring(3, 5)}";
          target.MFMBreedingCode = $"{target.MFMBreedingCode.Substring(0, 3)}00{target.MFMBreedingCode.Substring(3, 5)}";
          target.MMFBreedingCode = $"{target.MMFBreedingCode.Substring(0, 3)}00{target.MMFBreedingCode.Substring(3, 5)}";
          target.MMMBreedingCode = $"{target.MMMBreedingCode.Substring(0, 3)}00{target.MMMBreedingCode.Substring(3, 5)}";

          if (!await TrySaveAsync())
          {
            return;
          }
        }

        foreach (var target in db.HorseBloods!.FromSql($"SELECT * FROM HorseBloods WHERE length(Key) = 8"))
        {
          target.Key = $"{target.Key.Substring(0, 3)}00{target.Key.Substring(3, 5)}";

          if (!await TrySaveAsync())
          {
            return;
          }
        }

        foreach (var target in db.HorseBloodInfos!.FromSql($"SELECT * FROM HorseBloodInfos WHERE length(Key) = 8"))
        {
          target.Key = $"{target.Key.Substring(0, 3)}00{target.Key.Substring(3, 5)}";

          if (!await TrySaveAsync())
          {
            return;
          }
        }

        await db.SaveChangesAsync();
        await db.CommitAsync();
      }
      catch (Exception ex)
      {
        logger.Error("4.5.3からのマイグレーションでエラー", ex);
      }
    }
  }
}
