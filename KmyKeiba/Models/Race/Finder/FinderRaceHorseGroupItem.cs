using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.Finder
{
  public class FinderRaceHorseGroupItem
  {
    public string GroupKey { get; }

    public MemoColor Color { get; }

    public ReactiveCollection<FinderRow> Rows { get; } = new();

    public ReactiveCollection<FinderRaceHorseItem> Items { get; } = new();

    public ResultOrderGradeMap Grades { get; }

    public ValueComparation RecoveryRateComparation { get; }

    public double PlaceBetsRecoveryRate { get; }

    public ValueComparation PlaceBetsRRComparation { get; }

    public double FrameRecoveryRate { get; }

    public ValueComparation FrameRRComparation { get; }

    public double QuinellaPlaceRecoveryRate { get; }

    public ValueComparation QuinellaPlaceRRComparation { get; }

    public double QuinellaRecoveryRate { get; }

    public ValueComparation QuinellaRRComparation { get; }

    public double ExactaRecoveryRate { get; }

    public ValueComparation ExactaRRComparation { get; }

    public double TrioRecoveryRate { get; }

    public ValueComparation TrioRRComparation { get; }

    public double TrifectaRecoveryRate { get; }

    public ValueComparation TrifectaRRComparation { get; }

    public FinderRaceHorseGroupItem(string key, IEnumerable<FinderRaceHorseItem> group)
    {
      this.GroupKey = key;
      this.Grades = new ResultOrderGradeMap(group.Select(g => g.Analyzer).ToArray());

      foreach (var item in group)
      {
        this.Items.Add(item);
      }

      var targets = group.Where(s => s.Analyzer.Data.AbnormalResult == RaceAbnormality.Unknown &&
        s.Analyzer.Race.DataStatus != RaceDataStatus.Canceled && s.Analyzer.Race.DataStatus != RaceDataStatus.Delete).ToArray();
      if (targets.Any())
      {
        var won = targets.Where(s => s.Analyzer.Data.ResultOrder == 1 && s.Analyzer.Data.Odds != default);
        Func<RaceData, short> horsesCount = g => g.ResultHorsesCount > 0 ? g.ResultHorsesCount : g.HorsesCount;

        // 複数馬の絡むものは全流しで
        this.PlaceBetsRecoveryRate = targets.Sum(g => g.PlaceBetsPayoff) / ((double)targets.Length * 100);
        this.FrameRecoveryRate = targets.Sum(g => g.FramePayoff) /
          (double)(group.Sum(g => Math.Min(horsesCount(g.Analyzer.Race), (short)8) * 100));
        this.QuinellaPlaceRecoveryRate = targets.Sum(g => g.QuinellaPlacePayoff) /
          (double)(group.Sum(g => (horsesCount(g.Analyzer.Race) - 1) * 100));
        this.QuinellaRecoveryRate = targets.Sum(g => g.QuinellaPayoff) /
          (double)(group.Sum(g => (horsesCount(g.Analyzer.Race) - 1) * 100));
        this.ExactaRecoveryRate = targets.Sum(g => g.ExactaPayoff) /
          (double)(group.Sum(g => (horsesCount(g.Analyzer.Race) - 1) * 100 * 2));
        this.TrioRecoveryRate = targets.Sum(g => g.TrioPayoff) /
          (double)(group.Sum(g => (horsesCount(g.Analyzer.Race) - 1) * (horsesCount(g.Analyzer.Race) - 2) / 2 * 100));
        this.TrifectaRecoveryRate = targets.Sum(g => g.TrifectaPayoff) /
          (double)(group.Sum(g => (horsesCount(g.Analyzer.Race) - 1) * (horsesCount(g.Analyzer.Race) - 2) * 100 * 3));

        this.RecoveryRateComparation = AnalysisUtil.CompareValue(this.Grades.RecoveryRate, 1, 0.7);
        this.PlaceBetsRRComparation = AnalysisUtil.CompareValue(this.PlaceBetsRecoveryRate, 1, 0.7);
        this.FrameRRComparation = AnalysisUtil.CompareValue(this.FrameRecoveryRate, 0.9, 0.6);
        this.QuinellaPlaceRRComparation = AnalysisUtil.CompareValue(this.QuinellaPlaceRecoveryRate, 0.9, 0.6);
        this.QuinellaRRComparation = AnalysisUtil.CompareValue(this.QuinellaRecoveryRate, 0.9, 0.6);
        this.ExactaRRComparation = AnalysisUtil.CompareValue(this.ExactaRecoveryRate, 0.9, 0.6);
        this.TrioRRComparation = AnalysisUtil.CompareValue(this.TrioRecoveryRate, 0.85, 0.5);
        this.TrifectaRRComparation = AnalysisUtil.CompareValue(this.TrifectaRecoveryRate, 0.85, 0.5);
      }
    }

    public FinderRaceHorseGroupItem(IEnumerable<FinderRaceHorseItem> group) : this("デフォルト", group)
    {
    }

    public FinderRaceHorseGroupItem(IGrouping<object, FinderRaceHorseItem> group) : this(group.Key.ToString() ?? string.Empty, group)
    {
      if (group.Key is PointLabelItem label)
      {
        this.Color = label.Color;
      }
    }

    public FinderRaceHorseGroupItem(IGrouping<string, FinderRaceHorseItem> group) : this(group.Key, group)
    {
    }

    public FinderRaceHorseGroupItem(IGrouping<int, FinderRaceHorseItem> group) : this(group.Key.ToString(), group)
    {
    }
  }
}
