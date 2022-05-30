﻿using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis.Math;
using KmyKeiba.Models.Connection;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Injection;
using KmyKeiba.Models.Race;
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

namespace KmyKeiba.Models.Analysis
{
  public class RaceHorseAnalyzer : IDisposable
  {
    private static ITimeDeviationValueCalculator? _timeDeviationValueCalculator = InjectionManager.GetInstance<ITimeDeviationValueCalculator>(InjectionManager.TimeDeviationValueCalculator);

    public static RaceHorseAnalyzer Empty { get; } = new(new RaceData(), new RaceHorseData());

    public RaceData Race { get; }

    public RaceHorseData Data { get; }

    public RaceSubjectInfo Subject { get; }

    public RaceHorseBloodTrendAnalysisSelectorMenu? BloodSelectors { get; init; }

    public RaceHorseTrendAnalysisSelector? TrendAnalyzers { get; init; }

    public RaceRiderTrendAnalysisSelector? RiderTrendAnalyzers { get; init; }

    public RaceTrainerTrendAnalysisSelector? TrainerTrendAnalyzers { get; init; }

    public ReactiveProperty<TrainingAnalyzer?> Training { get; } = new();

    public ReactiveProperty<RaceHorseMark> Mark { get; } = new();

    public ReactiveProperty<string> Memo { get; } = new();

    public ReactiveProperty<bool> IsMemoSaving { get; } = new();

    public IReadOnlyList<RaceHorseCornerGrade> CornerGrades { get; } = Array.Empty<RaceHorseCornerGrade>();

    public ValueComparation ResultOrderComparation { get; }

    public bool IsAbnormalResult => this.Data.AbnormalResult != RaceAbnormality.Unknown;

    public double ResultTimePerMeter { get; }

    /// <summary>
    /// 結果からのタイム指数
    /// </summary>
    public double ResultTimeDeviationValue { get; }

    /// <summary>
    /// 後３ハロンタイム指数
    /// </summary>
    public double A3HResultTimeDeviationValue { get; }

    public RaceMovieInfo Movie => this._movie ??= new(this.Race);
    private RaceMovieInfo? _movie;

    public HistoryData? History { get; }

    public CurrentRaceData? CurrentRace { get; }

    public class HistoryData
    {
      public IReadOnlyList<RaceHorseAnalyzer> BeforeRaces { get; } = Array.Empty<RaceHorseAnalyzer>();

      public IReadOnlyList<RaceHorseAnalyzer> Before15Races { get; } = Array.Empty<RaceHorseAnalyzer>();

      public IReadOnlyList<RaceHorseAnalyzer> BeforeFiveRaces { get; } = Array.Empty<RaceHorseAnalyzer>();

      public bool HasHistories => this.BeforeRaces.Any();

      public RunningStyle RunningStyle { get; }

      public ResultOrderGradeMap AllGrade { get; }

      public ResultOrderGradeMap SameCourseGrade { get; }

      public ResultOrderGradeMap SameGroundGrade { get; }

      public ResultOrderGradeMap SameDistanceGrade { get; }

      public ResultOrderGradeMap SameDirectionGrade { get; }

      public ResultOrderGradeMap SameConditionGrade { get; }

      public ResultOrderGradeMap SameRiderGrade { get; }

      public ResultOrderGradeMap SprinterGrade { get; }

      public ResultOrderGradeMap MylarGrade { get; }

      public ResultOrderGradeMap ClassicDistanceGrade { get; }

      public ResultOrderGradeMap SteyerGrade { get; }

      public ReactiveCollection<CourseHorseGrade> CourseGrades { get; } = new();

      public bool HasCourseGrades => this.CourseGrades.Count > 1;

      /// <summary>
      /// 後３ハロンタイム指数
      /// </summary>
      public double A3HTimeDeviationValue { get; }

      /// <summary>
      /// タイム指数
      /// </summary>
      public double TimeDeviationValue { get; }

      public ValueComparation TimeDVComparation { get; set; }

      public ValueComparation A3HTimeDVComparation { get; set; }

      /// <summary>
      /// 距離適性
      /// </summary>
      public DistanceAptitude BestDistance { get; }

      /// <summary>
      /// ２番目の距離適性
      /// </summary>
      public DistanceAptitude SecondDistance { get; }

