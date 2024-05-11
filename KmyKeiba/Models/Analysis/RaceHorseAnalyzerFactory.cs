using CefSharp.DevTools.CacheStorage;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race;
using KmyKeiba.Models.Race.Finder;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static KmyKeiba.JVLink.Entities.HorseWeight;

namespace KmyKeiba.Models.Analysis
{
  internal class RaceHorseAnalyzerRaceListFactory
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public string? RaceKey { get; }

    public bool IsJrdbData { get; set; }

    public bool IsHorseAllHistories { get; set; }

    public bool IsOddsTimeline { get; set; }

    public bool IsDetail { get; set; }

    public bool IsHorseHistorySameHorses { get; set; }

    public bool IsComparation { get; set; }

    public List<IDisposable> Disposables { get; } = new List<IDisposable>();

    public IReadOnlyList<(RaceData, RaceHorseData)> HorseAllHistories { get; private set; }
      = Array.Empty<(RaceData, RaceHorseData)>();

    public IReadOnlyList<RaceHorseData> HorseHistorySameRaceHorses { get; private set; }
      = Array.Empty<RaceHorseData>();

    public IReadOnlyList<HorseData> HorseDetails { get; private set; }
      = Array.Empty<HorseData>();

    public IReadOnlyList<HorseSaleData> HorseSales { get; private set; }
      = Array.Empty<HorseSaleData>();

    public RaceHorseAnalyzerRaceListFactory(string raceKey)
    {
      this.RaceKey = raceKey;
    }

