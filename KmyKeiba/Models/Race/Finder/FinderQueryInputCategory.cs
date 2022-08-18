using KmyKeiba.Data.Db;
using KmyKeiba.Data.Wrappers;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Analysis.Generic;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race.ExNumber;
using KmyKeiba.Models.Race.Memo;
using Microsoft.EntityFrameworkCore;
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
using System.Windows.Input;

namespace KmyKeiba.Models.Race.Finder
{
  public interface IFinderQueryInputCategory : IDisposable
  {
    ReactiveProperty<string> Query { get; }

    ReadOnlyReactiveProperty<bool> IsCustomized { get; }

    string Serialize();

    void Deserialize(string data);
  }

  public abstract class FinderQueryInputCategory : IFinderQueryInputCategory
  {
    protected CompositeDisposable Disposables { get; } = new();

    public ReactiveProperty<string> Query { get; } = new();

    public ReadOnlyReactiveProperty<bool> IsCustomized { get; }

    protected FinderQueryInputCategory()
    {
      this.IsCustomized = this.Query
        .Select(q => !string.IsNullOrEmpty(q))
        .ToReadOnlyReactiveProperty()
        .AddTo(this.Disposables);
    }

    protected void UpdateQuery()
    {
      var query = this.GetQuery();
      if (this.Query.Value != query)
      {
        this.Query.Value = query;
      }
    }

    protected virtual string GetQuery()
    {
      return string.Empty;
    }

    private string EscapeCharacters(string text)
    {
      return text.Replace("=", string.Empty)
        .Replace("@", string.Empty)
        .Replace(":", string.Empty)
        .Replace(">", string.Empty)
        .Replace("<", string.Empty)
        .Replace(";", string.Empty)
        .Replace("/", string.Empty)
        .Replace(",", string.Empty);
    }

    protected void AddTextCheckForEscape(ReactiveProperty<string> property)
    {
      property.Subscribe(value =>
      {
        var text = this.EscapeCharacters(value ?? string.Empty);
        if (text != value)
        {
          property.Value = text;
        }
      }).AddTo(this.Disposables);
    }

    public void Dispose()
    {
      this.Disposables.Dispose();
    }

    protected virtual bool IsIgnorePropertyToSerializing(string propertyName)
    {
      if (propertyName == nameof(Query) || propertyName == "Key" || propertyName == nameof(IsCustomized))
      {
        return true;
      }
      return false;
    }

    protected virtual void PropertyToString(PropertyInfo property, StringBuilder text, object obj)
    {
      var type = property.PropertyType;
      if (type == typeof(ReactiveProperty<bool>))
      {
        text.Append(property.Name);
        text.Append('=');
        text.Append(((ReactiveProperty<bool>)property.GetValue(obj)!).Value.ToString().ToLower());
        text.AppendLine();
      }
      else if (type == typeof(ReactiveProperty<int>))
      {
        text.Append(property.Name);
        text.Append('=');
        text.Append(((ReactiveProperty<int>)property.GetValue(obj)!).Value);
        text.AppendLine();
      }
      else if (type == typeof(ReactiveProperty<string>))
      {
        text.Append(property.Name);
        text.Append('=');
        text.Append(this.EscapeCharacters(((ReactiveProperty<string>)property.GetValue(obj)!).Value ?? string.Empty));
        text.AppendLine();
      }
      else if (type == typeof(bool))
      {
        text.Append(property.Name);
        text.Append('=');
        text.Append(((bool)property.GetValue(obj)!).ToString().ToLower());
        text.AppendLine();
      }
      else if (type == typeof(int))
      {
        text.Append(property.Name);
        text.Append('=');
        text.Append(((int)property.GetValue(obj)!));
        text.AppendLine();
      }
      else if (type == typeof(string))
      {
        text.Append(property.Name);
        text.Append('=');
        text.Append((property.GetValue(obj)!));
        text.AppendLine();
      }
      else if (type == typeof(BloodType))
      {
        text.Append(property.Name);
        text.Append('=');
        text.Append(((int)(BloodType)property.GetValue(obj)!));
        text.AppendLine();
      }
      else if (type == typeof(FinderQueryNumberInput) || type == typeof(FinderQueryFloatNumberInput) || type == typeof(FinderQueryStringInput) || type == typeof(FinderQueryBloodRelationInput))
      {
        text.Append("@@");
        text.Append(property.Name);
        text.AppendLine();
        this.SerializeObject(text, property.GetValue(obj)!);
        text.Append("@@");
        text.AppendLine();
      }
    }

    private void SerializeObject(StringBuilder text, object obj)
    {
      foreach (var property in obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
      {
        if (!this.IsIgnorePropertyToSerializing(property.Name))
        {
          if (obj is not FinderQueryNumberInput || property.Name != nameof(FinderQueryNumberInput.ComparationWithBeforeRaceComment))
          {
            this.PropertyToString(property, text, obj);
          }
        }
      }
    }

    public string Serialize()
    {
      var text = new StringBuilder();

      this.SerializeObject(text, this);

      return text.ToString();
    }

    public void Deserialize(string value)
    {
      var lines = value.Split(Environment.NewLine);

      object? subObject = null;
      var selfProperties = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

      PropertyInfo? GetProperty(string propertyName)
      {
        if (this.IsIgnorePropertyToSerializing(propertyName))
        {
          return null;
        }
        var property = selfProperties!.FirstOrDefault(p => p.Name == propertyName);
        return property;
      }

      this.ResetForce();

      for (var i = 0; i < lines.Length; i++)
      {
        var line = lines[i];

        if (line.StartsWith("@@"))
        {
          var property = GetProperty(line[2..]);
          if (property == null)
          {
            continue;
          }

          subObject = property.GetValue(this);
          if (subObject != null)
          {
            i += this.DeserializeObject(subObject, lines.Skip(i + 1));
          }
        }
        else if (line.StartsWith("[CUSTOM]"))
        {
          var propertyNameIndex = line.IndexOf('/');
          if (propertyNameIndex > 0)
          {
            var propertyName = line.Substring(8, propertyNameIndex - 8);
            var p = this.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p == null)
            {
              continue;
            }

            this.StringToProperty(p, line[(propertyNameIndex + 1)..], this);

            continue;
          }
        }
        else if (line.Contains('='))
        {
          var d = line.Split('=');
          var property = GetProperty(d[0]);
          if (property == null)
          {
            continue;
          }

          this.StringToProperty(property, d[1], this);
        }
      }

      this.UpdateQuery();
    }

    private int DeserializeObject(object obj, IEnumerable<string> lines)
    {
      var i = 0;

      foreach (var line in lines)
      {
        i++;

        if (line.StartsWith('@'))
        {
          return i;
        }
        if (!line.Contains('='))
        {
          continue;
        }

        var d = line.Split('=');
        var property = obj.GetType().GetProperty(d[0], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property == null)
        {
          continue;
        }

        this.StringToProperty(property, d[1], obj);
      }

      return i;
    }

    protected virtual void StringToProperty(PropertyInfo property, string data, object obj)
    {
      var type = property.PropertyType;

      if (type == typeof(ReactiveProperty<bool>))
      {
        var value = data == "true";
        var rp = property.GetValue(obj);
        rp!.GetType().GetProperty("Value")!.SetValue(rp, value);
      }
      else if (type == typeof(ReactiveProperty<int>))
      {
        int.TryParse(data, out var value);
        var rp = property.GetValue(obj);
        rp!.GetType().GetProperty("Value")!.SetValue(rp, value);
      }
      else if (type == typeof(ReactiveProperty<string>))
      {
        var value = data;
        var rp = property.GetValue(obj);
        rp!.GetType().GetProperty("Value")!.SetValue(rp, value);
      }
      else if (property.GetSetMethod() != null)
      {
        if (type == typeof(bool))
        {
          var value = data == "true";
          property.SetValue(obj, value);
        }
        else if (type == typeof(int))
        {
          int.TryParse(data, out var value);
          property.SetValue(obj, value);
        }
        else if (type == typeof(string))
        {
          var value = data;
          property.SetValue(obj, value);
        }
        else if (type == typeof(BloodType))
        {
          int.TryParse(data, out var value);
          property.SetValue(obj, (BloodType)value);
        }
      }
    }

    protected virtual void ResetForce()
    {
    }
  }

  public interface IListBoxInputCategory : IFinderQueryInputCategory
  {
    IEnumerable<object> Items { get; }

    ReactiveProperty<bool> IsSetListValue { get; }

    ReactiveProperty<bool> IsSetCurrentRaceValue { get; }

    ReactiveProperty<bool> IsSetCurrentRaceHorseValue { get; }

    ReactiveProperty<bool> IsSetNumericComparation { get; }

    FinderQueryNumberInput NumberInput { get; }

    bool CanInputNumber { get; }

    bool IsCompareWithHorse { get; }
  }

  public class ListBoxInputCategoryBase<T> : FinderQueryInputCategory, IListBoxInputCategory
  {
    public FinderQueryInputListItemCollection<T> Items { get; } = new FinderQueryInputListItemCollection<T>();

    IEnumerable<object> IListBoxInputCategory.Items => ((ListBoxInputCategoryBase<T>)this).Items;

    public string Key { get; }

    public ReactiveProperty<bool> IsSetListValue { get; } = new(true);

    public ReactiveProperty<bool> IsSetCurrentRaceValue { get; } = new();

    public ReactiveProperty<bool> IsSetCurrentRaceHorseValue { get; } = new();

    public ReactiveProperty<bool> IsSetNumericComparation { get; } = new();

    public FinderQueryNumberInput NumberInput { get; } = new();

    public bool CanInputNumber { get; }

    public bool IsCompareWithHorse { get; }

    public bool CanCompareCurrentRaceValue { get; protected set; } = true;

    protected ListBoxInputCategoryBase(string key) : this(key, true, false)
    {
    }

    protected ListBoxInputCategoryBase(string key, bool canInputNumber) : this(key, canInputNumber, false)
    {
    }

    protected ListBoxInputCategoryBase(string key, bool canInputNumber, bool isCompareWithHorse)
    {
      this.Key = key;
      this.Items.ChangedItemObservable.Subscribe(x => this.UpdateQuery()).AddTo(this.Disposables);
      this.IsSetListValue.Subscribe(x => this.UpdateQuery()).AddTo(this.Disposables);
      this.IsSetCurrentRaceValue.Subscribe(x => this.UpdateQuery()).AddTo(this.Disposables);
      this.IsSetCurrentRaceHorseValue.Subscribe(x => this.UpdateQuery()).AddTo(this.Disposables);
      this.IsSetNumericComparation.Subscribe(x => this.UpdateQuery()).AddTo(this.Disposables);
      this.NumberInput.ToObservable().Subscribe(x => this.UpdateQuery()).AddTo(this.Disposables);
      this.NumberInput.AddTo(this.Disposables);
      this.CanInputNumber = canInputNumber;
      this.IsCompareWithHorse = isCompareWithHorse;
    }

    protected void SetItems(IEnumerable<FinderQueryInputListItem<T>> items)
    {
      foreach (var item in items)
      {
        this.Items.Add(item);
      }
    }

    protected virtual string ToQueryValue(T value)
    {
      return value!.ToString()!;
    }

    protected override string GetQuery()
    {
      if (this.IsSetCurrentRaceValue.Value || this.IsSetCurrentRaceHorseValue.Value)
      {
        return this.Key;
      }
      if (this.IsSetNumericComparation.Value)
      {
        var right = this.NumberInput.GetRightQuery();
        if (string.IsNullOrEmpty(right))
        {
          return string.Empty;
        }
        return $"{this.Key}{right}";
      }

      if (this.Items.IsEmpty() || this.Items.IsAll())
      {
        return string.Empty;
      }

      var (mini, maxi) = this.Items.GetContinuity();
      if (mini >= 0 && mini != maxi)
      {
        return $"{this.Key}={this.ToQueryValue(this.Items[mini].Value)}-{this.ToQueryValue(this.Items[maxi].Value)}";
      }

      return $"{this.Key}={string.Join(',', this.Items.GetCheckedValues().Select(v => this.ToQueryValue(v)))}";
    }

    protected override void PropertyToString(PropertyInfo property, StringBuilder text, object obj)
    {
      base.PropertyToString(property, text, obj);

      var type = property.PropertyType;
      if (obj == this && type == typeof(FinderQueryInputListItemCollection<T>))
      {
        text.Append(property.Name);
        text.Append("=");
        text.Append(string.Join(',', this.Items.GetCheckedValues().Select(v => this.ToQueryValue(v))));
        text.AppendLine();
      }
    }

