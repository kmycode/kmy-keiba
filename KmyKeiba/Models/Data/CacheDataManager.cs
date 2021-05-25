using KmyKeiba.Data.DataObjects;
using KmyKeiba.Models.Threading;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Data
{
  static class CacheDataManager
  {
    private static string oldest;
    private static int skips = 0;

    private static readonly List<HorseRaceAnalyticsData> _data = new();
    public static IReadOnlyList<HorseRaceAnalyticsData> Cache => _data;

    public static event EventHandler? CacheAdded;

    private static bool isCacheing;

    static CacheDataManager()
    {
      var today = DateTime.Today;
      oldest = today.ToString("yyyyMMdd") + "00000000";
    }

    public static void BeginCache()
    {
      if (isCacheing)
      {
        return;
      }
      isCacheing = true;

      Task.Run(async () =>
      {
        while (true)
        {
          try
          {
            using (var db = new MyContext())
            {
              while (true)
              {
                try
                {
                  var adds = await db.RaceHorses!
                    .OrderByDescending((d) => string.Compare(d.RaceKey, oldest))
                    .Skip(skips)
                    .Take(500)
                    .Join(db.Races!, (h) => h.RaceKey, (r) => r.Key, (h, r) => new { Horse = h, Race = r, })
                    .ToArrayAsync();
                  if (adds.Any())
                  {
                    skips += 500;
                    oldest = adds.Min((d) => d.Race.Key)!;

                    foreach (var add in adds.Select((d) => new HorseRaceAnalyticsData(d.Horse, d.Race)))
                    {
                      _data.Add(add);
                    }

                    UiThreadUtil.Dispatcher?.Invoke(() =>
                    {
                      CacheAdded?.Invoke(null, new());
                    });

                    if (Cache.Count < 10000)
                    {
                      await Task.Delay(100);
                    }
                    else
                    {
                      await Task.Delay(1_000);
                    }
                  }
                  else
                  {
                    await Task.Delay(60_000);
                  }
                }
                catch
                {
                  await Task.Delay(3_000);
                }
              }
            }
          }
          catch
          {
          }
        }
      });
    }
  }
}
