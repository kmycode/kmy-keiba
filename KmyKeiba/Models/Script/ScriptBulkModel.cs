using KmyKeiba.Common;
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

namespace KmyKeiba.Models.Script
{
  public class ScriptBulkModel
  {
    private bool _isCanceled = false;

    public ReactiveCollection<ScriptResultItem> Results { get; } = new();

    public ReactiveProperty<DateTime> StartDate { get; } = new(DateTime.Today.AddMonths(-1));

    public ReactiveProperty<bool> IsExecuting { get; } = new();

    public ReactiveProperty<bool> IsError { get; } = new();

    public ReactiveProperty<int> SumOfIncomes { get; } = new();

    public ReactiveProperty<ValueComparation> IncomeComparation { get; } = new();

    public void BeginExecute()
    {
      Task.Run(async () =>
      {
        await this.ExecuteAsync();
      });
    }

    public async Task ExecuteAsync()
    {
      this._isCanceled = false;
      this.IsExecuting.Value = true;
      this.IsError.Value = false;
      this.SumOfIncomes.Value = 0;

      var divitions = 2;

      var engines = new List<EngineInfo>();
      try
      {
        for (var s = 0; s < divitions; s++)
        {
          engines.Add(new EngineInfo(new CompiledScriptEngineWrapper()));
        }
      }
      catch
      {
        // TODO: logs
        this.IsError.Value = true;
        return;
      }

      using var db = new MyContext();
      var startTime = this.StartDate.Value;
      var races = await db.Races!
        .Where(r => r.StartTime >= startTime && r.DataStatus >= RaceDataStatus.PreliminaryGrade)
        .OrderBy(r => r.Course)
        .OrderBy(r => r.StartTime)
        .ToArrayAsync();

      var items = new List<ScriptResultItem>();
      var i = 0;
      foreach (var race in races)
      {
        items.Add(new ScriptResultItem(i++, race));
      }
      ThreadUtil.InvokeOnUiThread(() =>
      {
        this.Results.Clear();
        foreach (var item in items)
        {
          this.Results.Add(item);
        }
      });

      for (var s = 0; s < divitions; s++)
      {
        var engine = engines[s];
        var t = s;
        _ = Task.Run(async () =>
        {
          await engine.DoAsync(this, items.Where(i => i.Index % divitions == t));
        });

        // 同時に始めるとInjectionManagerでIBuyer取得時にエラーが発生することがある
        await Task.Delay(1000);
      }

      while (engines.Any(f => !f.IsFinished))
      {
        await Task.Delay(1000);
      }

      this.IsExecuting.Value = false;
      this._isCanceled = false;

      foreach (var engine in engines)
      {
        engine.Dispose();
      }
    }

    public void Cancel()
    {
      if (!this.IsExecuting.Value)
      {
        return;
      }

      this._isCanceled = true;
    }

    public class EngineInfo : IDisposable
    {
      public ScriptEngineWrapper Engine { get; }

      public ReactiveProperty<ReactiveProperty<int>?> ProgressMax { get; } = new();

      public ReactiveProperty<ReactiveProperty<int>?> Progress { get; } = new();

      public bool IsFinished { get; set; }

      public EngineInfo(ScriptEngineWrapper engine)
      {
        this.Engine = engine;
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

            var info = await RaceInfo.FromKeyAsync(item.Race.Key);
            if (info != null)
            {
              while (!info.IsLoadCompleted.Value)
              {
                await Task.Delay(50);
              }

              this.ProgressMax.Value = this.Engine.Html.ProgressMaxObservable;
              this.Progress.Value = this.Engine.Html.ProgressObservable;

              var result = await this.Engine.ExecuteAsync(info);

              if (result.IsError)
              {
                item.IsError.Value = true;
                item.ErrorType.Value = ScriptBulkErrorType.ScriptError;
              }
              else if (info.Tickets.Value != null && info.Payoff != null)
              {
                info.Payoff.UpdateTicketsData(result.Suggestion.Tickets
                  .Select(t => TicketItem.FromData(t, info.Horses.Select(h => h.Data).ToArray(), info.Odds.Value))
                  .Where(t => t != null)
                  .Select(t => t!), info.Horses.Select(h => h.Data).ToArray());
                item.PaidMoney.Value = info.Payoff.PayMoneySum.Value;
                item.PayoffMoney.Value = info.Payoff.HitMoneySum.Value;
                item.Income.Value = info.Payoff.Income.Value;
                item.IncomeComparation.Value = item.Income.Value > 0 ? ValueComparation.Good :
                  item.Income.Value < 0 ? ValueComparation.Bad : ValueComparation.Standard;
                item.IsCompleted.Value = true;

                model.SumOfIncomes.Value += item.Income.Value;
                model.IncomeComparation.Value = model.SumOfIncomes.Value > 0 ? ValueComparation.Good :
                  model.SumOfIncomes.Value < 0 ? ValueComparation.Bad : ValueComparation.Standard;
              }
            }
            else
            {
              item.IsError.Value = true;
              item.ErrorType.Value = ScriptBulkErrorType.NoRace;
            }
          }
          catch
          {
            item.IsError.Value = true;
            item.ErrorType.Value = ScriptBulkErrorType.AnyError;
          }
          finally
          {
            item.IsExecuting.Value = false;
            item.HandlerEngine.Value = null;
          }

          if (model._isCanceled)
          {
            break;
          }
        }

        this.IsFinished = true;
      }

      public void Dispose()
      {
        this.Engine.Dispose();
      }
    }
  }

  public class ScriptResultItem
  {
    private RaceSubjectInfo _subject;

    public int Index { get; }

    public RaceData Race { get; }

    public RaceSubject Subject => this._subject.Subject;

    public DateTime StartTime => this.Race.StartTime;

    public RaceCourse Course => this.Race.Course;

    public string Name => this._subject.DisplayName;

    public short HorsesCount => this.Race.HorsesCount;

    public ReactiveProperty<int> PaidMoney { get; } = new();

    public ReactiveProperty<int> PayoffMoney { get; } = new();

    public ReactiveProperty<int> Income { get; } = new();

    public ReactiveProperty<ValueComparation> IncomeComparation { get; } = new();

    public ReactiveProperty<bool> IsExecuting { get; } = new();

    public ReactiveProperty<bool> IsError { get; } = new();

    public ReactiveProperty<bool> IsCompleted { get; } = new();

    public ReactiveProperty<ScriptBulkErrorType> ErrorType { get; } = new();

    public ReactiveProperty<ScriptBulkModel.EngineInfo?> HandlerEngine { get; } = new();

    public ScriptResultItem(int index, RaceData race)
    {
      this.Index = index;
      this.Race = race;
      this._subject = new RaceSubjectInfo(race);
    }
  }

  public enum ScriptBulkErrorType
  {
    Unknown,

    [Label("レースデータが取得できません")]
    NoRace,

    [Label("不明なエラー")]
    AnyError,

    [Label("スクリプトがエラーを返しました")]
    ScriptError,
  }
}