    protected override void StringToProperty(PropertyInfo property, string data, object obj)
    {
      base.StringToProperty(property, data, obj);

      var type = property.PropertyType;
      if (obj == this && type == typeof(FinderQueryInputListItemCollection<T>))
      {
        var values = data.Split(',');
        foreach (var item in this.Items)
        {
          item.IsChecked.Value = false;
        }
        foreach (var item in this.Items
          .Join(values, i => this.ToQueryValue(i.Value), v => v, (i, v) => i))
        {
          item.IsChecked.Value = true;
        }
      }
    }

    protected override bool IsIgnorePropertyToSerializing(string propertyName)
    {
      var r = base.IsIgnorePropertyToSerializing(propertyName);
      if (!this.CanInputNumber)
      {
        r = r || propertyName == nameof(NumberInput);
        r = r || propertyName == nameof(IsSetNumericComparation);
      }
      return r;
    }

    public ICommand ResetCommand =>
      this._resetCommand ??= new ReactiveCommand().WithSubscribe(() =>
      {
        foreach (var item in this.Items.Where(i => i.IsChecked.Value))
        {
          item.IsChecked.Value = false;
        }
        this.IsSetListValue.Value = true;
      }).AddTo(this.Disposables);
    private ICommand? _resetCommand;
  }

  public class StringInputCategoryBase : FinderQueryInputCategory
  {
    protected string Key { get; }

    public FinderQueryStringInput Input { get; } = new();

    protected StringInputCategoryBase(string key)
    {
      this.Key = key;

      this.Input.Value.Subscribe(_ => base.UpdateQuery()).AddTo(this.Disposables);
      this.Input.IsEqual.Subscribe(_ => base.UpdateQuery()).AddTo(this.Disposables);
      this.Input.IsStartsWith.Subscribe(_ => base.UpdateQuery()).AddTo(this.Disposables);
      this.Input.IsEndsWith.Subscribe(_ => base.UpdateQuery()).AddTo(this.Disposables);
      this.Input.IsContains.Subscribe(_ => base.UpdateQuery()).AddTo(this.Disposables);
    }

    protected override string GetQuery()
    {
      var right = this.Input.GetRightQuery();
      if (string.IsNullOrEmpty(right))
      {
        return string.Empty;
      }
      return $"{this.Key}{right}";
    }
  }

  public class StringInputCategoryWithTestBase<T> : StringInputCategoryBase
  {
    public ReactiveCollection<T> TestResult { get; } = new();

    public ReactiveProperty<bool> IsTestError { get; } = new();

    protected StringInputCategoryWithTestBase(string key) : base(key)
    {
    }

    protected override string GetQuery()
    {
      var right = this.Input.GetRightQuery();
      if (string.IsNullOrEmpty(right))
      {
        return string.Empty;
      }
      return $"{this.Key}{right}";
    }

    protected override bool IsIgnorePropertyToSerializing(string propertyName)
    {
      var v = base.IsIgnorePropertyToSerializing(propertyName);
      if (v)
      {
        return v;
      }

      if (propertyName == nameof(IsTestError))
      {
        return true;
      }
      return false;
    }

    protected virtual Task TestAsync()
    {
      return Task.CompletedTask;
    }

    public ICommand TestCommand =>
      this._testCommand ??= new AsyncReactiveCommand<object>().WithSubscribe(async _ =>
      {
        this.TestResult.Clear();
        this.IsTestError.Value = false;

        if (string.IsNullOrWhiteSpace(this.Input.Value.Value))
        {
          return;
        }

        try
        {
          await this.TestAsync();
        }
        catch
        {
          this.IsTestError.Value = true;
        }
      }).AddTo(this.Disposables);
    private ICommand? _testCommand;
  }

  public class NumberInputCategoryBase : FinderQueryInputCategory
  {
    protected string Key { get; }

    public FinderQueryNumberInput Input { get; }

    protected NumberInputCategoryBase(string key) : this(key, false)
    {
    }

    protected NumberInputCategoryBase(string key, bool isCompareWithHorse)
    {
      this.Key = key;
      this.Input = new FinderQueryNumberInput(isCompareWithHorse);

      this.Input.AddTo(this.Disposables);
      this.Input.ToObservable().Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
    }

    protected override string GetQuery()
    {
      var right = this.Input.GetRightQuery();
      if (string.IsNullOrEmpty(right))
      {
        return string.Empty;
      }
      return $"{this.Key}{right}";
    }
  }

  public class FloatNumberInputCategoryBase : FinderQueryInputCategory
  {
    protected string Key { get; }

    public FinderQueryFloatNumberInput Input { get; }

    protected FloatNumberInputCategoryBase(string key, int digit) : this(key, digit, false)
    {
    }

    protected FloatNumberInputCategoryBase(string key, int digit, bool isCompareWithHorse)
    {
      this.Key = key;
      this.Input = new FinderQueryFloatNumberInput(digit, isCompareWithHorse);

      this.Input.AddTo(this.Disposables);
      this.Input.ToObservable().Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
    }

    protected override string GetQuery()
    {
      var right = this.Input.GetRightQuery();
      if (string.IsNullOrEmpty(right))
      {
        return string.Empty;
      }
      return $"{this.Key}{right}";
    }
  }

  #region レース１

  public class CourseInputCategory : ListBoxInputCategoryBase<RaceCourse>
  {
    private bool _isAutoSelecting;

    public ReactiveProperty<bool> IsCentral { get; } = new();

    public ReactiveProperty<bool> IsLocal { get; } = new();

    public CourseInputCategory() : base("course")
    {
      this.SetItems(new List<FinderQueryInputListItem<RaceCourse>>
      {
        new FinderQueryInputListItem<RaceCourse>(ToLabel, RaceCourse.Sapporo),
        new FinderQueryInputListItem<RaceCourse>(ToLabel, RaceCourse.Hakodate),
        new FinderQueryInputListItem<RaceCourse>(ToLabel, RaceCourse.Fukushima),
        new FinderQueryInputListItem<RaceCourse>(ToLabel, RaceCourse.Niigata),
        new FinderQueryInputListItem<RaceCourse>(ToLabel, RaceCourse.Tokyo),
        new FinderQueryInputListItem<RaceCourse>(ToLabel, RaceCourse.Nakayama),
        new FinderQueryInputListItem<RaceCourse>(ToLabel, RaceCourse.Chukyo),
        new FinderQueryInputListItem<RaceCourse>(ToLabel, RaceCourse.Kyoto),
        new FinderQueryInputListItem<RaceCourse>(ToLabel, RaceCourse.Hanshin),
        new FinderQueryInputListItem<RaceCourse>(ToLabel, RaceCourse.Kokura),
        new FinderQueryInputListItem<RaceCourse>(ToLabel, RaceCourse.Mombetsu),
        new FinderQueryInputListItem<RaceCourse>(ToLabel, RaceCourse.Morioka),
        new FinderQueryInputListItem<RaceCourse>(ToLabel, RaceCourse.Mizusawa),
        new FinderQueryInputListItem<RaceCourse>(ToLabel, RaceCourse.Urawa),
        new FinderQueryInputListItem<RaceCourse>(ToLabel, RaceCourse.Funabashi),
        new FinderQueryInputListItem<RaceCourse>(ToLabel, RaceCourse.Oi),
        new FinderQueryInputListItem<RaceCourse>(ToLabel, RaceCourse.Kawazaki),
        new FinderQueryInputListItem<RaceCourse>(ToLabel, RaceCourse.Kanazawa),
        new FinderQueryInputListItem<RaceCourse>(ToLabel, RaceCourse.Kasamatsu),
        new FinderQueryInputListItem<RaceCourse>(ToLabel, RaceCourse.Nagoya),
        new FinderQueryInputListItem<RaceCourse>(ToLabel, RaceCourse.Sonoda),
        new FinderQueryInputListItem<RaceCourse>(ToLabel, RaceCourse.Himeji),
        new FinderQueryInputListItem<RaceCourse>(ToLabel, RaceCourse.Kochi),
        new FinderQueryInputListItem<RaceCourse>(ToLabel, RaceCourse.Saga),
        new FinderQueryInputListItem<RaceCourse>(ToLabel, RaceCourse.ObihiroBannei),
        new FinderQueryInputListItem<RaceCourse>("外国", RaceCourse.Foreign),
      });

      this.IsCentral.Subscribe(v =>
      {
        if (v && !this._isAutoSelecting)
        {
          this._isAutoSelecting = true;
          foreach (var item in this.Items)
          {
            item.IsChecked.Value = false;
          }
          foreach (var item in this.Items.Where(i => i.Value <= RaceCourse.CentralMaxValue))
          {
            item.IsChecked.Value = true;
          }
          this._isAutoSelecting = false;
        }
      }).AddTo(this.Disposables);
      this.IsLocal.Subscribe(v =>
      {
        if (v && !this._isAutoSelecting)
        {
          this._isAutoSelecting = true;
          foreach (var item in this.Items)
          {
            item.IsChecked.Value = false;
          }
          foreach (var item in this.Items.Where(i => i.Value > RaceCourse.CentralMaxValue && i.Value != RaceCourse.ObihiroBannei && i.Value != RaceCourse.Foreign))
          {
            item.IsChecked.Value = true;
          }
          this._isAutoSelecting = false;
        }
      }).AddTo(this.Disposables);
      this.Items.ChangedItemObservable.Subscribe(_ =>
      {
        if (!this._isAutoSelecting)
        {
          this._isAutoSelecting = true;
          var values = this.Items.GetCheckedValues().ToArray();
          if (values.All(v => v <= RaceCourse.CentralMaxValue) && values.Length == 10)
          {
            this.IsCentral.Value = true;
          }
          else if (values.All(v => v > RaceCourse.CentralMaxValue && v < RaceCourse.ObihiroBannei) && values.Length == 14)
          {
            this.IsLocal.Value = true;
          }
          else
          {
            this.IsCentral.Value = this.IsLocal.Value = false;
          }
          this._isAutoSelecting = false;
        }
      }).AddTo(this.Disposables);
    }

    protected override string GetQuery()
    {
      if (this.IsSetCurrentRaceValue.Value || this.IsSetCurrentRaceHorseValue.Value)
      {
        return this.Key;
      }

      var query = base.GetQuery();
      var values = this.Items.GetCheckedValues();

      if (this.Items.IsAll() || this.Items.IsEmpty())
      {
        return string.Empty;
      }

      if (values.Any(v => v == RaceCourse.Foreign))
      {
        values = values.Where(v => v != RaceCourse.Foreign);
        values = values.Concat(Enum.GetValues<RaceCourse>().Where(c => c > RaceCourse.Foreign));
      }
      if (values.Any(v => v >= RaceCourse.LocalMinValue))
      {
        return $"course={string.Join(',', values.Select(v => this.ToQueryValue(v)))}";
      }

      return query;
    }

    protected override string ToQueryValue(RaceCourse value)
    {
      return ((short)value).ToString();
    }

    private static string ToLabel(RaceCourse value) => value.GetName();
  }

  public class MonthInputCategory : ListBoxInputCategoryBase<short>
  {
    public MonthInputCategory() : base("month")
    {
      this.SetItems(new List<FinderQueryInputListItem<short>>
      {
        new FinderQueryInputListItem<short>(1),
        new FinderQueryInputListItem<short>(2),
        new FinderQueryInputListItem<short>(3),
        new FinderQueryInputListItem<short>(4),
        new FinderQueryInputListItem<short>(5),
        new FinderQueryInputListItem<short>(6),
        new FinderQueryInputListItem<short>(7),
        new FinderQueryInputListItem<short>(8),
        new FinderQueryInputListItem<short>(9),
        new FinderQueryInputListItem<short>(10),
        new FinderQueryInputListItem<short>(11),
        new FinderQueryInputListItem<short>(12),
      });
    }
  }

  public class YearInputCategory : ListBoxInputCategoryBase<short>
  {
    public YearInputCategory() : base("year")
    {
      this.SetItems(Enumerable.Range(1986, DateTime.Today.AddDays(14).Year - 1986 + 1)
        .Reverse()
        .Select(i => new FinderQueryInputListItem<short>((short)i)));
    }
  }

  public class NichijiInputCategory : ListBoxInputCategoryBase<short>
  {
    public NichijiInputCategory() : base("nichiji")
    {
      this.SetItems(Enumerable.Range(1, 12)
        .Select(i => new FinderQueryInputListItem<short>((short)i)));
    }
  }

