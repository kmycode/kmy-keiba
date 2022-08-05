using KmyKeiba.Models.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.Finder
{
  internal static class RaceFinderViewExtensions
  {
    public static IReadOnlyList<FinderRow> ToFinderRows(this IEnumerable<RaceAnalyzer> data, IEnumerable<FinderColumnDefinition<FinderRaceItem>> columns)
    {
      var rows = data.Select(d => new FinderRaceItem(d))
        .Select(a => new { Cells = columns.Select(c => c.CreateCell(a)).ToArray(), Race = a.Analyzer, });
      return rows.Select(r => new FinderRow(r.Cells, r.Race, null)).ToArray();
    }

    public static IReadOnlyList<FinderRow> ToFinderRows(this IEnumerable<RaceHorseAnalyzer> data, IEnumerable<FinderColumnDefinition<FinderRaceHorseItem>> columns)
    {
      var rows = data.Select(d => new FinderRaceHorseItem(d))
        .Select(a => new { Cells = columns.Select(c => c.CreateCell(a)).ToArray(), Horse = a.Analyzer, });
      return rows.Select(r => new FinderRow(r.Cells, null, r.Horse)).ToArray();
    }
  }
}
