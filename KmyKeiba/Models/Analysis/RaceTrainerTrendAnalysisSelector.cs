using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis.Generic;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis
{
  public class RaceTrainerTrendAnalysisSelector : TrendAnalysisSelector<RaceTrainerTrendAnalysisSelector.Key, RaceTrainerTrendAnalyzer>
  {
    public enum Key
    {
      [Label("コース")]
      SameCourse,

      [Label("馬場状態")]
      SameCondition,

      [Label("天気")]
      SameWeather,

      [Label("レース名")]
      SameRaceName,

      [Label("条件")]
      SameSubject,

      [Label("格")]
      SameGrade,

      [Label("月")]
      SameMonth,

      [Label("距離")]
      NearDistance,
    }

    public RaceData Race { get; }

    public RaceHorseData RaceHorse { get; }

    public RaceTrainerTrendAnalysisSelector(MyContext db, RaceData race, RaceHorseData horse) : base(db, typeof(Key))
    {
      this.Race = race;
      this.RaceHorse = horse;

      if (race.Grade == RaceGrade.Others)
      {
        this.Keys.RemoveKey(Key.SameRaceName);
      }
    }

    protected override RaceTrainerTrendAnalyzer GenerateAnalyzer()
    {
      return new RaceTrainerTrendAnalyzer(this.Race, this.RaceHorse);
    }

    protected override async Task InitializeAnalyzerAsync(MyContext db, RaceTrainerTrendAnalyzer analyzer)
    {
      var query = db.Races!
        .Where(r => r.StartTime < this.Race.StartTime && r.DataStatus != RaceDataStatus.Aborted && r.TrackType == this.Race.TrackType)
        .Join(db.RaceHorses!, r => r.Key, rh => rh.RaceKey, (r, rh) => new { Race = r, RaceHorse = rh, })
        .Where(d => d.RaceHorse.TrainerCode == this.RaceHorse.TrainerCode);
      var key = this.Keys;

      if (key.IsChecked(Key.SameCourse))
      {
        query = query.Where(r => r.Race.Course == this.Race.Course);
      }

      if (key.IsChecked(Key.NearDistance))
      {
        query = query.Where(r => r.Race.Distance >= this.Race.Distance - 100 && r.Race.Distance <= this.Race.Distance + 100);
      }
      if (key.IsChecked(Key.SameMonth))
      {
        query = query.Where(r => r.Race.StartTime.Month == this.Race.StartTime.Month);
      }
      if (key.IsChecked(Key.SameCondition))
      {
        query = query.Where(r => r.Race.TrackCondition == this.Race.TrackCondition);
      }
      if (key.IsChecked(Key.SameRaceName) && !string.IsNullOrWhiteSpace(this.Race.Name))
      {
        query = query.Where(r => r.Race.Name == this.Race.Name);
      }
      if (key.IsChecked(Key.SameSubject))
      {
        query = query.Where(r => r.Race.SubjectName == this.Race.SubjectName &&
                                 r.Race.SubjectAge2 == this.Race.SubjectAge2 &&
                                 r.Race.SubjectAge3 == this.Race.SubjectAge3 &&
                                 r.Race.SubjectAge4 == this.Race.SubjectAge4 &&
                                 r.Race.SubjectAge5 == this.Race.SubjectAge5 &&
                                 r.Race.SubjectAgeYounger == this.Race.SubjectAgeYounger);
      }
      if (key.IsChecked(Key.SameGrade))
      {
        query = query.Where(r => r.Race.Grade == this.Race.Grade);
      }
      if (key.IsChecked(Key.SameWeather))
      {
        query = query.Where(r => r.Race.TrackWeather == this.Race.TrackWeather);
      }

      var races = await query
        .OrderByDescending(r => r.Race.StartTime)
        .Take(300)
        .ToArrayAsync();
      var raceKeys = races.Select(r => r.Race.Key).ToArray();
      var raceHorses = await db.RaceHorses!
        .Where(rh => rh.ResultOrder >= 1 && rh.ResultOrder <= 5 && raceKeys.Contains(rh.RaceKey))
        .ToArrayAsync();

      var list = new List<RaceHorseAnalysisData>();
      foreach (var race in races)
      {
        list.Add(
          new RaceHorseAnalysisData(
            race.Race,
            race.RaceHorse,
            raceHorses.Where(rh => rh.RaceKey == race.Race.Key).ToArray(),
            await AnalysisUtil.GetRaceStandardTimeAsync(db, race.Race)));
      }
      analyzer.SetSource(list);
    }
  }
}