  public class RaceNumberInputCategory : ListBoxInputCategoryBase<short>
  {
    public RaceNumberInputCategory() : base("racenumber")
    {
      this.SetItems(Enumerable.Range(1, 12)
        .Select(i => new FinderQueryInputListItem<short>((short)i)));
    }
  }

  public class RiderWeightRuleInputCategory : ListBoxInputCategoryBase<RaceRiderWeightRule>
  {
    public RiderWeightRuleInputCategory() : base("riderweightrule")
    {
      this.SetItems(new List<FinderQueryInputListItem<RaceRiderWeightRule>>
      {
        new FinderQueryInputListItem<RaceRiderWeightRule>("なし", RaceRiderWeightRule.Unset),
        new FinderQueryInputListItem<RaceRiderWeightRule>("ハンデ", RaceRiderWeightRule.Handicap),
        new FinderQueryInputListItem<RaceRiderWeightRule>("別定", RaceRiderWeightRule.SpecialWeight),
        new FinderQueryInputListItem<RaceRiderWeightRule>("馬齢", RaceRiderWeightRule.WeightForAge),
        new FinderQueryInputListItem<RaceRiderWeightRule>("定量", RaceRiderWeightRule.SpecialWeightForAge),
      });
    }

    protected override string ToQueryValue(RaceRiderWeightRule value)
    {
      return ((short)value).ToString();
    }
  }

  public class RaceAreaRuleInputCategory : ListBoxInputCategoryBase<RaceHorseAreaRule>
  {
    public RaceAreaRuleInputCategory() : base("arearule")
    {
      this.SetItems(new List<FinderQueryInputListItem<RaceHorseAreaRule>>
      {
        new FinderQueryInputListItem<RaceHorseAreaRule>("限定なし", RaceHorseAreaRule.Unknown),
        new FinderQueryInputListItem<RaceHorseAreaRule>("混合", RaceHorseAreaRule.Mixed),
        new FinderQueryInputListItem<RaceHorseAreaRule>("（父）", RaceHorseAreaRule.Father),
        new FinderQueryInputListItem<RaceHorseAreaRule>("（市）", RaceHorseAreaRule.Market),
        new FinderQueryInputListItem<RaceHorseAreaRule>("（抽）", RaceHorseAreaRule.Lottery),
        new FinderQueryInputListItem<RaceHorseAreaRule>("「抽」", RaceHorseAreaRule.Lottery2),
        new FinderQueryInputListItem<RaceHorseAreaRule>("(市)(抽)", RaceHorseAreaRule.MarketLottery),
        new FinderQueryInputListItem<RaceHorseAreaRule>("（抽）関西", RaceHorseAreaRule.LotteryWest),
        new FinderQueryInputListItem<RaceHorseAreaRule>("（抽）関東", RaceHorseAreaRule.LotteryEast),
        new FinderQueryInputListItem<RaceHorseAreaRule>("「抽」関西", RaceHorseAreaRule.Lottery2West),
        new FinderQueryInputListItem<RaceHorseAreaRule>("「抽」関東", RaceHorseAreaRule.Lottery2East),
        new FinderQueryInputListItem<RaceHorseAreaRule>("(市)(抽) 関西", RaceHorseAreaRule.MarketLotteryWest),
        new FinderQueryInputListItem<RaceHorseAreaRule>("(市)(抽) 関東", RaceHorseAreaRule.MarketLotteryEast),
        new FinderQueryInputListItem<RaceHorseAreaRule>("九州", RaceHorseAreaRule.Kyushu),
        new FinderQueryInputListItem<RaceHorseAreaRule>("国際", RaceHorseAreaRule.International),
        new FinderQueryInputListItem<RaceHorseAreaRule>("地・兵庫など", RaceHorseAreaRule.LimitedHyogo),
        new FinderQueryInputListItem<RaceHorseAreaRule>("地・南関東", RaceHorseAreaRule.LimitedSouthKanto),
        new FinderQueryInputListItem<RaceHorseAreaRule>("地・JRA認定", RaceHorseAreaRule.JraCertificated),
      });
    }

    protected override string GetQuery()
    {
      if (base.IsSetCurrentRaceValue.Value)
      {
        return this.Key;
      }

      var query = base.GetQuery();
      var values = this.Items.GetCheckedValues();

      if (this.Items.IsAll() || this.Items.IsEmpty())
      {
        return string.Empty;
      }

      if (values.Any(v => v > RaceHorseAreaRule.International))
      {
        return $"{this.Key}={string.Join(',', values.Select(v => this.ToQueryValue(v)))}";
      }

      return query;
    }

    protected override string ToQueryValue(RaceHorseAreaRule value)
    {
      return ((short)value).ToString();
    }
  }

  public class RaceSexRuleInputCategory : ListBoxInputCategoryBase<RaceHorseSexRule>
  {
    public RaceSexRuleInputCategory() : base("sexrule")
    {
      this.SetItems(new List<FinderQueryInputListItem<RaceHorseSexRule>>
      {
        new FinderQueryInputListItem<RaceHorseSexRule>("限定なし", RaceHorseSexRule.Unknown),
        new FinderQueryInputListItem<RaceHorseSexRule>("牡", RaceHorseSexRule.Male),
        new FinderQueryInputListItem<RaceHorseSexRule>("牝", RaceHorseSexRule.Female),
        new FinderQueryInputListItem<RaceHorseSexRule>("牡／騸", RaceHorseSexRule.MaleCastrated),
        new FinderQueryInputListItem<RaceHorseSexRule>("牡／牝", RaceHorseSexRule.MaleFemale),
      });
    }

    protected override string GetQuery()
    {
      var values = this.Items.GetCheckedValues().ToArray();
      if (values.Contains(RaceHorseSexRule.Female))
      {
        return $"{this.Key}={string.Join(',', values.Append(RaceHorseSexRule.A).Append(RaceHorseSexRule.B).Select(v => this.ToQueryValue(v)))}";
      }

      return base.GetQuery();
    }

    protected override string ToQueryValue(RaceHorseSexRule value)
    {
      return ((short)value).ToString();
    }
  }

  public class RaceGradeInputCategory : ListBoxInputCategoryBase<RaceGrade>
  {
    public RaceGradeInputCategory() : base("grade")
    {
      this.SetItems(new List<FinderQueryInputListItem<RaceGrade>>
      {
        new FinderQueryInputListItem<RaceGrade>("G1", RaceGrade.Grade1),
        new FinderQueryInputListItem<RaceGrade>("G2", RaceGrade.Grade2),
        new FinderQueryInputListItem<RaceGrade>("G3", RaceGrade.Grade3),
        new FinderQueryInputListItem<RaceGrade>("G", RaceGrade.NoNamedGrade),
        new FinderQueryInputListItem<RaceGrade>("特別", RaceGrade.NonGradeSpecial),
        new FinderQueryInputListItem<RaceGrade>("障害G1", RaceGrade.Steeplechase1),
        new FinderQueryInputListItem<RaceGrade>("障害G2", RaceGrade.Steeplechase2),
        new FinderQueryInputListItem<RaceGrade>("障害G3", RaceGrade.Steeplechase3),
        new FinderQueryInputListItem<RaceGrade>("リステッド", RaceGrade.Listed),
        new FinderQueryInputListItem<RaceGrade>("条件レース", RaceGrade.Others),
        new FinderQueryInputListItem<RaceGrade>("海外G1", RaceGrade.ForeignGrade1),
        new FinderQueryInputListItem<RaceGrade>("海外G2", RaceGrade.ForeignGrade2),
        new FinderQueryInputListItem<RaceGrade>("海外G3", RaceGrade.ForeignGrade3),
        new FinderQueryInputListItem<RaceGrade>("地交流G1", RaceGrade.LocalGrade1),
        new FinderQueryInputListItem<RaceGrade>("地交流G2", RaceGrade.LocalGrade2),
        new FinderQueryInputListItem<RaceGrade>("地交流G3", RaceGrade.LocalGrade3),
        new FinderQueryInputListItem<RaceGrade>("地方G1", RaceGrade.LocalGrade1_UC),
        new FinderQueryInputListItem<RaceGrade>("地方G2", RaceGrade.LocalGrade2_UC),
        new FinderQueryInputListItem<RaceGrade>("地方G3", RaceGrade.LocalGrade3_UC),
        new FinderQueryInputListItem<RaceGrade>("地方重賞", RaceGrade.LocalGrade_UC),
        new FinderQueryInputListItem<RaceGrade>("地方OP", RaceGrade.LocalOpen_UC),
      });
    }

    protected override string ToQueryValue(RaceGrade value)
    {
      return ((short)value).ToString();
    }

    protected override string GetQuery()
    {
      if (base.IsSetCurrentRaceValue.Value)
      {
        return "grade";
      }

      var query = base.GetQuery();
      var values = this.Items.GetCheckedValues();

      if (this.Items.IsAll() || this.Items.IsEmpty())
      {
        return string.Empty;
      }

      if (values.Any(v => (short)v >= 100))
      {
        return $"{this.Key}={string.Join(',', values.Select(v => this.ToQueryValue(v)))}";
      }

      return query;
    }
  }

  public class RaceAgeInputCategory : ListBoxInputCategoryBase<short>
  {
    public RaceAgeInputCategory() : base("subjectage", false)
    {
      base.CanCompareCurrentRaceValue = false;
      this.SetItems(new List<FinderQueryInputListItem<short>>
      {
        new FinderQueryInputListItem<short>(2),
        new FinderQueryInputListItem<short>(3),
        new FinderQueryInputListItem<short>(4),
        new FinderQueryInputListItem<short>(5),
      });
    }
  }

  public class RaceSubjectInputCategory : ListBoxInputCategoryBase<object>
  {
    public RaceSubjectInputCategory() : base("subject", false)
    {
      base.CanCompareCurrentRaceValue = false;
      this.SetItems(new List<FinderQueryInputListItem<object>>
      {
        new FinderQueryInputListItem<object>("新馬戦", RaceSubjectType.NewComer),
        new FinderQueryInputListItem<object>("未出走", RaceSubjectType.Unraced),
        new FinderQueryInputListItem<object>("未勝利", RaceSubjectType.Maiden),
        new FinderQueryInputListItem<object>("オープン", RaceSubjectType.Open),
        new FinderQueryInputListItem<object>("１勝クラス", RaceSubjectType.Win1),
        new FinderQueryInputListItem<object>("２勝クラス", RaceSubjectType.Win2),
        new FinderQueryInputListItem<object>("３勝クラス", RaceSubjectType.Win3),
        new FinderQueryInputListItem<object>("地・Aクラス", RaceClass.ClassA),
        new FinderQueryInputListItem<object>("地・Bクラス", RaceClass.ClassB),
        new FinderQueryInputListItem<object>("地・Cクラス", RaceClass.ClassC),
        new FinderQueryInputListItem<object>("ば・Dクラス", RaceClass.ClassD),
        new FinderQueryInputListItem<object>("地・賞金", RaceClass.Money),
        new FinderQueryInputListItem<object>("地・年齢", RaceClass.Age),
      });
    }

    protected override string ToQueryValue(object value)
    {
      if (value is RaceSubjectType type)
      {
        return ((int)type).ToString();
      }
      if (value is RaceClass cls)
      {
        return cls.ToString().ToLower();
      }
      return base.ToQueryValue(value);
    }

    protected override string GetQuery()
    {
      if (this.Items.IsEmpty())
      {
        return string.Empty;
      }

      var values = this.Items.GetCheckedValues().ToArray();
      var queryValues = values
        .Select(v =>
        {
          switch (v)
          {
            case RaceSubjectType.NewComer:
            case RaceSubjectType.Unraced:
            case RaceSubjectType.Maiden:
            case RaceSubjectType.Open:
              return $"{(int)v},{v.ToString()!.ToLower()}";
            case RaceSubjectType.Win1:
            case RaceSubjectType.Win2:
            case RaceSubjectType.Win3:
              return ((int)v).ToString();
            case RaceClass:
              return v.ToString()!.ToLower();
            default:
              return string.Empty;
          }
        })
        .Where(v => !string.IsNullOrEmpty(v));

      return $"{this.Key}={string.Join(',', queryValues)}";
    }
  }

