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
  public class MigrateFrom500Process : IPostProcessing
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public ProcessingStep Step => ProcessingStep.MigrationFrom500;

    public async Task RunAsync()
    {
      var state = DownloadStatus.Instance;

      logger.Info($"後処理進捗変更: {Step}");
      await MigrateFrom500Async(isCanceled: state.IsCancelProcessing);
    }

    private static async Task MigrateFrom500Async(ReactiveProperty<bool>? isCanceled = null, ReactiveProperty<int>? progress = null, ReactiveProperty<int>? progressMax = null)
    {
      using var db = new MyContext();
      await db.TryBeginTransactionAsync();

      var count = 0;

      async Task<bool> TrySaveAsync()
      {
        count++;
        if (count >= 10000)
        {
          await db.SaveChangesAsync();
          await db.CommitAsync();
          db.ChangeTracker.Clear();
          count = 0;

          if (isCanceled?.Value == true)
          {
            return false;
          }
        }

        return true;
      }

      try
      {
        foreach (var target in db.Horses!.FromSql($"SELECT * FROM Horses WHERE length(FatherBreedingCode) = 8"))
        {
          target.OwnerCode = $"{target.OwnerCode}00";
          target.FatherBreedingCode = $"{target.FatherBreedingCode.Substring(0, 3)}00{target.FatherBreedingCode.Substring(3, 5)}";
          target.MotherBreedingCode = $"{target.MotherBreedingCode.Substring(0, 3)}00{target.MotherBreedingCode.Substring(3, 5)}";
          target.FFBreedingCode = $"{target.FFBreedingCode.Substring(0, 3)}00{target.FFBreedingCode.Substring(3, 5)}";
          target.FMBreedingCode = $"{target.FMBreedingCode.Substring(0, 3)}00{target.FMBreedingCode.Substring(3, 5)}";
          target.FFFBreedingCode = $"{target.FFFBreedingCode.Substring(0, 3)}00{target.FFFBreedingCode.Substring(3, 5)}";
          target.FFMBreedingCode = $"{target.FFMBreedingCode.Substring(0, 3)}00{target.FFMBreedingCode.Substring(3, 5)}";
          target.FMFBreedingCode = $"{target.FMFBreedingCode.Substring(0, 3)}00{target.FMFBreedingCode.Substring(3, 5)}";
          target.FMMBreedingCode = $"{target.FMMBreedingCode.Substring(0, 3)}00{target.FMMBreedingCode.Substring(3, 5)}";
          target.MFFBreedingCode = $"{target.MFFBreedingCode.Substring(0, 3)}00{target.MFFBreedingCode.Substring(3, 5)}";
          target.MFMBreedingCode = $"{target.MFMBreedingCode.Substring(0, 3)}00{target.MFMBreedingCode.Substring(3, 5)}";
          target.MMFBreedingCode = $"{target.MMFBreedingCode.Substring(0, 3)}00{target.MMFBreedingCode.Substring(3, 5)}";
          target.MMMBreedingCode = $"{target.MMMBreedingCode.Substring(0, 3)}00{target.MMMBreedingCode.Substring(3, 5)}";

          if (!await TrySaveAsync())
          {
            return;
          }
        }

        foreach (var target in db.HorseBloods!.FromSql($"SELECT * FROM HorseBloods WHERE length(Key) = 8"))
        {
          target.Key = $"{target.Key.Substring(0, 3)}00{target.Key.Substring(3, 5)}";

          if (!await TrySaveAsync())
          {
            return;
          }
        }

        foreach (var target in db.HorseBloodInfos!.FromSql($"SELECT * FROM HorseBloodInfos WHERE length(Key) = 8"))
        {
          target.Key = $"{target.Key.Substring(0, 3)}00{target.Key.Substring(3, 5)}";

          if (!await TrySaveAsync())
          {
            return;
          }
        }

        await db.SaveChangesAsync();
        await db.CommitAsync();
      }
      catch (Exception ex)
      {
        logger.Error("4.5.3からのマイグレーションでエラー", ex);
      }
    }
  }
}
