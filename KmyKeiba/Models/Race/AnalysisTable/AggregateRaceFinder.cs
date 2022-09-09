using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Race.Finder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.AnalysisTable
{
  public class AggregateRaceFinder
  {
    private readonly List<CacheItem> _caches = new();

    public async Task<RaceHorseFinderQueryResult> FindRaceHorsesAsync(string keys, int sizeMax, RaceHorseAnalyzer horse)
    {
      var reader = new ScriptKeysReader(keys);
      var queries = reader.GetQueries(horse);

      var relations = queries.QueryValueRelations;
      var currentRaceKeys = relations.Where(r => r.Type == QueryValueRelationType.CurrentRaceValue).Select(r => r.Key).ToList();
      var canUseCache = true;

      if (relations.Any(r => r.Type == QueryValueRelationType.CurrentRaceValue || r.Type == QueryValueRelationType.CurrentRaceItem))
      {
        var enableKeys = new[]
        {
          QueryKey.Course,
          QueryKey.Condition,
          QueryKey.Weather,
          QueryKey.Popular,
          QueryKey.Distance,
          QueryKey.RunningStyle,
          QueryKey.HorseNumber,
          QueryKey.FrameNumber,
          QueryKey.RiderCode,
          QueryKey.RiderName,
          QueryKey.TrainerCode,
          QueryKey.TrainerName,
          QueryKey.Subject,
        };
        if (!currentRaceKeys.All(c => enableKeys.Contains(c)))
        {
          canUseCache = false;
        }

        if (canUseCache)
        {
          var caches = this._caches.Where(c => c.Keys == keys && c.Horse.Race.StartTime < horse.Race.StartTime.AddMonths(3)).ToArray();
          if (caches.Any())
          {
            foreach (var cache in caches)
            {
              var ch = cache.Horse;
              canUseCache = true;

              foreach (var key in currentRaceKeys)
              {
                switch (key)
                {
                  case QueryKey.Course:
                    canUseCache = ch.Race.Course == horse.Race.Course;
                    break;
                  case QueryKey.Condition:
                    canUseCache = ch.Race.TrackCondition == horse.Race.TrackCondition;
                    break;
                  case QueryKey.Weather:
                    canUseCache = ch.Race.TrackWeather == horse.Race.TrackWeather;
                    break;
                  case QueryKey.Popular:
                    canUseCache = ch.Data.Popular == horse.Data.Popular;
                    break;
                  case QueryKey.Distance:
                    canUseCache = ch.Race.Distance == horse.Race.Distance;
                    break;
                  case QueryKey.Subject:
                    canUseCache = ch.Race.SubjectAgeYounger == horse.Race.SubjectAgeYounger &&
                      ch.Race.SubjectDisplayInfo == horse.Race.SubjectDisplayInfo;
                    break;
                  case QueryKey.RunningStyle:
                    canUseCache = ch.History?.RunningStyle == horse.History?.RunningStyle;
                    break;
                  case QueryKey.FrameNumber:
                    canUseCache = ch.Data.FrameNumber == horse.Data.FrameNumber;
                    break;
                  case QueryKey.HorseNumber:
                    canUseCache = ch.Data.Number == horse.Data.Number;
                    break;
                  case QueryKey.RiderCode:
                    canUseCache = ch.Data.RiderCode == horse.Data.RiderCode;
                    break;
                  case QueryKey.RiderName:
                    canUseCache = ch.Data.RiderName == horse.Data.RiderName;
                    break;
                  case QueryKey.TrainerCode:
                    canUseCache = ch.Data.TrainerCode == horse.Data.TrainerCode;
                    break;
                  case QueryKey.TrainerName:
                    canUseCache = ch.Data.TrainerName == horse.Data.TrainerName;
                    break;
                }

                if (!canUseCache)
                {
                  break;
                }
              }

              if (canUseCache && cache?.QueryResult != null)
              {
                return cache.QueryResult;
              }
            }
          }
        }
      }
      else
      {
        var cache = this._caches.FirstOrDefault(c => c.Keys == keys);
        if (cache?.QueryResult != null)
        {
          return cache.QueryResult;
        }
      }

      using var finder = new PureRaceFinder(horse);
      var result = await finder.FindRaceHorsesAsync(queries, sizeMax);

      if (canUseCache)
      {
        lock (this._caches)
        {
          this._caches.Add(new CacheItem(keys, currentRaceKeys, horse, result));
        }
      }

      return result;
    }

    private class CacheItem
    {
      public string Keys { get; }

      public IReadOnlyList<QueryKey> CurrentRaceValues { get; }

      public RaceHorseAnalyzer Horse { get; }

      public RaceHorseFinderQueryResult QueryResult { get; }

      public CacheItem(string keys, IReadOnlyList<QueryKey> qkeys, RaceHorseAnalyzer horse, RaceHorseFinderQueryResult result)
      {
        this.Keys = keys;
        this.CurrentRaceValues = qkeys;
        this.Horse = horse;
        this.QueryResult = result;
      }
    }
  }
}