  public class RaceCrossInputCategory : ListBoxInputCategoryBase<RaceCrossRaceRule>
  {
    public RaceCrossInputCategory() : base("crossrule")
    {
      this.SetItems(new List<FinderQueryInputListItem<RaceCrossRaceRule>>
        {
          new FinderQueryInputListItem<RaceCrossRaceRule>("限定なし", RaceCrossRaceRule.Unknown),
          new FinderQueryInputListItem<RaceCrossRaceRule>("指定", RaceCrossRaceRule.Specificated),
          new FinderQueryInputListItem<RaceCrossRaceRule>("見習", RaceCrossRaceRule.BeginnerRider),
          new FinderQueryInputListItem<RaceCrossRaceRule>("「指定」", RaceCrossRaceRule.Specificated2),
          new FinderQueryInputListItem<RaceCrossRaceRule>("特指", RaceCrossRaceRule.Special),
        });
    }

    protected override string ToQueryValue(RaceCrossRaceRule value)
    {
      return ((short)value).ToString();
    }
  }

  #endregion

  #region レース２

  public class RaceNameInputCategory : StringInputCategoryWithTestBase<RaceNameInputCategory.TestItem>
  {
    public RaceNameInputCategory() : base("racename")
    {
    }

    protected override async Task TestAsync()
    {
      using var db = new MyContext();
      IQueryable<RaceData> query = db.Races!;
      var value = this.Input.Value.Value;
      if (this.Input.IsEqual.Value)
      {
        query = query.Where(r => r.Name == value);
      }
      else if (this.Input.IsContains.Value)
      {
        query = query.Where(r => r.Name.Contains(value));
      }
      else if (this.Input.IsStartsWith.Value)
      {
        query = query.Where(r => r.Name.StartsWith(value));
      }
      else if (this.Input.IsEndsWith.Value)
      {
        query = query.Where(r => r.Name.EndsWith(value));
      }

      try
      {
        var names = await query
          .OrderByDescending(q => q.StartTime)
          .GroupBy(q => q.Name)
          .Select(q => q.Key)
          .Take(30)
          .ToArrayAsync();
        foreach (var n in names)
        {
          this.TestResult.Add(new TestItem
          {
            RaceName = n,
          });
        }
      }
      catch
      {
        this.IsTestError.Value = true;
      }
    }

    public class TestItem
    {
      public string RaceName { get; set; } = string.Empty;
    }
  }

  public class RaceFirstPrizeInputCategory : NumberInputCategoryBase
  {
    public RaceFirstPrizeInputCategory() : base("prize1")
    {
    }
  }

  public class RaceHorsesCountInputCategory : ListBoxInputCategoryBase<short>
  {
    public RaceHorsesCountInputCategory() : base("horsescount")
    {
      // 最大１８頭になったのは１９９２年から。１９８６～１９９１年のレース検索は
      // 現代のレース予想として利用するにはデータの信頼性がないので、このままで問題はなさそう
      this.SetItems(Enumerable.Range(1, 18)
        .Select(v => (short)v)
        .Select(v => new FinderQueryInputListItem<short>(v))
        .ToList());
    }
  }

  public class RaceHorsesGoalCountInputCategory : ListBoxInputCategoryBase<short>
  {
    public RaceHorsesGoalCountInputCategory() : base("goalhorsescount")
    {
      this.SetItems(Enumerable.Range(1, 18)
        .Select(v => (short)v)
        .Select(v => new FinderQueryInputListItem<short>(v))
        .ToList());
    }
  }

  #endregion

  #region トラック

  public class TrackGroundInputCategory : ListBoxInputCategoryBase<TrackGround>
  {
    public TrackGroundInputCategory() : base("ground")
    {
      this.SetItems(new List<FinderQueryInputListItem<TrackGround>>
      {
        new FinderQueryInputListItem<TrackGround>("芝", TrackGround.Turf),
        new FinderQueryInputListItem<TrackGround>("ダート", TrackGround.Dirt),
        new FinderQueryInputListItem<TrackGround>("芝→ダ", TrackGround.TurfToDirt),
        new FinderQueryInputListItem<TrackGround>("サンド", TrackGround.Sand),
      });
    }

    protected override string ToQueryValue(TrackGround value)
    {
      return ((short)value).ToString();
    }
  }

  public class TrackCornerDirectionInputCategory : ListBoxInputCategoryBase<TrackCornerDirection>
  {
    public TrackCornerDirectionInputCategory() : base("direction")
    {
      this.SetItems(new List<FinderQueryInputListItem<TrackCornerDirection>>
      {
        new FinderQueryInputListItem<TrackCornerDirection>("左", TrackCornerDirection.Left),
        new FinderQueryInputListItem<TrackCornerDirection>("右", TrackCornerDirection.Right),
        new FinderQueryInputListItem<TrackCornerDirection>("直線", TrackCornerDirection.Straight),
      });
    }

    protected override string ToQueryValue(TrackCornerDirection value)
    {
      return ((short)value).ToString();
    }
  }

  public class TrackOptionInputCategory : ListBoxInputCategoryBase<TrackOption>
  {
    public TrackOptionInputCategory() : base("trackoption")
    {
      this.SetItems(new List<FinderQueryInputListItem<TrackOption>>
      {
        new FinderQueryInputListItem<TrackOption>("分類なし", TrackOption.Unknown),
        new FinderQueryInputListItem<TrackOption>("外", TrackOption.Outside),
        new FinderQueryInputListItem<TrackOption>("内", TrackOption.Inside),
        new FinderQueryInputListItem<TrackOption>("外→内", TrackOption.OutsideToInside),
        new FinderQueryInputListItem<TrackOption>("内→外", TrackOption.InsideToOutside),
        new FinderQueryInputListItem<TrackOption>("外２周", TrackOption.Outside2),
        new FinderQueryInputListItem<TrackOption>("内２周", TrackOption.Inside2),
      });
    }

    protected override string ToQueryValue(TrackOption value)
    {
      return ((short)value).ToString();
    }
  }

  public class TrackTypeInputCategory : ListBoxInputCategoryBase<TrackType>
  {
    public TrackTypeInputCategory() : base("tracktype")
    {
      this.SetItems(new List<FinderQueryInputListItem<TrackType>>
      {
        new FinderQueryInputListItem<TrackType>("平地", TrackType.Flat),
        new FinderQueryInputListItem<TrackType>("障害", TrackType.Steeplechase),
      });
    }

    protected override string ToQueryValue(TrackType value)
    {
      return ((short)value).ToString();
    }
  }

  public class TrackConditionInputCategory : ListBoxInputCategoryBase<RaceCourseCondition>
  {
    public TrackConditionInputCategory() : base("condition")
    {
      this.SetItems(new List<FinderQueryInputListItem<RaceCourseCondition>>
      {
        new FinderQueryInputListItem<RaceCourseCondition>("良", RaceCourseCondition.Standard),
        new FinderQueryInputListItem<RaceCourseCondition>("稍重", RaceCourseCondition.Good),
        new FinderQueryInputListItem<RaceCourseCondition>("重", RaceCourseCondition.Yielding),
        new FinderQueryInputListItem<RaceCourseCondition>("不良", RaceCourseCondition.Soft),
      });
    }

    protected override string ToQueryValue(RaceCourseCondition value)
    {
      return ((short)value).ToString();
    }
  }

  public class TrackWeatherInputCategory : ListBoxInputCategoryBase<RaceCourseWeather>
  {
    public TrackWeatherInputCategory() : base("weather")
    {
      this.SetItems(new List<FinderQueryInputListItem<RaceCourseWeather>>
      {
        new FinderQueryInputListItem<RaceCourseWeather>("晴れ", RaceCourseWeather.Fine),
        new FinderQueryInputListItem<RaceCourseWeather>("曇り", RaceCourseWeather.Cloudy),
        new FinderQueryInputListItem<RaceCourseWeather>("雨", RaceCourseWeather.Rainy),
        new FinderQueryInputListItem<RaceCourseWeather>("大雨", RaceCourseWeather.Drizzle),
        new FinderQueryInputListItem<RaceCourseWeather>("雪", RaceCourseWeather.Snow),
        new FinderQueryInputListItem<RaceCourseWeather>("小雪", RaceCourseWeather.LightSnow),
      });
    }

    protected override string ToQueryValue(RaceCourseWeather value)
    {
      return ((short)value).ToString();
    }
  }

  public class TrackDistanceInputCategory : NumberInputCategoryBase
  {
    public TrackDistanceInputCategory() : base("distance")
    {
    }
  }

  #endregion

  #region 本レース出場馬

  public class HorseOfCurrentRaceInputCategory : FinderQueryInputCategory
  {
    public bool IsEnabled => this.Horses.Any();

    public ReactiveProperty<bool> IsUnspecified { get; } = new(true);

    public ReactiveProperty<bool> IsAllHorses { get; } = new();

    public ReactiveProperty<bool> IsHorseNumber { get; } = new();

    public ReactiveProperty<bool> IsHorseBlood { get; } = new();

    public ReactiveProperty<bool> IsAllRiders { get; } = new();

    public ReactiveProperty<bool> IsRider { get; } = new();

    public ReactiveProperty<bool> IsAllTrainers { get; } = new();

    public ReactiveProperty<bool> IsTrainer { get; } = new();

    public ReactiveProperty<bool> IsActiveHorse { get; } = new();

    public ReactiveProperty<bool> IsActiveHorseBlood { get; } = new();

    public ReactiveProperty<bool> IsActiveHorseSelf { get; } = new(true);

    public ReactiveProperty<bool> IsActiveHorseRider { get; } = new();

    public ReactiveProperty<bool> IsActiveHorseTrainer { get; } = new();

    public FinderQueryBloodRelationInput BloodInput { get; } = new();

    public ReactiveCollection<HorseItem> Horses { get; } = new();

    public ReactiveProperty<HorseItem?> SelectedHorse { get; } = new();

    public HorseOfCurrentRaceInputCategory(IReadOnlyList<RaceHorseData>? raceHorses)
    {
      if (raceHorses != null)
      {
        foreach (var horse in raceHorses)
        {
          this.Horses.Add(new HorseItem
          {
            Name = horse.Name,
            Number = horse.Number,
            Key = horse.Key,
          });
        }
      }

      this.IsUnspecified.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.IsAllHorses.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.IsHorseNumber.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.IsHorseBlood.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.IsAllRiders.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.IsRider.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.IsAllTrainers.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.IsTrainer.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.IsActiveHorse.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.IsActiveHorseBlood.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.IsActiveHorseSelf.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.IsActiveHorseRider.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.IsActiveHorseTrainer.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.SelectedHorse.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.BloodInput.AddTo(this.Disposables);
      this.BloodInput.ToObservable().Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
    }

    protected override string GetQuery()
    {
      if (this.IsUnspecified.Value)
      {
        return string.Empty;
      }
      if (this.IsAllHorses.Value)
      {
        return "horse#";
      }
      if (this.IsAllRiders.Value)
      {
        return "rider#";
      }
      if (this.IsAllTrainers.Value)
      {
        return "trainer#";
      }
      if (this.IsActiveHorse.Value)
      {
        var queries = new List<string>();
        if (this.IsActiveHorseSelf.Value)
        {
          queries.Add("horse");
        }
        if (this.IsActiveHorseRider.Value)
        {
          queries.Add("rider");
        } 
        if (this.IsActiveHorseTrainer.Value)
        {
          queries.Add("trainer");
        }
        return string.Join('|', queries);
      }
      if (this.IsHorseNumber.Value || this.IsRider.Value || this.IsTrainer.Value)
      {
        var key = this.IsHorseNumber.Value ? "horse" : this.IsRider.Value ? "rider" : "trainer";

        if (this.SelectedHorse.Value != null)
        {
          // 最初からKeyを使えよといわれるかもしれないが、
          // 検索条件の保存機能、読み込み機能を考慮してこのようにしている
          // （Keyを使っちゃうと、他のレースで検索条件を読み込んだ時に意図しない動きをするかもしれない）
          if (this.SelectedHorse.Value.Number != 0)
          {
            return $"{key}#{this.SelectedHorse.Value.Number}";
          }
          else
          {
            return $"{key}={this.SelectedHorse.Value.Key}";
          }
        }
      }
      if (this.IsHorseBlood.Value)
      {
        if (this.SelectedHorse.Value != null)
        {
          if (this.SelectedHorse.Value.Number != 0)
          {
            return $"{HorseBloodUtil.ToStringCode(this.BloodInput.GetBloodType())}:#{this.SelectedHorse.Value.Number}";
          }
          else
          {
            return $"{HorseBloodUtil.ToStringCode(this.BloodInput.GetBloodType())}:{this.SelectedHorse.Value.Key}";
          }
        }
      }
      if (this.IsActiveHorseBlood.Value)
      {
        return $"{HorseBloodUtil.ToStringCode(this.BloodInput.GetBloodType())}:#";
      }

      return string.Empty;
    }

