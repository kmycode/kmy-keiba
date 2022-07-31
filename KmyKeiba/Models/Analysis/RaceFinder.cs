using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis.Generic;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis
{
  public class RaceFinder
  {
    private readonly CompositeDisposable _disposables = new();

    public string Name => this.Subject.DisplayName;

    public RaceData Race { get; }

    public RaceSubjectInfo Subject { get; }

    public RaceFinder(RaceData race)
    {
      this.Race = race;
      this.Subject = new RaceSubjectInfo(race);
    }

    public async Task<IList<RaceHorseAnalyzer>> GetRaceHorsesAsync(MyContext db, string keys, int sizeMax, int offset = 0, bool isLoadSameHorses = false, bool withoutFutureRaces = true)
    {
      var reader = new ScriptKeysReader(keys);

      IQueryable<RaceData> races = db.Races!;
      if (withoutFutureRaces)
      {
        races = races.Where(r => r.StartTime < this.Race.StartTime && r.DataStatus != RaceDataStatus.Canceled && r.TrackType == this.Race.TrackType);
      }
      var horses = (IQueryable<RaceHorseData>)db.RaceHorses!;

      var raceQueries = reader.GetQueries(this.Race);
      // var horseQueries =

      foreach (var q in raceQueries)
      {
        races = q.Apply(db, races);
      }

      if (withoutFutureRaces)
      {
        horses = horses.Where(rh => rh.DataStatus >= RaceDataStatus.PreliminaryGrade);
      }
      var query = horses
        .Join(races, rh => rh.RaceKey, r => r.Key, (rh, r) => new { RaceHorse = rh, Race = r, });

      var racesData = await query
        .OrderByDescending(r => r.Race.StartTime)
        .Skip(offset)
        .Take(sizeMax)
        .ToArrayAsync();
      var raceKeys = racesData.Select(r => r.Race.Key).ToArray();
      var raceHorsesData = Array.Empty<RaceHorseData>();
      if (isLoadSameHorses)
      {
        raceHorsesData = await db.RaceHorses!
          .Where(rh => rh.ResultOrder >= 1 && rh.ResultOrder <= 5 && raceKeys.Contains(rh.RaceKey))
          .ToArrayAsync();
      }

      var list = new List<RaceHorseAnalyzer>();
      foreach (var race in racesData)
      {
        list.Add(
          new RaceHorseAnalyzer(
            race.Race,
            race.RaceHorse,
            raceHorsesData.Where(rh => rh.RaceKey == race.Race.Key).ToArray(),
            await AnalysisUtil.GetRaceStandardTimeAsync(db, race.Race)));
      }

      return list;
    }

    public async Task<IList<RaceData>> GetRacesAsync(MyContext db, string keys, int sizeMax, int offset = 0, bool withoutFutureRaces = true)
    {
      var reader = new ScriptKeysReader(keys);

      IQueryable<RaceData> races = db.Races!;
      if (withoutFutureRaces)
      {
        races = races.Where(r => r.StartTime < this.Race.StartTime && r.DataStatus != RaceDataStatus.Canceled && r.TrackType == this.Race.TrackType);
      }

      var raceQueries = reader.GetQueries(this.Race);

      foreach (var q in raceQueries)
      {
        races = q.Apply(db, races);
      }

      var racesData = await races
        .OrderByDescending(r => r.StartTime)
        .Skip(offset)
        .Take(sizeMax)
        .ToArrayAsync();

      return racesData;
    }

    public void Dispose()
    {
      this._disposables.Dispose();
    }
  }
}
