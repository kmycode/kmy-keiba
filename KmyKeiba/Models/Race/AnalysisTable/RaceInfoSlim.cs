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

    private RaceInfoSlim(RaceData race)
    {
      this.Data = race;
    }

    public void Dispose()
    {
      this.AnalysisTable.Value.Dispose();
      foreach (var horse in this.Horses)
      {
        horse.Dispose();
      }
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
