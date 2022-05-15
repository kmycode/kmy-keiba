using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis.Generic;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis
{
  /// <summary>
  /// 条件にあったレース傾向解析を選択
  /// </summary>
  public class RaceTrendAnalysisSelector : TrendAnalysisSelector<RaceTrendAnalysisSelector.Key, RaceTrendAnalyzer>
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

    public RaceTrendAnalysisSelector(MyContext db, RaceData race) : base(db, typeof(Key))
    {
      this.Race = race;

      if (race.Grade == RaceGrade.Others)
      {
        this.Keys.RemoveKey(Key.SameRaceName);
      }
    }

    protected override RaceTrendAnalyzer GenerateAnalyzer()
    {
      return new RaceTrendAnalyzer(this.Race);
    }

    protected override async Task InitializeAnalyzerAsync(MyContext db, IEnumerable<Key> keys, RaceTrendAnalyzer analyzer)
    {
      var query = db.Races!
        .Where(r => r.StartTime < this.Race.StartTime && r.DataStatus != RaceDataStatus.Aborted && r.TrackType == this.Race.TrackType);

      if (keys.Contains(Key.SameCourse))
      {
        query = query.Where(r => r.Course == this.Race.Course);
      }
      else
      {
        if (this.Race.Course <= RaceCourse.CentralMaxValue)
        {
          query = query.Where(r => r.Course <= RaceCourse.CentralMaxValue);
        }
        else
        {
          query = query.Where(r => r.Course > RaceCourse.CentralMaxValue);
        }
      }

      if (keys.Contains(Key.NearDistance))
      {
        query = query.Where(r => r.Distance >= this.Race.Distance - 100 && r.Distance <= this.Race.Distance + 100);
      }
      if (keys.Contains(Key.SameMonth))
      {
        query = query.Where(r => r.StartTime.Month == this.Race.StartTime.Month);
      }
      if (keys.Contains(Key.SameCondition))
      {
        query = query.Where(r => r.TrackCondition == this.Race.TrackCondition);
      }
      if (keys.Contains(Key.SameRaceName) && !string.IsNullOrWhiteSpace(this.Race.Name))
      {
        query = query.Where(r => r.Name == this.Race.Name);
      }
      if (keys.Contains(Key.SameSubject))
      {
        query = query.Where(r => r.SubjectName == this.Race.SubjectName &&
                                 r.SubjectAge2 == this.Race.SubjectAge2 &&
                                 r.SubjectAge3 == this.Race.SubjectAge3 &&
                                 r.SubjectAge4 == this.Race.SubjectAge4 &&
                                 r.SubjectAge5 == this.Race.SubjectAge5 &&
                                 r.SubjectAgeYounger == this.Race.SubjectAgeYounger);
      }
      if (keys.Contains(Key.SameGrade))
      {
        query = query.Where(r => r.Grade == this.Race.Grade);
      }
      if (keys.Contains(Key.SameWeather))
      {
        query = query.Where(r => r.TrackWeather == this.Race.TrackWeather);
      }

      var races = await query
        .OrderByDescending(r => r.StartTime)
        .Take(300)
        .ToArrayAsync();
      var raceKeys = races.Select(r => r.Key).ToArray();
      var raceHorses = await db.RaceHorses!
        .Where(rh => rh.ResultOrder >= 1 && rh.ResultOrder <= 5 && raceKeys.Contains(rh.RaceKey))
        .ToArrayAsync();

      var list = new List<RaceAnalyzer>();
      foreach (var race in races)
      {
        list.Add(
          new RaceAnalyzer(
            race,
            raceHorses.Where(rh => rh.RaceKey == race.Key).ToArray(),
            await AnalysisUtil.GetRaceStandardTimeAsync(db, race)));
      }
      analyzer.SetSource(list);
    }
  }
}
