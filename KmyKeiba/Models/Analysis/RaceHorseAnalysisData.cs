using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis.Math;
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

    public IReadOnlyList<RaceHorseAnalysisData> BeforeRaces { get; } = Array.Empty<RaceHorseAnalysisData>();

    public IReadOnlyList<RaceHorseAnalysisData> BeforeFiveRaces { get; } = Array.Empty<RaceHorseAnalysisData>();

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

    public RunningStyle RunningStyle { get; }

    private RaceHorseAnalysisData(RaceData race, RaceHorseData horse)
    {
      this.Race = race;
      this.Data = horse;
      this.ResultTimePerMeter = (double)horse.ResultTime.TotalSeconds / race.Distance;
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

    public RaceHorseAnalysisData(RaceData race, RaceHorseData horse, IEnumerable<RaceHorseAnalysisData> raceHistory, RaceStandardTimeMasterData? raceStandardTime)
      : this(race, horse, raceStandardTime)
    {
      this.BeforeRaces = raceHistory.OrderByDescending(h => h.Race.StartTime).ToArray();
      this.BeforeFiveRaces = this.BeforeRaces.Take(5).ToArray();

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
      }
    }
  }
}
