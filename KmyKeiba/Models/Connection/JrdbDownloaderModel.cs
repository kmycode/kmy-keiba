using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Data;
using KmyKeiba.Shared;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.AllJoyn;

namespace KmyKeiba.Models.Connection
{
  public class JrdbDownloaderModel
  {
    private static JrdbDownloaderModel? _instance;
    public static JrdbDownloaderModel Instance => _instance ??= (_instance = new JrdbDownloaderModel());

    public ReactiveProperty<bool> CanSaveOthers { get; } = new(true);

    public ReactiveProperty<bool> IsDownloading { get; } = new();

    public ReactiveProperty<int> DownloadingYear { get; } = new();

    public ReactiveProperty<int> DownloadingMonth { get; } = new();

    public ReactiveProperty<bool> IsCanceled { get; } = new();

    private JrdbDownloaderModel()
    {
    }

    public async Task LoadAsync(DateTime from, DateTime to, string id, string password)
    {
      // TODO: Error
      try
      {
        this.IsDownloading.Value = true;

        using var db = new MyContext();

        var raceDays = await db.Races!
          .Where(r => r.StartTime >= from && r.StartTime <= to && r.Course <= RaceCourse.CentralMaxValue)
          .Select(r => r.StartTime.Date)
          .GroupBy(d => d)
          .Select(g => g.Key)
          .ToArrayAsync();

        // https://keibasoft.memo.wiki/d/JRDB%a4%ab%a4%e9%a4%ce%a5%c7%a1%bc%a5%bf%bc%e8%c6%c0

        foreach (var day in raceDays)
        {
          this.DownloadingYear.Value = day.Year;
          this.DownloadingMonth.Value = day.Month;

          // 調教データは時間差で提供される
          /*
          var dayText = day.ToString("yyyyMMdd");
          if (day < DateTime.Today.AddDays(-10) && await db.JrdbRaceHorses!.AnyAsync(j => j.RaceKey.StartsWith(dayText)))
          {
            continue;
          }
          */

          await this.LoadDayAsync(db, day, id, password);

          if (this.IsCanceled.Value)
          {
            this.IsCanceled.Value = false;
            return;
          }
        }
      }
      catch (Exception ex) when (ex is not JrdbDownloadException)
      {
        throw new JrdbDownloadException("接続で想定しないエラーが発生しました", ex);
      }
      finally
      {
        this.IsDownloading.Value = false;
      }
    }

    private async Task LoadDayAsync(MyContext db, DateTime day, string id, string password)
    {
      var dateFormat = day.ToString("yyMMdd");
      var url = $"http://www.jrdb.com/member/data/Paci/PACI{dateFormat}.lzh";

      var path = Constrants.AppDataDir;
      var lzhDirPath = Path.Combine(path, "jrdbtmp");
      var cacheDirPath = Path.Combine(path, "jrdbcache");
      var lzhFilePath = Path.Combine(path, "jrdbtmp.lzh");
      var cacheFilePath = Path.Combine(cacheDirPath, $"PACI{dateFormat}.lzh");

      try
      {
        Directory.CreateDirectory(lzhDirPath);
      }
      catch (Exception ex)
      {
        throw new JrdbDownloadException("テンポラリディレクトリの作成に失敗しました", ex);
      }

      // Basic認証するユーザ名とパスワード
      // 後々セキュリティ

      if (File.Exists(cacheFilePath))
      {
        File.Copy(cacheFilePath, lzhFilePath, true);
      }
      else
      {
        var myweb = new HttpClient();
        var request = new HttpRequestMessage
        {
          Method = HttpMethod.Get,
          RequestUri = new Uri(url),
          Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", id, password)))), },
        };

        var response = await myweb.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
          throw new JrdbDownloadException("IDまたはパスワードが違います");
        }
        if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
        {
          throw new JrdbDownloadException("サーバーが動作していません");
        }
        if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
          throw new JrdbDownloadException("サーバーが正常に動作していません");
        }
        if (!response.IsSuccessStatusCode)
        {
          return;
        }

        try
        {
          var lzh = await response.Content.ReadAsByteArrayAsync();
          await File.WriteAllBytesAsync(lzhFilePath, lzh);
        }
        catch
        {
          throw new JrdbDownloadException("データのダウンロードに失敗しました");
        }
      }

      // LHA解凍
      try
      {
        await DownloaderConnector.Instance.UnzipLhaAsync(lzhFilePath, lzhDirPath + "\\");
      }
      catch (Exception ex)
      {
        throw new JrdbDownloadException("データの読み込みに失敗しました", ex);
      }

