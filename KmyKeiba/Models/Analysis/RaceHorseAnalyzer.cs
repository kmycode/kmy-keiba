using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis.Math;
using KmyKeiba.Models.Connection;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Injection;
using KmyKeiba.Models.Race;
using KmyKeiba.Shared;
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

    public ReactiveProperty<bool> CanSave => DownloaderModel.Instance.CanSaveOthers;

    public float RiderPlaceBitsRate { get; } = new();

    public ValueComparation RiderPlaceBitsRateComparation { get; set; }

    public short RiderFirstCount { get; } = new();

    public short RiderSecondCount { get; } = new();

    public short RiderThirdCount { get; } = new();

    public short RiderLosedCount { get; } = new();

    public short RiderAllCount { get; } = new();

    public IReadOnlyList<RaceHorseCornerGrade> CornerGrades { get; } = Array.Empty<RaceHorseCornerGrade>();

    public ValueComparation ResultOrderComparation { get; }

    public CornerGradeType ResultOrderComparationWithLastCorner { get; }

    public bool IsAbnormalResult => this.Data.AbnormalResult != RaceAbnormality.Unknown;

    public TimeSpan UntilA3HResultTime { get; }

    public ValueComparation ResultTimeDVComparation { get; set; }

    public ValueComparation ResultA3HTimeDVComparation { get; set; }

    public double ResultTimePerMeter { get; }

    /// <summary>
    /// 結果からのタイム指数
    /// </summary>
    public double ResultTimeDeviationValue { get; }

    /// <summary>
    /// 後３より前のハロンタイム指数
    /// </summary>
    public double UntilA3HResultTimeDeviationValue { get; }

    /// <summary>
    /// 後３ハロンタイム指数
    /// </summary>
    public double A3HResultTimeDeviationValue { get; }

    public ReactiveProperty<bool> IsActive { get; } = new();

    public ReactiveCollection<OddsTimelineItem> OddsTimeline { get; } = new();

    public ReactiveCollection<OddsTimelineItem> OddsTimelineLatestItems { get; } = new();

    public bool IsOddsTimelineAvailable => this.OddsTimelineLatestItems.Any();

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
      /// 後３ハロンまでのタイム指数
      /// </summary>
      public double UntilA3HTimeDeviationValue { get; }

      /// <summary>
      /// タイム指数
      /// </summary>
      public double TimeDeviationValue { get; }

      /// <summary>
      /// 乱調度
      /// </summary>
      public double DisturbanceRate { get; }

      public ValueComparation TimeDVComparation { get; set; }

      public ValueComparation A3HTimeDVComparation { get; set; }

      public ValueComparation UntilA3HTimeDVComparation { get; set; }

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

        // 着順で比較すると、JV-Linkしか使ってない状態で地方競馬のデータも混ざってしまう
        // （JVLinkにおける地方競馬のデータは、時間など大半のデータがゼロになっている）
        if (this.BeforeRaces.Any(r => r.Data.ResultTime.TotalSeconds > 0))
        {
          var targetRaces = this.BeforeRaces.Where(r => r.Data.ResultTime.TotalSeconds > 0).Take(10);

          var startTime = new DateTime(1980, 1, 1);
          var statistic = new StatisticSingleArray(targetRaces.Select(r => r.ResultTimeDeviationValue).Where(r => r != default).ToArray());
          var statistica3h = new StatisticSingleArray(targetRaces.Select(r => r.A3HResultTimeDeviationValue).Where(r => r != default).ToArray());
          var statisticau3h = new StatisticSingleArray(targetRaces.Select(r => r.UntilA3HResultTimeDeviationValue).Where(r => r != default).ToArray());

          //this.TimeDeviationValue = st.CalcRegressionValue((race.StartTime.Date - startTime).TotalDays);
          this.TimeDeviationValue = statistic.Median;
          this.A3HTimeDeviationValue = statistica3h.Median;
          this.UntilA3HTimeDeviationValue = statisticau3h.Median;
          this.DisturbanceRate = AnalysisUtil.CalcDisturbanceRate(targetRaces);

          this.RunningStyle = targetRaces
            .OrderBy(r => r.Data.ResultOrder)
            .GroupBy(r => r.Data.RunningStyle)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

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
          this.TopDeviationValue = _timeDeviationValueCalculator.GetTimeDeviationValueAsync(race, this.TopHorse, raceStandardTime).Result;
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
      if (race.Distance >= 800)
      {
        this.UntilA3HResultTime = horse.ResultTime - horse.AfterThirdHalongTime;
      }

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
        => beforeOrder == 0 ? CornerGradeType.Standard :
           order > beforeOrder ? CornerGradeType.Bad :
           order < beforeOrder ? CornerGradeType.Good :
           CornerGradeType.Standard;
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
      this.ResultOrderComparationWithLastCorner = corners.LastOrDefault().Type;
    }

    public RaceHorseAnalyzer(RaceData race, RaceHorseData horse, RaceStandardTimeMasterData? raceStandardTime)
      : this(race, horse)
    {
      if (raceStandardTime != null && raceStandardTime.SampleCount > 0 && _timeDeviationValueCalculator != null)
      {
        this.ResultTimeDeviationValue = _timeDeviationValueCalculator.GetTimeDeviationValueAsync(race, horse, raceStandardTime).Result;
        this.A3HResultTimeDeviationValue = _timeDeviationValueCalculator.GetA3HTimeDeviationValueAsync(race, horse, raceStandardTime).Result;
        this.UntilA3HResultTimeDeviationValue = _timeDeviationValueCalculator.GetUntilA3HTimeDeviationValueAsync(race, horse, raceStandardTime).Result;
      }
    }

    public RaceHorseAnalyzer(RaceData race, RaceHorseData horse, IEnumerable<RaceHorseData> sameRaceHorses, RaceStandardTimeMasterData? raceStandardTime)
      : this(race, horse, raceStandardTime)
    {
      this.CurrentRace = new CurrentRaceData(race, horse, sameRaceHorses, raceStandardTime);
    }

    public RaceHorseAnalyzer(RaceData race, RaceHorseData horse, IEnumerable<RaceHorseData> sameRaceHorses, IEnumerable<RaceHorseAnalyzer> raceHistory, RaceStandardTimeMasterData? raceStandardTime, RiderWinRateMasterData riderWinRate)
      : this(race, horse, sameRaceHorses, raceStandardTime)
    {
      this.History = new HistoryData(race, horse, raceHistory);

      short allCount, firstCount, secondCount, thirdCount;
      if (race.TrackType == TrackType.Flat)
      {
        if (race.TrackGround == TrackGround.Turf)
        {
          allCount = riderWinRate.AllTurfCount;
          firstCount = riderWinRate.FirstTurfCount;
          secondCount = riderWinRate.SecondTurfCount;
          thirdCount = riderWinRate.ThirdTurfCount;
        }
        else
        {
          allCount = riderWinRate.AllDirtCount;
          firstCount = riderWinRate.FirstDirtCount;
          secondCount = riderWinRate.SecondDirtCount;
          thirdCount = riderWinRate.ThirdDirtCount;
        }
      }
      else
      {
        if (race.TrackGround == TrackGround.Turf)
        {
          allCount = riderWinRate.AllTurfSteepsCount;
          firstCount = riderWinRate.FirstTurfSteepsCount;
          secondCount = riderWinRate.SecondTurfSteepsCount;
          thirdCount = riderWinRate.ThirdTurfSteepsCount;
        }
        else
        {
          allCount = riderWinRate.AllDirtSteepsCount;
          firstCount = riderWinRate.FirstDirtSteepsCount;
          secondCount = riderWinRate.SecondDirtSteepsCount;
          thirdCount = riderWinRate.ThirdDirtSteepsCount;
        }
      }

      if (allCount > 0)
      {
        this.RiderPlaceBitsRate = (firstCount + secondCount + thirdCount) / (float)allCount;
        this.RiderFirstCount = firstCount;
        this.RiderSecondCount = secondCount;
        this.RiderThirdCount = thirdCount;
        this.RiderLosedCount = (short)(allCount - firstCount - secondCount - thirdCount);
        this.RiderAllCount = allCount;
      }
    }

    #region Command

    public ICommand SetMarkCommand =>
      this._setDoubleCircleMarkCommand ??=
        new AsyncReactiveCommand<string>(Connection.DownloaderModel.Instance.CanSaveOthers).WithSubscribe(p => this.ChangeHorseMarkAsync(p));
    private AsyncReactiveCommand<string>? _setDoubleCircleMarkCommand;

    public ICommand PlayRaceMovieCommand =>
      this._playRaceMovieCommand ??=
        new AsyncReactiveCommand<object>(this.Movie.IsRaceError.Select(e => !e).CombineLatest(DownloaderModel.Instance.CanSaveOthers, (a, b) => a && b)).WithSubscribe(async _ => await this.Movie.PlayRaceAsync());
    private AsyncReactiveCommand<object>? _playRaceMovieCommand;

    public ICommand PlayPaddockCommand =>
      this._playPaddockCommand ??=
        new AsyncReactiveCommand<object>(this.Movie.IsPaddockError.Select(e => !e).CombineLatest(DownloaderModel.Instance.CanSaveOthers, (a, b) => a && b)).WithSubscribe(async _ => await this.Movie.PlayPaddockAsync());
    private AsyncReactiveCommand<object>? _playPaddockCommand;

    public ICommand PlayPatrolCommand =>
      this._playPatrolCommand ??=
        new AsyncReactiveCommand<object>(this.Movie.IsPatrolError.Select(e => !e).CombineLatest(DownloaderModel.Instance.CanSaveOthers, (a, b) => a && b)).WithSubscribe(async _ => await this.Movie.PlayPatrolAsync());
    private AsyncReactiveCommand<object>? _playPatrolCommand;

    public ICommand PlayMultiCamerasCommand =>
      this._playMultiCamerasCommand ??=
        new AsyncReactiveCommand<object>(this.Movie.IsMultiCamerasError.Select(e => !e).CombineLatest(DownloaderModel.Instance.CanSaveOthers, (a, b) => a && b)).WithSubscribe(async _ => await this.Movie.PlayMultiCamerasAsync());
    private AsyncReactiveCommand<object>? _playMultiCamerasCommand;

    public ICommand OpenRaceWindowCommand =>
      this._openRaceWindowCommand ??=
        new ReactiveCommand<string>().WithSubscribe(key => OpenRaceRequest.Default.Request(key, this.Data.Key));
    private ReactiveCommand<string>? _openRaceWindowCommand;

    #endregion

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

    public void SetOddsTimeline(IEnumerable<SingleOddsTimeline> timeline)
    {
      foreach (var data in timeline.OrderBy(t => t.Time))
      {
        var odds = data.GetSingleOdds();
        var item = new OddsTimelineItem(this.Race, data, odds.ElementAtOrDefault(this.Data.Number - 1));
        this.OddsTimeline.Add(item);

        if (data.Time >= this.Race.StartTime.AddMinutes(-60))
        {
          this.OddsTimelineLatestItems.Add(item);
        }
      }

      if (this.OddsTimelineLatestItems.Skip(3).Any())
      {
        var oddsMax = this.OddsTimelineLatestItems.OrderByDescending(o => o.Odds).ElementAtOrDefault(1)?.Odds ?? default;
        var oddsMin = this.OddsTimelineLatestItems.OrderBy(o => o.Odds).ElementAtOrDefault(1)?.Odds ?? default;
        foreach (var item in this.OddsTimelineLatestItems)
        {
          item.SingleOddsComparation = AnalysisUtil.CompareValue(item.Odds, oddsMax, oddsMin, true);
        }
      }
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
    Unknown,
    Standard,
    Good,
    Bad,
    Warning,
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

    public TimeSpan ShortestTimeNormalized { get; }

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
        this.ShortestTimeNormalized = shortestTime.Data.ResultTime / shortestTime.Race.Distance * distance;
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

  public class OddsTimelineItem
  {
    public TimeSpan LeftTime { get; }

    public DateTime Time { get; }

    public short Odds { get; }

    public ValueComparation SingleOddsComparation { get; set; }

    public OddsTimelineItem(RaceData race, SingleOddsTimeline item, short singleOdds)
    {
      this.Odds = singleOdds;
      this.LeftTime = race.StartTime - item.Time;
      this.Time = item.Time;
    }
  }
}
