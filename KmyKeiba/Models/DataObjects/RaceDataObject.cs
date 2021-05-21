using KmyKeiba.JVLink.Entities;
using KmyKeiba.JVLink.Wrappers;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KmyKeiba.Models.DataObjects
{
  public class RaceDataObject
  {
    public RaceData Data { get; private set; }

    public ReactiveProperty<RaceSubject> Subject { get; } = new();

    public ReactiveProperty<string> ShorterName { get; } = new(string.Empty);

    public ObservableCollection<RaceHorseDataObject> Horses { get; } = new();

    public void SetEntity(Race race)
    {
      this.Data.SetEntity(race);

      this.ReadData();
    }

    private void ReadData()
    {
      this.Subject.Value = RaceSubject.Parse(this.Data.SubjectName);
      this.ShorterName.Value = new Regex(@"(\s|　)[\s　]+").Replace(this.Data.Name, "　");
    }

    public RaceDataObject()
    {
      this.Data = new();
    }

    public RaceDataObject(RaceData data)
    {
      this.Data = data;
      this.ReadData();
    }

    public RaceDataObject(Race entity)
    {
      this.Data = new();
      this.SetEntity(entity);
    }

    public void SetHorses(IEnumerable<RaceHorseData> horses)
    {
      this.Horses.Clear();
      this.Horses.AddRange(horses
        .Where((h) => h.RaceKey == this.Data.Key)
        .OrderBy((h) => h.Number)
        .Select((h) => new RaceHorseDataObject(h)));
    }

    public void SetHorses(IEnumerable<RaceHorseDataObject> horses)
    {
      this.Horses.Clear();
      this.Horses.AddRange(horses
        .Where((h) => h.Data.RaceKey == this.Data.Key)
        .OrderBy((h) => h.Data.Number));
    }

    public async Task SetRaceHorsesAsync(MyContext db, int nest = 1)
    {
      var horses = await db.RaceHorses!
        .Where((h) => h.RaceKey == this.Data.Key)
        .ToArrayAsync();
      this.SetHorses(horses);

      // 出走馬の過去のレースを取得
      foreach (var horse in this.Horses)
      {
        var sameHorses = await db.RaceHorses!
          .Where((h) => h.Name == horse.Data.Name)
          .ToArrayAsync();
        var raceKeys = sameHorses.Select((h) => h.RaceKey).ToList();
        var horseRaces = await db.Races!
          .Where((r) => raceKeys.Contains(r.Key) && r.StartTime < this.Data.StartTime)
          .ToArrayAsync();

        var sameHorseObjects = new List<RaceHorseDataObject>();
        foreach (var sameHorse in sameHorses)
        {
          var horseRace = horseRaces.FirstOrDefault((r) => r.Key == sameHorse.RaceKey);
          if (horseRace != null)
          {
            var obj = new RaceHorseDataObject(sameHorse);
            obj.Race.Value = new RaceDataObject(horseRace);
            sameHorseObjects.Add(obj);
          }
        }

        horse.SetOldRaceHorses(sameHorseObjects);

        // ネスト
        if (nest > 1)
        {
          foreach (var sameHorseRace in horse.OldRaceHorses.Select((h) => h.Race.Value))
          {
            await sameHorseRace.SetRaceHorsesAsync(db, nest - 1);
          }
        }
      }
    }
  }
}
