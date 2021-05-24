using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.JVLink.Wrappers;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KmyKeiba.Data.DataObjects
{
  public class RaceDataObject
  {
    public RaceData Data { get; private set; }

    public ReactiveProperty<RaceSubject> Subject { get; } = new();

    public ReactiveProperty<string> ShorterName { get; } = new(string.Empty);

    public ObservableCollection<RaceHorseDataObject> Horses { get; } = new();

    public ReactiveProperty<string> DisplayName { get; } = new(string.Empty);

    public ReactiveProperty<bool> HasResults { get; } = new();

    public ReactiveProperty<bool> IsCanceled { get; } = new();

    public void SetEntity(Race race)
    {
      this.Data.SetEntity(race);

      this.ReadData();
    }

    private void ReadData()
    {
      this.IsCanceled.Value = this.Data.DataStatus == RaceDataStatus.Aborted;

      this.Subject.Value = RaceSubject.Parse(this.Data.SubjectName);

      this.Subject.Value.Grade = this.Data.Grade;
      if (this.Data.SubjectAge2 != RaceSubjectType.Unknown)
      {
        this.Subject.Value.AgeSubjects.Add(new RaceSubject.SubjectTypeItem
        {
          Age = 2,
          Type = this.Data.SubjectAge2,
        });
      }
      if (this.Data.SubjectAge3 != RaceSubjectType.Unknown)
      {
        this.Subject.Value.AgeSubjects.Add(new RaceSubject.SubjectTypeItem
        {
          Age = 3,
          Type = this.Data.SubjectAge3,
        });
      }
      if (this.Data.SubjectAge4 != RaceSubjectType.Unknown)
      {
        this.Subject.Value.AgeSubjects.Add(new RaceSubject.SubjectTypeItem
        {
          Age = 4,
          Type = this.Data.SubjectAge4,
        });
      }
      if (this.Data.SubjectAge5 != RaceSubjectType.Unknown)
      {
        this.Subject.Value.AgeSubjects.Add(new RaceSubject.SubjectTypeItem
        {
          Age = 5,
          Type = this.Data.SubjectAge5,
        });
      }
      if (this.Data.SubjectAgeYounger != RaceSubjectType.Unknown)
      {
        this.Subject.Value.AgeSubjects.Add(new RaceSubject.SubjectTypeItem
        {
          Age = 6,
          Type = this.Data.SubjectAgeYounger,
        });
      }

      if ((short)this.Data.Course >= 30)
      {
        if (this.Subject.Value.Grade == RaceGrade.Grade1)
        {
          this.Subject.Value.Grade = RaceGrade.LocalGrade1;
        }
        if (this.Subject.Value.Grade == RaceGrade.Grade2)
        {
          this.Subject.Value.Grade = RaceGrade.LocalGrade2;
        }
        if (this.Subject.Value.Grade == RaceGrade.Grade3)
        {
          this.Subject.Value.Grade = RaceGrade.LocalGrade3;
        }
      }

      if (!string.IsNullOrWhiteSpace(this.Data.Name))
      {
        this.DisplayName.Value = this.Data.Name;
      }
      else if (!string.IsNullOrWhiteSpace(this.Data.SubjectName))
      {
        this.DisplayName.Value = this.Data.SubjectName;
      }
      else
      {
        this.DisplayName.Value = this.Subject.Value.ToString();
      }
      this.DisplayName.Value = new Regex(@"(\s|　)[\s　]+").Replace(this.DisplayName.Value, "　");
      this.ShorterName.Value = this.DisplayName.Value.Substring(0, Math.Min(6, this.DisplayName.Value.Length));
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
      if (!horses.Any())
      {
        return;
      }

      var maxTime = horses.Max((h) => h.ResultTime).TotalMilliseconds;
      var minTimeArray = horses
        .Where((h) => h.ResultTime != default);
      var minTime = minTimeArray.Any() ? minTimeArray.Min((h) => h.ResultTime).TotalMilliseconds : 0;
      var diffTime = Math.Max(maxTime - minTime, 1);

      var newHorses = horses
        .Where((h) => h.RaceKey == this.Data.Key)
        .OrderBy((h) => h.Number)
        .Select((h) => {
          var time = h.ResultTime;
          return new RaceHorseDataObject(h)
          {
            TimeRate = { Value = time == default ? 0 : 1 - ((float)h.ResultTime.TotalMilliseconds - minTime) / diffTime },
          };
        });
      foreach (var horse in newHorses)
      {
        horse.Race.Value = this;
        this.Horses.Add(horse);
      }

      if (this.Horses.All((h) => h.TimeRate.Value == 1))
      {
        foreach (var horse in this.Horses)
        {
          horse.TimeRate.Value = 0;
        }
      }

      this.HasResults.Value = this.Horses.Any((h) => h.Data.ResultOrder > 0);
    }

    public void SetHorses(IEnumerable<RaceHorseDataObject> horses)
    {
      this.Horses.Clear();
      var newHorses = horses
        .Where((h) => h.Data.RaceKey == this.Data.Key)
        .OrderBy((h) => h.Data.Number);
      foreach (var horse in newHorses)
      {
        this.Horses.Add(horse);
      }
    }

    public async Task SetRaceHorsesAsync(MyContextBase db, int nest = 1)
    {
      var horses = await db.RaceHorses!
        .Where((h) => h.RaceKey == this.Data.Key)
        .ToArrayAsync();
      this.SetHorses(horses);

      foreach (var horse in this.Horses)
      {
        // 出走馬の過去のレースを取得
        var sameHorses = await db.RaceHorses!
          .Where((h) => h.Name == horse.Data.Name)
          .ToArrayAsync();
        var raceKeys = sameHorses.Select((h) => h.RaceKey).ToList();
        var horseRaces = db.Races!
          .Where((r) => raceKeys.Contains(r.Key) && r.StartTime < this.Data.StartTime)
          .OrderByDescending((r) => r.StartTime);

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

        // 騎手の成績を取得
        var sameRider = sameHorses
          .Where((h) => h.RiderCode == horse.Data.RiderCode)
          .Join(horseRaces.Where((r) => r.StartTime < this.Data.StartTime), (h) => h.RaceKey, (r) => r.Key, (h, r) => h)
          .ToArray();
        horse.RiderFirst.Value = sameRider.Count((h) => h.ResultOrder == 1);
        horse.RiderSecond.Value = sameRider.Count((h) => h.ResultOrder == 2);
        horse.RiderThird.Value = sameRider.Count((h) => h.ResultOrder == 3);
        horse.RiderFourthAndWorse.Value = sameRider.Count((h) => h.ResultOrder >= 4);

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
