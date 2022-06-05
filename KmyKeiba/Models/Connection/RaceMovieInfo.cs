using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Data;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Connection
{
  public class RaceMovieInfo
  {
    public RaceData? Race { get; }

    public ReactiveProperty<bool> IsRaceError { get; } = new();

    public ReactiveProperty<bool> IsPaddockError { get; } = new();

    public ReactiveProperty<bool> IsPaddockForceError { get; } = new();

    public ReactiveProperty<bool> IsPatrolError { get; } = new();

    public ReactiveProperty<bool> IsMultiCamerasError { get; } = new();

    public RaceMovieInfo(RaceData race)
    {
      this.Race = race;
      var today = DateTime.Today;
      var now = DateTime.Now;

      if (race.StartTime > now)
      {
        this.IsPatrolError.Value = true;
        this.IsMultiCamerasError.Value = true;
        this.IsRaceError.Value = true;
      }
      if (race.StartTime.Date != today)
      {
        this.IsPaddockForceError.Value = true;
      }

      var nextMonday = race.StartTime.Date.AddHours(12);
      while (nextMonday.DayOfWeek != DayOfWeek.Monday)
      {
        nextMonday = nextMonday.AddDays(1);
      }
      if (nextMonday > now)
      {
        this.IsPatrolError.Value = true;
        this.IsMultiCamerasError.Value = true;
      }

      if (race.Course >= RaceCourse.LocalMinValue)
      {
        this.IsPaddockError.Value = true;
        this.IsPaddockForceError.Value = true;
        this.IsPatrolError.Value = true;
        this.IsMultiCamerasError.Value = true;
        if (race.StartTime < now.AddMonths(-11) && (race.Grade == RaceGrade.Others || race.Grade == RaceGrade.Unknown))
        {
          this.IsRaceError.Value = true;
        }
        // 外国
        if (race.Course >= RaceCourse.Foreign)
        {
          this.IsRaceError.Value = true;
        }
      }
    }

    public async Task PlayRaceAsync()
    {
      await this.PlayRaceAsync(MovieType.Race, this.IsRaceError);
    }

    public async Task PlayPaddockAsync()
    {
      await this.PlayRaceAsync(MovieType.Paddock, this.IsPaddockError);
    }

    public async Task PlayPaddockForceAsync()
    {
      await this.PlayRaceAsync(MovieType.Paddock, this.IsPaddockError);
    }

    public async Task PlayPatrolAsync()
    {
      await this.PlayRaceAsync(MovieType.Patrol, this.IsPatrolError);
    }

    public async Task PlayMultiCamerasAsync()
    {
      await this.PlayRaceAsync(MovieType.MultiCameras, this.IsMultiCamerasError);
    }

    private async Task PlayRaceAsync(MovieType type, ReactiveProperty<bool> error)
    {
      error.Value = false;

      if (this.Race == null)
      {
        error.Value = true;
        return;
      }

      try
      {
        var link = this.Race.Course <= RaceCourse.CentralMaxValue ? DownloadLink.Central : DownloadLink.Local;
        if (link == DownloadLink.Local && type != MovieType.Race)
        {
          throw new ArgumentException("地方競馬でレース以外の動画は見ることができません");
        }
        await DownloaderConnector.Instance.OpenMovieAsync(type, link, this.Race.Key);
      }
      catch
      {
        error.Value = true;
      }
    }
  }

  public class TrainingMovieInfo
  {
    private uint _id;
    private bool _isWoodtip;
    private MovieStatus _status;

    public static ReactiveProperty<bool> IsLoading { get; } = new();

    public ReactiveProperty<bool> IsTrainingError { get; } = new();

    public bool IsChecked => this._status != MovieStatus.Unchecked;

    public MovieStatus Status
    {
      get => this._status;
      set
      {
        this._status = value;
        this.IsTrainingError.Value = value == MovieStatus.Unavailable;
      }
    }

    public TrainingMovieInfo(uint dataId, bool isWoodtip, MovieStatus status)
    {
      this._id = dataId;
      this._isWoodtip = isWoodtip;
      this._status = status;

      if (status == MovieStatus.Unavailable)
      {
        this.IsTrainingError.Value = true;
      }
    }

    public async Task PlayTrainingAsync(string key)
    {
      var isNotFound = false;
      var isAvailable = false;
      IsLoading.Value = true;

      try
      {
        await DownloaderConnector.Instance.OpenMovieAsync(MovieType.Training, DownloadLink.Central, key);
        isAvailable = true;
      }
      catch (DownloaderCommandException ex)
      {
        // 動画が取得できなかった情報はDBに保存され、次回起動以降も反映される
        // サーバーエラーや認証エラーなどのせいで永遠にボタンが押せなくなってみられなくなったら世話ない
        if (ex.Error == DownloaderError.TargetsNotExists)
        {
          isNotFound = true;
        }
        this.IsTrainingError.Value = true;
      }
      catch (InvalidOperationException)
      {
        // 別の調教動画を読み込み中にボタンを押したとき
      }
      catch
      {
        this.IsTrainingError.Value = true;
      }
      finally
      {
        IsLoading.Value = false;
      }

      if (this._status == MovieStatus.Unchecked && (isNotFound || isAvailable) && DownloaderModel.Instance.CanSaveOthers.Value)
      {
        try
        {
          var status = isNotFound ? MovieStatus.Unavailable : isAvailable ? MovieStatus.Available : MovieStatus.Unchecked;
          using var db = new MyContext();
          
          if (this._isWoodtip)
          {
            var data = new WoodtipTrainingData
            {
              Id = this._id,
            };
            db.WoodtipTrainings!.Attach(data);
            data.MovieStatus = status;
          }
          else
          {
            var data = new TrainingData
            {
              Id = this._id,
            };
            db.Trainings!.Attach(data);
            data.MovieStatus = status;
          }

          await db.SaveChangesAsync();
          this._status = status;
        }
        catch
        {
          // TODO: log
        }
      }
    }
  }
}
