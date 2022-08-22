using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
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

    public AnalysisTableRowData Data { get; }

    public FinderModel FinderModelForConfig { get; }

    public ReactiveProperty<string> Name { get; } = new();

    public ReactiveProperty<double> BaseWeight { get; } = new(1.0);

    public ReactiveProperty<AnalysisTableRowOutputType> Output { get; } = new();

    public ReactiveCollection<AnalysisTableCell> Cells { get; } = new();

    public ReactiveProperty<bool> IsLoading { get; } = new();

    public ReactiveProperty<bool> IsLoaded { get; } = new();

    public AnalysisTableRow(AnalysisTableRowData data, IEnumerable<RaceHorseAnalyzer> horses)
    {
      this.Data = data;

      foreach (var horse in horses.Select(h => new AnalysisTableCell(h)))
      {
        this.Cells.Add(horse);
      }

      var dummyRace = new RaceData();
      var finder = new FinderModel(dummyRace, new Analysis.RaceHorseAnalyzer(dummyRace, new RaceHorseData(), null), null);
      this.FinderModelForConfig = finder;

      this.Name.Value = data.Name;
      this.Output.Value = data.Output;
      this.BaseWeight.Value = data.BaseWeight;

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
        .Skip(1).Subscribe(async _ =>
        {
          // TODO try catch
          using var db = new MyContext();
          db.AnalysisTableRows!.Attach(this.Data);
          this.Data.Name = this.Name.Value;
          this.Data.Output = this.Output.Value;
          this.Data.BaseWeight = this.BaseWeight.Value;
          await db.SaveChangesAsync();
        }).AddTo(this._disposables);
    }

    public async Task LoadAsync(RaceData race, IReadOnlyList<RaceFinder> finders, IReadOnlyList<AnalysisTableWeight> weights, bool isCacheOnly = false)
    {
      this.IsLoading.Value = true;

      List<Task> tasks = new();
      var keys = this.FinderModelForConfig.Input.Query.Value;

      var weight = weights.FirstOrDefault(w => w.Data.Id == this.Data.WeightId);

      foreach (var item in this.Cells.Join(finders, c => c.Horse.Data.Key, f => f.RaceHorse?.Key, (c, f) => new { Cell = c, Finder = f, }))
      {
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

          this.AnalysisSource(cache, weight, item.Cell, item.Finder);
          this.IsLoaded.Value = true;
          this.IsLoading.Value = false;
        }
        else if (!isCacheOnly)
        {
          var task = Task.Run(async () =>
          {
            if (this.Data.Output != AnalysisTableRowOutputType.FixedValue)
            {
              var source = await item.Finder.FindRaceHorsesAsync(query, size, withoutFutureRaces: true, withoutFutureRacesForce: true);
              this.AnalysisSource(source, weight, item.Cell, item.Finder);
            }
            else
            {
              this.AnalysisFixedValue(weight, item.Cell, item.Finder);
            }

            this.IsLoaded.Value = true;
            this.IsLoading.Value = false;
          });
          tasks.Add(task);
        }
      }

      if (!isCacheOnly)
      {
        await Task.WhenAll(tasks);
      }

      this.UpdateComparation();
    }

    private void AnalysisSource(RaceHorseFinderQueryResult source, AnalysisTableWeight? weight, AnalysisTableCell cell, RaceFinder finder)
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
        cell.SampleSize = 0;

        if (weight != null && isAny)
        {
          var weightValue = weight.CalcWeight(items) * this.Data.BaseWeight;
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
        this.AnalysisFixedValue(weight, cell, finder);
      }
      else
      {
        var items = source.Items;

        this.SetValueOfRaceHorseAnalyzer(finder, this.Data.Output, source, cell);

        if (weight != null)
        {
          var weightValue = weight.CalcWeight(items) * this.Data.BaseWeight;
          cell.Weight = weightValue;
          cell.Point.Value = cell.PointCalcValue.Value * weightValue;
        }
        else
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

      if (weight == null)
      {
        cell.Weight = 1;
        cell.Point.Value = cell.PointCalcValue.Value;
      }
    }

    private void AnalysisFixedValue(AnalysisTableWeight? weight, AnalysisTableCell cell, RaceFinder finder)
    {
      if (weight != null && finder.RaceHorseAnalyzer != null)
      {
        var weightValue = weight.CalcWeight(new[] { finder.RaceHorseAnalyzer, }) * this.Data.BaseWeight;
        cell.Weight = weightValue;
        cell.PointCalcValue.Value = 1;
        cell.Point.Value = cell.PointCalcValue.Value * weightValue;
        cell.Value.Value = cell.Point.Value.ToString("N3");
      }
    }

    private void SetValueOfRaceHorseAnalyzer(RaceFinder finder, AnalysisTableRowOutputType value, RaceHorseFinderQueryResult source, AnalysisTableCell cell)
    {
      var analyzer = source.Analyzer;

      if (value == AnalysisTableRowOutputType.Time)
      {
        cell.Value.Value = analyzer.TimeDeviationValue.ToString("F1");
        cell.ComparationValue.Value = (float)analyzer.TimeDeviationValue;
        cell.HasComparationValue.Value = analyzer.AllGrade.AllCount > 0;
        cell.PointCalcValue.Value = analyzer.TimeDeviationValue / 100;
        cell.SampleSize = 0;
      }
      else if (value == AnalysisTableRowOutputType.A3HTime)
      {
        cell.Value.Value = analyzer.A3HTimeDeviationValue.ToString("F1");
        cell.ComparationValue.Value = (float)analyzer.A3HTimeDeviationValue;
        cell.HasComparationValue.Value = analyzer.AllGrade.AllCount > 0;
        cell.PointCalcValue.Value = analyzer.A3HTimeDeviationValue / 100;
        cell.SampleSize = 0;
      }
      else if (value == AnalysisTableRowOutputType.UA3HTime)
      {
        cell.Value.Value = analyzer.UntilA3HTimeDeviationValue.ToString("F1");
        cell.ComparationValue.Value = (float)analyzer.UntilA3HTimeDeviationValue;
        cell.HasComparationValue.Value = analyzer.AllGrade.AllCount > 0;
        cell.PointCalcValue.Value = analyzer.UntilA3HTimeDeviationValue / 100;
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
          .Select(s => s.Data.ResultTime.TotalSeconds / s.Race.Distance * finder.Race.Distance)
          .Select(s => TimeSpan.FromSeconds(s))
          .ToArray();
        if (timePerMeters.Any())
        {
          var shortestTime = timePerMeters.Min();
          cell.Value.Value = shortestTime.ToString("m\\:ss\\.f");
          cell.SubValue.Value = timePerMeters.Length.ToString();
          cell.ComparationValue.Value = (float)shortestTime.TotalSeconds * -1;
          cell.HasComparationValue.Value = true;
          cell.PointCalcValue.Value = shortestTime.TotalSeconds;
          cell.SampleSize = 1;
          cell.SampleFilter = filter;
        }
      }
      else if (value == AnalysisTableRowOutputType.RecoveryRace)
      {
        cell.Value.Value = analyzer.RecoveryRate.ToString("P1");
        cell.ComparationValue.Value = (float)analyzer.RecoveryRate;
        cell.HasComparationValue.Value = analyzer.AllGrade.AllCount > 0;
        cell.PointCalcValue.Value = analyzer.RecoveryRate;
        cell.SampleSize = 0;
      }
      else
      {
        this.SetValueOfGradeMap(value, analyzer.AllGrade, cell);
      }
    }

    private void SetValueOfGradeMap(AnalysisTableRowOutputType value, ResultOrderGradeMap grade, AnalysisTableCell cell)
    {
      if (value == AnalysisTableRowOutputType.PlaceBetsRate)
      {
        cell.Value.Value = grade.PlacingBetsRate.ToString("P1");
        cell.SubValue.Value = $"{grade.PlacingBetsCount} / {grade.AllCount}";
        cell.ComparationValue.Value = grade.PlacingBetsRate;
        cell.HasComparationValue.Value = grade.AllCount > 0;
        cell.PointCalcValue.Value = grade.PlacingBetsRate;
      }
      if (value == AnalysisTableRowOutputType.WinRate)
      {
        cell.Value.Value = grade.WinRate.ToString("P1");
        cell.SubValue.Value = $"{grade.FirstCount} / {grade.AllCount}";
        cell.ComparationValue.Value = grade.WinRate;
        cell.HasComparationValue.Value = grade.AllCount > 0;
        cell.PointCalcValue.Value = grade.WinRate;
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
