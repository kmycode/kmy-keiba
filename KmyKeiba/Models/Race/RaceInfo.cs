using KmyKeiba.Data.Db;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race
{
  internal class RaceInfo
  {
    private readonly MyContext db;
    private bool hasInitialized;

    public string Key { get; }

    public RaceData? Data { get; private set; }

    public RaceInfo(MyContext db, string key)
    {
      this.Key = key;
      this.db = db;
    }

    public async Task InitializeAsync()
    {
      if (this.hasInitialized)
      {
        return;
      }
      this.hasInitialized = true;

      this.Data = await this.db.Races!.FirstOrDefaultAsync(r => r.Key == this.Key);

      if (this.Data == null)
      {
        return;
      }

      var groups = RaceHorsePassingOrder.FromString(this.Data.Corner1Result);
    }
  }
}
