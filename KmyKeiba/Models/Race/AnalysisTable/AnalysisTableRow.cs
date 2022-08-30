using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race.ExNumber;
using KmyKeiba.Models.Race.Finder;
using KmyKeiba.Models.Race.Memo;
using KmyKeiba.Models.Script;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.AnalysisTable
{
  public class AnalysisTableRow : IDisposable
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    private readonly CompositeDisposable _disposables = new();
    public bool IsFreezeParentSelection { get; set; } = false;
    private readonly bool _isInitialized = false;

    public AnalysisTableRowData Data { get; }

    public AnalysisTableSurface Table { get; }

    public FinderModel FinderModelForConfig { get; }

    public ReactiveProperty<string> Name { get; } = new();

    public ReactiveProperty<string> BaseWeight { get; } = new();

    public ReactiveProperty<string> Limited { get; } = new();

    public ReactiveProperty<string> AlternativeValueIfEmpty { get; } = new();

    public ReactiveProperty<string> ValueScript { get; } = new();

    public ReactiveProperty<AnalysisTableRowOutputType> Output { get; } = new();

    public ReadOnlyReactiveProperty<bool> CanSetExternalNumber { get; }

    public ReadOnlyReactiveProperty<bool> CanSetMemoConfig { get; }

    public ReadOnlyReactiveProperty<bool> CanSetQuery { get; }

    public ReadOnlyReactiveProperty<bool> CanSetWeight { get; }

    public ReadOnlyReactiveProperty<bool> CanSetLimited { get; }

    public ReadOnlyReactiveProperty<bool> CanSetSubOutput { get; }

    public ReadOnlyReactiveProperty<bool> CanSetJrdbOutput { get; }

    public ReadOnlyReactiveProperty<bool> CanSetAlternativeValue { get; }

    public ReadOnlyReactiveProperty<bool> CanSetScript { get; }

    public ReactiveCollection<AnalysisTableCell> Cells { get; } = new();

    public ReactiveProperty<AnalysisTableWeight?> Weight { get; } = new();

    public ReactiveProperty<AnalysisTableWeight?> Weight2 { get; } = new();

    public ReactiveProperty<AnalysisTableWeight?> Weight3 { get; } = new();

    public List<AnalysisTableRowOutputItem> RowOutputItems => AnalysisTableUtil.RowOutputItems;

    public List<AnalysisTableRowOutputItem> RowOutputSubItems => AnalysisTableUtil.RowOutputSubItems;

    public List<AnalysisTableRowOutputItem> RowOutputJrdbItems => AnalysisTableUtil.RowOutputJrdbItems;

    public ReactiveProperty<AnalysisTableRowOutputItem?> SelectedOutput { get; } = new();

    public ReactiveProperty<AnalysisTableRowOutputItem?> SelectedSubOutput { get; } = new();

    public ReactiveProperty<AnalysisTableRowOutputItem?> SelectedJrdbOutput { get; } = new();

    public ReactiveProperty<AnalysisTableRow?> SelectedParent { get; } = new();

    public ReactiveProperty<ExpansionMemoConfig?> SelectedMemoConfig { get; } = new();

    public ReactiveProperty<ExternalNumberConfigItem?> SelectedExternalNumber { get; } = new();

    public ReactiveProperty<bool> IsLoading { get; } = new();

    public ReactiveProperty<bool> IsLoaded { get; } = new();

    public bool IsFreezeExpansionMemoConfig { get; set; }

    public bool IsFreezeExternalNumberConfig { get; set; }

    public AnalysisTableRow(AnalysisTableRowData data, AnalysisTableSurface table, IEnumerable<RaceHorseAnalyzer> horses)
    {
      this.Data = data;
      this.Table = table;

      foreach (var horse in horses.Select(h => new AnalysisTableCell(h)))
      {
        this.Cells.Add(horse);
      }

      var dummyRace = new RaceData();
      var finder = new FinderModel(dummyRace, RaceHorseAnalyzer.Empty, null)
      {
        DefaultSize = 1000,
      };
      this.FinderModelForConfig = finder;

      this.Name.Value = data.Name;
      this.Output.Value = data.Output;
      this.BaseWeight.Value = data.BaseWeight.ToString();
      this.Limited.Value = data.RequestedSize.ToString();
      this.AlternativeValueIfEmpty.Value = data.AlternativeValueIfEmpty.ToString();
      this.ValueScript.Value = data.ValueScript;

      if (string.IsNullOrEmpty(this.ValueScript.Value))
      {
        this.ValueScript.Value = "value";
      }

      this.CanSetExternalNumber = this.Output.Select(o => o == AnalysisTableRowOutputType.ExternalNumber).ToReadOnlyReactiveProperty().AddTo(this._disposables);
      this.CanSetMemoConfig = this.Output.Select(o => o == AnalysisTableRowOutputType.ExpansionMemo).ToReadOnlyReactiveProperty().AddTo(this._disposables);
      this.CanSetQuery = this.SelectedOutput.Where(o => o != null).Select(o => o!.OutputType).Select(o =>
        o != AnalysisTableRowOutputType.ExternalNumber &&
        o != AnalysisTableRowOutputType.FixedValue &&
        o != AnalysisTableRowOutputType.ExpansionMemo &&
        o != AnalysisTableRowOutputType.HorseValues &&
        o != AnalysisTableRowOutputType.JrdbValues)
        .ToReadOnlyReactiveProperty().AddTo(this._disposables);
      this.CanSetWeight = this.SelectedOutput.Where(o => o != null).Select(o => o!.OutputType).Select(o =>
        o == AnalysisTableRowOutputType.FixedValue ||
        o == AnalysisTableRowOutputType.FixedValuePerPastRace ||
        o == AnalysisTableRowOutputType.PlaceBetsRate ||
        o == AnalysisTableRowOutputType.RecoveryRate ||
        o == AnalysisTableRowOutputType.WinRate ||
        o == AnalysisTableRowOutputType.ExternalNumber ||
        o == AnalysisTableRowOutputType.ExpansionMemo ||
        o == AnalysisTableRowOutputType.Binary ||
        o == AnalysisTableRowOutputType.HorseValues ||
        o == AnalysisTableRowOutputType.JrdbValues)
        .ToReadOnlyReactiveProperty().AddTo(this._disposables);
      this.CanSetLimited = this.Output.Select(o =>
        o == AnalysisTableRowOutputType.PlaceBetsRate ||
        o == AnalysisTableRowOutputType.RecoveryRate ||
        o == AnalysisTableRowOutputType.WinRate ||
        o == AnalysisTableRowOutputType.ShortestTime).ToReadOnlyReactiveProperty().AddTo(this._disposables);
      this.CanSetAlternativeValue = this.SelectedOutput.Where(o => o != null).Select(o => o!.OutputType).Select(o =>
        o != AnalysisTableRowOutputType.ExpansionMemo &&
        o != AnalysisTableRowOutputType.HorseValues &&
        o != AnalysisTableRowOutputType.FixedValue).ToReadOnlyReactiveProperty().AddTo(this._disposables);
      this.CanSetScript = this.SelectedOutput.Where(o => o != null).Select(o => o!.OutputType).Select(o =>
        o == AnalysisTableRowOutputType.ExpansionMemo ||
        o == AnalysisTableRowOutputType.ExternalNumber ||
        o == AnalysisTableRowOutputType.PlaceBetsRate ||
        o == AnalysisTableRowOutputType.WinRate ||
        o == AnalysisTableRowOutputType.RecoveryRate ||
        o == AnalysisTableRowOutputType.HorseValues ||
        o == AnalysisTableRowOutputType.JrdbValues).ToReadOnlyReactiveProperty().AddTo(this._disposables);
      this.CanSetSubOutput = this.SelectedOutput
        .Where(o => o != null)
        .Select(o => o!.OutputType)
        .Select(o => o == AnalysisTableRowOutputType.HorseValues).ToReadOnlyReactiveProperty().AddTo(this._disposables);
      this.CanSetJrdbOutput = this.SelectedOutput
        .Where(o => o != null)
        .Select(o => o!.OutputType)
        .Select(o => o == AnalysisTableRowOutputType.JrdbValues).ToReadOnlyReactiveProperty().AddTo(this._disposables);

      async Task SetWeightAsync()
      {
        // TODO try catch
        using var db = new MyContext();
        db.AnalysisTableRows!.Attach(this.Data);
        this.Data.WeightId = this.Weight.Value?.Data.Id ?? 0;
        this.Data.Weight2Id = this.Weight2.Value?.Data.Id ?? 0;
        this.Data.Weight3Id = this.Weight3.Value?.Data.Id ?? 0;
        await db.SaveChangesAsync();
      }
      this.Weight.Value = AnalysisTableUtil.Weights.FirstOrDefault(w => w.Data.Id == data.WeightId);
      this.Weight2.Value = AnalysisTableUtil.Weights.FirstOrDefault(w => w.Data.Id == data.Weight2Id);
      this.Weight3.Value = AnalysisTableUtil.Weights.FirstOrDefault(w => w.Data.Id == data.Weight3Id);
      this.Weight.Skip(1).Subscribe(async _ => await SetWeightAsync()).AddTo(this._disposables);
      this.Weight2.Skip(1).Subscribe(async _ => await SetWeightAsync()).AddTo(this._disposables);
      this.Weight3.Skip(1).Subscribe(async _ => await SetWeightAsync()).AddTo(this._disposables);

      this.SelectedParent.Skip(1).Subscribe(async _ =>
      {
        if (this.IsFreezeParentSelection)
        {
          return;
        }
        // TODO try catch
        using var db = new MyContext();
        db.AnalysisTableRows!.Attach(this.Data);
        this.Data.ParentRowId = this.SelectedParent.Value?.Data.Id ?? 0;
        await db.SaveChangesAsync();
      }).AddTo(this._disposables);

      // 外部指数の初期値は他のクラスから設定する
      this.SelectedExternalNumber.Skip(1).Subscribe(async _ =>
      {
        if (this.Data.ExternalNumberId == (this.SelectedExternalNumber.Value?.Data.Id ?? 0))
        {
          return;
        }
        if (this.IsFreezeExternalNumberConfig)
        {
          return;
        }
        // TODO try catch
        using var db = new MyContext();
        db.AnalysisTableRows!.Attach(this.Data);
        this.Data.ExternalNumberId = this.SelectedExternalNumber.Value?.Data.Id ?? 0;
        await db.SaveChangesAsync();
      }).AddTo(this._disposables);

      this.SelectedMemoConfig.Value = MemoUtil.Configs.FirstOrDefault(c => c.Id == this.Data.MemoConfigId);
      this.SelectedMemoConfig.Skip(1).Subscribe(async _ =>
      {
        if (this.IsFreezeExpansionMemoConfig)
        {
          return;
        }
        // TODO try catch
        using var db = new MyContext();
        db.AnalysisTableRows!.Attach(this.Data);
        this.Data.MemoConfigId = this.SelectedMemoConfig.Value?.Id ?? 0;
        await db.SaveChangesAsync();
      }).AddTo(this._disposables);

      this.SelectedOutput.Value = this.RowOutputItems.FirstOrDefault(i => i.OutputType == data.Output);
      if (this.SelectedOutput.Value == null)
      {
        if (this.RowOutputSubItems.Any(i => i.OutputType == data.Output))
        {
          this.SelectedSubOutput.Value = this.RowOutputSubItems.FirstOrDefault(i => i.OutputType == data.Output);
          this.SelectedOutput.Value = this.RowOutputItems.FirstOrDefault(i => i.OutputType == AnalysisTableRowOutputType.HorseValues);
        }
        else if (this.RowOutputJrdbItems.Any(i => i.OutputType == data.Output))
        {
          this.SelectedJrdbOutput.Value = this.RowOutputJrdbItems.FirstOrDefault(i => i.OutputType == data.Output);
          this.SelectedOutput.Value = this.RowOutputItems.FirstOrDefault(i => i.OutputType == AnalysisTableRowOutputType.JrdbValues);
        }
        if (this.SelectedOutput.Value == null)
        {
          this.SelectedOutput.Value = this.RowOutputItems.First();
        }
      }
      void UpdateOutput(AnalysisTableRowOutputItem? output)
      {
        if (output != null)
        {
          if (output.OutputType == AnalysisTableRowOutputType.JrdbValues)
          {
            if (this.SelectedJrdbOutput.Value != null)
            {
              var sub = this.SelectedJrdbOutput.Value.OutputType;
              this.Output.Value = sub;
            }
          }
          else if (output.OutputType == AnalysisTableRowOutputType.HorseValues)
          {
            if (this.SelectedSubOutput.Value != null)
            {
              var sub = this.SelectedSubOutput.Value.OutputType;
              this.Output.Value = sub;
            }
          }
          else
          {
            this.Output.Value = output.OutputType;
          }
        }
      }
      UpdateOutput(this.SelectedOutput.Value);
      this.SelectedOutput.Skip(1).Subscribe(output =>
      {
        UpdateOutput(output);
      }).AddTo(this._disposables);
      this.SelectedSubOutput.Skip(1).Subscribe(output =>
      {
        if (output != null)
        {
          if (this.SelectedOutput.Value?.OutputType == AnalysisTableRowOutputType.HorseValues)
          {
            this.Output.Value = output.OutputType;
          }
        }
      });
      this.SelectedJrdbOutput.Skip(1).Subscribe(output =>
      {
        if (output != null)
        {
          if (this.SelectedOutput.Value?.OutputType == AnalysisTableRowOutputType.JrdbValues)
          {
            this.Output.Value = output.OutputType;
          }
        }
      });

      finder.Input.Deserialize(data.FinderConfig);
      finder.Input.Query.Skip(1).Subscribe(async _ =>
      {
        // TODO try catch
        using var db = new MyContext();
        db.AnalysisTableRows!.Attach(this.Data);
        this.Data.FinderConfig = this.FinderModelForConfig.Input.Serialize(false);
        await db.SaveChangesAsync();
      }).AddTo(this._disposables);
      this.Output
        .CombineLatest(this.Name)
        .CombineLatest(this.BaseWeight)
        .CombineLatest(this.Limited)
        .CombineLatest(this.AlternativeValueIfEmpty)
        .CombineLatest(this.ValueScript)
        .Skip(1).Subscribe(async _ =>
        {
          // TODO try catch
          using var db = new MyContext();
          db.AnalysisTableRows!.Attach(this.Data);
          this.Data.Name = this.Name.Value;
          this.Data.Output = this.Output.Value;
          this.Data.ValueScript = this.ValueScript.Value;
          if (double.TryParse(this.BaseWeight.Value, out var bw))
          {
            this.Data.BaseWeight = bw;
          }
          if (short.TryParse(this.Limited.Value, out var lm))
          {
            this.Data.RequestedSize = lm;
          }
          if (double.TryParse(this.AlternativeValueIfEmpty.Value, out var anv))
          {
            this.Data.AlternativeValueIfEmpty = anv;
          }
          await db.SaveChangesAsync();
        }).AddTo(this._disposables);

      this._isInitialized = true;
    }

    public async Task LoadAsync(RaceData race, IReadOnlyList<RaceFinder> finders, IReadOnlyList<AnalysisTableWeight> weights, bool isCacheOnly = false, bool isBilk = false)
    {
      this.IsLoaded.Value = false;
      this.IsLoading.Value = true;

      List<Task> tasks = new();
      var keys = this.FinderModelForConfig.Input.Query.Value;
      var myWeights = weights.Where(w => w.Data.Id == this.Data.WeightId || w.Data.Id == this.Data.Weight2Id || w.Data.Id == this.Data.Weight3Id);

      // 一括実行時は意味のない行は調べない
      if (isBilk)
      {
        if (this.Data.Output != AnalysisTableRowOutputType.Binary && this.Data.BaseWeight == default && this.Data.AlternativeValueIfEmpty == default)
        {
          return;
        }
      }

      // 外部指数は最初にまとめて取得
      var externalNumbers = (IReadOnlyList<ExternalNumberData>)Array.Empty<ExternalNumberData>();
      if (this.Data.Output == AnalysisTableRowOutputType.ExternalNumber)
      {
        var exconfig = ExternalNumberUtil.GetConfig(this.Data.ExternalNumberId);
        if (exconfig != null)
        {
          using var db = new MyContext();
          externalNumbers = await ExternalNumberUtil.GetValuesAsync(db, race.Key);
        }
      }

      foreach (var item in this.Cells.Join(finders, c => c.Horse.Data.Key, f => f.RaceHorse?.Key, (c, f) => new { Cell = c, Finder = f, }))
      {
        // 親ブーリアンがFALSEなら、子セルを調べる必要なし
        if (this.SelectedParent.Value != null && this.SelectedParent.Value.Data.Output == AnalysisTableRowOutputType.Binary)
        {
          var targetCell = this.SelectedParent.Value.Cells.FirstOrDefault(c => c.Horse.Data.Key == item.Cell.Horse.Data.Key);
          if (targetCell != null)
          {
            if (targetCell.IsSkipped.Value)
            {
              continue;
            }
          }
        }

        var query = keys;
        var size = this.FinderModelForConfig.DefaultSize;
        if (this.Data.Output == AnalysisTableRowOutputType.Binary)
        {
          query += "|[currentonly]";
          size = 1;
        }
        else
        {
          query += "|datastatus=5-7";
        }

        var cache = item.Finder.TryFindRaceHorseCache(query);
        if (cache != null)
        {
          var type = this.SelectedOutput.Value?.OutputType;
          if (type != AnalysisTableRowOutputType.JrdbValues &&
            type != AnalysisTableRowOutputType.HorseValues &&
            type != AnalysisTableRowOutputType.FixedValue &&
            type != AnalysisTableRowOutputType.ExpansionMemo &&
            type != AnalysisTableRowOutputType.ExternalNumber)
          {
            // OutputType = FixedValueの場合、そもそも検索はしないのでキャッシュも来ない。処理不要

            this.AnalysisSource(cache, myWeights, item.Cell, item.Finder);

            this.IsLoaded.Value = true;
            this.IsLoading.Value = false;
          }
        }

        if (!isCacheOnly && !this.IsLoaded.Value)
        {
          var task = Task.Run(async () =>
          {
            if (this.Data.Output == AnalysisTableRowOutputType.FixedValue)
            {
              this.AnalysisFixedValue(1, myWeights, item.Cell, item.Finder);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.ExpansionMemo)
            {
              if (this.SelectedMemoConfig.Value != null && item.Finder.Race != null)
              {
                using var db = new MyContext();
                var memo = await MemoUtil.GetMemoAsync(db, item.Finder.Race, this.SelectedMemoConfig.Value, item.Finder.RaceHorseAnalyzer);
                this.AnalysisFixedValue(memo?.Point ?? 0, myWeights, item.Cell, item.Finder, digit: 0);
              }
              else
              {
                this.SetPointOrEmpty(item.Cell, -1, myWeights);
              }
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.ExternalNumber)
            {
              var exconfig = ExternalNumberUtil.GetConfig(this.Data.ExternalNumberId);
              var exNumber = externalNumbers.FirstOrDefault(n => n.HorseNumber == item.Cell.Horse.Data.Number && n.ConfigId == this.Data.ExternalNumberId);
              if (exconfig != null && exNumber != null)
              {
                item.Cell.ComparationValue.Value = -exNumber.Order;
                item.Cell.Value.Value = ((decimal)exNumber.Value / 100).ToString();
                item.Cell.SubValue.Value = exNumber.Order.ToString();
                item.Cell.PointCalcValue.Value = (float)exNumber.Value / 100;
                item.Cell.Point.Value = item.Cell.PointCalcValue.Value * this.Data.BaseWeight / 100;
                item.Cell.HasComparationValue.Value = exNumber.Order != default;

                if (myWeights.Any())
                {
                  var weightValue = myWeights.GetWeight(item.Cell.Horse) * this.Data.BaseWeight;
                  item.Cell.Weight = weightValue;
                  item.Cell.Point.Value = item.Cell.PointCalcValue.Value * weightValue;
                }
              }
              else
              {
                this.SetPointOrEmpty(item.Cell, -1, myWeights);
              }
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.RiderWeight)
            {
              this.AnalysisFixedValue(item.Cell.Horse.Data.RiderWeight / 10.0, myWeights, item.Cell, item.Finder, digit: 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.RiderWeightDiff)
            {
              this.AnalysisFixedValue(item.Cell.Horse.Data.RiderWeight / 10.0 - 55, myWeights, item.Cell, item.Finder, digit: 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.Odds)
            {
              this.AnalysisFixedValue(item.Cell.Horse.Data.Odds / 10.0, myWeights, item.Cell, item.Finder, digit: 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.Age)
            {
              this.AnalysisFixedValue(item.Cell.Horse.Data.Age, myWeights, item.Cell, item.Finder, digit: 0);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.Weight)
            {
              this.AnalysisFixedValue(item.Cell.Horse.Data.Weight, myWeights, item.Cell, item.Finder, digit: 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.WeightDiff)
            {
              this.AnalysisFixedValue(item.Cell.Horse.Data.WeightDiff, myWeights, item.Cell, item.Finder, digit: 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.PciAverage)
            {
              if (item.Cell.Horse.History == null)
              {
                this.SetPointOrEmpty(item.Cell, -1, myWeights);
              }
              else
              {
                this.AnalysisFixedValue(item.Cell.Horse.History.PciAverage, myWeights, item.Cell, item.Finder, digit: 1);
              }
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.RiderPoint)
            {
              this.TrySetJrdbValue(j => j.RiderPoint / 10.0, myWeights, item.Cell, item.Finder, 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.InfoPoint)
            {
              this.TrySetJrdbValue(j => j.InfoPoint / 10.0, myWeights, item.Cell, item.Finder, 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.TotalPoint)
            {
              this.TrySetJrdbValue(j => j.TotalPoint / 10.0, myWeights, item.Cell, item.Finder, 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.IdmPoint)
            {
              this.TrySetJrdbValue(j => j.IdmPoint / 10.0, myWeights, item.Cell, item.Finder, 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.PopularPoint)
            {
              this.TrySetJrdbValue(j => j.PopularPoint / 10.0, myWeights, item.Cell, item.Finder, 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.StablePoint)
            {
              this.TrySetJrdbValue(j => j.StablePoint / 10.0, myWeights, item.Cell, item.Finder, 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.TrainingPoint)
            {
              this.TrySetJrdbValue(j => j.TrainingPoint / 10.0, myWeights, item.Cell, item.Finder, 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.RiderTopRatioExpectPoint)
            {
              this.TrySetJrdbValue(j => j.RiderTopRatioExpectPoint / 10.0, myWeights, item.Cell, item.Finder, 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.SpeedPoint)
            {
              this.TrySetJrdbValue(j => j.SpeedPoint / 10.0, myWeights, item.Cell, item.Finder, 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.RaceBefore3Point)
            {
              this.TrySetJrdbValue(j => j.RaceBefore3Point / 10.0, myWeights, item.Cell, item.Finder, 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.RaceBasePoint)
            {
              this.TrySetJrdbValue(j => j.RaceBasePoint / 10.0, myWeights, item.Cell, item.Finder, 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.RaceAfter3Point)
            {
              this.TrySetJrdbValue(j => j.RaceAfter3Point / 10.0, myWeights, item.Cell, item.Finder, 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.RacePositionPoint)
            {
              this.TrySetJrdbValue(j => j.RacePositionPoint / 10.0, myWeights, item.Cell, item.Finder, 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.RaceStartPoint)
            {
              this.TrySetJrdbValue(j => j.RaceStartPoint / 10.0, myWeights, item.Cell, item.Finder, 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.RaceStartDelayPoint)
            {
              this.TrySetJrdbValue(j => j.RaceStartDelayPoint / 10.0, myWeights, item.Cell, item.Finder, 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.BigTicketPoint)
            {
              this.TrySetJrdbValue(j => j.BigTicketPoint / 10.0, myWeights, item.Cell, item.Finder, 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.IdentificationMarkCount1)
            {
              this.TrySetJrdbValue(j => j.IdentificationMarkCount1, myWeights, item.Cell, item.Finder, 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.IdentificationMarkCount2)
            {
              this.TrySetJrdbValue(j => j.IdentificationMarkCount2, myWeights, item.Cell, item.Finder, 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.IdentificationMarkCount3)
            {
              this.TrySetJrdbValue(j => j.IdentificationMarkCount3, myWeights, item.Cell, item.Finder, 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.IdentificationMarkCount4)
            {
              this.TrySetJrdbValue(j => j.IdentificationMarkCount4, myWeights, item.Cell, item.Finder, 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.IdentificationMarkCount5)
            {
              this.TrySetJrdbValue(j => j.IdentificationMarkCount5, myWeights, item.Cell, item.Finder, 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.TotalMarkCount1)
            {
              this.TrySetJrdbValue(j => j.TotalMarkCount1, myWeights, item.Cell, item.Finder, 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.TotalMarkCount2)
            {
              this.TrySetJrdbValue(j => j.TotalMarkCount2, myWeights, item.Cell, item.Finder, 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.TotalMarkCount3)
            {
              this.TrySetJrdbValue(j => j.TotalMarkCount3, myWeights, item.Cell, item.Finder, 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.TotalMarkCount4)
            {
              this.TrySetJrdbValue(j => j.TotalMarkCount4, myWeights, item.Cell, item.Finder, 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.TotalMarkCount5)
            {
              this.TrySetJrdbValue(j => j.TotalMarkCount5, myWeights, item.Cell, item.Finder, 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.MiningTime)
            {
              await this.TrySetExtraValueAsync(e => e.MiningTime / 10.0, item.Cell.Horse, myWeights, item.Cell, item.Finder, 1);
            }
            else if (this.Data.Output == AnalysisTableRowOutputType.MiningMatch)
            {
              await this.TrySetExtraValueAsync(e => e.MiningMatchScore / 10.0, item.Cell.Horse, myWeights, item.Cell, item.Finder, 1);
            }
            else
            {
              var source = await item.Finder.FindRaceHorsesAsync(query, size, withoutFutureRaces: true, withoutFutureRacesForce: true);
              this.AnalysisSource(source, myWeights, item.Cell, item.Finder);
            }
          });
          tasks.Add(task);
        }
      }

      if (!isCacheOnly)
      {
        await Task.WhenAll(tasks);

        this.IsLoaded.Value = true;
        this.IsLoading.Value = false;
      }

      this.UpdateComparation();
    }

    private void TrySetJrdbValue(Func<JrdbRaceHorseData, double> point, IEnumerable<AnalysisTableWeight> weights, AnalysisTableCell cell, RaceFinder finder, int digit)
    {
      var horse = cell.Horse;
      if (horse.JrdbData != null)
      {
        this.AnalysisFixedValue(point(horse.JrdbData), weights, cell, finder, digit: digit);
      }
      else
      {
        this.SetPointOrEmpty(cell, -1, weights);
      }
    }

    private async Task TrySetExtraValueAsync(Func<RaceHorseExtraData, double> point, RaceHorseAnalyzer horse, IEnumerable<AnalysisTableWeight> weights, AnalysisTableCell cell, RaceFinder finder, int digit)
    {
      if (!horse.IsCheckedExtraData && horse.ExtraData == null)
      {
        using var db = new MyContext();
        horse.ExtraData = await db.RaceHorseExtras!.FirstOrDefaultAsync(e => e.Key == horse.Data.Key && e.RaceKey == horse.Data.RaceKey);
        horse.IsCheckedExtraData = true;
      }

      if (horse.ExtraData != null)
      {
        this.AnalysisFixedValue(point(horse.ExtraData), weights, cell, finder, digit: digit);
      }
      else
      {
        this.SetPointOrEmpty(cell, -1, weights);
      }
    }

    private void AnalysisSource(RaceHorseFinderQueryResult source, IEnumerable<AnalysisTableWeight> weights, AnalysisTableCell cell, RaceFinder finder)
    {
      if (this.Data.Output == AnalysisTableRowOutputType.Binary)
      {
        // 指定した馬が存在するか
        var items = source.Items;
        var isAny = source.Items.Any(h => h.Data.Key == cell.Horse.Data.Key);

        cell.ComparationValue.Value = isAny ? 1 : 0;
        cell.PointCalcValue.Value = isAny ? 1 : 0;
        cell.HasComparationValue.Value = true;
        cell.IsSkipped.Value = !isAny;
        cell.SampleSize = 0;

        if (isAny)
        {
          this.AnalysisFixedValue(1, weights, cell, finder);
        }
        else if (!isAny)
        {
          this.AnalysisFixedValue(this.Data.AlternativeValueIfEmpty, weights, cell, finder);
        }

        cell.Value.Value = isAny ? "●" : string.Empty;
        cell.ScriptValue.Value = isAny ? "true" : "false";
      }
      else
      {
        var items = source.Items;

        this.SetValueOfRaceHorseAnalyzer(finder, this.Data.Output, source, cell, weights);
        this.SetPointOrEmpty(cell, items.Count, weights);

        var samples = items.Where(cell.SampleFilter).Take(10);
        ThreadUtil.InvokeOnUiThread(() =>
        {
          cell.Samples.Clear();
          foreach (var sample in samples)
          {
            cell.Samples.Add(sample);
          }
        });
      }
    }

    private void SetPointOrEmpty(AnalysisTableCell cell, int count, IEnumerable<AnalysisTableWeight> weights)
    {
      if (count >= this.Data.RequestedSize)
      {
        var value = this.ExecuteScriptValue(cell.PointCalcValue.Value, cell);
        cell.Point.Value = value * this.Data.BaseWeight;
      }
      if (count < this.Data.RequestedSize || count == 0)
      {
        var value = this.ExecuteScriptValue(this.Data.AlternativeValueIfEmpty * this.Data.BaseWeight * weights.GetWeight(cell.Horse), cell);
        cell.Point.Value = value;
        if (count == 0)
        {
          cell.Value.Value = cell.SubValue.Value = string.Empty;
        }
        cell.HasComparationValue.Value = true;
      }
    }

    private double ExecuteScriptValue(double value, AnalysisTableCell cell)
    {
      cell.IsScriptError.Value = false;

      if (this.CanSetScript.Value)
      {
        if (!string.IsNullOrWhiteSpace(this.Data.ValueScript) && this.Data.ValueScript.Trim() != "value")
        {
          using var engine = new StringScriptEngine();

          var scriptVariables = new StringBuilder();
          if (this.Data.ValueScript.Contains("race"))
          {
            var scriptRace = new ScriptRace(cell.Horse.Race);
            var raceJson = scriptRace.ToJson();
            scriptVariables.Append("const race=");
            scriptVariables.Append(raceJson);
            scriptVariables.Append(';');
          }
          if (this.Data.ValueScript.Contains("horse"))
          {
            var scriptHorse = new ScriptRaceHorse(cell.Horse.Race.Key, cell.Horse, false);
            var horseJson = scriptHorse.ToJson();
            scriptVariables.Append("const horse=");
            scriptVariables.Append(horseJson);
            scriptVariables.Append(';');
          }
          if (this.Data.ValueScript.Contains('$'))
          {
            var values = string.Join(';', this.Table.Rows
              .Where(r => r.Data.Order < this.Data.Order)
              .Select(r => new { RowId = r.Data.Id, Cell = r.Cells.FirstOrDefault(c => c.Horse.Data.Id == cell.Horse.Data.Id), })
              .Where(c => c.Cell != null)
              .Select(c => $"const ${c!.RowId}={(!string.IsNullOrEmpty(c.Cell!.ScriptValue.Value) ? c.Cell!.ScriptValue.Value : "0")}"));
            scriptVariables.Append(values);
            scriptVariables.Append(';');
          }

          try
          {
            var result = engine.Execute($"{scriptVariables} let value = {value}; " + this.Data.ValueScript);
            if (result is int intval)
            {
              value = intval;
            }
            else if (result is double doval)
            {
              value = doval;
            }
            else if (result is float fval)
            {
              value = fval;
            }
            else
            {
              cell.IsScriptError.Value = true;
            }
          }
          catch (Exception ex)
          {
            cell.IsScriptError.Value = true;
            logger.Error($"スクリプトエラー {value}", ex);
          }
        }
      }

      return value;
    }

    private void AnalysisFixedValue(double value, IEnumerable<AnalysisTableWeight> weights, AnalysisTableCell cell, RaceFinder finder, int digit = 3)
    {
      cell.SubValue.Value = string.Empty;
      value = this.ExecuteScriptValue(value, cell);

      if (weights.Any() && finder.RaceHorseAnalyzer != null)
      {
        var weightValue = weights.CalcWeight(new[] { finder.RaceHorseAnalyzer, }) * this.Data.BaseWeight;
        cell.Weight = weightValue;
        cell.PointCalcValue.Value = value;
        cell.Point.Value = cell.PointCalcValue.Value * weightValue;
      }
      else
      {
        cell.Weight = this.Data.BaseWeight;
        cell.PointCalcValue.Value = value;
        cell.Point.Value = cell.PointCalcValue.Value * this.Data.BaseWeight;
      }
      cell.Value.Value = value.ToString("N" + digit);
      cell.ScriptValue.Value = value.ToString();
      cell.ComparationValue.Value = (float)cell.Point.Value;
      cell.SampleSize = 0;
      cell.HasComparationValue.Value = true;
    }

    private void SetValueOfRaceHorseAnalyzer(RaceFinder finder, AnalysisTableRowOutputType value, RaceHorseFinderQueryResult source, AnalysisTableCell cell, IEnumerable<AnalysisTableWeight> weights)
    {
      var analyzerSlim = source.AnalyzerSlim;

      if (value == AnalysisTableRowOutputType.Time)
      {
        cell.Value.Value = analyzerSlim.TimeDeviationValue.ToString("F1");
        cell.ComparationValue.Value = (float)analyzerSlim.TimeDeviationValue;
        cell.HasComparationValue.Value = analyzerSlim.AllGrade.AllCount > 0;
        cell.PointCalcValue.Value = analyzerSlim.TimeDeviationValue / 100;
        cell.SampleSize = 0;
      }
      else if (value == AnalysisTableRowOutputType.A3HTime)
      {
        cell.Value.Value = analyzerSlim.A3HTimeDeviationValue.ToString("F1");
        cell.ComparationValue.Value = (float)analyzerSlim.A3HTimeDeviationValue;
        cell.HasComparationValue.Value = analyzerSlim.AllGrade.AllCount > 0;
        cell.PointCalcValue.Value = analyzerSlim.A3HTimeDeviationValue / 100;
        cell.SampleSize = 0;
      }
      else if (value == AnalysisTableRowOutputType.UA3HTime)
      {
        cell.Value.Value = analyzerSlim.UntilA3HTimeDeviationValue.ToString("F1");
        cell.ComparationValue.Value = (float)analyzerSlim.UntilA3HTimeDeviationValue;
        cell.HasComparationValue.Value = analyzerSlim.AllGrade.AllCount > 0;
        cell.PointCalcValue.Value = analyzerSlim.UntilA3HTimeDeviationValue / 100;
        cell.SampleSize = 0;
      }
      else if (value == AnalysisTableRowOutputType.ShortestTime)
      {
        if (finder.Race == null || finder.Race.Distance <= 0)
        {
          return;
        }
        Func<RaceHorseAnalyzer, bool> filter = s => s.Data.ResultOrder >= 1 && s.Data.ResultTime != TimeSpan.Zero && s.Race.Distance > 0;
        var timePerMeters = source.Items
          .Where(filter)
          .Select(s => new { Time = s.Data.ResultTime.TotalSeconds / s.Race.Distance * finder.Race.Distance, Analyzer = s, })
          .Select(s => new { Time = TimeSpan.FromSeconds(s.Time), s.Analyzer, })
          .ToArray();
        if (timePerMeters.Any())
        {
          var shortestTime = timePerMeters.OrderBy(s => s.Time).First();
          cell.Value.Value = shortestTime.Time.ToString("m\\:ss\\.f");
          cell.SubValue.Value = timePerMeters.Length.ToString();
          cell.ComparationValue.Value = (float)shortestTime.Time.TotalSeconds * -1;
          cell.HasComparationValue.Value = true;
          cell.PointCalcValue.Value = shortestTime.Time.TotalSeconds / 100;
          cell.SampleSize = 1;
          cell.SampleFilter = filter;
          cell.ScriptValue.Value = shortestTime.Time.TotalSeconds.ToString();

          if (weights.Any())
          {
            cell.PointCalcValue.Value *= weights.GetWeight(shortestTime.Analyzer);
          }
        }
      }
      else if (value == AnalysisTableRowOutputType.RecoveryRate)
      {
        cell.Value.Value = analyzerSlim.RecoveryRate.ToString("P1");
        cell.ComparationValue.Value = (float)analyzerSlim.RecoveryRate;
        cell.HasComparationValue.Value = analyzerSlim.AllGrade.AllCount > 0;
        cell.PointCalcValue.Value = analyzerSlim.RecoveryRate;
        cell.SampleSize = 0;
        cell.ScriptValue.Value = analyzerSlim.RecoveryRate.ToString();

        if (weights.Any() && source.Items.Any())
        {
          var calc = 0.0;
          foreach (var item in source.AsItems())
          {
            if (item.Analyzer.Data.ResultOrder == 1)
            {
              calc += weights.GetWeight(item.Analyzer) * item.Analyzer.Data.Odds * 10;
            }
          }
          cell.PointCalcValue.Value = calc / (source.Items.Count(i => i.Data.ResultOrder > 0) * 100);
        }
      }
      else if (value == AnalysisTableRowOutputType.FixedValuePerPastRace)
      {
        if (weights.Any() && source.Items.Any())
        {
          var calc = 0.0;
          foreach (var item in source.Items)
          {
            calc += weights.GetWeight(item);
          }
          cell.PointCalcValue.Value = calc / source.Items.Count;

          cell.Value.Value = cell.PointCalcValue.Value.ToString("P1");
          cell.SubValue.Value = source.Items.Count.ToString();
          cell.ComparationValue.Value = (float)cell.PointCalcValue.Value;
          cell.ScriptValue.Value = cell.PointCalcValue.Value.ToString();
          cell.HasComparationValue.Value = true;
        }
        else
        {
          this.SetPointOrEmpty(cell, -1, weights);
        }
      }
      else
      {
        var grade = analyzerSlim.AllGrade;
        if (value == AnalysisTableRowOutputType.PlaceBetsRate)
        {
          cell.Value.Value = grade.PlacingBetsRate.ToString("P1");
          cell.SubValue.Value = $"{grade.PlacingBetsCount} / {grade.AllCount}";
          cell.ScriptValue.Value = grade.PlacingBetsRate.ToString();
          cell.ComparationValue.Value = grade.PlacingBetsRate;
          cell.HasComparationValue.Value = grade.AllCount > 0;
          cell.PointCalcValue.Value = grade.PlacingBetsRate;

          if (weights.Any() && source.Items.Any())
          {
            var calc = 0.0;
            foreach (var item in source.Items)
            {
              if (item.Data.ResultOrder > 0 && item.Data.ResultOrder <= (item.Race.ResultHorsesCount <= 7 ? 2 : 3))
              {
                calc += weights.GetWeight(item);
              }
            }
            cell.PointCalcValue.Value = calc / source.Items.Count;
          }
        }
        if (value == AnalysisTableRowOutputType.WinRate)
        {
          cell.Value.Value = grade.WinRate.ToString("P1");
          cell.SubValue.Value = $"{grade.FirstCount} / {grade.AllCount}";
          cell.ScriptValue.Value = grade.WinRate.ToString();
          cell.ComparationValue.Value = grade.WinRate;
          cell.HasComparationValue.Value = grade.AllCount > 0;
          cell.PointCalcValue.Value = grade.WinRate;

          if (weights.Any() && source.Items.Any())
          {
            var calc = 0.0;
            foreach (var item in source.Items)
            {
              if (item.Data.ResultOrder == 1)
              {
                calc += weights.GetWeight(item);
              }
            }
            cell.PointCalcValue.Value = calc / source.Items.Count;
          }
        }
      }
    }

    private void UpdateComparation()
    {
      if (this.Cells.Any())
      {
        if (this.Cells.Count() >= 4 && !this.Cells.All(c => c.ComparationValue.Value == float.MinValue))
        {
          var max = this.Cells.OrderByDescending(c => c.ComparationValue.Value).ElementAtOrDefault(2)?.ComparationValue.Value ?? default;
          var min = this.Cells.OrderBy(c => c.ComparationValue.Value).ElementAtOrDefault(2)?.ComparationValue.Value ?? default;
          foreach (var cell in this.Cells)
          {
            cell.Comparation.Value = AnalysisUtil.CompareValue(cell.ComparationValue.Value, max, min);
          }
        }
      }
    }

    public void OnParentListUpdated()
    {
      this.IsFreezeParentSelection = true;
      this.SelectedParent.Value = this.Table.ParentRowSelections.FirstOrDefault(r => r.Data.Id == this.Data.ParentRowId);
      this.IsFreezeParentSelection = false;
    }

    public void ReloadMemoConfigProperty()
    {
      this.SelectedMemoConfig.Value = MemoUtil.Configs.FirstOrDefault(c => c.Id == this.Data.MemoConfigId);
    }

    public void ReloadExternalNumbersProperty(IEnumerable<ExternalNumberConfigItem> configs)
    {
      if (!this._isInitialized)
      {
        return;
      }
      this.SelectedExternalNumber.Value = configs.FirstOrDefault(c => c.Data.Id == this.Data.ExternalNumberId);
    }

    public void Dispose()
    {
      this.FinderModelForConfig.Dispose();
      this._disposables.Dispose();
    }
  }

  public class AnalysisTableCell
  {
    public RaceHorseAnalyzer Horse { get; }

    public ReactiveProperty<string> Value { get; } = new();

    public ReactiveProperty<string> SubValue { get; } = new();

    public ReactiveProperty<double> Point { get; } = new();

    public ReactiveProperty<string> ScriptValue { get; } = new();

    public ReactiveProperty<float> ComparationValue { get; } = new();

    public ReactiveProperty<bool> HasComparationValue { get; } = new();

    public ReactiveProperty<bool> IsSkipped { get; } = new();

    public ReactiveProperty<bool> IsScriptError { get; } = new();

    public ReactiveProperty<ValueComparation> Comparation { get; } = new();

    public ReactiveProperty<double> PointCalcValue { get; } = new();

    public int SampleSize { get; set; }

    public Func<RaceHorseAnalyzer, bool> SampleFilter { get; set; } = DefaultSampleFilter;

    public double Weight { get; set; }

    public ReactiveCollection<RaceHorseAnalyzer> Samples { get; } = new();

    public AnalysisTableCell(RaceHorseAnalyzer horse)
    {
      this.Horse = horse;
    }

    private static bool DefaultSampleFilter(RaceHorseAnalyzer _)
    {
      return true;
    }
  }
}
