using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
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
  public class AnalysisTableWeight : IDisposable
  {
    private readonly CompositeDisposable _disposables = new();
    private readonly List<IAnalysisWeight> _weights = new();

    public ReactiveProperty<string> Name { get; } = new();

    public AnalysisTableWeightData Data { get; }

    public RaceGradeAnalysisWeight Grade { get; }

    public RaceDistanceAnalysisWeight Distance { get; }

    public AnalysisTableWeight(AnalysisTableWeightData data)
    {
      this.Data = data;
      this.Name.Value = data.Name;
      this.Deserialize(data.Config);

      this._weights.Add(this.Grade = new RaceGradeAnalysisWeight());
      this._weights.Add(this.Distance = new RaceDistanceAnalysisWeight());

      foreach (var weight in this._weights)
      {
        Observable.FromEventPattern(ev => weight.ConfigUpdated += ev, ev => weight.ConfigUpdated -= ev)
          .Subscribe(async _ =>
          {
            using var db = new MyContext();
            db.AnalysisTableWeights!.Attach(this.Data);
            this.Data.Config = this.Serialize();
            await db.SaveChangesAsync();
          }).AddTo(this._disposables);
      }
      this.Name.Skip(1).Subscribe(async value =>
      {
        using var db = new MyContext();
        db.AnalysisTableWeights!.Attach(this.Data);
        this.Data.Name = this.Name.Value;
        await db.SaveChangesAsync();
      }).AddTo(this._disposables);
    }

    public double CalcWeight(IReadOnlyList<RaceHorseAnalyzer> source)
    {
      if (source.Any())
      {
        using var db = new MyContext();
        var weights = source.Sum(s => this._weights.Sum(w => w.GetWeight(db, s.Race) * w.GetWeight(db, s.Data)));
        return weights * source.Count;
      }
      return 1;
    }

    public bool IsThroughFilter(RaceData race)
    {
      using var db = new MyContext();
      return this._weights.All(w => w.IsThroughFilter(db, race));
    }

    public string Serialize()
    {
      StringBuilder text = new();

      foreach (var property in this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
      {
        if (property.CanRead)
        {
          var weight = property.GetValue(this) as IAnalysisWeight;
          if (weight != null)
          {
            text.Append(property.Name);
            text.Append('=');
            text.Append(weight.Serialize());
            text.AppendLine();
          }
        }
      }

      return text.ToString();
    }

    public void Deserialize(string data)
    {
      var properties = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanRead)
        .Select(p => new { Info = p, Weight = p.GetValue(this) as IAnalysisWeight })
        .Where(p => p.Weight != null);

      foreach (var line in data.Split(Environment.NewLine).Where(l => l.Contains('=')))
      {
        var split = line.IndexOf('=');
        var name = line[..split];
        var d = line[(split + 1)..];

        var property = properties.FirstOrDefault(p => p.Info.Name == name);
        if (property != null)
        {
          property.Weight!.Deserialize(d);
        }
      }
    }

    public void Dispose()
    {
      this._disposables.Dispose();
    }
  }
}
