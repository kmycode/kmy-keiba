using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Analysis.Generic;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.Tickets
{
  public class TicketItemRow : IMultipleCheckableItem
  {
    public TicketType Type { get; set; }

    string IMultipleCheckableItem.GroupName => string.Empty;

    public bool IsSingleRow { get; set; }

    public short Number1 { get; init; }

    public short Number2 { get; init; }

    public short Number3 { get; init; }

    public int Money { get; init; }

    public int MoneyMax { get; init; }

    public ReactiveProperty<int>? Count => null;  // xamlバインディング用

    public int DataCount { get; set; }

    public ReactiveProperty<bool> IsChecked { get; } = new();

    public ReactiveProperty<bool>? IsAllRowsChecked { get; set; }

    public ReactiveProperty<ValueComparation> Comparation { get; } = new();
  }
}
