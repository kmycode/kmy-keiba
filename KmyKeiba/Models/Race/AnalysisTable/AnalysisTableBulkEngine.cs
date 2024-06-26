﻿using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
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
    private readonly AggregateRaceFinder _aggregateFinder;

    public ReactiveProperty<ReactiveProperty<int>?> ProgressMax { get; } = new();

    public ReactiveProperty<ReactiveProperty<int>?> Progress { get; } = new();

    private bool _isRapidMode;

    private readonly AggregateBuySimulator _simulator;

    public bool IsFinished { get; set; }

    public AnalysisTableBulkEngine(AggregateRaceFinder aggregateFinder, AggregateBuySimulator simulator, bool isRapidMode)
    {
      this._aggregateFinder = aggregateFinder;
      this._simulator = simulator;
      this._isRapidMode = isRapidMode;
    }

    public void Dispose()
    {
    }

    public async Task DoAsync(int index, ScriptBulkModel model, IEnumerable<ScriptResultItem> items)
    {
      this.ProgressMax.Value = new ReactiveProperty<int>(1);
      this.Progress.Value = new ReactiveProperty<int>();
      var collectCount = 0;

      using var db = new MyContext();
      await Race.AnalysisTable.AnalysisTableUtil.InitializeAsync(db);

      var finder = this._isRapidMode ? this._aggregateFinder : null;

      foreach (var item in items)
      {
        if (item.IsCompleted.Value || item.IsExecuting.Value)
        {
          continue;
        }

        item.IsExecuting.Value = true;
        item.HandlerEngine.Value = this;

        var simulator = this._simulator;

        try
        {
          using var info = await RaceInfoSlim.FromKeyAsync(item.Race.Key);
          if (info != null)
          {
            this.Progress.Value = info.AnalysisTable.Value.Aggregate.Progress;
            this.ProgressMax.Value = info.AnalysisTable.Value.Aggregate.ProgressMax;
            await info.AnalysisTable.Value.Aggregate.LoadAsync(isBulk: true, finder);

            if (info.HasResults.Value)
            {
              var sorted = info.AnalysisTable.Value.Aggregate.Horses.Where(h => h.Horse.Data.ResultOrder > 0).OrderBy(h => h.Horse.Data.ResultOrder);
              item.FirstHorseMark.Value = sorted.FirstOrDefault(s => s.Horse.Data.ResultOrder == 1)?.MarkSuggestion.Value ?? RaceHorseMark.Default;
              item.SecondHorseMark.Value = sorted.FirstOrDefault(s => s.Horse.Data.ResultOrder == 2)?.MarkSuggestion.Value ?? RaceHorseMark.Default;
              item.ThirdHorseMark.Value = sorted.FirstOrDefault(s => s.Horse.Data.ResultOrder == 3)?.MarkSuggestion.Value ?? RaceHorseMark.Default;

              var payoff = await info.GetPayoffInfoAsync();
              if (payoff != null)
              {
                var odds = await info.GetOddsInfoAsync();
                var markData = sorted.Select(s => (s.MarkSuggestion.Value, s.Horse.Data.Number)).ToArray();
                var result = simulator.CalcPayoff(payoff, odds, info.Horses, markData);

                this.AddResultsToTicketTypeCollection(model, result);

                item.PaidMoney.Value = result.PaidMoney;
                item.PayoffMoney.Value = result.PayoffMoney;
                item.Income.Value = result.Income;
                item.IncomeComparation.Value = item.Income.Value > 0 ? ValueComparation.Good :
                  item.Income.Value < 0 ? ValueComparation.Bad : ValueComparation.Standard;
                item.IsCompleted.Value = true;

                model.SumOfIncomes.Value += item.Income.Value;
                model.IncomeComparation.Value = model.SumOfIncomes.Value > 0 ? ValueComparation.Good :
                  model.SumOfIncomes.Value < 0 ? ValueComparation.Bad : ValueComparation.Standard;
              }
            }

            item.IsCompleted.Value = true;
          }
          else
          {
            item.IsError.Value = true;
            item.ErrorType.Value = ScriptBulkErrorType.NoRace;
          }

          if (index == 0)
          {
            ++collectCount;
            if (collectCount >= 3 && info != null)
            {
              GC.Collect();
              this._aggregateFinder.CompressCache(info.Data);
              collectCount = 0;
            }
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

      if (collectCount > 0)
      {
        GC.Collect();
      }

      this.IsFinished = true;
    }

    private void AddResultsToTicketTypeCollection(ScriptBulkModel model, AggregateBuySimulator.Result result)
    {
      foreach (var item in result.ResultPerTicketTypes.Join(model.ResultsPerTicketType, r => r.Key, r => r.TicketType, (r1, r2) => new { Result = r1, Model = r2, }))
      {
        item.Model.PaidMoney.Value += item.Result.Value.PaidMoney;
        item.Model.PayoffMoney.Value += item.Result.Value.PayoffMoney;
        item.Model.IncomeMoney.Value += item.Result.Value.Income;
        item.Model.IncomeComparation.Value = AnalysisUtil.CompareValue(item.Model.IncomeMoney.Value, 1, -1);
        item.Model.RecoveryRate.Value = (double)item.Model.PayoffMoney.Value / item.Model.PaidMoney.Value;
      }
      model.TotalResult.PaidMoney.Value += result.PaidMoney;
      model.TotalResult.PayoffMoney.Value += result.PayoffMoney;
      model.TotalResult.IncomeMoney.Value += result.Income;
      model.TotalResult.IncomeComparation.Value = AnalysisUtil.CompareValue(model.TotalResult.IncomeMoney.Value, 1, -1);
      model.TotalResult.RecoveryRate.Value = (double)model.TotalResult.PayoffMoney.Value / model.TotalResult.PaidMoney.Value;
    }

    public void EnableBulk()
    {
      // Not to do
    }
  }
}
