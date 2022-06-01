using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis.Math;
using KmyKeiba.Models.Race;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis
{
  public class RaceAnalyzer : IDisposable
  {
    public RaceData Data { get; }

    public RaceSubjectInfo Subject { get; }

    public RaceHorseData? TopHorseData => this.TopHorse.Data;

    public RaceHorseAnalyzer TopHorse { get; } = RaceHorseAnalyzer.Empty;

    public IReadOnlyList<RaceHorseAnalyzer> TopHorses { get; }

    public RunningStyle TopRunningStyle { get; }

    public IReadOnlyList<RunningStyle> RunningStyles { get; }

    /// <summary>
    /// 荒れ度
    /// </summary>
    public double RoughRate { get; }

    public double ResultTimeDeviationValue { get; }

    public double A3HResultTimeDeviationValue { get; }

    public RacePace Pace { get; }

    public RacePace A3HPace { get; }

    public RaceAnalyzer(RaceData race, IReadOnlyList<RaceHorseData> topHorses, RaceStandardTimeMasterData raceStandardTime)
    {
      var topHorse = topHorses.OrderBy(h => h.ResultOrder).FirstOrDefault(h => h.ResultOrder == 1) ?? new();

      this.Data = race;
      this.TopHorses = topHorses.Select(h => new RaceHorseAnalyzer(race, h, raceStandardTime)).ToArray();
      this.Subject = new RaceSubjectInfo(race);
      this.RunningStyles = topHorses.OrderBy(h => h.ResultOrder)
        .Take(3)
        .Select(rh => rh.RunningStyle)
        .Where(rs => rs != RunningStyle.Unknown)
        .ToArray();
      this.TopRunningStyle = this.RunningStyles.FirstOrDefault();

      this.RoughRate = AnalysisUtil.CalcRoughRate(topHorses);

      if (topHorse != null)
      {
        this.TopHorse = new RaceHorseAnalyzer(race, topHorse, raceStandardTime);

        this.Pace = this.TopHorse.ResultTimeDeviationValue < 38 ? RacePace.VeryLow :
          this.TopHorse.ResultTimeDeviationValue < 45 ? RacePace.Low :
          this.TopHorse.ResultTimeDeviationValue < 55 ? RacePace.Standard :
          this.TopHorse.ResultTimeDeviationValue < 62 ? RacePace.High : RacePace.VeryHigh;
        this.A3HPace = this.TopHorse.A3HResultTimeDeviationValue < 38 ? RacePace.VeryLow :
          this.TopHorse.A3HResultTimeDeviationValue < 45 ? RacePace.Low :
          this.TopHorse.A3HResultTimeDeviationValue < 55 ? RacePace.Standard :
          this.TopHorse.A3HResultTimeDeviationValue < 62 ? RacePace.High : RacePace.VeryHigh;
        this.ResultTimeDeviationValue = this.TopHorse.ResultTimeDeviationValue;
        this.A3HResultTimeDeviationValue = this.TopHorse.A3HResultTimeDeviationValue;
      }
    }

    public void Dispose()
    {
      this.TopHorse.Dispose();
      foreach (var h in this.TopHorses)
      {
        h.Dispose();
      }
    }
  }

  public enum RacePace
  {
    [Label("とても速い")]
    VeryHigh,

    [Label("速い")]
    High,

    [Label("標準")]
    Standard,

    [Label("遅い")]
    Low,

    [Label("とても遅い")]
    VeryLow,
  }
}
