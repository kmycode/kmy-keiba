using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Downloader
{
  internal partial class Program
  {
    private static void OpenMovie()
    {
      var task = currentTask;
      if (task == null)
      {
        logger.Warn("動画再生のタスクが見つかりませんでした");
        return;
      }

      logger.Info($"動画再生を開始します。パラメータ: {task.Parameter}");

      var p = task.Parameter.Split(',');
      if (p.Length < 3)
      {
        logger.Error("パラメータの数が足りません");
        return;
      }
      var raceKey = p[0];
      var typeStr = p[1];
      var linkName = p[2];

      var link = linkName == "central" ? JVLinkObject.Central : JVLinkObject.Local;
      short.TryParse(typeStr, out var type);

      try
      {
        logger.Info($"動画再生をリンクに問い合わせます {type} / {raceKey}");

        link.PlayMovie((JVLinkMovieType)type, raceKey);
        SetTask(task, t =>
        {
          t.IsFinished = true;
          t.Result = "ok";
        });

        logger.Info("動画再生に成功しました");
      }
      catch (JVLinkException<JVLinkMovieResult> ex)
      {
        logger.Error($"動画再生に失敗しました {ex.Code}", ex);

        SetTask(task, t =>
        {
          t.IsFinished = true;
          t.Result = JVLinkException.GetAttribute(ex.Code).Message;
          t.Error = ex.Code.ToDownloaderError();
        });
      }
      catch (Exception ex)
      {
        logger.Error("動画再生でエラーが発生しました", ex);
      }
    }

    private static void UpdateTrainingMovieList()
    {
      var task = currentTask;
      if (task == null)
      {
        return;
      }

      var p = task.Parameter.Split(',');
      if (p.Length < 1)
      {
        SetTask(task, t =>
        {
          t.Error = DownloaderError.ApplicationError;
          t.IsFinished = true;
        });
        logger.Error("タスクのパラメータが足りません");
        return;
      }

      var horseKey = p[0];
      logger.Info($"キー: {horseKey} の調教動画一覧を取得します");

      using var db = new MyContext();
      db.DownloaderTasks!.Attach(task);

      try
      {
        IEnumerable<DateOnly> list;

        try
        {
          var link = JVLinkObject.Central;
          using var reader = link.OpenMovie(JVLinkTrainingMovieType.Horse, horseKey);
          list = reader.ReadKeys()
            .Select(k =>
            {
              var dateStr = k[..8];
              short.TryParse(k.Substring(0, 4), out var year);
              short.TryParse(k.Substring(4, 2), out var month);
              short.TryParse(k.Substring(6, 2), out var day);
              return new DateOnly(year, month, day);
            }).ToArray();
          logger.Info($"動画の数: {list.Count()}");
        }
        catch (JVLinkException<JVLinkMovieResult> ex) when (ex.Code == JVLinkMovieResult.NotFound)
        {
          // 動画の数はゼロということ
          list = Enumerable.Empty<DateOnly>();
          logger.Info($"動画の数: {list.Count()}");
        }

        var trainings = db.Trainings!.Where(t => t.HorseKey == horseKey).ToArray();
        var woodTrainings = db.WoodtipTrainings!.Where(t => t.HorseKey == horseKey).ToArray();
        foreach (var data in trainings)
        {
          data.MovieStatus = list.Any(i => i.Year == data.StartTime.Year && i.Month == data.StartTime.Month && i.Day == data.StartTime.Day)
            ? MovieStatus.Available : MovieStatus.Unavailable;
        }
        foreach (var data in woodTrainings)
        {
          data.MovieStatus = list.Any(i => i.Year == data.StartTime.Year && i.Month == data.StartTime.Month && i.Day == data.StartTime.Day)
            ? MovieStatus.Available : MovieStatus.Unavailable;
        }
        db.SaveChanges();
      }
      catch (JVLinkException<JVLinkMovieResult> ex)
      {
        logger.Error($"動画リストダウンロードでエラーが発生しました {ex.Code}", ex);
        task.Error = ex.Code.ToDownloaderError();
        task.Result = ex.Message;
      }
      catch (Exception ex)
      {
        logger.Error("動画リストダウンロードでエラーが発生しました", ex);
      }

      task.IsFinished = true;
      db.SaveChanges();
    }
  }
}
