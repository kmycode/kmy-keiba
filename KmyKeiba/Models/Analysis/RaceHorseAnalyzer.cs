﻿using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis.Math;
using KmyKeiba.Models.Connection;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Injection;
using KmyKeiba.Models.Race;
using KmyKeiba.Models.Race.Finder;
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
  public class RaceHorseAnalyzer : IDisposable, IHorseMarkSetter
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    private static ITimeDeviationValueCalculator? _timeDeviationValueCalculator = InjectionManager.GetInstance<ITimeDeviationValueCalculator>(InjectionManager.TimeDeviationValueCalculator);
    private bool _isChecking;

    private readonly CompositeDisposable _disposables = new();

    public static RaceHorseAnalyzer Empty { get; } = new(new RaceData(), new RaceHorseData());

    public RaceData Race { get; }

    public RaceHorseData Data { get; }

    public RaceSubjectInfo Subject { get; }

    public HorseData? DetailData { get; init; }

    public HorseSaleData? SaleData { get; init; }

    public JrdbRaceHorseData? JrdbData { get; }

    public RaceHorseExtraData? ExtraData { get; set; }

    public bool IsCheckedExtraData { get; set; }

    public ReactiveProperty<FinderModel?> FinderModel { get; } = new();

    public RaceHorseBloodModel? BloodSelectors { get; init; }

    public ReactiveProperty<TrainingAnalyzer?> Training { get; } = new();

    public ReactiveProperty<Race.Memo.RaceHorseMemoItem?> MemoEx { get; } = new();

    public ReactiveProperty<RaceHorseMark> Mark { get; } = new();

    public ReactiveProperty<RaceHorseMark> AnalysisTableMark { get; } = new();

    public ReactiveProperty<bool> IsChecked { get; } = new();

    public ReactiveProperty<string> Memo { get; } = new();

    public ReactiveProperty<bool> IsMemoSaving { get; } = new();

    public ReactiveProperty<bool> CanSave => DownloaderModel.Instance.CanSaveOthers;

    public IReadOnlyList<RaceHorseCornerGrade> CornerGrades { get; } = Array.Empty<RaceHorseCornerGrade>();

    public ValueComparation ResultOrderComparation { get; }

    public CornerGradeType ResultOrderComparationWithLastCorner { get; }

    public bool IsAbnormalResult => this.Data.AbnormalResult != RaceAbnormality.Unknown;

    public bool IsRaceCanceled => this.Race.DataStatus == RaceDataStatus.Canceled;

    public TimeSpan UntilA3HResultTime { get; }

    public ValueComparation ResultA3HTimeComparation { get; set; }

    public ValueComparation PciDVComparation { get; set; }

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

    public double Pci { get; }

    public ReactiveProperty<bool> IsActive { get; } = new();

    public ReactiveCollection<OddsTimelineItem> OddsTimeline { get; } = new();

    public ReactiveCollection<OddsTimelineItem> OddsTimelineLatestItems { get; } = new();

    public bool IsOddsTimelineAvailable => this.OddsTimelineLatestItems.Any();

    public RaceMovieInfo Movie => this._movie ??= new(this.Race);
    private RaceMovieInfo? _movie;

    public HistoryData? History { get; }

    public CurrentRaceData? CurrentRace { get; }

    public bool IsDisposed { get; private set; }

    public class HistoryData
    {
      public IReadOnlyList<RaceHorseAnalyzer> BeforeRaces { get; } = Array.Empty<RaceHorseAnalyzer>();

      public IReadOnlyList<RaceHorseAnalyzer> Before15Races { get; } = Array.Empty<RaceHorseAnalyzer>();

      public IReadOnlyList<RaceHorseAnalyzer> BeforeFiveRaces { get; } = Array.Empty<RaceHorseAnalyzer>();

      public RunningStyle RunningStyle { get; }

      public ResultOrderGradeMap AllGrade { get; }

      public ResultOrderGradeMap SameCourseGrade { get; }

      public ResultOrderGradeMap SameGroundGrade { get; }

      public ResultOrderGradeMap SameDistanceGrade { get; }

      public ResultOrderGradeMap SameDirectionGrade { get; }

      public ResultOrderGradeMap SameConditionGrade { get; }

      public ResultOrderGradeMap SameRiderGrade { get; }

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
      /// PCI
      /// </summary>
      public double PciAverage { get; }

      public ValueComparation TimeDVComparation { get; set; }

      public ValueComparation A3HTimeDVComparation { get; set; }

      public ValueComparation UntilA3HTimeDVComparation { get; set; }

      public ValueComparation PciAverageComparation { get; set; }

      public long PrizeMoney { get; }

      public string PrizeMoneyLabel { get; } = string.Empty;

      public HistoryData(RaceData race, RaceHorseData horse, IEnumerable<RaceHorseAnalyzer> raceHistory, JrdbRaceHorseData? jrdbHorse)
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
          var date = new StatisticSingleArray(Enumerable.Range(1, targetRaces.Count()).Select(v => (double)v).ToArray());
          var points = new StatisticDoubleArray(date, statistic);
          var pointsa3h = new StatisticDoubleArray(date, statistica3h);
          var pointsua3h = new StatisticDoubleArray(date, statisticau3h);

          //this.TimeDeviationValue = st.CalcRegressionValue((race.StartTime.Date - startTime).TotalDays);
          var predictValue = 0;
          var single = targetRaces.Count() == 1 ? targetRaces.First() : null;
          this.TimeDeviationValue = MathUtil.AvoidNan(single?.ResultTimeDeviationValue ?? points.CalcRegressionValue(predictValue));
          this.A3HTimeDeviationValue = MathUtil.AvoidNan(single?.A3HResultTimeDeviationValue ?? pointsa3h.CalcRegressionValue(predictValue));
          this.UntilA3HTimeDeviationValue = MathUtil.AvoidNan(single?.UntilA3HResultTimeDeviationValue ?? pointsua3h.CalcRegressionValue(predictValue));

          var pcis = this.Before15Races.Select(h => h.Pci).Where(h => h != default);
          if (pcis.Any())
            this.PciAverage = pcis.Average();
          
          if (jrdbHorse != null && jrdbHorse.RunningStyle != JdbcRunningStyle.Unknown)
          {
            this.RunningStyle = (RunningStyle)(short)jrdbHorse.RunningStyle;
          }
          else
          {
            this.RunningStyle = targetRaces
              .OrderBy(r => r.Data.ResultOrder)
              .GroupBy(r => r.Data.RunningStyle)
              .OrderByDescending(g => g.Count())
              .Select(g => g.Key)
              .FirstOrDefault();
          }

          var nearDistance = ConfigUtil.GetIntValue(
            race.Course <= RaceCourse.CentralMaxValue ? SettingKey.NearDistanceDiffCentral : SettingKey.NearDistanceDiffLocal,
            50);

          // 成績
          this.AllGrade = new ResultOrderGradeMap(this.BeforeRaces.Select(r => r.Data).ToArray());
          this.SameCourseGrade = new ResultOrderGradeMap(this.BeforeRaces
            .Where(r => r.Race.Course == race.Course).Select(r => r.Data).ToArray());
          this.SameGroundGrade = new ResultOrderGradeMap(this.BeforeRaces
            .Where(r => r.Race.TrackGround == race.TrackGround).Select(r => r.Data).ToArray());
          this.SameDistanceGrade = new ResultOrderGradeMap(this.BeforeRaces
            .Where(r => r.Race.Distance >= race.Distance - nearDistance && r.Race.Distance <= race.Distance + nearDistance).Select(r => r.Data).ToArray());
          this.SameDirectionGrade = new ResultOrderGradeMap(this.BeforeRaces
            .Where(r => r.Race.TrackCornerDirection == race.TrackCornerDirection).Select(r => r.Data).ToArray());
          this.SameConditionGrade = new ResultOrderGradeMap(this.BeforeRaces
            .Where(r => r.Race.TrackCondition == race.TrackCondition).Select(r => r.Data).ToArray());
          this.SameRiderGrade = new ResultOrderGradeMap(this.BeforeRaces
            .Where(r => r.Data.RiderCode == horse.RiderCode).Select(r => r.Data).ToArray());

          // 競馬場ごとの成績
          this.CourseGrades.Add(new CourseHorseGrade(race, race.Distance, race.TrackGround, this.BeforeRaces));
          foreach (var courseSource in this.BeforeRaces
            .Where(r => r.Data.AbnormalResult == RaceAbnormality.Unknown && r.Data.ResultTime != default)
            .GroupBy(r => r.Race.Course))
          {
            var grade = new CourseHorseGrade(race, courseSource.Key, race.Distance, race.TrackGround, courseSource.ToArray())
            {
              IsCurrentCourse = courseSource.Key == race.Course,
            };
            this.CourseGrades.Add(grade);
          }

          // 賞金
          var prizeMoney = 0L;
          foreach (var data in raceHistory.Where(h => h.Data.ResultOrder <= 5 && h.Data.ResultOrder > 0))
          {
            var order = data.Data.ResultOrder;
            if (data.CurrentRace != null)
            {
              order = 0;
              foreach (var sameHorse in data.CurrentRace.TopHorses.Where(h => h.ResultOrder <= data.Data.ResultOrder && h.ResultOrder > 0))
              {
                order++;
              }
            }
            if (order > 7) order = 7;

            var pm = data.Race.GetPrizeMoneys();
            var epm = data.Race.GetExtraPrizeMoneys();
            prizeMoney += pm.ElementAtOrDefault(order - 1) + epm.ElementAtOrDefault(order - 1);
          }
          this.PrizeMoney = prizeMoney * 100;
          this.PrizeMoneyLabel = ValueUtil.ToMoneyLabel(this.PrizeMoney);
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

      /// <summary>
      /// １着のタイム指数。レースのペースを決めるのに使う
      /// </summary>
      public double TopDeviationValue { get; }

      public RaceStandardTimeMasterData? StandardTime { get; }

      public CurrentRaceData(RaceData race, RaceHorseData horse, IEnumerable<RaceHorseData> sameRaceHorses, RaceStandardTimeMasterData? raceStandardTime)
      {
        this.StandardTime = raceStandardTime;

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
      this.ResultOrderComparation = horse.ResultOrder >= 1 && horse.ResultOrder <= 3 ? ValueComparation.Good :
        horse.ResultOrder >= 8 || horse.ResultOrder >= race.HorsesCount * 0.7f ? ValueComparation.Bad : ValueComparation.Standard;
      if (race.Distance >= 800)
      {
        this.UntilA3HResultTime = horse.ResultTime - horse.AfterThirdHalongTime;
      }

      this.Memo.Value = horse.Memo ?? string.Empty;
      AnalysisUtil.SetMemoEvents(() => this.Data.Memo ?? string.Empty, (db, m) =>
      {
        db.RaceHorses!.Attach(this.Data);
        this.Data.Memo = m;
      }, this.Memo, this.IsMemoSaving).AddTo(this._disposables);

      this.IsChecked.Skip(1).Subscribe(async c =>
      {
        if (this._isChecking)
        {
          return;
        }

        try
        {
          using var db = new MyContext();
          if (c)
          {
            await CheckHorseUtil.CheckAsync(db, this.Data.Key, HorseCheckType.CheckRace);
          }
          else
          {
            await CheckHorseUtil.UncheckAsync(db, this.Data.Key, HorseCheckType.CheckRace);
          }
        }
        catch (Exception ex)
        {
          logger.Error("馬のチェックに失敗", ex);
        }
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

      this.Pci = AnalysisUtil.CalcPci(race, horse);
      this.PciDVComparation = AnalysisUtil.CompareValue(this.Pci, 45, 55, true);
    }

    public RaceHorseAnalyzer(RaceData race, RaceHorseData horse, RaceStandardTimeMasterData? raceStandardTime, JrdbRaceHorseData? jrdbHorse = null)
      : this(race, horse)
    {
      try
      {
        if (raceStandardTime != null && raceStandardTime.SampleCount > 0 && _timeDeviationValueCalculator != null)
        {
          this.ResultTimeDeviationValue = MathUtil.AvoidNan(_timeDeviationValueCalculator.GetTimeDeviationValueAsync(race, horse, raceStandardTime).Result);
          this.A3HResultTimeDeviationValue = MathUtil.AvoidNan(_timeDeviationValueCalculator.GetA3HTimeDeviationValueAsync(race, horse, raceStandardTime).Result);
          this.UntilA3HResultTimeDeviationValue = MathUtil.AvoidNan(_timeDeviationValueCalculator.GetUntilA3HTimeDeviationValueAsync(race, horse, raceStandardTime).Result);
        }
      }
      catch (Exception ex)
      {
        logger.Error("不明なエラーが発生", ex);
      }
    }

    public RaceHorseAnalyzer(RaceData race, RaceHorseData horse, IEnumerable<RaceHorseData> sameRaceHorses, RaceStandardTimeMasterData? raceStandardTime, JrdbRaceHorseData? jrdbHorse = null)
      : this(race, horse, raceStandardTime, jrdbHorse)
    {
      this.JrdbData = jrdbHorse;
      this.CurrentRace = new CurrentRaceData(race, horse, sameRaceHorses, raceStandardTime);
    }

    public RaceHorseAnalyzer(RaceData race, RaceHorseData horse, IEnumerable<RaceHorseData> sameRaceHorses, IEnumerable<RaceHorseAnalyzer> raceHistory, RaceStandardTimeMasterData? raceStandardTime, JrdbRaceHorseData? jrdbHorse = null)
      : this(race, horse, sameRaceHorses, raceStandardTime, jrdbHorse)
    {
      this.History = new HistoryData(race, horse, raceHistory, jrdbHorse);
    }

    #region Command

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

    public ICommand OpenRaceWindowCommand =>
      this._openRaceWindowCommand ??=
        new ReactiveCommand<string>().WithSubscribe(key => OpenRaceRequest.Default.Request(key, this.Data.Key));
    private ReactiveCommand<string>? _openRaceWindowCommand;

    #endregion

    public void ChangeIsCheck(bool status)
    {
      this._isChecking = true;
      this.IsChecked.Value = status;
      this._isChecking = false;
    }

    private async Task ChangeHorseMarkAsync(string marks)
    {
      var mark = EnumUtil.ToHorseMark(marks);
      var oldMark = this.Mark.Value;

      try
      {
        using var db = new MyContext();
        this.ChangeHorseMark(db, mark);
        await db.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        // TODO: エラーを画面に出す
        logger.Warn("印付けでエラー", ex);
        this.Mark.Value = this.Data.Mark = oldMark;
      }
    }

    public void ChangeHorseMark(MyContext db, RaceHorseMark mark)
    {
      db.RaceHorses!.Attach(this.Data);
      this.Mark.Value = this.Data.Mark = mark;
    }

    public void SetOddsTimeline(IEnumerable<SingleOddsTimeline> timeline)
    {
      var prevTime = 60;

      foreach (var data in timeline.OrderBy(t => t.Time))
      {
        var odds = data.GetSingleOdds();
        var item = new OddsTimelineItem(this.Race, data, odds.ElementAtOrDefault(this.Data.Number - 1));
        this.OddsTimeline.Add(item);

        if (data.Time >= this.Race.StartTime.AddMinutes(-60) && data.Time <= this.Race.StartTime.AddMinutes(5))
        {
          if (item.LeftTime.Minutes <= prevTime ||
            (this.Race.Course <= RaceCourse.CentralMaxValue && prevTime <= 10) || prevTime <= 4) // 地方競馬は１分に１回送ってくる
          {
            this.OddsTimelineLatestItems.Add(item);
            prevTime -= 5;
          }
        }
      }

      if (this.OddsTimelineLatestItems.Skip(3).Any())
      {
        var oddsMax = this.OddsTimelineLatestItems.OrderByDescending(o => o.Odds).ElementAtOrDefault(1)?.Odds ?? default;
        var oddsMin = this.OddsTimelineLatestItems.OrderBy(o => o.Odds).ElementAtOrDefault(1)?.Odds ?? default;
        foreach (var item in this.OddsTimelineLatestItems)
        {
          item.SingleOddsComparation = AnalysisUtil.CompareValue(item.Odds, oddsMin, oddsMax, true);
        }
      }
    }

    public void Dispose()
    {
      this._disposables.Dispose();
      this.BloodSelectors?.Dispose();
      this.FinderModel.Value?.Dispose();

      if (this.History != null)
      {
        foreach (var h in this.History.BeforeRaces)
        {
          h.Dispose();
        }
      }

      this.IsDisposed = true;
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
    Exception,
  }

  public struct ResultOrderGradeMap
  {
    public int FirstCount { get; } = 0;

    public int SecondCount { get; } = 0;

    public int ThirdCount { get; } = 0;

    public int LoseCount { get; } = 0;

    public int AllCount => this.FirstCount + this.SecondCount + this.ThirdCount + this.LoseCount;

    public int PlacingBetsCount { get; } = 0;

    public bool IsZero => this.AllCount == 0;

    /// <summary>
    /// 複勝勝率
    /// </summary>
    public float PlacingBetsRate { get; } = 0;

    /// <summary>
    /// 連対率
    /// </summary>
    public float TopRatio => (this.FirstCount + this.SecondCount) / (float)this.AllCount;

    public float WinRate { get; } = 0;

    /// <summary>
    /// 単勝回収率
    /// </summary>
    public float RecoveryRate { get; } = 0;

    public ValueComparation PlacingBetsRateComparation => this.PlacingBetsRate >= 0.5 ? ValueComparation.Good : this.PlacingBetsRate <= 0.15 ? ValueComparation.Bad : ValueComparation.Standard;

    public ResultOrderGradeMap(IReadOnlyList<RaceHorseData> source)
    {
      var targets = source.Where(s => s.AbnormalResult == RaceAbnormality.Unknown).ToArray();
      if (targets.Any())
      {
        this.FirstCount = targets.Count(f => f.ResultOrder == 1);
        this.SecondCount = targets.Count(f => f.ResultOrder == 2);
        this.ThirdCount = targets.Count(f => f.ResultOrder == 3);
        this.LoseCount = targets.Count(f => f.ResultOrder > 3);

        this.PlacingBetsCount = this.FirstCount + this.SecondCount + this.ThirdCount;
        this.PlacingBetsRate = this.PlacingBetsCount / (float)targets.Length;
        this.WinRate = this.FirstCount / (float)targets.Length;

        var won = source.Where(s => s.ResultOrder == 1 && s.Odds != default);
        if (won.Any())
        {
          this.RecoveryRate = won.Sum(w => w.Odds * 10) / (float)(source.Count(s => s.ResultOrder > 1) * 100);
        }
      }
    }

    public ResultOrderGradeMap(IReadOnlyList<RaceHorseAnalyzer> source)
    {
      var targets = source.Where(s => s.Data.AbnormalResult == RaceAbnormality.Unknown &&
        s.Race.DataStatus != RaceDataStatus.Canceled && s.Race.DataStatus != RaceDataStatus.Delete).ToArray();
      if (targets.Any())
      {
        this.FirstCount = targets.Count(f => f.Data.ResultOrder == 1);
        this.SecondCount = targets.Count(f => f.Data.ResultOrder == 2);
        this.ThirdCount = targets.Count(f => f.Data.ResultOrder == 3);
        this.LoseCount = targets.Count(f => f.Data.ResultOrder > 3);

        this.PlacingBetsCount = targets.Count(f => f.Data.ResultOrder <= (f.Race.HorsesCount >= 7 ? 3 : 2) && f.Data.ResultOrder > 0);
        this.PlacingBetsRate = this.PlacingBetsCount / (float)targets.Length;
        this.WinRate = this.FirstCount / (float)targets.Length;

        var won = source.Where(s => s.Data.ResultOrder == 1 && s.Data.Odds != default);
        if (won.Any())
        {
          this.RecoveryRate = won.Sum(w => w.Data.Odds * 10) / (float)(source.Count(s => s.Data.ResultOrder >= 1) * 100);
        }
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

    public RacePace ShortestTimeRacePace { get; }

    public RaceHorseData? ShortestTimeRaceHorse { get; }

    public RaceHorseAnalyzer? ShortestTimeRaceHorseAnalyzer { get; }

    public RaceSubjectInfo? ShortestTimeRaceSubject { get; }

    public double ShortestTimeRaceHorseDeviationValue { get; }

    public double ShortestTimeRaceHorseA3HDeviationValue { get; }

    public double ShortestTimeRaceTopHorseDeviationValue { get; }

    public CourseHorseGrade(RaceData currentRace, RaceCourse course, short distance, TrackGround ground, IReadOnlyList<RaceHorseAnalyzer> source)
    {
      var filtered = source.Where(s => s.Data.AbnormalResult == RaceAbnormality.Unknown && s.Data.ResultOrder > 0);

      this.Course = course;
      this.AllGrade = new ResultOrderGradeMap(filtered.Select(s => s.Data).ToArray());

      var diff = ConfigUtil.GetIntValue(course <= RaceCourse.CentralMaxValue ? SettingKey.NearDistanceDiffCentral : SettingKey.NearDistanceDiffLocal, 50);
      var daysDiff = ConfigUtil.GetIntValue(course <= RaceCourse.CentralMaxValue ? SettingKey.ShortestTimeNearYearCentral : SettingKey.ShortestTimeNearYearLocal, 10) * 365;
      var nearDistanceRaces = filtered
        .Where(s => System.Math.Abs(s.Race.Distance - distance) <= diff && s.Race.TrackGround == ground)
        .Where(s => s.Race.StartTime != default && currentRace.StartTime > s.Race.StartTime)
        .Where(s => (currentRace.StartTime - s.Race.StartTime).TotalDays <= daysDiff);
      if (nearDistanceRaces.Any())
      {
        this.HasData = true;
        this.Grade = new ResultOrderGradeMap(nearDistanceRaces.Select(s => s.Data).ToArray());

        var shortestTime = nearDistanceRaces
          .Select(s => new { Data = s, ComparationValue = s.Data.ResultTime.TotalSeconds / s.Race.Distance, })
          .OrderBy(s => s.ComparationValue)
          .First().Data;
        this.ShortestTime = shortestTime.Data.ResultTime;
        if (shortestTime.Race.Distance == distance)
        {
          // 小数がびみょうに変わったりしてしまう
          this.ShortestTimeNormalized = shortestTime.Data.ResultTime;
        }
        else
        {
          this.ShortestTimeNormalized = TimeSpan.FromTicks(
            (int)System.Math.Round((float)shortestTime.Data.ResultTime.Ticks / shortestTime.Race.Distance * distance));
        }
        this.ShortestTimeRace = shortestTime.Race;
        this.ShortestTimeRacePace = AnalysisUtil.CalcRacePace(shortestTime.Race);
        this.ShortestTimeRaceHorse = shortestTime.Data;
        this.ShortestTimeRaceHorseAnalyzer = shortestTime;
        this.ShortestTimeRaceSubject = shortestTime.Subject;
        this.ShortestTimeRaceHorseDeviationValue = shortestTime.ResultTimeDeviationValue;
        this.ShortestTimeRaceHorseA3HDeviationValue = shortestTime.A3HResultTimeDeviationValue;
        this.ShortestTimeRaceTopHorseDeviationValue = shortestTime.CurrentRace?.TopDeviationValue ?? default;
      }
    }

    public CourseHorseGrade(RaceData currentRace, short distance, TrackGround ground, IReadOnlyList<RaceHorseAnalyzer> source) : this(currentRace, RaceCourse.All, distance, ground, source)
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
