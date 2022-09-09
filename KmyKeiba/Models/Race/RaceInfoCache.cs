using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Race.AnalysisTable;
using KmyKeiba.Models.Race.Finder;
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
        //cache.AnalysisTable = exists.AnalysisTable;
        //cache.Finder = exists.Finder;
        cache.HorseDetails = exists.HorseDetails;
        _caches.Remove(exists);
      }
      _caches.Add(cache);

      if (_caches.Count > ApplicationConfiguration.Current.Value.RaceInfoCacheMax)
      {
        _caches.RemoveAt(0);
      }
    }

    public static void Register(RaceInfo race,
      IReadOnlyList<(RaceData Race, RaceHorseData Horse)> horseAllHistories,
      IReadOnlyList<RaceHorseData> horseHistorySameHorses,
      IReadOnlyList<HorseData> horseDetails,
      IReadOnlyList<TrainingData> trainings,
      IReadOnlyList<WoodtipTrainingData> woodtipTrainings,
      RaceFinder? finder,
      AnalysisTableCache? analysisTable,
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
        if (_caches.Count > ApplicationConfiguration.Current.Value.RaceInfoCacheMax)
        {
          _caches.RemoveAt(0);
        }
      }
      cache.HorseAllHistories = horseAllHistories;
      cache.HorseDetails = horseDetails;
      cache.HorseHistorySameHorses = horseHistorySameHorses;
      cache.Trainings = trainings;
      cache.WoodtipTrainings = woodtipTrainings;
      cache.Refund = refund;
      cache.Finder = finder;
      cache.AnalysisTable = analysisTable;

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

    public static void UpdateCache(string raceKey, AnalysisTableCache? analysisTable)
    {
      var cache = TryGetCache(raceKey);
      if (cache != null)
      {
        cache.AnalysisTable = analysisTable;
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
      if (cache.Finder != null) race.Finder.CopyFrom(cache.Finder);
      foreach (var horse in race.Horses.Join(cache.Horses, r => r.Data.Id, c => c.Data.Id, (r, c) => new { New = r, Old = c, }))
      {
        if (horse.Old.RaceRiderAnalyzers != null && horse.New.Data.RiderCode == horse.Old.Data.RiderCode)
          horse.New.RiderTrendAnalyzers?.CopyFrom(horse.Old.RaceRiderAnalyzers);
        if (horse.Old.RaceTrainerAnalyzers != null)
          horse.New.TrainerTrendAnalyzers?.CopyFrom(horse.Old.RaceTrainerAnalyzers);
        if (horse.Old.RaceHorseBloodAnalyzers != null)
          horse.New.BloodSelectors?.CopyFrom(horse.Old.RaceHorseBloodAnalyzers);
        if (horse.Old.Finder != null)
          horse.New.FinderModel.Value?.ReplaceFrom(horse.Old.Finder);
      }

      return true;
    }

    public static void Remove(string raceKey)
    {
      var exists = TryGetCache(raceKey);
      if (exists != null)
      {
        _caches.Remove(exists);
      }
    }
  }

  internal class RaceInfoCache
  {
    public RaceData Data { get; }

    public RaceTrendAnalysisSelector? RaceAnalyzers { get; set; }

    public RaceWinnerHorseTrendAnalysisSelector? RaceWinnerAnalyzers { get; set; }

    public RaceFinder? Finder { get; set; }

    public AnalysisTableCache? AnalysisTable { get; set; }

    public IReadOnlyList<(RaceData Race, RaceHorseData RaceHorse)>? HorseAllHistories { get; set; }

    public IReadOnlyList<RaceHorseData>? HorseHistorySameHorses { get; set; }

    public IReadOnlyList<HorseData>? HorseDetails { get; set; }

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
          RaceRiderAnalyzers = horse.RiderTrendAnalyzers,
          RaceTrainerAnalyzers = horse.TrainerTrendAnalyzers,
          RaceHorseBloodAnalyzers = horse.BloodSelectors,
          Finder = horse.FinderModel.Value,
        });
      }
    }

    public class RaceInfoHorseCache
    {
      public RaceHorseData Data { get; }

      public RaceRiderTrendAnalysisSelector? RaceRiderAnalyzers { get; set; }

      public RaceTrainerTrendAnalysisSelector? RaceTrainerAnalyzers { get; set; }

      public RaceHorseBloodTrendAnalysisSelectorMenu? RaceHorseBloodAnalyzers { get; set; }

      public FinderModel? Finder { get; set; }

      public RaceInfoHorseCache(RaceHorseData horse)
      {
        this.Data = horse;
      }
    }
  }
}
