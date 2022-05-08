﻿using KmyKeiba.Common;
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
  public class RaceTrendAnalysisSelector : TrendAnalysisSelector<RaceTrendAnalysisSelector.Key, RaceTrendAnalyzer.Key, RaceTrendAnalyzer>
  {
    public enum Key
    {
      [Label("コース")]
      SameCourse,

      [Label("馬場状態")]
      SameCondition,

      [Label("レース名")]
      SameRaceName,

      [Label("条件")]
      SameSubject,

      [Label("時刻")]
      SameTime,

      [Label("月")]
      SameMonth,
    }

    public RaceData Race { get; }

    public RaceTrendAnalysisSelector(MyContext db, RaceData race) : base(db, typeof(Key))
    {
      this.Race = race;
    }

    protected override RaceTrendAnalyzer GenerateAnalyzer()
    {
      return new RaceTrendAnalyzer(this.Race);
    }

    protected override async Task InitializeAnalyzerAsync(MyContext db, RaceTrendAnalyzer analyzer)
    {
      var query = db.Races!.Where(r => r.StartTime < this.Race.StartTime && r.DataStatus != RaceDataStatus.Aborted);
      var key = this.Keys;

      if (key.IsChecked(Key.SameCourse))
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

      if (key.IsChecked(Key.SameMonth))
      {
        query = query.Where(r => r.StartTime.Month == this.Race.StartTime.Month);
      }
      if (key.IsChecked(Key.SameCondition))
      {
        query = query.Where(r => r.TrackCondition == this.Race.TrackCondition);
      }
      if (key.IsChecked(Key.SameRaceName) && !string.IsNullOrWhiteSpace(this.Race.Name))
      {
        query = query.Where(r => r.Name == this.Race.Name);
      }
      if (key.IsChecked(Key.SameSubject))
      {
        query = query.Where(r => r.SubjectName == this.Race.SubjectName &&
                                 r.SubjectAge2 == this.Race.SubjectAge2 &&
                                 r.SubjectAge3 == this.Race.SubjectAge3 &&
                                 r.SubjectAge4 == this.Race.SubjectAge4 &&
                                 r.SubjectAge5 == this.Race.SubjectAge5 &&
                                 r.SubjectAgeYounger == this.Race.SubjectAgeYounger);
      }
      if (key.IsChecked(Key.SameTime))
      {
        query = query.Where(r => r.StartTime.Hour == this.Race.StartTime.Hour);
      }

      var races = await query
        .OrderByDescending(r => r.StartTime)
        .Take(200)
        .ToArrayAsync();
      var raceKeys = races.Select(r => r.Key).ToArray();
      var raceHorses = await db.RaceHorses!
        .Where(rh => rh.ResultOrder >= 1 && rh.ResultOrder <= 5 && raceKeys.Contains(rh.RaceKey))
        .ToArrayAsync();
      analyzer.SetRaces(races.Select(r => new RaceTrendAnalyzer.LightRaceInfo(r, raceHorses.Where(rh => rh.RaceKey == r.Key).ToArray())));
    }
  }
}
