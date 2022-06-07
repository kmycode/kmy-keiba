using KmyKeiba.Models.Race.Tickets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace KmyKeiba.Views.Details
{
  internal class TicketSingleRowTemplateSelector : DataTemplateSelector
  {
    public DataTemplate? SingleRow { get; set; }

    public DataTemplate? MultipleRow { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
      if (item is TicketItem rows)
      {
        return rows.IsSingleRow ? this.SingleRow : this.MultipleRow;
      }
      return base.SelectTemplate(item, container);
    }
  }
}
