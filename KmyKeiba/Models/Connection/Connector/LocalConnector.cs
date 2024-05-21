using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KmyKeiba.Models.Connection.Connector
{
  internal class LocalConnector : LinkConnectorBase
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public override DownloadLink Link => DownloadLink.Local;

    protected override async Task UpdateDownloadYearConfigsAsync()
    {
      var config = DownloadConfig.Instance;

      var date = ConfigUtil.GetIntValue(SettingKey.LastDownloadLocalDate);
      var year = date / 100;
      var month = date % 100;

      if (config.StartYear.Value != year || config.StartMonth.Value != month)
      {
        await ConfigUtil.SetIntValueAsync(SettingKey.LastDownloadLocalDate, year * 100 + month);
      }
    }
  }
}
