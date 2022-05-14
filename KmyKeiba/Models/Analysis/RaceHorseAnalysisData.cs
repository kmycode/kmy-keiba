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
  public class RaceHorseAnalysisData
  {
    public static RaceHorseAnalysisData Empty { get; } = new(new RaceData(), new RaceHorseData());

    public RaceData Race { get; }

    public RaceHorseData Data { get; }

    public RaceSubjectInfo Subject { get; }

    public RaceHorseTrendAnalysisSelector? TrendAnalyzers { get; init; }

    public ReactiveProperty<RiderAnalysisData?> Rider { get; } = new();

    public ReactiveProperty<TrainerAnalysisData?> Trainer { get; } = new();

    public ReactiveProperty<TrainingAnalysisData?> Training { get; } = new();

    public IReadOnlyList<RaceHorseAnalysisData> BeforeRaces { get; } = Array.Empty<RaceHorseAnalysisData>();

    public IReadOnlyList<RaceHorseAnalysisData> BeforeFiveRaces { get; } = Array.Empty<RaceHorseAnalysisData>();

    public IReadOnlyList<RaceHorseCornerGrade> CornerGrades { get; } = Array.Empty<RaceHorseCornerGrade>();

    public double ResultTimePerMeter { get; }

    /// <summary>
    /// 結果からのタイム指数
    /// </summary>
    public double ResultTimeDeviationValue { get; }

    /// <summary>
    /// タイム指数
    /// </summary>
    public double TimeDeviationValue { get; }

    /// <summary>
    /// 後３ハロンタイム指数
    /// </summary>
    public double A3HTimeDeviationValue { get; }

    /// <summary>
    /// 斤量の相対評価
    /// </summary>
    public ValueComparation RiderWeightComparation { get; }

    public ValueComparation AgeComparation { get; }

    public RunningStyle RunningStyle { get; }

    public ResultOrderGradeMap AllGrade { get; }

    public ResultOrderGradeMap SameGroundGrade { get; }

    public ResultOrderGradeMap SameDistanceGrade { get; }

    public ResultOrderGradeMap SameDirectionGrade { get; }

    public ResultOrderGradeMap SameConditionGrade { get; }

    private RaceHorseAnalysisData(RaceData race, RaceHorseData horse)
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

    public RaceHorseAnalysisData(RaceData race, RaceHorseData horse, RaceStandardTimeMasterData? raceStandardTime)
      : this(race, horse)
    {
      if (raceStandardTime != null && raceStandardTime.SampleCount > 0)
      {
        this.ResultTimeDeviationValue = 100 - StatisticSingleArray.CalcDeviationValue(this.ResultTimePerMeter, raceStandardTime.Average, raceStandardTime.Deviation);
        this.A3HTimeDeviationValue = 100 - StatisticSingleArray.CalcDeviationValue(horse.AfterThirdHalongTime.TotalSeconds, raceStandardTime.A3FAverage, raceStandardTime.A3FDeviation);
      }
    }

    public RaceHorseAnalysisData(RaceData race, RaceHorseData horse, IEnumerable<RaceHorseData> sameRaceHorses, IEnumerable<RaceHorseAnalysisData> raceHistory, RaceStandardTimeMasterData? raceStandardTime)
      : this(race, horse, raceStandardTime)
    {
      this.BeforeRaces = raceHistory.OrderByDescending(h => h.Race.StartTime).ToArray();
      this.BeforeFiveRaces = this.BeforeRaces.Take(5).ToArray();

      var weightPoint = new StatisticSingleArray(sameRaceHorses.Select(h => (double)h.RiderWeight).ToArray());
      var median = (short)weightPoint.Median;  // 小数点以下切り捨て。中央値が常に整数とは限らない（全体の数が偶数の時））
      this.RiderWeightComparation = horse.RiderWeight > median ? ValueComparation.Bad :
        horse.RiderWeight < median ? ValueComparation.Good : ValueComparation.Standard;

      var agePoint = new StatisticSingleArray(sameRaceHorses.Select(h => (double)h.Age).ToArray());
      var ageMedian = (short)agePoint.Median;
      this.AgeComparation = horse.Age > ageMedian ? ValueComparation.Bad :
        horse.Age < ageMedian ? ValueComparation.Good : ValueComparation.Standard;

      if (this.BeforeRaces.Any())
      {
        var targetRaces = this.BeforeRaces.Take(10).ToArray();

        var startTime = new DateTime(1980, 1, 1);
        var statistic = new StatisticSingleArray(targetRaces.Select(r => r.ResultTimeDeviationValue).ToArray());
        var statistica3h = new StatisticSingleArray(targetRaces.Select(r => r.A3HTimeDeviationValue).ToArray());
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
      }
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
}
