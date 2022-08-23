using KmyKeiba.Data.Db;
using KmyKeiba.Models.Script;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KmyKeiba.Models.Script.ScriptBulkModel;

namespace KmyKeiba.Models.Race.AnalysisTable
{
  internal class AnalysisTableBulkEngine : IScriptBulkEngine
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public ReactiveProperty<ReactiveProperty<int>?> ProgressMax { get; } = new();

    public ReactiveProperty<ReactiveProperty<int>?> Progress { get; } = new();

    public bool IsFinished { get; set; }

    public void Dispose()
    {
    }

    public async Task DoAsync(ScriptBulkModel model, IEnumerable<ScriptResultItem> items)
    {
      this.ProgressMax.Value = new ReactiveProperty<int>(1);
      this.Progress.Value = new ReactiveProperty<int>();

      foreach (var item in items)
      {
        if (item.IsCompleted.Value || item.IsExecuting.Value)
        {
          continue;
        }

        item.IsExecuting.Value = true;
        item.HandlerEngine.Value = this;

        try
        {
          if (this.Progress.Value != null && this.ProgressMax.Value != null)
          {
            this.ProgressMax.Value.Value = this.Progress.Value.Value = 0;
          }

          using var info = await RaceInfo.FromKeyAsync(item.Race.Key, withTransaction: false, isCache: false);
          if (info != null)
          {
            while (!info.IsLoadCompleted.Value)
            {
              await Task.Delay(50);
            }

            if (info.AnalysisTable.Value == null)
            {
              continue;
            }

            this.Progress.Value!.Value = 0;
            await info.AnalysisTable.Value.Aggregate.LoadAsync();
            this.Progress.Value!.Value = 1;

            // TODO: 買い目処理はここに
            //if (info.Tickets.Value != null && info.Payoff != null)
            //{
            //}

            if (info.HasResults.Value)
            {
              var sorted = info.AnalysisTable.Value.Aggregate.Horses.Where(h => h.Horse.Data.ResultOrder > 0).OrderBy(h => h.Horse.Data.ResultOrder);
              item.FirstHorseMark.Value = sorted.FirstOrDefault(s => s.Horse.Data.ResultOrder == 1)?.MarkSuggestion.Value ?? RaceHorseMark.Default;
              item.SecondHorseMark.Value = sorted.FirstOrDefault(s => s.Horse.Data.ResultOrder == 2)?.MarkSuggestion.Value ?? RaceHorseMark.Default;
              item.ThirdHorseMark.Value = sorted.FirstOrDefault(s => s.Horse.Data.ResultOrder == 3)?.MarkSuggestion.Value ?? RaceHorseMark.Default;
            }

            item.IsCompleted.Value = true;
          }
          else
          {
            item.IsError.Value = true;
            item.ErrorType.Value = ScriptBulkErrorType.NoRace;
          }
        }
        catch (Exception ex)
        {
          logger.Error("予想ファクタ一括実行で例外", ex);
          item.IsError.Value = true;
          item.ErrorType.Value = ScriptBulkErrorType.AnyError;
        }
        finally
        {
          item.IsExecuting.Value = false;
          item.HandlerEngine.Value = null;
        }

        if (model.IsCanceled)
        {
          break;
        }
      }

      this.IsFinished = true;
    }

    public void EnableBulk()
    {
      // Not to do
    }
  }
}
