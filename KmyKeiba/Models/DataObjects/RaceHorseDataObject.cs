using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Data;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.DataObjects
{
  public class RaceHorseDataObject
  {
    public RaceHorseData Data { get; private set; }

    public ReactiveProperty<RaceDataObject> Race { get; } = new();

    public ObservableCollection<RaceHorseDataObject> OldRaceHorses { get; } = new();

    public void SetEntity(RaceHorse entity)
    {
      this.Data.Name = entity.Name;
      this.Data.Number = entity.Number;
      this.Data.Popular = entity.Popular;
      this.Data.RaceKey = entity.RaceKey;
      this.Data.ResultOrder = entity.ResultOrder;
      this.Data.FrameNumber = entity.FrameNumber;
    }

    public RaceHorseDataObject()
    {
      this.Data = new();
    }

    public RaceHorseDataObject(RaceHorseData data)
    {
      this.Data = data;
    }

    public RaceHorseDataObject(RaceHorse entity)
    {
      this.Data = new();
      this.SetEntity(entity);
    }

    public void SetOldRaceHorses(IEnumerable<RaceHorseDataObject> horses)
    {
      this.OldRaceHorses.Clear();
      this.OldRaceHorses.AddRange(horses
        .OrderByDescending((h) => h.Data.RaceKey));
    }
  }
}
