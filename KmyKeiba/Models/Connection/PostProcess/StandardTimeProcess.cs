using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Data.Wrappers;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis.Math;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace KmyKeiba.Models.Connection.PostProcess
{
  public class StandardTimeProcess : IPostProcessing
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public ProcessingStep Step => ProcessingStep.StandardTime;

    public async Task RunAsync()
    {
      var state = DownloadStatus.Instance;

      logger.Info($"後処理進捗変更: {Step}");
      await MakeStandardTimeMasterDataAsync(
        1990,
        isCanceled: state.IsCancelProcessing,
        progressMax: state.ProcessingProgressMax,
        progress: state.ProcessingProgress
      );
      await ConfigUtil.SetIntValueAsync(SettingKey.LastUpdateStandardTimeYear, DateTime.Today.Year);
    }

    private static async Task MakeStandardTimeMasterDataAsync(int startYear, ReactiveProperty<bool>? isCanceled = null, ReactiveProperty<int>? progress = null, ReactiveProperty<int>? progressMax = null)
    {
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
  }
}
