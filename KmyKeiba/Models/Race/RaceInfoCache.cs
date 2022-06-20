﻿using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race
{
  internal static class RaceInfoCacheManager
  {
    private static readonly List<RaceInfoCache> _caches = new();

    public static void Register(RaceInfo race)
    {
      var cache = new RaceInfoCache(race);

      // 既存のものをDisposeする必要はない（すでにされているので）
      var exists = TryGetCache(race.Data.Key);
      if (exists != null)
      {
        cache.HorseAllHistories = exists.HorseAllHistories;
        cache.HorseHistorySameHorses = exists.HorseHistorySameHorses;
        cache.Trainings = exists.Trainings;
        cache.WoodtipTrainings = exists.WoodtipTrainings;
        cache.Refund = exists.Refund;
        cache.FrameNumberOdds = exists.FrameNumberOdds;
        cache.QuinellaPlaceOdds = exists.QuinellaPlaceOdds;
        cache.QuinellaOdds = exists.QuinellaOdds;
        cache.ExactaOdds = exists.ExactaOdds;
        cache.TrioOdds = exists.TrioOdds;
        cache.TrifectaOdds = exists.TrifectaOdds;
        _caches.Remove(exists);
      }
      _caches.Add(cache);

      if (_caches.Count > 48)
      {
        _caches.RemoveAt(0);
      }
    }

    public static void Register(RaceInfo race,
      IReadOnlyList<(RaceData Race, RaceHorseData Horse)> horseAllHistories,
      IReadOnlyList<RaceHorseData> horseHistorySameHorses,
      IReadOnlyList<TrainingData> trainings,
      IReadOnlyList<WoodtipTrainingData> woodtipTrainings,
      RefundData? refund,
      FrameNumberOddsData? frameNumberOdds,
      QuinellaPlaceOddsData? quinellaPlaceOdds,
      QuinellaOddsData? quinellaOdds,
      ExactaOddsData? exactaOdds,
      TrioOddsData? trioOdds,
      TrifectaOddsData? trifectaOdds)
    {
      // 既存のものをDisposeする必要はない（すでにされているので）
      var cache = TryGetCache(race.Data.Key);
      if (cache == null)
      {
        cache = new RaceInfoCache(race);
        _caches.Add(cache);
        if (_caches.Count > 48)
        {
          _caches.RemoveAt(0);
        }
      }
      cache.HorseAllHistories = horseAllHistories;
      cache.HorseHistorySameHorses = horseHistorySameHorses;
      cache.Trainings = trainings;
      cache.WoodtipTrainings = woodtipTrainings;
      cache.Refund = refund;

      if (refund != null)
      {
        cache.FrameNumberOdds = frameNumberOdds;
        cache.QuinellaPlaceOdds = quinellaPlaceOdds;
        cache.QuinellaOdds = quinellaOdds;
        cache.ExactaOdds = exactaOdds;
        cache.TrioOdds = trioOdds;
        cache.TrifectaOdds = trifectaOdds;
      }
    }

    public static RaceInfoCache? TryGetCache(string raceKey)
    {
      var exists = _caches.FirstOrDefault(c => c.Data.Key == raceKey);
      return exists;
    }

    public static bool TryApplyTrendAnalyzers(RaceInfo race)
    {
      var cache = TryGetCache(race.Data.Key);
      if (cache == null)
      {
        return false;
      }

      if (RaceInfo.IsWillResetTrendAnalyzersDataOnUpdate(cache.Data, race.Data))
      {
        _caches.Remove(cache);
        return false;
      }

      if (cache.RaceAnalyzers != null) race.TrendAnalyzers.CopyFrom(cache.RaceAnalyzers);
      if (cache.RaceWinnerAnalyzers != null) race.WinnerTrendAnalyzers.CopyFrom(cache.RaceWinnerAnalyzers);
      foreach (var horse in race.Horses.Join(cache.Horses, r => r.Data.Id, c => c.Data.Id, (r, c) => new { New = r, Old = c, }))
      {
        if (horse.Old.RaceHorseAnalyzers != null && horse.New.Data.RiderCode == horse.Old.Data.RiderCode)
          horse.New.TrendAnalyzers?.CopyFrom(horse.Old.RaceHorseAnalyzers);
        if (horse.Old.RaceRiderAnalyzers != null && horse.New.Data.RiderCode == horse.Old.Data.RiderCode)
          horse.New.RiderTrendAnalyzers?.CopyFrom(horse.Old.RaceRiderAnalyzers);
        if (horse.Old.RaceTrainerAnalyzers != null)
          horse.New.TrainerTrendAnalyzers?.CopyFrom(horse.Old.RaceTrainerAnalyzers);
        if (horse.Old.RaceHorseBloodAnalyzers != null)
          horse.New.BloodSelectors?.CopyFrom(horse.Old.RaceHorseBloodAnalyzers);
      }

      return true;
    }
  }

  internal class RaceInfoCache
  {
    public RaceData Data { get; }

    public RaceTrendAnalysisSelector? RaceAnalyzers { get; set; }

    public RaceWinnerHorseTrendAnalysisSelector? RaceWinnerAnalyzers { get; set; }

    public IReadOnlyList<(RaceData Race, RaceHorseData RaceHorse)>? HorseAllHistories { get; set; }

    public IReadOnlyList<RaceHorseData>? HorseHistorySameHorses { get; set; }

    public IReadOnlyList<TrainingData>? Trainings { get; set; }

    public IReadOnlyList<WoodtipTrainingData>? WoodtipTrainings { get; set; }

    public FrameNumberOddsData? FrameNumberOdds { get; set; }

    public QuinellaPlaceOddsData? QuinellaPlaceOdds { get; set; }

    public QuinellaOddsData? QuinellaOdds { get; set; }

    public ExactaOddsData? ExactaOdds { get; set; }

    public TrioOddsData? TrioOdds { get; set; }

    public TrifectaOddsData? TrifectaOdds { get; set; }

    public RefundData? Refund { get; set; }

    public List<RaceInfoHorseCache> Horses { get; set; } = new();

    public RaceInfoCache(RaceInfo race)
    {
      this.Data = race.Data;

      this.RaceAnalyzers = race.TrendAnalyzers;
      this.RaceWinnerAnalyzers = race.WinnerTrendAnalyzers;
      foreach (var horse in race.Horses)
      {
        this.Horses.Add(new RaceInfoHorseCache(horse.Data)
        {
          RaceHorseAnalyzers = horse.TrendAnalyzers,
          RaceRiderAnalyzers = horse.RiderTrendAnalyzers,
          RaceTrainerAnalyzers = horse.TrainerTrendAnalyzers,
          RaceHorseBloodAnalyzers = horse.BloodSelectors,
        });
      }
    }

    public class RaceInfoHorseCache
    {
      public RaceHorseData Data { get; }

      public RaceHorseTrendAnalysisSelector? RaceHorseAnalyzers { get; set; }

      public RaceRiderTrendAnalysisSelector? RaceRiderAnalyzers { get; set; }

      public RaceTrainerTrendAnalysisSelector? RaceTrainerAnalyzers { get; set; }

      public RaceHorseBloodTrendAnalysisSelectorMenu? RaceHorseBloodAnalyzers { get; set; }

      public RaceInfoHorseCache(RaceHorseData horse)
      {
        this.Data = horse;
      }
    }
  }
}