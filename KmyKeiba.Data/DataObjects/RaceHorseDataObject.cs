using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.DataObjects
{
  public class RaceHorseDataObject
  {
    public RaceHorseData Data { get; private set; }

    public ReactiveProperty<RaceDataObject> Race { get; } = new();

    public ObservableCollection<RaceHorseDataObject> OldRaceHorses { get; } = new();

    public ReactiveProperty<double> TimeRate { get; } = new();

    public ReactiveProperty<RaceHorseMark> Mark { get; } = new();

    public void SetEntity(RaceHorse entity)
    {
      this.Data.SetEntity(entity);
    }

    public RaceHorseDataObject()
    {
      this.Data = new();
      this.Initialize();
    }

    public RaceHorseDataObject(RaceHorseData data)
    {
      this.Data = data;
      this.Initialize();
    }

    public RaceHorseDataObject(RaceHorse entity)
    {
      this.Data = new();
      this.SetEntity(entity);
      this.Initialize();
    }

    private void Initialize()
    {
      this.Mark.Value = this.Data.Mark;
      this.Mark.Subscribe((m) => this.Data.Mark = m);
    }

    public void SetOldRaceHorses(IEnumerable<RaceHorseDataObject> horses)
    {
      this.OldRaceHorses.Clear();

      var newHorses = horses
        .OrderByDescending((h) => h.Data.RaceKey);
      foreach (var horse in newHorses)
      {
        this.OldRaceHorses.Add(horse);
      }
    }
  }
}
