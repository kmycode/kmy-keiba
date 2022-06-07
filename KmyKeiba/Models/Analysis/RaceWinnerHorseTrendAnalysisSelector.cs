using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis.Generic;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis
{
  public class RaceWinnerHorseTrendAnalysisSelector : TrendAnalysisSelector<RaceWinnerHorseTrendAnalysisSelector.Key, RaceWinnerHorseTrendAnalyzer>
  {
    public enum Key
    {
      [Label("コース")]
      [ScriptParameterKey("course")]
      SameCourse,

      [Label("地面")]
      [ScriptParameterKey("ground")]
      SameGround,

      [Label("馬場状態")]
      [ScriptParameterKey("condition")]
      SameCondition,

      [Label("天気")]
      [ScriptParameterKey("weather")]
      SameWeather,

      [Label("レース名")]
      [ScriptParameterKey("name")]
      SameRaceName,

      [Label("条件")]
      [ScriptParameterKey("subject")]
      SameSubject,

      [Label("格")]
      [ScriptParameterKey("grade")]
      SameGrade,

      [Label("月")]
      [ScriptParameterKey("month")]
      SameMonth,

      [Label("距離")]
      [ScriptParameterKey("distance")]
      NearDistance,

      [Label("向き")]
      [ScriptParameterKey("direction")]
      SameDirection,

      [Label("複勝")]
      [ScriptParameterKey("placebits")]
      [GroupName("ResultOrder")]
      PlaceBets,

      [Label("着外")]
      [ScriptParameterKey("losed")]
      [GroupName("ResultOrder")]
      Losed,

      [Label("内枠")]
      [ScriptParameterKey("inside")]
      [GroupName("Frame")]
      Inside,

      [Label("外枠")]
      [ScriptParameterKey("outside")]
      [GroupName("Frame")]
      Outside,

      [ScriptParameterKey("sex_male")]
      [IgnoreKey]
      Sex_Male,

      [ScriptParameterKey("sex_female")]
      [IgnoreKey]
      Sex_Female,

      [ScriptParameterKey("sex_castrated")]
      [IgnoreKey]
      Sex_Castrated,

      [ScriptParameterKey("interval_1_15")]
      [IgnoreKey]
      Interval_1_15,

      [ScriptParameterKey("interval_16_30")]
      [IgnoreKey]
      Interval_16_30,

      [ScriptParameterKey("interval_31_60")]
      [IgnoreKey]
      Interval_31_60,

      [ScriptParameterKey("interval_61_90")]
      [IgnoreKey]
      Interval_61_90,

      [ScriptParameterKey("interval_91_150")]
      [IgnoreKey]
      Interval_91_150,

      [ScriptParameterKey("interval_151_300")]
      [IgnoreKey]
      Interval_151_300,

      [ScriptParameterKey("interval_301_")]
      [IgnoreKey]
      Interval_301_,
    }

    private readonly CompositeDisposable _disposables = new();

    public override string Name => this.Subject.DisplayName;

    public RaceData Race { get; }

    public RaceSubjectInfo Subject { get; }

    public ReactiveProperty<bool> IsMale { get; } = new();

    public ReactiveProperty<bool> IsFemale { get; } = new();

    public ReactiveProperty<bool> IsCastrated { get; } = new();

    public ReactiveProperty<bool> IsInterval_1_15 { get; } = new();

    public ReactiveProperty<bool> IsInterval_16_30 { get; } = new();

    public ReactiveProperty<bool> IsInterval_31_60 { get; } = new();

    public ReactiveProperty<bool> IsInterval_61_90 { get; } = new();

    public ReactiveProperty<bool> IsInterval_91_150 { get; } = new();

    public ReactiveProperty<bool> IsInterval_151_300 { get; } = new();

    public ReactiveProperty<bool> IsInterval_301_ { get; } = new();

    public RaceWinnerHorseTrendAnalysisSelector(RaceData race) : base(typeof(Key))
    {
      this.Race = race;
      this.Subject = new RaceSubjectInfo(race);

      this.IsMale.Subscribe(v => this.IgnoreKeys.SetChecked(Key.Sex_Male, v)).AddTo(this._disposables);
      this.IsFemale.Subscribe(v => this.IgnoreKeys.SetChecked(Key.Sex_Female, v)).AddTo(this._disposables);
      this.IsCastrated.Subscribe(v => this.IgnoreKeys.SetChecked(Key.Sex_Castrated, v)).AddTo(this._disposables);

      this.IsInterval_1_15.Subscribe(v => this.IgnoreKeys.SetChecked(Key.Interval_1_15, v)).AddTo(this._disposables);
      this.IsInterval_16_30.Subscribe(v => this.IgnoreKeys.SetChecked(Key.Interval_16_30, v)).AddTo(this._disposables);
      this.IsInterval_31_60.Subscribe(v => this.IgnoreKeys.SetChecked(Key.Interval_31_60, v)).AddTo(this._disposables);
      this.IsInterval_61_90.Subscribe(v => this.IgnoreKeys.SetChecked(Key.Interval_61_90, v)).AddTo(this._disposables);
      this.IsInterval_91_150.Subscribe(v => this.IgnoreKeys.SetChecked(Key.Interval_91_150, v)).AddTo(this._disposables);
      this.IsInterval_151_300.Subscribe(v => this.IgnoreKeys.SetChecked(Key.Interval_151_300, v)).AddTo(this._disposables);
      this.IsInterval_301_.Subscribe(v => this.IgnoreKeys.SetChecked(Key.Interval_301_, v)).AddTo(this._disposables);

      base.OnFinishedInitialization();
    }

    protected override RaceWinnerHorseTrendAnalyzer GenerateAnalyzer()
    {
      return new RaceWinnerHorseTrendAnalyzer(this.Race, new RaceHorseData());
    }

    protected override async Task InitializeAnalyzerAsync(MyContext db, IEnumerable<Key> keys, RaceWinnerHorseTrendAnalyzer analyzer, int count = 500, int offset = 0, bool isLoadSameHorses = false)
    {
      var query = db.RaceHorses!
        .Join(db.Races!.Where(r => r.StartTime < this.Race.StartTime && r.DataStatus != RaceDataStatus.Canceled && r.TrackType == this.Race.TrackType),
          rh => rh.RaceKey, r => r.Key, (rh, r) => new { RaceHorse = rh, Race = r, });

      if (keys.Contains(Key.SameCourse))
      {
        query = query.Where(r => r.Race.Course == this.Race.Course);
      }
      else
      {
        if (this.Race.Course <= RaceCourse.CentralMaxValue)
        {
          query = query.Where(r => r.Race.Course <= RaceCourse.CentralMaxValue);
        }
        else
        {
          query = query.Where(r => r.Race.Course > RaceCourse.CentralMaxValue);
        }
      }

      if (keys.Contains(Key.SameGround))
      {
        query = query.Where(r => r.Race.TrackGround == this.Race.TrackGround);
      }
      if (keys.Contains(Key.NearDistance))
      {
        query = query.Where(r => r.Race.Distance >= this.Race.Distance - 100 && r.Race.Distance <= this.Race.Distance + 100);
      }
      if (keys.Contains(Key.SameDirection))
      {
        query = query.Where(r => r.Race.TrackCornerDirection == this.Race.TrackCornerDirection);
      }
      if (keys.Contains(Key.SameMonth))
      {
        query = query.Where(r => r.Race.StartTime.Month == this.Race.StartTime.Month);
      }
      if (keys.Contains(Key.SameCondition))
      {
        query = query.Where(r => r.Race.TrackCondition == this.Race.TrackCondition);
      }
      if (keys.Contains(Key.SameRaceName) && !string.IsNullOrWhiteSpace(this.Race.Name))
      {
        if (this.Race.GradeId == default)
        {
          query = query.Where(r => r.Race.Name == this.Race.Name);
        }
        else
        {
          query = query.Where(r => r.Race.Name == this.Race.Name || r.Race.GradeId == this.Race.GradeId);
        }
      }
      if (keys.Contains(Key.SameSubject))
      {
        if (this.Race.Course <= RaceCourse.CentralMaxValue)
        {
          query = query.Where(r => r.Race.SubjectName == this.Race.SubjectName &&
                                   r.Race.SubjectAge2 == this.Race.SubjectAge2 &&
                                   r.Race.SubjectAge3 == this.Race.SubjectAge3 &&
                                   r.Race.SubjectAge4 == this.Race.SubjectAge4 &&
                                   r.Race.SubjectAge5 == this.Race.SubjectAge5 &&
                                   r.Race.SubjectAgeYounger == this.Race.SubjectAgeYounger);
        }
        else if (this.Race.Course >= RaceCourse.LocalMinValue && !string.IsNullOrEmpty(this.Race.SubjectDisplayInfo))
        {
          query = query.Where(r => r.Race.SubjectDisplayInfo == this.Race.SubjectDisplayInfo);
        }
      }
      if (keys.Contains(Key.SameGrade))
      {
        query = query.Where(r => r.Race.Grade == this.Race.Grade);
      }
      if (keys.Contains(Key.SameWeather))
      {
        query = query.Where(r => r.Race.TrackWeather == this.Race.TrackWeather);
      }
      if (keys.Contains(Key.PlaceBets))
      {
        query = query.Where(r => r.RaceHorse.ResultOrder >= 1 && r.RaceHorse.ResultOrder <= 3);
      }
      if (keys.Contains(Key.Losed))
      {
        query = query.Where(r => r.RaceHorse.ResultOrder > 5);
      }
      if (keys.Contains(Key.Outside))
      {
        query = query.Where(r => r.RaceHorse.Number >= r.Race.HorsesCount * 2 / 3f);
      }
      if (keys.Contains(Key.Inside))
      {
        query = query.Where(r => r.RaceHorse.Number <= r.Race.HorsesCount / 3f);
      }

      var sexes = new List<HorseSex>();
      if (keys.Contains(Key.Sex_Male))
      {
        sexes.Add(HorseSex.Male);
      }
      if (keys.Contains(Key.Sex_Female))
      {
        sexes.Add(HorseSex.Female);
      }
      if (keys.Contains(Key.Sex_Castrated))
      {
        sexes.Add(HorseSex.Castrated);
      }
      if (sexes.Any())
      {
        query = query.Where(r => sexes.Contains(r.RaceHorse.Sex));
      }

      var intervals = new List<int>();
      if (keys.Contains(Key.Interval_1_15))
      {
        intervals.Add(1);
      }
      if (keys.Contains(Key.Interval_16_30))
      {
        intervals.Add(2);
      }
      if (keys.Contains(Key.Interval_31_60))
      {
        intervals.Add(3);
        intervals.Add(4);
      }
      if (keys.Contains(Key.Interval_61_90))
      {
        intervals.Add(5);
        intervals.Add(6);
      }
      if (keys.Contains(Key.Interval_91_150))
      {
        intervals.Add(7);
        intervals.Add(8);
        intervals.Add(9);
        intervals.Add(10);
      }
      if (keys.Contains(Key.Interval_151_300))
      {
        intervals.Add(11);
        intervals.Add(12);
        intervals.Add(13);
        intervals.Add(14);
        intervals.Add(15);
        intervals.Add(16);
        intervals.Add(17);
        intervals.Add(18);
        intervals.Add(19);
        intervals.Add(20);
      }
      if (keys.Contains(Key.Interval_301_))
      {
        intervals.Add(-1);
      }
      if (intervals.Any())
      {
        if (keys.Contains(Key.Interval_301_))
        {
          query = query.Where(r => intervals.Contains((r.RaceHorse.PreviousRaceDays - 1) / 15 + 1) || r.RaceHorse.PreviousRaceDays >= 301);
        }
        else
        {
          intervals.Remove(-1);
          query = query.Where(r => intervals.Contains((r.RaceHorse.PreviousRaceDays - 1) / 15 + 1));
        }
      }

      var races = await query
        .OrderByDescending(r => r.Race.StartTime)
        .Skip(offset)
        .Take(count)
        .ToArrayAsync();
      var raceKeys = races.Select(r => r.Race.Key).ToArray();
      var raceHorses = Array.Empty<RaceHorseData>();
      if (isLoadSameHorses)
      {
        raceHorses = await db.RaceHorses!
          .Where(rh => rh.ResultOrder >= 1 && rh.ResultOrder <= 5 && raceKeys.Contains(rh.RaceKey))
          .ToArrayAsync();
      }

      var list = new List<RaceHorseAnalyzer>();
      foreach (var race in races)
      {
        list.Add(
          new RaceHorseAnalyzer(
            race.Race,
            race.RaceHorse,
            raceHorses.Where(rh => rh.RaceKey == race.Race.Key).ToArray(),
            await AnalysisUtil.GetRaceStandardTimeAsync(db, race.Race)));
      }
      analyzer.SetSource(list);
    }

    public override void Dispose()
    {
      this._disposables.Dispose();
      base.Dispose();
    }
  }
}
