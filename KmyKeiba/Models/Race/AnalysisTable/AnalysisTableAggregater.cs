using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.AnalysisTable
{
  public class AnalysisTableAggregater
  {
    private readonly AnalysisTableModel _model;
    private readonly IReadOnlyList<RaceHorseAnalyzer> _horses;

    public ReactiveCollection<HorseItem> Horses { get; } = new();

    public ReactiveCollection<TableItem> Tables { get; } = new();

    public ReactiveProperty<bool> IsLoading { get; } = new();

    public AnalysisTableAggregater(AnalysisTableModel model, IReadOnlyList<RaceHorseAnalyzer> horses)
    {
      this._model = model;
      this._horses = horses;
    }

    public void BeginLoad()
    {
      Task.Run(async () => await this.LoadAsync());
    }

    public async Task LoadAsync()
    {
      this.IsLoading.Value = true;

      bool isCleared = false;

      ThreadUtil.InvokeOnUiThread(() =>
      {
        this.Tables.Clear();
        foreach (var table in this._model.Tables)
        {
          this.Tables.Add(new TableItem(table, this._horses));
        }

        this.Horses.Clear();
        foreach (var horse in this._horses)
        {
          this.Horses.Add(new HorseItem(horse));
        }

        isCleared = true;
      });

      while (!isCleared)
      {
        await Task.Delay(50);
      }

      foreach (var horse in this.Horses)
      {
        horse.Comparation.Value = default;
        horse.TotalPoint.Value = default;
        horse.MarkSuggestion.Value = default;
      }

      foreach (var table in this.Tables)
      {
        await this._model.AnalysisTableAsync(table.Table);

        if (!table.Cells.Any())
        {
          continue;
        }

        foreach (var cell in table.Cells)
        {
          var targetCells = table.Table.Rows
            .SelectMany(r => r.Cells)
            .Where(c => c.Horse.Data.Key == cell.Horse.Data.Key)
            .ToArray();
          if (targetCells.Any())
          {
            cell.Point.Value = targetCells.Sum(c => c.Point.Value);

            var targetHorse = this.Horses.FirstOrDefault(h => h.Horse.Data.Key == cell.Horse.Data.Key);
            if (targetHorse != null)
            {
              targetHorse.TotalPoint.Value += cell.Point.Value;
            }

            cell.IsLoadCompleted.Value = true;
          }
        }

        var max = table.Cells.OrderByDescending(c => c.Point.Value).ElementAtOrDefault(2)?.Point.Value ?? 0;
        var min = table.Cells.OrderBy(c => c.Point.Value).ElementAtOrDefault(2)?.Point.Value ?? double.MaxValue;

        foreach (var cell in table.Cells)
        {
          cell.Comparation.Value = AnalysisUtil.CompareValue(cell.Point.Value, max, min);
        }
      }

      var hmax = this.Horses.OrderByDescending(c => c.TotalPoint.Value).ElementAtOrDefault(2)?.TotalPoint.Value ?? 0;
      var hmin = this.Horses.OrderBy(c => c.TotalPoint.Value).ElementAtOrDefault(2)?.TotalPoint.Value ?? double.MaxValue;
      var i = 1;
      foreach (var horse in this.Horses.OrderByDescending(h => h.TotalPoint.Value))
      {
        horse.Comparation.Value = AnalysisUtil.CompareValue(horse.TotalPoint.Value, hmax, hmin);

        if (i == 1)
        {
          horse.MarkSuggestion.Value = RaceHorseMark.DoubleCircle;
        }
        else if (i == 2)
        {
          horse.MarkSuggestion.Value = RaceHorseMark.Circle;
        }
        else if (i == 3)
        {
          horse.MarkSuggestion.Value = RaceHorseMark.FilledTriangle;
        }
        else if (i == 4)
        {
          if (this.Horses.Count > 7)
          {
            horse.MarkSuggestion.Value = RaceHorseMark.Triangle;
          }
        }
        else if (i == 5)
        {
          if (this.Horses.Count > 10)
          {
            horse.MarkSuggestion.Value = RaceHorseMark.Triangle;
          }
        }
        else if (this.Horses.Count >= 10 && i > this.Horses.Count - (this.Horses.Count > 15 ? 3 : 2))
        {
          horse.MarkSuggestion.Value = RaceHorseMark.Deleted;
        }

        i++;
      }

      this.IsLoading.Value = false;
    }

    public async Task ApplyHorseMarksAsync()
    {
      using var db = new MyContext();
      foreach (var horse in this.Horses)
      {
        horse.Horse.ChangeHorseMark(db, horse.MarkSuggestion.Value);
      }
      await db.SaveChangesAsync();
    }

    public class HorseItem
    {
      public RaceHorseAnalyzer Horse { get; }

      public ReactiveProperty<double> TotalPoint { get; } = new();

      public ReactiveProperty<ValueComparation> Comparation { get; } = new();

      public ReactiveProperty<RaceHorseMark> MarkSuggestion { get; } = new();

      public HorseItem(RaceHorseAnalyzer horse)
      {
        this.Horse = horse;
      }
    }

    public class TableItem
    {
      public AnalysisTableSurface Table { get; }

      public ReactiveCollection<TableCellItem> Cells { get; } = new();

      public TableItem(AnalysisTableSurface table, IReadOnlyList<RaceHorseAnalyzer> horses)
      {
        this.Table = table;

        foreach (var horse in horses)
        {
          this.Cells.Add(new TableCellItem(horse));
        }
      }
    }

    public class TableCellItem
    {
      public RaceHorseAnalyzer Horse { get; }

      public ReactiveProperty<double> Point { get; } = new();

      public ReactiveProperty<ValueComparation> Comparation { get; } = new();

      public ReactiveProperty<bool> IsLoadCompleted { get; } = new();

      public TableCellItem(RaceHorseAnalyzer horse)
      {
        this.Horse = horse;
      }
    }
  }
}
