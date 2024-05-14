using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.JVLink.Wrappers;
using KmyKeiba.Shared;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Downloader
{
  internal partial class Program
  {
    private static void Test()
    {
      try
      {
        var loader = new JVLinkLoader();
        loader.StartLoad(JVLinkObject.Local, JVLinkDataspec.Race | JVLinkDataspec.Diff,
          JVLinkOpenOption.Setup, null, new DateTime(2018, 1, 1), new DateTime(2024, 2, 1));
      }
      catch (Exception ex)
      {

      }
    }

    private static void StartLoad(DownloaderTaskData task, bool isRealTime = false)
    {
      var loader = new JVLinkLoader();
      var isLoaded = false;
      var isDbLooping = false;

      Task.Run(() =>
      {
        var loopCount = 0;
        isDbLooping = true;

        void UpdateProcess()
        {
          var p = loader.Process.ToString().ToLower();
          if (p != task.Result)
          {
            logger.Info($"ダウンロード状態が {p} に移行しました");
            try
            {
              SetTask(task, t =>
              {
                t.Result = p;
              });
            }
            catch (Exception ex)
            {
              logger.Warn("ダウンロード状態のタスクへの書き込みでエラー", ex);
            }
          }
          else
          {
            try
            {
              SetTask(task, t =>
              {
                if (loader.Process == LoadProcessing.Downloading)
                {
                  task.ProgressMax = loader.DownloadSize;
                  task.Progress = loader.Downloaded;
                }
                else
                {
                  task.ProgressMax = loader.LoadSize;
                  task.Progress = loader.Loaded;
                }
              });
            }
            catch { }
          }
        }

        // トランザクションを開始する前に、データ保存中という情報をアプリに渡す
        loader.StartingTransaction += (sender, e) =>
        {
          UpdateProcess();

          if (!isRealTime)
          {
            // SaveChangesが終わる前にトランザクション始まる？
            Task.Delay(500).Wait();
          }
        };

        while (!isLoaded)
        {
          UpdateProcess();

          loopCount++;
          if (loopCount % 60 == 0)
          {
            logger.Info($"DWN [{loader.Downloaded} / {loader.DownloadSize}] LD [{loader.Loaded} / {loader.LoadSize}] ENT({loader.LoadEntityCount}) SV [{loader.Saved} / {loader.SaveSize}] PC [{loader.Processed} / {loader.ProcessSize}]");
          }

          Task.Delay(800).Wait();
        }

        if (!task.IsFinished)
        {
          try
          {
            SetTask(task, t => t.IsFinished = true);
          }
          catch (Exception ex)
          {
            logger.Error("タスクへの完了報告書き込みに失敗", ex);
          }
          finally
          {
            isDbLooping = false;
          }
        }

        isDbLooping = false;
      });

      var isHostTaskCanceled = false;
      try
      {
        if (isRealTime)
        {
          logger.Info("ダウンロードを開始します（RT）");
          RTLoadAsync(loader, task).Wait();
        }
        else
        {
          logger.Info("ダウンロードを開始します（セットアップ／通常）");
          LoadAsync(loader, task).Wait();
        }
      }
      catch (TaskCanceledAndContinueProgramException)
      {
        logger.Warn("ダウンロードはキャンセルされました");
        isHostTaskCanceled = true;
      }
      catch (Exception ex)
      {
        logger.Error("ダウンロードでエラーが発生しました", ex);
      }
      finally
      {
        loader.Dispose();
        isLoaded = true;
      }

      var timeout = 200;
      while (isDbLooping && timeout-- > 0)
      {
        Task.Delay(50).Wait();
      }
      if (timeout <= 0)
      {
        logger.Warn("ダウンロード・書き込み処理がタイムアウトしました");
      }

      if (!isHostTaskCanceled && !isHost)
      {
        KillMe();
      }
    }

    private static async Task LoadAsync(JVLinkLoader loader, DownloaderTaskData task)
    {
      var end = DateTime.Now.AddMonths(1);

      var parameters = task.Parameter.Split(',');
      if (parameters.Length < 3)
      {
        task.Error = DownloaderError.ApplicationError;
        task.IsFinished = true;
        DownloaderTaskDataExtensions.Save(task);
        logger.Error("タスクのパラメータが足りません");
        return;
      }

      int.TryParse(parameters[0], out var startYear);
      int.TryParse(parameters[1], out var startMonth);
      var startDay = 0;
      if (startMonth > 100)
      {
        startDay = startMonth % 100;
        startMonth /= 100;
      }
      if (startYear < 1986 || startYear > end.Year || startMonth < 0 || startMonth > 12)
      {
        task.Error = DownloaderError.ApplicationError;
        task.IsFinished = true;
        DownloaderTaskDataExtensions.Save(task);
        logger.Error($"開始年月が誤りです {parameters[0]} {parameters[1]}");
        return;
      }

      var link = parameters[2] == "central" ? JVLinkObject.Central : parameters[2] == "local" ? JVLinkObject.Local : null;
      if (link == null)
      {
        task.Error = DownloaderError.ApplicationError;
        task.IsFinished = true;
        DownloaderTaskDataExtensions.Save(task);
        logger.Error($"リンクの指定が誤りです {parameters[2]}");
        return;
      }
      if (link.Type == JVLinkObjectType.Unknown || link.IsError)
      {
        Shutdown(DownloaderError.NotInstalledCom);
        return;
      }

      var dataspec = JVLinkDataspec.Race | JVLinkDataspec.Blod | JVLinkDataspec.Diff | JVLinkDataspec.Slop | JVLinkDataspec.Toku;
      if (parameters[2] == "central")
      {
        dataspec |= JVLinkDataspec.Wood | JVLinkDataspec.Hose | JVLinkDataspec.Ming;
      }
      else
      {
        // 2020年4月以降のデータが提供されていない／南関東しかない
        // 実用性は低い
        // dataspec1 |= JVLinkDataspec.Nosi;
      }

      SystemData? isNotDownloadBlod, isNotDownloadSlop;
      using (var db = new MyContext())
      {
        isNotDownloadBlod = await db.SystemData!.FirstOrDefaultAsync(d => d.Key == SettingKey.IsNotDownloadHorseBloods);
        isNotDownloadSlop = await db.SystemData!.FirstOrDefaultAsync(d => d.Key == SettingKey.IsNotDownloadTrainings);
      }

      if (isNotDownloadBlod != null && isNotDownloadBlod.IntValue != 0)
      {
        dataspec &= ~JVLinkDataspec.Blod;
      }
      if (isNotDownloadSlop != null && isNotDownloadSlop.IntValue != 0)
      {
        dataspec &= ~JVLinkDataspec.Slop;
        dataspec &= ~JVLinkDataspec.Wood;
      }

      if (parameters[2] == "local" && startYear < 2005)
      {
        startYear = 2005;
        startMonth = 1;
      }

      var mode = parameters[3];

      var option = (DateTime.Now.Year * 12 + DateTime.Now.Month) - (startYear * 12 + startMonth) > 11 ? JVLinkOpenOption.Setup : JVLinkOpenOption.Normal;

      var start = new DateTime(startYear, startMonth, System.Math.Max(1, startDay));
      mode = "race";
      task.Parameter = $"{startYear},{startMonth},{parameters[2]},{mode},{string.Join(',', parameters.Skip(4))}";
      DownloaderTaskDataExtensions.Save(task);

      logger.Info("レースのダウンロードを開始します");
      loader.StartLoad(link,
        dataspec,
        option,
        raceKey: null,
        startTime: start,
        endTime: DateTime.Now.AddMonths(1),
        loadSpecs: null);
    }

    private static async Task RTLoadAsync(JVLinkLoader loader, DownloaderTaskData task)
    {
      MyContext? db = new();

      try
      {
        var parameters = task.Parameter.Split(',');
        if (parameters.Length < 2)
        {
          task.Error = DownloaderError.ApplicationError;
          task.IsFinished = true;
          DownloaderTaskDataExtensions.Save(task);
          logger.Error("タスクのパラメータが足りません");
          return;
        }

        var date = parameters[0];
        var type = parameters[1];
        var spec = parameters[2];
        var skip = parameters[3];

        var link = type == "central" ? JVLinkObject.Central : type == "local" ? JVLinkObject.Local : null;
        if (link == null)
        {
          task.Error = DownloaderError.ApplicationError;
          task.IsFinished = true;
          DownloaderTaskDataExtensions.Save(task);
          logger.Error($"リンクの指定が誤りです {parameters[2]}");
          return;
        }
        if (link.Type == JVLinkObjectType.Unknown || link.IsError)
        {
          Shutdown(DownloaderError.NotInstalledCom);
          return;
        }

        var todayFormat = DateTime.Today.ToString("yyyyMMdd");
        if (date == "today")
        {
          date = todayFormat;
        }
        int.TryParse(date.AsSpan(0, 4), out var year);
        int.TryParse(date.AsSpan(4, 2), out var month);
        int.TryParse(date.AsSpan(6, 2), out var day);

        var dataspecs = link.Type == JVLinkObjectType.Central ? new[]
        {
          JVLinkDataspec.RB12,
          JVLinkDataspec.RB15,
          JVLinkDataspec.RB30,
          JVLinkDataspec.RB11,
          JVLinkDataspec.RB14,
          JVLinkDataspec.RB41,
          JVLinkDataspec.RB13,
          JVLinkDataspec.RB17,
        } : new[]
        {
          JVLinkDataspec.RB12,
          JVLinkDataspec.RB15,
          JVLinkDataspec.RB30,
          JVLinkDataspec.RB11,
          JVLinkDataspec.RB14,
          JVLinkDataspec.RB41,
        };
        int.TryParse(spec, out var startIndex);
        if (startIndex == default || startIndex > dataspecs.Length)
        {
          // Restartメソッドを考慮し、エラーにはしない
          task.IsFinished = true;
          DownloaderTaskDataExtensions.Save(task);
          logger.Warn("このタスクはすでに完了している可能性があります");
          return;
        }

        var start = new DateTime(year, month, day);
        var today = DateTime.Today;
        var now = DateTime.Now;

        int.TryParse(skip, out var skipCount);
        var query = db.Races!
          .Where(r => r.StartTime.Date >= start)
          .OrderBy(r => r.StartTime)
          .Select(r => new { r.Key, r.StartTime, r.Course, r.Grade, r.DataStatus, });
        var races = await query.ToArrayAsync();

        // 今日以降のレースがない場合
        if (!races.Any())
        {
          dataspecs = link.Type == JVLinkObjectType.Central ? new[]
          {
            JVLinkDataspec.RB12,
            JVLinkDataspec.RB15,
            JVLinkDataspec.RB11,
            JVLinkDataspec.RB14,
            JVLinkDataspec.RB13,
            JVLinkDataspec.RB17,
          } : new[]
          {
            JVLinkDataspec.RB12,
            JVLinkDataspec.RB15,
            JVLinkDataspec.RB11,
            JVLinkDataspec.RB14,
          };
        }

        var raceKeys = races.Select(r => r.Key).ToArray();
        var oddsTImeline = await db.SingleOddsTimelines!
          .Where(r => raceKeys.Contains(r.RaceKey))
          .ToArrayAsync();
        var placeOdds = await db.PlaceOdds!
          .Where(r => raceKeys.Contains(r.RaceKey))
          .Select(o => o.RaceKey)
          .ToArrayAsync();

        var isDownloadAfterThursdayData = await db.SystemData!.FirstOrDefaultAsync(s => s.Key == SettingKey.IsDownloadCentralOnThursdayAfterOnly);
        var isDownloadAfterThursday = (isDownloadAfterThursdayData?.IntValue ?? 0) != 0;

        db?.Dispose();
        db = null;

        races = races.Where(r =>
        {
          if (type == "central")
          {
            if (r.StartTime.Date == today)
            {
              return true;
            }
            if (today.DayOfWeek == DayOfWeek.Saturday && r.StartTime.DayOfWeek == DayOfWeek.Sunday)
            {
              // 日曜日のレースは土曜日発売
              return true;
            }
            if (today.DayOfWeek == DayOfWeek.Friday && r.StartTime.DayOfWeek == DayOfWeek.Saturday && now.Hour >= 12)
            {
              // 夕方から売ってることがある
              return true;
            }

            if (!isDownloadAfterThursday)
            {
              if (today.AddDays(1) <= r.StartTime && r.StartTime < today.AddDays(2))
              {
                // とりあえず翌日のレースは全部取得
                return true;
              }
            }

            bool result;
            if (r.Course <= RaceCourse.CentralMaxValue)
            {
              // 馬券が金曜日日販売になるのは一部のG1レースのみ
              result = r.Grade == RaceGrade.Grade1 || r.Grade == RaceGrade.Grade2 || r.Grade == RaceGrade.Grade3;
            }
            else
            {
              result = r.Grade == RaceGrade.LocalGrade1 || r.Grade == RaceGrade.LocalGrade2 || r.Grade == RaceGrade.LocalGrade3 || r.Grade == RaceGrade.LocalNoNamedGrade;
            }
            return result;
          }
          else
          {
            return r.StartTime.Date == start && r.Course >= RaceCourse.LocalMinValue;
          }
        }).ToArray();

        var skiped = races.Skip(skipCount).ToArray();
        if (!races.Any())
        {
          startIndex++;
          skipCount = 0;
        }

        logger.Info("リアルタイムデータのダウンロードを開始します");
        for (var i = startIndex - 1; i < dataspecs.Length; i++)
        {
          logger.Info($"spec: {dataspecs[i]}");

          var currentRaceIndex = 0;
          var targets = races;
          if (i == startIndex - 1)
          {
            targets = skiped;
            currentRaceIndex = skipCount;
          }

          foreach (var race in targets)
          {
            var useKey = race.Key;
            if (dataspecs[i] == JVLinkDataspec.RB14 || dataspecs[i] == JVLinkDataspec.RB12 || dataspecs[i] == JVLinkDataspec.RB15 || dataspecs[i] == JVLinkDataspec.RB11 || dataspecs[i] == JVLinkDataspec.RB13 || dataspecs[i] == JVLinkDataspec.RB17)
            {
              useKey = null;
            }
            else if (dataspecs[i] == JVLinkDataspec.RB30)
            {
              // オッズは各レースごとに落とすから時間がかかる。必要ないものは切り捨てる
              if (race.DataStatus >= RaceDataStatus.PreliminaryGrade3 && placeOdds.Contains(race.Key))
              {
                continue;
              }
            }
            else if (dataspecs[i] == JVLinkDataspec.RB41)
            {
              if (race.StartTime > now.AddMinutes(90))
              {
                continue;
              }

              // 発走直前の時系列オッズデータがあれば省略
              var latestTimeline = oddsTImeline.Where(o => o.RaceKey == race.Key).OrderByDescending(o => o.Time).FirstOrDefault();
              if (latestTimeline != null && !(race.Course <= RaceCourse.CentralMaxValue ? latestTimeline.Time < race.StartTime : latestTimeline.Time < race.StartTime.AddMinutes(-1)))
              {
                continue;
              }
            }

            task.Parameter = $"{parameters[0]},{parameters[1]},{i + 1},{currentRaceIndex},{string.Join(',', parameters.Skip(4))}";
            DownloaderTaskDataExtensions.Save(task);

            CheckShutdown();

            loader.StartLoad(link,
              dataspecs[i],
              JVLinkOpenOption.RealTime,
              raceKey: useKey,
              startTime: start,
              endTime: null,
              loadSpecs: null);

            if (useKey == null)
            {
              break;
            }
            currentRaceIndex++;
          }
        }
      }
      finally
      {
        db?.Dispose();
      }
    }
  }
}
