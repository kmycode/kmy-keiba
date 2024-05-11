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
  public class ResetHorseExtraDataProcess : IPostProcessing
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public ProcessingStep Step => ProcessingStep.ResetHorseExtraData;

    public async Task RunAsync()
    {
      var state = DownloadStatus.Instance;

      logger.Info($"後処理進捗変更: {Step}");
      await ResetHorseExtraTableDataAsync();
    }

    private static async Task ResetHorseExtraTableDataAsync()
    {
      using var db = new MyContext();
      try
      {
        var targets = db.RaceHorses!.Where(rh => rh.ExtraDataVersion == 0);
        await db.Database.ExecuteSqlRawAsync("UPDATE RaceHorses SET ExtraDataVersion = 0;");
      }
      catch (Exception ex)
      {
        logger.Error("拡張情報リセットでエラー", ex);
      }
    }
  }
}
