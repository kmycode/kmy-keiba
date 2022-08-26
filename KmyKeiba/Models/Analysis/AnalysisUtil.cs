using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis.Math;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis
{
  internal static class AnalysisUtil
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    private static readonly Dictionary<RaceCourse, IReadOnlyList<RaceStandardTimeMasterData>> _standardData = new();
    private static readonly Dictionary<string, IReadOnlyList<RiderWinRateMasterData>> _riderWinRateData = new();

    public static RaceStandardTimeMasterData DefaultStandardTime { get; } = new();

    public static async Task<RaceStandardTimeMasterData> GetRaceStandardTimeAsync(MyContext? db, RaceData race, Dictionary<RaceCourse, IReadOnlyList<RaceStandardTimeMasterData>>? cache = null)
    {
      // logger.Debug($"基準タイム取得 {race.Key}");

      if (db == null && cache == null)
      {
        throw new ArgumentException("db");
      }
      var standardData = cache ?? _standardData;
      standardData.TryGetValue(race.Course, out var list);

      if (list == null)
      {
        if (cache != null || db == null)
        {
          return DefaultStandardTime;
        }
        list = await db.RaceStandardTimes!
          .Where(st => st.Course == race.Course && st.SampleCount > 0)
          .ToArrayAsync();
        standardData[race.Course] = list;
        logger.Info($"基準タイム {race.Course} キャッシュをDBから読み込みました 項目数: {list.Count}");
      }

      var query = list
        .OrderByDescending(st => st.SampleStartTime)
        .Where(st => st.TrackType == race.TrackType && st.SampleEndTime < race.StartTime);

      if (race.TrackType != TrackType.Steeplechase)
      {
        query = query.Where(st => race.Distance >= st.Distance && race.Distance < st.DistanceMax);
      }

      RaceStandardTimeMasterData? item;

      // 翌日のレース予定などではこれが設定されていない
      if (race.TrackCondition != RaceCourseCondition.Unknown)
      {
        try
        {
          item = query.FirstOrDefault(st => st.Condition == race.TrackCondition);

          if (item == null || item.SampleCount < 10)
          {
            item = query.FirstOrDefault(st => st.Condition == RaceCourseCondition.Unknown);
          }
        }
        catch (Exception ex)
        {
          item = null;
        }
      }
      else
      {
        item = query.FirstOrDefault();
      }

      // logger.Debug($"レース {race.Key} 基準タイムのサンプル数: {item?.SampleCount}");
      return item ?? new();
    }

    public static void ClearStandardTimeCaches()
    {
      logger.Info("基準タイムのキャッシュをリセット");
      _standardData.Clear();
    }

    public static async Task<RiderWinRateMasterData> GetRiderWinRateAsync(MyContext db, RaceData race, string riderCode)
    {
      // logger.Debug($"騎手の勝率情報 騎手コード: {riderCode}");

      _riderWinRateData.TryGetValue(riderCode, out var list);

      if (list == null)
      {
        list = await db.RiderWinRates!
          .Where(rw => rw.RiderCode == riderCode)
          .ToArrayAsync();
        _riderWinRateData[riderCode] = list;
        logger.Info($"騎手 {riderCode} 勝率情報キャッシュをDBから読み込みました 項目数: {list.Count}");
      }

      var raceMonth = new DateOnly(race.StartTime.Year, race.StartTime.Month, 1);
      var query = list
        .Select(rw => new { Month = new DateOnly(rw.Year, rw.Month, 1), Data = rw, })
        .Where(rw => rw.Month < raceMonth && rw.Month >= raceMonth.AddYears(-1))
        .Where(rw => race.Distance >= rw.Data.Distance && race.Distance < rw.Data.DistanceMax);

      RiderWinRateMasterData item = query.Aggregate(new RiderWinRateMasterData
      {
        RiderCode = riderCode,
      }, (a, b) =>
      {
        a.AllTurfCount += b.Data.AllTurfCount;
        a.FirstTurfCount += b.Data.FirstDirtCount;
        a.SecondTurfCount += b.Data.SecondTurfCount;
        a.ThirdTurfCount += b.Data.ThirdTurfCount;
        a.AllDirtCount += b.Data.AllDirtCount;
        a.FirstDirtCount += b.Data.FirstDirtCount;
        a.SecondDirtCount += b.Data.SecondDirtCount;
        a.ThirdDirtCount += b.Data.ThirdDirtCount;
        a.AllTurfSteepsCount += b.Data.AllTurfSteepsCount;
        a.FirstTurfSteepsCount += b.Data.FirstDirtSteepsCount;
        a.SecondTurfSteepsCount += b.Data.SecondTurfSteepsCount;
        a.ThirdTurfSteepsCount += b.Data.ThirdTurfSteepsCount;
        a.AllDirtSteepsCount += b.Data.AllDirtSteepsCount;
        a.FirstDirtSteepsCount += b.Data.FirstDirtSteepsCount;
        a.SecondDirtSteepsCount += b.Data.SecondDirtSteepsCount;
        a.ThirdDirtSteepsCount += b.Data.ThirdDirtSteepsCount;
        return a;
      });

      // logger.Info($"騎手 {riderCode} 勝率情報のサンプル数: {item.AllTurfCount} / {item.AllDirtCount} / {item.AllTurfSteepsCount} / {item.AllDirtSteepsCount}");
      return item;
    }

    public static void ClearRiderWinRateCaches()
    {
      logger.Info("騎手勝率情報のキャッシュをリセット");
      _riderWinRateData.Clear();
    }

    public static double CalcRoughRate(IReadOnlyList<RaceHorseData> topHorses)
    {
      // logger.Debug($"レース荒れ度を計算 馬数: {topHorses.Count}");
      return topHorses.Where(rh => rh.ResultOrder >= 1 && rh.ResultOrder <= 3)
            .Select(rh => (double)rh.Popular * rh.Popular)
            .Append(0)    // Sum時の例外防止
            .Sum() / (1 * 1 + 2 * 2 + 3 * 3);
    }

    public static double CalcDisturbanceRate(IEnumerable<RaceHorseAnalyzer> horses)
      => CalcDisturbanceRate(horses.Select(h => (h.Data.ResultOrder, h.Race.HorsesCount)).ToArray());

    public static double CalcDisturbanceRate(IEnumerable<(RaceData Race, RaceHorseData Horse)> horses)
      => CalcDisturbanceRate(horses.Select(h => (h.Horse.ResultOrder, h.Race.HorsesCount)).ToArray());

    public static double CalcDisturbanceRate(IReadOnlyList<(short ResultOrder, short HorsesCount)> data)
    {
      // logger.Debug($"馬の乱調度を計算 馬数: {data.Count()}");
      var statistic = new StatisticSingleArray(data
        .Where(d => d.HorsesCount > 1 && d.ResultOrder >= 1)
        .Select(d => (double)(d.ResultOrder - 1) / (d.HorsesCount - 1))
        .ToArray());
      return statistic.Deviation * 100;
    }

    public static (int min, int max) GetIntervalRange(int interval)
    {
      if (interval <= 0)
      {
        return default;
      }

      int min, max;
      if (interval <= 59)
      {
        min = interval / 7 * 7;
        max = min + 7;
      }
      else
      {
        min = interval / 30 * 30;
        max = min + 30;
      }
      return (min, max);
    }

    public static (int min, int max) GetOddsRange(int odds)
    {
      if (odds <= 0)
      {
        return default;
      }

      int min, max;
      if (odds < 60)
      {
        min = odds / 10 * 10;
        max = min + 10;
      }
      else if (odds < 200)
      {
        min = odds / 20 * 20;
        max = min + 20;
      }
      else if (odds < 1000)
      {
        min = odds / 100 * 100;
        max = min + 100;
      }
      else
      {
        min = odds / 1000 * 1000;
        max = min + 1000;
      }

      return (min, max);
    }

    public static ValueComparation CompareValue(double value, double good, double bad, bool isReverse = false)
    {
      if (isReverse)
      {
        return value <= good ? ValueComparation.Good : value >= bad ? ValueComparation.Bad : ValueComparation.Standard;
      }
      else
      {
        return value >= good ? ValueComparation.Good : value <= bad ? ValueComparation.Bad : ValueComparation.Standard;
      }
    }

    public static ValueComparation CompareValue(float value, float good, float bad)
    {
      var comp = value >= good ? ValueComparation.Good : value <= bad ? ValueComparation.Bad : ValueComparation.Standard;
      if (good < bad)
      {
        comp = comp == ValueComparation.Good ? ValueComparation.Bad : comp == ValueComparation.Bad ? ValueComparation.Good : ValueComparation.Standard;
      }
      return comp;
    }

    public static IDisposable SetMemoEvents(Func<string> getDataMemo, Action<MyContext, string> setDbMemo, ReactiveProperty<string> memo, ReactiveProperty<bool> isSaving)
    {
      return memo.Skip(1).Where(m => m != getDataMemo()).Subscribe(m =>
      {
        _ = Task.Run(async () =>
        {
          if (isSaving.Value)
          {
            return;
          }
          isSaving.Value = true;

          try
          {
            using var db = new MyContext();
            /*
            db.RaceHorses!.Attach(data);
            data.Memo = m;
            */
            setDbMemo(db, m);

            await db.SaveChangesAsync(timeout: 5_000);
          }
          catch (Exception ex)
          {
            logger.Error($"メモ保存中にエラー: {m}", ex);
            OpenErrorSavingMemoRequest.Default.Request(m);

            // Dispose後に例外発生する可能性のあるコードはここへ
            memo.Value = getDataMemo();
          }
          finally
          {
            isSaving.Value = false;
          }
        });
      });
    }

    public static double CalcPci(RaceData race, RaceHorseData horse)
    {
      if (horse.ResultOrder == default || horse.ResultTimeValue == default || horse.AfterThirdHalongTimeValue == default)
      {
        return default;
      }
      var baseTime = (horse.ResultTime.TotalSeconds - horse.AfterThirdHalongTime.TotalSeconds) / (race.Distance - 600) * 600;
      return baseTime / horse.AfterThirdHalongTime.TotalSeconds * 100 - 50;
    }

    public static double CalcRpci(RaceData race, RaceHorseData topHorse)
    {
      if (race.AfterHaronTime3 == default)
      {
        return default;
      }
      var baseTime = (double)(topHorse.ResultTime.TotalSeconds * 10 - race.AfterHaronTime3) / (race.Distance - 600) * 600;
      return baseTime / race.AfterHaronTime3 * 100 - 50;
    }
  }
}
