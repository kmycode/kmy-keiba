using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KmyKeiba.Models.Analysis.TrainingAnalyzer;

namespace KmyKeiba.Models.Connection
{
  internal static class MovieInfo
  {
    internal static bool IsRacingViewerAvailable { get; set; } = true;
  }

  public class RaceMovieInfo
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public RaceData Race { get; }

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

      if (MovieInfo.IsRacingViewerAvailable)
      {
        if (race.StartTime > now)
        {
          this.IsPatrolError.Value = true;
          this.IsMultiCamerasError.Value = true;
          this.IsRaceError.Value = true;
        }
        if (race.StartTime.Date > today)
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
      else
      {
        // アプリ自体のレーシングビューアー連携機能が無効
        this.IsRaceError.Value = this.IsPatrolError.Value = this.IsMultiCamerasError.Value = this.IsPaddockForceError.Value = this.IsPaddockError.Value = true;
      }
    }

    public async Task PlayRaceAsync()
    {
      if (this.Race.Course <= RaceCourse.CentralMaxValue)
      {
        await this.PlayRaceAsync(MovieType.Race, this.IsRaceError);
      }
      else
      {
        // 地方競馬は楽天から
        var courseCode = this.Race.Key.Substring(8, 2);
        var rakutenCourseCode = courseCode switch
        {
          "45" => "2135",      // 川崎
          "41" => "2015",      // 大井
          "43" => "1914",      // 船橋
          "42" => "1813",      // 浦和
          "36" => "1106",      // 水沢
          "30" => "3601",      // 門別
          "83" => "0304",      // 帯広（ば）
          "46" => "2218",      // 金沢
          "47" => "2320",      // 笠松
          "48" => "2433",      // 名古屋
          "50" => "2726",      // 園田
          "54" => "3129",      // 高知
          "55" => "3230",      // 佐賀
          "51" => "2826",      // 姫路
          "35" => "1006",      // 盛岡
          _ => string.Empty,
        };
        var rakutenKey = this.Race.Key.Substring(0, 8) + rakutenCourseCode + this.Race.Key.Substring(10);

        try
        {
          System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
          {
            FileName = "https://keiba.rakuten.co.jp/archivemovie/RACEID/" + rakutenKey,
            UseShellExecute = true,
          });
        }
        catch (Exception ex)
        {
          logger.Error($"レース {this.Race.Key} 動画再生でエラー", ex);
          this.IsRaceError.Value = true;
        }
      }
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
      catch (Exception ex)
      {
        logger.Error($"レース動画再生でエラー key: {this.Race.Key}, type: {type}", ex);
        error.Value = true;
      }
    }
  }

  public class TrainingMovieInfo
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

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

      if (status == MovieStatus.Unavailable || !MovieInfo.IsRacingViewerAvailable)
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
          logger.Warn($"調教動画 {key} が見つかりませんでした");
          isNotFound = true;
        }
        else if (ex.Error == DownloaderError.RacingViewerNotAvailable)
        {
          logger.Warn("動画再生機能が有効ではありません");
          MovieInfo.IsRacingViewerAvailable = false;
        }
        this.IsTrainingError.Value = true;
      }
      catch (InvalidOperationException)
      {
        // 別の調教動画を読み込み中にボタンを押したとき
        logger.Warn($"調教動画 {key} を再生しようとしましたが、再生ボタン連打を検出したため動作はキャンセルされました");
      }
      catch (Exception ex)
      {
        logger.Error($"調教動画 {key} 取得中にエラー", ex);
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
        catch (Exception ex)
        {
          logger.Error($"調教動画 {key} 再生で例外", ex);
        }
      }
    }

    public static async Task UpdateTrainingListAsync(IReadOnlyList<TrainingRow> trainingList)
    {
      if (!MovieInfo.IsRacingViewerAvailable)
      {
        ThreadUtil.InvokeOnUiThread(() =>
        {
          foreach (var item in trainingList)
          {
            item.Movie.Status = MovieStatus.Unavailable;
          }
        });
        return;
      }

      if (trainingList.Any(t => !t.Movie.IsChecked) && DownloaderModel.Instance.CanSaveOthers.Value)
      {
        var horseKey = trainingList.First().HorseKey;
        try
        {
          await DownloaderConnector.Instance.UpdateMovieListAsync(horseKey);

          using var db = new MyContext();
          var trainings = await db.Trainings!
            .Where(t => t.HorseKey == horseKey)
            .Select(t => new { t.StartTime, t.MovieStatus, })
            .Concat(db.WoodtipTrainings!
              .Where(t => t.HorseKey == horseKey)
              .Select(t => new { t.StartTime, t.MovieStatus, }))
            .ToArrayAsync();
          ThreadUtil.InvokeOnUiThread(() =>
          {
            foreach (var item in trainings
              .Join(trainingList, dt => dt.StartTime, t => t.StartTime, (dt, t) => new { Row = t, dt.MovieStatus, })
              .Where(i => i.MovieStatus != MovieStatus.Unchecked))
            {
              item.Row.Movie.Status = item.MovieStatus;
            };
          });
        }
        catch (DownloaderCommandException ex) when (ex.Error == DownloaderError.RacingViewerNotAvailable)
        {
          logger.Warn("レーシングビューアーが有効ではありません");
          MovieInfo.IsRacingViewerAvailable = false;
        }
        catch (Exception ex)
        {
          logger.Error("調教リスト更新で例外", ex);
        }
      }
    }
  }
}
