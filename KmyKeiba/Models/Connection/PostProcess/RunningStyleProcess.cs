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
  public class RunningStyleProcess : IPostProcessing
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public ProcessingStep Step => ProcessingStep.RunningStyle;

    public async Task RunAsync()
    {
      var state = DownloadStatus.Instance;

      logger.Info($"後処理進捗変更: {Step}");
      await PredictRunningStyleAsync(state.IsCancelProcessing, state.ProcessingProgress, state.ProcessingProgressMax);
    }

    private static async Task PredictRunningStyleAsync(ReactiveProperty<bool>? isCanceled = null, ReactiveProperty<int>? progress = null, ReactiveProperty<int>? progressMax = null)
    {
      progressMax ??= new();
      progress ??= new();
      isCanceled ??= new();

      try
      {
        using var db = new MyContext();

        await db.TryBeginTransactionAsync();

        var targets = db.RaceHorses!.Where((h) => h.Course >= RaceCourse.CentralMaxValue &&
                                                  !h.IsRunningStyleSetManually &&
                                                  h.ResultOrder > 0 &&
                                                  h.FourthCornerOrder > 0)
          .Join(db.Races!, rh => rh.RaceKey, r => r.Key, (rh, r) => new
          {
            RaceHorse = rh,
            HorsesCount = r.ResultHorsesCount,
          })
          .Where((d) => d.HorsesCount > 1);

        // progressMax.Value = await targets.CountAsync();
        progress.Value = 0;

        var batchSize = 8192;

        while (targets.Any())
        {
          var items = await targets.Take(batchSize).ToArrayAsync();

          foreach (var item in items)
          {
            var corners = new[]
            {
              item.RaceHorse.FirstCornerOrder,
              item.RaceHorse.SecondCornerOrder,
              item.RaceHorse.ThirdCornerOrder,
              item.RaceHorse.FourthCornerOrder,
            }.Where(c => c != default).ToArray();

            if (corners.Length > 0)
            {
              var lastCorner = corners[corners.Length - 1];

              if (corners.Length == 1 ? corners[0] == 1 : corners.Reverse().Skip(1).Any(c => c == 1))
              {
                item.RaceHorse.RunningStyle = RunningStyle.FrontRunner;
              }
              else if (lastCorner <= 4)
              {
                item.RaceHorse.RunningStyle = RunningStyle.Stalker;
              }
              else if (item.HorsesCount >= 8 && lastCorner <= item.HorsesCount * 2 / 3)
              {
                item.RaceHorse.RunningStyle = RunningStyle.Sotp;
              }
              else
              {
                item.RaceHorse.RunningStyle = RunningStyle.SaveRunner;
              }
            }

            item.RaceHorse.IsRunningStyleSetManually = true;
          }

          await db.SaveChangesAsync();

          var oldProgress = progress.Value;
          progress.Value += items.Length;
          if (items.Length == 0)
          {
            break;
          }
          if (oldProgress % 10_0000 != progress.Value % 10_0000)
          {
            await db.CommitAsync();
          }

          if (isCanceled.Value)
          {
            break;
          }
        }

        await db.CommitAsync();
      }
      catch (Exception ex)
      {
        logger.Warn("脚質の予測でエラー", ex);
      }
    }
  }
}
