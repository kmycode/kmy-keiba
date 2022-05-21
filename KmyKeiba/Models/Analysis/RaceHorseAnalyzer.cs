using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis.Math;
using KmyKeiba.Models.Race;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KmyKeiba.Models.Analysis
{
  public class RaceHorseAnalyzer : IDisposable
  {
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

    public HistoryData? History { get; }

    public CurrentRaceData? CurrentRace { get; }

    public class HistoryData
    {
      public IReadOnlyList<RaceHorseAnalyzer> BeforeRaces { get; } = Array.Empty<RaceHorseAnalyzer>();

      public IReadOnlyList<RaceHorseAnalyzer> BeforeFiveRaces { get; } = Array.Empty<RaceHorseAnalyzer>();

      public RunningStyle RunningStyle { get; }

      public ResultOrderGradeMap AllGrade { get; }

      public ResultOrderGradeMap SameGroundGrade { get; }

      public ResultOrderGradeMap SameDistanceGrade { get; }

      public ResultOrderGradeMap SameDirectionGrade { get; }

      public ResultOrderGradeMap SameConditionGrade { get; }

      public ResultOrderGradeMap SameRiderGrade { get; }

      public ResultOrderGradeMap SprinterGrade { get; }

      public ResultOrderGradeMap MylarGrade { get; }

      public ResultOrderGradeMap ClassicDistanceGrade { get; }

      public ResultOrderGradeMap SteyerGrade { get; }

      /// <summary>
      /// 後３ハロンタイム指数
      /// </summary>
      public double A3HTimeDeviationValue { get; }

      /// <summary>
      /// タイム指数
      /// </summary>
      public double TimeDeviationValue { get; }

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

      public CurrentRaceData(RaceHorseData horse, IEnumerable<RaceHorseData> sameRaceHorses)
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
      }
    }

    private RaceHorseAnalyzer(RaceData race, RaceHorseData horse)
    {
      this.Race = race;
      this.Data = horse;
      this.Subject = new RaceSubjectInfo(race);
      this.Mark.Value = horse.Mark;
      this.ResultTimePerMeter = (double)horse.ResultTime.TotalSeconds / race.Distance;
      this.ResultOrderComparation = horse.ResultOrder >= 1 && horse.ResultOrder <= 3 ? ValueComparation.Good :
        horse.ResultOrder >= 8 || horse.ResultOrder >= race.HorsesCount * 0.7f ? ValueComparation.Bad : ValueComparation.Standard;

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
      if (raceStandardTime != null && raceStandardTime.SampleCount > 0)
      {
        this.ResultTimeDeviationValue = 100 - StatisticSingleArray.CalcDeviationValue(this.ResultTimePerMeter, raceStandardTime.Average, raceStandardTime.Deviation);
        this.A3HResultTimeDeviationValue = 100 - StatisticSingleArray.CalcDeviationValue(horse.AfterThirdHalongTime.TotalSeconds, raceStandardTime.A3FAverage, raceStandardTime.A3FDeviation);
      }
    }

    public RaceHorseAnalyzer(RaceData race, RaceHorseData horse, IEnumerable<RaceHorseData> sameRaceHorses, RaceStandardTimeMasterData? raceStandardTime)
      : this(race, horse, raceStandardTime)
    {
      this.CurrentRace = new CurrentRaceData(horse, sameRaceHorses);
    }

    public RaceHorseAnalyzer(RaceData race, RaceHorseData horse, IEnumerable<RaceHorseData> sameRaceHorses, IEnumerable<RaceHorseAnalyzer> raceHistory, RaceStandardTimeMasterData? raceStandardTime)
      : this(race, horse, sameRaceHorses, raceStandardTime)
    {
      this.History = new HistoryData(race, horse, raceHistory);
    }

    public ICommand SetMarkCommand =>
      this._setDoubleCircleMarkCommand ??=
        new AsyncReactiveCommand<string>().WithSubscribe(p => this.ChangeHorseMarkAsync(p));
    private AsyncReactiveCommand<string>? _setDoubleCircleMarkCommand;

    private async Task ChangeHorseMarkAsync(string marks)
    {
      short.TryParse(marks, out var markss);
      var mark = (RaceHorseMark)markss;

      using var db = new Data.MyContext();
      db.RaceHorses!.Attach(this.Data);

      this.Mark.Value = this.Data.Mark = mark;

      await db.SaveChangesAsync();
    }

    public void Dispose()
    {
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
      if (source.Any())
      {
        this.FirstCount = source.Count(f => f.ResultOrder == 1);
        this.SecondCount = source.Count(f => f.ResultOrder == 2);
        this.ThirdCount = source.Count(f => f.ResultOrder == 3);
        this.LoseCount = source.Count(f => f.ResultOrder > 3);

        this.PlacingBetsRate = (this.FirstCount + this.SecondCount + this.ThirdCount) / (float)source.Count;
        this.WinRate = this.FirstCount / (float)source.Count;
      }
    }
  }
}
