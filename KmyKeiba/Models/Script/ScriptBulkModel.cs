﻿using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race;
using KmyKeiba.Models.Race.AnalysisTable;
using KmyKeiba.Models.Race.Finder;
using KmyKeiba.Models.Race.Tickets;
using KmyKeiba.Shared;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.System.Preview;
using static KmyKeiba.Models.Script.ScriptBulkModel;

namespace KmyKeiba.Models.Script
{
  public class ScriptBulkModel
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public bool IsCanceled { get; private set; }

    public ReactiveCollection<ScriptResultItem> Results { get; } = new();

    public ReactiveProperty<DateTime> StartDate { get; } = new(DateTime.Today.AddMonths(-1));

    public ReactiveProperty<DateTime> EndDate { get; } = new(DateTime.Today);

    public ReactiveProperty<string> ThreadSize { get; } = new("2");

    public ReactiveProperty<bool> IsExecuting { get; } = new();

    public ReactiveProperty<bool> IsError { get; } = new();

    public ReactiveProperty<long> SumOfIncomes { get; } = new();

    public ReactiveProperty<ValueComparation> IncomeComparation { get; } = new();

    public ReactiveCollection<TicketTypeResultItem> ResultsPerTicketType { get; } = new();

    public TicketTypeResultItem TotalResult { get; } = new(TicketType.Unknown);

    public ReactiveProperty<bool> IsAnalysisTableMode { get; } = new(true);

    public ReactiveProperty<bool> IsRapidMode { get; } = new();

    public FinderModel FinderModelForConfig { get; } = new FinderModel(null, null, null);

    public AggregateBuySimulator BuySimulator { get; } = new AggregateBuySimulator();

    public ReactiveProperty<DateTime> EstimateTime { get; } = new(DateTime.MinValue);

    public ScriptBulkModel()
    {
      this.BuySimulator.Items.Add(new AggregateBuyItem(TicketType.Single));
      this.BuySimulator.Items.Add(new AggregateBuyItem(TicketType.Place));
      this.BuySimulator.Items.Add(new AggregateBuyItem(TicketType.QuinellaPlace));
      this.BuySimulator.Items.Add(new AggregateBuyItem(TicketType.Quinella));
      this.BuySimulator.Items.Add(new AggregateBuyItem(TicketType.Exacta));
      this.BuySimulator.Items.Add(new AggregateBuyItem(TicketType.Trio));
      this.BuySimulator.Items.Add(new AggregateBuyItem(TicketType.Trifecta));
      this.FinderModelForConfig.Input.OtherSetting.IsFinishedRaceOnly.Value = true;

      this.ResultsPerTicketType.Add(new TicketTypeResultItem(TicketType.Single));
      this.ResultsPerTicketType.Add(new TicketTypeResultItem(TicketType.Place));
      this.ResultsPerTicketType.Add(new TicketTypeResultItem(TicketType.QuinellaPlace));
      this.ResultsPerTicketType.Add(new TicketTypeResultItem(TicketType.Quinella));
      this.ResultsPerTicketType.Add(new TicketTypeResultItem(TicketType.Exacta));
      this.ResultsPerTicketType.Add(new TicketTypeResultItem(TicketType.Trio));
      this.ResultsPerTicketType.Add(new TicketTypeResultItem(TicketType.Trifecta));
    }

    public void BeginExecute()
    {
      Task.Run(async () =>
      {
        if (this.IsAnalysisTableMode.Value)
        {
          var aggregateFinder = new AggregateRaceFinder();
          await this.ExecuteAsync(() => new AnalysisTableBulkEngine(aggregateFinder, this.BuySimulator, this.IsRapidMode.Value));
        }
        else
        {
          await this.ExecuteAsync(() => new EngineInfo(new CompiledScriptEngineWrapper()));
        }
      });
    }

