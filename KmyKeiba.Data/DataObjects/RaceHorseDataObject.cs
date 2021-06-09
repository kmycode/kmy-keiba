using KmyKeiba.Data.Db;
using KmyKeiba.Data.Wrappers;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.JVLink.Wrappers;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
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

    public ReactiveCollection<HorseAnalyticsResult> AnalyticsResults { get; } = new();

    public ReactiveProperty<RunningStyle> MajorRunningStyle { get; } = new();

    private void SetEntity(RaceHorse entity)
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

      if (horses.Any())
      {
        var groups = horses.GroupBy((h) => h.Data.RunningStyle);
        var max = groups.OrderBy((g) => g.Count()).First();
        this.MajorRunningStyle.Value = max.Key;
      }

      var newHorses = horses
        .OrderByDescending((h) => h.Data.RaceKey)
        .Take(10);
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
        /*
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
        */
        throw new NotSupportedException();
      }

      this.Uniform.Value = this.Data.UniformFormatData!;
    }

    public void Analytics(
      IEnumerable<HorseRaceAnalyticsData> cache,
      Func<IEnumerable<IHorseAnalyticsGroup>> groups,
      IEnumerable<IHorseAnalyticsFilter> filters)
    {
      var activeGroups = this.AnalyticsResults.Select((r) => r.Group).ToArray() as IEnumerable<IHorseAnalyticsGroup>;
      if (!activeGroups.Any())
      {
        activeGroups = groups();
      }

      {
        foreach (var group in activeGroups.Where((g) => g.GetCache(this) == null || g.GetCache(this)!.Count < 10000))
        {
          var data = group.CreateAnalyticsGroup(cache, this)
            .Where((h) => h.Race.StartTime < this.Race.Value.Data.StartTime)
            .OrderByDescending((h) => h.Race.StartTime)
            .Take(10000)
            .ToArray();
          group.SetCache(data
            .Select((d) => new HorseRaceAnalyticsData(d.Horse, d.Race))
            .ToArray(), this);
        }
      }

      this.AnalyticsResults.Clear();


      foreach (var group in activeGroups)
      {
        var data = group.GetCache(this) as IEnumerable<HorseRaceAnalyticsData>;
        if (data == null)
        {
          continue;
        }

        foreach (var filter in filters.Where((f) => f.IsEnabled.Value))
        {
          data = filter.Filtering(data, this);
        }

        var result = new HorseAnalyticsResult(group)
        {
          AllCount = data!.Count(),
          First = data!.Count((c) => c.Horse.ResultOrder == 1),
          Second = data!.Count((c) => c.Horse.ResultOrder == 2),
          Third = data!.Count((c) => c.Horse.ResultOrder == 3),
        };
        this.AnalyticsResults.Add(result);
      }
    }
  }

  public interface IHorseAnalyticsGroup
  {
    public string Label { get; }

    public IReadOnlyList<HorseRaceAnalyticsData>? GetCache(RaceHorseDataObject horse);

    public void SetCache(IReadOnlyList<HorseRaceAnalyticsData> cache, RaceHorseDataObject horse);

    // IQueryable<RaceHorseData> CreateAnalyticsGroup(IQueryable<RaceHorseData> horses, RaceHorseDataObject horse);

    // string CreateAnalyticsGroup(RaceHorseDataObject horse);

    IEnumerable<HorseRaceAnalyticsData> CreateAnalyticsGroup(IEnumerable<HorseRaceAnalyticsData> horses, RaceHorseDataObject horse);
  }

  public interface IHorseAnalyticsFilter
  {
    string Label { get; }

    ReactiveProperty<bool> IsEnabled { get; }

    IEnumerable<HorseRaceAnalyticsData> Filtering(IEnumerable<HorseRaceAnalyticsData> data, RaceHorseDataObject horse);
  }

  public interface IHorseOnlyAnalyticsFilter : IHorseAnalyticsFilter
  {
    IQueryable<RaceHorseData> Filtering(IQueryable<RaceHorseData> query, RaceHorseDataObject horse);
  }

  public interface IHorseRaceAnalyticsFilter : IHorseAnalyticsFilter
  {
    IQueryable<RaceHorseData> Filtering(IQueryable<RaceHorseData> query, IQueryable<RaceData> races, RaceHorseDataObject horse);
  }

  public class HorseRaceAnalyticsData
  {
    public RaceHorseData Horse { get; }

    public RaceData Race { get; }

    public HorseRaceAnalyticsData(RaceHorseData horse, RaceData race)
    {
      this.Horse = horse;
      this.Race = race;
    }
  }

  public class HorseAnalyticsResult
  {
    public IHorseAnalyticsGroup Group { get; init; }

    public int AllCount { get; init; }

    public int First { get; init; }

    public int Second { get; init; }

    public int Third { get; init; }

    public float ThirdRate => this.AllCount == 0 ? 0 : (float)(this.First + this.Second + this.Third) / this.AllCount;

    public float ThirdRatePercentage => this.ThirdRate * 100;

    internal HorseAnalyticsResult(IHorseAnalyticsGroup group)
    {
      this.Group = group;
    }
  }
}
