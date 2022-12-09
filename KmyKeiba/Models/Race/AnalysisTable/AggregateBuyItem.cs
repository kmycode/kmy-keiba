using KmyKeiba.Data.Db;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.AnalysisTable
{
  public class AggregateBuyItem
  {
    public TicketType TicketType { get; }

    public IReadOnlyList<MarkData> Marks { get; }

    public AggregateBuyItem(TicketType ticketType)
    {
      this.TicketType = ticketType;
      
      var marks = new List<MarkData>();
      marks.Add(new MarkData(ticketType, RaceHorseMark.DoubleCircle));
      marks.Add(new MarkData(ticketType, RaceHorseMark.Circle));
      marks.Add(new MarkData(ticketType, RaceHorseMark.FilledTriangle));
      marks.Add(new MarkData(ticketType, RaceHorseMark.Triangle));
      marks.Add(new MarkData(ticketType, RaceHorseMark.Default));
      marks.Add(new MarkData(ticketType, RaceHorseMark.Deleted));
      this.Marks = marks;
    }

    public IReadOnlyList<TicketData> ToTickets(IReadOnlyList<(RaceHorseMark Mark, short Number)> horses)
    {
      var tickets = new List<TicketData>();
      if (!this.Marks.Any() || !this.Marks[0].Items.Any())
      {
        return tickets;
      }

      IEnumerable<short> GetNumbers(RaceHorseMark mark)
      {
        var ds = horses.Where(h => h.Mark == mark).Select(h => h.Number);
        return ds;
      }

      if (this.Marks[0].Items.Count == 1)
      {
        var marks1 = this.Marks.Where(m => m.Items[0].IsChecked.Value).Select(m => m.Mark);
        foreach (var mark1 in marks1)
        {
          var num = GetNumbers(mark1);
          if (!num.Any()) continue;

          foreach (var n in num)
          {
            tickets.Add(new TicketData
            {
              Count = 1,
              Type = this.TicketType,
              FormType = TicketFormType.Formation,
              Numbers1 = new byte[] { (byte)n, },
            });
          }
        }
      }
      else if (this.Marks[0].Items.Count == 2)
      {
        var marks1 = this.Marks.Where(m => m.Items[0].IsChecked.Value).SelectMany(m => GetNumbers(m.Mark)).Select(n => (byte)n).Where(n => n != default);
        var marks2 = this.Marks.Where(m => m.Items[1].IsChecked.Value).SelectMany(m => GetNumbers(m.Mark)).Select(n => (byte)n).Where(n => n != default);
        if (marks1.Any() && marks2.Any())
        {
          tickets.Add(new TicketData
          {
            Count = 1,
            Type = this.TicketType,
            FormType = TicketFormType.Formation,
            Numbers1 = marks1.ToArray(),
            Numbers2 = marks2.ToArray(),
          });
        }
      }
      else if (this.Marks[0].Items.Count == 3)
      {
        var marks1 = this.Marks.Where(m => m.Items[0].IsChecked.Value).SelectMany(m => GetNumbers(m.Mark)).Select(n => (byte)n).Where(n => n != default);
        var marks2 = this.Marks.Where(m => m.Items[1].IsChecked.Value).SelectMany(m => GetNumbers(m.Mark)).Select(n => (byte)n).Where(n => n != default);
        var marks3 = this.Marks.Where(m => m.Items[2].IsChecked.Value).SelectMany(m => GetNumbers(m.Mark)).Select(n => (byte)n).Where(n => n != default);
        if (marks1.Any() && marks2.Any() && marks3.Any())
        {
          tickets.Add(new TicketData
          {
            Count = 1,
            Type = this.TicketType,
            FormType = TicketFormType.Formation,
            Numbers1 = marks1.ToArray(),
            Numbers2 = marks2.ToArray(),
            Numbers3 = marks3.ToArray(),
          });
        }
      }

      return tickets;
    }

    public class MarkData
    {
      public RaceHorseMark Mark { get; init; }

      public IReadOnlyList<MarkDataItem> Items { get; }

      public class MarkDataItem
      {
        public short Number { get; init; }

        public ReactiveProperty<bool> IsChecked { get; } = new();
      }

      public MarkData(TicketType ticketType, RaceHorseMark mark)
      {
        this.Mark = mark;

        var items = new List<MarkDataItem>();
        for (var i = 0; i < ((ticketType == TicketType.Single || ticketType == TicketType.Place) ? 1 : (ticketType == TicketType.Trifecta || ticketType == TicketType.Trio) ? 3 : 2); i++)
        {
          items.Add(new MarkDataItem() { Number = (short)(i + 1), });
        }
        this.Items = items;
      }
    }
  }
}
