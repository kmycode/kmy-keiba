using KmyKeiba.JVLink.Wrappers;
using Microsoft.EntityFrameworkCore;

namespace KmyKeiba.Downloader
{
  internal class Program
  {
    public static void Main(string[] args)
    {
      // マイグレーション
      Task.Run(async () =>
      {
        using var db = new MyContext();
        db.Database.SetCommandTimeout(1200);
        await db.Database.MigrateAsync();

        // 通常のクエリは３分まで
        db.Database.SetCommandTimeout(180);
      }).Wait();

      using var loader = new JVLinkLoader();

      var isLoaded = false;
      Task.Run(async () =>
      {
        await LoadAsync(loader);
        isLoaded = true;
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
      for (var year = 2018; year <= 2022; year++)
      {
        Console.WriteLine($"{year} 年");
        await loader.LoadAsync(JVLinkObject.Local,
          // JVLinkDataspec.Race | JVLinkDataspec.Blod | JVLinkDataspec.Diff | JVLinkDataspec.Slop | JVLinkDataspec.Toku,
          JVLinkDataspec.Race | JVLinkDataspec.Blod | JVLinkDataspec.Diff | JVLinkDataspec.Slop | JVLinkDataspec.Toku,
          JVLinkOpenOption.Normal,
          raceKey: null,
          startTime: new DateTime(year, 1, 1),
          endTime: new DateTime(year + 1, 1, 1),
          loadSpecs: new string[] { "RA", "SE", "WH", "WE", "AV", "UM", "HN", "JC", "HC", "HR", });
        Console.WriteLine();
        Console.WriteLine();
      }


      for (var year = 2017; year <= 2022; year++)
      {
        for (var month = 1; month <= 12; month++)
        {
          var start = new DateTime(year, month, 1);

          var dates = new DateTime[]
          {
            // start, start.AddDays(15),
            // start.AddDays(15), start.AddMonths(1),
            start, start.AddMonths(1),
          };
          for (var i = 0; i < /*dates.Length / 2*/ 1; i++)
          {
            //if (year < 2021) continue;
            //if (year == 2021 && month < 10) continue;
            //if (year == 2021 && month == 10 && i < 1) continue;
            if (year == 2017 && month <= 3) continue;

            Console.WriteLine($"{year} 年 {month} 月 {dates[i * 2].Day} 日");
            await loader.LoadAsync(JVLinkObject.Local,
              // JVLinkDataspec.Race | JVLinkDataspec.Blod | JVLinkDataspec.Diff | JVLinkDataspec.Slop | JVLinkDataspec.Toku,
              JVLinkDataspec.Race | JVLinkDataspec.Blod | JVLinkDataspec.Diff | JVLinkDataspec.Slop | JVLinkDataspec.Toku,
              JVLinkOpenOption.Setup,
              raceKey: null,
              startTime: dates[i * 2],
              endTime: dates[i * 2 + 1],
              loadSpecs: new string[] { "RA", "SE", "WH", "WE", "AV", "UM", "HN", "JC", "HC", "HR", });
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
  }
}