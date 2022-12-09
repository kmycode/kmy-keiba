using CefSharp.DevTools.CacheStorage;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using static KmyKeiba.JVLink.Entities.HorseWeight;

namespace KmyKeiba.Models.Race.AnalysisTable
{
  public class RaceInfoSlim : IDisposable
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    private readonly CompositeDisposable _disposables = new();

    public RaceData Data { get; }

    public ReactiveCollection<RaceHorseAnalyzer> Horses { get; } = new();

    public ReactiveProperty<bool> HasResults { get; } = new();

    public ReactiveProperty<AnalysisTable.AnalysisTableModel> AnalysisTable { get; } = new();

    private OddsInfo? _odds;
    private PayoffInfo? _payoff;

    private RaceInfoSlim(RaceData race)
    {
      this.Data = race;
    }

    public async Task<OddsInfo> GetOddsInfoAsync()
    {
      if (this._odds == null)
      {
        using var db = new MyContext();
        var frameOdds = await db.FrameNumberOdds!.FirstOrDefaultAsync(o => o.RaceKey == this.Data.Key);
        var quinellaPlaceOdds = await db.QuinellaPlaceOdds!.FirstOrDefaultAsync(o => o.RaceKey == this.Data.Key);
        var quinellaOdds = await db.QuinellaOdds!.FirstOrDefaultAsync(o => o.RaceKey == this.Data.Key);
        var exactaOdds = await db.ExactaOdds!.FirstOrDefaultAsync(o => o.RaceKey == this.Data.Key);
        var trioOdds = await db.TrioOdds!.FirstOrDefaultAsync(o => o.RaceKey == this.Data.Key);
        var trifectaOdds = await db.TrifectaOdds!.FirstOrDefaultAsync(o => o.RaceKey == this.Data.Key);
        this._odds = new OddsInfo(this.Horses.Select(h => h.Data).ToArray(), frameOdds, quinellaPlaceOdds, quinellaOdds, exactaOdds, trioOdds, trifectaOdds);
      }

      return this._odds;
    }

    public async Task<PayoffInfo?> GetPayoffInfoAsync()
    {
      if (this._payoff == null)
      {
        using var db = new MyContext();
        var payoff = await db.Refunds!.FirstOrDefaultAsync(r => r.RaceKey == this.Data.Key);
        if (payoff != null)
        {
          this._payoff = new PayoffInfo(payoff);
        }
      }

      return this._payoff;
    }

    public void Dispose()
    {
      this.AnalysisTable.Value.Dispose();
      foreach (var horse in this.Horses)
      {
        horse.Dispose();
      }
      this._payoff?.Dispose();
      this._odds?.Dispose();
      this._disposables.Dispose();
    }

    public static async Task<RaceInfoSlim?> FromKeyAsync(string key)
    {
      logger.Info($"{key} のレースを読み込みます (Slim)");

      using var db = new MyContext();

      var race = await db.Races!.FirstOrDefaultAsync(r => r.Key == key);
      if (race == null)
      {
        logger.Warn("レースがDBから見つかりませんでした");
        return null;
      }
      var info = new RaceInfoSlim(race);

      var factory = new RaceHorseAnalyzerRaceListFactory(race.Key)
      {
        IsDetail = true,
        IsHorseAllHistories = true,
        IsJrdbData = true,
      };
      var horseInfos = await factory.ToAnalyzerAsync(db, null);
      if (horseInfos == null)
      {
        throw new InvalidOperationException();
      }
      foreach (var obj in factory.Disposables)
      {
        info._disposables.Add(obj);
      }

      foreach (var horse in horseInfos)
      {
        info.Horses.Add(horse);
      }
      info.AnalysisTable.Value = new AnalysisTableModel(race, info.Horses, null);
      info.HasResults.Value = info.Horses.Any(h => h.Data.ResultOrder > 0);
      return info;
    }
  }
}
