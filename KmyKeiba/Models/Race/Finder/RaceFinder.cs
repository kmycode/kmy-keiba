using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Analysis.Generic;
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
  public class RaceFinder
  {
    // 外部指数の更新に対応できない＆検索速度が改善傾向にある＆キャッシュを使ったところで速度向上の実感がないので、
    // キャッシュはとらないか別途慎重に検討する
    //private Dictionary<string, (int, IReadOnlyList<RaceHorseAnalyzer>)> _raceHorseCaches = new();
    private Dictionary<string, (int, IReadOnlyList<RaceAnalyzer>)> _raceCaches = new();

    public string Name => this.Subject.DisplayName;

    public RaceData? Race { get; }

    public RaceSubjectInfo Subject { get; }

    public RaceHorseData? RaceHorse { get; }

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

    public async Task<RaceHorseFinderQueryResult> FindRaceHorsesAsync(string keys, int sizeMax, int offset = 0, bool isLoadSameHorses = false, bool withoutFutureRaces = true)
    {
      //if (withoutFutureRaces && this._raceHorseCaches.TryGetValue(keys, out var cache) && cache.Item1 >= sizeMax)
      //{
      //  return new FinderQueryResult<RaceHorseAnalyzer>(cache.Item2, QueryKey.Unknown, null);
      //}

      using var db = new MyContext();
      var reader = new ScriptKeysReader(keys);

      if (DownloaderModel.Instance.CanSaveOthers.Value)
      {
        try
        {
          //await db.BeginTransactionAsync();
        }
        catch
        {
        }
      }

      IQueryable<RaceData> races = db.Races!;
      if (withoutFutureRaces)
      {
        if (this.Race != null)
        {
          races = races.Where(r => r.StartTime < this.Race.StartTime && r.DataStatus != RaceDataStatus.Canceled && r.TrackType == this.Race.TrackType);
        }
        else
        {
          races = races.Where(r => r.StartTime < DateTime.Now && r.DataStatus != RaceDataStatus.Canceled && r.TrackType == this.Race.TrackType);
        }
      }
      var horses = (IQueryable<RaceHorseData>)db.RaceHorses!;

      var raceQueries = reader.GetQueries(this.Race, this.RaceHorse);
      if (raceQueries.Limit != default) sizeMax = raceQueries.Limit;
      if (raceQueries.Offset != default) offset = raceQueries.Offset;

      foreach (var q in raceQueries.Queries)
      {
        races = q.Apply(db, races);
        horses = q.Apply(db, horses);
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
      var raceKeys = racesData.Select(r => r.Race.Key).Distinct().ToArray();
      var raceHorsesData = Array.Empty<RaceHorseData>();
      if (isLoadSameHorses)
      {
        raceHorsesData = await db.RaceHorses!
          .Where(rh => rh.ResultOrder >= 1 && rh.ResultOrder <= 5 && raceKeys.Contains(rh.RaceKey))
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
      if (withoutFutureRaces && this.IsCache(keys))
      {
      //  this._raceHorseCaches[keys] = (sizeMax, list);
      }

      return new RaceHorseFinderQueryResult(list, raceQueries.GroupKey, raceQueries.MemoGroupInfo, refunds);
    }

    public async Task<FinderQueryResult<RaceAnalyzer>> FindRacesAsync(string keys, int sizeMax, int offset = 0, bool withoutFutureRaces = true)
    {
      if (withoutFutureRaces && this._raceCaches.TryGetValue(keys, out var cache) && cache.Item1 >= sizeMax)
      {
        return new FinderQueryResult<RaceAnalyzer>(cache.Item2, QueryKey.Unknown, null);
      }

      using var db = new MyContext();
      var reader = new ScriptKeysReader(keys);

      IQueryable<RaceData> races = db.Races!;
      if (withoutFutureRaces)
      {
        if (this.Race != null)
        {
          races = races.Where(r => r.StartTime < this.Race.StartTime && r.DataStatus != RaceDataStatus.Canceled && r.TrackType == this.Race.TrackType);
        }
        else
        {
          races = races.Where(r => r.StartTime < DateTime.Now && r.DataStatus != RaceDataStatus.Canceled && r.TrackType == this.Race.TrackType);
        }
      }

      var raceQueries = reader.GetQueries(this.Race);
      if (raceQueries.Limit != default) sizeMax = raceQueries.Limit;
      if (raceQueries.Offset != default) offset = raceQueries.Offset;

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
      if (withoutFutureRaces && this.IsCache(keys))
      {
        this._raceCaches[keys] = (sizeMax, list);
      }

      return new FinderQueryResult<RaceAnalyzer>(list, raceQueries.GroupKey, raceQueries.MemoGroupInfo);
    }

    private bool IsCache(string keys)
    {
      // ここから先は結果が変わりようがないのでキャッシュ対象
      if (this.Race == null || this.Race.DataStatus >= RaceDataStatus.PreliminaryGrade3)
      {
        return true;
      }

      // 単に「popular」とだけ書いてあって条件式が指定されていないものは、現在のオッズを参照するのでキャッシュ不可
      var keysArr = keys.Split('|');
      if (keysArr.Contains("popular") || keysArr.Contains("odds") || keysArr.Contains("placeoddsmin") || keysArr.Contains("placeoddsmax"))
      {
        return false;
      }

      return true;
    }

    public RaceHorseTrendAnalysisSelectorWrapper AsTrendAnalysisSelector()
    {
      return new RaceHorseTrendAnalysisSelectorWrapper(this);
    }

    public void ReplaceFrom(RaceFinder other)
    {
      this.Dispose();

      this._raceCaches = other._raceCaches;
      //this._raceHorseCaches = other._raceHorseCaches;
    }

    public void Dispose()
    {
      foreach (var disposable in this._raceCaches
        .SelectMany(c => c.Value.Item2.Cast<IDisposable>()))
        //.Concat(this._raceHorseCaches.SelectMany(c => c.Value.Item2.Cast<IDisposable>())))
      {
        disposable.Dispose();
      }
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
}
