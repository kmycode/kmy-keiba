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
    private readonly RaceHorseFinderResultAnalyzer _result;

    public string GroupKey { get; }

    public MemoColor Color { get; }

    public ReactiveCollection<FinderRow> Rows { get; } = new();

    public ReactiveCollection<FinderRaceHorseItem> Items { get; } = new();

    public ResultOrderGradeMap Grades => this._result.AllGrade;

    public ValueComparation RecoveryRateComparation => this._result.RecoveryRateComparation;

    public double PlaceBetsRecoveryRate => this._result.PlaceBetsRecoveryRate;

    public ValueComparation PlaceBetsRRComparation => this._result.PlaceBetsRRComparation;

    public double FrameRecoveryRate => this._result.FrameRecoveryRate;

    public ValueComparation FrameRRComparation => this._result.FrameRRComparation;

    public double QuinellaPlaceRecoveryRate => this._result.QuinellaPlaceRecoveryRate;

    public ValueComparation QuinellaPlaceRRComparation => this._result.QuinellaPlaceRRComparation;

    public double QuinellaRecoveryRate => this._result.QuinellaRecoveryRate;

    public ValueComparation QuinellaRRComparation => this._result.QuinellaRRComparation;

    public double ExactaRecoveryRate => this._result.ExactaRecoveryRate;

    public ValueComparation ExactaRRComparation => this._result.ExactaRRComparation;

    public double TrioRecoveryRate => this._result.TrioRecoveryRate;

    public ValueComparation TrioRRComparation => this._result.TrioRRComparation;

    public double TrifectaRecoveryRate => this._result.TrifectaRecoveryRate;

    public ValueComparation TrifectaRRComparation => this._result.TrifectaRRComparation;

    public FinderRaceHorseGroupExpandedData? ExpandedData { get; set; }

    public FinderRaceHorseGroupItem(string key, IEnumerable<FinderRaceHorseItem> group)
    {
      this.GroupKey = key;

      foreach (var item in group)
      {
        this.Items.Add(item);
      }

      this._result = new RaceHorseFinderResultAnalyzer(group.ToArray());
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

  public class FinderRaceHorseGroupExpandedData
  {
    public double Before3hAverage { get; }

    public double After3hAverage { get; }

    public FinderRaceHorseGroupExpandedData(IEnumerable<FinderRaceHorseItem> group)
    {
      this.Before3hAverage = group.Select(i => (double)i.Analyzer.Race.BeforeHaronTime3).Where(v => v != default).Average() / 10;
      this.After3hAverage = group.Select(i => (double)i.Analyzer.Race.AfterHaronTime3).Where(v => v != default).Average() / 10;
    }
  }
}
