using KmyKeiba.JVLink.Entities;
using KmyKeiba.JVLink.Wrappers;
using KmyKeiba.Models.Data;
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

    public ReactiveProperty<string> CourseName { get; } = new(string.Empty);

    public ReactiveProperty<string> ShorterName { get; } = new(string.Empty);

    public ObservableCollection<RaceHorseDataObject> Horses { get; } = new();

    public void SetEntity(Race race)
    {
      this.Data.Key = race.Key;
      this.Data.Name = race.Name;
      this.Data.SubName = race.SubName;
      this.Data.SubjectName = race.Subject.Name;
      this.Data.Course = race.Course;
      this.Data.CourseRaceNumber = race.CourseRaceNumber;
      this.Data.HorsesCount = race.HorsesCount;
      this.Data.StartTime = race.StartTime;

      this.ReadData();
    }

    private void ReadData()
    {
      this.Subject.Value = RaceSubject.Parse(this.Data.SubjectName);
      this.CourseName.Value = this.Data.Course.GetName();
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
  }
}
