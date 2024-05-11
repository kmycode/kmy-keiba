using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Connection.PostProcess
{
  public class PreviousRaceDaysProcess : IPostProcessing
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public ProcessingStep Step => ProcessingStep.PreviousRaceDays;

    public async Task RunAsync()
    {
      var state = DownloadStatus.Instance;

      logger.Info($"後処理進捗変更: {Step}");
      await SetHorseExtraDataAsync(
        isCanceled: state.IsCancelProcessing,
        progress: state.ProcessingProgress,
        progressMax: state.ProcessingProgressMax
      );
    }

    private static async Task SetHorseExtraDataAsync(DateOnly? date = null, ReactiveProperty<bool>? isCanceled = null, ReactiveProperty<int>? progress = null, ReactiveProperty<int>? progressMax = null)
    {
      var count = 0;
      var prevCommitCount = 0;

      progress ??= new();
      progressMax ??= new();
      progress.Value = progressMax.Value = 0;

      using var db = new MyContext();

      // 通常のクエリは３分まで
      db.Database.SetCommandTimeout(180);
      //await db.BeginTransactionAsync();

      var query = db.RaceHorses!
        .Where(rh => rh.PreviousRaceDays == 0 || rh.RaceCount == 0)
        .Where(rh => rh.Key != "0000000000" && rh.Key != "" && rh.RaceKey != "");
      if (date != null)
      {
        var d = date.Value;
        var str = d.ToString("yyyyMMdd");
        query = query.Where(rh => rh.RaceKey.StartsWith(str));
      }

      if (!(await query.AnyAsync()))
      {
        return;
      }

      var allTargets = query
        .Select(g => g.Key);
      progressMax.Value = await allTargets.GroupBy(t => t).CountAsync();
      var targets = await allTargets.Take(1024).ToArrayAsync();
      var skipSize = 0;

      var completedHorses = new Dictionary<string, bool>();

      try
      {
        while (targets.Any())
        {
          targets = targets.GroupBy(k => k).Select(g => g.Key).Where(t => !completedHorses.ContainsKey(t)).ToArray();

          if (targets.Any())
          {
            var targetHorses = await db.RaceHorses!
              .Where(rh => targets.Contains(rh.Key))
              .Select(rh => new { rh.Id, rh.Key, rh.RaceKey, rh.AbnormalResult, })
              .ToArrayAsync();

            foreach (var horse in targets)
            {
              var races = targetHorses.Where(rh => rh.Key == horse).OrderBy(rh => rh.RaceKey);
              var beforeRaceDh = DateTime.MinValue;
              var isFirst = true;
              var isRested = false;

              var raceCount = 1;
              var raceCountWithinRunning = 0;
              var raceCountCompletely = 0;
              var raceCountAfterRest = 1;
              foreach (var race in races)
              {
                var y = race.RaceKey.Substring(0, 4);
                var m = race.RaceKey.Substring(4, 2);
                var d = race.RaceKey.Substring(6, 2);
                _ = int.TryParse(y, out var year);
                _ = int.TryParse(m, out var month);
                _ = int.TryParse(d, out var day);
                var dh = new DateTime(year, month, day);

                var attach = new RaceHorseData { Id = race.Id, };
                db.RaceHorses!.Attach(attach);
                if (!isFirst)
                {
                  attach.PreviousRaceDays = System.Math.Max((short)(dh - beforeRaceDh).TotalDays, (short)1);
                }
                else
                {
                  attach.PreviousRaceDays = -1;
                  isFirst = false;
                }
                if (attach.PreviousRaceDays >= 90)
                {
                  raceCountAfterRest = 1;
                  isRested = true;
                }

                attach.RaceCount = (short)raceCount;

                attach.RaceCountWithinRunning = -2;
                attach.RaceCountWithinRunningCompletely = -2;
                if (race.AbnormalResult == RaceAbnormality.Unknown || race.AbnormalResult > RaceAbnormality.ExcludedByStewards)
                {
                  // とりあえず走った
                  raceCountWithinRunning++;
                  attach.RaceCountWithinRunning = (short)raceCountWithinRunning;

                  if (isRested)
                  {
                    attach.RaceCountAfterLastRest = (short)raceCountAfterRest;
                    raceCountAfterRest++;
                  }
                  else
                  {
                    attach.RaceCountAfterLastRest = -2;
                  }

                  // ちゃんとゴールできた
                  if (race.AbnormalResult == RaceAbnormality.Unknown)
                  {
                    raceCountCompletely++;
                    attach.RaceCountWithinRunningCompletely = (short)raceCountCompletely;
                  }
                }

                beforeRaceDh = dh;
                count++;
                raceCount++;
              }

              completedHorses[horse] = true;
            }
            progress.Value += targets.Length;

            await db.SaveChangesAsync();

            if (isCanceled?.Value == true)
            {
              await db.CommitAsync();
              return;
            }

            if (count >= prevCommitCount + 10000)
            {
              await db.CommitAsync();
              db.ChangeTracker.Clear();
              prevCommitCount = count;
            }
            logger.Debug($"馬のInterval日数計算完了: {count} / {progressMax.Value}");
          }
          else
          {
            skipSize += 1024;
          }

          targets = await allTargets.Skip(skipSize).Take(1024).ToArrayAsync();
        }
      }
      catch (Exception ex)
      {
        logger.Error("馬データ成型中にエラー", ex);
      }
    }
  }
}
