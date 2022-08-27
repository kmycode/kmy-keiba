using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Analysis.Generic;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race.ExNumber;
using KmyKeiba.Models.Race.Finder;
using KmyKeiba.Models.Race.Memo;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.AnalysisTable
{
  public class AnalysisTableConfigModel
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public static AnalysisTableConfigModel Instance => _instance ??= new AnalysisTableConfigModel();
    private static AnalysisTableConfigModel? _instance;

    public CheckableCollection<AnalysisTableSurface> Tables { get; } = new();

    public ReactiveProperty<AnalysisTableSurface?> ActiveTable => this.Tables.ActiveItem;

    public CheckableCollection<AnalysisTableWeight> Weights { get; } = new();

    public ReactiveProperty<AnalysisTableWeight?> ActiveWeight => this.Weights.ActiveItem;

    public CheckableCollection<ValueDelimiter> Delimiters { get; } = new();

    public ReactiveProperty<ValueDelimiter?> ActiveDelimiter => this.Delimiters.ActiveItem;

    public CheckableCollection<ValueDelimiter> SelectedDelimiters { get; } = new();

    public ReactiveProperty<ValueDelimiter?> ActiveSelectedDelimiter => this.SelectedDelimiters.ActiveItem;

    public ReactiveProperty<ValueDelimiter?> SelectedDelimiterForAdd { get; } = new();

    public CheckableCollection<ExternalNumberConfigItem> ExternalNumbers { get; } = new();

    public ReactiveCollection<ExpansionMemoConfig> MemoConfigs { get; } = new();

    public ReactiveProperty<bool> IsBulkMode { get; } = new();

    private AnalysisTableConfigModel()
    {
      this.InitializeTables();
    }

    private void InitializeTables()
    {
      this.Tables.Clear();
      this.Weights.Clear();
      this.Delimiters.Clear();
      this.ExternalNumbers.Clear();
      this.ActiveTable.Value = null;
      this.ActiveWeight.Value = null;
      this.ActiveDelimiter.Value = null;

      foreach (var table in AnalysisTableUtil.TableConfigs.OrderBy(t => t.Order))
      {
        this.Tables.Add(new AnalysisTableSurface(new RaceData(), table, Array.Empty<RaceHorseAnalyzer>()));
      }
      if (this.Tables.Any())
      {
        this.Tables.First().IsChecked.Value = true;
      }

      foreach (var weight in AnalysisTableUtil.Weights)
      {
        this.Weights.Add(weight);
      }
      if (this.Weights.Any())
      {
        this.Weights.First().IsChecked.Value = true;
      }

      foreach (var delimiter in AnalysisTableUtil.Delimiters)
      {
        this.Delimiters.Add(delimiter);
      }
      if (this.Delimiters.Any())
      {
        this.Delimiters.First().IsChecked.Value = true;
      }

      foreach (var en in ExternalNumberUtil.Configs)
      {
        this.ExternalNumbers.Add(new ExternalNumberConfigItem(en));
      }
      foreach (var row in this.Tables.SelectMany(t => t.Rows).Where(r => r.Data.Output == AnalysisTableRowOutputType.ExternalNumber))
      {
        row.SelectedExternalNumber.Value = this.ExternalNumbers.FirstOrDefault(e => e.Data.Id == row.Data.ExternalNumberId);
      }

      this.UpdateMemoConfigs();
    }

    public async Task<AnalysisTableSurface?> AddTableAsync()
    {
      try
      {
        using var db = new MyContext();

        var data = new AnalysisTableData();
        await db.AnalysisTables!.AddAsync(data);
        await db.SaveChangesAsync();

        data.Order = data.Id;
        await db.SaveChangesAsync();

        AnalysisTableUtil.TableConfigs.Add(data);
        var table = new AnalysisTableSurface(new RaceData(), data, Array.Empty<RaceHorseAnalyzer>());

        ThreadUtil.InvokeOnUiThread(() =>
        {
          this.Tables.Add(table);
          table.IsChecked.Value = true;
        });

        return table;
      }
      catch (Exception ex)
      {
        logger.Error("テーブル作成でエラー", ex);
      }

      return null;
    }

    public async Task RemoveTableAsync(AnalysisTableSurface table)
    {
      if (!this.Tables.Contains(table))
      {
        return;
      }

      ThreadUtil.InvokeOnUiThread(() =>
      {
        this.Tables.Remove(table);
      });

      try
      {
        using var db = new MyContext();

        db.AnalysisTableRows!.RemoveRange(table.Rows.Select(r => r.Data));

        db.AnalysisTables!.Remove(table.Data);
        await db.SaveChangesAsync();
        AnalysisTableUtil.TableConfigs.Remove(table.Data);

        table.Dispose();
      }
      catch (Exception ex)
      {
        logger.Error("テーブル削除でエラー", ex);
      }
    }

    public async Task UpTableAsync(AnalysisTableSurface table)
    {
      try
      {
        var index = this.Tables.IndexOf(table);
        if (index <= 0)
        {
          return;
        }

        var prev = this.Tables[index - 1];
        var tmp = prev.Data.Order;

        using var db = new MyContext();
        db.AnalysisTables!.Attach(prev.Data);
        db.AnalysisTables!.Attach(table.Data);
        prev.Data.Order = table.Data.Order;
        table.Data.Order = tmp;
        await db.SaveChangesAsync();

        this.Tables.Remove(prev);
        this.Tables.Insert(index, prev);
      }
      catch (Exception ex)
      {
        logger.Error("テーブルの並べ替えでエラー", ex);
      }
    }

    public async Task DownTableAsync(AnalysisTableSurface table)
    {
      var index = this.Tables.IndexOf(table);
      if (index < 0 || index > this.Tables.Count - 2)
      {
        return;
      }

      try
      {
        var next = this.Tables[index + 1];
        var tmp = next.Data.Order;

        using var db = new MyContext();
        db.AnalysisTables!.Attach(next.Data);
        db.AnalysisTables!.Attach(table.Data);
        next.Data.Order = table.Data.Order;
        table.Data.Order = tmp;
        await db.SaveChangesAsync();

        this.Tables.Remove(next);
        this.Tables.Insert(index, next);
      }
      catch (Exception ex)
      {
        logger.Error("テーブルの並べ替えでエラー", ex);
      }
    }

    public async Task AddTableRowAsync()
    {
      if (this.ActiveTable.Value != null)
      {
        await this.AddTableRowAsync(this.ActiveTable.Value);
      }
    }

    public async Task<AnalysisTableRow?> AddTableRowAsync(AnalysisTableSurface table)
    {
      try
      {
        using var db = new MyContext();

        var row = new AnalysisTableRowData
        {
          TableId = table.Data.Id,
          Output = AnalysisTableRowOutputType.PlaceBetsRate,
          BaseWeight = 1,
        };
        await db.AnalysisTableRows!.AddAsync(row);
        await db.SaveChangesAsync();

        row.Order = row.Id;
        await db.SaveChangesAsync();

        AnalysisTableUtil.TableRowConfigs.Add(row);
        var rowobj = new AnalysisTableRow(row, table, Array.Empty<RaceHorseAnalyzer>());
        table.Rows.Add(rowobj);
        return rowobj;
      }
      catch (Exception ex)
      {
        logger.Error("テーブル行作成でエラー", ex);
      }

      return null;
    }

    public async Task RemoveTableRowAsync(AnalysisTableRow row)
    {
      var table = this.Tables.FirstOrDefault(t => t.Data.Id == row.Data.TableId);
      if (table == null || !table.Rows.Contains(row))
      {
        return;
      }

      ThreadUtil.InvokeOnUiThread(() =>
      {
        table.Rows.Remove(row);
      });

      try
      {
        using var db = new MyContext();

        db.AnalysisTableRows!.Remove(row.Data);
        await db.SaveChangesAsync();
        AnalysisTableUtil.TableRowConfigs.Remove(row.Data);

        foreach (var r in this.Tables.SelectMany(t => t.Rows).Where(r => r.SelectedParent.Value?.Data.ParentRowId == row.Data.Id))
        {
          r.SelectedParent.Value = null;
        }
      }
      catch (Exception ex)
      {
        logger.Error("テーブル行の削除でエラー", ex);
      }
      finally
      {
        row.Dispose();
      }
    }

    public async Task UpTableRowAsync(AnalysisTableRow row)
    {
      var table = this.Tables.FirstOrDefault(d => d.Rows.Contains(row));
      if (table == null)
      {
        return;
      }

      var index = table.Rows.IndexOf(row);
      if (index <= 0)
      {
        return;
      }

      var prev = table.Rows[index - 1];
      var tmp = prev.Data.Order;

      try
      {
        using var db = new MyContext();
        db.AnalysisTableRows!.Attach(prev.Data);
        db.AnalysisTableRows!.Attach(row.Data);
        prev.Data.Order = row.Data.Order;
        row.Data.Order = tmp;
        await db.SaveChangesAsync();

        table.Rows.Remove(prev);
        table.Rows.Insert(index, prev);
      }
      catch (Exception ex)
      {
        logger.Error("テーブル行の順序変更でエラー発生", ex);
      }
    }

    public async Task DownTableRowAsync(AnalysisTableRow row)
    {
      var table = this.Tables.FirstOrDefault(d => d.Rows.Contains(row));
      if (table == null)
      {
        return;
      }

      var index = table.Rows.IndexOf(row);
      if (index < 0 || index > table.Rows.Count - 2)
      {
        return;
      }

      var next = table.Rows[index + 1];
      var tmp = next.Data.Order;

      try
      {
        using var db = new MyContext();
        db.AnalysisTableRows!.Attach(next.Data);
        db.AnalysisTableRows!.Attach(row.Data);
        next.Data.Order = row.Data.Order;
        row.Data.Order = tmp;
        await db.SaveChangesAsync();

        table.Rows.Remove(next);
        table.Rows.Insert(index, next);
      }
      catch (Exception ex)
      {
        logger.Error("テーブル行の順序変更でエラー発生", ex);
      }
    }

    public void UnselectTableRowWeight(AnalysisTableRow row)
    {
      row.Weight.Value = null;
    }

    public void UnselectTableRowWeight2(AnalysisTableRow row)
    {
      row.Weight2.Value = null;
    }

    public void UnselectTableRowWeight3(AnalysisTableRow row)
    {
      row.Weight3.Value = null;
    }

    public void UnselectTableRowParent(AnalysisTableRow row)
    {
      row.SelectedParent.Value = null;
    }

    public async Task AddWeightAsync()
    {
      try
      {
        using var db = new MyContext();

        var data = new AnalysisTableWeightData();
        await db.AnalysisTableWeights!.AddAsync(data);
        await db.SaveChangesAsync();

        var weight = new AnalysisTableWeight(data);
        AnalysisTableUtil.Weights.Add(weight);
        ThreadUtil.InvokeOnUiThread(() =>
        {
          this.Weights.Add(weight);
          weight.IsChecked.Value = true;
        });
      }
      catch (Exception ex)
      {
        logger.Error("重みの追加でエラー", ex);
      }
    }

    public async Task RemoveWeightAsync(AnalysisTableWeight weight)
    {
      if (!this.Weights.Contains(weight))
      {
        return;
      }

      ThreadUtil.InvokeOnUiThread(() =>
      {
        this.Weights.Remove(weight);
      });

      try
      {
        using var db = new MyContext();

        db.AnalysisTableWeightRows!.RemoveRange(weight.Rows.Select(r => r.Data));

        db.AnalysisTableWeights!.Remove(weight.Data);
        await db.SaveChangesAsync();
        AnalysisTableUtil.Weights.Remove(weight);

        var rows = this.Tables.SelectMany(t => t.Rows).Where(r => r.Data.WeightId == weight.Data.Id ||
          r.Data.Weight2Id == weight.Data.Id || r.Data.Weight3Id == weight.Data.Id);
        foreach (var row in rows)
        {
          if (row.Weight.Value?.Data.Id == weight.Data.Id)
          {
            row.Weight.Value = null;
          }
          if (row.Weight2.Value?.Data.Id == weight.Data.Id)
          {
            row.Weight2.Value = null;
          }
          if (row.Weight3.Value?.Data.Id == weight.Data.Id)
          {
            row.Weight3.Value = null;
          }
        }

        weight.Dispose();
      }
      catch (Exception ex)
      {
        logger.Error("重みの削除でエラー", ex);
      }
    }

    public async Task AddWeightRowAsync()
    {
      if (this.ActiveWeight.Value == null)
      {
        return;
      }

      try
      {
        using var db = new MyContext();

        var row = new AnalysisTableWeightRowData
        {
          WeightId = this.ActiveWeight.Value.Data.Id,
          Behavior = WeightBehavior.Multiply,
          Weight = 1.0,
        };
        await db.AnalysisTableWeightRows!.AddAsync(row);
        await db.SaveChangesAsync();

        row.Order = (int)row.Id;
        await db.SaveChangesAsync();

        this.ActiveWeight.Value.Rows.Add(new AnalysisTableWeightRow(row));
      }
      catch (Exception ex)
      {
        logger.Error("重み追加でエラー発生", ex);
      }
    }

    public async Task RemoveTableWeightRowAsync(AnalysisTableWeightRow row)
    {
      var weight = this.Weights.FirstOrDefault(t => t.Data.Id == row.Data.WeightId);
      if (weight == null || !weight.Rows.Contains(row))
      {
        return;
      }

      ThreadUtil.InvokeOnUiThread(() =>
      {
        weight.Rows.Remove(row);
      });

      try
      {
        using var db = new MyContext();

        db.AnalysisTableWeightRows!.Remove(row.Data);
        await db.SaveChangesAsync();

        row.Dispose();
      }
      catch (Exception ex)
      {
        logger.Error("重み削除でエラー発生", ex);
      }
    }

    public async Task UpTableWeightRowAsync(AnalysisTableWeightRow row)
    {
      var weight = this.Weights.FirstOrDefault(d => d.Rows.Contains(row));
      if (weight == null)
      {
        return;
      }

      var index = weight.Rows.IndexOf(row);
      if (index <= 0)
      {
        return;
      }

      var prev = weight.Rows[index - 1];
      var tmp = prev.Data.Order;

      try
      {
        using var db = new MyContext();
        db.AnalysisTableWeightRows!.Attach(prev.Data);
        db.AnalysisTableWeightRows!.Attach(row.Data);
        prev.Data.Order = row.Data.Order;
        row.Data.Order = tmp;
        await db.SaveChangesAsync();

        weight.Rows.Remove(prev);
        weight.Rows.Insert(index, prev);
      }
      catch (Exception ex)
      {
        logger.Error("重み順序変更でエラー発生", ex);
      }
    }

    public async Task DownTableWeightRowAsync(AnalysisTableWeightRow row)
    {
      var weight = this.Weights.FirstOrDefault(d => d.Rows.Contains(row));
      if (weight == null)
      {
        return;
      }

      var index = weight.Rows.IndexOf(row);
      if (index < 0 || index > weight.Rows.Count - 2)
      {
        return;
      }

      var next = weight.Rows[index + 1];
      var tmp = next.Data.Order;

      try
      {
        using var db = new MyContext();
        db.AnalysisTableWeightRows!.Attach(next.Data);
        db.AnalysisTableWeightRows!.Attach(row.Data);
        next.Data.Order = row.Data.Order;
        row.Data.Order = tmp;
        await db.SaveChangesAsync();

        weight.Rows.Remove(next);
        weight.Rows.Insert(index, next);
      }
      catch (Exception ex)
      {
        logger.Error("重み順序変更でエラー発生", ex);
      }
    }

    public async Task AddDelimiterAsync()
    {
      try
      {
        using var db = new MyContext();

        var data = new DelimiterData();
        await db.Delimiters!.AddAsync(data);
        await db.SaveChangesAsync();

        var delimiter = new ValueDelimiter(data);
        AnalysisTableUtil.Delimiters.Add(delimiter);
        ThreadUtil.InvokeOnUiThread(() =>
        {
          this.Delimiters.Add(delimiter);
          delimiter.IsChecked.Value = true;
        });
      }
      catch (Exception ex)
      {
        logger.Error("重み区切り設定でエラー発生", ex);
      }
    }

    public async Task RemoveDelimiterAsync(ValueDelimiter delimiter)
    {
      if (!this.Delimiters.Contains(delimiter))
      {
        return;
      }

      ThreadUtil.InvokeOnUiThread(() =>
      {
        this.Delimiters.Remove(delimiter);
      });

      try
      {
        using var db = new MyContext();

        db.DelimiterRows!.RemoveRange(delimiter.Rows.Select(r => r.Data));

        db.Delimiters!.Remove(delimiter.Data);
        await db.SaveChangesAsync();
        AnalysisTableUtil.Delimiters.Remove(delimiter);

        delimiter.Dispose();
      }
      catch (Exception ex)
      {
        logger.Error("重み区切り設定でエラー発生", ex);
      }
    }

    public async Task AddDelimiterRowAsync()
    {
      if (this.ActiveDelimiter.Value == null)
      {
        return;
      }

      try
      {
        using var db = new MyContext();

        var row = new DelimiterRowData
        {
          DelimiterId = this.ActiveDelimiter.Value.Data.Id,
        };
        await db.DelimiterRows!.AddAsync(row);
        await db.SaveChangesAsync();

        row.Order = (int)row.Id;
        await db.SaveChangesAsync();

        this.ActiveDelimiter.Value.Rows.Add(new ValueDelimiterRow(row));
      }
      catch (Exception ex)
      {
        logger.Error("重み区切り設定でエラー発生", ex);
      }
    }

    public async Task RemoveDelimiterRowAsync(ValueDelimiterRow row)
    {
      var Delimiter = this.Delimiters.FirstOrDefault(t => t.Data.Id == row.Data.DelimiterId);
      if (Delimiter == null || !Delimiter.Rows.Contains(row))
      {
        return;
      }

      ThreadUtil.InvokeOnUiThread(() =>
      {
        Delimiter.Rows.Remove(row);
      });

      try
      {
        using var db = new MyContext();

        db.DelimiterRows!.Remove(row.Data);
        await db.SaveChangesAsync();

        row.Dispose();
      }
      catch (Exception ex)
      {
        logger.Error("重み区切り設定でエラー発生", ex);
      }
    }

    public async Task UpDelimiterRowAsync(ValueDelimiterRow row)
    {
      var delimiter = this.Delimiters.FirstOrDefault(d => d.Rows.Contains(row));
      if (delimiter == null)
      {
        return;
      }

      var index = delimiter.Rows.IndexOf(row);
      if (index <= 0)
      {
        return;
      }

      var prev = delimiter.Rows[index - 1];
      var tmp = prev.Data.Order;

      try
      {
        using var db = new MyContext();
        db.DelimiterRows!.Attach(prev.Data);
        db.DelimiterRows!.Attach(row.Data);
        prev.Data.Order = row.Data.Order;
        row.Data.Order = tmp;
        await db.SaveChangesAsync();

        delimiter.Rows.Remove(prev);
        delimiter.Rows.Insert(index, prev);
      }
      catch (Exception ex)
      {
        logger.Error("重み区切り設定でエラー発生", ex);
      }
    }

    public async Task DownDelimiterRowAsync(ValueDelimiterRow row)
    {
      var delimiter = this.Delimiters.FirstOrDefault(d => d.Rows.Contains(row));
      if (delimiter == null)
      {
        return;
      }

      var index = delimiter.Rows.IndexOf(row);
      if (index < 0 || index > delimiter.Rows.Count - 2)
      {
        return;
      }

      var next = delimiter.Rows[index + 1];
      var tmp = next.Data.Order;

      try
      {
        using var db = new MyContext();
        db.DelimiterRows!.Attach(next.Data);
        db.DelimiterRows!.Attach(row.Data);
        next.Data.Order = row.Data.Order;
        row.Data.Order = tmp;
        await db.SaveChangesAsync();

        delimiter.Rows.Remove(next);
        delimiter.Rows.Insert(index, next);
      }
      catch (Exception ex)
      {
        logger.Error("重み区切り設定でエラー発生", ex);
      }
    }

    public void AddSelectedDelimiter()
    {
      if (this.SelectedDelimiterForAdd.Value == null || this.SelectedDelimiters.Contains(this.SelectedDelimiterForAdd.Value))
      {
        return;
      }

      var order = this.SelectedDelimiterForAdd.Value.GetParameterOrder();
      var index = 0;
      foreach (var delimiter in this.SelectedDelimiters.Select(d => d.GetParameterOrder()))
      {
        if (delimiter > order)
        {
          break;
        }
        index++;
      }

      this.SelectedDelimiters.Insert(index, this.SelectedDelimiterForAdd.Value);
      this.SelectedDelimiterForAdd.Value.IsChecked.Value = true;
      this.ActiveSelectedDelimiter.Value = this.SelectedDelimiterForAdd.Value;
      this.SelectedDelimiterForAdd.Value = null;
    }

    public void RemoveSelectedDelimiter()
    {
      if (this.ActiveSelectedDelimiter.Value == null)
      {
        return;
      }

      this.SelectedDelimiters.Remove(this.ActiveSelectedDelimiter.Value);
      this.ActiveSelectedDelimiter.Value = null;
    }

    public async Task AddWeightRowsBulkAsync()
    {
      if (this.ActiveWeight.Value == null)
      {
        return;
      }

      var rows = new List<AnalysisTableWeightRowData>();
      var delimiters = this.SelectedDelimiters
        .Where(d => d.Rows.Any(r => r.IsChecked.Value))
        .OrderBy(d => d.GetParameterOrder())
        .ToArray();
      var keys = new ValueDelimiterRow[delimiters.Length];

      var maxOrder = 1;
      if (this.ActiveWeight.Value.Rows.Any())
      {
        maxOrder = this.ActiveWeight.Value.Rows.Max(r => r.Data.Order) + 1;
      }

      void AddData(int keyIndex)
      {
        foreach (var delimiterRow in delimiters![keyIndex].Rows.Where(r => r.IsChecked.Value))
        {
          keys![keyIndex] = delimiterRow;
          if (keyIndex < keys.Length - 1)
          {
            AddData(keyIndex + 1);
          }
          else
          {
            using var finder = new FinderModel(new RaceData(), RaceHorseAnalyzer.Empty, Array.Empty<RaceHorseAnalyzer>());
            foreach (var key in keys)
            {
              finder.Input.Deserialize(key.Data.FinderConfig);
            }
            var row = new AnalysisTableWeightRowData
            {
              FinderConfig = finder.Input.Serialize(false),
              Behavior = WeightBehavior.Multiply,
              Weight = 1,
              WeightId = this.ActiveWeight.Value!.Data.Id,
              Order = maxOrder++,
            };
            rows!.Add(row);
          }
        }
      }

      AddData(0);

      // TODO error
      using var db = new MyContext();
      await db.AnalysisTableWeightRows!.AddRangeAsync(rows);
      await db.SaveChangesAsync();

      foreach (var row in rows)
      {
        this.ActiveWeight.Value.Rows.Add(new AnalysisTableWeightRow(row));
      }

      this.IsBulkMode.Value = false;
    }

    public async Task ClearWeightRowsAsync()
    {
      if (this.ActiveWeight.Value == null || !this.ActiveWeight.Value.Rows.Any())
      {
        return;
      }

      // TODO error
      using var db = new MyContext();
      db.AnalysisTableWeightRows!.RemoveRange(this.ActiveWeight.Value.Rows.Select(r => r.Data));
      await db.SaveChangesAsync();

      this.ActiveWeight.Value.Rows.Clear();
    }

    public void OnMemoConfigChanged()
    {
      ThreadUtil.InvokeOnUiThread(() =>
      {
        this.UpdateMemoConfigs();
      });
    }

    private void UpdateMemoConfigs()
    {
      foreach (var row in this.Tables.SelectMany(t => t.Rows))
      {
        row.IsFreezeExpansionMemoConfig = true;
      }

      var olds = this.MemoConfigs.ToArray();
      this.MemoConfigs.Clear();
      foreach (var memoconfig in MemoUtil.Configs.Where(c => c.Style == MemoStyle.Point || c.Style == MemoStyle.MemoAndPoint))
      {
        var exists = olds.FirstOrDefault(o => o.Id == memoconfig.Id);
        if (exists == null)
        {
          this.MemoConfigs.Add(memoconfig);
        }
        else
        {
          this.MemoConfigs.Add(exists);
        }
      }

      foreach (var row in this.Tables.SelectMany(t => t.Rows).Where(r => r.SelectedMemoConfig.Value != null))
      {
        if (!this.MemoConfigs.Any(c => c.Id == row.SelectedMemoConfig.Value!.Id))
        {
          row.IsFreezeExpansionMemoConfig = false;
          row.SelectedMemoConfig.Value = null;
        }
      }
      foreach (var row in this.Tables.SelectMany(t => t.Rows))
      {
        row.ReloadMemoConfigProperty();
        row.IsFreezeExpansionMemoConfig = false;
      }
    }
  }
}
