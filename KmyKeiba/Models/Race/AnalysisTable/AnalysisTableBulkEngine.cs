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
            await info.AnalysisTable.Value.AnalysisTablesAsync();

            //this.ProgressMax.Value = this.Engine.Html.ProgressMaxObservable;
            //this.Progress.Value = this.Engine.Html.ProgressObservable;

            // TODO: 買い目処理はここに
            //if (info.Tickets.Value != null && info.Payoff != null)
            //{
            //}

            if (info.HasResults.Value)
            {
              item.FirstHorseMark.Value = RaceHorseMark.Default;
              item.SecondHorseMark.Value = RaceHorseMark.Default;
              item.ThirdHorseMark.Value = RaceHorseMark.Default;
            }

            item.IsResultRead.Value = true;
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
