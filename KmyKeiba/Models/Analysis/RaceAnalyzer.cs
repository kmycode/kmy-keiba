using KmyKeiba.Data.Db;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis
{
  /// <summary>
  /// レースの傾向などを解析
  /// </summary>
  public class RaceAnalyzer
  {
    private bool _isLoadedRelatedRaces;

    public RaceInfo Race { get; }

    public ObservableCollection<RaceData> SameCourseRaces { get; } = new();

    public ObservableCollection<RaceData> SamePastRaces { get; } = new();

    public RaceAnalyzer(RaceInfo race)
    {
      this.Race = race;
    }

    public async Task PredictAsync(MyContext db)
    {
      await this.LoadSimilarRacesAsync(db);
    }

    public async Task AnalysisResultAsync(MyContext db)
    {
      await this.LoadSimilarRacesAsync(db);
    }

    private async Task LoadSimilarRacesAsync(MyContext db)
    {
      if (this._isLoadedRelatedRaces)
      {
        return;
      }
      this._isLoadedRelatedRaces = true;

      var races = db.Races!
        .Where(r => r.Course == this.Race.Data.Course)
        .Where(r => r.TrackGround == this.Race.Data.TrackGround)
        .Where(r => r.TrackType == this.Race.Data.TrackType)
        .Where(r => r.CourseType == this.Race.Data.CourseType)
        .Where(r => r.Distance <= this.Race.Data.Distance + 200 && r.Distance >= this.Race.Data.Distance - 200)
        .OrderByDescending(r => r.StartTime);

      // 条件の近いレース
      var similarRaces = await races
        .Where(r => r.StartTime < this.Race.Data.StartTime && r.StartTime >= this.Race.Data.StartTime.AddYears(-1))
        .Take(100)
        .ToArrayAsync();
      foreach (var race in similarRaces)
      {
        this.SameCourseRaces.Add(race);
      }

      // 同じ名前・条件の過去レース
      var samePastRacesQuery =
        !string.IsNullOrWhiteSpace(this.Race.Data.Name) ? races.Where(r => r.Name == this.Race.Data.Name) :
        !string.IsNullOrWhiteSpace(this.Race.Data.SubjectName) ? races.Where(r => r.SubjectName == this.Race.Data.Name) :
        races.Where(r => r.SubjectAge2 == this.Race.Data.SubjectAge2 &&
                         r.SubjectAge3 == this.Race.Data.SubjectAge3 &&
                         r.SubjectAge4 == this.Race.Data.SubjectAge4 &&
                         r.SubjectAge5 == this.Race.Data.SubjectAge5);
      var samePastRaces = await samePastRacesQuery
        .Where(r => r.Distance == this.Race.Data.Distance)
        .Take(20)
        .ToArrayAsync();
      foreach (var race in samePastRaces)
      {
        this.SamePastRaces.Add(race);
      }
    }
  }
}
