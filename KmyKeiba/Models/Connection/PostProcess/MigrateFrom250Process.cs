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
  public class MigrateFrom250Process : IPostProcessing
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public ProcessingStep Step => ProcessingStep.MigrationFrom250;

    public async Task RunAsync()
    {
      var state = DownloadStatus.Instance;

      logger.Info($"後処理進捗変更: {Step}");
      await MigrateFrom250Async(isCanceled: state.IsCancelProcessing);
    }

    private static async Task MigrateFrom250Async(ReactiveProperty<bool>? isCanceled = null, ReactiveProperty<int>? progress = null, ReactiveProperty<int>? progressMax = null)
    {
      try
      {
        using var db = new MyContext();
        await db.TryBeginTransactionAsync();

        var races = db.Races!.Where(r => r.SubjectDisplayInfo != string.Empty && r.SubjectInfo1 == string.Empty);

        if (!(await races.AnyAsync()))
        {
          return;
        }

        if (progressMax != null)
        {
          progressMax.Value = await races.CountAsync();
        }
        if (progress != null)
        {
          progress.Value = 0;
        }

        var i = 0;
        foreach (var race in races)
        {
          race.SubjectDisplayInfo = string.Empty;
          i++;

          if (i >= 10000)
          {
            await db.SaveChangesAsync();
            await db.CommitAsync();

            if (isCanceled?.Value == true)
            {
              return;
            }

            if (progress != null)
            {
              progress.Value += i;
            }
            i = 0;
          }
        }

        await db.SaveChangesAsync();
        await db.CommitAsync();
      }
      catch (Exception ex)
      {
        logger.Error("2.5.0からのマイグレーション中にエラー", ex);
      }
    }
  }
}
