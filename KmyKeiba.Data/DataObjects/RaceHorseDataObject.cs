using KmyKeiba.Data.Db;
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

    public ObservableCollection<HorseAnalyticsResult> AnalyticsResults { get; } = new();

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

    public async Task AnalyticsAsync(string connectionString, string dbName, IEnumerable<IHorseAnalyticsGroup> groups,
      IEnumerable<IHorseAnalyticsFilter> filters)
    {
      this.AnalyticsResults.Clear();

      var connection = new MySqlConnection(connectionString);
      await connection.OpenAsync();

      using (var cmd = connection.CreateCommand())
      {
        cmd.CommandText = $"USE `{dbName}`";
        await cmd.ExecuteNonQueryAsync();
      }

      foreach (var group in groups)
      {
        /*
        var data = group.CreateAnalyticsGroup(db.RaceHorses!, this);
        foreach (var filter in filters.Where((f) => f.IsEnabled.Value))
        {
          if (filter is IHorseOnlyAnalyticsFilter ho)
          {
            data = ho.Filtering(data, this);
          }
          else if (filter is IHorseRaceAnalyticsFilter hr)
          {
            data = hr.Filtering(data, db.Races!, this);
          }
        }
        */
        var whereQuery = new List<string>();
        whereQuery.Add(group.CreateAnalyticsGroup(this));
        foreach (var filter in filters.Where((f) => f.IsEnabled.Value))
        {
          whereQuery.Add(filter.Filtering(this));
        }

        async Task<int> GetCountQueryAsync(int order)
        {
          /*SELECT COUNT(*) FROM
	(SELECT g.Id FROM (SELECT * FROM racehorses 
	   ORDER BY RaceKey DESC LIMIT 30000) AS g
		INNER JOIN Races ON g.RaceKey LIKE Races.Key
		WHERE g.CourseCode = 36)
	AS c;*/
          var subject = string.Join(" AND ", whereQuery!);
          if (order != 0)
          {
            if (string.IsNullOrEmpty(subject))
            {
              subject = $"Horse.ResultOrder = {order}";
            }
            else
            {
              subject = subject + $" AND Horse.ResultOrder = {order}";
            }
          }
          var q =  @$"SELECT COUNT(*) FROM (SELECT Horse.Id FROM (SELECT * FROM RaceHorses
ORDER BY RaceKey DESC LIMIT 20000) AS Horse
INNER JOIN Races ON Horse.RaceKey LIKE Races.Key WHERE {subject}) AS c";

          using var cmd = connection.CreateCommand();
          cmd.CommandText = q;
          cmd.CommandTimeout = 90;

          using (var reader = await cmd.ExecuteReaderAsync())
          {
            await reader.ReadAsync();
            var count = reader.GetValue(0).ToString();
            int.TryParse(count, out int c);

            return c;
          }
        }

        var result = new HorseAnalyticsResult(group)
        {
          AllCount = await GetCountQueryAsync(0),
          First = await GetCountQueryAsync(1),
          Second = await GetCountQueryAsync(2),
          Third = await GetCountQueryAsync(3),
        };
        this.AnalyticsResults.Add(result);
      }
    }
  }

  public interface IHorseAnalyticsGroup
  {
    public string Label { get; }

    // IQueryable<RaceHorseData> CreateAnalyticsGroup(IQueryable<RaceHorseData> horses, RaceHorseDataObject horse);

    string CreateAnalyticsGroup(RaceHorseDataObject horse);
  }

  public interface IHorseAnalyticsFilter
  {
    string Label { get; }

    ReactiveProperty<bool> IsEnabled { get; }

    string Filtering(RaceHorseDataObject horse);
  }

  public interface IHorseOnlyAnalyticsFilter : IHorseAnalyticsFilter
  {
    IQueryable<RaceHorseData> Filtering(IQueryable<RaceHorseData> query, RaceHorseDataObject horse);
  }

  public interface IHorseRaceAnalyticsFilter : IHorseAnalyticsFilter
  {
    IQueryable<RaceHorseData> Filtering(IQueryable<RaceHorseData> query, IQueryable<RaceData> races, RaceHorseDataObject horse);
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
