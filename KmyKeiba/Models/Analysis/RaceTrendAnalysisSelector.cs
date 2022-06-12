using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis.Generic;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race;
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
    }

    public override string Name => this._subject.DisplayName;

    public RaceData Race { get; }

    private readonly RaceSubjectInfo _subject;

    public RaceTrendAnalysisSelector(RaceData race) : base(typeof(Key))
    {
      this.Race = race;
      this._subject = new RaceSubjectInfo(race);

      base.OnFinishedInitialization();
    }

    protected override RaceTrendAnalyzer GenerateAnalyzer(int sizeMax)
    {
      return new RaceTrendAnalyzer(sizeMax, this.Race);
    }

    protected override async Task InitializeAnalyzerAsync(MyContext db, IEnumerable<Key> keys, RaceTrendAnalyzer analyzer, int sizeMax, int offset, bool isLoadSameHorses)
    {
      var query = db.Races!
        .Where(r => r.StartTime < this.Race.StartTime && r.DataStatus != RaceDataStatus.Canceled && r.TrackType == this.Race.TrackType);

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

      if (keys.Contains(Key.SameGround))
      {
        query = query.Where(r => r.TrackGround == this.Race.TrackGround);
      }
      if (keys.Contains(Key.NearDistance))
      {
        query = query.Where(r => r.Distance >= this.Race.Distance - 100 && r.Distance <= this.Race.Distance + 100);
      }
      if (keys.Contains(Key.SameDirection))
      {
        query = query.Where(r => r.TrackCornerDirection == this.Race.TrackCornerDirection);
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
        if (this.Race.GradeId == default)
        {
          query = query.Where(r => r.Name == this.Race.Name);
        }
        else
        {
          query = query.Where(r => r.Name == this.Race.Name || r.GradeId == this.Race.GradeId);
        }
      }
      if (keys.Contains(Key.SameSubject))
      {
        if (this.Race.Course <= RaceCourse.CentralMaxValue)
        {
          query = query.Where(r => r.SubjectName == this.Race.SubjectName &&
                                   r.SubjectAge2 == this.Race.SubjectAge2 &&
                                   r.SubjectAge3 == this.Race.SubjectAge3 &&
                                   r.SubjectAge4 == this.Race.SubjectAge4 &&
                                   r.SubjectAge5 == this.Race.SubjectAge5 &&
                                   r.SubjectAgeYounger == this.Race.SubjectAgeYounger);
        }
        else if (this.Race.Course >= RaceCourse.LocalMinValue && !string.IsNullOrEmpty(this.Race.SubjectDisplayInfo))
        {
          query = query.Where(r => r.SubjectDisplayInfo == this.Race.SubjectDisplayInfo);
        }
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
        .Skip(offset)
        .Take(sizeMax)
        .ToArrayAsync();
      var raceKeys = races.Select(r => r.Key).ToArray();
      var raceHorses = Array.Empty<RaceHorseData>();
      if (isLoadSameHorses)
      {
        raceHorses = await db.RaceHorses!
          .Where(rh => rh.ResultOrder >= 1 && rh.ResultOrder <= 5 && raceKeys.Contains(rh.RaceKey))
          .ToArrayAsync();
      }

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
