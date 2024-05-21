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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.AnalysisTable
{
  public class AnalysisTableWeight : IDisposable, ICheckableItem
  {
    private readonly CompositeDisposable _disposables = new();

    public AnalysisTableWeightData Data { get; }

    public ReactiveProperty<string> Name { get; } = new();

    public ReactiveCollection<AnalysisTableWeightRow> Rows { get; } = new();

    public ReactiveProperty<bool> IsChecked { get; } = new();

    public AnalysisTableWeight(AnalysisTableWeightData data)
    {
      this.Data = data;
      this.Name.Value = data.Name;

      this.Name.Skip(1).Subscribe(async value =>
      {
        // TODO: error
        using var db = new MyContext();
        db.AnalysisTableWeights!.Attach(this.Data);
        this.Data.Name = this.Name.Value;
        await db.SaveChangesAsync();
      }).AddTo(this._disposables);
    }

    public AnalysisTableWeightRow? GetMatchRow(RaceHorseAnalyzer horse)
    {
      foreach (var row in this.Rows)
      {
        if (row.IsMatch(horse))
        {
          return row;
        }
      }

      return null;
    }

    public double GetWeight(RaceHorseAnalyzer horse)
    {
      var row = this.GetMatchRow(horse);
      if (row != null)
      {
        return row.Data.Weight;
      }
      return 1;
    }

    public double CalcWeight(IEnumerable<RaceHorseAnalyzer> horses)
    {
      var weight = 0.0;
      var count = 0;

      foreach (var horse in horses)
      {
        weight += this.GetWeight(horse);
        count++;
      }

      if (count > 0)
      {
        return weight / count;
      }
      return 1;
    }

    public void Dispose()
    {
      this._disposables.Dispose();
    }
  }

  public class AnalysisTableWeightRow : IDisposable
  {
    private readonly CompositeDisposable _disposables = new();

    public FinderModel FinderModelForConfig { get; }

    public AnalysisTableWeightRowData Data { get; }

    public ReactiveProperty<string> Weight { get; } = new();

    public ReactiveCollection<FinderQueryParameterItem> FinderModelParameters { get; } = new();

    public AnalysisTableWeightRow(AnalysisTableWeightRowData data)
    {
      this.Data = data;
      this.Weight.Value = data.Weight.ToString();

      this.Weight.Skip(1).Subscribe(async _ =>
      {
        if (!double.TryParse(this.Weight.Value, out var value))
        {
          return;
        }
        using var db = new MyContext();
        db.AnalysisTableWeightRows!.Attach(this.Data);
        this.Data.Weight = value;
        await db.SaveChangesAsync();
      }).AddTo(this._disposables);

      this.FinderModelForConfig = new FinderModel(new RaceData(), RaceHorseAnalyzer.Empty, Array.Empty<RaceHorseAnalyzer>());
      this.FinderModelForConfig.Input.Deserialize(data.FinderConfig);

      this.FinderModelForConfig.Input.Query.Skip(1).Subscribe(async _ =>
      {
        // TODO try catch
        using var db = new MyContext();
        db.AnalysisTableWeightRows!.Attach(this.Data);
        this.Data.FinderConfig = this.FinderModelForConfig.Input.Serialize(false);
        await db.SaveChangesAsync();

        this.UpdateParameters();
      }).AddTo(this._disposables);

      this.UpdateParameters();
    }

    private void UpdateParameters()
    {
      AnalysisTableUtil.UpdateParameters(this.FinderModelForConfig, this.FinderModelParameters);
    }

    public bool IsMatch(RaceHorseAnalyzer horse)
    {
      var query = this.FinderModelForConfig.Input.Query.Value;
      var reader = new ScriptKeysReader(query);
      var queries = reader.GetQueries(horse.Race, horse.Data, horse);

      var analyzers = (IEnumerable<RaceHorseAnalyzer>)new[] { horse, };
      var races = (IEnumerable<RaceData>)new[] { horse.Race, };
      var horses = (IEnumerable<RaceHorseData>)new[] { horse.Data, };
      using var db = new MyContext();
      foreach (var q in queries.Queries)
      {
        if (q is IRaceHorseAnalyzerScriptQuery aq)
        {
          analyzers = aq.Apply(db, analyzers);
        }
        else
        {
          races = q.Apply(db, races);
          horses = q.Apply(db, horses);
        }
      }

      analyzers = analyzers.Where(a => races.Any(r => r.Id == a.Race.Id) && horses.Any(h => h.Id == a.Data.Id));
      return analyzers.Any();
    }

    public void Dispose()
    {
      this.FinderModelForConfig.Dispose();
      this._disposables.Dispose();
    }
  }

  internal static class AnalysisWeightExtensions
  {
    public static double CalcWeight(this IEnumerable<AnalysisTableWeight> weights, IReadOnlyList<RaceHorseAnalyzer> horses)
    {
      if (weights.Any())
      {
        var d = 1.0;
        foreach (var w in weights.Select(w => w.CalcWeight(horses)))
        {
          d *= w;
        }
        return d;
      }
      return 1;
    }

    public static double GetWeight(this IEnumerable<AnalysisTableWeight> weights, RaceHorseAnalyzer horse)
    {
      if (weights.Any())
      {
        var d = 1.0;
        foreach (var w in weights.Select(w => w.GetWeight(horse)))
        {
          d *= w;
        }
        return d;
      }
      return 1;
    }
  }
}
