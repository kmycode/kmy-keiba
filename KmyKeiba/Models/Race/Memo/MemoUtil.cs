using KmyKeiba.Data.Db;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.Memo
{
  internal static class MemoUtil
  {
    public static async Task<(IReadOnlyList<MemoData>, PointLabelData?)> GetRaceListMemosAsync(MyContext? db, IEnumerable<string> raceKeys)
    {
      var isDbNull = false;
      if (db == null)
      {
        isDbNull = true;
        db = new();
      }

      var memoConfig = await db.MemoConfigs!
        .Where(m => m.Target1 == MemoTarget.Race && m.Target2 == MemoTarget.Unknown && m.Target3 == MemoTarget.Unknown && m.Type == MemoType.Race && m.PointLabelId != default)
        .Where(m => m.Style == MemoStyle.Point || m.Style == MemoStyle.MemoAndPoint)
        .Join(db.PointLabels!, m => (uint)m.PointLabelId, p => p.Id, (m, p) => new { Memo = m, PointLabel = p, })
        .OrderBy(m => m.Memo.Order)
        .FirstOrDefaultAsync();
      var memos = Array.Empty<MemoData>();
      PointLabelData? pointLabel = null;
      if (memoConfig != null)
      {
        var number = memoConfig.Memo.MemoNumber;
        var memoData = await db.Memos!
          .Where(m => m.Target1 == MemoTarget.Race && m.Target2 == MemoTarget.Unknown && m.Target3 == MemoTarget.Unknown && m.Number == number)
          .Where(m => raceKeys.Contains(m.Key1))
          .ToArrayAsync();
        if (memoData.Any())
        {
          memos = memoData;
          pointLabel = memoConfig.PointLabel;
        }
      }

      if (isDbNull)
      {
        db.Dispose();
      }

      return (memos, pointLabel);
    }
  }
}
