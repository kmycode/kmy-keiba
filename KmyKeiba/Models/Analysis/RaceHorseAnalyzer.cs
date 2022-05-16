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

namespace KmyKeiba.Models.Analysis
{
  public class RaceHorseAnalyzer
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

    public IReadOnlyList<RaceHorseCornerGrade> CornerGrades { get; } = Array.Empty<RaceHorseCornerGrade>();

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

      /// <summary>
      /// 後３ハロンタイム指数
      /// </summary>
      public double A3HTimeDeviationValue { get; }

      /// <summary>
      /// タイム指数
      /// </summary>
      public double TimeDeviationValue { get; }

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
        this.RoughRate = AnalysisUtil.CalcRoughRate(sameRaceHorses.ToArray());
      }
    }

    private RaceHorseAnalyzer(RaceData race, RaceHorseData horse)
    {
      this.Race = race;
      this.Data = horse;
      this.Subject = new RaceSubjectInfo(race);
      this.ResultTimePerMeter = (double)horse.ResultTime.TotalSeconds / race.Distance;

      // コーナーの成績
      CornerGradeType GetCornerGradeType(short order, short beforeOrder)
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
  public struct ResultOrderGradeMap
  {
    public int FirstCount { get; }

    public int SecondCount { get; }

    public int ThirdCount { get; }

    public int LoseCount { get; }

    public int AllCount => this.FirstCount + this.SecondCount + this.ThirdCount + this.LoseCount;

    public bool IsZero => this.AllCount == 0;

    /// <summary>
    /// 複勝勝率
    /// </summary>
    public float PlacingBetsRate { get; }

    public ValueComparation PlacingBetsRateComparation => this.PlacingBetsRate >= 0.75 ? ValueComparation.Good : this.PlacingBetsRate <= 0.2 ? ValueComparation.Bad : ValueComparation.Standard;

    public ResultOrderGradeMap(IReadOnlyList<RaceHorseData> source)
    {
      this.FirstCount = source.Count(f => f.ResultOrder == 1);
      this.SecondCount = source.Count(f => f.ResultOrder == 2);
      this.ThirdCount = source.Count(f => f.ResultOrder == 3);
      this.LoseCount = source.Count(f => f.ResultOrder > 3);

      this.PlacingBetsRate = (this.FirstCount + this.SecondCount + this.ThirdCount) / (float)source.Count;
    }
  }
}
