using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Data;
using KmyKeiba.ViewEvents;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Logics
{
  class PredictRunningStyleModel
  {
    private readonly ClusteringModel ml = new();

    public ReactiveProperty<bool> IsProcessing { get; } = new();

    public ReactiveProperty<bool> IsError { get; } = new();

    public ReactiveProperty<bool> CanPredict { get; } = new();

    public ReactiveProperty<int> ProcessCount { get; } = new(1);

    public ReactiveProperty<int> Processed { get; } = new();

    public event EventHandler<FileDialogCalledEventArgs>? FileDialogCalled;

    public void OpenFile()
    {
      this.IsError.Value = false;

      this.FileDialogCalled?.Invoke(this, new FileDialogCalledEventArgs
      {
        Type = FileDialogType.Open,
        Filter = "脚質学習データ|*.kkrs",
        OnCompleted = (fileName) =>
        {
          Task.Run(() =>
          {
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
          });
        },
      });
    }

    public void SaveFile()
    {
      this.IsError.Value = false;

      this.FileDialogCalled?.Invoke(this, new FileDialogCalledEventArgs
      {
        Type = FileDialogType.Save,
        Filter = "脚質学習データ|*.kkrs",
        OnCompleted = (fileName) =>
        {
          Task.Run(() =>
          {
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
          });
        },
      });
    }

    public void Training()
    {
      this.IsError.Value = false;

      Task.Run(() =>
      {
        try
        {
          this.IsProcessing.Value = true;
          using (var db = new MyContext())
          {
            var targets = db.RaceHorses!
            .Where((h) => (short)h.Course < 30 && !h.IsRunningStyleSetManually &&
                          h.ResultOrder > 0);
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
      });
    }

    public void Predict()
    {
      this.IsError.Value = false;

      Task.Run(() =>
      {
        try
        {
          this.IsProcessing.Value = true;
          using (var db = new MyContext())
          {
            var targets = db.RaceHorses!.Where((h) => (short)h.Course >= 30 &&
              !h.IsRunningStyleSetManually && h.RunningStyle == RunningStyle.Unknown &&
              h.ResultOrder > 0);
            this.ProcessCount.Value = targets.Count();
            this.Processed.Value = 0;
            foreach (var horse in targets)
            {
              var result = this.ml.Predict(horse);
              horse.RunningStyle = (RunningStyle)result;
              horse.IsRunningStyleSetManually = true;
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
      });
    }

    public void Reset()
    {
      this.IsError.Value = false;

      Task.Run(() =>
      {
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
      });
    }
  }
}
