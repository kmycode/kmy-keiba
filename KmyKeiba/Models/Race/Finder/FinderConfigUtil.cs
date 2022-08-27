using KmyKeiba.Data.Db;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.Finder
{
  internal static class FinderConfigUtil
  {
    private static bool _isInitialized;

    public static ReactiveCollection<FinderConfigData> Configs { get; } = new();

    public static async Task InitializeAsync(MyContext db)
    {
      if (!_isInitialized)
      {
        var configs = await db.FinderConfigs!.ToArrayAsync();
        foreach (var config in configs)
        {
          Configs.Add(config);
        }
        _isInitialized = true;
      }
    }

    public static async Task AddAsync(MyContext db, FinderConfigData config)
    {
      await db.FinderConfigs!.AddAsync(config);
      await db.SaveChangesAsync();
      Configs.Add(config);
    }

    public static async Task RemoveAsync(MyContext db, FinderConfigData config)
    {
      db.FinderConfigs!.Remove(config);
      await db.SaveChangesAsync();
      Configs.Remove(config);
    }
  }
}
