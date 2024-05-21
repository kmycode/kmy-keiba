using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race
{
  public class CourseTrendInfo
  {
    public bool HasData { get; }

    public List<short> GoodFrameNumbers { get; } = new();

    public List<PersonInfo> GoodRiders { get; } = new();

    public List<PersonInfo> GoodTrainers { get; } = new();

    public RaceRefundTrend RefundTrend { get; }

    public RacePace MostRacePace { get; }

    public RunningStyle MostRunningStyle { get; }

    public RunningStyle MostRunningStylePlace { get; }

    public class PersonInfo
    {
      public string Name { get; init; } = string.Empty;

      public List<short> CurrentRaceHorseNumbers { get; init; } = new();

      public override string ToString()
      {
        if (this.CurrentRaceHorseNumbers.Any())
        {
          return this.Name + " (" + string.Join(',', this.CurrentRaceHorseNumbers.Order().Take(3)) + ")";
        }

        return this.Name;
      }
    }

    private CourseTrendInfo(bool isLocal, IEnumerable<RaceData> todayRaces, IEnumerable<RaceHorseData> currentRaceHorses, IEnumerable<RaceHorseData> todayHorses, IEnumerable<RefundData> todayRefunds)
    {
      this.HasData = todayHorses.Any();

      if (todayHorses.Any(h => h.FrameNumber > 0))
      {
        var frameNumbers = todayHorses
          .Where(h => h.FrameNumber > 0)
          .GroupBy(h => h.FrameNumber)
          .OrderByDescending(g => g.Count())
          .Select(g => g.Key)
          .ToArray();
        this.GoodFrameNumbers = frameNumbers.Take(3).ToList();

        var riderNames = todayHorses
          .GroupBy(h => h.RiderName)
          .OrderByDescending(g => g.Count())
          .Select(g => g.Key)
          .ToArray();
        this.GoodRiders = riderNames.Select(n => new PersonInfo
        {
          Name = n,
          CurrentRaceHorseNumbers = currentRaceHorses.Where(rh => rh.RiderName == n).Select(rh => rh.Number).ToList(),
        }).Take(3).ToList();

        var trainerNames = todayHorses
          .GroupBy(h => h.TrainerName)
          .OrderByDescending(g => g.Count())
          .Select(g => g.Key)
          .ToArray();
        this.GoodTrainers = trainerNames.Select(n => new PersonInfo
        {
          Name = n,
          CurrentRaceHorseNumbers = currentRaceHorses.Where(rh => rh.TrainerName == n).Select(rh => rh.Number).ToList(),
        }).Take(3).ToList();
      }

      if (todayRefunds.Any())
      {
        var averageSource = todayRefunds
          .Select(r => new[] { r.TrifectaNumber1Money, r.TrifectaNumber2Money, r.TrifectaNumber3Money, r.TrifectaNumber4Money, r.TrifectaNumber5Money, r.TrifectaNumber6Money, }.Max())
          .Where(m => m > 0);
        if (averageSource.Any())
        {
          var average = averageSource.Average();

          if (isLocal)
          {
            this.RefundTrend = average < 20_000 ? RaceRefundTrend.Normal :
                               average < 80_000 ? RaceRefundTrend.Warning : RaceRefundTrend.Turbulence;
          }
          else
          {
            this.RefundTrend = average < 30_000 ? RaceRefundTrend.Normal :
                               average < 200_000 ? RaceRefundTrend.Warning : RaceRefundTrend.Turbulence;
          }
        }
      }

      this.MostRacePace = todayRaces
        .Select(AnalysisUtil.CalcRacePace)
        .Where(p => p != RacePace.Unknown)
        .Select(p => p == RacePace.VeryHigh ?  RacePace.High : p == RacePace.VeryLow ? RacePace.Low : p)
        .GroupBy(p => p)
        .OrderByDescending(p => p.Count())
        .Select(p => p.Key)
        .FirstOrDefault();

      this.MostRunningStyle = todayHorses
        .Where(rh => rh.ResultOrder == 1)
        .Select(rh => rh.RunningStyle)
        .Where(rs => rs != RunningStyle.Unknown)
        .GroupBy(rs => rs)
        .OrderByDescending(rs => rs.Count())
        .Select(rs => rs.Key)
        .FirstOrDefault();

      this.MostRunningStylePlace = todayHorses
        .Select(rh => rh.RunningStyle)
        .Where(rs => rs != RunningStyle.Unknown)
        .GroupBy(rs => rs)
        .OrderByDescending(rs => rs.Count())
        .Select(rs => rs.Key)
        .FirstOrDefault();
    }

    public static async Task<CourseTrendInfo> GetDataUntilAsync(MyContext db, RaceData race, IEnumerable<RaceHorseData> currentRaceHorses)
    {
      var date = race.StartTime.Date;

      var races = await db.Races!.Where(r => r.StartTime < race.StartTime && r.StartTime >= date && r.Course == race.Course).ToArrayAsync();
      var raceKeys = races.Select(r => r.Key).ToArray();

      var raceHorses = await db.RaceHorses!.Where(rh => raceKeys.Contains(rh.RaceKey) && rh.ResultOrder >= 1 && rh.ResultOrder <= 3).ToArrayAsync();
      var refunds = await db.Refunds!.Where(r => raceKeys.Contains(r.RaceKey)).ToArrayAsync();

      return new CourseTrendInfo(race.Course.IsLocal(), races, currentRaceHorses, raceHorses, refunds);
    }
  }

  public enum RaceRefundTrend
  {
    [Label("順当")]
    Normal,

    [Label("波乱")]
    Warning,

    [Label("大荒れ")]
    Turbulence,
  }
}
