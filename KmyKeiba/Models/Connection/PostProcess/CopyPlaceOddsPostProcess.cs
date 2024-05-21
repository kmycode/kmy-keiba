using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Connection.PostProcess
{
  public class CopyPlaceOddsPostProcess : IPostProcessing
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public ProcessingStep Step => ProcessingStep.CopyPlaceOdds;

    public async Task RunAsync()
    {
      var state = DownloadStatus.Instance;

      logger.Info($"後処理進捗変更: {Step}");
      var isCanceled = state.IsCancelProcessing;
      var progress = state.ProcessingProgress;
      var progressMax = state.ProcessingProgressMax;

      try
      {
        using var db = new MyContext();
        await db.TryBeginTransactionAsync();

        var targets = db.PlaceOdds!.Where(o => !o.IsCopied);

        progressMax.Value = await targets.CountAsync();
        progress.Value = 0;

        while (await targets.AnyAsync())
        {
          var sources = await targets.Take(1024).ToArrayAsync();
          var raceKeys = sources.Select(s => s.RaceKey);
          var horses = await db.RaceHorses!.Where(rh => raceKeys.Contains(rh.RaceKey)).ToArrayAsync();

          foreach (var source in sources.GroupJoin(horses, s => s.RaceKey, h => h.RaceKey, (s, h) => new { Source = s, RaceHorses = h, }))
          {
            foreach (var horse in source.RaceHorses.Join(source.Source.GetPlaceOdds(), h => h.Number, o => o.HorseNumber, (h, o) => new { RaceHorse = h, Odds = o, }))
            {
              horse.RaceHorse.PlaceOddsMax = horse.Odds.Max;
              horse.RaceHorse.PlaceOddsMin = horse.Odds.Min;
            }
            source.Source.IsCopied = true;
          }

          await db.SaveChangesAsync();
          await db.CommitAsync();

          progress.Value += sources.Length;
        }

        progress.Value = progressMax.Value;
      }
      catch (Exception ex)
      {
        logger.Warn("複勝オッズのコピーでエラー", ex);
      }
    }
  }
}
