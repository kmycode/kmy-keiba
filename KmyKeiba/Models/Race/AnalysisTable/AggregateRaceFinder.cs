using CefSharp.DevTools.CacheStorage;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Race.Finder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace KmyKeiba.Models.Race.AnalysisTable
{
  public class AggregateRaceFinder
  {
    private readonly Dictionary<string, List<CacheItem>> _caches = new();
    private readonly List<CacheItem> _notProceedCaches = new();

    public async Task<AggregateRaceFinderCacheItem> FindRaceHorsesAsync(string keys, int sizeMax, RaceHorseAnalyzer horse, object? tag = null)
    {
      var reader = new ScriptKeysReader(keys);
      var queries = reader.GetQueries(horse);

      var relations = queries.QueryValueRelations;
      var currentRaceKeys = relations.Where(r => r.Type == QueryValueRelationType.CurrentRaceValue).Select(r => r.Key).ToList();
      var canUseCache = true;

      this._caches.TryGetValue(keys, out var cacheList);

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
          QueryKey.Age,
          QueryKey.Sex,
          QueryKey.Color,
          QueryKey.WeightDiff,
          QueryKey.RiderWeight,
        };
        if (!currentRaceKeys.All(c => enableKeys.Contains(c)))
        {
          canUseCache = false;
        }

        if (canUseCache && cacheList != null)
        {
          IEnumerable<CacheItem> caches;
          lock (cacheList)
          {
            caches = cacheList.Where(c => c.Keys == keys && c.Horse.Race.StartTime > horse.Race.StartTime.AddMonths(-2)).ToArray();
          }
          if (caches.Any())
          {
            foreach (var cache in caches)
            {
              var ch = cache.Horse;
              var isHit = true;

              foreach (var key in currentRaceKeys)
              {
                switch (key)
                {
                  case QueryKey.Course:
                    isHit = ch.Race.Course == horse.Race.Course;
                    break;
                  case QueryKey.Condition:
                    isHit = ch.Race.TrackCondition == horse.Race.TrackCondition;
                    break;
                  case QueryKey.Weather:
                    isHit = ch.Race.TrackWeather == horse.Race.TrackWeather;
                    break;
                  case QueryKey.Popular:
                    isHit = ch.Data.Popular == horse.Data.Popular;
                    break;
                  case QueryKey.Distance:
                    isHit = ch.Race.Distance == horse.Race.Distance;
                    break;
                  case QueryKey.Subject:
                    isHit = ch.Race.SubjectAgeYounger == horse.Race.SubjectAgeYounger &&
                      ch.Race.SubjectDisplayInfo == horse.Race.SubjectDisplayInfo;
                    break;
                  case QueryKey.RunningStyle:
                    isHit = ch.History?.RunningStyle == horse.History?.RunningStyle;
                    break;
                  case QueryKey.FrameNumber:
                    isHit = ch.Data.FrameNumber == horse.Data.FrameNumber;
                    break;
                  case QueryKey.HorseNumber:
                    isHit = ch.Data.Number == horse.Data.Number;
                    break;
                  case QueryKey.RiderCode:
                    isHit = ch.Data.RiderCode == horse.Data.RiderCode;
                    break;
                  case QueryKey.RiderName:
                    isHit = ch.Data.RiderName == horse.Data.RiderName;
                    break;
                  case QueryKey.TrainerCode:
                    isHit = ch.Data.TrainerCode == horse.Data.TrainerCode;
                    break;
                  case QueryKey.TrainerName:
                    isHit = ch.Data.TrainerName == horse.Data.TrainerName;
                    break;
                  case QueryKey.Age:
                    isHit = ch.Data.Age == horse.Data.Age;
                    break;
                  case QueryKey.Sex:
                    isHit = ch.Data.Sex == horse.Data.Sex;
                    break;
                  case QueryKey.Color:
                    isHit = ch.Data.Color == horse.Data.Color;
                    break;
                  case QueryKey.WeightDiff:
                    isHit = ch.Data.WeightDiff == horse.Data.WeightDiff;
                    break;
                  case QueryKey.RiderWeight:
                    isHit = ch.Data.RiderWeight == horse.Data.RiderWeight;
                    break;
                }

                if (!isHit)
                {
                  break;
                }
              }

              if (isHit && cache != null)
              {
                return cache.ToResult();
              }
            }
          }
        }
      }
      else if (cacheList != null)
      {
        // Fixed values
        var cache = cacheList.FirstOrDefault(c => c.Keys == keys);
        if (cache != null)
        {
          return cache.ToResult();
        }
      }

      using var finder = new PureRaceFinder(horse);
      var result = await finder.FindRaceHorsesAsync(queries, sizeMax);

      // メモの編集などしないので、この時点で破棄しておく
      result.Dispose();

      if (canUseCache)
      {
        lock (this._caches)
        {
          if (cacheList == null)
          {
            this._caches.TryGetValue(keys, out cacheList);
            if (cacheList == null)
            {
              cacheList = new List<CacheItem>();
              this._caches[keys] = cacheList;
            }
          }

          var cacheItem = new CacheItem(keys, currentRaceKeys, horse, result) { Tag = tag, };
          lock (cacheList)
          {
            cacheList.Add(cacheItem);
          }
          lock (this._notProceedCaches)
          {
            this._notProceedCaches.Add(cacheItem);
          }
        }
      }

      return new AggregateRaceFinderCacheItem(result, null);
    }

    public void CompressCache(RaceData race)
    {
      lock (this._notProceedCaches)
      {
        var removes = new List<CacheItem>();
        foreach (var cache in this._notProceedCaches.Where(c => c.Tag != null && c.QueryResult != null))
        {
          cache.QueryResult = null;
          removes.Add(cache);
        }
        foreach (var cache in removes)
        {
          this._notProceedCaches.Remove(cache);
        }
      }

      foreach (var cacheList in this._caches.Select(c => c.Value))
      {
        lock (cacheList)
        {
          var removes = cacheList.Where(c => c.Horse.Race.StartTime <= race.StartTime.AddMonths(-2)).ToArray();
          foreach (var cache in removes)
          {
            cacheList.Remove(cache);
          }
        }
      }
    }

    private class CacheItem
    {
      public string Keys { get; }

      public IReadOnlyList<QueryKey> CurrentRaceValues { get; }

      public RaceHorseAnalyzer Horse { get; }

      public RaceHorseFinderQueryResult? QueryResult { get; set; }

      public object? Tag { get; set; }

      public CacheItem(string keys, IReadOnlyList<QueryKey> qkeys, RaceHorseAnalyzer horse, RaceHorseFinderQueryResult result)
      {
        this.Keys = keys;
        this.CurrentRaceValues = qkeys;
        this.Horse = horse;
        this.QueryResult = result;
      }

      public AggregateRaceFinderCacheItem ToResult()
      {
        return new AggregateRaceFinderCacheItem(this.QueryResult, this.Tag);
      }
    }
  }

  public class AggregateRaceFinderCacheItem
  {
    public RaceHorseFinderQueryResult? QueryResult { get; }

    public object? Tag { get; }

    public AggregateRaceFinderCacheItem(RaceHorseFinderQueryResult? queryResult, object? tag)
    {
      this.QueryResult = queryResult;
      this.Tag = tag;
    }
  }
}
