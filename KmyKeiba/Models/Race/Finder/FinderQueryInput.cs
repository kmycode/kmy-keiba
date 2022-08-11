using KmyKeiba.Data.Db;
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

    public RaceNameInputCategory RaceName { get; }

    public RaceFirstPrizeInputCategory Prize1 { get; }

    public RaceHorsesCountInputCategory HorsesCount { get; }

    public RaceHorsesGoalCountInputCategory ResultHorsesCount { get; }

    public TrackDistanceInputCategory Distance { get; }

    public TrackGroundInputCategory Ground { get; }

    public TrackCornerDirectionInputCategory CornerDirection { get; }

    public TrackOptionInputCategory TrackOption { get; }

    public TrackTypeInputCategory TrackType { get; }

    public TrackConditionInputCategory TrackCondition { get; }

    public TrackWeatherInputCategory Weather { get; }

    public HorseOfCurrentRaceInputCategory HorseOfCurrentRace { get; }

    public HorseNameInputCategory HorseName { get; }

    public HorseAgeInputCategory Age { get; }

    public HorseColorInputCategory Color { get; }

    public HorseSexInputCategory Sex { get; }

    public HorseBelongsInputCategory HorseBelongs { get; }

    public HorseWeightInputCategory Weight { get; }

    public HorseWeightDiffInputCategory WeightDiff { get; }

    public RiderWeightInputCategory RiderWeight { get; }

    public HorseMarkInputCategory Mark { get; }

    public HorseNumberInputCategory Number { get; }

    public HorseFrameNumberInputCategory FrameNumber { get; }

    public HorsePopularInputCategory Popular { get; }

    public OddsInputCategory Odds { get; }

    public PlaceOddsMinInputCategory PlaceOddsMin { get; }

    public PlaceOddsMaxInputCategory PlaceOddsMax { get; }

    public HorsePlaceInputCategory Place { get; }

    public HorseGoalPlaceInputCategory GoalPlace { get; }

    public ReactiveProperty<string> Query { get; } = new();

    public FinderQueryInput(RaceData? race, RaceHorseData? horse, IReadOnlyList<RaceHorseData>? raceHorses)
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
      this._categories.Add(this.RaceName = new RaceNameInputCategory());
      this._categories.Add(this.Prize1 = new RaceFirstPrizeInputCategory());
      this._categories.Add(this.HorsesCount = new RaceHorsesCountInputCategory());
      this._categories.Add(this.ResultHorsesCount = new RaceHorsesGoalCountInputCategory());
      this._categories.Add(this.Distance = new TrackDistanceInputCategory());
      this._categories.Add(this.Ground = new TrackGroundInputCategory());
      this._categories.Add(this.CornerDirection = new TrackCornerDirectionInputCategory());
      this._categories.Add(this.TrackOption = new TrackOptionInputCategory());
      this._categories.Add(this.TrackType = new TrackTypeInputCategory());
      this._categories.Add(this.TrackCondition = new TrackConditionInputCategory());
      this._categories.Add(this.Weather = new TrackWeatherInputCategory());
      this._categories.Add(this.HorseOfCurrentRace = new HorseOfCurrentRaceInputCategory(raceHorses));
      this._categories.Add(this.HorseName = new HorseNameInputCategory());
      this._categories.Add(this.Place = new HorsePlaceInputCategory());
      this._categories.Add(this.GoalPlace = new HorseGoalPlaceInputCategory());
      this._categories.Add(this.Age = new HorseAgeInputCategory());
      this._categories.Add(this.Color = new HorseColorInputCategory());
      this._categories.Add(this.Sex = new HorseSexInputCategory());
      this._categories.Add(this.HorseBelongs = new HorseBelongsInputCategory());
      this._categories.Add(this.Weight = new HorseWeightInputCategory());
      this._categories.Add(this.WeightDiff = new HorseWeightDiffInputCategory());
      this._categories.Add(this.RiderWeight = new RiderWeightInputCategory());
      this._categories.Add(this.Mark = new HorseMarkInputCategory());
      this._categories.Add(this.Number = new HorseNumberInputCategory());
      this._categories.Add(this.FrameNumber = new HorseFrameNumberInputCategory());
      this._categories.Add(this.Popular = new HorsePopularInputCategory());
      this._categories.Add(this.Odds = new OddsInputCategory());
      this._categories.Add(this.PlaceOddsMin = new PlaceOddsMinInputCategory());
      this._categories.Add(this.PlaceOddsMax = new PlaceOddsMaxInputCategory());

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