    public class HorseItem
    {
      public string Name { get; init; } = string.Empty;

      public short Number { get; init; }

      public string Key { get; init; } = string.Empty;
    }
  }

  #endregion

  #region 馬

  public class HorseNameInputCategory : StringInputCategoryWithTestBase<HorseNameInputCategory.TestItem>
  {
    public HorseNameInputCategory() : base("horsename")
    {
    }

    protected override async Task TestAsync()
    {
      using var db = new MyContext();
      IQueryable<RaceHorseData> query = db.RaceHorses!;
      var value = this.Input.Value.Value;
      if (this.Input.IsEqual.Value)
      {
        query = query.Where(r => r.Name == value);
      }
      else if (this.Input.IsContains.Value)
      {
        query = query.Where(r => r.Name.Contains(value));
      }
      else if (this.Input.IsStartsWith.Value)
      {
        query = query.Where(r => r.Name.StartsWith(value));
      }
      else if (this.Input.IsEndsWith.Value)
      {
        query = query.Where(r => r.Name.EndsWith(value));
      }

      try
      {
        var names = await query
          .OrderByDescending(q => q.LastModified)
          .GroupBy(q => q.Name)
          .Select(q => q.Key)
          .Take(50)
          .ToArrayAsync();
        foreach (var n in names)
        {
          this.TestResult.Add(new TestItem
          {
            Name = n,
          });
        }
      }
      catch
      {
        this.IsTestError.Value = true;
      }
    }

    public class TestItem
    {
      public string Name { get; set; } = string.Empty;
    }
  }

  public class HorseAgeInputCategory : ListBoxInputCategoryBase<short>
  {
    public HorseAgeInputCategory() : base("age", true, true)
    {
      this.SetItems(Enumerable.Range(2, 17)
        .Select(v => (short)v)
        .Select(v => new FinderQueryInputListItem<short>(v))
        .ToList());
    }
  }

  public class HorseSexInputCategory : ListBoxInputCategoryBase<HorseSex>
  {
    public HorseSexInputCategory() : base("sex", true, true)
    {
      this.SetItems(new List<FinderQueryInputListItem<HorseSex>>
      {
        new FinderQueryInputListItem<HorseSex>("牡", HorseSex.Male),
        new FinderQueryInputListItem<HorseSex>("牝", HorseSex.Female),
        new FinderQueryInputListItem<HorseSex>("騸", HorseSex.Castrated),
      });
    }

    protected override string ToQueryValue(HorseSex value)
    {
      return ((short)value).ToString();
    }
  }

  public class HorseColorInputCategory : ListBoxInputCategoryBase<HorseBodyColor>
  {
    public HorseColorInputCategory() : base("color", true, true)
    {
      this.SetItems(new List<FinderQueryInputListItem<HorseBodyColor>>
      {
        new FinderQueryInputListItem<HorseBodyColor>("栗", HorseBodyColor.Chestnut),
        new FinderQueryInputListItem<HorseBodyColor>("栃栗", HorseBodyColor.DarkChestnut),
        new FinderQueryInputListItem<HorseBodyColor>("鹿", HorseBodyColor.Bay),
        new FinderQueryInputListItem<HorseBodyColor>("黒鹿", HorseBodyColor.DarkBay),
        new FinderQueryInputListItem<HorseBodyColor>("青鹿", HorseBodyColor.Brown),
        new FinderQueryInputListItem<HorseBodyColor>("青", HorseBodyColor.Black),
        new FinderQueryInputListItem<HorseBodyColor>("芦", HorseBodyColor.Grey),
        new FinderQueryInputListItem<HorseBodyColor>("栗かす", HorseBodyColor.DregChestnut),
        new FinderQueryInputListItem<HorseBodyColor>("鹿かす", HorseBodyColor.DregBay),
        new FinderQueryInputListItem<HorseBodyColor>("青かす", HorseBodyColor.DregBlack),
        new FinderQueryInputListItem<HorseBodyColor>("白", HorseBodyColor.White),
      });
    }

    protected override string ToQueryValue(HorseBodyColor value)
    {
      return ((short)value).ToString();
    }
  }

  public class HorseNumberInputCategory : ListBoxInputCategoryBase<short>
  {
    public HorseNumberInputCategory() : base("horsenumber", true, true)
    {
      this.SetItems(Enumerable.Range(1, 18)
        .Select(v => (short)v)
        .Select(v => new FinderQueryInputListItem<short>(v))
        .ToList());
    }
  }

  public class HorseFrameNumberInputCategory : ListBoxInputCategoryBase<short>
  {
    public HorseFrameNumberInputCategory() : base("framenumber", true, true)
    {
      this.SetItems(Enumerable.Range(1, 8)
        .Select(v => (short)v)
        .Select(v => new FinderQueryInputListItem<short>(v))
        .ToList());
    }
  }

  public class HorsePopularInputCategory : ListBoxInputCategoryBase<short>
  {
    public HorsePopularInputCategory() : base("popular", true, true)
    {
      this.SetItems(Enumerable.Range(1, 18)
        .Select(v => (short)v)
        .Select(v => new FinderQueryInputListItem<short>(v))
        .ToList());
    }
  }

  public class HorseBelongsInputCategory : ListBoxInputCategoryBase<HorseBelongs>
  {
    public HorseBelongsInputCategory() : this("horsebelongs")
    {
      base.CanCompareCurrentRaceValue = false;
    }

    protected HorseBelongsInputCategory(string key): base(key, true, true)
    {
      this.SetItems(new List<FinderQueryInputListItem<HorseBelongs>>
      {
        new FinderQueryInputListItem<HorseBelongs>("美浦", HorseBelongs.Miho),
        new FinderQueryInputListItem<HorseBelongs>("栗東", HorseBelongs.Ritto),
        new FinderQueryInputListItem<HorseBelongs>("地方", HorseBelongs.Local),
        new FinderQueryInputListItem<HorseBelongs>("海外", HorseBelongs.Foreign),
      });
    }

    protected override string ToQueryValue(HorseBelongs value)
    {
      return ((short)value).ToString();
    }
  }

  public class HorseMarkInputCategory : FinderQueryInputCategory
  {
    public ReactiveProperty<bool> IsDoubleCircle { get; } = new();

    public ReactiveProperty<bool> IsCircle { get; } = new();

    public ReactiveProperty<bool> IsFilledTriangle { get; } = new();

    public ReactiveProperty<bool> IsTriangle { get; } = new();

    public ReactiveProperty<bool> IsStar { get; } = new();

    public ReactiveProperty<bool> IsDelete { get; } = new();

    public ReactiveProperty<bool> IsDefault { get; } = new();

    public HorseMarkInputCategory()
    {
      this.IsDoubleCircle.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.IsCircle.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.IsFilledTriangle.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.IsTriangle.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.IsStar.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.IsDelete.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.IsDefault.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
    }

    protected override string GetQuery()
    {
      var values = new List<RaceHorseMark>();
      if (this.IsDoubleCircle.Value) values.Add(RaceHorseMark.DoubleCircle);
      if (this.IsCircle.Value) values.Add(RaceHorseMark.Circle);
      if (this.IsFilledTriangle.Value) values.Add(RaceHorseMark.FilledTriangle);
      if (this.IsTriangle.Value) values.Add(RaceHorseMark.Triangle);
      if (this.IsStar.Value) values.Add(RaceHorseMark.Star);
      if (this.IsDelete.Value) values.Add(RaceHorseMark.Deleted);
      if (this.IsDefault.Value) values.Add(RaceHorseMark.Default);

      if (!values.Any())
      {
        return string.Empty;
      }

      return $"mark={string.Join(',', values.Select(v => (short)v))}";
    }
  }

  public class HorseWeightInputCategory : NumberInputCategoryBase
  {
    public HorseWeightInputCategory() : base("weight", true)
    {
    }
  }

  public class HorseWeightDiffInputCategory : NumberInputCategoryBase
  {
    public HorseWeightDiffInputCategory() : base("weightdiff", true)
    {
    }
  }

  public class RiderWeightInputCategory : FloatNumberInputCategoryBase
  {
    public RiderWeightInputCategory() : base("riderweight", 1, true)
    {
    }
  }

  public class OddsInputCategory : FloatNumberInputCategoryBase
  {
    public OddsInputCategory() : base("odds", 1, true)
    {
    }
  }

  public class PlaceOddsMinInputCategory : FloatNumberInputCategoryBase
  {
    public PlaceOddsMinInputCategory() : base("placeoddsmin", 1, true)
    {
    }
  }

  public class PlaceOddsMaxInputCategory : FloatNumberInputCategoryBase
  {
    public PlaceOddsMaxInputCategory() : base("placeoddsmax", 1, true)
    {
    }
  }

  #endregion

  #region 馬（ローテーション）

  public class PreviousRaceDaysInputCategory : NumberInputCategoryBase
  {
    public PreviousRaceDaysInputCategory() : base("prevdays", true)
    {
    }
  }

  public class HorseRaceCountInputCategory : NumberInputCategoryBase
  {
    public HorseRaceCountInputCategory() : base("racecount", true)
    {
    }
  }

  public class HorseRaceCountRunInputCategory : NumberInputCategoryBase
  {
    public HorseRaceCountRunInputCategory() : base("racecount_run", true)
    {
    }
  }

  public class HorseRaceCountCompleteInputCategory : NumberInputCategoryBase
  {
    public HorseRaceCountCompleteInputCategory() : base("racecount_complete", true)
    {
    }
  }

  public class HorseRaceCountRestInputCategory : NumberInputCategoryBase
  {
    public HorseRaceCountRestInputCategory() : base("racecount_rest", true)
    {
    }
  }

  #endregion

  #region 馬（結果）

  public class ResultTimeInputCategory : FloatNumberInputCategoryBase
  {
    public ResultTimeInputCategory() : base("resulttime", 1, true)
    {
    }
  }

  public class A3HResultTimeInputCategory : FloatNumberInputCategoryBase
  {
    public A3HResultTimeInputCategory() : base("a3htime", 1, true)
    {
    }
  }

  public class ResultTimeDiffInputCategory : FloatNumberInputCategoryBase
  {
    public ResultTimeDiffInputCategory() : base("resulttimediff", 1, true)
    {
    }
  }

  public class HorsePlaceInputCategory : ListBoxInputCategoryBase<short>
  {
    public HorsePlaceInputCategory() : base("place", true, true)
    {
      this.SetItems(Enumerable.Range(1, 18)
        .Select(v => (short)v)
        .Select(v => new FinderQueryInputListItem<short>(v))
        .ToList());
    }
  }

  public class HorseGoalPlaceInputCategory : ListBoxInputCategoryBase<short>
  {
    public HorseGoalPlaceInputCategory() : base("goalplace", true, true)
    {
      this.SetItems(Enumerable.Range(1, 18)
        .Select(v => (short)v)
        .Select(v => new FinderQueryInputListItem<short>(v))
        .ToList());
    }
  }

  public class AbnormalResultInputCategory : ListBoxInputCategoryBase<RaceAbnormality>
  {
    public AbnormalResultInputCategory() : base("abnormal", true, true)
    {
      this.SetItems(new List<FinderQueryInputListItem<RaceAbnormality>>
      {
        new FinderQueryInputListItem<RaceAbnormality>("なし", RaceAbnormality.Unknown),
        new FinderQueryInputListItem<RaceAbnormality>("出走取消", RaceAbnormality.Scratched),
        new FinderQueryInputListItem<RaceAbnormality>("発走除外", RaceAbnormality.ExcludedByStarters),
        new FinderQueryInputListItem<RaceAbnormality>("競走除外", RaceAbnormality.ExcludedByStewards),
        new FinderQueryInputListItem<RaceAbnormality>("競走中止", RaceAbnormality.FailToFinish),
        new FinderQueryInputListItem<RaceAbnormality>("失格", RaceAbnormality.Disqualified),
        new FinderQueryInputListItem<RaceAbnormality>("再騎乗", RaceAbnormality.Remount),
        new FinderQueryInputListItem<RaceAbnormality>("降着", RaceAbnormality.DisqualifiedAndPlaced),
      });
    }

