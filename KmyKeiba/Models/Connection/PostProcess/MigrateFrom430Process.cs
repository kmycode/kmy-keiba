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
  public class MigrateFrom430Process : IPostProcessing
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public ProcessingStep Step => ProcessingStep.MigrationFrom430;

    public async Task RunAsync()
    {
      var state = DownloadStatus.Instance;

      logger.Info($"後処理進捗変更: {Step}");
      await MigrateFrom430Async(isCanceled: state.IsCancelProcessing);
    }

    private static async Task MigrateFrom430Async(ReactiveProperty<bool>? isCanceled = null, ReactiveProperty<int>? progress = null, ReactiveProperty<int>? progressMax = null)
    {
      using var db = new MyContext();
      await db.TryBeginTransactionAsync();

      var targets = db.RaceHorseExtras!
        .Where(e => e.Rpci == 0 || e.Pci3 == 0)
        .Join(db.RaceHorses!, e => new { e.Key, e.RaceKey, }, rh => new { rh.Key, rh.RaceKey, }, (e, rh) => new { Extra = e, RaceHorse = rh, })
        .Where(d => d.RaceHorse.ExtraDataVersion >= 1 && d.RaceHorse.ExtraDataState == HorseExtraDataState.AfterRace &&
          (d.RaceHorse.Course <= RaceCourse.CentralMaxValue || d.RaceHorse.Course == RaceCourse.Urawa || d.RaceHorse.Course == RaceCourse.Oi || d.RaceHorse.Course == RaceCourse.Kawazaki || d.RaceHorse.Course == RaceCourse.Funabashi));

      try
      {
        if (!await targets.AnyAsync())
        {
          return;
        }

        var count = 0;

        foreach (var target in targets)
        {
          target.RaceHorse.ExtraDataState = HorseExtraDataState.Unset;
          target.RaceHorse.ExtraDataVersion = 0;

          count++;
          if (count >= 10000)
          {
            await db.SaveChangesAsync();
            await db.CommitAsync();
            db.ChangeTracker.Clear();
            count = 0;

            if (isCanceled?.Value == true)
            {
              return;
            }
          }
        }

        await db.SaveChangesAsync();
        await db.CommitAsync();
      }
      catch (Exception ex)
      {
        logger.Error("4.3.0からのマイグレーションでエラー", ex);
      }
    }
  }
}
