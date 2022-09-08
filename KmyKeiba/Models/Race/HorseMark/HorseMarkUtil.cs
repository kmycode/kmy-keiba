using KmyKeiba.Data.Db;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.HorseMark
{
  public static class HorseMarkUtil
  {
    private static bool _isInitialized;

    public static List<HorseMarkConfigData> Configs { get; } = new();

    public static async Task InitializeAsync(MyContext db)
    {
      if (!_isInitialized)
      {
        var configs = await db.HorseMarkConfigs!.OrderBy(c => c.Order).ToArrayAsync();
        foreach (var config in configs)
        {
          Configs.Add(config);
        }

        _isInitialized = true;
      }
    }
  }
}