      // 解凍したファイルを読み込む
      var fileName = Path.Combine(lzhDirPath, $"KYI{dateFormat}.txt");
      if (!File.Exists(fileName))
      {
        // 例外はいらないんでは
        return;
      }

      var binary = await File.ReadAllBytesAsync(fileName);
      var lines = Encoding.GetEncoding(932).GetString(binary);

      var dataList = new List<JrdbRaceHorseData>();
      foreach (var line in lines.Split(Environment.NewLine).Where(l => !string.IsNullOrEmpty(l)))
      {
        try
        {
          var data = new JrdbRaceHorseData();
          var result = await data.ReadStringAsync(db, line);
          if (result)
          {
            var exists = await db.JrdbRaceHorses!.FirstOrDefaultAsync(e => e.Key == data.Key && e.RaceKey == data.RaceKey);
            if (exists != null)
            {
              await exists.ReadStringAsync(db, line);
            }
            else
            {
              dataList.Add(data);
            }
          }
        }
        catch (Exception ex)
        {
          throw new JrdbDownloadException("データの読み込み時に内部エラーが発生しました", ex);
        }
      }

      // 調教
      fileName = Path.Combine(lzhDirPath, $"CYB{dateFormat}.txt");
      if (File.Exists(fileName))
      {
        binary = await File.ReadAllBytesAsync(fileName);
        lines = Encoding.GetEncoding(932).GetString(binary);

        foreach (var line in lines.Split(Environment.NewLine).Where(l => !string.IsNullOrEmpty(l)))
        {
          try
          {
            var keys = await JrdbRaceHorseData.GetKeysAsync(db, line);
            if (keys.Item1 == null || keys.Item2 == null)
            {
              continue;
            }

            var extra = await db.RaceHorseExtras!.FirstOrDefaultAsync(e => e.RaceKey == keys.Item1 && e.Key == keys.Item2);
            if (extra == null)
            {
              extra = new RaceHorseExtraData
              {
                RaceKey = keys.Item1,
                Key = keys.Item2,
              };
              await db.RaceHorseExtras!.AddAsync(extra);
            }
            extra.SetJrdbCybData(line);
          }
          catch (Exception ex)
          {
            throw new JrdbDownloadException("CYBデータの読み込み時に内部エラーが発生しました", ex);
          }
        }
      }

      // 調教追い切り
      fileName = Path.Combine(lzhDirPath, $"CHA{dateFormat}.txt");
      if (File.Exists(fileName))
      {
        binary = await File.ReadAllBytesAsync(fileName);
        lines = Encoding.GetEncoding(932).GetString(binary);

        foreach (var line in lines.Split(Environment.NewLine).Where(l => !string.IsNullOrEmpty(l)))
        {
          try
          {
            var keys = await JrdbRaceHorseData.GetKeysAsync(db, line);
            if (keys.Item1 == null || keys.Item2 == null)
            {
              continue;
            }

            var extra = await db.RaceHorseExtras!.FirstOrDefaultAsync(e => e.RaceKey == keys.Item1 && e.Key == keys.Item2);
            if (extra == null)
            {
              extra = new RaceHorseExtraData
              {
                RaceKey = keys.Item1,
                Key = keys.Item2,
              };
              await db.RaceHorseExtras!.AddAsync(extra);
            }
            extra.SetJrdbChaData(line);
          }
          catch (Exception ex)
          {
            throw new JrdbDownloadException("CHAデータの読み込み時に内部エラーが発生しました", ex);
          }
        }
      }

      try
      {
        this.CanSaveOthers.Value = false;

        var keys = dataList.Select(d => d.Key + d.RaceKey).ToArray();
        var exists = await db.JrdbRaceHorses!.Where(j => keys.Contains(j.Key + j.RaceKey)).ToArrayAsync();
        var targets = dataList.ExceptBy(exists.Select(j => j.Key + j.RaceKey), j => j.Key + j.RaceKey).ToList();
        await db.JrdbRaceHorses!.AddRangeAsync(targets);
        await db.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        throw new JrdbDownloadException("データベースへの書き込みに失敗しました", ex);
      }
      finally
      {
        this.CanSaveOthers.Value = true;
      }

      // ファイルを削除
      try
      {
        Directory.Delete(lzhDirPath, true);
      }
      catch (Exception ex)
      {
        // キャッシュファイルの削除はそんなにクリティカルではないと思う
        // throw new JrdbDownloadException("キャッシュファイルの削除に失敗しました", ex);
        return;
      }
    }
  }

  internal class JrdbDownloadException : Exception
  {
    public JrdbDownloadException(string message) : base(message)
    {
    }

    public JrdbDownloadException(string message, Exception innerException) : base(message, innerException)
    {
    }
  }
}