    protected override string ToQueryValue(RaceAbnormality value)
    {
      return ((short)value).ToString();
    }
  }

  public class CornerResultInputCategory : ListBoxInputCategoryBase<short>
  {
    public CornerResultInputCategory(int cornerNumber) : base("corner" + cornerNumber, true, true)
    {
      this.SetItems(Enumerable.Range(1, 18)
        .Select(v => (short)v)
        .Select(v => new FinderQueryInputListItem<short>(v))
        .ToList());
    }
  }

  public class RunningStyleInputCategory : ListBoxInputCategoryBase<RunningStyle>
  {
    public RunningStyleInputCategory() : base("runningstyle", true, true)
    {
      this.SetItems(new List<FinderQueryInputListItem<RunningStyle>>
      {
        new FinderQueryInputListItem<RunningStyle>("不明", RunningStyle.Unknown),
        new FinderQueryInputListItem<RunningStyle>("逃げ", RunningStyle.FrontRunner),
        new FinderQueryInputListItem<RunningStyle>("先行", RunningStyle.Stalker),
        new FinderQueryInputListItem<RunningStyle>("差し", RunningStyle.Sotp),
        new FinderQueryInputListItem<RunningStyle>("追込", RunningStyle.SaveRunner),
      });
    }

    protected override string ToQueryValue(RunningStyle value)
    {
      return ((short)value).ToString();
    }
  }

  #endregion

  #region 血統

  public class HorseBloodInputCategory : FinderQueryInputCategory
  {
    public FinderQueryStringInput HorseName { get; } = new();

    public FinderQueryBloodRelationInput HorseBlood { get; } = new();

    public ReactiveCollection<HorseBloodItem> Horses { get; } = new();

    public ReactiveProperty<HorseBloodItem?> SelectedHorse { get; } = new();

    public ReactiveCollection<HorseBloodConfigItem> Configs { get; } = new();

    public ReactiveProperty<bool> IsSameBloods { get; } = new(true);

    public ReactiveProperty<bool> IsSelfBloods { get; } = new();

    public ReactiveProperty<bool> IsSearchError { get; } = new();

    public HorseBloodInputCategory()
    {
      this.Configs.CollectionChangedAsObservable().Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
    }

    public ICommand SearchHorsesCommand =>
      this._searchHorsesCommand ??= new AsyncReactiveCommand().WithSubscribe(async () =>
      {
        this.IsSearchError.Value = false;
        this.Horses.Clear();

        var value = this.HorseName.Value.Value;
        if (string.IsNullOrWhiteSpace(value))
        {
          return;
        }

        try
        {
          using var db = new MyContext();
          IQueryable<HorseBloodData> horses = db.HorseBloods!;

          if (this.HorseName.IsEqual.Value)
          {
            horses = horses.Where(h => h.Name == value);
          }
          else if (this.HorseName.IsEndsWith.Value)
          {
            horses = horses.Where(h => h.Name.EndsWith(value));
          }
          else if (this.HorseName.IsStartsWith.Value)
          {
            horses = horses.Where(h => h.Name.StartsWith(value));
          }
          else if (this.HorseName.IsContains.Value)
          {
            horses = horses.Where(h => h.Name.Contains(value));
          }
          else
          {
            return;
          }

          var result = await horses.Take(500).ToArrayAsync();
          foreach (var item in result)
          {
            this.Horses.Add(new HorseBloodItem(item.Code, item.Name));
          }
        }
        catch (Exception ex)
        {
          this.IsSearchError.Value = true;
        }
      });
    private ICommand? _searchHorsesCommand;

    public ICommand AddConfigCommand =>
      this._addConfigCommand ??= new ReactiveCommand().WithSubscribe(() =>
      {
        var horse = this.SelectedHorse.Value;
        if (horse != null)
        {
          this.Configs.Add(new HorseBloodConfigItem
          {
            Key = horse.Key,
            Name = horse.Name,
            Type = this.HorseBlood.GetBloodType(),
            IsSelfBlood = this.IsSelfBloods.Value,
          });
        }
      }).AddTo(this.Disposables);
    private ICommand? _addConfigCommand;

    public ICommand RemoveConfigCommand =>
      this._removeConfigCommand ??= new ReactiveCommand<HorseBloodConfigItem>().WithSubscribe(item =>
      {
        this.Configs.Remove(item);
      }).AddTo(this.Disposables);
    private ICommand? _removeConfigCommand;

    protected override string GetQuery()
    {
      return string.Join('|', this.Configs.Select(c => c.ToQuery()));
    }

    protected override bool IsIgnorePropertyToSerializing(string propertyName)
    {
      var r = base.IsIgnorePropertyToSerializing(propertyName);
      r = r || propertyName == nameof(HorseName);
      r = r || propertyName == nameof(HorseBlood);
      r = r || propertyName == nameof(IsSameBloods);
      r = r || propertyName == nameof(IsSelfBloods);
      return r;
    }

    protected override void PropertyToString(PropertyInfo property, StringBuilder text, object obj)
    {
      base.PropertyToString(property, text, obj);

      if (obj == this && property.Name == nameof(Configs))
      {
        var values = string.Join('|', this.Configs.Select(c => c.Serialize()));
        text.Append("Configs=");
        text.Append(values);
        text.AppendLine();
      }
    }

    protected override void StringToProperty(PropertyInfo property, string data, object obj)
    {
      base.StringToProperty(property, data, obj);

      if (obj == this && property.Name == nameof(Configs))
      {
        this.Configs.Clear();

        var values = data.Split('|');
        foreach (var value in values)
        {
          var config = new HorseBloodConfigItem();
          config.Deserialize(value);
          if (!string.IsNullOrEmpty(config.Key))
          {
            this.Configs.Add(config);
          }
        }
      }
    }

    protected override void ResetForce()
    {
      this.Configs.Clear();
    }

    public class HorseBloodItem
    {
      public string Name { get; }

      public string Key { get; }

      public HorseBloodItem(string key, string name)
      {
        this.Key = key;
        this.Name = name;
      }
    }

    public class HorseBloodConfigItem
    {
      public string Key { get; set; } = string.Empty;

      public string Name { get; set; } = string.Empty;

      public BloodType Type { get; set; }

      public bool IsSelfBlood { get; set; }

      public string ToQuery()
      {
        return $"{this.Type.ToStringCode()}:{(this.IsSelfBlood ? "@" : string.Empty)}{this.Key}";
      }

      public string Serialize()
      {
        return $"{this.Key},{this.Name},{(int)this.Type},{this.IsSelfBlood.ToString().ToLower()}";
      }

      public void Deserialize(string data)
      {
        var d = data.Split(',');
        if (d.Length < 4)
        {
          return;
        }
        this.Key = d[0];
        this.Name = d[1];
        int.TryParse(d[2], out var type);
        this.Type = (BloodType)type;
        this.IsSelfBlood = d[3] == "true";
      }
    }
  }

  #endregion

  #region 騎手／調教師

  public class RiderNameInputCategory : StringInputCategoryWithTestBase<RiderNameInputCategory.TestItem>
  {
    public RiderNameInputCategory() : base("ridername")
    {
    }

    protected override async Task TestAsync()
    {
      using var db = new MyContext();
      IQueryable<RaceHorseData> query = db.RaceHorses!;
      var value = this.Input.Value.Value;
      if (this.Input.IsEqual.Value)
      {
        query = query.Where(r => r.RiderName == value);
      }
      else if (this.Input.IsContains.Value)
      {
        query = query.Where(r => r.RiderName.Contains(value));
      }
      else if (this.Input.IsStartsWith.Value)
      {
        query = query.Where(r => r.RiderName.StartsWith(value));
      }
      else if (this.Input.IsEndsWith.Value)
      {
        query = query.Where(r => r.RiderName.EndsWith(value));
      }

      try
      {
        var names = await query
          .OrderByDescending(q => q.LastModified)
          .GroupBy(q => q.RiderName)
          .Select(q => q.Key)
          .Take(50)
          .ToArrayAsync();
        foreach (var n in names)
        {
          this.TestResult.Add(new TestItem
          {
            Name = n,
          });
        }
      }
      catch
      {
        this.IsTestError.Value = true;
      }
    }

    public class TestItem
    {
      public string Name { get; set; } = string.Empty;
    }
  }

  public class RiderBelongsInputCategory : HorseBelongsInputCategory
  {
    public RiderBelongsInputCategory() : base("riderbelongs")
    {
    }
  }

  public class TrainerNameInputCategory : StringInputCategoryWithTestBase<TrainerNameInputCategory.TestItem>
  {
    public TrainerNameInputCategory() : base("trainername")
    {
    }

    protected override async Task TestAsync()
    {
      using var db = new MyContext();
      IQueryable<RaceHorseData> query = db.RaceHorses!;
      var value = this.Input.Value.Value;
      if (this.Input.IsEqual.Value)
      {
        query = query.Where(r => r.TrainerName == value);
      }
      else if (this.Input.IsContains.Value)
      {
        query = query.Where(r => r.TrainerName.Contains(value));
      }
      else if (this.Input.IsStartsWith.Value)
      {
        query = query.Where(r => r.TrainerName.StartsWith(value));
      }
      else if (this.Input.IsEndsWith.Value)
      {
        query = query.Where(r => r.TrainerName.EndsWith(value));
      }

      try
      {
        var names = await query
          .OrderByDescending(q => q.LastModified)
          .GroupBy(q => q.TrainerName)
          .Select(q => q.Key)
          .Take(50)
          .ToArrayAsync();
        foreach (var n in names)
        {
          this.TestResult.Add(new TestItem
          {
            Name = n,
          });
        }
      }
      catch
      {
        this.IsTestError.Value = true;
      }
    }

    public class TestItem
    {
      public string Name { get; set; } = string.Empty;
    }
  }

  public class TrainerBelongsInputCategory : HorseBelongsInputCategory
  {
    public TrainerBelongsInputCategory() : base("trainerbelongs")
    {
    }
  }

  #endregion

  #region レース出走する他馬

  public class SameRaceHorseInputCategory : FinderQueryInputCategory
  {
    public RaceData? Race { get; }

    public IReadOnlyList<RaceHorseAnalyzer>? RaceHorses { get; }

    public ReactiveCollection<FinderModelItem> Items { get; } = new();

    public SameRaceHorseInputCategory(RaceData? race)
    {
      this.Race = race;
    }

    public void AddItem()
    {
      var model = new FinderModel(this.Race, null, this.RaceHorses).AddTo(this.Disposables);
      model.Input.Query.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.Items.Add(new FinderModelItem(model));
    }

    public ICommand AddItemCommand =>
      this._addItemCommand ??= new ReactiveCommand().WithSubscribe(() => this.AddItem()).AddTo(this.Disposables);
    private ICommand? _addItemCommand;

    public ICommand RemoveItemCommand =>
      this._removeItemCommand ??= new ReactiveCommand<FinderModelItem>().WithSubscribe(i =>
      {
        this.Items.Remove(i);
        i.Model.Dispose();
        this.Disposables.Remove(i.Model);
        this.UpdateQuery();
      }).AddTo(this.Disposables);
    private ICommand? _removeItemCommand;

    protected override string GetQuery()
    {
      return string.Join('|', this.Items.Select(i => i.GetQuery()));
    }

    protected override void PropertyToString(PropertyInfo property, StringBuilder text, object obj)
    {
      base.PropertyToString(property, text, obj);

      if (obj == this && property.Name == nameof(Items))
      {
        text.Append("[CUSTOM]Items/");
        foreach (var item in this.Items)
        {
          var serialized = item.Model.Input.Serialize(false)
            .Replace(Environment.NewLine, ";");
          text.Append(item.Name.Value);
          text.Append(";");
          text.Append(serialized);
          text.Append('|');
        }
      }
    }

    protected override void StringToProperty(PropertyInfo property, string data, object obj)
    {
      base.StringToProperty(property, data, obj);

      if (obj == this && property.Name == nameof(Items))
      {
        this.Items.Clear();

        var items = data.Split('|');
        foreach (var item in items)
        {
          var nameSeparator = item.IndexOf(';');
          if (nameSeparator < 0)
          {
            continue;
          }
          var name = item[..nameSeparator];

          var model = new FinderModel(this.Race, null, this.RaceHorses).AddTo(this.Disposables);
          model.Input.Deserialize(item[nameSeparator..].Replace(";", Environment.NewLine));
          model.Input.Query.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
          model.Input.SameRaceHorse.UpdateQuery();

          var i = new FinderModelItem(model)
          {
            Name =
            {
              Value = name,
            }
          };
          this.AddTextCheckForEscape(i.Name);
          this.Items.Add(i);
        }

        this.UpdateQuery();
      }
    }

