using KmyKeiba.Data.Db;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Image;
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

    public RaceCorner? Corner1 { get; private set; }

    public RaceCorner? Corner2 { get; private set; }

    public RaceCorner? Corner3 { get; private set; }

    public RaceCorner? Corner4 { get; private set; }

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

      this.Corner1 = this.AddCorner(this.Data.Corner1Result, this.Data.Corner1Number, this.Data.Corner1Position, this.Data.Corner1LapTime);
      this.Corner2 = this.AddCorner(this.Data.Corner2Result, this.Data.Corner2Number, this.Data.Corner2Position, this.Data.Corner2LapTime);
      this.Corner3 = this.AddCorner(this.Data.Corner3Result, this.Data.Corner3Number, this.Data.Corner3Position, this.Data.Corner3LapTime);
      this.Corner4 = this.AddCorner(this.Data.Corner4Result, this.Data.Corner4Number, this.Data.Corner4Position, this.Data.Corner4LapTime);
    }

    private RaceCorner? AddCorner(string result, int num, int pos, TimeSpan lap)
    {
      if (string.IsNullOrWhiteSpace(result))
      {
        return null;
      }

      var corner = RaceCorner.FromString(result);
      corner.Number = num;
      corner.Position = pos;
      corner.LapTime = lap;

      return corner;
    }
  }
}
