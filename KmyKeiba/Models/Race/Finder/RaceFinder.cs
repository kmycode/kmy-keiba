using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KmyKeiba.Models.Race.Finder
{
  public class RaceFinder : IRaceFinder
  {
    private readonly PureRaceFinder _finder;
    private Dictionary<string, (int, RaceHorseFinderQueryResult)> _raceHorseCaches = new();

    public RaceData? Race => this._finder.Race;
    public RaceHorseData? RaceHorse => this._finder.RaceHorse;
    public RaceHorseAnalyzer? RaceHorseAnalyzer => this._finder.RaceHorseAnalyzer;

    public RaceFinder(RaceHorseAnalyzer horse)
    {
      this._finder = new PureRaceFinder(horse);
    }

    public RaceFinder(RaceData? race = null, RaceHorseData? raceHorse = null)
    {
      this._finder = new PureRaceFinder(race, raceHorse);
    }

    public void ClearCache()
    {
      foreach (var disposable in this._raceHorseCaches
        .SelectMany(c => c.Value.Item2.Items)
        .Cast<IDisposable>())
      {
        disposable.Dispose();
      }

      this._raceHorseCaches.Clear();
    }

    public void Dispose()
    {
      this.ClearCache();
      this._finder.Dispose();
    }

    public async Task<RaceHorseFinderQueryResult> FindRaceHorsesAsync(ScriptKeysParseResult raceQueries, int sizeMax, int offset = 0, bool isLoadSameHorses = false, bool withoutFutureRaces = true, bool withoutFutureRacesForce = false, CancellationToken cancellationToken = default)
    {
      // キャッシュはここで返す
      if (!raceQueries.IsRealtimeResult && !raceQueries.IsContainsFutureRaces &&
        this._raceHorseCaches.TryGetValue(raceQueries.Keys, out var cache) && cache.Item1 <= (raceQueries.Limit == default ? 3000 : raceQueries.Limit))
      {
        return cache.Item2;
      }

      var result = await this._finder.FindRaceHorsesAsync(raceQueries, sizeMax, offset, isLoadSameHorses, withoutFutureRaces, withoutFutureRacesForce, cancellationToken);

      if (!raceQueries.IsContainsFutureRaces && !raceQueries.IsRealtimeResult)
      {
        this._raceHorseCaches[raceQueries.Keys] = (sizeMax, result);
      }

      return result;
    }

    public Task<FinderQueryResult<RaceAnalyzer>> FindRacesAsync(string keys, int sizeMax, int offset = 0, bool withoutFutureRaces = true, bool withoutFutureRacesForce = false)
    {
      return this._finder.FindRacesAsync(keys, sizeMax, offset, withoutFutureRaces, withoutFutureRacesForce);
    }

    public bool HasRaceHorseCache(string keys)
    {
      return this.TryFindRaceHorseCache(keys) != null;
    }

    public RaceHorseFinderQueryResult? TryFindRaceHorseCache(string keys)
    {
      var reader = new ScriptKeysReader(keys);
      var raceQueries = reader.GetQueries(this.Race, this.RaceHorse);

      if (!raceQueries.IsRealtimeResult && this._raceHorseCaches.TryGetValue(keys, out var cache))
      {
        return cache.Item2;
      }
      return null;
    }

    public void CopyFrom(RaceFinder others)
    {
      CopyFrom(others, others._finder.Race, others._finder.RaceHorse, others._finder.RaceHorseAnalyzer);
    }

    public static RaceFinder CopyFrom(RaceFinder old, RaceData? race = null, RaceHorseData? horse = null, RaceHorseAnalyzer? analyzer = null)
    {
      var finder = analyzer == null ? new RaceFinder(race, horse) : new RaceFinder(analyzer);
      finder._raceHorseCaches = old._raceHorseCaches;
      return finder;
    }
  }
}
