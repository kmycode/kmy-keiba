using KmyKeiba.Models.Common;
using KmyKeiba.Shared;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace KmyKeiba.Models.Connection
{
  internal class UpdateChecker
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public ReactiveProperty<bool> CanUpdate { get; } = new();

    public ReactiveProperty<bool> IsCheckError { get; } = new();

    public ReactiveProperty<StatusFeeling> UpdatingFeeling { get; } = new(StatusFeeling.Standard);

    public ReactiveProperty<string> NewestVersionName { get; } = new();

    public async Task CheckAsync()
    {
      var url = "https://github.com/kmycode/kmy-keiba/releases.atom";

      try
      {
        logger.Info("最新バージョンチェックを試みます");

        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        using var client = new HttpClient();
        var raw = await client.GetStringAsync(url);

        var xml = new XmlDocument();
        xml.LoadXml(raw);

        var feed = xml.GetElementsByTagName("feed")[0] as XmlElement;
        var entities = feed!.GetElementsByTagName("entry");
        var newestEntity = entities[0] as XmlElement;
        var newestVersion = (newestEntity!.GetElementsByTagName("title")[0] as XmlElement)!.InnerText;

        var newVer = Version.Parse(newestVersion);
        var currentVer = Version.Parse(Constrants.ApplicationVersion);
        logger.Info($"現在のバージョン: {Constrants.ApplicationVersion}, 最新バージョン: {newestVersion}");
        if (newVer > currentVer)
        {
          logger.Info("アップデート可能");
          this.UpdatingFeeling.Value = StatusFeeling.Good;
          this.CanUpdate.Value = true;
          this.NewestVersionName.Value = newestVersion;
        }
        else
        {
          logger.Info("アップデートの必要なし");
        }
      }
      catch (Exception ex)
      {
        logger.Error("バージョンチェック中にエラー", ex);
        this.IsCheckError.Value = true;
      }
    }
  }
}
