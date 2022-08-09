using KmyKeiba.Data.Wrappers;
using KmyKeiba.JVLink.Entities;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
