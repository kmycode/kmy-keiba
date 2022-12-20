using KmyKeiba.Data.Db;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race.ExNumber;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.AnalysisTable.Script
{
  internal static class AnalysisTableScriptUtil
  {
    public static List<AnalysisTableScriptData> Scripts { get; } = new();
    private static bool _isInitialized;

    public static async Task InitializeAsync(MyContext db)
    {
      if (!_isInitialized)
      {
        var configs = await db.AnalysisTableScripts!.ToArrayAsync();
        foreach (var config in configs)
        {
          Scripts.Add(config);
        }

        AnalysisTableScriptConfigModel.Default.Initialize();

        _isInitialized = true;
      }
    }
  }
}
