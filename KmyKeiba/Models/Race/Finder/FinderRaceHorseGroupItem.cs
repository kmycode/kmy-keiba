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

    public ReactiveCollection<FinderRow> Rows { get; } = new();

    public ReactiveCollection<FinderRaceHorseItem> Items { get; } = new();

    public ResultOrderGradeMap Grades { get; }

    private FinderRaceHorseGroupItem(string key, IEnumerable<RaceHorseAnalyzer> group)
    {
      this.GroupKey = key;
      this.Grades = new ResultOrderGradeMap(group.ToArray());

      foreach (var item in group)
      {
        this.Items.Add(new FinderRaceHorseItem(item));
      }
    }

    public FinderRaceHorseGroupItem(IEnumerable<RaceHorseAnalyzer> group) : this("デフォルト", group)
    {
    }

    public FinderRaceHorseGroupItem(IGrouping<string, RaceHorseAnalyzer> group) : this(group.Key, group)
    {
    }

    public FinderRaceHorseGroupItem(IGrouping<int, RaceHorseAnalyzer> group) : this(group.Key.ToString(), group)
    {
    }
  }
}