    public async Task ExecuteAsync(Func<IScriptBulkEngine> bulkEngineGenerator)
    {
      this.IsCanceled = false;
      this.IsExecuting.Value = true;
      this.IsError.Value = false;
      this.SumOfIncomes.Value = 0;

      this.TotalResult.Reset();
      foreach (var result in this.ResultsPerTicketType)
      {
        result.Reset();
      }

      short.TryParse(this.ThreadSize.Value, out var divitions);
      if (divitions < 1)
      {
        divitions = 2;
      }

      var engines = new List<IScriptBulkEngine>();
      try
      {
        for (var s = 0; s < divitions; s++)
        {
          engines.Add(bulkEngineGenerator());
        }
      }
      catch (Exception ex)
      {
        logger.Error("スクリプトエンジン生成でエラー", ex);
        this.IsError.Value = true;
        return;
      }

      var startTime = this.StartDate.Value;
      var endTime = this.EndDate.Value.AddDays(1);

      using var db = new MyContext();

      using var finder = new PureRaceFinder();
      var reader = new ScriptKeysReader($"[from]{startTime:yyyyMMdd}|[to]{endTime:yyyyMMdd}|" + this.FinderModelForConfig.Input.Query.Value);
      var queries = reader.GetQueries();
      var finderResult = await finder.FindRaceHorsesAsync(queries, int.MaxValue);
      var races = finderResult.Items
        .Where(i => i.Race.DataStatus >= RaceDataStatus.PreliminaryGradeFull)
        .GroupBy(i => i.Race.Key)
        .Select(g => g.First().Race)
        .OrderBy(r => r.Course)
        .OrderBy(r => r.StartTime)
        .ToArray();

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

      var processingStartTime = DateTime.Now;
      for (var s = 0; s < divitions; s++)
      {
        var engine = engines[s];
        engine.EnableBulk();

        var t = s;
        _ = Task.Run(() =>
        {
          engine.DoAsync(s, this, items.Where(i => i.Index % divitions == t)).Wait();
        });

        // 同時に始めるとInjectionManagerでIBuyer取得時にエラーが発生することがある（スクリプト選択時）
        await Task.Delay(1000);
      }

      while (engines.Any(f => !f.IsFinished))
      {
        await Task.Delay(1000);

        // 残り時間
        if (items.Any(i => i.IsCompleted.Value || i.IsSkipped.Value || i.IsError.Value))
        {
          try
          {
            this.EstimateTime.Value = DateTime.Now + ((DateTime.Now - processingStartTime) * races.Length / items.Count(i => i.IsCompleted.Value || i.IsSkipped.Value || i.IsError.Value));
          }
          catch (Exception ex)
          {
          }
        }
      }

      foreach (var engine in engines)
      {
        engine.Dispose();
      }

      this.IsExecuting.Value = false;
      this.IsCanceled = false;
    }

    public void Cancel()
    {
      if (!this.IsExecuting.Value)
      {
        return;
      }

      this.IsCanceled = true;
    }

    public class TicketTypeResultItem
    {
      public TicketType TicketType { get; }

      public ReactiveProperty<long> PaidMoney { get; } = new();

      public ReactiveProperty<long> PayoffMoney { get; } = new();

      public ReactiveProperty<long> IncomeMoney { get; } = new();

      public ReactiveProperty<ValueComparation> IncomeComparation { get; } = new();

      public ReactiveProperty<double> RecoveryRate { get; } = new();

      public TicketTypeResultItem(TicketType type)
      {
        this.TicketType = type;
      }

      public void Reset()
      {
        this.PaidMoney.Value = this.PayoffMoney.Value = this.IncomeMoney.Value = 0;
        this.IncomeComparation.Value = ValueComparation.Standard;
        this.RecoveryRate.Value = double.NaN;
      }
    }

    public interface IScriptBulkEngine : IDisposable
    {
      ReactiveProperty<ReactiveProperty<int>?> ProgressMax { get; }

      ReactiveProperty<ReactiveProperty<int>?> Progress { get; }

      public bool IsFinished { get; }

      void EnableBulk();

      Task DoAsync(int index, ScriptBulkModel model, IEnumerable<ScriptResultItem> items);
    }