    protected override void ResetForce()
    {
      this.Items.Clear();
    }

    public class FinderModelItem
    {
      public ReactiveProperty<string> Name { get; } = new(string.Empty);

      public FinderModel Model { get; }

      public FinderModelItem(FinderModel model)
      {
        this.Model = model;
      }

      public string GetQuery()
      {
        var query = this.Model.Input.Query.Value.Replace('|', ';');
        if (string.IsNullOrEmpty(query))
        {
          return string.Empty;
        }
        return "(race)" + query;
      }
    }
  }

  #endregion

  #region 前走

  public class BeforeRaceInputCategory : FinderQueryInputCategory
  {
    public RaceData? Race { get; }

    public ReactiveCollection<FinderModelItem> Items { get; } = new();

    public BeforeRaceInputCategory(RaceData? race)
    {
      this.Race = race;
    }

    public void AddItem()
    {
      var model = new FinderModel(this.Race, null, null).AddTo(this.Disposables);
      model.Input.Query.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      var i = new FinderModelItem(model).AddTo(this.Disposables);
      this.AddTextCheckForEscape(i.BeforeRaceCount);
      this.AddTextCheckForEscape(i.TargetRaceCount);
      this.Items.Add(i);

      Observable.FromEventPattern(ev => i.Updated += ev, ev => i.Updated -= ev)
        .Subscribe(_ => this.UpdateQuery())
        .AddTo(this.Disposables);
    }

    public ICommand AddItemCommand =>
      this._addItemCommand ??= new ReactiveCommand().WithSubscribe(() => this.AddItem()).AddTo(this.Disposables);
    private ICommand? _addItemCommand;

    public ICommand RemoveItemCommand =>
      this._removeItemCommand ??= new ReactiveCommand<FinderModelItem>().WithSubscribe(i =>
      {
        this.Items.Remove(i);
        i.Model.Dispose();
        this.Disposables.Remove(i.Model);
        this.UpdateQuery();
      }).AddTo(this.Disposables);
    private ICommand? _removeItemCommand;

    protected override string GetQuery()
    {
      return string.Join('|', this.Items.Select(i => i.GetQuery()));
    }

    protected override void PropertyToString(PropertyInfo property, StringBuilder text, object obj)
    {
      base.PropertyToString(property, text, obj);

      if (obj == this && property.Name == nameof(Items))
      {
        text.Append("[CUSTOM]Items/");
        foreach (var item in this.Items)
        {
          var serialized = item.Model.Input.Serialize(false)
            .Replace(Environment.NewLine, ";");
          text.Append(item.BeforeRaceCount.Value);
          text.Append(';');
          text.Append(item.TargetRaceCount.Value);
          text.Append(";");
          text.Append(item.IsCountCompletedRaces.Value ? "complete" : item.IsCountRunningRaces.Value ? "run" : "all");
          text.Append(";");
          text.Append(serialized);
          text.Append('|');
        }
      }
    }

    protected override void StringToProperty(PropertyInfo property, string data, object obj)
    {
      base.StringToProperty(property, data, obj);

      if (obj == this && property.Name == nameof(Items))
      {
        this.Items.Clear();

        var items = data.Split('|');
        foreach (var item in items)
        {
          var separator1 = item.IndexOf(';');
          if (separator1 < 0) continue;
          var beforeCount = item[..separator1];

          var separator2 = item.IndexOf(';', separator1 + 1);
          if (separator2 < 0) continue;
          var targetCount = item[(separator1 + 1)..separator2];

          var separator3 = item.IndexOf(';', separator2 + 1);
          if (separator3 < 0) continue;
          var option = item[(separator2 + 1)..separator3];

          var model = new FinderModel(this.Race, null, null).AddTo(this.Disposables);
          model.Input.Deserialize(item[separator3..].Replace(";", Environment.NewLine));
          model.Input.Query.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
          model.Input.BeforeRace.UpdateQuery();

          var i = new FinderModelItem(model)
          {
            BeforeRaceCount = { Value = beforeCount, },
            TargetRaceCount = { Value = targetCount, },
            IsCountCompletedRaces = { Value = option == "complete", },
            IsCountRunningRaces = { Value = option == "run", },
            IsCountAllRaces = { Value = (option != "complete" && option != "run"), },
          }.AddTo(this.Disposables);
          this.AddTextCheckForEscape(i.BeforeRaceCount);
          this.AddTextCheckForEscape(i.TargetRaceCount);
          this.Items.Add(i);

          Observable.FromEventPattern(ev => i.Updated += ev, ev => i.Updated -= ev)
            .Subscribe(_ => this.UpdateQuery())
            .AddTo(this.Disposables);
        }

        this.UpdateQuery();
      }
    }

    protected override void ResetForce()
    {
      this.Items.Clear();
    }

    public class FinderModelItem : IDisposable
    {
      private readonly CompositeDisposable _disposables = new();

      public FinderModel Model { get; }

      public ReactiveProperty<string> BeforeRaceCount { get; } = new("1");

      public ReactiveProperty<string> TargetRaceCount { get; } = new();

      public ReactiveProperty<bool> IsCountAllRaces { get; } = new(true);

      public ReactiveProperty<bool> IsCountRunningRaces { get; } = new();

      public ReactiveProperty<bool> IsCountCompletedRaces { get; } = new();

      public event EventHandler? Updated;

      public FinderModelItem(FinderModel model)
      {
        this.Model = model;

        this.BeforeRaceCount
          .CombineLatest(this.TargetRaceCount)
          .CombineLatest(this.IsCountAllRaces)
          .CombineLatest(this.IsCountRunningRaces)
          .CombineLatest(this.IsCountCompletedRaces)
          .Subscribe(_ => this.Updated?.Invoke(this, EventArgs.Empty))
          .AddTo(this._disposables);
      }

      public string GetQuery()
      {
        var r1 = int.TryParse(this.BeforeRaceCount.Value, out var beforeCount);
        var r2 = int.TryParse(this.TargetRaceCount.Value, out var targetCount);
        if (!r1)
        {
          return string.Empty;
        }

        var query = this.Model.Input.Query.Value.Replace('|', ';');
        if (string.IsNullOrEmpty(query))
        {
          return string.Empty;
        }

        var option = this.IsCountRunningRaces.Value ? "run" :
          this.IsCountCompletedRaces.Value ? "complete" : "all";

        return $"(before<{option}>:{beforeCount},{targetCount}){query}";
      }

      public void Dispose()
      {
        this._disposables.Dispose();
      }
    }
  }

  #endregion

  #region 拡張メモ

  public class MemoInputCategory : FinderQueryInputCategory
  {
    public ReactiveCollection<MemoConfigItem> Items { get; } = new();

    public ReactiveCollection<ExpansionMemoConfig> Configs { get; } = new();

    public ReactiveProperty<ExpansionMemoConfig?> SelectedConfig { get; } = new();

    public ReactiveProperty<bool> IsUpdateRequested { get; } = new();

    public MemoInputCategory()
    {
      this.Items.CollectionChangedAsObservable().Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.Update();
    }

    private MemoConfigItem SetConfigItemEvents(MemoConfigItem item)
    {
      item.Point.AddTo(this.Disposables);
      item.Labels.ChangedItemObservable.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      item.Point.ToObservable().Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      return item;
    }

    private void Update()
    {
      this.Configs.Clear();
      this.SelectedConfig.Value = null;

      foreach (var config in MemoUtil.Configs.Where(c => c.Style != MemoStyle.Memo))
      {
        this.Configs.Add(config);
      }

      foreach (var item in this.Items.ToArray())
      {
        if (!item.Update())
        {
          this.Items.Remove(item);
        }
      }

      this.UpdateQuery();
      this.IsUpdateRequested.Value = false;
    }

    public ICommand UpdateConfigsCommand =>
      this._updateConfigsCommand ??= new ReactiveCommand().WithSubscribe(() => this.Update()).AddTo(this.Disposables);
    private ICommand? _updateConfigsCommand;

    public ICommand AddItemCommand =>
      this._addItemCommand ??= new ReactiveCommand().WithSubscribe(() =>
      {
        var config = this.SelectedConfig.Value;
        if (config != null && !this.Items.Any(i => i.Config.Id == config.Id))
        {
          if (MemoUtil.Configs.Any(c => c.Id == config.Id))
          {
            this.Items.Add(this.SetConfigItemEvents(new MemoConfigItem(config)));
          }
          else
          {
            this.IsUpdateRequested.Value = true;
          }
        }
      }).AddTo(this.Disposables);
    private ICommand? _addItemCommand;

    public ICommand RemoveItemCommand =>
      this._removeItemCommand ??= new ReactiveCommand<MemoConfigItem>().WithSubscribe(item =>
      {
        this.Items.Remove(item);
      }).AddTo(this.Disposables);
    private ICommand? _removeItemCommand;

    protected override string GetQuery()
    {
      return string.Join('|', this.Items.Select(c => c.ToQuery()).Where(q => !string.IsNullOrEmpty(q)));
    }

    protected override bool IsIgnorePropertyToSerializing(string propertyName)
    {
      var r = base.IsIgnorePropertyToSerializing(propertyName);
      r = r || propertyName == nameof(IsUpdateRequested);
      return r;
    }

    protected override void PropertyToString(PropertyInfo property, StringBuilder text, object obj)
    {
      base.PropertyToString(property, text, obj);

      if (obj == this && property.Name == nameof(Items))
      {
        text.Append("Items=");

        foreach (var item in this.Items)
        {
          text.Append(item.Config.Id);
          text.Append(',');

          if (item.LabelConfig != null)
          {
            text.Append("A,");
            var values = string.Join(',', item.Labels.GetCheckedValues().Select(v => v.Point));
            text.Append(values);
          }
          else
          {
            text.Append("B,");
            base.PropertyToString(item.GetType().GetProperty(nameof(MemoConfigItem.Point), BindingFlags.Instance | BindingFlags.Public)!,
              text, item);
          }

          text.Append('|');
        }

        text.AppendLine();
      }
    }

    protected override void StringToProperty(PropertyInfo property, string data, object obj)
    {
      base.StringToProperty(property, data, obj);

      if (obj == this && property.Name == nameof(Items))
      {
        this.Items.Clear();

        var values = data.Split('|');
        foreach (var value in values)
        {
          var configIdIndex = value.IndexOf(',');
          if (configIdIndex <= 0 || !uint.TryParse(value[..configIdIndex], out var configId))
          {
            continue;
          }
          var configTypeIndex = value.IndexOf(',', configIdIndex + 1);
          if (configTypeIndex <= 0)
          {
            continue;
          }
          var configType = value[(configIdIndex + 1)..configTypeIndex];
          var raw = value[(configTypeIndex + 1)..];

          var config = MemoUtil.Configs.FirstOrDefault(c => c.Id == configId);
          if (config == null)
          {
            continue;
          }
          var item = this.SetConfigItemEvents(new MemoConfigItem(config));

          if (configType == "A")
          {
            {
              var label = PointLabelModel.Default.Configs.FirstOrDefault(c => c.Data.Id == (uint)config.PointLabelId);
              if (label == null)
              {
                continue;
              }
            }
            var points = raw.Split(',').Select(v =>
            {
              short.TryParse(v, out var point);
              return point;
            }).Where(v => v != default).ToArray();

            foreach (var lv in points)
            {
              var checkItem = item.Labels.FirstOrDefault(l => l.Value.Point == lv);
              if (checkItem != null)
              {
                checkItem.IsChecked.Value = true;
              }
            }
          }
          else
          {
            this.StringToProperty(item.GetType().GetProperty(nameof(MemoConfigItem.Point), BindingFlags.Instance | BindingFlags.Public)!,
              raw, item);
          }

          this.Items.Add(item);
        }
      }
    }

    protected override void ResetForce()
    {
      this.Items.Clear();
    }

    public class MemoConfigItem
    {
      public ExpansionMemoConfig Config { get; }

      public PointLabelConfig? LabelConfig { get; set; }

      public FinderQueryNumberInput Point { get; } = new();

      public FinderQueryInputListItemCollection<MemoConfigLabelItem> Labels { get; } = new();

      public ReactiveProperty<bool> IsLabel { get; } = new();

      public ReactiveProperty<string> ConfigHeader { get; } = new();

