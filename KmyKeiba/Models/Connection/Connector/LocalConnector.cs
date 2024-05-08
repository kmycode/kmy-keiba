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
  }
}
