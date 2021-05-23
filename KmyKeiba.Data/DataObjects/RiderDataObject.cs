using KmyKeiba.Data.Db;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.DataObjects
{
  public class RiderDataObject
  {
    public RiderData Data { get; } = new();

    public ObservableCollection<RaceHorseDataObject> RecentRaces { get; } = new();

    public RiderGrades AllGrades { get; } = new();

    public RiderGrades CentralGrades { get; } = new();

    public RiderGrades LocalGrades { get; } = new();

    public static async Task<RiderDataObject> CreateAsync(MyContextBase db, string code)
    {
      var obj = new RiderDataObject();

      obj.Data.Code = code;

      var horses = db.RaceHorses!.Where((h) => h.RiderCode == code);
      if (horses.Any())
      {
        var first = await horses.FirstAsync();
        obj.Data.Name = first.RiderName;
      }

      var horsesWithRaces = horses.Join(db.Races!, (h) => h.RaceKey, (r) => r.Key, (h, r) => new { Horse = h, Race = r, });
      foreach (var item in horsesWithRaces.OrderByDescending((i) => i.Race.StartTime).Take(72))
      {
        var ho = new RaceHorseDataObject(item.Horse);
        ho.Race.Value = new RaceDataObject(item.Race);
        obj.RecentRaces.Add(ho);
      }

      var centrals = horsesWithRaces.Where((h) => (short)h.Race.Course < 30);
      var locals = horsesWithRaces.Where((h) => (short)h.Race.Course >= 30);

      await obj.CentralGrades.SetCountsAsync(centrals.Select((h) => h.Horse));
      await obj.LocalGrades.SetCountsAsync(locals.Select((h) => h.Horse));
      obj.AllGrades.AllCount = obj.CentralGrades.AllCount + obj.LocalGrades.AllCount;
      obj.AllGrades.First = obj.CentralGrades.First + obj.LocalGrades.First;
      obj.AllGrades.Second = obj.CentralGrades.Second + obj.LocalGrades.Second;
      obj.AllGrades.Third = obj.CentralGrades.Third + obj.LocalGrades.Third;
      obj.AllGrades.Fourth = obj.CentralGrades.Fourth + obj.LocalGrades.Fourth;
      obj.AllGrades.Fifth = obj.CentralGrades.Fifth + obj.LocalGrades.Fifth;
      obj.AllGrades.SixthAndWorse = obj.CentralGrades.SixthAndWorse + obj.LocalGrades.SixthAndWorse;

      return obj;
    }
  }

  public class RiderGrades
  {
    public int AllCount { get; set; }

    public int First { get; set; }

    public float FirstRate => this.AllCount == 0 ? 0 : (float)this.First / this.AllCount;

    public int Second { get; set; }

    public float SecondRate => this.AllCount == 0 ? 0 : (float)this.Second / this.AllCount;

    public int Third { get; set; }

    public float ThirdRate => this.AllCount == 0 ? 0 : (float)this.Third / this.AllCount;

    public float ThirdRatePercentage => this.ThirdRate * 100;

    public int Fourth { get; set; }

    public int Fifth { get; set; }

    public int SixthAndWorse { get; set; }

    public int FourthAndWorse { get; set; }

    public async Task SetCountsAsync(IQueryable<RaceHorseData> horses)
    {
      this.First = await horses.CountAsync((h) => h.ResultOrder == 1);
      this.Second = await horses.CountAsync((h) => h.ResultOrder == 2);
      this.Third = await horses.CountAsync((h) => h.ResultOrder == 3);
      this.Fourth = await horses.CountAsync((h) => h.ResultOrder == 4);
      this.Fifth = await horses.CountAsync((h) => h.ResultOrder == 5);
      this.SixthAndWorse = await horses.CountAsync((h) => h.ResultOrder <= 6);
      this.FourthAndWorse = this.SixthAndWorse + this.Fourth + this.Fifth;
      this.AllCount = this.FourthAndWorse + this.First + this.Second + this.Third;
    }
  }
}
