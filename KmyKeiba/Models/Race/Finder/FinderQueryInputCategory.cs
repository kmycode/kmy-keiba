using KmyKeiba.Data.Db;
using KmyKeiba.Data.Wrappers;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KmyKeiba.Models.Race.Finder
{
  public interface IFinderQueryInputCategory : IDisposable
  {
    ReactiveProperty<string> Query { get; }

    ReadOnlyReactiveProperty<bool> IsCustomized { get; }
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

    public void Dispose()
    {
      this.Disposables.Dispose();
    }
  }

  public class ListBoxInputCategoryBase<T> : FinderQueryInputCategory
  {
    public FinderQueryInputListItemCollection<T> Items { get; } = new FinderQueryInputListItemCollection<T>();

    public string Key { get; }

    protected ListBoxInputCategoryBase(string key)
    {
      this.Key = key;
      this.Items.ChangedItemObservable.Subscribe(x => this.UpdateQuery()).AddTo(this.Disposables);
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

    public FinderQueryNumberInput Input { get; } = new();

    protected NumberInputCategoryBase(string key)
    {
      this.Key = key;

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

    protected FloatNumberInputCategoryBase(string key, int digit)
    {
      this.Key = key;
      this.Input = new FinderQueryFloatNumberInput(digit);

      this.Input.Value.Subscribe(_ => base.UpdateQuery()).AddTo(this.Disposables);
      this.Input.MaxValue.Subscribe(_ => base.UpdateQuery()).AddTo(this.Disposables);
      this.Input.IsRange.Subscribe(_ => base.UpdateQuery()).AddTo(this.Disposables);
      this.Input.IsEqual.Subscribe(_ => base.UpdateQuery()).AddTo(this.Disposables);
      this.Input.IsGreaterThan.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.Input.IsGreaterThanOrEqual.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.Input.IsLessThan.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.Input.IsLessThanOrEqual.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.Input.IsNotEqual.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
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
        new FinderQueryInputListItem<RaceGrade>("なし", RaceGrade.Unknown),
        new FinderQueryInputListItem<RaceGrade>("G1", RaceGrade.Grade1),
        new FinderQueryInputListItem<RaceGrade>("G2", RaceGrade.Grade2),
        new FinderQueryInputListItem<RaceGrade>("G3", RaceGrade.Grade3),
        new FinderQueryInputListItem<RaceGrade>("G", RaceGrade.NoNamedGrade),
        new FinderQueryInputListItem<RaceGrade>("特別", RaceGrade.NonGradeSpecial),
        new FinderQueryInputListItem<RaceGrade>("障害G1", RaceGrade.Steeplechase1),
        new FinderQueryInputListItem<RaceGrade>("障害G2", RaceGrade.Steeplechase2),
        new FinderQueryInputListItem<RaceGrade>("障害G3", RaceGrade.Steeplechase3),
        new FinderQueryInputListItem<RaceGrade>("リステッド", RaceGrade.Listed),
        new FinderQueryInputListItem<RaceGrade>("その他", RaceGrade.Others),
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
    public RaceAgeInputCategory() : base("subjectage")
    {
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
    public RaceSubjectInputCategory() : base("subject")
    {
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

    protected override string GetQuery()
    {
      var right = this.Input.GetRightQuery();
      if (string.IsNullOrEmpty(right))
      {
        return string.Empty;
      }
      return $"racename{right}";
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
      if (this.IsHorseNumber.Value)
      {
        if (this.SelectedHorse.Value != null)
        {
          // 最初からKeyを使えよといわれるかもしれないが、
          // 検索条件の保存機能、読み込み機能を考慮してこのようにしている
          // （Keyを使っちゃうと、他のレースで検索条件を読み込んだ時に意図しない動きをするかもしれない）
          if (this.SelectedHorse.Value.Number != 0)
          {
            return $"horse#{this.SelectedHorse.Value.Number}";
          }
          else
          {
            return $"horse={this.SelectedHorse.Value.Key}";
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

    protected override string GetQuery()
    {
      var right = this.Input.GetRightQuery();
      if (string.IsNullOrEmpty(right))
      {
        return string.Empty;
      }
      return $"horsename{right}";
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
    public HorseAgeInputCategory() : base("age")
    {
      this.SetItems(Enumerable.Range(2, 17)
        .Select(v => (short)v)
        .Select(v => new FinderQueryInputListItem<short>(v))
        .ToList());
    }
  }

  public class HorseSexInputCategory : ListBoxInputCategoryBase<HorseSex>
  {
    public HorseSexInputCategory() : base("sex")
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
    public HorseColorInputCategory() : base("color")
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
    public HorseNumberInputCategory() : base("number")
    {
      this.SetItems(Enumerable.Range(1, 18)
        .Select(v => (short)v)
        .Select(v => new FinderQueryInputListItem<short>(v))
        .ToList());
    }
  }

  public class HorseFrameNumberInputCategory : ListBoxInputCategoryBase<short>
  {
    public HorseFrameNumberInputCategory() : base("framenumber")
    {
      this.SetItems(Enumerable.Range(1, 8)
        .Select(v => (short)v)
        .Select(v => new FinderQueryInputListItem<short>(v))
        .ToList());
    }
  }

  public class HorsePopularInputCategory : ListBoxInputCategoryBase<short>
  {
    public HorsePopularInputCategory() : base("popular")
    {
      this.SetItems(Enumerable.Range(1, 18)
        .Select(v => (short)v)
        .Select(v => new FinderQueryInputListItem<short>(v))
        .ToList());
    }
  }

  public class HorseBelongsInputCategory : ListBoxInputCategoryBase<HorseBelongs>
  {
    public HorseBelongsInputCategory() : base("horsebelongs")
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

    public HorseMarkInputCategory()
    {
      this.IsDoubleCircle.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.IsCircle.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.IsFilledTriangle.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.IsTriangle.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.IsStar.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
      this.IsDelete.Subscribe(_ => this.UpdateQuery()).AddTo(this.Disposables);
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

      if (!values.Any())
      {
        return string.Empty;
      }

      return $"mark={string.Join(',', values.Select(v => (short)v))}";
    }
  }

  public class HorseWeightInputCategory : NumberInputCategoryBase
  {
    public HorseWeightInputCategory() : base("weight")
    {
    }
  }

  public class HorseWeightDiffInputCategory : NumberInputCategoryBase
  {
    public HorseWeightDiffInputCategory() : base("weightdiff")
    {
    }
  }

  public class RiderWeightInputCategory : FloatNumberInputCategoryBase
  {
    public RiderWeightInputCategory() : base("riderweight", 1)
    {
    }
  }

  public class OddsInputCategory : FloatNumberInputCategoryBase
  {
    public OddsInputCategory() : base("odds", 1)
    {
    }
  }

  public class PlaceOddsMinInputCategory : FloatNumberInputCategoryBase
  {
    public PlaceOddsMinInputCategory() : base("placeoddsmin", 1)
    {
    }
  }

  public class PlaceOddsMaxInputCategory : FloatNumberInputCategoryBase
  {
    public PlaceOddsMaxInputCategory() : base("placeoddsmax", 1)
    {
    }
  }

  #endregion

  #region 馬（結果）

  public class HorsePlaceInputCategory : ListBoxInputCategoryBase<short>
  {
    public HorsePlaceInputCategory() : base("place")
    {
      this.SetItems(Enumerable.Range(1, 18)
        .Select(v => (short)v)
        .Select(v => new FinderQueryInputListItem<short>(v))
        .ToList());
    }
  }

  public class HorseGoalPlaceInputCategory : ListBoxInputCategoryBase<short>
  {
    public HorseGoalPlaceInputCategory() : base("goalplace")
    {
      this.SetItems(Enumerable.Range(1, 18)
        .Select(v => (short)v)
        .Select(v => new FinderQueryInputListItem<short>(v))
        .ToList());
    }
  }

  #endregion
}
