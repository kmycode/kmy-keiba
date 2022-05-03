using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Image;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race
{
  public class RaceInfo
  {
    public RaceData? Data { get; }

    public ObservableCollection<RaceHorseInfo> Horses { get; } = new();

    public ObservableCollection<RaceCorner> Corners { get; } = new();

    public RaceHorsePassingOrderImage ResultMap { get; } = new();

    public RaceSubjectInfo Subject { get; }

    public string Name => this.Subject.DisplayName;

    private RaceInfo(RaceData race, IReadOnlyList<RaceHorseData> horses)
    {
      this.Data = race;
      this.Subject = new(race);
      this.ResultMap.Groups = RaceCorner.GetGroupListFromResult(horses);
    }

    public static async Task<RaceInfo?> FromKeyAsync(MyContext db, string key)
    {
      var race = await db.Races!.FirstOrDefaultAsync(r => r.Key == key);
      if (race == null)
      {
        return null;
      }

      var horses = await db.RaceHorses!.Where(rh => rh.RaceKey == key).ToArrayAsync();

      var info = new RaceInfo(race, horses);
      AddCorner(info.Corners, race.Corner1Result, race.Corner1Number, race.Corner1Position, race.Corner1LapTime);
      AddCorner(info.Corners, race.Corner2Result, race.Corner2Number, race.Corner2Position, race.Corner2LapTime);
      AddCorner(info.Corners, race.Corner3Result, race.Corner3Number, race.Corner3Position, race.Corner3LapTime);
      AddCorner(info.Corners, race.Corner4Result, race.Corner4Number, race.Corner4Position, race.Corner4LapTime);

      return info;
    }

    private static void AddCorner(IList<RaceCorner> corners, string result, int num, int pos, TimeSpan lap)
    {
      if (string.IsNullOrWhiteSpace(result))
      {
        return;
      }

      var corner = RaceCorner.FromString(result);
      corner.Number = num;
      corner.Position = pos;
      corner.LapTime = lap;

      corners.Add(corner);
    }
  }
}
