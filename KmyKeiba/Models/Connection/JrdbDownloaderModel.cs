﻿using KmyKeiba.Data.Db;
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

          var dayText = day.ToString("yyyyMMdd");
          if (day < DateTime.Today.AddDays(-10) && await db.JrdbRaceHorses!.AnyAsync(j => j.RaceKey.StartsWith(dayText)))
          {
            continue;
          }

          await this.LoadDayAsync(db, day, id, password);

          if (this.IsCanceled.Value)
          {
            this.IsCanceled.Value = false;
            return;
          }
        }
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
      var lzhFilePath = Path.Combine(path, "jrdbtmp.lzh");
      var lzhDirPath = Path.Combine(path, "jrdbtmp");
      Directory.CreateDirectory(lzhDirPath);

      // Basic認証するユーザ名とパスワード
      // 後々セキュリティ

      var myweb = new HttpClient();

      var request = new HttpRequestMessage
      {
        Method = HttpMethod.Post,
        RequestUri = new Uri(url),
        Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", id, password)))), },
      };

      var response = await myweb.SendAsync(request);
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
        // ダウンロード失敗
        return;
      }

      // LHA解凍
      try
      {
        await DownloaderConnector.Instance.UnzipLhaAsync(lzhFilePath, lzhDirPath + "\\");
      }
      catch (Exception ex)
      {
        // TODO 解凍失敗
        return;
      }

      // 解凍したファイルを読み込む
      var fileName = Path.Combine(lzhDirPath, $"KYI{dateFormat}.txt");
      if (!File.Exists(fileName))
      {
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
            dataList.Add(data);
          }
        }
        catch (Exception ex)
        {
          // TODO
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
        // TODO
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
        // TODO
      }
    }
  }
}