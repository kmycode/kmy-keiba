using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Connection
{
  internal static class JVLinkServiceWatcher
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public static bool IsRunningAsAdministrator()
    {
      var identity = WindowsIdentity.GetCurrent();
      var principal = new WindowsPrincipal(identity);
      return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    public static JVLinkServiceResult CheckAndTryStart()
    {
      try
      {
        var service = new ServiceController("JVLinkAgent", ".");
        if (service != null)
        {
          logger.Info($"JVLinkAgentサービスの状態: {service.Status}");
          if (service.Status == ServiceControllerStatus.Stopped ||
            service.Status == ServiceControllerStatus.StopPending)
          {
            logger.Debug("JVLinkAgentサービスがStopPendingの場合、Stoppedまで待機します");
            service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
            logger.Debug("サービス開始を要求します");
            service.Start();
          }
          else if (service.Status == ServiceControllerStatus.Paused)
          {
            service.Continue();
          }
          else
          {
            return JVLinkServiceResult.Running;
          }

          logger.Info("JVLinkAgentサービス開始を待機");
          service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
          logger.Info("JVLinkAgentサービスを開始しました");
          return JVLinkServiceResult.Running;
        }
        else
        {
          return JVLinkServiceResult.NotFound;
        }
      }
      catch (Exception ex)
      {
        logger.Warn("JVLinkAgentサービスの確認で例外", ex);
      }

      return JVLinkServiceResult.StartFailed;
    }
  }

  enum JVLinkServiceResult
  {
    Running,
    StartFailed,
    NotFound,
  }
}