      public HistoryData(RaceData race, RaceHorseData horse, IEnumerable<RaceHorseAnalyzer> raceHistory)
      {
        this.BeforeRaces = raceHistory.OrderByDescending(h => h.Race.StartTime).ToArray();
        this.BeforeFiveRaces = this.BeforeRaces.Take(5).ToArray();
        this.Before15Races = this.BeforeRaces.Take(15).ToArray();

        if (this.BeforeRaces.Any())
        {
          var targetRaces = this.BeforeRaces.Take(10).ToArray();

          var startTime = new DateTime(1980, 1, 1);
          var statistic = new StatisticSingleArray(targetRaces.Select(r => r.ResultTimeDeviationValue).ToArray());
          var statistica3h = new StatisticSingleArray(targetRaces.Select(r => r.A3HResultTimeDeviationValue).ToArray());
          var datePoint = new StatisticSingleArray(targetRaces.Select(r => (r.Race.StartTime.Date - startTime).TotalDays).ToArray());
          var st = new StatisticDoubleArray(datePoint, statistic);

          //this.TimeDeviationValue = st.CalcRegressionValue((race.StartTime.Date - startTime).TotalDays);
          this.TimeDeviationValue = statistic.Median;
          this.A3HTimeDeviationValue = statistica3h.Median;

          this.RunningStyle = targetRaces
            .OrderBy(r => r.Data.ResultOrder)
            .GroupBy(r => r.Data.RunningStyle)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

          // 成績
          this.AllGrade = new ResultOrderGradeMap(this.BeforeRaces.Select(r => r.Data).ToArray());
          this.SameCourseGrade = new ResultOrderGradeMap(this.BeforeRaces
            .Where(r => r.Race.Course == race.Course).Select(r => r.Data).ToArray());
          this.SameGroundGrade = new ResultOrderGradeMap(this.BeforeRaces
            .Where(r => r.Race.TrackGround == race.TrackGround).Select(r => r.Data).ToArray());
          this.SameDistanceGrade = new ResultOrderGradeMap(this.BeforeRaces
            .Where(r => r.Race.Distance / 100 == race.Distance / 100).Select(r => r.Data).ToArray());
          this.SameDirectionGrade = new ResultOrderGradeMap(this.BeforeRaces
            .Where(r => r.Race.TrackCornerDirection == race.TrackCornerDirection).Select(r => r.Data).ToArray());
          this.SameConditionGrade = new ResultOrderGradeMap(this.BeforeRaces
            .Where(r => r.Race.TrackCondition == race.TrackCondition).Select(r => r.Data).ToArray());
          this.SameRiderGrade = new ResultOrderGradeMap(this.BeforeRaces
            .Where(r => r.Data.RiderCode == horse.RiderCode).Select(r => r.Data).ToArray());

          // 距離適性
          this.SprinterGrade = new ResultOrderGradeMap(this.BeforeRaces
            .Where(r => r.Race.Distance < 1400).Select(r => r.Data).ToArray());
          this.MylarGrade = new ResultOrderGradeMap(this.BeforeRaces
            .Where(r => r.Race.Distance >= 1400 && r.Race.Distance < 1800).Select(r => r.Data).ToArray());
          this.ClassicDistanceGrade = new ResultOrderGradeMap(this.BeforeRaces
            .Where(r => r.Race.Distance >= 1800 && r.Race.Distance < 2400).Select(r => r.Data).ToArray());
          this.SteyerGrade = new ResultOrderGradeMap(this.BeforeRaces
            .Where(r => r.Race.Distance >= 2400).Select(r => r.Data).ToArray());
          var distances = new Dictionary<DistanceAptitude, ResultOrderGradeMap>
          {
            { DistanceAptitude.Sprinter, this.SprinterGrade },
            { DistanceAptitude.Mylar, this.MylarGrade },
            { DistanceAptitude.ClassicDistance, this.ClassicDistanceGrade },
            { DistanceAptitude.Steyer, this.SteyerGrade },
          };
          var ranking = distances.OrderByDescending(d => d.Value.PlacingBetsRate).ToArray();
          this.BestDistance = distances.All(d => d.Value.LoseCount == d.Value.AllCount) ? DistanceAptitude.Unknown :
            ranking.ElementAt(0).Key;
          this.SecondDistance = distances.Count(d => d.Value.LoseCount == d.Value.AllCount) >= 3 ? DistanceAptitude.Unknown :
            ranking.ElementAt(1).Key;

          // 競馬場ごとの成績
          this.CourseGrades.Add(new CourseHorseGrade(race.Distance, this.BeforeRaces));
          foreach (var courseSource in this.BeforeRaces
            .Where(r => r.Data.AbnormalResult == RaceAbnormality.Unknown && r.Data.ResultTime != default)
            .GroupBy(r => r.Race.Course))
          {
            var grade = new CourseHorseGrade(courseSource.Key, race.Distance, courseSource.ToArray())
            {
              IsCurrentCourse = courseSource.Key == race.Course,
            };
            this.CourseGrades.Add(grade);
          }
        }
      }
    }

