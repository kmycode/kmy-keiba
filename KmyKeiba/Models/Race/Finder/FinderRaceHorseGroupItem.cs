using KmyKeiba.Data.Db;
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

    public double PlaceBetsRecoveryRate { get; }

    public double QuinellaPlaceRecoveryRate { get; }

    public FinderRaceHorseGroupItem(string key, IEnumerable<FinderRaceHorseItem> group)
    {
      this.GroupKey = key;
      this.Grades = new ResultOrderGradeMap(group.Select(g => g.Analyzer).ToArray());

      foreach (var item in group)
      {
        this.Items.Add(item);
      }

      if (group.Any())
      {
        this.PlaceBetsRecoveryRate = group.Sum(g => g.PlaceBetsPayoff) / ((double)group.Count() * 100);
        this.QuinellaPlaceRecoveryRate = group.Sum(g => g.QuinellaPlacePayoff) /
          (double)(group.Sum(g => g.Analyzer.Race.ResultHorsesCount > 0 ? g.Analyzer.Race.ResultHorsesCount : g.Analyzer.Race.HorsesCount) * 100);
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
