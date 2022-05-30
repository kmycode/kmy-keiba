using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
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

    public ReactiveProperty<bool> IsPatrolError { get; } = new();

    public ReactiveProperty<bool> IsMultiCamerasError { get; } = new();

    public ReactiveProperty<bool> IsTrainingError { get; } = new();

    public RaceMovieInfo()
    {
    }

    public RaceMovieInfo(RaceData race)
    {
      this.Race = race;
    }

    public async Task PlayRaceAsync()
    {
      await this.PlayRaceAsync(MovieType.Race, this.IsRaceError);
    }

    public async Task PlayPaddockAsync()
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

    public async Task PlayTrainingAsync(string key)
    {
      try
      {
        await DownloaderConnector.Instance.OpenMovieAsync(MovieType.Training, DownloadLink.Central, key);
      }
      catch
      {
        this.IsTrainingError.Value = true;
      }
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
}
