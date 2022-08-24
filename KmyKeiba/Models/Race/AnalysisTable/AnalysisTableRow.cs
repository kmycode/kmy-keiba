using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race.ExNumber;
using KmyKeiba.Models.Race.Finder;
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
    private readonly CompositeDisposable _disposables = new();
    private bool _isInitializingParentList = false;

    public AnalysisTableRowData Data { get; }

    public AnalysisTableSurface Table { get; }

    public FinderModel FinderModelForConfig { get; }

    public ReactiveProperty<string> Name { get; } = new();

    public ReactiveProperty<string> BaseWeight { get; } = new();

    public ReactiveProperty<string> Limited { get; } = new();

    public ReactiveProperty<AnalysisTableRowOutputType> Output { get; } = new();

    public ReadOnlyReactiveProperty<bool> CanSetExternalNumber { get; }

    public ReadOnlyReactiveProperty<bool> CanSetQuery { get; }

    public ReadOnlyReactiveProperty<bool> CanSetWeight { get; }

    public ReadOnlyReactiveProperty<bool> CanSetLimited { get; }

    public ReactiveCollection<AnalysisTableCell> Cells { get; } = new();

    public ReactiveProperty<AnalysisTableWeight?> Weight { get; } = new();

    public ReactiveProperty<AnalysisTableWeight?> Weight2 { get; } = new();

    public ReactiveProperty<AnalysisTableWeight?> Weight3 { get; } = new();

    public List<AnalysisTableRowOutputItem> RowOutputItems => AnalysisTableUtil.RowOutputItems;

    public ReactiveProperty<AnalysisTableRowOutputItem?> SelectedOutput { get; } = new();

    public ReactiveProperty<AnalysisTableRow?> SelectedParent { get; } = new();

    public ReactiveProperty<ExternalNumberConfigItem?> SelectedExternalNumber { get; } = new();

    public ReactiveProperty<bool> IsLoading { get; } = new();

    public ReactiveProperty<bool> IsLoaded { get; } = new();

    public AnalysisTableRow(AnalysisTableRowData data, AnalysisTableSurface table, IEnumerable<RaceHorseAnalyzer> horses)
    {
      this.Data = data;
      this.Table = table;

      foreach (var horse in horses.Select(h => new AnalysisTableCell(h)))
      {
        this.Cells.Add(horse);
      }

      var dummyRace = new RaceData();
      var finder = new FinderModel(dummyRace, new Analysis.RaceHorseAnalyzer(dummyRace, new RaceHorseData(), null), null);
      this.FinderModelForConfig = finder;

      this.Name.Value = data.Name;
      this.Output.Value = data.Output;
      this.BaseWeight.Value = data.BaseWeight.ToString();
      this.Limited.Value = data.RequestedSize.ToString();

      this.CanSetExternalNumber = this.Output.Select(o => o == AnalysisTableRowOutputType.ExternalNumber).ToReadOnlyReactiveProperty().AddTo(this._disposables);
      this.CanSetQuery = this.Output.Select(o => o != AnalysisTableRowOutputType.ExternalNumber &&
        o != AnalysisTableRowOutputType.FixedValue &&
        o != AnalysisTableRowOutputType.FixedValuePerPastRace).ToReadOnlyReactiveProperty().AddTo(this._disposables);
      this.CanSetWeight = this.Output.Select(o => o == AnalysisTableRowOutputType.FixedValue ||
        o == AnalysisTableRowOutputType.FixedValuePerPastRace ||
        o == AnalysisTableRowOutputType.PlaceBetsRate ||
        o == AnalysisTableRowOutputType.RecoveryRate ||
        o == AnalysisTableRowOutputType.WinRate).ToReadOnlyReactiveProperty().AddTo(this._disposables);
      this.CanSetLimited = this.Output.Select(o => o == AnalysisTableRowOutputType.PlaceBetsRate ||
        o == AnalysisTableRowOutputType.RecoveryRate ||
        o == AnalysisTableRowOutputType.WinRate ||
        o == AnalysisTableRowOutputType.ShortestTime).ToReadOnlyReactiveProperty().AddTo(this._disposables);

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
        if (this._isInitializingParentList)
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
        // TODO try catch
        using var db = new MyContext();
        db.AnalysisTableRows!.Attach(this.Data);
        this.Data.ExternalNumberId = this.SelectedExternalNumber.Value?.Data.Id ?? 0;
        await db.SaveChangesAsync();
      }).AddTo(this._disposables);

      this.SelectedOutput.Value = this.RowOutputItems.FirstOrDefault(i => i.OutputType == data.Output);
      if (this.SelectedOutput.Value == null)
      {
        this.SelectedOutput.Value = this.RowOutputItems.First();
      }
      this.Output.Value = this.SelectedOutput.Value.OutputType;
      this.SelectedOutput.Skip(1).Subscribe(_ =>
      {
        this.Output.Value = this.SelectedOutput.Value.OutputType;
      }).AddTo(this._disposables);

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
        .Skip(1).Subscribe(async _ =>
        {
          // TODO try catch
          using var db = new MyContext();
          db.AnalysisTableRows!.Attach(this.Data);
          this.Data.Name = this.Name.Value;
          this.Data.Output = this.Output.Value;
          if (double.TryParse(this.BaseWeight.Value, out var bw))
          {
            this.Data.BaseWeight = bw;
          }
          if (short.TryParse(this.Limited.Value, out var lm))
          {
            this.Data.RequestedSize = lm;
          }
          await db.SaveChangesAsync();
        }).AddTo(this._disposables);
    }

    public async Task LoadAsync(RaceData race, IReadOnlyList<RaceFinder> finders, IReadOnlyList<AnalysisTableWeight> weights, bool isCacheOnly = false)
    {
      this.IsLoading.Value = true;

      List<Task> tasks = new();
      var keys = this.FinderModelForConfig.Input.Query.Value;
      var myWeights = weights.Where(w => w.Data.Id == this.Data.WeightId || w.Data.Id == this.Data.Weight2Id || w.Data.Id == this.Data.Weight3Id);

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
        var size = 3000;
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
          // OutputType = FixedValueの場合、そもそも検索はしないのでキャッシュも来ない。処理不要

          this.AnalysisSource(cache, myWeights, item.Cell, item.Finder);

          this.IsLoaded.Value = true;
          this.IsLoading.Value = false;
        }
        else if (!isCacheOnly)
        {
          var task = Task.Run(async () =>
          {
            if (this.Data.Output == AnalysisTableRowOutputType.FixedValue)
            {
              this.AnalysisFixedValue(myWeights, item.Cell, item.Finder);
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
              }
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

    private void AnalysisSource(RaceHorseFinderQueryResult source, IEnumerable<AnalysisTableWeight> weights, AnalysisTableCell cell, RaceFinder finder)
    {
      if (this.Data.Output == AnalysisTableRowOutputType.Binary)
      {
        // 指定した馬が存在するか
        var items = source.Items;
        var isAny = source.Items.Any(h => h.Data.Key == cell.Horse.Data.Key);

        cell.ComparationValue.Value = isAny ? 1 : 0;
        cell.PointCalcValue.Value = isAny ? 1 : 0;
        cell.Value.Value = isAny ? "●" : string.Empty;
        cell.HasComparationValue.Value = isAny;
        cell.IsSkipped.Value = !isAny;
        cell.SampleSize = 0;

        if (weights.Any() && isAny)
        {
          var weightValue = weights.CalcWeight(items) * this.Data.BaseWeight;
          cell.Weight = weightValue;
          cell.Point.Value = cell.PointCalcValue.Value * weightValue;
        }
        else
        {
          cell.Point.Value = cell.PointCalcValue.Value * this.Data.BaseWeight;
        }
      }
      else if (this.Data.Output == AnalysisTableRowOutputType.FixedValue)
      {
        // 固定値を使用。純粋に重みをそのままポイントに転換する
        this.AnalysisFixedValue(weights, cell, finder);
      }
      else
      {
        var items = source.Items;

        this.SetValueOfRaceHorseAnalyzer(finder, this.Data.Output, source, cell, weights);

        if (items.Count >= this.Data.RequestedSize)
        {
          cell.Point.Value = cell.PointCalcValue.Value * this.Data.BaseWeight;
        }

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

    private void AnalysisFixedValue(IEnumerable<AnalysisTableWeight> weights, AnalysisTableCell cell, RaceFinder finder)
    {
      if (weights.Any() && finder.RaceHorseAnalyzer != null)
      {
        var weightValue = weights.CalcWeight(new[] { finder.RaceHorseAnalyzer, }) * this.Data.BaseWeight;
        cell.Weight = weightValue;
        cell.PointCalcValue.Value = 1;
        cell.Point.Value = cell.PointCalcValue.Value * weightValue;
        cell.Value.Value = cell.Point.Value.ToString("N3");
      }
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
          cell.PointCalcValue.Value = shortestTime.Time.TotalSeconds;
          cell.SampleSize = 1;
          cell.SampleFilter = filter;

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
          cell.HasComparationValue.Value = true;
        }
      }
      else
      {
        var grade = analyzerSlim.AllGrade;
        if (value == AnalysisTableRowOutputType.PlaceBetsRate)
        {
          cell.Value.Value = grade.PlacingBetsRate.ToString("P1");
          cell.SubValue.Value = $"{grade.PlacingBetsCount} / {grade.AllCount}";
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
      this._isInitializingParentList = true;
      this.SelectedParent.Value = this.Table.ParentRowSelections.FirstOrDefault(r => r.Data.Id == this.Data.ParentRowId);
      this._isInitializingParentList = false;
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

    public ReactiveProperty<float> ComparationValue { get; } = new();

    public ReactiveProperty<bool> HasComparationValue { get; } = new();

    public ReactiveProperty<bool> IsSkipped { get; } = new();

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