    public class CurrentRaceData
    {
      /// <summary>
      /// 斤量の相対評価
      /// </summary>
      public ValueComparation RiderWeightComparation { get; }


      public ValueComparation AgeComparation { get; }

      public RaceHorseData? TopHorse { get; }

      public IReadOnlyList<RaceHorseData> TopHorses { get; }

      public double RoughRate { get; }

      /// <summary>
      /// １着のタイム指数。レースのペースを決めるのに使う
      /// </summary>
      public double TopDeviationValue { get; }

      public CurrentRaceData(RaceData race, RaceHorseData horse, IEnumerable<RaceHorseData> sameRaceHorses, RaceStandardTimeMasterData? raceStandardTime)
      {
        var weightPoint = new StatisticSingleArray(sameRaceHorses.Select(h => (double)h.RiderWeight).ToArray());
        var median = (short)weightPoint.Median;  // 小数点以下切り捨て。中央値が常に整数とは限らない（全体の数が偶数の時））
        this.RiderWeightComparation = horse.RiderWeight > median ? ValueComparation.Bad :
          horse.RiderWeight < median ? ValueComparation.Good : ValueComparation.Standard;

        var agePoint = new StatisticSingleArray(sameRaceHorses.Select(h => (double)h.Age).ToArray());
        var ageMedian = (short)agePoint.Median;
        this.AgeComparation = horse.Age > ageMedian ? ValueComparation.Bad :
          horse.Age < ageMedian ? ValueComparation.Good : ValueComparation.Standard;

        this.TopHorse = sameRaceHorses.FirstOrDefault(h => h.ResultOrder == 1);
        this.TopHorses = sameRaceHorses.OrderBy(h => h.ResultOrder).ToArray();
        this.RoughRate = AnalysisUtil.CalcRoughRate(sameRaceHorses.ToArray());

        if (raceStandardTime != null && raceStandardTime.SampleCount > 0 && _timeDeviationValueCalculator != null && this.TopHorse != null)
        {
          this.TopDeviationValue = _timeDeviationValueCalculator.GetTimeDeviationValue(race, this.TopHorse, raceStandardTime);
        }
      }
    }

    private RaceHorseAnalyzer(RaceData race, RaceHorseData horse)
    {
      this.Race = race;
      this.Data = horse;
      this.Subject = new RaceSubjectInfo(race);
      this.Mark.Value = horse.Mark;
      this.Memo.Value = horse.Memo ?? string.Empty;
      this.ResultTimePerMeter = (double)horse.ResultTime.TotalSeconds / race.Distance;
      this.ResultOrderComparation = horse.ResultOrder >= 1 && horse.ResultOrder <= 3 ? ValueComparation.Good :
        horse.ResultOrder >= 8 || horse.ResultOrder >= race.HorsesCount * 0.7f ? ValueComparation.Bad : ValueComparation.Standard;

      this.Memo.Skip(1).Subscribe(async m =>
      {
        if (this.IsMemoSaving.Value)
        {
          return;
        }

        this.IsMemoSaving.Value = true;

        using var db = new MyContext();
        db.RaceHorses!.Attach(this.Data);
        this.Data.Memo = m;

        await db.SaveChangesAsync();

        this.IsMemoSaving.Value = false;
      }).AddTo(this._disposables);

      // コーナーの成績
      static CornerGradeType GetCornerGradeType(short order, short beforeOrder)
        => order > beforeOrder ? CornerGradeType.Bad : order < beforeOrder ? CornerGradeType.Good : CornerGradeType.Standard;
      var corners = new List<RaceHorseCornerGrade>();
      if (horse.FirstCornerOrder > 0)
      {
        corners.Add(new RaceHorseCornerGrade
        {
          Order = horse.FirstCornerOrder,
          Type = CornerGradeType.Standard,
        });
      }
      if (horse.SecondCornerOrder > 0)
      {
        corners.Add(new RaceHorseCornerGrade
        {
          Order = horse.SecondCornerOrder,
          Type = GetCornerGradeType(horse.SecondCornerOrder, horse.FirstCornerOrder),
        });
      }
      if (horse.ThirdCornerOrder > 0)
      {
        corners.Add(new RaceHorseCornerGrade
        {
          Order = horse.ThirdCornerOrder,
          Type = GetCornerGradeType(horse.ThirdCornerOrder, horse.SecondCornerOrder),
        });
      }
      if (horse.FourthCornerOrder > 0)
      {
        corners.Add(new RaceHorseCornerGrade
        {
          Order = horse.FourthCornerOrder,
          Type = GetCornerGradeType(horse.FourthCornerOrder, horse.ThirdCornerOrder),
        });
      }
      if (horse.ResultOrder > 0)
      {
        corners.Add(new RaceHorseCornerGrade
        {
          Order = horse.ResultOrder,
          Type = GetCornerGradeType(horse.ResultOrder, horse.FourthCornerOrder),
          IsResult = true,
        });
      }
      this.CornerGrades = corners;
    }

