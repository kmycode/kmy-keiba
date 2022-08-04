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

    public ReactiveCollection<FinderRaceHorseItem> Items { get; } = new();

    private FinderRaceHorseGroupItem(string key, IEnumerable<RaceHorseAnalyzer> group)
    {
      this.GroupKey = key;

      foreach (var item in group)
      {
        this.Items.Add(new FinderRaceHorseItem(item));
      }
    }

    public FinderRaceHorseGroupItem(IGrouping<string, RaceHorseAnalyzer> group) : this(group.Key, group)
    {
    }

    public FinderRaceHorseGroupItem(IGrouping<int, RaceHorseAnalyzer> group) : this(group.Key.ToString(), group)
    {
    }
  }
}
