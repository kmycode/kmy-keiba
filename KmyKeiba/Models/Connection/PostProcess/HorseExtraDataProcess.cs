using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Connection.PostProcess
{
  public class HorseExtraDataProcess : IPostProcessing
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public ProcessingStep Step => ProcessingStep.HorseExtraData;

    public async Task RunAsync()
    {
      if (!DownloadConfig.Instance.IsBuildExtraData.Value) return;

      var state = DownloadStatus.Instance;

      logger.Info($"後処理進捗変更: {Step}");
      await SetHorseExtraTableDataAsync(
        isCanceled: state.IsCancelProcessing,
        progress: state.ProcessingProgress,
        progressMax: state.ProcessingProgressMax
      );
    }

    private static async Task SetHorseExtraTableDataAsync(DateOnly? startDate = null, ReactiveProperty<bool>? isCanceled = null, ReactiveProperty<int>? progress = null, ReactiveProperty<int>? progressMax = null)
    {
      short currentDataVersion = 1;

      try
      {
        using var db = new MyContext();
        var startTime = startDate == null ? DateTime.MinValue : startDate.Value.ToDateTime(TimeOnly.MinValue);

        var horses = db.RaceHorses!
          .Where(rh => rh.ExtraDataVersion < currentDataVersion || (rh.ExtraDataState != HorseExtraDataState.Ignored && rh.ExtraDataState != HorseExtraDataState.AfterRace))
          .Join(db.Races!.Where(r => r.StartTime >= startTime), rh => rh.RaceKey, r => r.Key, (rh, r) => new { RaceHorse = rh, Race = r, })
          .Where(rh => rh.Race.DataStatus <= RaceDataStatus.Canceled)
          .OrderByDescending(rh => rh.Race.StartTime);

        var notyetRaces = horses
          .Where(h => h.RaceHorse.ExtraDataVersion < currentDataVersion || h.RaceHorse.ExtraDataState < HorseExtraDataState.UntilRace)
          .Where(h => h.Race.DataStatus < RaceDataStatus.PreliminaryGradeFull);
        var finishedRaces = horses
          .Where(h => h.RaceHorse.ExtraDataVersion < currentDataVersion || h.RaceHorse.ExtraDataState < HorseExtraDataState.AfterRace)
          .Where(h => h.Race.DataStatus >= RaceDataStatus.PreliminaryGradeFull);

        if (!await notyetRaces.AnyAsync() && !await finishedRaces.AnyAsync())
        {
          return;
        }

        await db.BeginTransactionAsync();

        progress ??= new ReactiveProperty<int>();
        progressMax ??= new ReactiveProperty<int>();
        isCanceled ??= new ReactiveProperty<bool>();

        progress.Value = 0;
        progressMax.Value = await notyetRaces.CountAsync() + await finishedRaces.CountAsync();

        var pciCaches = new Dictionary<string, (short, short)>();

        var lastCommit = 0;
        var state = HorseExtraDataState.UntilRace;
        while (true)
        {
          var buffer = state == HorseExtraDataState.UntilRace ?
            await notyetRaces.Take(192).ToArrayAsync() :
            await finishedRaces.Take(192).ToArrayAsync();
          if (!buffer.Any())
          {
            if (state < HorseExtraDataState.AfterRace)
            {
              state = HorseExtraDataState.AfterRace;
              continue;
            }
            break;
          }

          var raceKeys = buffer.Select(b => b.RaceHorse.RaceKey).ToArray();
          var existingDataCache = await db.RaceHorseExtras!.Where(e => raceKeys.Contains(e.RaceKey)).ToArrayAsync();
          var sameRaceHorsesCache = await db.RaceHorses!
            .Where(rh => raceKeys.Contains(rh.RaceKey) && rh.ResultOrder > 0)
            .Select(rh => new { rh.RaceKey, rh.ResultOrder, rh.AfterThirdHalongTimeValue, rh.ResultTimeValue, })
            .ToArrayAsync();

          var newData = new List<RaceHorseExtraData>();

          foreach (var horse in buffer)
          {
            var data = existingDataCache.FirstOrDefault(e => e.RaceKey == horse.RaceHorse.RaceKey && e.Key == horse.RaceHorse.Key);
            if (data == null)
            {
              data = new RaceHorseExtraData
              {
                RaceKey = horse.RaceHorse.RaceKey,
                Key = horse.RaceHorse.Key,
              };
              newData.Add(data);
            }
            if (horse.RaceHorse.ExtraDataVersion < currentDataVersion)
            {
              horse.RaceHorse.ExtraDataState = HorseExtraDataState.Unset;
            }

            if (horse.RaceHorse.ExtraDataState < HorseExtraDataState.UntilRace)
            {
            }
            if (horse.RaceHorse.ExtraDataState < HorseExtraDataState.AfterRace &&
              horse.Race.DataStatus >= RaceDataStatus.PreliminaryGradeFull)
            {
              if (horse.Race.DataStatus <= RaceDataStatus.Grade)
              {
                var sameRaceHorses = sameRaceHorsesCache
                  .Where(rh => rh.RaceKey == horse.Race.Key && rh.ResultOrder > 0)
                  .Select(rh => new { rh.ResultOrder, rh.AfterThirdHalongTimeValue, rh.ResultTimeValue, })
                  .ToArray();

                // PCI
                try
                {
                  var pci = (short)(AnalysisUtil.CalcPci(horse.Race, horse.RaceHorse) * 100);
                  if (pci == default) pci = -1;
                  data.Pci = pci;
                }
                catch { }

                // PCI3, RPCI
                if (pciCaches.TryGetValue(horse.Race.Key, out var pciCache))
                {
                  data.Pci3 = pciCache.Item1;
                  data.Rpci = pciCache.Item2;
                }
                else
                {
                  double pci3 = default;
                  double rpci = default;

                  if (sameRaceHorses.Any(h => h.ResultOrder <= 3))
                  {
                    pci3 = sameRaceHorses.Where(h => h.ResultOrder <= 3)
                      .Select(h => AnalysisUtil.CalcPci(horse.Race.Distance, h.ResultTimeValue, h.AfterThirdHalongTimeValue) * 100)
                      .Average();
                  }
                  if (sameRaceHorses.Any(h => h.ResultOrder == 1))
                  {
                    var top = sameRaceHorses.First(h => h.ResultOrder == 1);
                    rpci = AnalysisUtil.CalcRpci(horse.Race.Distance, horse.Race.AfterHaronTime3, top.ResultTimeValue, top.AfterThirdHalongTimeValue) * 100;
                  }

                  try
                  {
                    var pci3Value = (short)pci3;
                    var rpciValue = (short)rpci;

                    data.Pci3 = pci3Value;
                    data.Rpci = rpciValue;

                    pciCaches[horse.Race.Key] = (pci3Value, rpciValue);
                  }
                  catch { }
                }

                // 後3ハロンタイム順位
                {
                  var a3hsorted = sameRaceHorses.Where(h => h.AfterThirdHalongTimeValue > 0).OrderBy(h => h.AfterThirdHalongTimeValue);
                  var a3horder = 0;
                  var a3hhit = false;
                  foreach (var item in a3hsorted)
                  {
                    a3horder++;
                    if (item.ResultOrder == horse.RaceHorse.ResultOrder)
                    {
                      a3hhit = true;
                      break;
                    }
                  }
                  if (!a3hhit)
                  {
                    a3horder = -1;
                  }

                  data.After3HaronOrder = (short)a3horder;
                }

                // 調整済前3ハロンタイム
                var b3hFixed = AnalysisUtil.NormalizeB3FTime(horse.Race);
                data.Before3HaronTimeFixed = b3hFixed;

                if (horse.Race.Distance > 600)
                {
                  // ベース部分タイム
                  var baseTime = horse.RaceHorse.ResultTimeValue - horse.RaceHorse.AfterThirdHalongTimeValue;
                  data.BaseTime = (short)baseTime;

                  // ベース部分タイムの3ハロン換算
                  var baseTime3h = (short)((baseTime / (float)(horse.Race.Distance - 600)) * 600);
                  data.BaseTimeAs3Haron = baseTime3h;
                }

                // コーナー順位１２／２３／３４／４結の差
                if (horse.RaceHorse.FirstCornerOrder > 0)
                  data.CornerOrderDiff2 = (short)(horse.RaceHorse.SecondCornerOrder - horse.RaceHorse.FirstCornerOrder);
                if (horse.RaceHorse.SecondCornerOrder > 0)
                  data.CornerOrderDiff3 = (short)(horse.RaceHorse.ThirdCornerOrder - horse.RaceHorse.SecondCornerOrder);
                if (horse.RaceHorse.ThirdCornerOrder > 0)
                  data.CornerOrderDiff4 = (short)(horse.RaceHorse.FourthCornerOrder - horse.RaceHorse.ThirdCornerOrder);
                if (horse.RaceHorse.FourthCornerOrder > 0)
                  data.CornerOrderDiffGoal = (short)(horse.RaceHorse.ResultOrder - horse.RaceHorse.FourthCornerOrder);

                // コーナーで内・中・外に位置した回数
                {
                  var number = horse.RaceHorse.Number;
                  var inside = 0;
                  var center = 0;
                  var outside = 0;
                  var one = 0;
                  foreach (var corner in new string[]
                  {
                    horse.Race.Corner1Result,
                    horse.Race.Corner2Result,
                    horse.Race.Corner3Result,
                    horse.Race.Corner4Result,
                  }.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => RaceCorner.FromString(c)))
                  {
                    var group = corner.Groups.FirstOrDefault(g => g.HorseNumbers.Contains(number));
                    if (group != null)
                    {
                      if (group.HorseNumbers.Count == 1)
                      {
                        one++;
                      }
                      else
                      {
                        var index = group.HorseNumbers.ToList().IndexOf(number);
                        if (index == 0)
                        {
                          inside++;
                        }
                        else if (index == group.HorseNumbers.Count - 1)
                        {
                          outside++;
                        }
                        else
                        {
                          center++;
                        }
                      }
                    }
                  }
                  data.CornerInsideCount = (short)inside;
                  data.CornerMiddleCount = (short)center;
                  data.CornerOutsideCount = (short)outside;
                  data.CornerAloneCount = (short)one;
                }
              }
            }

            if (horse.Race.DataStatus < RaceDataStatus.PreliminaryGradeFull)
            {
              horse.RaceHorse.ExtraDataState = HorseExtraDataState.UntilRace;
            }
            else if (horse.Race.DataStatus <= RaceDataStatus.Grade)
            {
              horse.RaceHorse.ExtraDataState = HorseExtraDataState.AfterRace;
            }
            else
            {
              // 中止、地方、外国など
              horse.RaceHorse.ExtraDataState = HorseExtraDataState.Ignored;
            }
            horse.RaceHorse.ExtraDataVersion = currentDataVersion;
          }

          progress.Value += buffer.Length;

          await db.RaceHorseExtras!.AddRangeAsync(newData);
          await db.SaveChangesAsync();
          if (lastCommit + 10000 < progress.Value)
          {
            await db.CommitAsync();
            db.ChangeTracker.Clear();
            lastCommit = progress.Value;
          }

          if (isCanceled.Value)
          {
            await db.CommitAsync();
            return;
          }
        }

        await db.SaveChangesAsync();
        await db.CommitAsync();
      }
      catch (Exception ex)
      {
        logger.Error("拡張データ作成でエラー", ex);
      }
    }
  }
}