    public RaceHorseAnalyzer(RaceData race, RaceHorseData horse, RaceStandardTimeMasterData? raceStandardTime)
      : this(race, horse)
    {
      if (raceStandardTime != null && raceStandardTime.SampleCount > 0 && _timeDeviationValueCalculator != null)
      {
        this.ResultTimeDeviationValue = _timeDeviationValueCalculator.GetTimeDeviationValue(race, horse, raceStandardTime);
        this.A3HResultTimeDeviationValue = _timeDeviationValueCalculator.GetA3HTimeDeviationValue(race, horse, raceStandardTime);
      }
    }

    public RaceHorseAnalyzer(RaceData race, RaceHorseData horse, IEnumerable<RaceHorseData> sameRaceHorses, RaceStandardTimeMasterData? raceStandardTime)
      : this(race, horse, raceStandardTime)
    {
      this.CurrentRace = new CurrentRaceData(race, horse, sameRaceHorses, raceStandardTime);
    }

    public RaceHorseAnalyzer(RaceData race, RaceHorseData horse, IEnumerable<RaceHorseData> sameRaceHorses, IEnumerable<RaceHorseAnalyzer> raceHistory, RaceStandardTimeMasterData? raceStandardTime)
      : this(race, horse, sameRaceHorses, raceStandardTime)
    {
      this.History = new HistoryData(race, horse, raceHistory);
    }

    public ICommand SetMarkCommand =>
      this._setDoubleCircleMarkCommand ??=
        new AsyncReactiveCommand<string>(Connection.DownloaderModel.Instance.CanSaveOthers).WithSubscribe(p => this.ChangeHorseMarkAsync(p));
    private AsyncReactiveCommand<string>? _setDoubleCircleMarkCommand;

    public ICommand PlayRaceMovieCommand =>
      this._playRaceMovieCommand ??=
        new AsyncReactiveCommand<object>(this.Movie.IsRaceError.Select(e => !e)).WithSubscribe(async _ => await this.Movie.PlayRaceAsync());
    private AsyncReactiveCommand<object>? _playRaceMovieCommand;

    public ICommand PlayPaddockCommand =>
      this._playPaddockCommand ??=
        new AsyncReactiveCommand<object>(this.Movie.IsPaddockError.Select(e => !e)).WithSubscribe(async _ => await this.Movie.PlayPaddockAsync());
    private AsyncReactiveCommand<object>? _playPaddockCommand;

    public ICommand PlayPatrolCommand =>
      this._playPatrolCommand ??=
        new AsyncReactiveCommand<object>(this.Movie.IsPatrolError.Select(e => !e)).WithSubscribe(async _ => await this.Movie.PlayPatrolAsync());
    private AsyncReactiveCommand<object>? _playPatrolCommand;

    public ICommand PlayMultiCamerasCommand =>
      this._playMultiCamerasCommand ??=
        new AsyncReactiveCommand<object>(this.Movie.IsMultiCamerasError.Select(e => !e)).WithSubscribe(async _ => await this.Movie.PlayMultiCamerasAsync());
    private AsyncReactiveCommand<object>? _playMultiCamerasCommand;

    private readonly CompositeDisposable _disposables = new();

    private async Task ChangeHorseMarkAsync(string marks)
    {
      short.TryParse(marks, out var markss);
      var mark = (RaceHorseMark)markss;

      using var db = new MyContext();
      this.ChangeHorseMark(db, mark);
      await db.SaveChangesAsync();
    }

    public void ChangeHorseMark(MyContext db, RaceHorseMark mark)
    {
      db.RaceHorses!.Attach(this.Data);
      this.Mark.Value = this.Data.Mark = mark;
    }

    public void Dispose()
    {
      this._disposables.Dispose();
      this.BloodSelectors?.Dispose();
      this.RiderTrendAnalyzers?.Dispose();
      this.TrainerTrendAnalyzers?.Dispose();
      this.TrendAnalyzers?.Dispose();
    }
  }

  public struct RaceHorseCornerGrade
  {
    public CornerGradeType Type { get; init; }

    public bool IsResult { get; init; }

