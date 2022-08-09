using KmyKeiba.Data.Wrappers;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis.Generic;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.Finder
{
  public class FinderQueryInput : IDisposable
  {
    private readonly CompositeDisposable _disposables = new();
    private readonly List<IFinderQueryInputCategory> _categories = new();

    public CourseInputCategory Course { get; }

    public MonthInputCategory Month { get; }

    public YearInputCategory Year { get; }

    public NichijiInputCategory Nichiji { get; }

    public RaceNumberInputCategory RaceNumber { get; }

    public RiderWeightRuleInputCategory RiderWeightRule { get; }

    public RaceAreaRuleInputCategory AreaRule { get; }

    public RaceSexRuleInputCategory SexRule { get; }

    public RaceGradeInputCategory Grade { get; }

    public RaceAgeInputCategory RaceAge { get; }

    public RaceSubjectInputCategory Subject { get; }

    public RaceCrossInputCategory Cross { get; }

    public ReactiveProperty<string> Query { get; } = new();

    public FinderQueryInput()
    {
      this._categories.Add(this.Course = new CourseInputCategory());
      this._categories.Add(this.Month = new MonthInputCategory());
      this._categories.Add(this.Year = new YearInputCategory());
      this._categories.Add(this.Nichiji = new NichijiInputCategory());
      this._categories.Add(this.RaceNumber = new RaceNumberInputCategory());
      this._categories.Add(this.RiderWeightRule = new RiderWeightRuleInputCategory());
      this._categories.Add(this.AreaRule = new RaceAreaRuleInputCategory());
      this._categories.Add(this.SexRule = new RaceSexRuleInputCategory());
      this._categories.Add(this.Grade = new RaceGradeInputCategory());
      this._categories.Add(this.RaceAge = new RaceAgeInputCategory());
      this._categories.Add(this.Subject = new RaceSubjectInputCategory());
      this._categories.Add(this.Cross = new RaceCrossInputCategory());

      foreach (var category in this._categories)
      {
        category.Query.Subscribe(_ => this.UpdateQuery()).AddTo(this._disposables);
      }
    }

    private void UpdateQuery()
    {
      this.Query.Value = string.Join('|', this._categories
        .Select(c => c.Query.Value)
        .Where(q => !string.IsNullOrEmpty(q)));
    }

    public void Dispose()
    {
      this._disposables.Dispose();
      foreach (var category in this._categories)
      {
        category.Dispose();
      }
    }
  }

  public class FinderQueryInputListItem<T> : IMultipleCheckableItem
  {
    public ReactiveProperty<bool> IsChecked { get; } = new();

    public string Label { get; }

    public T Value { get; }

    public string? GroupName => null;

    public FinderQueryInputListItem(T value) : this(value!.ToString()!, value)
    {
    }

    public FinderQueryInputListItem(string label, T value)
    {
      this.Label = label;
      this.Value = value;
    }

    public FinderQueryInputListItem(Func<T, string> labelGenerator, T value)
    {
      this.Label = labelGenerator(value);
      this.Value = value;
    }
  }

  public class FinderQueryInputListItemCollection<T> : MultipleCheckableCollection<FinderQueryInputListItem<T>>
  {
    public bool IsEmpty() => !this.Any(i => i.IsChecked.Value);

    public bool IsAll() => this.All(i => i.IsChecked.Value);

    public (int MinIndex, int MaxIndex) GetContinuity()
    {
      var min = -1;
      var max = -1;
      var falseCount = 0;
      var isLastTrue = false;
      for (var i = 0; i < this.Count; i++)
      {
        var isChecked = this[i].IsChecked.Value;

        if (isChecked)
        {
          if (min < 0)
          {
            min = i;
          }
          max = i;
          isLastTrue = true;
        }
        else
        {
          if (isLastTrue)
          {
            isLastTrue = false;
            falseCount++;
            if (falseCount > 1)
            {
              return (-1, -1);
            }
          }
        }
      }

      if (isLastTrue && falseCount >= 1)
      {
        return (-1, -1);
      }

      return (min, max);
    }

    public IEnumerable<T> GetCheckedValues()
    {
      return this.Where(i => i.IsChecked.Value).Select(i => i.Value);
    }
  }
}
