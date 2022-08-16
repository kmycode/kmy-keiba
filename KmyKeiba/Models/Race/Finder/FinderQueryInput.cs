﻿using KmyKeiba.Data.Db;
using KmyKeiba.Data.Wrappers;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis.Generic;
using KmyKeiba.Models.Data;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reflection;
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

    public PreviousRaceDaysInputCategory PreviousRaceDays { get; }

    public HorseRaceCountInputCategory RaceCount { get; }

    public HorseRaceCountRunInputCategory RaceCountRun { get; }

    public HorseRaceCountCompleteInputCategory RaceCountComplete { get; }

    public HorseRaceCountRestInputCategory RaceCountRest { get; }

    public HorseNumberInputCategory Number { get; }

    public HorseFrameNumberInputCategory FrameNumber { get; }

    public HorsePopularInputCategory Popular { get; }

    public OddsInputCategory Odds { get; }

    public PlaceOddsMinInputCategory PlaceOddsMin { get; }

    public PlaceOddsMaxInputCategory PlaceOddsMax { get; }

    public ResultTimeInputCategory ResultTime { get; }

    public ResultTimeDiffInputCategory ResultTimeDiff { get; }

    public A3HResultTimeInputCategory A3HResultTime { get; }

    public HorsePlaceInputCategory Place { get; }

    public HorseGoalPlaceInputCategory GoalPlace { get; }

    public AbnormalResultInputCategory AbnormalResult { get; }

    public CornerResultInputCategory Corner1 { get; }
    public CornerResultInputCategory Corner2 { get; }
    public CornerResultInputCategory Corner3 { get; }
    public CornerResultInputCategory Corner4 { get; }

    public RunningStyleInputCategory RunningStyle { get; }

    public HorseBloodInputCategory HorseBlood { get; }

    public RiderNameInputCategory RiderName { get; }

    public RiderBelongsInputCategory RiderBelongs { get; }

    public TrainerNameInputCategory TrainerName { get; }

    public TrainerBelongsInputCategory TrainerBelongs { get; }

    public SameRaceHorseInputCategory SameRaceHorse { get; }

    public BeforeRaceInputCategory BeforeRace { get; }

    public MemoInputCategory Memo { get; }

    public ExternalNumberInputCategory ExternalNumber { get; }

    public GroupByCategoryInput GroupBy { get; }

    public OtherSettingInputCategory OtherSetting { get; }

    public ReactiveCollection<FinderConfigData> Configs => FinderConfigUtil.Configs;

    public ReactiveProperty<FinderConfigData?> SelectedConfig { get; } = new();

    public ReactiveProperty<string> ConfigName { get; } = new(string.Empty);

    public ReactiveProperty<string> Query { get; } = new();

    private readonly bool _hasRace;
    private readonly bool _hasHorse;

    public FinderQueryInput(RaceData? race, RaceHorseData? horse, IReadOnlyList<RaceHorseData>? raceHorses)
    {
      this._hasRace = race != null;
      this._hasHorse = horse != null;

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
      this._categories.Add(this.PreviousRaceDays = new PreviousRaceDaysInputCategory());
      this._categories.Add(this.RaceCount = new HorseRaceCountInputCategory());
      this._categories.Add(this.RaceCountRun = new HorseRaceCountRunInputCategory());
      this._categories.Add(this.RaceCountComplete = new HorseRaceCountCompleteInputCategory());
      this._categories.Add(this.RaceCountRest = new HorseRaceCountRestInputCategory());
      this._categories.Add(this.Number = new HorseNumberInputCategory());
      this._categories.Add(this.FrameNumber = new HorseFrameNumberInputCategory());
      this._categories.Add(this.Popular = new HorsePopularInputCategory());
      this._categories.Add(this.Odds = new OddsInputCategory());
      this._categories.Add(this.AbnormalResult = new AbnormalResultInputCategory());
      this._categories.Add(this.ResultTime = new ResultTimeInputCategory());
      this._categories.Add(this.ResultTimeDiff = new ResultTimeDiffInputCategory());
      this._categories.Add(this.A3HResultTime = new A3HResultTimeInputCategory());
      this._categories.Add(this.PlaceOddsMin = new PlaceOddsMinInputCategory());
      this._categories.Add(this.PlaceOddsMax = new PlaceOddsMaxInputCategory());
      this._categories.Add(this.Corner1 = new CornerResultInputCategory(1));
      this._categories.Add(this.Corner2 = new CornerResultInputCategory(2));
      this._categories.Add(this.Corner3 = new CornerResultInputCategory(3));
      this._categories.Add(this.Corner4 = new CornerResultInputCategory(4));
      this._categories.Add(this.RunningStyle = new RunningStyleInputCategory());
      this._categories.Add(this.HorseBlood = new HorseBloodInputCategory());
      this._categories.Add(this.RiderName = new RiderNameInputCategory());
      this._categories.Add(this.RiderBelongs = new RiderBelongsInputCategory());
      this._categories.Add(this.TrainerName = new TrainerNameInputCategory());
      this._categories.Add(this.TrainerBelongs = new TrainerBelongsInputCategory());
      this._categories.Add(this.SameRaceHorse = new SameRaceHorseInputCategory(race));
      this._categories.Add(this.BeforeRace = new BeforeRaceInputCategory(race));
      this._categories.Add(this.Memo = new MemoInputCategory());
      this._categories.Add(this.ExternalNumber = new ExternalNumberInputCategory());
      this._categories.Add(this.GroupBy = new GroupByCategoryInput());
      this._categories.Add(this.OtherSetting = new OtherSettingInputCategory());

      foreach (var category in this._categories)
      {
        category.Query.Subscribe(_ => this.UpdateQuery()).AddTo(this._disposables);
      }

      this.UpdateConfigs();
    }

    private void UpdateQuery()
    {
      this.Query.Value = string.Join('|', this._categories
        .Select(c => c.Query.Value)
        .Where(q => !string.IsNullOrEmpty(q)));
    }

    public string Serialize(bool isIndent)
    {
      var indent = isIndent ? " " : string.Empty;

      var text = new StringBuilder();
      foreach (var property in this.GetType()
        .GetProperties(BindingFlags.Public | BindingFlags.Instance))
      {
        var category = property.GetValue(this) as IFinderQueryInputCategory;
        if (category == null)
        {
          continue;
        }

        text.Append(indent);
        text.Append(':');
        text.Append(property.Name);
        text.AppendLine();

        var serialized = category.Serialize();
        if (!isIndent)
        {
          text.Append(indent);
          text.Append(serialized);
          text.AppendLine();
        }
        else
        {
          var lines = serialized.Split(Environment.NewLine);
          foreach (var line in lines)
          {
            text.Append(indent);
            text.Append(line);
            text.AppendLine();
          }
        }
      }

      return text.ToString();
    }

    public void Deserialize(string text)
    {
      var lines = text.Split(Environment.NewLine);

      var propertyName = string.Empty;
      var categoryLines = new StringBuilder();
      for (var i = 0; i < lines.Length; i++)
      {
        var line = lines[i];

        if (line.StartsWith(':') || i == lines.Length - 1)
        {
          if (!string.IsNullOrEmpty(propertyName))
          {
            var property = this.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            if (property != null)
            {
              var category = property.GetValue(this) as IFinderQueryInputCategory;
              if (category != null)
              {
                category.Deserialize(categoryLines.ToString());
              }
            }
          }

          if (!string.IsNullOrEmpty(line))
          {
            propertyName = line[1..];
            categoryLines.Clear();
          }
        }
        else
        {
          categoryLines.AppendLine(line);
        }
      }

      if (!this._hasRace)
      {
        if (this.HorseOfCurrentRace.IsAllHorses.Value ||
          this.HorseOfCurrentRace.IsHorseNumber.Value ||
          this.HorseOfCurrentRace.IsHorseBlood.Value ||
          this.HorseOfCurrentRace.IsAllRiders.Value ||
          this.HorseOfCurrentRace.IsAllTrainers.Value)
        {
          this.HorseOfCurrentRace.IsAllHorses.Value =
            this.HorseOfCurrentRace.IsHorseNumber.Value =
            this.HorseOfCurrentRace.IsHorseBlood.Value =
            this.HorseOfCurrentRace.IsAllRiders.Value =
            this.HorseOfCurrentRace.IsAllTrainers.Value = false;
          this.HorseOfCurrentRace.IsUnspecified.Value = true;
        }
        foreach (var category in this._categories)
        {
          if (category is IListBoxInputCategory listbox)
          {
            if (listbox.NumberInput.IsUseCurrentRaceValue.Value)
            {
              listbox.NumberInput.IsUseCurrentRaceValue.Value = false;
              listbox.NumberInput.IsCompareWithFixedValue.Value = true;
            }
          }
          if (category is NumberInputCategoryBase num)
          {
            if (num.Input.IsUseCurrentRaceValue.Value)
            {
              num.Input.IsUseCurrentRaceValue.Value = false;
              num.Input.IsCompareWithFixedValue.Value = true;
            }
          }
        }
      }
      if (!this._hasHorse)
      {
        if (this.HorseOfCurrentRace.IsActiveHorse.Value ||
          this.HorseOfCurrentRace.IsActiveHorseBlood.Value)
        {
          this.HorseOfCurrentRace.IsActiveHorse.Value =
            this.HorseOfCurrentRace.IsActiveHorseBlood.Value = false;
          this.HorseOfCurrentRace.IsUnspecified.Value = true;
        }
        foreach (var category in this._categories)
        {
          if (category is IListBoxInputCategory listbox)
          {
            if (listbox.NumberInput.IsUseCurrentRaceHorseValue.Value)
            {
              listbox.NumberInput.IsUseCurrentRaceHorseValue.Value = false;
              listbox.NumberInput.IsCompareWithFixedValue.Value = true;
            }
          }
          if (category is NumberInputCategoryBase num)
          {
            if (num.Input.IsUseCurrentRaceHorseValue.Value)
            {
              num.Input.IsUseCurrentRaceHorseValue.Value = false;
              num.Input.IsCompareWithFixedValue.Value = true;
            }
          }
        }
      }
    }

    public void UpdateConfigs()
    {
      /*
      this.SelectedConfig.Value = null;
      this.Configs.Clear();
      foreach (var config in FinderConfigUtil.Configs)
      {
        this.Configs.Add(config);
      }
      */
    }

    public async Task AddConfigAsync()
    {
      var config = new FinderConfigData
      {
        Name = this.ConfigName.Value,
        Config = this.Serialize(false),
      };

      try
      {
        using var db = new MyContext();
        await FinderConfigUtil.AddAsync(db, config);
        //this.Configs.Add(config);

        this.SelectedConfig.Value = config;
        this.ConfigName.Value = string.Empty;
      }
      catch (Exception ex)
      {
        // TODO
      }
    }

    public async Task RemoveConfigAsync()
    {
      if (this.SelectedConfig.Value == null)
      {
        return;
      }

      try
      {
        using var db = new MyContext();
        await FinderConfigUtil.RemoveAsync(db, this.SelectedConfig.Value);
        //this.Configs.Remove(this.SelectedConfig.Value);
        this.SelectedConfig.Value = null;
      }
      catch (Exception ex)
      {
        // TODO
      }
    }

    public void LoadConfig()
    {
      if (this.SelectedConfig.Value == null)
      {
        return;
      }

      try
      {
        this.Deserialize(this.SelectedConfig.Value.Config);
      }
      catch (Exception ex)
      {
        // TODO
      }
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
