using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
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

  public class FinderQueryNumberInput
  {
    public int InvalidValue { get; set; }

    public ReactiveProperty<string> Value { get; } = new("0");

    public ReactiveProperty<string> MaxValue { get; } = new("0");

    public ReactiveProperty<bool> IsRange { get; } = new();

    public ReactiveProperty<bool> IsGreaterThan { get; } = new();

    public ReactiveProperty<bool> IsGreaterThanOrEqual { get; } = new();

    public ReactiveProperty<bool> IsLessThan { get; } = new();

    public ReactiveProperty<bool> IsLessThanOrEqual { get; } = new();

    public ReactiveProperty<bool> IsEqual { get; } = new(true);

    public ReactiveProperty<bool> IsNotEqual { get; } = new();

    public string GetRightQuery()
    {
      if (!int.TryParse(this.Value.Value, out var min) || min == this.InvalidValue)
      {
        return string.Empty;
      }

      if (this.IsRange.Value)
      {
        if (!int.TryParse(this.MaxValue.Value, out var max) || max == this.InvalidValue)
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

      return sign + this.Value.Value;
    }
  }

  public class FinderQueryFloatNumberInput
  {
    public decimal InvalidValue { get; set; }

    public int Digit { get; }

    public ReactiveProperty<string> Value { get; } = new("0");

    public ReactiveProperty<string> MaxValue { get; } = new("0");

    public ReactiveProperty<bool> IsRange { get; } = new();

    public ReactiveProperty<bool> IsGreaterThan { get; } = new();

    public ReactiveProperty<bool> IsGreaterThanOrEqual { get; } = new();

    public ReactiveProperty<bool> IsLessThan { get; } = new();

    public ReactiveProperty<bool> IsLessThanOrEqual { get; } = new();

    public ReactiveProperty<bool> IsEqual { get; } = new(true);

    public ReactiveProperty<bool> IsNotEqual { get; } = new();

    public FinderQueryFloatNumberInput(int digit)
    {
      this.Digit = digit;
    }

    private int GetDigitValue(decimal value)
    {
      for (var i = 0; i < this.Digit; i++)
      {
        value *= 10;
      }
      return (int)value;
    }

    public string GetRightQuery()
    {
      if (!decimal.TryParse(this.Value.Value, out var min) || min == this.InvalidValue)
      {
        return string.Empty;
      }
      min = this.GetDigitValue(min);

      if (this.IsRange.Value)
      {
        if (!decimal.TryParse(this.MaxValue.Value, out var max) || max == this.InvalidValue)
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

      return sign + this.Value.Value;
    }
  }
}
