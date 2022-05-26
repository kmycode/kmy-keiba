using KmyKeiba.JVLink.Entities;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Data
{
  class PredictRunningStyleModel
  {
    private readonly ClusteringModel ml = new();

    public ReactiveProperty<bool> IsProcessing { get; } = new();

    public ReactiveProperty<bool> IsError { get; } = new();

    public ReactiveProperty<bool> CanPredict { get; } = new();

    public ReactiveProperty<int> ProcessCount { get; } = new(1);

    public ReactiveProperty<int> Processed { get; } = new();

    public void OpenFile(string fileName)
    {
      this.IsError.Value = false;

      try
      {
        this.IsProcessing.Value = true;
        this.ml.LoadFile(fileName);
        this.CanPredict.Value = this.ml.CanSave;
      }
      catch
      {
        this.IsError.Value = true;
      }
      finally
      {
        this.IsProcessing.Value = false;
      }
    }

    public void SaveFile(string fileName)
    {
      this.IsError.Value = false;

      try
      {
        this.IsProcessing.Value = true;
        this.ml.SaveFile(fileName);
      }
      catch
      {
        this.IsError.Value = true;
      }
      finally
      {
        this.IsProcessing.Value = false;
      }
    }

    public void Training()
    {
      this.IsError.Value = false;

      try
      {
        this.IsProcessing.Value = true;
        using (var db = new MyContext())
        {
          var targets = db.RaceHorses!
            .Where((h) => h.Course <= RaceCourse.CentralMaxValue && !h.IsRunningStyleSetManually &&
                          h.ResultOrder > 0 && h.RunningStyle != RunningStyle.Unknown)
            .Join(db.Races!, rh => rh.RaceKey, r => r.Key, (rh, r) => new { RaceHorse = rh, Race = r, })
            .OrderByDescending(d => d.Race.StartTime)
            .Take(100000)
            .ToArray()
            .Select(d => new ClusteringModel.RaceHorseDataInput
            {
              Race = d.Race,
              RaceHorse = d.RaceHorse,
            });
          this.ml.Training(targets);
        }
        this.CanPredict.Value = this.ml.CanSave;
      }
      catch
      {
        this.IsError.Value = true;
      }
      finally
      {
        this.IsProcessing.Value = false;
      }
    }

    public async Task<int> PredictAsync(int count)
    {
      this.IsError.Value = false;
      var done = 0;

      try
      {
        this.IsProcessing.Value = true;
        using (var db = new MyContext())
        {
          await db.BeginTransactionAsync();

          var targets = db.RaceHorses!.Where((h) => h.Course >= RaceCourse.CentralMaxValue &&
            !h.IsRunningStyleSetManually && h.RunningStyle == RunningStyle.Unknown &&
            h.ResultOrder > 0)
            .Join(db.Races!, rh => rh.RaceKey, r => r.Key, (rh, r) => new
            {
              Race = r,
              RaceHorse = rh,
            })
            .Take(count)
            .ToArray()
            .Select(d => new ClusteringModel.RaceHorseDataInput
            {
              Race = d.Race,
              RaceHorse = d.RaceHorse,
            });
          this.ProcessCount.Value = targets.Count();
          this.Processed.Value = 0;
          foreach (var horse in targets)
          {
            var result = this.ml.Predict(horse);
            horse.RaceHorse.RunningStyle = (RunningStyle)result;
            horse.RaceHorse.IsRunningStyleSetManually = true;
            this.Processed.Value++;
            done++;
          }
          await db.SaveChangesAsync();
          await db.CommitAsync();
        }
        this.CanPredict.Value = this.ml.CanSave;
      }
      catch
      {
        this.IsError.Value = true;
      }
      finally
      {
        this.IsProcessing.Value = false;
      }

      return done;
    }

    public void Reset()
    {
      this.IsError.Value = false;

      try
      {
        this.IsProcessing.Value = true;
        using (var db = new MyContext())
        {
          var targets = db.RaceHorses!.Where((h) => h.IsRunningStyleSetManually);
          this.ProcessCount.Value = targets.Count();
          this.Processed.Value = 0;
          foreach (var horse in targets)
          {
            horse.RunningStyle = RunningStyle.Unknown;
            horse.IsRunningStyleSetManually = false;
            this.Processed.Value++;
          }
          db.SaveChanges();
        }
        this.CanPredict.Value = this.ml.CanSave;
      }
      catch
      {
        this.IsError.Value = true;
      }
      finally
      {
        this.IsProcessing.Value = false;
      }
    }
  }
}
