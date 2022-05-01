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
        await db.Database.MigrateAsync();
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
        Console.Write($"\rDWN [{loader.Downloaded.Value} / {loader.DownloadSize.Value}] LD [{loader.Loaded.Value} / {loader.LoadSize.Value}] ENT({loader.LoadEntityCount.Value}) PC [{loader.Processed.Value} / {loader.ProcessSize.Value}]");
        Task.Delay(100).Wait();
      }
    }

    private static async Task LoadAsync(JVLinkLoader loader)
    {
      for (var year = 2003; year <= 2022; year++)
      {
        for (var month = 1; month <= 12; month++)
        {
          if (year < 2009) continue;
          if (year == 2009 && month <= 5) continue;

          Console.WriteLine($"{year} 年 {month} 月");
          var start = new DateTime(year, month, 1);
          await loader.LoadAsync(JVLinkObject.Central,
            JVLinkDataspec.Race | JVLinkDataspec.Blod | JVLinkDataspec.Diff | JVLinkDataspec.Slop | JVLinkDataspec.Toku,
            JVLinkOpenOption.Setup,
            raceKey: null,
            startTime: start,
            endTime: start.AddMonths(1),
            loadSpecs: new string[] { "RA", "SE", "WH", "WE", "AV", "UM", "HN", "JC", "HC", "HR", });
          Console.WriteLine();
          Console.WriteLine();
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