using KmyKeiba.Models.Analysis;
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
using System.Windows.Input;

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

    public ReactiveProperty<bool> IsCustomized { get; } = new();

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

    public ReactiveProperty<bool> IsUseCurrentRaceValue { get; } = new();

    public ReactiveProperty<bool> IsUseCurrentRaceHorseValue { get; } = new();

    public ReactiveProperty<string> ComparationWithBeforeRaceComment { get; } = new();

    public bool IsCompareWithHorse { get; }

    public bool CanCompareCurrentRaceValue { get; set; } = true;

    public bool CanCompareAsBeforeRace { get; set; } = true;

    public bool CanCompareDefaultValue { get; set; } = true;

    public FinderQueryNumberInput(bool isCompareWithHorse)
    {
      this.IsCompareWithHorse = isCompareWithHorse;

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
        .CombineLatest(this.IsUseCurrentRaceValue)
        .CombineLatest(this.IsUseCurrentRaceHorseValue)
        .Subscribe(_ =>
        {
          this.Updated?.Invoke(this, EventArgs.Empty);
        }).AddTo(this._disposables);

      this.Value.Skip(1).Where(_ => this.IsUnset.Value).Subscribe(_ => this.IsEqual.Value = true).AddTo(this._disposables);
    }

    public FinderQueryNumberInput() : this(false)
    {
    }

    public virtual string GetRightQuery()
    {
      this.IsCustomized.Value = false;
      this.ComparationWithBeforeRaceComment.Value = string.Empty;

      if (this.IsUnset.Value)
      {
        return string.Empty;
      }

      var valuePrefix = string.Empty;
      if (this.IsUseCurrentRaceValue.Value || this.IsUseCurrentRaceHorseValue.Value)
      {
        // horses#など以外の末尾の＃は解析時に無視される
        this.IsCustomized.Value = true;
        valuePrefix = ":";
      }

      // 前走検索「対象レースと比較」「現在のレースと比較」のいずれかを選択している場合
      if (this.IsCompareWithTargetRace.Value || this.IsCompareWithCurrentRace.Value || this.IsUseCurrentRaceValue.Value || this.IsUseCurrentRaceHorseValue.Value)
      {
        var left = "前走一覧画面で比較対象として設定したn走前";
        var right = "前走一覧画面で設定したn走前";
        if (this.IsUseCurrentRaceHorseValue.Value || this.IsUseCurrentRaceValue.Value)
        {
          left = "検索結果に出現する";
          right = "現在の";
        }
        if (this.IsCompareWithCurrentRace.Value)
        {
          left = "検索結果に出現する";
          right = "検索結果に出現するレースのn走前";
        }
        if (this.IsCompareWithTargetRace.Value)
        {
          left = "検索結果に出現するレースの(比較対象)n走前";
          right = "検索結果に出現するレースのn走前";
        }

        var value = string.IsNullOrEmpty(this.Value.Value) ? "0" : this.Value.Value;
        var maxValue = string.IsNullOrEmpty(this.MaxValue.Value) ? "0" : this.MaxValue.Value;

        if (this.IsGreaterThan.Value || this.IsGreaterThanOrEqual.Value)
        {
          this.ComparationWithBeforeRaceComment.Value = $"{left}レースの値は、{right}レースの値に {value} を足したもの以上である";
        }
        else if (this.IsLessThan.Value || this.IsLessThanOrEqual.Value)
        {
          this.ComparationWithBeforeRaceComment.Value = $"{left}レースの値は、{right}レースの値に {value} を足したもの以下である";
        }
        else if (this.IsEqual.Value)
        {
          this.ComparationWithBeforeRaceComment.Value = $"{left}レースの値は、{right}レースの値に {value} を足したものと等しい";
        }
        else if (this.IsNotEqual.Value)
        {
          this.ComparationWithBeforeRaceComment.Value = $"{left}レースの値は、{right}レースの値に {value} を足したものと等しくない";
        }
        else if (this.IsRange.Value)
        {
          this.ComparationWithBeforeRaceComment.Value = $"{left}レースの値は、{right}レースの値に {value} を足したもの以上で、{maxValue} を足したもの以下である";
        }
      }

      if (!decimal.TryParse(this.Value.Value, out var dmin))
      {
        if (this.CanCompareDefaultValue)
        {
          return this.IsCustomized.Value ? "#" : string.Empty;
        }
        else
        {
          return this.IsCustomized.Value ? "=:0" : string.Empty;
        }
      }
      var min = this.ConvertValue(dmin);

      if (this.IsRange.Value)
      {
        if (!decimal.TryParse(this.MaxValue.Value, out var dmax))
        {
          return string.Empty;
        }
        var max = this.ConvertValue(dmax);
        if (min > max)
        {
          var tmp = min;
          min = max;
          max = tmp;
        }

        this.IsCustomized.Value = true;
        return $"={valuePrefix}{min}-{max}";
      }

      var sign = this.IsGreaterThan.Value ? ">" :
        this.IsGreaterThanOrEqual.Value ? ">=" :
        this.IsLessThan.Value ? "<" :
        this.IsLessThanOrEqual.Value ? "<=" :
        this.IsNotEqual.Value ? "<>" :
        "=";
      var prefix = this.IsCompareWithCurrentRace.Value ? "$$" :
        this.IsCompareWithTargetRace.Value ? "$" : valuePrefix;

      this.IsCustomized.Value = true;
      return sign + prefix + min;
    }

    protected virtual int ConvertValue(decimal value)
    {
      try
      {
        // ユーザーが入れた数字が大きすぎると例外が投げられる
        return (int)value;
      }
      catch { }

      return default;
    }

    public IObservable<EventPattern<object>> ToObservable()
    {
      return Observable.FromEventPattern(ev => this.Updated += ev, ev => this.Updated -= ev);
    }

    public void Dispose()
    {
      this._disposables.Dispose();
    }

    public void Reset()
    {
      this.IsEqual.Value = true;
      this.IsCompareWithFixedValue.Value = true;
      this.Value.Value = this.MaxValue.Value = string.Empty;
    }

    public ICommand ResetCommand =>
      this._resetCommand ??= new ReactiveCommand().WithSubscribe(() =>
      {
        this.Reset();
      }).AddTo(this._disposables);
    private ICommand? _resetCommand;
  }

  public class FinderQueryFloatNumberInput : FinderQueryNumberInput
  {
    public int Digit { get; }

    public FinderQueryFloatNumberInput(int digit) : this(digit, false)
    {
    }

    public FinderQueryFloatNumberInput(int digit, bool isCompareWithHorse) : base(isCompareWithHorse)
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

    protected override int ConvertValue(decimal value)
    {
      return this.GetDigitValue(value);
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
