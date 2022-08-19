using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Data;
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
  public interface IAnalysisWeight : IDisposable
  {
    string Serialize();

    void Deserialize(string data);

    bool IsThroughFilter(MyContext db, RaceData race);

    double GetWeight(MyContext db, RaceData race);

    double GetWeight(MyContext db, RaceHorseData horse);

    event EventHandler? ConfigUpdated;
  }

  public abstract class AnalysisWeight : IAnalysisWeight
  {
    protected CompositeDisposable Disposables { get; } = new();

    public abstract string Serialize();

    public abstract void Deserialize(string data);

    public virtual bool IsThroughFilter(MyContext db, RaceData data)
    {
      return true;
    }

    public virtual double GetWeight(MyContext db, RaceData race)
    {
      return 1;
    }

    public virtual double GetWeight(MyContext db, RaceHorseData horse)
    {
      return 1;
    }

    public void Dispose()
    {
      this.Disposables.Dispose();
    }

    protected void OnConfigUpdated()
    {
      this.ConfigUpdated?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? ConfigUpdated;
  }

  public class EnumAnalysisWeight<T> : AnalysisWeight
  {
    public ReactiveCollection<Item> Items { get; } = new();

    public EnumAnalysisWeight()
    {
      this.Items.ToCollectionChanged().Subscribe(_ => this.OnConfigUpdated()).AddTo(this.Disposables);
    }

    public override string Serialize()
    {
      return string.Join('/', this.Items.Select(i => $"{this.ConvertValue(i.Value.Value)},{i.Weight},{(i.IsThroughFilter.Value ? 1 : 0)}"));
    }

    public override void Deserialize(string data)
    {
      this.Items.Clear();

      var lines = data.Split('/');
      foreach (var line in lines.Where(l => l.Contains(',')))
      {
        var d = line.Split(',');
        double.TryParse(d[1], out var weight);

        var item = new Item
        {
          Value = { Value = this.ConvertValue(d[1]), },
          Weight = { Value = weight, },
          IsThroughFilter = { Value = d.ElementAtOrDefault(2) == "1", },
        };
        this.Items.Add(item);

        item.Value.CombineLatest(item.Weight).CombineLatest(item.IsThroughFilter).Subscribe(_ => this.OnConfigUpdated()).AddTo(this.Disposables);
      }
    }

    public override bool IsThroughFilter(MyContext db, RaceData race)
    {
      var item = this.Items.FirstOrDefault(i => this.PredicateData(race, i.Value.Value));
      if (item != null)
      {
        return item.IsThroughFilter.Value;
      }
      return true;
    }

    public override double GetWeight(MyContext db, RaceData race)
    {
      var item = this.Items.FirstOrDefault(i => this.PredicateData(race, i.Value.Value));
      if (item != null)
      {
        return item.Weight.Value;
      }
      return 1;
    }

    public override double GetWeight(MyContext db, RaceHorseData horse)
    {
      var item = this.Items.FirstOrDefault(i => this.PredicateData(horse, i.Value.Value));
      if (item != null)
      {
        return item.Weight.Value;
      }
      return 1;
    }

    protected virtual bool PredicateData(RaceData race, T value)
    {
      return true;
    }

    protected virtual bool PredicateData(RaceHorseData horse, T value)
    {
      return true;
    }

    protected virtual string ConvertValue(T value)
    {
      return string.Empty;
    }

    protected virtual T ConvertValue(string value)
    {
      return default(T)!;
    }

    public class Item
    {
      public ReactiveProperty<T> Value { get; } = new();

      public ReactiveProperty<double> Weight { get; } = new();

      public ReactiveProperty<bool> IsThroughFilter { get; } = new();
    }
  }

  public class NumericAnalysisWeight : AnalysisWeight
  {
    public ReactiveCollection<Item> Items { get; } = new();

    protected bool IsCompareWithRace { get; }

    protected bool IsCompareWithRaceHorse { get; }

    protected NumericAnalysisWeight(bool isCompareWithRace, bool isCompareWithRaceHorse)
    {
      this.IsCompareWithRace = isCompareWithRace;
      this.IsCompareWithRaceHorse = isCompareWithRaceHorse;

      this.Items.ToCollectionChanged().Subscribe(_ => this.OnConfigUpdated()).AddTo(this.Disposables);
    }

    public override string Serialize()
    {
      return string.Join('/', this.Items.Select(i => $"{i.MinValue},{i.Weight},{(i.IsThroughFilter.Value ? 1 : 0)}"));
    }

    public override void Deserialize(string data)
    {
      this.Items.Clear();

      var lines = data.Split('/');
      foreach (var line in lines.Where(l => l.Contains(',')))
      {
        var d = line.Split(',');
        int.TryParse(d[0], out var min);
        double.TryParse(d[1], out var weight);

        var item = new Item
        {
          MinValue = { Value = min, },
          Weight = { Value = weight, },
          IsThroughFilter = { Value = d.ElementAtOrDefault(2) == "1", },
        };
        this.Items.Add(item);

        item.MinValue.CombineLatest(item.Weight).CombineLatest(item.IsThroughFilter).Subscribe(_ => this.OnConfigUpdated()).AddTo(this.Disposables);
      }
    }

    public override bool IsThroughFilter(MyContext db, RaceData race)
    {
      if (!this.IsCompareWithRace)
      {
        return false;
      }

      var item = this.Items
        .OrderByDescending(i => i.MinValue)
        .FirstOrDefault(i => this.GetData(db, race) >= i.MinValue.Value);
      if (item != null)
      {
        return item.IsThroughFilter.Value;
      }
      return true;
    }

    public override double GetWeight(MyContext db, RaceData race)
    {
      if (!this.IsCompareWithRace)
      {
        return 1;
      }

      var item = this.Items
        .OrderByDescending(i => i.MinValue)
        .FirstOrDefault(i => this.GetData(db, race) >= i.MinValue.Value);
      if (item != null)
      {
        return item.Weight.Value;
      }
      return 1;
    }

    public override double GetWeight(MyContext db, RaceHorseData horse)
    {
      if (!this.IsCompareWithRaceHorse)
      {
        return 1;
      }

      var item = this.Items
        .OrderByDescending(i => i.MinValue)
        .FirstOrDefault(i => this.GetData(db, horse) >= i.MinValue.Value);
      if (item != null)
      {
        return item.Weight.Value;
      }
      return 1;
    }

    protected virtual int GetData(MyContext db, RaceData race)
    {
      return int.MaxValue;
    }

    protected virtual int GetData(MyContext db, RaceHorseData horse)
    {
      return int.MaxValue;
    }

    public class Item
    {
      public ReactiveProperty<int> MinValue { get; } = new();

      public ReactiveProperty<double> Weight { get; } = new();

      public ReactiveProperty<bool> IsThroughFilter { get; } = new();
    }
  }

  public class RaceGradeAnalysisWeight : EnumAnalysisWeight<RaceGrade>
  {
    protected override bool PredicateData(RaceData race, RaceGrade value)
    {
      return race.Grade == value;
    }

    protected override string ConvertValue(RaceGrade value)
    {
      return ((short)value).ToString();
    }

    protected override RaceGrade ConvertValue(string value)
    {
      short.TryParse(value, out var v);
      return (RaceGrade)v;
    }
  }

  public class RaceDistanceAnalysisWeight : NumericAnalysisWeight
  {
    public RaceDistanceAnalysisWeight() : base(true, false)
    {
    }

    protected override int GetData(MyContext db, RaceData race)
    {
      return race.Distance;
    }
  }
}
