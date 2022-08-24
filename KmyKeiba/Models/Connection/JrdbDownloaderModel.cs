﻿using KmyKeiba.Models.Data;
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

    public ReactiveProperty<bool> CanSaveOthers { get; } = new();

    private JrdbDownloaderModel()
    {
    }

    public async Task LoadTestAsync(DateTime from, DateTime to, string id, string password)
    {
      // TODO: Error
      using var db = new MyContext();

      // https://keibasoft.memo.wiki/d/JRDB%a4%ab%a4%e9%a4%ce%a5%c7%a1%bc%a5%bf%bc%e8%c6%c0

      var date = from;

      await this.LoadDayAsync(from, id, password);
    }

    private async Task LoadDayAsync(DateTime day, string id, string password)
    {
      var dateFormat = day.ToString("yyMMdd");
      var url = $"http://www.jrdb.com/member/data/Paci/PACI{dateFormat}.lzh";

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

      var lzh = await response.Content.ReadAsByteArrayAsync();
      await File.WriteAllBytesAsync("test.lzh", lzh);

      // 書き出したファイルを解凍
      Directory.CreateDirectory("test.lzh-tmp");

      // LHA解凍
      try
      {
        await DownloaderConnector.Instance.UnzipLhaAsync(@"C:\Users\tt\Documents\repo\KmyKeiba\dist\x64\Debug\test.lzh", @"C:\Users\tt\Documents\repo\KmyKeiba\dist\x64\Debug\test.lzh-tmp\");
      }
      catch (Exception ex)
      {
        // TODO
      }

      // 解凍したファイルを読み込む

      // ファイルを削除
      //File.Delete("test.lzh");
      //Directory.Delete("testdir", true);
    }
  }
}
