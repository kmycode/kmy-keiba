using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Analysis.Generic;
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
  public class ValueDelimiter : ICheckableItem, IDisposable
  {
    private readonly CompositeDisposable _disposables = new();

    public DelimiterData Data { get; }

    public MultipleCheckableCollection<ValueDelimiterRow> Rows { get; } = new();

    public ReactiveProperty<string> Name { get; } = new();

    public ReactiveProperty<bool> IsChecked { get; } = new();

    public ValueDelimiter(DelimiterData data)
    {
      this.Data = data;
      this.Name.Value = data.Name;

      this.Name.Skip(1).Subscribe(async value =>
      {
        // TODO: error
        using var db = new MyContext();
        db.Delimiters!.Attach(this.Data);
        this.Data.Name = this.Name.Value;
        await db.SaveChangesAsync();
      }).AddTo(this._disposables);
    }

    public int GetParameterOrder()
    {
      if (this.Rows.Any())
      {
        return this.Rows.Min(r => r.GetParameterOrder());
      }
      return -1;
    }

    public void Dispose()
    {
      this._disposables.Dispose();
      this.Rows.Dispose();
    }
  }

  public class ValueDelimiterRow : IDisposable, IMultipleCheckableItem
  {
    private readonly CompositeDisposable _disposables = new();

    public DelimiterRowData Data { get; }

    public FinderModel FinderModelForConfig { get; }

    public ReactiveProperty<string> DisplayText { get; } = new();

    public ReactiveProperty<bool> IsChecked { get; } = new();

    string? IMultipleCheckableItem.GroupName => null;

    public ReactiveCollection<FinderQueryParameterItem> FinderModelParameters { get; } = new();

    public ValueDelimiterRow(DelimiterRowData data)
    {
      this.Data = data;

      this.FinderModelForConfig = new FinderModel(new RaceData(), RaceHorseAnalyzer.Empty, Array.Empty<RaceHorseAnalyzer>());
      this.FinderModelForConfig.Input.Deserialize(data.FinderConfig);

      this.FinderModelForConfig.Input.Query.Skip(1).Subscribe(async _ =>
      {
        // TODO try catch
        using var db = new MyContext();
        db.DelimiterRows!.Attach(this.Data);
        this.Data.FinderConfig = this.FinderModelForConfig.Input.Serialize(false, isDiffOnly: true);
        await db.SaveChangesAsync();

        this.UpdateParameters();
      }).AddTo(this._disposables);

      this.UpdateParameters();
    }

    private void UpdateParameters()
    {
      AnalysisTableUtil.UpdateParameters(this.FinderModelForConfig, this.FinderModelParameters);

      this.DisplayText.Value = string.Join('/', this.FinderModelParameters.Select(p => p.DisplayValue.Value));
    }

    public int GetParameterOrder()
    {
      var parameters = this.FinderModelForConfig.Input.ToParameters();
      if (parameters.Any())
      {
        return parameters.Min(p => p.Order);
      }
      return -1;
    }

    public void Dispose()
    {
      this.FinderModelForConfig.Dispose();
      this._disposables.Dispose();
    }
  }
}
