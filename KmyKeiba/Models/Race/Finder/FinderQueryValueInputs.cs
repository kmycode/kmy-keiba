﻿using KmyKeiba.Models.Analysis;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.Finder
{
  public class FinderQueryStringInput
  {
    public ReactiveProperty<string> Value { get; } = new();

    public ReactiveProperty<bool> IsStartsWith { get; } = new();

    public ReactiveProperty<bool> IsEndsWith { get; } = new();

    public ReactiveProperty<bool> IsContains { get; } = new(true);

    public ReactiveProperty<bool> IsEqual { get; } = new();

    public string GetRightQuery()
    {
      if (string.IsNullOrEmpty(this.Value.Value))
      {
        return string.Empty;
      }

      var sign = this.IsStartsWith.Value ? "@<" :
        this.IsEndsWith.Value ? "@>" :
        this.IsContains.Value ? "@=" :
        "=";

      return sign + this.Value.Value;
    }
  }

  public class FinderQueryNumberInput : IDisposable
  {
    private readonly CompositeDisposable _disposables = new();
    public event EventHandler? Updated;

    public ReactiveProperty<string> Value { get; } = new();

    public ReactiveProperty<string> MaxValue { get; } = new();

    public ReactiveProperty<bool> IsUnset { get; } = new();

    public ReactiveProperty<bool> IsRange { get; } = new();

    public ReactiveProperty<bool> IsGreaterThan { get; } = new();

    public ReactiveProperty<bool> IsGreaterThanOrEqual { get; } = new();

    public ReactiveProperty<bool> IsLessThan { get; } = new();

    public ReactiveProperty<bool> IsLessThanOrEqual { get; } = new();

    public ReactiveProperty<bool> IsEqual { get; } = new(true);

    public ReactiveProperty<bool> IsNotEqual { get; } = new();

    public ReactiveProperty<bool> IsCompareWithFixedValue { get; } = new(true);

    public ReactiveProperty<bool> IsCompareWithCurrentRace { get; } = new();

    public ReactiveProperty<bool> IsCompareWithTargetRace { get; } = new();

    public FinderQueryNumberInput()
    {
      this.IsUnset
        .CombineLatest(this.IsRange)
        .CombineLatest(this.IsGreaterThan)
        .CombineLatest(this.IsGreaterThanOrEqual)
        .CombineLatest(this.IsLessThan)
        .CombineLatest(this.IsLessThanOrEqual)
        .CombineLatest(this.IsEqual)
        .CombineLatest(this.IsNotEqual)
        .CombineLatest(this.Value)
        .CombineLatest(this.MaxValue)
        .CombineLatest(this.IsCompareWithCurrentRace)
        .CombineLatest(this.IsCompareWithFixedValue)
        .CombineLatest(this.IsCompareWithTargetRace)
        .Subscribe(_ =>
        {
          this.Updated?.Invoke(this, EventArgs.Empty);
        }).AddTo(this._disposables);

      this.Value.Skip(1).Where(_ => this.IsUnset.Value).Subscribe(_ => this.IsEqual.Value = true).AddTo(this._disposables);
    }

    public virtual string GetRightQuery()
    {
      if (this.IsUnset.Value)
      {
        return string.Empty;
      }

      if (!int.TryParse(this.Value.Value, out var min))
      {
        return string.Empty;
      }

      if (this.IsRange.Value)
      {
        if (!int.TryParse(this.MaxValue.Value, out var max))
        {
          return string.Empty;
        }
        if (min > max)
        {
          var tmp = min;
          min = max;
          max = tmp;
        }

        return $"={min}-{max}";
      }

      var sign = this.IsGreaterThan.Value ? ">" :
        this.IsGreaterThanOrEqual.Value ? ">=" :
        this.IsLessThan.Value ? "<" :
        this.IsLessThanOrEqual.Value ? "<=" :
        this.IsNotEqual.Value ? "<>" :
        "=";
      var prefix = this.IsCompareWithCurrentRace.Value ? "$$" :
        this.IsCompareWithTargetRace.Value ? "$" : string.Empty;

      return sign + prefix + this.Value.Value;
    }

    public IObservable<EventPattern<object>> ToObservable()
    {
      return Observable.FromEventPattern(ev => this.Updated += ev, ev => this.Updated -= ev);
    }

    public void Dispose()
    {
      this._disposables.Dispose();
    }
  }

  public class FinderQueryFloatNumberInput : FinderQueryNumberInput
  {
    public int Digit { get; }

    public FinderQueryFloatNumberInput(int digit)
    {
      this.Digit = digit;
    }

    private int GetDigitValue(decimal value)
    {
      if (this.Digit > 0)
      {
        for (var i = 0; i < this.Digit; i++)
        {
          value *= 10;
        }
      }
      else if (this.Digit < 0)
      {
        for (var i = 0; i < -this.Digit; i++)
        {
          value /= 10;
        }
      }
      return (int)value;
    }

    public override string GetRightQuery()
    {
      if (!decimal.TryParse(this.Value.Value, out var min))
      {
        return string.Empty;
      }
      min = this.GetDigitValue(min);

      if (this.IsRange.Value)
      {
        if (!decimal.TryParse(this.MaxValue.Value, out var max))
        {
          return string.Empty;
        }
        max = this.GetDigitValue(max);
        if (min > max)
        {
          var tmp = min;
          min = max;
          max = tmp;
        }

        return $"={min}-{max}";
      }

      var sign = this.IsGreaterThan.Value ? ">" :
        this.IsGreaterThanOrEqual.Value ? ">=" :
        this.IsLessThan.Value ? "<" :
        this.IsLessThanOrEqual.Value ? "<=" :
        this.IsNotEqual.Value ? "<>" :
        "=";

      return sign + min;
    }
  }

  public class FinderQueryBloodRelationInput : IDisposable
  {
    private readonly CompositeDisposable _disposables = new();
    public event EventHandler? Updated;

    public ReactiveProperty<bool> Father { get; } = new();
    public ReactiveProperty<bool> FatherFather { get; } = new();
    public ReactiveProperty<bool> FatherFatherFather { get; } = new();
    public ReactiveProperty<bool> FatherFatherMother { get; } = new();
    public ReactiveProperty<bool> FatherMother { get; } = new();
    public ReactiveProperty<bool> FatherMotherFather { get; } = new();
    public ReactiveProperty<bool> FatherMotherMother { get; } = new();
    public ReactiveProperty<bool> Mother { get; } = new();
    public ReactiveProperty<bool> MotherFather { get; } = new(true);
    public ReactiveProperty<bool> MotherFatherFather { get; } = new();
    public ReactiveProperty<bool> MotherFatherMother { get; } = new();
    public ReactiveProperty<bool> MotherMother { get; } = new();
    public ReactiveProperty<bool> MotherMotherFather { get; } = new();
    public ReactiveProperty<bool> MotherMotherMother { get; } = new();

    public ReactiveProperty<BloodType> SelectedBloodType { get; } = new();

    public FinderQueryBloodRelationInput()
    {
      this.Father
        .CombineLatest(this.FatherFather)
        .CombineLatest(this.FatherFatherFather)
        .CombineLatest(this.FatherFatherMother)
        .CombineLatest(this.FatherMother)
        .CombineLatest(this.FatherMotherFather)
        .CombineLatest(this.FatherMotherMother)
        .CombineLatest(this.Mother)
        .CombineLatest(this.MotherFather)
        .CombineLatest(this.MotherFatherFather)
        .CombineLatest(this.MotherFatherMother)
        .CombineLatest(this.MotherMother)
        .CombineLatest(this.MotherMotherFather)
        .CombineLatest(this.MotherMotherMother)
        .Subscribe(_ =>
        {
          this.SelectedBloodType.Value = this.GetBloodType();
          this.Updated?.Invoke(this, EventArgs.Empty);
        }).AddTo(this._disposables);
    }

    public IObservable<EventPattern<object>> ToObservable()
    {
      return Observable.FromEventPattern(ev => this.Updated += ev, ev => this.Updated -= ev);
    }

    public BloodType GetBloodType()
    {
      if (this.Father.Value) return BloodType.Father;
      if (this.FatherFather.Value) return BloodType.FatherFather;
      if (this.FatherFatherFather.Value) return BloodType.FatherFatherFather;
      if (this.FatherFatherMother.Value) return BloodType.FatherFatherMother;
      if (this.FatherMother.Value) return BloodType.FatherMother;
      if (this.FatherMotherFather.Value) return BloodType.FatherMotherFather;
      if (this.FatherMotherMother.Value) return BloodType.FatherMotherMother;
      if (this.Mother.Value) return BloodType.Mother;
      if (this.MotherFather.Value) return BloodType.MotherFather;
      if (this.MotherFatherFather.Value) return BloodType.MotherFatherFather;
      if (this.MotherFatherMother.Value) return BloodType.MotherFatherMother;
      if (this.MotherMother.Value) return BloodType.MotherMother;
      if (this.MotherMotherFather.Value) return BloodType.MotherMotherFather;
      if (this.MotherMotherMother.Value) return BloodType.MotherMotherMother;
      return BloodType.Unknown;
    }

    public void Dispose()
    {
      this._disposables.Dispose();
    }
  }
}