using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Image;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race
{
  public class RaceInfo
  {
    public RaceData Data { get; }

    public ReactiveCollection<RaceCourseDetail> CourseDetails { get; } = new();

    public RaceTrendAnalysisSelector TrendAnalyzers { get; }

    public ReactiveCollection<RaceHorseAnalysisData> Horses { get; } = new();

    public ReactiveCollection<RaceHorseAnalysisData> HorsesResultOrdered { get; } = new();

    public ReactiveCollection<RaceCorner> Corners { get; } = new();

    public RaceCourseSummaryImage CourseSummaryImage { get; } = new();

    public RaceSubjectInfo Subject { get; }

    public string Name => this.Subject.DisplayName;

    private RaceInfo(MyContext db, RaceData race)
    {
      this.Data = race;
      this.Subject = new(race);

      var details = RaceCourses.TryGetCourses(race);
      foreach (var detail in details)
      {
        this.CourseDetails.Add(detail);
      }

      this.TrendAnalyzers = new RaceTrendAnalysisSelector(db, race);
      this.CourseSummaryImage.Race = race;
    }

    private void SetHorses(IReadOnlyList<RaceHorseAnalysisData> horses)
    {
      ThreadUtil.InvokeOnUiThread(() =>
      {
        this.Horses.AddRangeOnScheduler(horses.OrderBy(h => h.Data.Number));
        this.HorsesResultOrdered.AddRangeOnScheduler(
          horses.Where(h => h.Data.ResultOrder > 0).OrderBy(h => h.Data.ResultOrder).Concat(
            horses.Where(h => h.Data.ResultOrder == 0).OrderBy(h => h.Data.Number).OrderBy(h => h.Data.AbnormalResult)));
      });
    }

    public static async Task<RaceInfo?> FromKeyAsync(MyContext db, string key)
    {
      var race = await db.Races!.FirstOrDefaultAsync(r => r.Key == key);
      if (race == null)
      {
        return null;
      }

      return await FromDataAsync(db, race);
    }

    public static Task<RaceInfo> FromDataAsync(MyContext db, RaceData race)
    {
      var info = new RaceInfo(db, race);

      AddCorner(info.Corners, race.Corner1Result, race.Corner1Number, race.Corner1Position, race.Corner1LapTime);
      AddCorner(info.Corners, race.Corner2Result, race.Corner2Number, race.Corner2Position, race.Corner2LapTime);
      AddCorner(info.Corners, race.Corner3Result, race.Corner3Number, race.Corner3Position, race.Corner3LapTime);
      AddCorner(info.Corners, race.Corner4Result, race.Corner4Number, race.Corner4Position, race.Corner4LapTime);

      // 以降の情報は遅延読み込み
      _ = Task.Run(async () =>
      {
        var horses = await db.RaceHorses!.Where(rh => rh.RaceKey == race.Key).ToArrayAsync();
        var horseKeys = horses.Select(h => h.Key).ToArray();
        var horseAllHistories = await db.RaceHorses!
          .Where(rh => horseKeys.Contains(rh.Key))
          .Join(db.Races!, rh => rh.RaceKey, r => r.Key, (rh, r) => new { Race = r, RaceHorse = rh, })
          .Where(d => d.Race.StartTime < race.StartTime)
          .OrderByDescending(d => d.Race.StartTime)
          .ToArrayAsync();
        var standardTime = await AnalysisUtil.GetRaceStandardTimeAsync(db, race);

        // 各馬の情報
        var horseInfos = new List<RaceHorseAnalysisData>();
        foreach (var horse in horses)
        {
          var histories = new List<RaceHorseAnalysisData>();
          foreach (var history in horseAllHistories.Where(h => h.RaceHorse.Key == horse.Key))
          {
            var historyStandardTime = await AnalysisUtil.GetRaceStandardTimeAsync(db, history.Race);
            histories.Add(new RaceHorseAnalysisData(history.Race, history.RaceHorse, historyStandardTime));
          }

          horseInfos.Add(new RaceHorseAnalysisData(race, horse, histories, standardTime));
        }
        info.SetHorses(horseInfos);
      });

      return Task.FromResult(info);
    }

    private static void AddCorner(IList<RaceCorner> corners, string result, int num, int pos, TimeSpan lap)
    {
      if (string.IsNullOrWhiteSpace(result))
      {
        return;
      }

      var corner = RaceCorner.FromString(result);
      corner.Number = num;
      corner.Position = pos;
      corner.LapTime = lap;

      corners.Add(corner);
    }
  }
}