    public class EngineInfo : IScriptBulkEngine
    {
      public ScriptEngineWrapper Engine { get; }

      public ReactiveProperty<ReactiveProperty<int>?> ProgressMax { get; } = new();

      public ReactiveProperty<ReactiveProperty<int>?> Progress { get; } = new();

      public bool IsFinished { get; set; }

      public EngineInfo(ScriptEngineWrapper engine)
      {
        this.Engine = engine;
      }

      public void EnableBulk()
      {
        this.Engine.BulkConfig.SetBulk(true);
      }

      public async Task DoAsync(int index, ScriptBulkModel model, IEnumerable<ScriptResultItem> items)
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

            if (!this.Engine.BulkConfig.IsCentral && item.Race.Course <= RaceCourse.CentralMaxValue)
            {
              item.IsSkipped.Value = true;
              continue;
            }
            if (!this.Engine.BulkConfig.IsLocal && item.Race.Course >= RaceCourse.LocalMinValue)
            {
              item.IsSkipped.Value = true;
              continue;
            }
            if (!this.Engine.BulkConfig.IsBanei && item.Race.Course == RaceCourse.ObihiroBannei)
            {
              item.IsSkipped.Value = true;
              continue;
            }

            using var info = await RaceInfo.FromKeyAsync(item.Race.Key, withTransaction: false, isCache: false);
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
              else
              {
                if (!this.Engine.IsReadResults)
                {
                  if (info.Tickets.Value != null && info.Payoff != null)
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

                  if (info.HasResults.Value && result.Suggestion.HasMarks.Value)
                  {
                    var horseMarks = info.Horses
                      .Join(result.Suggestion.Marks, h => h.Data.Number, s => s.HorseNumber, (h, s) => new { h.Data.ResultOrder, s.HorseNumber, s.Mark, })
                      .ToArray();
                    var first = horseMarks.FirstOrDefault(h => h.ResultOrder == 1)?.Mark ?? RaceHorseMark.Default;
                    var second = horseMarks.FirstOrDefault(h => h.ResultOrder == 2)?.Mark ?? RaceHorseMark.Default;
                    var third = horseMarks.FirstOrDefault(h => h.ResultOrder == 3)?.Mark ?? RaceHorseMark.Default;
                    item.FirstHorseMark.Value = first;
                    item.SecondHorseMark.Value = second;
                    item.ThirdHorseMark.Value = third;
                  }
                }
                else
                {
                  item.IsResultRead.Value = true;
                }
              }
            }
            else
            {
              item.IsError.Value = true;
              item.ErrorType.Value = ScriptBulkErrorType.NoRace;
            }
          }
          catch (Exception ex)
          {
            logger.Error("スクリプト一括実行で例外", ex);
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

    public ReactiveProperty<RaceHorseMark> FirstHorseMark { get; } = new();

    public ReactiveProperty<RaceHorseMark> SecondHorseMark { get; } = new();

    public ReactiveProperty<RaceHorseMark> ThirdHorseMark { get; } = new();

    public ReactiveProperty<bool> IsExecuting { get; } = new();

    public ReactiveProperty<bool> IsError { get; } = new();

    public ReactiveProperty<bool> IsCompleted { get; } = new();

    public ReactiveProperty<bool> IsSkipped { get; } = new();

    public ReactiveProperty<bool> IsResultRead { get; } = new();

    public ReactiveProperty<ScriptBulkErrorType> ErrorType { get; } = new();

    public ReactiveProperty<IScriptBulkEngine?> HandlerEngine { get; } = new();

    public ScriptResultItem(int index, RaceData race)
    {
      this.Index = index;
      this.Race = race;
      this._subject = new RaceSubjectInfo(race);
    }

    public ICommand OpenRaceWindowCommand =>
      this._openRaceWindowCommand ??=
        new ReactiveCommand().WithSubscribe(() => OpenRaceRequest.Default.Request(this.Race.Key));
    private ReactiveCommand? _openRaceWindowCommand;
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
