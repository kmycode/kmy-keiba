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
  public class RaceHorseTrendAnalysisSelector : TrendAnalysisSelector<RaceHorseTrendAnalysisSelector.Key, RaceHorseTrendAnalyzer>
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

      [Label("条件")]
      [ScriptParameterKey("subject")]
      SameSubject,

      [Label("格")]
      [ScriptParameterKey("grade")]
      SameGrade,

      [Label("季節")]
      [ScriptParameterKey("season")]
      SameSeason,

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

      [Label("間隔")]
      [ScriptParameterKey("interval")]
      NearInterval,
    }

    private IReadOnlyList<RaceHorseAnalyzer>? _allRaces;

    public override string Name => this.RaceHorse.Name;

    public RaceData Race { get; }

    public RaceHorseData RaceHorse { get; }

    protected override bool IsAutoLoad => this._allRaces != null;

    public RaceHorseTrendAnalysisSelector(RaceData race, RaceHorseData horse) : base(typeof(Key))
    {
      this.Race = race;
      this.RaceHorse = horse;

      base.OnFinishedInitialization();
    }

    public RaceHorseTrendAnalysisSelector(RaceData race, RaceHorseData horse, IReadOnlyList<RaceHorseAnalyzer> source) : this(race, horse)
    {
      this._allRaces = source;
      this.BeginLoad();
    }

    protected override RaceHorseTrendAnalyzer GenerateAnalyzer()
    {
      return new RaceHorseTrendAnalyzer(this.Race, this.RaceHorse);
    }

    protected override async Task InitializeAnalyzerAsync(MyContext db, IEnumerable<Key> keys, RaceHorseTrendAnalyzer analyzer, int count, int offset, bool isLoadSameHorses)
    {
      // WARNING: 全体の総数が多くないと予想されるのでここでDBからすべて取得し、配分している
      //          間違ってもこれをこのまま他のSelectorクラスにコピペしないように
      if (this._allRaces == null)
      {
        var allRaces = await db.RaceHorses!
          .Where(h => h.Key == this.RaceHorse.Key)
          .Join(db.Races!, rh => rh.RaceKey, r => r.Key, (rh, r) => new { RaceHorse = rh, Race = r, })
          .Where(d => d.Race.StartTime < this.Race.StartTime)
          .OrderByDescending(d => d.Race.StartTime)
          .ToArrayAsync();
        var list = new List<RaceHorseAnalyzer>();
        foreach (var race in allRaces)
        {
          var standardTime = await AnalysisUtil.GetRaceStandardTimeAsync(db, race.Race);
          list.Add(new RaceHorseAnalyzer(race.Race, race.RaceHorse, standardTime));
        }
        this._allRaces = list;
      }

      var query = this._allRaces.Where(r => r.Data.Key == this.RaceHorse.Key);

      if (keys.Contains(Key.SameCourse))
      {
        query = query.Where(r => r.Race.Course == this.Race.Course);
      }
      if (keys.Contains(Key.SameGround))
      {
        query = query.Where(r => r.Race.TrackGround == this.Race.TrackGround);
      }
      if (keys.Contains(Key.NearDistance))
      {
        query = query.Where(r => r.Race.Distance / 200 == this.Race.Distance / 200);
      }
      if (keys.Contains(Key.SameDirection))
      {
        query = query.Where(r => r.Race.TrackCornerDirection == this.Race.TrackCornerDirection);
      }
      if (keys.Contains(Key.SameSeason))
      {
        query = query.Where(r => r.Race.StartTime.Month % 12 / 3 == this.Race.StartTime.Month % 12 / 3);
      }
      if (keys.Contains(Key.SameCondition))
      {
        query = query.Where(r => r.Race.TrackCondition == this.Race.TrackCondition);
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
        query = query.Where(r => r.Data.ResultOrder >= 1 && r.Data.ResultOrder <= 3);
      }
      if (keys.Contains(Key.Losed))
      {
        query = query.Where(r => r.Data.ResultOrder > 5);
      }
      if (keys.Contains(Key.NearInterval))
      {
        var (min, max) = AnalysisUtil.GetIntervalRange(this.RaceHorse.PreviousRaceDays);
        query = query.Where(r => r.Data.PreviousRaceDays >= min && r.Data.PreviousRaceDays <= max);
      }

      analyzer.SetSource(query);
    }
  }
}
