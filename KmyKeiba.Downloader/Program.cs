using KmyKeiba.JVLink.Wrappers;
using Microsoft.EntityFrameworkCore;

namespace KmyKeiba.Downloader
{
  internal class Program
  {
    [STAThread]
    public static void Main(string[] args)
    {
      // マイグレーション
      Task.Run(async () =>
      {
        using var db = new MyContext();
        db.Database.SetCommandTimeout(1200);
        await db.Database.MigrateAsync();
      }).Wait();

      StartSetPreviousRaceDays();
      //StartLoad();
    }

    private static void StartLoad()
    {
      var loader = new JVLinkLoader();

      var isLoaded = false;
      Task.Run(async () =>
      {
        await LoadAsync(loader);
        isLoaded = true;

        loader.Dispose();
      });

      while (!isLoaded)
      {
        Console.Write($"\rDWN [{loader.Downloaded.Value} / {loader.DownloadSize.Value}] LD [{loader.Loaded.Value} / {loader.LoadSize.Value}] ENT({loader.LoadEntityCount.Value}) SV [{loader.Saved.Value} / {loader.SaveSize.Value}] PC [{loader.Processed.Value} / {loader.ProcessSize.Value}]");
        Task.Delay(100).Wait();
      }
    }

    private static async Task LoadAsync(JVLinkLoader loader)
    {
      // 2005  2011-
      for (var year = 1990; year <= 2022; year++)
      {
        break;

        Console.WriteLine($"{year} 年");
        await loader.LoadAsync(JVLinkObject.Central,
          // JVLinkDataspec.Race | JVLinkDataspec.Blod | JVLinkDataspec.Diff | JVLinkDataspec.Slop | JVLinkDataspec.Toku,
          // JVLinkDataspec.Race | JVLinkDataspec.Blod | JVLinkDataspec.Diff | JVLinkDataspec.Slop | JVLinkDataspec.Toku,
          JVLinkDataspec.Race,
          JVLinkOpenOption.Setup,
          raceKey: null,
          startTime: new DateTime(year, 1, 1),
          endTime: new DateTime(year + 1, 1, 1),
          // loadSpecs: new string[] { "RA", "SE", "WH", "WE", "AV", "UM", "HN", "JC", "HC", "HR", });
          loadSpecs: new string[] { "O1", "O2", "O3", "O4", "O5", "O6", });
        Console.WriteLine();
        Console.WriteLine();
      }


      var startYear = 2018;
      var startMonth = 11;

      for (var year = startYear; year <= 2022; year++)
      {
        for (var month = 1; month <= 12; month++)
        {
          if (year == startYear && month < startMonth)
          {
            continue;
          }

          var start = new DateTime(year, month, 1);

          var dates = new DateTime[]
          {
            // start, start.AddDays(15),
            // start.AddDays(15), start.AddMonths(1),
            start, start.AddMonths(1),
          };
          for (var i = 0; i < dates.Length / 2; i++)
          {
            //if (year < 2021) continue;
            //if (year == 2021 && month < 10) continue;
            //if (year == 2021 && month == 10 && i < 1) continue;
            // if (year == 2017 && month <= 3) continue;

            Console.WriteLine($"{year} 年 {month} 月 {dates[i * 2].Day} 日");
            await loader.LoadAsync(JVLinkObject.Central,
              // JVLinkDataspec.Race | JVLinkDataspec.Blod | JVLinkDataspec.Diff | JVLinkDataspec.Slop | JVLinkDataspec.Toku,
              JVLinkDataspec.Race,
              JVLinkOpenOption.Setup,
              raceKey: null,
              startTime: dates[i * 2],
              endTime: dates[i * 2 + 1],
              loadSpecs: new string[] { "O1", "O2", "O3", "O4", "O5", "O6", });
              // loadSpecs: new string[] { "RA", "SE", "WH", "WE", "AV", "UM", "HN", "JC", "HC", "HR", });
            Console.WriteLine();
            Console.WriteLine();
          }
        }
      }
      /*
      await loader.LoadAsync(JVLinkObject.Central,
        JVLinkDataspec.Race | JVLinkDataspec.Blod | JVLinkDataspec.Diff,
        JVLinkOpenOption.Normal,
        raceKey: null,
        startTime: DateTime.Today.AddMonths(-1),
        endTime: null);
      */
    }

    private static void StartSetPreviousRaceDays()
    {
      Task.Run(async () => await SetPreviousRaceDays()).Wait();
    }

    private static async Task SetPreviousRaceDays()
    {
      var count = 0;

      using (var db = new MyContext())
      {
        // 通常のクエリは３分まで
        db.Database.SetCommandTimeout(180);

        var allTargets = db.RaceHorses!
          .Where(rh => rh.PreviousRaceDays == 0)
          .Where(rh => rh.Key != "0000000000")
          .GroupBy(rh => rh.Key)
          .Select(g => g.Key);

        var targets = await allTargets.Take(96).ToArrayAsync();

        while (targets.Any())
        {
          var targetHorses = await db.RaceHorses!.Where(rh => targets.Contains(rh.Key)).ToArrayAsync();

          foreach (var horse in targets)
          {
            var races = targetHorses.Where(rh => rh.Key == horse).OrderBy(rh => rh.RaceKey);
            var beforeRaceDh = DateTime.MinValue;
            var isFirst = true;

            foreach (var race in races)
            {
              var y = race.RaceKey.Substring(0, 4);
              var m = race.RaceKey.Substring(4, 2);
              var d = race.RaceKey.Substring(6, 2);
              int.TryParse(y, out var year);
              int.TryParse(m, out var month);
              int.TryParse(d, out var day);
              var dh = new DateTime(year, month, day);

              if (!isFirst)
              {
                race.PreviousRaceDays = (short)(dh - beforeRaceDh).TotalDays;
              }
              else
              {
                race.PreviousRaceDays = -1;
                isFirst = false;
              }

              beforeRaceDh = dh;
              count++;
            }
          }

          await db.SaveChangesAsync();
          Console.Write($"\r完了: {count}");

          targets = await allTargets.Take(96).ToArrayAsync();
        }
      }
    }
  }
}