    public async Task<IReadOnlyList<RaceHorseAnalyzer>> ToAnalyzerAsync(MyContext db, RaceInfoCache? cache = null)
    {
      var race = await db.Races!.FirstOrDefaultAsync(r => r.Key == this.RaceKey);
      if (race == null)
      {
        return Array.Empty<RaceHorseAnalyzer>();
      }

      var horses = await db.RaceHorses!.Where(rh => rh.RaceKey == this.RaceKey).ToArrayAsync();
      var standardTime = await AnalysisUtil.GetRaceStandardTimeAsync(db, race);

      IReadOnlyList<JrdbRaceHorseData> jrdbHorses = Array.Empty<JrdbRaceHorseData>();
      if (this.IsJrdbData)
      {
        jrdbHorses = await db.JrdbRaceHorses!.Where(j => j.RaceKey == this.RaceKey).ToArrayAsync();
      }
      logger.Info($"馬の数: {horses.Length}, レース情報に記録されている馬の数: {horses.Length}, JRDB: {jrdbHorses.Count}");

      var horseKeys = horses.Select(h => h.Key).ToArray();

      var horseAllHistories = cache?.HorseAllHistories;
      if (horseAllHistories == null)
      {
        if (this.IsHorseAllHistories)
        {
          horseAllHistories = (await db.RaceHorses!
            .Where(rh => horseKeys.Contains(rh.Key))
            .Where(rh => rh.Key != "0000000000")
            .Join(db.Races!, rh => rh.RaceKey, r => r.Key, (rh, r) => new { Race = r, RaceHorse = rh, })
            .Where(d => d.Race.StartTime < race.StartTime)
            .OrderByDescending(d => d.Race.StartTime)
            .ToArrayAsync())
            .Select(d => (d.Race, d.RaceHorse))
            .ToArray();
          logger.Debug($"馬の過去レースの総数: {horseAllHistories.Count}");
        }
        else
        {
          horseAllHistories = Array.Empty<(RaceData, RaceHorseData)>();
        }
      }
      else
      {
        logger.Debug($"馬の過去レースの総数: {horseAllHistories.Count}");
      }

      // 時系列オッズ
      var oddsTimeline = Array.Empty<SingleOddsTimeline>();
      if (this.IsOddsTimeline)
      {
        oddsTimeline = await db.SingleOddsTimelines!.Where(o => o.RaceKey == race.Key).ToArrayAsync();
        logger.Debug($"時系列オッズ {oddsTimeline.Length}件");
      }

      // 各馬の情報
      var horseDetails = cache?.HorseDetails;
      if (horseDetails == null)
      {
        if (this.IsDetail)
        {
          horseDetails = await db.Horses!
            .Where(h => horseKeys.Contains(h.Code))
            .ToArrayAsync();
        }
        else
        {
          horseDetails = Array.Empty<HorseData>();
        }
      }
      var horseSales = cache?.HorseSales;
      if (horseSales == null)
      {
        if (this.IsDetail)
        {
          horseSales = await db.HorseSales!
            .Where(h => horseKeys.Contains(h.Code))
            .ToArrayAsync();
        }
        else
        {
          horseSales = Array.Empty<HorseSaleData>();
        }
      }
      var horseHistorySameHorses = cache?.HorseHistorySameHorses;
      if (horseHistorySameHorses == null)
      {
        if (this.IsHorseHistorySameHorses)
        {
          var horseHistoryKeys = horseAllHistories.Select(h => h.RaceHorse.RaceKey).ToArray();
          horseHistorySameHorses = await db.RaceHorses!
            .Where(h => horseHistoryKeys.Contains(h.RaceKey))
            .Where(h => h.ResultOrder >= 1 && h.ResultOrder <= 5)
            .ToArrayAsync();
        }
        else
        {
          horseHistorySameHorses = Array.Empty<RaceHorseData>();
        }
      }
      logger.Debug($"馬の過去レースの同走馬数: {horseHistorySameHorses.Count}");

      var horseInfos = new List<RaceHorseAnalyzer>();
      foreach (var horse in horses)
      {
        var histories = new List<RaceHorseAnalyzer>();
        foreach (var history in horseAllHistories.Where(h => h.RaceHorse.Key == horse.Key))
        {
          var historyStandardTime = await AnalysisUtil.GetRaceStandardTimeAsync(db, history.Race);
          var sameHorses = horseHistorySameHorses.Where(h => h.RaceKey == history.RaceHorse.RaceKey);
          histories.Add(new RaceHorseAnalyzer(history.Race, history.RaceHorse, sameHorses.ToArray(), historyStandardTime).AddTo(this.Disposables));
        }

        var jrdb = jrdbHorses.FirstOrDefault(j => j.Key == horse.Key);

        var analyzer = new RaceHorseAnalyzer(race, horse, horses, histories, standardTime, jrdbHorse: jrdb)
        {
          BloodSelectors = new RaceHorseBloodModel(race, horse),
          DetailData = horseDetails.FirstOrDefault(h => h.Code == horse.Key),
          SaleData = horseSales.FirstOrDefault(h => h.Code == horse.Key),
        };
        analyzer.SetOddsTimeline(oddsTimeline);
        analyzer.ChangeIsCheck(CheckHorseUtil.IsChecked(horse.Key, HorseCheckType.CheckRace));

        horseInfos.Add(analyzer);
        logger.Debug($"馬 {horse.Name} の情報を登録");
      }

      if (this.IsComparation)
      {
        var sortedHorses = horseInfos.All(h => h.Data.Number == default) ? horseInfos.OrderBy(h => h.Data.Name) : horseInfos.OrderBy(h => h.Data.Number);
        {
          // タイム指数の相対評価
          var timedvMax = horseInfos.Select(i => i.History?.TimeDeviationValue ?? default).Where(v => v != default).OrderByDescending(v => v).Skip(2).FirstOrDefault();
          var timedvMin = horseInfos.Select(i => i.History?.TimeDeviationValue ?? default).Where(v => v != default).OrderBy(v => v).Skip(2).FirstOrDefault();
          var a3htimedvMax = horseInfos.Select(i => i.History?.A3HTimeDeviationValue ?? default).Where(v => v != default).OrderByDescending(v => v).Skip(2).FirstOrDefault();
          var a3htimedvMin = horseInfos.Select(i => i.History?.A3HTimeDeviationValue ?? default).Where(v => v != default).OrderBy(v => v).Skip(2).FirstOrDefault();
          var ua3htimedvMax = horseInfos.Select(i => i.History?.UntilA3HTimeDeviationValue ?? default).Where(v => v != default).OrderByDescending(v => v).Skip(2).FirstOrDefault();
          var ua3htimedvMin = horseInfos.Select(i => i.History?.UntilA3HTimeDeviationValue ?? default).Where(v => v != default).OrderBy(v => v).Skip(2).FirstOrDefault();
          var pciMax = horseInfos.Select(i => i.History?.PciAverage ?? default).Where(v => v != default).OrderByDescending(v => v).Skip(2).FirstOrDefault();
          var pciMin = horseInfos.Select(i => i.History?.PciAverage ?? default).Where(v => v != default).OrderBy(v => v).Skip(2).FirstOrDefault();
          var resultA3hMax = horseInfos.Where(i => !i.IsAbnormalResult).Select(i => i.Data.AfterThirdHalongTime).Where(v => v != default).OrderBy(v => v).Skip(2).FirstOrDefault().TotalSeconds + 0.001;  // 等価比較対策
          var resultA3hMin = horseInfos.Where(i => !i.IsAbnormalResult).Select(i => i.Data.AfterThirdHalongTime).Where(v => v != default).OrderByDescending(v => v).Skip(2).FirstOrDefault().TotalSeconds - 0.001;
          foreach (var horse in horseInfos)
          {
            if (horse.Data.AfterThirdHalongTime != default && !horse.IsAbnormalResult)
            {
              // 書式指定子の「f」は四捨五入してくれないようなので
              // 古いバージョンでダウンロードしたデータには浮動小数点数の除算時のゴミが残っているので、手動で四捨五入する
              var ticks = (int)System.Math.Round(horse.Data.AfterThirdHalongTime.Ticks / 1000000.0) * 1000000;
              horse.Data.AfterThirdHalongTime = TimeSpan.FromTicks(ticks);
              horse.ResultA3HTimeComparation = AnalysisUtil.CompareValue(horse.Data.AfterThirdHalongTime.TotalSeconds, resultA3hMax, resultA3hMin, true);
            }

            if (horse.History != null)
            {
              if (horse.History.BeforeRaces.Where(r => r.Data.ResultTime.TotalSeconds > 0).Take(10)
                .Count(r => r.Race.TrackGround != race.TrackGround || r.Race.TrackType != race.TrackType || System.Math.Abs(r.Race.Distance - race.Distance) >= 400) >= 4)
              {
                // 条件の大きく異なるレース
                horse.History.TimeDVComparation = horse.History.A3HTimeDVComparation = horse.History.UntilA3HTimeDVComparation = horse.History.PciAverageComparation = ValueComparation.Warning;
              }
              else
              {
                horse.History.TimeDVComparation = horse.History.TimeDeviationValue + 0.5 >= timedvMax ? ValueComparation.Good :
                  horse.History.TimeDeviationValue - 0.5 <= timedvMin ? ValueComparation.Bad : ValueComparation.Standard;
                horse.History.A3HTimeDVComparation = horse.History.A3HTimeDeviationValue + 0.5 >= a3htimedvMax ? ValueComparation.Good :
                  horse.History.A3HTimeDeviationValue - 0.5 <= a3htimedvMin ? ValueComparation.Bad : ValueComparation.Standard;
                horse.History.UntilA3HTimeDVComparation = horse.History.UntilA3HTimeDeviationValue + 0.5 >= ua3htimedvMax ? ValueComparation.Good :
                  horse.History.UntilA3HTimeDeviationValue - 0.5 <= ua3htimedvMin ? ValueComparation.Bad : ValueComparation.Standard;
                horse.History.PciAverageComparation = AnalysisUtil.CompareValue(horse.History.PciAverage, pciMin, pciMax, true);
              }
            }

            horse.FinderModel.Value = new FinderModel(race, horse, sortedHorses);
          }
        }
        logger.Debug("馬のタイム指数相対評価を設定");
      }

      this.HorseAllHistories = horseAllHistories;
      this.HorseHistorySameRaceHorses = horseHistorySameHorses;
      this.HorseDetails = horseDetails;
      this.HorseSales = horseSales;

      return horseInfos;
    }
  }
}