    public short Order { get; init; }
  }

  public enum CornerGradeType
  {
    Standard,
    Good,
    Bad,
  }

  // 数字が少ないほどよい、という場合もあるのでHigh、Lowにはしない
  public enum ValueComparation
  {
    Standard,
    Good,
    Bad,
  }

  public enum DistanceAptitude
  {
    [Label("不明")]
    Unknown,

    [Label("短距離")]
    Sprinter,

    [Label("マイル")]
    Mylar,

    [Label("中距離")]
    ClassicDistance,

    [Label("長距離")]
    Steyer,
  }

  public struct ResultOrderGradeMap
  {
    public int FirstCount { get; } = 0;

    public int SecondCount { get; } = 0;

    public int ThirdCount { get; } = 0;

    public int LoseCount { get; } = 0;

    public int AllCount => this.FirstCount + this.SecondCount + this.ThirdCount + this.LoseCount;

    public bool IsZero => this.AllCount == 0;

    /// <summary>
    /// 複勝勝率
    /// </summary>
    public float PlacingBetsRate { get; } = 0;

    public float WinRate { get; } = 0;

    public ValueComparation PlacingBetsRateComparation => this.PlacingBetsRate >= 0.75 ? ValueComparation.Good : this.PlacingBetsRate <= 0.2 ? ValueComparation.Bad : ValueComparation.Standard;

    public ResultOrderGradeMap(IReadOnlyList<RaceHorseData> source)
    {
      var targets = source.Where(s => s.AbnormalResult == RaceAbnormality.Unknown).ToArray();
      if (targets.Any())
      {
        this.FirstCount = targets.Count(f => f.ResultOrder == 1);
        this.SecondCount = targets.Count(f => f.ResultOrder == 2);
        this.ThirdCount = targets.Count(f => f.ResultOrder == 3);
        this.LoseCount = targets.Count(f => f.ResultOrder > 3);

        this.PlacingBetsRate = (this.FirstCount + this.SecondCount + this.ThirdCount) / (float)targets.Length;
        this.WinRate = this.FirstCount / (float)targets.Length;
      }
    }
  }

  public class CourseHorseGrade
  {
    public bool HasData { get; }

    public bool IsCurrentCourse { get; init; }

    public RaceCourse Course { get; }

    public ResultOrderGradeMap AllGrade { get; }

    public ResultOrderGradeMap Grade { get; }

    public TimeSpan ShortestTime { get; }

    public RaceData? ShortestTimeRace { get; }

    public RaceHorseData? ShortestTimeRaceHorse { get; }

    public RaceSubjectInfo? ShortestTimeRaceSubject { get; }

    public double ShortestTimeRaceHorseDeviationValue { get; }

    public double ShortestTimeRaceHorseA3HDeviationValue { get; }

    public double ShortestTimeRaceTopHorseDeviationValue { get; }

    public CourseHorseGrade(RaceCourse course, short distance, IReadOnlyList<RaceHorseAnalyzer> source)
    {
      var filtered = source.Where(s => s.Data.AbnormalResult == RaceAbnormality.Unknown && s.Data.ResultOrder > 0);

      this.Course = course;
      this.AllGrade = new ResultOrderGradeMap(filtered.Select(s => s.Data).ToArray());

      var nearDistanceRaces = filtered
        .Where(s => System.Math.Abs(s.Race.Distance - distance) <= 100);
      if (nearDistanceRaces.Any())
      {
        this.HasData = true;
        this.Grade = new ResultOrderGradeMap(nearDistanceRaces.Select(s => s.Data).ToArray());

        var shortestTime = nearDistanceRaces
          .Select(s => new { Data = s, ComparationValue = s.Data.ResultTime.TotalSeconds / s.Race.Distance, })
          .OrderBy(s => s.ComparationValue)
          .First().Data;
        this.ShortestTime = shortestTime.Data.ResultTime;
        this.ShortestTimeRace = shortestTime.Race;
        this.ShortestTimeRaceHorse = shortestTime.Data;
        this.ShortestTimeRaceSubject = shortestTime.Subject;
        this.ShortestTimeRaceHorseDeviationValue = shortestTime.ResultTimeDeviationValue;
        this.ShortestTimeRaceHorseA3HDeviationValue = shortestTime.A3HResultTimeDeviationValue;
        this.ShortestTimeRaceTopHorseDeviationValue = shortestTime.CurrentRace?.TopDeviationValue ?? default;
      }
    }

    public CourseHorseGrade(short distance, IReadOnlyList<RaceHorseAnalyzer> source) : this(RaceCourse.All, distance, source)
    {
    }
  }
}
