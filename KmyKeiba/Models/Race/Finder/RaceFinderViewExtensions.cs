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
    public static IReadOnlyList<FinderRow> ToFinderRows(this IEnumerable<FinderRaceHorseItem> data, IEnumerable<FinderColumnDefinition<FinderRaceHorseItem>> columns)
    {
      var rows = data.Select(d => d)
        .Select(a => new { Cells = columns.Select(c => c.CreateCell(a)).ToArray(), Horse = a.Analyzer, });
      return rows.Select(r => new FinderRow(r.Cells, null, r.Horse)).ToArray();
    }
  }
}
