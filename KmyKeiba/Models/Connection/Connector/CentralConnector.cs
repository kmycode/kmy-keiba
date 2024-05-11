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

    public ReactiveProperty<bool> IsLongDownloadMonth { get; } = new();
  }
}
