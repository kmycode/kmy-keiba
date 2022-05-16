using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis
{
  internal static class AnalysisUtil
  {
    private static readonly Dictionary<RaceCourse, IReadOnlyList<RaceStandardTimeMasterData>> _standardData = new();

    public static async Task<RaceStandardTimeMasterData> GetRaceStandardTimeAsync(MyContext db, RaceData race)
    {
      _standardData.TryGetValue(race.Course, out var list);

      if (list == null)
      {
        list = await db.RaceStandardTimes!
          .Where(st => st.Course == race.Course && st.SampleCount > 0)
          .ToArrayAsync();
        _standardData[race.Course] = list;
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
        item = query.FirstOrDefault(st => st.Condition == race.TrackCondition);
        if (item == null || item.SampleCount < 10)
        {
          item = query.FirstOrDefault(st => st.Condition == RaceCourseCondition.Unknown);
        }
      }
      else
      {
        item = query.FirstOrDefault();
      }

      return item ?? new();
    }

    public static double CalcRoughRate(IReadOnlyList<RaceHorseData> topHorses)
    {
      return topHorses.Where(rh => rh.ResultOrder >= 1 && rh.ResultOrder <= 3)
            .Select(rh => (double)rh.Popular * rh.Popular)
            .Append(0)    // Sum時の例外防止
            .Sum() / (1 * 1 + 2 * 2 + 3 * 3);
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
  }
}
