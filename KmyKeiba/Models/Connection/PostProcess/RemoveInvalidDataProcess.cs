using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Connection.PostProcess
{
  public class RemoveInvalidDataProcess : IPostProcessing
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public ProcessingStep Step => ProcessingStep.InvalidData;

    public async Task RunAsync()
    {
      logger.Info($"後処理進捗変更: {Step}");
      await RemoveInvalidDataAsync();
    }

    private static async Task RemoveInvalidDataAsync()
    {
      using var db = new MyContext();

      {
        var targets = db.RaceHorses!.Where(rh => rh.RaceKey == "" || rh.Key == "");
        db.RaceHorses!.RemoveRange(targets);
        await db.SaveChangesAsync();

        var targets2 = db.Horses!.Where(h => h.CentralFlag == 0 && (h.Belongs == HorseBelongs.Ritto || h.Belongs == HorseBelongs.Miho));
        db.Horses!.RemoveRange(targets2);
        await db.SaveChangesAsync();
      }
    }
  }
}
