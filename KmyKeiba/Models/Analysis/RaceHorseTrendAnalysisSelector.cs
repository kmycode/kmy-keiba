using KmyKeiba.Common;
using KmyKeiba.Data.Db;
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
      SameCourse,

      [Label("馬場状態")]
      SameCondition,

      [Label("天気")]
      SameWeather,

      [Label("条件")]
      SameSubject,

      [Label("格")]
      SameGrade,

      [Label("季節")]
      SameSeason,

      [Label("距離")]
      NearDistance,
    }

    private IReadOnlyList<RaceHorseAnalysisData>? _allRaces;

    public RaceData Race { get; }

    public RaceHorseData RaceHorse { get; }

    public RaceHorseTrendAnalysisSelector(MyContext db, RaceData race, RaceHorseData horse) : base(db, typeof(Key))
    {
      this.Race = race;
      this.RaceHorse = horse;
    }

    protected override RaceHorseTrendAnalyzer GenerateAnalyzer()
    {
      return new RaceHorseTrendAnalyzer(this.Race, this.RaceHorse);
    }

    protected override async Task InitializeAnalyzerAsync(MyContext db, RaceHorseTrendAnalyzer analyzer)
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
        var list = new List<RaceHorseAnalysisData>();
        foreach (var race in allRaces)
        {
          var standardTime = await AnalysisUtil.GetRaceStandardTimeAsync(db, race.Race);
          list.Add(new RaceHorseAnalysisData(race.Race, race.RaceHorse, standardTime));
        }
        this._allRaces = list;
      }

      var query = this._allRaces.Where(r => r.Data.Key == this.RaceHorse.Key);
      var key = this.Keys;

      if (key.IsChecked(Key.SameCourse))
      {
        query = query.Where(r => r.Race.Course == this.Race.Course);
      }
      if (key.IsChecked(Key.NearDistance))
      {
        query = query.Where(r => r.Race.Distance / 200 == this.Race.Distance / 200);
      }
      if (key.IsChecked(Key.SameSeason))
      {
        query = query.Where(r => r.Race.StartTime.Month % 12 / 3 == this.Race.StartTime.Month % 12 / 3);
      }
      if (key.IsChecked(Key.SameCondition))
      {
        query = query.Where(r => r.Race.TrackCondition == this.Race.TrackCondition);
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

      analyzer.SetSource(query);
    }
  }
}
