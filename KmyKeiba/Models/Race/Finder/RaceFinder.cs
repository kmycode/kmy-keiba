using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Analysis.Generic;
using KmyKeiba.Models.Analysis.Math;
using KmyKeiba.Models.Connection;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race.Memo;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.Finder
{
  public class RaceFinder : IDisposable
  {
    private Dictionary<string, (int, RaceHorseFinderQueryResult)> _raceHorseCaches = new();
    private Dictionary<string, (int, IReadOnlyList<RaceAnalyzer>)> _raceCaches = new();

    public string Name => this.Subject.DisplayName;

    public RaceData? Race { get; }

    public RaceSubjectInfo Subject { get; }

    public RaceHorseData? RaceHorse { get; }

    public RaceHorseAnalyzer? RaceHorseAnalyzer { get; }

    public RaceFinder(RaceData? race = null, RaceHorseData? raceHorse = null)
    {
      this.Race = race;
      if (race != null)
      {
        this.Subject = new RaceSubjectInfo(race);
      }
      else
      {
        this.Subject = new RaceSubjectInfo(new());
      }
      this.RaceHorse = raceHorse;
    }

    public RaceFinder(RaceHorseAnalyzer horse) : this(horse.Race, horse.Data)
    {
      this.RaceHorseAnalyzer = horse;
    }

    public static RaceFinder CopyFrom(RaceFinder old, RaceData? race = null, RaceHorseData? horse = null, RaceHorseAnalyzer? analyzer = null)
    {
      var finder = analyzer == null ? new RaceFinder(race, horse) : new RaceFinder(analyzer);
      finder._raceHorseCaches = old._raceHorseCaches;
      finder._raceCaches = old._raceCaches;
      return finder;
    }

    public async Task<RaceHorseFinderQueryResult> FindRaceHorsesAsync(string keys, int sizeMax, int offset = 0, bool isLoadSameHorses = false, bool withoutFutureRaces = true, bool withoutFutureRacesForce = false)
    {
      using var db = new MyContext();
      var reader = new ScriptKeysReader(keys);

      var raceQueries = reader.GetQueries(this.Race, this.RaceHorse, this.RaceHorseAnalyzer);
      if (raceQueries.Limit != default) sizeMax = raceQueries.Limit;
      if (raceQueries.Offset != default) offset = raceQueries.Offset;
      if (raceQueries.IsContainsFutureRaces && !withoutFutureRacesForce) withoutFutureRaces = false;
      if (raceQueries.IsExpandedResult) isLoadSameHorses = true;

      IQueryable<RaceData> races = db.Races!;
      var horses = (IQueryable<RaceHorseData>)db.RaceHorses!;
      if (raceQueries.IsCurrentRaceOnly)
      {
        if (this.Race != null)
          races = races.Where(r => r.Key == this.Race.Key);
        if (this.RaceHorse != null)
          horses = horses.Where(h => h.Key == this.RaceHorse.Key);
      }
      else if (withoutFutureRaces)
      {
        // キャッシュはここで返す
        if (withoutFutureRaces && !raceQueries.IsRealtimeResult &&
          this._raceHorseCaches.TryGetValue(keys, out var cache) && cache.Item1 >= sizeMax)
        {
          return cache.Item2;
        }

        if (this.Race != null)
        {
          races = races.Where(r => r.StartTime < this.Race.StartTime);
        }
        else
        {
          races = races.Where(r => r.StartTime < DateTime.Now);
        }
      }

      foreach (var q in raceQueries.Queries.Where(q => q is not DropoutScriptKeyQuery))
      {
        races = q.Apply(db, races);
        horses = q.Apply(db, horses);
      }

      var query = horses
        .Join(races, rh => rh.RaceKey, r => r.Key, (rh, r) => new { RaceHorse = rh, Race = r, });

      var racesDataQuery = query
        .OrderByDescending(r => r.Race.StartTime)
        .Skip(offset);

      // Distinct自体に非常に時間がかかる（時間が使わないときの１００倍以上になることも）ので、特別にTakeしてからDistinctする
      // 件数に対して結果が少なくなる可能性はあるが仕方ない
      if (raceQueries.Queries.OfType<HorseBeforeRacesCountScriptKeyQuery>().Any())
      {
        var countQueries = raceQueries.Queries.OfType<HorseBeforeRacesCountScriptKeyQuery>();
        var size = 1;
        foreach (var q in countQueries.Where(q => q.Count >= 1))
        {
          size *= q.Count;
        }
        if (countQueries.Any(q => q.Rule == HorseBeforeRacesCountScriptKeyQuery.RaceCountComparationRule.MorePast))
        {
          size = Math.Max(100, size);  // この100は、１頭の馬の平均前走数が目安（地方では100以上走る馬が多い）ハルウララ大好き
        }
        racesDataQuery = racesDataQuery.Take(sizeMax * size).Distinct();
      }

      var racesData = await racesDataQuery
        .Take(sizeMax)
        .ToArrayAsync();

      // ドロップアウトの条件にマッチするか確認する
      foreach (var q in raceQueries.Queries.Where(q => q is DropoutScriptKeyQuery))
      {
        var filtered = q.Apply(db, racesData.Select(r => r.RaceHorse));
        if (!filtered.Any())
        {
          T[] GetAnonymousClassArray<T>(IEnumerable<T> test)
          {
            return Array.Empty<T>();
          }

          racesData = GetAnonymousClassArray(racesData);
        }
      }

      var raceKeys = racesData.Select(r => r.Race.Key).Distinct().ToArray();
      var raceHorsesData = Array.Empty<RaceHorseData>();
      if (isLoadSameHorses)
      {
        raceHorsesData = await db.RaceHorses!
          .Where(rh => rh.ResultOrder >= 1 && rh.ResultOrder <= 3 && raceKeys.Contains(rh.RaceKey))
          .ToArrayAsync();
      }
      var refunds = await db.Refunds!.Where(r => raceKeys.Contains(r.RaceKey)).ToArrayAsync();

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

      var result = new RaceHorseFinderQueryResult(list, raceQueries.GroupKey, raceQueries.MemoGroupInfo, refunds)
      {
        IsExpandedResult = raceQueries.IsExpandedResult,
      };
      if (withoutFutureRaces && !raceQueries.IsRealtimeResult)
      {
        this._raceHorseCaches[keys] = (sizeMax, result);
      }

      return result;
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

    public bool HasRaceHorseCache(string keys)
    {
      return this.TryFindRaceHorseCache(keys) != null;
    }

    public void ClearCache()
    {
      foreach (var disposable in this._raceCaches
        .SelectMany(c => c.Value.Item2.Cast<IDisposable>()))
      //.Concat(this._raceHorseCaches.SelectMany(c => c.Value.Item2.Cast<IDisposable>())))
      {
        disposable.Dispose();
      }
      foreach (var disposable in this._raceHorseCaches
        .SelectMany(c => c.Value.Item2.Items)
        .Cast<IDisposable>())
      {
        disposable.Dispose();
      }

      this._raceHorseCaches.Clear();
      this._raceCaches.Clear();
    }

    public async Task<FinderQueryResult<RaceAnalyzer>> FindRacesAsync(string keys, int sizeMax, int offset = 0, bool withoutFutureRaces = true, bool withoutFutureRacesForce = false)
    {
      if (withoutFutureRaces && this._raceCaches.TryGetValue(keys, out var cache) && cache.Item1 >= sizeMax)
      {
        return new FinderQueryResult<RaceAnalyzer>(cache.Item2, QueryKey.Unknown, null);
      }

      using var db = new MyContext();
      var reader = new ScriptKeysReader(keys);

      var raceQueries = reader.GetQueries(this.Race);
      if (raceQueries.Limit != default) sizeMax = raceQueries.Limit;
      if (raceQueries.Offset != default) offset = raceQueries.Offset;
      if (raceQueries.IsContainsFutureRaces && !withoutFutureRacesForce) withoutFutureRaces = false;

      IQueryable<RaceData> races = db.Races!;
      if (withoutFutureRaces)
      {
        if (this.Race != null)
        {
          races = races.Where(r => r.StartTime < this.Race.StartTime);
        }
        else
        {
          races = races.Where(r => r.StartTime < DateTime.Now);
        }
      }

      foreach (var q in raceQueries.Queries)
      {
        races = q.Apply(db, races);
      }

      var racesData = await races
        .OrderByDescending(r => r.StartTime)
        .Skip(offset)
        .Take(sizeMax)
        .ToArrayAsync();

      var list = new List<RaceAnalyzer>();
      foreach (var race in racesData)
      {
        list.Add(new RaceAnalyzer(race, Array.Empty<RaceHorseData>(), await AnalysisUtil.GetRaceStandardTimeAsync(db, race)));
      }
      if (withoutFutureRaces && !raceQueries.IsRealtimeResult)
      {
        this._raceCaches[keys] = (sizeMax, list);
      }

      return new FinderQueryResult<RaceAnalyzer>(list, raceQueries.GroupKey, raceQueries.MemoGroupInfo);
    }

    public RaceHorseTrendAnalysisSelectorWrapper AsTrendAnalysisSelector()
    {
      return new RaceHorseTrendAnalysisSelectorWrapper(this);
    }

    public void ReplaceFrom(RaceFinder other)
    {
      this.ClearCache();

      this._raceCaches = other._raceCaches;
      //this._raceHorseCaches = other._raceHorseCaches;
    }

    public void Dispose()
    {
      this.ClearCache();
    }
  }

  public class FinderQueryResult<T>
  {
    public IReadOnlyList<T> Items { get; }

    public QueryKey GroupKey { get; }

    internal ScriptKeysMemoGroupInfo? GroupInfo { get; }

    internal FinderQueryResult(IReadOnlyList<T> items, QueryKey group, ScriptKeysMemoGroupInfo? groupInfo)
    {
      this.Items = items;
      this.GroupKey = group;
      this.GroupInfo = groupInfo;
    }
  }

  public class RaceHorseFinderQueryResult : FinderQueryResult<RaceHorseAnalyzer>
  {
    private readonly IReadOnlyList<RefundData> _refunds;
    private RaceHorseFinderResultAnalyzer? _analyzer;
    private RaceHorseFinderResultAnalyzerSlim? _analyzerSlim;

    public RaceHorseFinderResultAnalyzer Analyzer => this._analyzer ??= new RaceHorseFinderResultAnalyzer(this.AsItems(), this._analyzerSlim);

    public RaceHorseFinderResultAnalyzerSlim AnalyzerSlim => this._analyzerSlim ??= new RaceHorseFinderResultAnalyzerSlim(this.AsItems());

    public bool IsExpandedResult { get; init; }

    internal RaceHorseFinderQueryResult(
      IReadOnlyList<RaceHorseAnalyzer> items, QueryKey group, ScriptKeysMemoGroupInfo? groupInfo,
      IReadOnlyList<RefundData> refunds)
      : base(items, group, groupInfo)
    {
      this._refunds = refunds;
    }

    public IReadOnlyList<FinderRaceHorseItem> AsItems()
    {
      return this.Items
        .GroupJoin(this._refunds, i => i.Race.Key, r => r.RaceKey, (i, rs) => new { Analyzer = i, Refund = rs.FirstOrDefault(), })
        .Select(d => new FinderRaceHorseItem(d.Analyzer, d.Refund))
        .ToArray();
    }
  }

  public class RaceHorseFinderResultAnalyzerSlim
  {
    public double DisturbanceRate { get; }

    public double TimeDeviationValue { get; }

    public double A3HTimeDeviationValue { get; }

    public double UntilA3HTimeDeviationValue { get; }

    public double RecoveryRate { get; }

    public ResultOrderGradeMap AllGrade { get; }

    public RaceHorseFinderResultAnalyzerSlim(IReadOnlyList<FinderRaceHorseItem> source) : this(source, null)
    {
    }

    protected RaceHorseFinderResultAnalyzerSlim(IReadOnlyList<FinderRaceHorseItem> source, RaceHorseFinderResultAnalyzerSlim? other)
    {
      if (other != null)
      {
        this.DisturbanceRate = other.DisturbanceRate;
        this.TimeDeviationValue = other.TimeDeviationValue;
        this.A3HTimeDeviationValue = other.TimeDeviationValue;
        this.UntilA3HTimeDeviationValue = other.TimeDeviationValue;
        this.AllGrade = other.AllGrade;
        this.RecoveryRate = other.RecoveryRate;
      }
      else
      {
        var items = source;

        var sourceItems = items.Select(i => i.Analyzer).ToArray();
        var count = sourceItems.Length;

        if (count > 0)
        {
          // 分析
          this.DisturbanceRate = AnalysisUtil.CalcDisturbanceRate(sourceItems);

          var timePoint = new StatisticSingleArray(sourceItems.Select(h => h.ResultTimeDeviationValue).Where(v => v != default).ToArray());
          var a3htimePoint = new StatisticSingleArray(sourceItems.Select(h => h.A3HResultTimeDeviationValue).Where(v => v != default).ToArray());
          var ua3htimePoint = new StatisticSingleArray(sourceItems.Select(h => h.UntilA3HResultTimeDeviationValue).Where(v => v != default).ToArray());
          this.TimeDeviationValue = timePoint.Median;
          this.A3HTimeDeviationValue = a3htimePoint.Median;
          this.UntilA3HTimeDeviationValue = ua3htimePoint.Median;

          var validRaces = sourceItems.Where(r => r.Data.ResultOrder != 0);
          var sourceArr = sourceItems.Select(s => s.Data).ToArray();
          this.AllGrade = new ResultOrderGradeMap(sourceItems);

          // 回収率
          if (sourceItems.Any(s => s.Data.ResultOrder == 1))
          {
            this.RecoveryRate = sourceItems.Where(s => s.Data.ResultOrder == 1).Sum(s => s.Data.Odds * 10) / (float)(sourceItems.Count(i => i.Data.ResultOrder > 0) * 100);
          }
        }
      }
    }
  }

  public class RaceHorseFinderResultAnalyzer : RaceHorseFinderResultAnalyzerSlim
  {
    public ValueComparation RecoveryRateComparation { get; }

    public double PlaceBetsRecoveryRate { get; }

    public ValueComparation PlaceBetsRRComparation { get; }

    public double FrameRecoveryRate { get; }

    public ValueComparation FrameRRComparation { get; }

    public double QuinellaPlaceRecoveryRate { get; }

    public ValueComparation QuinellaPlaceRRComparation { get; }

    public double QuinellaRecoveryRate { get; }

    public ValueComparation QuinellaRRComparation { get; }

    public double ExactaRecoveryRate { get; }

    public ValueComparation ExactaRRComparation { get; }

    public double TrioRecoveryRate { get; }

    public ValueComparation TrioRRComparation { get; }

    public double TrifectaRecoveryRate { get; }

    public ValueComparation TrifectaRRComparation { get; }

    public RaceHorseFinderResultAnalyzer(RaceHorseFinderQueryResult source) : this(source.AsItems())
    {
    }

    public RaceHorseFinderResultAnalyzer(IReadOnlyList<FinderRaceHorseItem> source) : this(source, null)
    {
    }

    public RaceHorseFinderResultAnalyzer(IReadOnlyList<FinderRaceHorseItem> source, RaceHorseFinderResultAnalyzerSlim? slim) : base(source, slim)
    {
      var items = source;

      var sourceItems = items.Select(i => i.Analyzer).ToArray();
      var count = sourceItems.Length;

      if (count > 0)
      {
        // 各種馬券回収率
        var targets = items.Where(s => s.Analyzer.Data.AbnormalResult == RaceAbnormality.Unknown &&
          s.Analyzer.Race.DataStatus != RaceDataStatus.Canceled && s.Analyzer.Race.DataStatus != RaceDataStatus.Delete &&
          s.Analyzer.Data.ResultOrder > 0).ToArray();
        if (targets.Any())
        {
          var won = targets.Where(s => s.Analyzer.Data.ResultOrder == 1 && s.Analyzer.Data.Odds != default);
          Func<RaceData, short> horsesCount = g => g.ResultHorsesCount > 0 ? g.ResultHorsesCount : g.HorsesCount;

          // 複数馬の絡むものは全流しで
          this.PlaceBetsRecoveryRate = targets.Sum(g => g.PlaceBetsPayoff) / ((double)targets.Length * 100);
          this.FrameRecoveryRate = targets.Sum(g => g.FramePayoff) /
            (double)(items.Sum(g => Math.Min(horsesCount(g.Analyzer.Race), (short)8) * 100));
          this.QuinellaPlaceRecoveryRate = targets.Sum(g => g.QuinellaPlacePayoff) /
            (double)(items.Sum(g => (horsesCount(g.Analyzer.Race) - 1) * 100));
          this.QuinellaRecoveryRate = targets.Sum(g => g.QuinellaPayoff) /
            (double)(items.Sum(g => (horsesCount(g.Analyzer.Race) - 1) * 100));
          this.ExactaRecoveryRate = targets.Sum(g => (long)g.ExactaPayoff) /
            (double)(items.Sum(g => (long)(horsesCount(g.Analyzer.Race) - 1) * 100 * 2));
          this.TrioRecoveryRate = targets.Sum(g => (long)g.TrioPayoff) /
            (double)(items.Sum(g => (long)(horsesCount(g.Analyzer.Race) - 1) * (horsesCount(g.Analyzer.Race) - 2) / 2 * 100));
          this.TrifectaRecoveryRate = targets.Sum(g => (long)g.TrifectaPayoff) /
            (double)(items.Sum(g => (long)(horsesCount(g.Analyzer.Race) - 1) * (horsesCount(g.Analyzer.Race) - 2) * 100 * 3));

          this.RecoveryRateComparation = AnalysisUtil.CompareValue(this.AllGrade.RecoveryRate, 1, 0.7);
          this.PlaceBetsRRComparation = AnalysisUtil.CompareValue(this.PlaceBetsRecoveryRate, 1, 0.7);
          this.FrameRRComparation = AnalysisUtil.CompareValue(this.FrameRecoveryRate, 0.9, 0.6);
          this.QuinellaPlaceRRComparation = AnalysisUtil.CompareValue(this.QuinellaPlaceRecoveryRate, 0.9, 0.6);
          this.QuinellaRRComparation = AnalysisUtil.CompareValue(this.QuinellaRecoveryRate, 0.9, 0.6);
          this.ExactaRRComparation = AnalysisUtil.CompareValue(this.ExactaRecoveryRate, 0.9, 0.6);
          this.TrioRRComparation = AnalysisUtil.CompareValue(this.TrioRecoveryRate, 0.85, 0.5);
          this.TrifectaRRComparation = AnalysisUtil.CompareValue(this.TrifectaRecoveryRate, 0.85, 0.5);
        }
      }
    }
  }
}
