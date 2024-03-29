﻿using KmyKeiba.Common;
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
  public class RaceRiderTrendAnalysisSelector : TrendAnalysisSelector<RaceRiderTrendAnalysisSelector.Key, RaceRiderTrendAnalyzer>
  {
    public enum Key
    {
      [Label("コース")]
      [ScriptParameterKey("course")]
      SameCourse,

      [Label("地面")]
      [ScriptParameterKey("ground")]
      SameGround,

      [Label("馬場状態")]
      [ScriptParameterKey("condition")]
      SameCondition,

      [Label("天気")]
      [ScriptParameterKey("weather")]
      SameWeather,

      [Label("レース名")]
      [ScriptParameterKey("name")]
      SameRaceName,

      [Label("条件")]
      [ScriptParameterKey("subject")]
      SameSubject,

      [Label("格")]
      [ScriptParameterKey("grade")]
      SameGrade,

      [Label("月")]
      [ScriptParameterKey("month")]
      SameMonth,

      [Label("距離")]
      [ScriptParameterKey("distance")]
      NearDistance,

      [Label("向き")]
      [ScriptParameterKey("direction")]
      SameDirection,

      [Label("複勝")]
      [ScriptParameterKey("placebits")]
      [GroupName("ResultOrder")]
      PlaceBets,

      [Label("着外")]
      [ScriptParameterKey("losed")]
      [GroupName("ResultOrder")]
      Losed,

      [Label("運営")]
      [ScriptParameterKey("region")]
      SameRegion,

      [Label("重賞")]
      [ScriptParameterKey("grades")]
      Grades,

      [Label("性")]
      [ScriptParameterKey("sex")]
      Sex,

      [Label("枠")]
      [ScriptParameterKey("frame")]
      Frame,

      [Label("オッズ")]
      [ScriptParameterKey("odds")]
      [NotCacheKeyUntilRace]
      Odds,

      [ScriptParameterKey("runningstyle")]
      [IgnoreKey]
      RunningStyle,

      [ScriptParameterKey("rs_frontrunner")]
      [IgnoreKey]
      RS_FrontRunner,

      [ScriptParameterKey("rs_stalker")]
      [IgnoreKey]
      RS_Stalker,

      [ScriptParameterKey("rs_sotp")]
      [IgnoreKey]
      RS_Sotp,

      [ScriptParameterKey("rs_saverunner")]
      [IgnoreKey]
      RS_SaveRunner,
    }

    public override string Name => this.RaceHorse.RiderName;

    public override RaceData Race { get; }

    public RaceHorseData RaceHorse { get; }

    private RaceHorseAnalyzer? _horseAnalyzer;

    public RaceRiderTrendAnalysisSelector(RaceData race, RaceHorseData horse) : base(typeof(Key))
    {
      this.Race = race;
      this.RaceHorse = horse;

      if (race.Grade == RaceGrade.Others)
      {
        this.Keys.RemoveKey(Key.SameRaceName);
      }

      base.OnFinishedInitialization();
    }

    public RaceRiderTrendAnalysisSelector(RaceHorseAnalyzer horse) : this(horse.Race, horse.Data)
    {
      this._horseAnalyzer = horse;
    }

    protected override RaceRiderTrendAnalyzer GenerateAnalyzer(int sizeMax)
    {
      return new RaceRiderTrendAnalyzer(sizeMax, this.Race, this.RaceHorse);
    }

    protected override async Task InitializeAnalyzerAsync(MyContext db, IEnumerable<Key> keys, RaceRiderTrendAnalyzer analyzer, int sizeMax, int offset, bool isLoadSameHorses)
    {
      /*
      var query = db.Races!
        .Where(r => r.StartTime < this.Race.StartTime && r.DataStatus != RaceDataStatus.Canceled && r.TrackType == this.Race.TrackType)
        .Join(db.RaceHorses!, r => r.Key, rh => rh.RaceKey, (r, rh) => new { Race = r, RaceHorse = rh, })
        .Where(d => d.RaceHorse.RiderCode == this.RaceHorse.RiderCode);
      */
      var query = db.RaceHorses!
        .Where(d => d.RiderCode == this.RaceHorse.RiderCode)
        .Join(db.Races!.Where(r => r.StartTime < this.Race.StartTime && r.DataStatus != RaceDataStatus.Canceled && r.TrackType == this.Race.TrackType),
          rh => rh.RaceKey, r => r.Key, (rh, r) => new { RaceHorse = rh, Race = r, });

      if (keys.Contains(Key.SameCourse))
      {
        query = query.Where(r => r.Race.Course == this.Race.Course);
      }
      if (keys.Contains(Key.SameRegion))
      {
        if (this.Race.Course <= RaceCourse.CentralMaxValue)
        {
          query = query.Where(r => r.Race.Course <= RaceCourse.CentralMaxValue);
        }
        else
        {
          query = query.Where(r => r.Race.Course >= RaceCourse.LocalMinValue);
        }
      }

      if (keys.Contains(Key.SameGround))
      {
        query = query.Where(r => r.Race.TrackGround == this.Race.TrackGround);
      }
      if (keys.Contains(Key.NearDistance))
      {
        var diff = this.Race.Course <= RaceCourse.CentralMaxValue ?
          ApplicationConfiguration.Current.Value.NearDistanceDiffCentral :
          ApplicationConfiguration.Current.Value.NearDistanceDiffLocal;
        query = query.Where(r => r.Race.Distance >= this.Race.Distance - diff && r.Race.Distance <= this.Race.Distance + diff);
      }
      if (keys.Contains(Key.SameDirection))
      {
        query = query.Where(r => r.Race.TrackCornerDirection == this.Race.TrackCornerDirection);
      }
      if (keys.Contains(Key.SameMonth))
      {
        query = query.Where(r => r.Race.StartTime.Month == this.Race.StartTime.Month);
      }
      if (keys.Contains(Key.SameCondition))
      {
        query = query.Where(r => r.Race.TrackCondition == this.Race.TrackCondition);
      }
      if (keys.Contains(Key.SameRaceName) && !string.IsNullOrWhiteSpace(this.Race.Name))
      {
        query = query.Where(r => r.Race.Name == this.Race.Name);
      }
      if (keys.Contains(Key.SameSubject))
      {
        if (this.Race.Course <= RaceCourse.CentralMaxValue)
        {
          query = query.Where(r => r.Race.SubjectName == this.Race.SubjectName &&
                                   r.Race.SubjectAge2 == this.Race.SubjectAge2 &&
                                   r.Race.SubjectAge3 == this.Race.SubjectAge3 &&
                                   r.Race.SubjectAge4 == this.Race.SubjectAge4 &&
                                   r.Race.SubjectAge5 == this.Race.SubjectAge5 &&
                                   r.Race.SubjectAgeYounger == this.Race.SubjectAgeYounger);
        }
        else if (this.Race.Course >= RaceCourse.LocalMinValue && !string.IsNullOrEmpty(this.Race.SubjectDisplayInfo))
        {
          query = query.Where(r => r.Race.SubjectDisplayInfo == this.Race.SubjectDisplayInfo);
        }
      }
      if (keys.Contains(Key.SameGrade))
      {
        query = query.Where(r => r.Race.Grade == this.Race.Grade);
      }
      if (keys.Contains(Key.SameWeather))
      {
        query = query.Where(r => r.Race.TrackWeather == this.Race.TrackWeather);
      }
      if (keys.Contains(Key.PlaceBets))
      {
        query = query.Where(r => r.RaceHorse.ResultOrder >= 1 && r.RaceHorse.ResultOrder <= 3);
      }
      if (keys.Contains(Key.Losed))
      {
        query = query.Where(r => r.RaceHorse.ResultOrder > 5);
      }
      if (keys.Contains(Key.Grades))
      {
        query = query.Where(r => r.Race.Grade == RaceGrade.Grade1 || r.Race.Grade == RaceGrade.Grade2 || r.Race.Grade == RaceGrade.Grade3 ||
                                 r.Race.Grade == RaceGrade.LocalGrade1 || r.Race.Grade == RaceGrade.LocalGrade2 || r.Race.Grade == RaceGrade.LocalGrade3);
      }
      if (keys.Contains(Key.Sex))
      {
        query = query.Where(r => r.RaceHorse.Sex == this.RaceHorse.Sex);
      }
      if (keys.Contains(Key.Frame))
      {
        query = query.Where(r => r.RaceHorse.FrameNumber == this.RaceHorse.FrameNumber);
      }
      if (keys.Contains(Key.Odds))
      {
        var (min, max) = AnalysisUtil.GetOddsRange(this.RaceHorse.Odds);
        query = query.Where(r => r.RaceHorse.Odds >= min && r.RaceHorse.Odds < max);
      }
      if (this._horseAnalyzer?.History != null && keys.Contains(Key.RunningStyle))
      {
        var rs = this._horseAnalyzer.History.RunningStyle;
        query = query.Where(r => r.RaceHorse.RunningStyle == rs);
      }

      var rss = new List<RunningStyle>();
      if (keys.Contains(Key.RS_FrontRunner))
      {
        rss.Add(RunningStyle.FrontRunner);
      }
      if (keys.Contains(Key.RS_Stalker))
      {
        rss.Add(RunningStyle.Stalker);
      }
      if (keys.Contains(Key.RS_Sotp))
      {
        rss.Add(RunningStyle.Sotp);
      }
      if (keys.Contains(Key.RS_SaveRunner))
      {
        rss.Add(RunningStyle.SaveRunner);
      }
      if (rss.Any())
      {
        query = query.Where(r => rss.Contains(r.RaceHorse.RunningStyle));
      }

      var races = await query
        .OrderByDescending(r => r.Race.StartTime)
        .Skip(offset)
        .Take(sizeMax)
        .ToArrayAsync();
      var raceKeys = races.Select(r => r.Race.Key).ToArray();
      var raceHorses = Array.Empty<RaceHorseData>();
      if (isLoadSameHorses)
      {
        raceHorses = await db.RaceHorses!
          .Where(rh => rh.ResultOrder >= 1 && rh.ResultOrder <= 5 && raceKeys.Contains(rh.RaceKey))
          .ToArrayAsync();
      }

      var list = new List<RaceHorseAnalyzer>();
      foreach (var race in races)
      {
        list.Add(
          new RaceHorseAnalyzer(
            race.Race,
            race.RaceHorse,
            raceHorses.Where(rh => rh.RaceKey == race.Race.Key).ToArray(),
            await AnalysisUtil.GetRaceStandardTimeAsync(db, race.Race)));
      }
      analyzer.SetSource(list);
    }
  }
}
