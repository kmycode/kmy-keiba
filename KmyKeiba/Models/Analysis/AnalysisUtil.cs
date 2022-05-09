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
        if (item == null || item.SampleCount < 50)
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
  }
}
