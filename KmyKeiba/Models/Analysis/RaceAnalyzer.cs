﻿using KmyKeiba.Data.Db;
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
  public class RaceAnalyzer
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

    public RaceAnalyzer(RaceData race, IReadOnlyList<RaceHorseData> topHorses, RaceStandardTimeMasterData raceStandardTime)
    {
      var topHorse = topHorses.OrderBy(h => h.ResultOrder).FirstOrDefault() ?? new();

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

      if (this.TopHorseData != null)
      {
        this.TopHorse = new RaceHorseAnalyzer(race, this.TopHorseData, raceStandardTime);
      }
    }
  }
}
