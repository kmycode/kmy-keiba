using KmyKeiba.Data.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.Tickets
{
  public static class TicketExtensions
  {
    public static string ToSummaryString(this TicketData ticket)
    {
      if (!ticket.Numbers2.Any())
      {
        return string.Join(',', ticket.Numbers1);
      }
      else if (!ticket.Numbers3.Any())
      {
        if (ticket.FormType == TicketFormType.Formation)
        {
          var label = ticket.IsMulti ? "フォメマルチ" : "フォメ";
          return label + " " + string.Join(',', ticket.Numbers1) + " - " + string.Join(',', ticket.Numbers2);
        }
        else if (ticket.FormType == TicketFormType.Box)
        {
          return "BOX " + string.Join(',', ticket.Numbers1);
        }
        else if (ticket.FormType == TicketFormType.Single)
        {
          return ticket.Numbers1.FirstOrDefault() + "　" + ticket.Numbers2.FirstOrDefault();
        }
      }
      else
      {
        if (ticket.FormType == TicketFormType.Formation)
        {
          var label = ticket.IsMulti ? "フォメマルチ" : "フォメ";
          return label + " " + string.Join(',', ticket.Numbers1) + " - " + string.Join(',', ticket.Numbers2) + " - " + string.Join(',', ticket.Numbers3);
        }
        else if (ticket.FormType == TicketFormType.Box)
        {
          return "BOX " + string.Join(',', ticket.Numbers1);
        }
        else if (ticket.FormType == TicketFormType.Nagashi)
        {
          var label = ticket.IsMulti ? "流しマルチ" : "流し";
          return label + " 軸:" + string.Join(',', ticket.Numbers1) + " - " + string.Join(',', ticket.Numbers2);
        }
        else if (ticket.FormType == TicketFormType.Single)
        {
          return ticket.Numbers1.FirstOrDefault() + "　" + ticket.Numbers2.FirstOrDefault() + "　" + ticket.Numbers3.FirstOrDefault();
        }
      }

      return ticket.ToString() ?? string.Empty;
    }
  }
}
