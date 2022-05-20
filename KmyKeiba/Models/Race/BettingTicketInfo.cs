﻿using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race
{
  public class BettingTicketInfo : IDisposable
  {
    private readonly CompositeDisposable _disposables = new();
    private readonly string _raceKey;
    private readonly IReadOnlyList<RaceHorseData> _horses;

    public OddsInfo Odds { get; }

    public IReadOnlyList<BettingHorseItem> Numbers1 { get; }

    public IReadOnlyList<BettingHorseItem> Numbers2 { get; }

    public IReadOnlyList<BettingHorseItem> Numbers3 { get; }

    public IReadOnlyList<BettingFrameItem> FrameNumbers1 { get; }

    public IReadOnlyList<BettingFrameItem> FrameNumbers2 { get; }

    public ReactiveCollection<TicketItem> Tickets { get; } = new();

    public ReactiveProperty<TicketType> Type { get; } = new(TicketType.Single);

    public ReactiveProperty<TicketFormType> FormType { get; } = new(TicketFormType.Formation);

    public BettingTicketInfo(IEnumerable<RaceHorseAnalyzer> horses, OddsInfo odds, IReadOnlyList<TicketData> existTickets)
    {
      this.Odds = odds;
      this._raceKey = horses.FirstOrDefault()?.Race.Key ?? string.Empty;

      var items = horses
        .OrderBy(h => h.Data.Number)
        .Select(h => new BettingHorseItem(h)
        {
          HorseNumber = h.Data.Number,
          Name = h.Data.Name,
          IsEnabled = { Value = h.Data.AbnormalResult != RaceAbnormality.Scratched &&
                                h.Data.AbnormalResult != RaceAbnormality.ExcludedByStarters, },
        }.AddTo(this._disposables));
      var frames = horses
        .GroupBy(h => h.Data.FrameNumber)
        .OrderBy(h => h.Key)
        .Select(h => new BettingFrameItem(h)
        {
          FrameNumber = h.Key,
        }.AddTo(this._disposables));
      // Linqの遅延評価を利用して、全く同じ内容で異なるインスタンスを持った配列を作成・格納する
      this.Numbers1 = items.ToArray();
      this.Numbers2 = items.ToArray();
      this.Numbers3 = items.ToArray();
      this.FrameNumbers1 = frames.ToArray();
      this.FrameNumbers2 = frames.ToArray();

      this._horses = horses.Select(h => h.Data).OrderBy(h => h.Number).ToArray();

      foreach (var ticket in existTickets)
      {
        if (ticket.Type == TicketType.Single)
        {
          this.Tickets.Add(new SingleTicketItem(ticket, odds));
        }
        else if (ticket.Type == TicketType.Place)
        {
          this.Tickets.Add(new PlaceTicketItem(ticket, odds));
        }
        else if (ticket.Type == TicketType.FrameNumber)
        {
          this.Tickets.Add(new FrameNumberTicketItem(ticket, odds, this._horses));
        }
      }
    }

    public void Dispose() => this._disposables.Dispose();

    public void SetType(string str)
    {
      short.TryParse(str, out var num);
      this.Type.Value = (TicketType)num;
    }

    public void SetFormType(string str)
    {
      short.TryParse(str, out var num);
      this.FormType.Value = (TicketFormType)num;
    }

    public async Task BuyAsync()
    {
      TicketItem? ticket = null;
      if (this.Type.Value == TicketType.Single)
      {
        ticket = this.GenerateSingleTicket();
      }

      if (ticket == null)
      {
        return;
      }

      this.Tickets.Add(ticket);

      using var db = new MyContext();
      await db.Tickets!.AddAsync(ticket.Data);
      await db.SaveChangesAsync();
    }

    public TicketItem? GenerateSingleTicket()
    {
      var data = new TicketData
      {
        RaceKey = this._raceKey,
        Type = TicketType.Single,
        FormType = TicketFormType.Formation,
        Numbers1 = this.Numbers1.Where(n => n.IsChecked.Value = true).Select(n => (byte)n.HorseNumber).ToArray(),
      };
      var item = new SingleTicketItem(data, this.Odds);

      if (item.Rows.Any())
      {
        return item;
      }
      return null;
    }

    public async Task RemoveTicketAsync()
    {
      using var db = new MyContext();

      var targetTickets = this.Tickets
        .Where(t => t.Rows.Any(r => r.IsChecked.Value))
        .ToArray();
      var removeTickets = new List<TicketItem>();
      foreach (var ticket in targetTickets)
      {
        var index = this.Tickets.IndexOf(ticket);
        this.Tickets.RemoveAt(index);

        db.Tickets!.Remove(ticket.Data);

        var broken = ticket.Break();
        var leaves = broken.Where(r => r.Rows.Any() && r.Rows.All(rr => !rr.IsChecked.Value)).ToArray();
        foreach (var item in leaves)
        {
          this.Tickets.Insert(index++, item);
          await db.Tickets!.AddAsync(item.Data);
        }
      }

      await db.SaveChangesAsync();
    }
  }

  public abstract class TicketItem
  {
    public TicketData Data { get; }

    public ReactiveCollection<TicketItemRow> Rows { get; private set; } = new();

    public bool IsSingleRow => this.Rows.Count == 1;

    public short Number1 => this.Rows[0].Number1;

    public short Number2 => this.Rows[0].Number2;

    public short Number3 => this.Rows[0].Number3;

    public int Money => this.Rows[0].Money;

    public int MoneyMax => this.Rows[0].MoneyMax;

    public TicketItem(TicketData data)
    {
      this.Data = data;
    }

    protected void SetRows(IReadOnlyList<TicketItemRow> rows)
    {
      foreach (var row in rows)
      {
        this.Rows.Add(row);
      }
    }

    public IReadOnlyList<TicketItem> Break()
    {
      if (this.Rows.Count == 1)
      {
        return new[] { this, };
      }

      var rows = new List<TicketItem>();
      foreach (var row in this.Rows)
      {
        var data = new TicketData
        {
          RaceKey = this.Data.RaceKey,
          FormType = TicketFormType.Single,
          Numbers1 = new[] { (byte)row.Number1, },
        };
        if (row.Number2 != default) data.Numbers2 = new byte[] { (byte)row.Number2, };
        if (row.Number3 != default) data.Numbers3 = new byte[] { (byte)row.Number3, };

        var item = this.GenerateNewItem(data, row.Money, row.MoneyMax);
        item.Rows.Add(row);
        rows.Add(item);
      }

      return rows;
    }

    protected abstract TicketItem GenerateNewItem(TicketData data, int money, int moneyMax);

    public virtual IReadOnlyList<TicketItemRow> GetHitRows(short num1, short num2, short num3, short frame1, short frame2, short frame3)
    {
      var rows = this.Rows.Where(r => r.Number1 == num1 && (num2 == default || r.Number2 == num2) && (num3 == default || r.Number3 == num3));
      return rows.ToArray();
    }

    protected static IReadOnlyList<(byte, byte, byte)> GetFormationNumbers(byte[] num1, byte[]? num2, byte[]? num3, bool isOrder, bool canSameNumber)
    {
      var list = new List<(byte, byte, byte)>();
      foreach (var n1 in num1)
      {
        if (num2 != null)
        {
          foreach (var n2 in num2)
          {
            if (!canSameNumber)
            {
              if (n1 == n2) continue;
            }
            if (!isOrder)
            {
              if (list.Any(i => i.Item1 == n2 && i.Item2 == n1)) continue;
            }
            if (num3 != null)
            {
              foreach (var n3 in num3)
              {
                if (!canSameNumber)
                {
                  if (n2 == n3) continue;
                }
                if (!isOrder)
                {
                  if (list.Any(i => i.Item1 == n1 && i.Item2 == n3 && i.Item3 == n2)) continue;
                  if (list.Any(i => i.Item1 == n2 && i.Item2 == n1 && i.Item3 == n3)) continue;
                  if (list.Any(i => i.Item1 == n2 && i.Item2 == n3 && i.Item3 == n1)) continue;
                  if (list.Any(i => i.Item1 == n3 && i.Item2 == n1 && i.Item3 == n2)) continue;
                  if (list.Any(i => i.Item1 == n3 && i.Item2 == n2 && i.Item3 == n1)) continue;
                }
                list.Add((n1, n2, n3));
              }
            }
            else
            {
              list.Add((n1, n2, default));
            }
          }
        }
        else
        {
          list.Add((n1, default, default));
        }
      }

      return list;
    }
  }

  public class SingleTicketItem : TicketItem
  {
    public SingleTicketItem(TicketData data, OddsInfo odds) : base(data)
    {
      if (odds.Singles.Value != null)
      {
        this.SetRows(data.Numbers1
          .Join(odds.Singles.Value, n => n, o => o.Number1, (n, o) => new { Number = n, Odds = o.Odds, })
          .Select(n => new TicketItemRow
          {
            Number1 = n.Number,
            Money = n.Odds * 10,
          })
          .ToArray());
      }
      else
      {
        this.SetRows(data.Numbers1
          .Select(n => new TicketItemRow
          {
            Number1 = n,
          })
          .ToArray());
      }
    }

    private SingleTicketItem(TicketData data, int money) : base(data)
    {
      this.SetRows(new[] { new TicketItemRow
      {
        Money = money,
        Number1 = data.Numbers1[0],
      }, });
    }

    protected override TicketItem GenerateNewItem(TicketData data, int money, int moneyMax)
    {
      data.Type = TicketType.Single;
      return new SingleTicketItem(data, money);
    }
  }

  public class PlaceTicketItem : TicketItem
  {
    public PlaceTicketItem(TicketData data, OddsInfo odds) : base(data)
    {
      if (odds.Singles.Value != null)
      {
        this.SetRows(data.Numbers1
          .Join(odds.Singles.Value, n => n, o => o.Number1, (n, o) => new { Number = n, Odds = o.PlaceOddsMin, OddsMax = o.PlaceOddsMax, })
          .Select(n => new TicketItemRow
          {
            Number1 = n.Number,
            Money = n.Odds * 10,
            MoneyMax = n.OddsMax * 10,
          })
          .ToArray());
      }
      else
      {
        this.SetRows(data.Numbers1
          .Select(n => new TicketItemRow
          {
            Number1 = n,
          })
          .ToArray());
      }
    }

    private PlaceTicketItem(TicketData data, int money, int moneyMax) : base(data)
    {
      this.SetRows(new[] { new TicketItemRow
      {
        Money = money,
        MoneyMax = moneyMax,
        Number1 = data.Numbers1[0],
      }, });
    }

    protected override TicketItem GenerateNewItem(TicketData data, int money, int moneyMax)
    {
      data.Type = TicketType.Place;
      return new PlaceTicketItem(data, money, moneyMax);
    }
  }

  public class FrameNumberTicketItem : TicketItem
  {
    public FrameNumberTicketItem(TicketData data, OddsInfo odds, IEnumerable<RaceHorseData> horses) : base(data)
    {
      var nums = GetFormationNumbers(data.Numbers1, data.Numbers2, null, false, true);
      var rows = new List<TicketItemRow>();

      if (odds.Frames.Value != null)
      {
        foreach (var n in nums)
        {
          // 枠連は同じ数字同士もOK
          if (n.Item1 == n.Item2)
          {
            if (horses
              .Where(h => h.AbnormalResult != RaceAbnormality.Scratched && h.AbnormalResult != RaceAbnormality.ExcludedByStarters)
              .GroupBy(h => h.FrameNumber).Count(h => h.Key == n.Item1) <= 1)
            {
              continue;
            }
          }

          var od = odds.Frames.Value.Columns.SelectMany(c => c.Odds).FirstOrDefault(o => o.Data.Frame1 == n.Item1 && o.Data.Frame2 == n.Item2);
          rows.Add(new TicketItemRow
          {
            Money = od.Data.Odds * 10,
            Number1 = n.Item1,
            Number2 = n.Item2,
          });
        }
      }
      else
      {
        foreach (var n in nums)
        {
          // 枠連は同じ数字同士もOK
          if (n.Item1 == n.Item2)
          {
            if (horses
              .Where(h => h.AbnormalResult != RaceAbnormality.Scratched && h.AbnormalResult != RaceAbnormality.ExcludedByStarters)
              .GroupBy(h => h.FrameNumber).Count(h => h.Key == n.Item1) <= 1)
            {
              continue;
            }
          }

          rows.Add(new TicketItemRow
          {
            Number1 = n.Item1,
            Number2 = n.Item2,
          });
        }
      }

      this.SetRows(rows);
    }

    private FrameNumberTicketItem(TicketData data, int money) : base(data)
    {
      this.SetRows(new[] { new TicketItemRow
      {
        Money = money,
        Number1 = data.Numbers1[0],
        Number2 = data.Numbers2[0],
      }, });
    }

    protected override TicketItem GenerateNewItem(TicketData data, int money, int moneyMax)
    {
      data.Type = TicketType.FrameNumber;
      return new FrameNumberTicketItem(data, money);
    }

    public override IReadOnlyList<TicketItemRow> GetHitRows(short num1, short num2, short num3, short frame1, short frame2, short frame3)
    {
      var rows = this.Rows.Where(r => r.Number1 == frame1 && (frame2 == default || r.Number2 == frame2) && (frame3 == default || r.Number3 == frame3));
      return rows.ToArray();
    }
  }

  public class TicketItemRow
  {
    public short Number1 { get; init; }

    public short Number2 { get; init; }

    public short Number3 { get; init; }

    public int Money { get; init; }

    public int MoneyMax { get; init; }

    public ReactiveProperty<bool> IsChecked { get; } = new();
  }

  public class BettingHorseItem : IDisposable
  {
    public short HorseNumber { get; init; }

    public string Name { get; init; } = string.Empty;

    public ReactiveProperty<bool> IsChecked { get; } = new();

    public ReactiveProperty<bool> IsEnabled { get; } = new();

    public BettingHorseItem(RaceHorseAnalyzer horse)
    {
      this.IsEnabled = horse.Mark
        .Select(m => m != RaceHorseMark.Deleted && horse.Data.AbnormalResult != RaceAbnormality.Scratched && horse.Data.AbnormalResult != RaceAbnormality.ExcludedByStarters)
        .ToReactiveProperty();
    }

    public void Dispose()
    {
      this.IsEnabled.Dispose();
    }
  }

  public class BettingFrameItem : IDisposable
  {
    private readonly CompositeDisposable _disposables = new();

    public short FrameNumber { get; init; }

    public ReactiveProperty<bool> IsChecked { get; } = new();

    public ReactiveProperty<bool> IsEnabled { get; } = new();

    public BettingFrameItem(IEnumerable<RaceHorseAnalyzer> horses)
    {
      if (horses.Any())
      {
        void OnHorsesChanged()
        {
          var isDeleted = horses.All(h => h.Mark.Value == RaceHorseMark.Deleted || h.Data.AbnormalResult == RaceAbnormality.Scratched || h.Data.AbnormalResult == RaceAbnormality.ExcludedByStarters);
          this.IsEnabled.Value = !isDeleted;
        }

        var marks = (IObservable<RaceHorseMark>)horses.First().Mark;
        foreach (var horse in horses.Skip(1))
        {
          marks = marks.Concat(horse.Mark);
        }
        marks.Subscribe(m =>
        {
          OnHorsesChanged();
        }).AddTo(this._disposables);

        OnHorsesChanged();
      }
    }

    public void Dispose()
    {
      this._disposables.Dispose();
    }
  }
}
