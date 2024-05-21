using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KmyKeiba.Models.Connection.Connector
{
  internal class CentralConnector : LinkConnectorBase
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public override DownloadLink Link => DownloadLink.Central;

    private static readonly IReadOnlyList<DayOfWeek> _blockWeekdays =
    [
      DayOfWeek.Thursday,
      DayOfWeek.Friday,
      DayOfWeek.Saturday,
      DayOfWeek.Sunday,
    ];

    protected override bool CanDownloadRT
    {
      get
      {
        if (!DownloadConfig.Instance.IsRTDownloadCentralAfterThursdayOnly.Value)
        {
          return true;
        }

        return _blockWeekdays.Contains(DateTime.Today.DayOfWeek);
      }
    }

    protected override async Task UpdateDownloadYearConfigsAsync()
    {
      var config = DownloadConfig.Instance;

      var date = ConfigUtil.GetIntValue(SettingKey.LastDownloadCentralDate);
      var year = date / 100;
      var month = date % 100;

      if (config.StartYear.Value != year || config.StartMonth.Value != month)
      {
        await ConfigUtil.SetIntValueAsync(SettingKey.LastDownloadCentralDate, year * 100 + month);
      }
    }
  }
}
