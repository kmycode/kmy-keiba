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
  public class MigrateFrom322Process : IPostProcessing
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public ProcessingStep Step => ProcessingStep.MigrationFrom322;

    public async Task RunAsync()
    {
      var state = DownloadStatus.Instance;

      logger.Info($"後処理進捗変更: {Step}");
      await MigrateFrom322Async(isCanceled: state.IsCancelProcessing);
    }

    private static async Task MigrateFrom322Async(ReactiveProperty<bool>? isCanceled = null, ReactiveProperty<int>? progress = null, ReactiveProperty<int>? progressMax = null)
    {
      using var db = new MyContext();
      await db.TryBeginTransactionAsync();

      var targets = db.Horses!
        .GroupBy(h => h.Code, (h, hs) => new { Key = h, Count = hs.Count(), IsCentral = hs.Any(h => h.CentralFlag != 0), IsLocal = hs.Any(h => h.CentralFlag == 0), })
        .Where(h => h.Count >= 2);

      try
      {
        if (!await targets.AnyAsync())
        {
          return;
        }

        var keys = await targets.Select(t => t.Key).ToArrayAsync();

        if (progressMax != null)
        {
          progressMax.Value = keys.Length;
        }
        if (progress != null)
        {
          progress.Value = 0;
        }

        var count = 0;
        foreach (var key in keys)
        {
          var horses = await db.Horses!.Where(h => h.Code == key).ToArrayAsync();

          HorseData? central = null;
          HorseData? local = null;

          foreach (var horse in horses)
          {
            if (horse.IsCentral)
            {
              if (central == null && horse.Belongs != HorseBelongs.Local)
              {
                central = horse;
              }
              else
              {
                db.Horses!.Remove(horse);
              }
            }
            else if (!horse.IsCentral)
            {
              if (local == null && horse.Belongs == HorseBelongs.Local)
              {
                local = horse;
              }
              else
              {
                db.Horses!.Remove(horse);
              }
            }
          }

          count++;

          if (count >= 100)
          {
            await db.SaveChangesAsync();
            await db.CommitAsync();

            if (progress != null)
            {
              progress.Value += count;
            }
            count = 0;
          }
        }

        await db.SaveChangesAsync();
        await db.CommitAsync();
      }
      catch (Exception ex)
      {
        logger.Error("3.2.2からのマイグレーションでエラー", ex);
      }
    }
  }
}
