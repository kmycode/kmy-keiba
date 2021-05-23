using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.JVLink.Wrappers;
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

    public ReactiveProperty<byte[]> Uniform { get; } = new();

    public ReactiveProperty<int> RiderFirst { get; } = new();

    public ReactiveProperty<int> RiderSecond { get; } = new();

    public ReactiveProperty<int> RiderThird { get; } = new();

    public ReactiveProperty<int> RiderFourthAndWorse { get; } = new();

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

    public async Task RequestUniformBitmapAsync(MyContextBase db)
    {
      if (this.Data.UniformFormatData == null || this.Data.UniformFormatData.Length <= 0)
      {
        // 服の画像を設定
        try
        {
          var link = this.Data.Course.GetCourseType() switch
          {
            RaceCourseType.Central => JVLinkObject.Central,
            RaceCourseType.Local => JVLinkObject.Local,
            _ => JVLinkObject.Local,
          };
          if (!link.IsError)
          {
            var buff = link.GetUniformBitmap(this.Data.UniformFormat);
            this.Data.UniformFormatData = buff;

            var horse = await db.RaceHorses!.FindAsync(this.Data.Id);
            if (horse != null)
            {
              horse.UniformFormatData = buff;
              await db.SaveChangesAsync();
            }
          }
        }
        catch (Exception ex)
        {
        }
      }

      this.Uniform.Value = this.Data.UniformFormatData!;
    }
  }
}