      public MemoConfigItem(ExpansionMemoConfig config)
      {
        this.Config = config;
        this.Update();
      }

      public bool Update()
      {
        var config = MemoUtil.Configs.FirstOrDefault(c => c.Id == this.Config.Id);
        if (config == null || config.Style == MemoStyle.Memo)
        {
          return false;
        }
        this.ConfigHeader.Value = config.Header;

        var oldValues = this.Labels.GetCheckedValues().Select(v => v.Point).ToArray();
        this.Labels.Clear();

        this.LabelConfig = PointLabelModel.Default.Configs.FirstOrDefault(c => c.Data.Id == (uint)this.Config.PointLabelId);
        if (this.LabelConfig != null)
        {
          foreach (var item in this.LabelConfig.Data.GetItems())
          {
            var i = new FinderQueryInputListItem<MemoConfigLabelItem>(new MemoConfigLabelItem(item));
            this.Labels.Add(i);

            if (oldValues.Contains(item.Point))
            {
              i.IsChecked.Value = true;
            }
          }
          this.IsLabel.Value = true;
        }
        else
        {
          this.IsLabel.Value = false;
        }

        return true;
      }

      public class MemoConfigLabelItem
      {
        public string Label { get; }

        public short Point { get; }

        public MemoColor Color { get; }

        public MemoConfigLabelItem(PointLabelItem label)
        {
          this.Label = label.Label;
          this.Color = label.Color;
          this.Point = label.Point;
        }
      }

      public string ToQuery()
      {
        var targets = new[] { this.Config.Target1, this.Config.Target2, this.Config.Target3 };
        var targets2 = targets.Where(t => t != MemoTarget.Unknown).Select(t => MemoUtil.GetMemoTarget(t));

        string point;
        if (this.LabelConfig != null)
        {
          var raw = string.Join(',', this.Labels.GetCheckedValues().Select(v => v.Point));
          if (string.IsNullOrEmpty(raw))
          {
            return string.Empty;
          }
          point = "point=" + raw;
        }
        else
        {
          point = "point" + this.Point.GetRightQuery();
        }

        return $"memo/{string.Join('/', targets2)}/number:{this.Config.MemoNumber}/:{point}";
      }
    }
  }

  #endregion

  #region 外部指数

  public class ExternalNumberInputCategory : FinderQueryInputCategory
  {
    public ReactiveCollection<ExternalNumberConfigItem> Items { get; } = new();

    public ReactiveCollection<ExternalNumberConfig> Configs { get; } = new();

    public ReactiveProperty<ExternalNumberConfig?> SelectedConfig { get; } = new();

    public ReactiveProperty<bool> IsUpdateRequested { get; } = new();

    public ExternalNumberInputCategory()
    {
      this.Items.CollectionChangedAsObservable().Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.Update();
    }

    private ExternalNumberConfigItem SetConfigItemEvents(ExternalNumberConfigItem item)
    {
      item.Point.AddTo(this.Disposables);
      item.Point.ToObservable().Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      return item;
    }

    private void Update()
    {
      this.Configs.Clear();
      this.SelectedConfig.Value = null;

      foreach (var config in ExternalNumberUtil.Configs)
      {
        this.Configs.Add(config);
      }

      foreach (var item in this.Items.ToArray())
      {
        if (!item.Update())
        {
          this.Items.Remove(item);
        }
      }

      this.UpdateQuery();
      this.IsUpdateRequested.Value = false;
    }

    public ICommand UpdateConfigsCommand =>
      this._updateConfigsCommand ??= new ReactiveCommand().WithSubscribe(() => this.Update()).AddTo(this.Disposables);
    private ICommand? _updateConfigsCommand;

    public ICommand AddItemCommand =>
      this._addItemCommand ??= new ReactiveCommand().WithSubscribe(() =>
      {
        var config = this.SelectedConfig.Value;
        if (config != null && !this.Items.Any(i => i.Config.Id == config.Id))
        {
          if (ExternalNumberUtil.Configs.Any(c => c.Id == config.Id))
          {
            this.Items.Add(this.SetConfigItemEvents(new ExternalNumberConfigItem(config)));
          }
          else
          {
            this.IsUpdateRequested.Value = true;
          }
        }
      }).AddTo(this.Disposables);
    private ICommand? _addItemCommand;

    public ICommand RemoveItemCommand =>
      this._removeItemCommand ??= new ReactiveCommand<ExternalNumberConfigItem>().WithSubscribe(item =>
      {
        this.Items.Remove(item);
      }).AddTo(this.Disposables);
    private ICommand? _removeItemCommand;

    protected override string GetQuery()
    {
      return string.Join('|', this.Items.Select(c => c.ToQuery()).Where(q => !string.IsNullOrEmpty(q)));
    }

    protected override bool IsIgnorePropertyToSerializing(string propertyName)
    {
      var r = base.IsIgnorePropertyToSerializing(propertyName);
      r = r || propertyName == nameof(IsUpdateRequested);
      return r;
    }

    protected override void PropertyToString(PropertyInfo property, StringBuilder text, object obj)
    {
      base.PropertyToString(property, text, obj);

      if (obj == this && property.Name == nameof(Items))
      {
        foreach (var item in this.Items)
        {
          text.Append(item.Config.Id);
          text.Append(',');

          base.PropertyToString(item.GetType().GetProperty(nameof(ExternalNumberConfigItem.Point), BindingFlags.Instance | BindingFlags.Public)!,
            text, item);

          text.Append('|');
        }
      }
    }

    protected override void StringToProperty(PropertyInfo property, string data, object obj)
    {
      base.StringToProperty(property, data, obj);

      if (obj == this && property.Name == nameof(Configs))
      {
        this.Configs.Clear();

        var values = data.Split('|');
        foreach (var value in values)
        {
          var configIdIndex = value.IndexOf(',');
          if (configIdIndex <= 0 || !uint.TryParse(value[..configIdIndex], out var configId))
          {
            continue;
          }
          var raw = value[(configIdIndex + 1)..];

          var config = ExternalNumberUtil.Configs.FirstOrDefault(c => c.Id == configId);
          if (config == null)
          {
            continue;
          }
          var item = this.SetConfigItemEvents(new ExternalNumberConfigItem(config));

          this.StringToProperty(item.GetType().GetProperty(nameof(ExternalNumberConfigItem.Point), BindingFlags.Instance | BindingFlags.Public)!,
            raw, item);

          this.Items.Add(item);
        }
      }
    }

    protected override void ResetForce()
    {
      this.Items.Clear();
    }

    public class ExternalNumberConfigItem
    {
      public ExternalNumberConfig Config { get; }

      public FinderQueryNumberInput Point { get; } = new();

      public ReactiveProperty<string> ConfigHeader { get; } = new();

      public ExternalNumberConfigItem(ExternalNumberConfig config)
      {
        this.Config = config;
        this.Update();
      }

      public bool Update()
      {
        var config = ExternalNumberUtil.Configs.FirstOrDefault(c => c.Id == this.Config.Id);
        if (config == null)
        {
          return false;
        }
        this.ConfigHeader.Value = config.Name;

        return true;
      }

      public string ToQuery()
      {
        var point = "point" + this.Point.GetRightQuery();
        return $"ext:{this.Config.Id}/:{point}";
      }
    }
  }

  #endregion

  #region グループ化

  public class GroupByCategoryInput : FinderQueryInputCategory
  {
    public ReactiveProperty<bool> NotGroups { get; } = new(true);
    public ReactiveProperty<bool> IsGroupByHorse { get; } = new();
    public ReactiveProperty<bool> IsGroupByRider { get; } = new();
    public ReactiveProperty<bool> IsGroupByTrainer { get; } = new();
    public ReactiveProperty<bool> IsGroupByOwner { get; } = new();
    public ReactiveProperty<bool> IsGroupByCourse { get; } = new();
    public ReactiveProperty<bool> IsGroupByWeather { get; } = new();
    public ReactiveProperty<bool> IsGroupByCondition { get; } = new();
    public ReactiveProperty<bool> IsGroupByGrade { get; } = new();
    public ReactiveProperty<bool> IsGroupByFrameNumber { get; } = new();
    public ReactiveProperty<bool> IsGroupByHorseNumber { get; } = new();
    public ReactiveProperty<bool> IsGroupByMemo { get; } = new();

    public ReactiveCollection<ExpansionMemoConfig> MemoConfigs { get; } = new();

    public ReactiveProperty<ExpansionMemoConfig?> SelectedMemoConfig { get; } = new();

    public GroupByCategoryInput()
    {
      this.Update();
      this.NotGroups
        .CombineLatest(this.IsGroupByHorse)
        .CombineLatest(this.IsGroupByRider)
        .CombineLatest(this.IsGroupByTrainer)
        .CombineLatest(this.IsGroupByOwner)
        .CombineLatest(this.IsGroupByCourse)
        .CombineLatest(this.IsGroupByWeather)
        .CombineLatest(this.IsGroupByCondition)
        .CombineLatest(this.IsGroupByGrade)
        .CombineLatest(this.IsGroupByFrameNumber)
        .CombineLatest(this.IsGroupByHorseNumber)
        .CombineLatest(this.IsGroupByMemo)
        .CombineLatest(this.SelectedMemoConfig)
        .Subscribe(_ => this.UpdateQuery())
        .AddTo(this.Disposables);
    }

    public void Update()
    {
      this.MemoConfigs.Clear();
      this.SelectedMemoConfig.Value = null;
      foreach (var config in MemoUtil.Configs.Where(c => c.Style != MemoStyle.Memo))
      {
        this.MemoConfigs.Add(config);
      }
      this.UpdateQuery();
    }

    public ICommand UpdateConfigsCommand =>
      this._updateConfigsCommand ??= new ReactiveCommand().WithSubscribe(() => this.Update()).AddTo(this.Disposables);
    private ICommand? _updateConfigsCommand;

    protected override string GetQuery()
    {
      var groups = new List<string>();

      if (this.IsGroupByHorse.Value)
      {
        groups.Add("horse");
      }
      if (this.IsGroupByRider.Value)
      {
        groups.Add("rider");
      }
      if (this.IsGroupByTrainer.Value)
      {
        groups.Add("trainer");
      }
      if (this.IsGroupByOwner.Value)
      {
        groups.Add("owner");
      }
      if (this.IsGroupByCourse.Value)
      {
        groups.Add("course");
      }
      if (this.IsGroupByWeather.Value)
      {
        groups.Add("weather");
      }
      if (this.IsGroupByCondition.Value)
      {
        groups.Add("condition");
      }
      if (this.IsGroupByGrade.Value)
      {
        groups.Add("grade");
      }
      if (this.IsGroupByHorseNumber.Value)
      {
        groups.Add("horsenumber");
      }
      if (this.IsGroupByFrameNumber.Value)
      {
        groups.Add("framenumber");
      }
      if (this.IsGroupByMemo.Value)
      {
        if (this.SelectedMemoConfig.Value != null)
        {
          var config = this.SelectedMemoConfig.Value;
          var targets = new[] { config.Target1, config.Target2, config.Target3 }
            .Select(t => MemoUtil.GetMemoTarget(t))
            .Where(t => !string.IsNullOrEmpty(t))
            .ToArray();
          if (targets.Any())
          {
            groups.Add($"memo/{string.Join('/', targets)}/number:{config.MemoNumber}");
          }
        }
      }

      if (!groups.Any())
      {
        return string.Empty;
      }
      return $"[group]{groups[0]}";
    }
  }

  #endregion

  #region その他の設定

  public class OtherSettingInputCategory : FinderQueryInputCategory
  {
    public ReactiveProperty<bool> IsFinishedRaceOnly { get; } = new();

    public ReactiveProperty<bool> IsContainsFutureRaces { get; } = new();

    public ReactiveProperty<string> LimitBy { get; } = new("3000");

    public OtherSettingInputCategory()
    {
      this.IsFinishedRaceOnly
        .CombineLatest(this.LimitBy)
        .CombineLatest(this.IsContainsFutureRaces)
        .Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
    }

    protected override string GetQuery()
    {
      var queries = new List<string>();

      if (this.IsFinishedRaceOnly.Value)
      {
        queries.Add("datastatus=5,6,7,101,102");
      }
      if (this.IsContainsFutureRaces.Value)
      {
        queries.Add("[future]");
      }
      if (uint.TryParse(this.LimitBy.Value, out var limit) && limit != 3000)
      {
        queries.Add($"[limit]{limit}");
      }

      return string.Join('|', queries);
    }
  }

  #endregion
}
