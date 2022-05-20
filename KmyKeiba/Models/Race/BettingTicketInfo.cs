using KmyKeiba.Data.Db;
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

    public ReactiveProperty<string> Count { get; } = new("1");

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
        else if (ticket.Type == TicketType.QuinellaPlace)
        {
          this.Tickets.Add(new QuinellaPlaceTicketItem(ticket, odds, this._horses.Count));
        }
        else if (ticket.Type == TicketType.Quinella)
        {
          this.Tickets.Add(new QuinellaTicketItem(ticket, odds));
        }
        else if (ticket.Type == TicketType.Exacta)
        {
          this.Tickets.Add(new ExactaTicketItem(ticket, odds));
        }
      }

      this.SortTickets();
    }

    private void SortTickets()
    {
      var oldItems = this.Tickets.ToArray();
      this.Tickets.Clear();
      foreach (var item in oldItems.OrderBy(i => i))
      {
        this.Tickets.Add(item);
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
      short.TryParse(this.Count.Value, out var count);
      if (count <= 0)
      {
        return;
      }
      
      using var db = new MyContext();
      IReadOnlyList<TicketItem>? tickets = null;
      if (this.Type.Value == TicketType.Single)
      {
        tickets = await this.GenerateSingleTicketsAsync(db, count);
      }
      else if (this.Type.Value == TicketType.Place)
      {
        tickets = await this.GeneratePlaceTicketsAsync(db, count);
      }
      else if (this.Type.Value == TicketType.FrameNumber)
      {
        tickets = await this.GenerateFrameNumberTicketsAsync(db, count);
      }
      else if (this.Type.Value == TicketType.QuinellaPlace)
      {
        tickets = await this.GenerateQuinellaPlaceTicketsAsync(db, count);
      }
      else if (this.Type.Value == TicketType.Quinella)
      {
        tickets = await this.GenerateQuinellaTicketsAsync(db, count);
      }
      else if (this.Type.Value == TicketType.Exacta)
      {
        tickets = await this.GenerateExactaTicketsAsync(db, count);
      }

      if (tickets == null)
      {
        return;
      }

      foreach (var t in tickets)
      {
        t.Data.Count = count;
        this.Tickets.Add(t);
      }
      this.SortTickets();

      await db.Tickets!.AddRangeAsync(tickets.Select(t => t.Data));
      await db.SaveChangesAsync();
    }

    private async Task<IReadOnlyList<TicketItem>?> GenerateSingleTicketsAsync(MyContext db, int count)
      => await this.GenerateSingleNumberTicketsAsync(db, count, TicketType.Single, data => new SingleTicketItem(data, this.Odds));

    private async Task<IReadOnlyList<TicketItem>?> GeneratePlaceTicketsAsync(MyContext db, int count)
      => await this.GenerateSingleNumberTicketsAsync(db, count, TicketType.Place, data => new PlaceTicketItem(data, this.Odds));

    private async Task<IReadOnlyList<TicketItem>?> GenerateFrameNumberTicketsAsync(MyContext db, int count)
      => await this.GenerateDoubleNumberTicketsAsync(db, count, TicketType.FrameNumber, data => new FrameNumberTicketItem(data, this.Odds, this._horses));

    private async Task<IReadOnlyList<TicketItem>?> GenerateQuinellaPlaceTicketsAsync(MyContext db, int count)
      => await this.GenerateDoubleNumberTicketsAsync(db, count, TicketType.QuinellaPlace, data => new QuinellaPlaceTicketItem(data, this.Odds, this._horses.Count));

    private async Task<IReadOnlyList<TicketItem>?> GenerateQuinellaTicketsAsync(MyContext db, int count)
      => await this.GenerateDoubleNumberTicketsAsync(db, count, TicketType.Quinella, data => new QuinellaTicketItem(data, this.Odds));

    private async Task<IReadOnlyList<TicketItem>?> GenerateExactaTicketsAsync(MyContext db, int count)
      => await this.GenerateDoubleNumberTicketsAsync(db, count, TicketType.Exacta, data => new ExactaTicketItem(data, this.Odds));

    private async Task<IReadOnlyList<TicketItem>?> GenerateSingleNumberTicketsAsync(MyContext db, int count, TicketType type, Func<TicketData, TicketItem> itemGenerator)
    {
      var list = new List<TicketItem>();
      var isChanged = false;
      foreach (var num in this.Numbers1.Where(n => n.IsChecked.Value == true).Select(n => (byte)n.HorseNumber))
      {
        var hit = false;

        foreach (var ticket in this.Tickets.Where(t => t.Type == type))
        {
          if (ticket.Number1 == num)
          {
            db.Tickets!.Attach(ticket.Data);
            ticket.Data.Count = (short)(ticket.Data.Count + count);
            ticket.Count.Value = ticket.Data.Count;
            hit = true;
            isChanged = true;
          }
        }

        if (!hit)
        {
          var data = new TicketData
          {
            RaceKey = this._raceKey,
            Type = type,
            FormType = TicketFormType.Single,
            Numbers1 = new byte[] { num, },
            Count = (short)count,
          };
          var item = itemGenerator(data);
          list.Add(item);
        }
      }

      if (isChanged) await db.SaveChangesAsync();

      if (list.Any()) return list;
      return null;
    }

    private async Task<IReadOnlyList<TicketItem>?> GenerateDoubleNumberTicketsAsync(MyContext db, int count, TicketType type, Func<TicketData, TicketItem> itemGenerator)
    {
      var list = new List<TicketItem>();
      var isChanged = false;
      var hit = false;
      var isOrder = type == TicketType.Exacta;

      var nums1 = (type == TicketType.FrameNumber ? this.FrameNumbers1.Where(n => n.IsChecked.Value).Select(n => (byte)n.FrameNumber) :
        this.Numbers1.Where(n => n.IsChecked.Value).Select(n => (byte)n.HorseNumber)).ToArray();
      var nums2 = (type == TicketType.FrameNumber ? this.FrameNumbers2.Where(n => n.IsChecked.Value).Select(n => (byte)n.FrameNumber) :
        this.Numbers2.Where(n => n.IsChecked.Value).Select(n => (byte)n.HorseNumber)).ToArray();

      if (!nums1.Any())
      {
        return null;
      }

      var formType = this.FormType.Value;
      if (nums1.Length == 1 && nums2.Length == 1 && !isOrder)
      {
        formType = TicketFormType.Single;
      }
      if (formType == TicketFormType.Box)
      {
        nums2 = nums1;
      }

      foreach (var ticket in this.Tickets.Where(t => t.Type == type))
      {
        if (ticket.Data.FormType == formType)
        {
          if ((ticket.Data.Numbers1.SequenceEqual(nums1) && ticket.Data.Numbers2.SequenceEqual(nums2)) ||
            (!isOrder && (ticket.Data.Numbers1.SequenceEqual(nums2) && ticket.Data.Numbers2.SequenceEqual(nums1))))
          {
            db.Tickets!.Attach(ticket.Data);
            ticket.Data.Count = (short)(ticket.Data.Count + count);
            ticket.Count.Value = ticket.Data.Count;
            hit = true;
            isChanged = true;
            break;
          }
        }
      }
      if (!hit)
      {
        if (formType == TicketFormType.Formation || formType == TicketFormType.Box)
        {
          if (!nums2.Any())
          {
            return null;
          }
        }

        var data = new TicketData
        {
          RaceKey = this._raceKey,
          Type = type,
          FormType = formType,
          Numbers1 = nums1,
          Numbers2 = nums2,
          Count = (short)count,
        };
        var item = itemGenerator(data);
        if (item.Rows.Any())
        {
          list.Add(item);
        }
      }

      if (isChanged) await db.SaveChangesAsync();

      if (list.Any()) return list;
      return null;
    }

    public async Task RemoveTicketAsync()
    {
      var targetTickets = this.Tickets
        .Where(t => t.IsAllRowsChecked.Value || t.Rows.Any(r => r.IsChecked.Value))
        .ToArray();
      if (!targetTickets.Any())
      {
        return;
      }

      using var db = new MyContext();

      var removeTickets = new List<TicketItem>();
      foreach (var ticket in targetTickets)
      {
        var index = this.Tickets.IndexOf(ticket);
        this.Tickets.RemoveAt(index);

        db.Tickets!.Remove(ticket.Data);

        if (ticket.IsAllRowsChecked.Value)
        {
          continue;
        }

        // フォーメーション、ボックスなどのうち一部だけを残す
        var broken = ticket.Break();
        var leaves = broken.Where(r => r.Rows.Any() && r.Rows.All(rr => !rr.IsChecked.Value)).ToArray();
        foreach (var item in leaves)
        {
          var exists = this.Tickets.FirstOrDefault(t => t.IsSingleRow && t.Type == ticket.Type && t.Number1 == item.Number1 && t.Number2 == item.Number2 && t.Number3 == item.Number3);
          if (exists != null)
          {
            db.Tickets!.Attach(exists.Data);
            exists.Count.Value += item.Count.Value;
            exists.Data.Count = (short)exists.Count.Value;
          }
          else
          {
            this.Tickets.Insert(index++, item);
            await db.Tickets!.AddAsync(item.Data);
          }
        }
      }

      this.SortTickets();
      await db.SaveChangesAsync();
    }
  }

  public abstract class TicketItem : IComparable<TicketItem>
  {
    public TicketData Data { get; }

    public ReactiveCollection<TicketItemRow> Rows { get; private set; } = new();

    public ReactiveProperty<int> Count { get; } = new();

    public ReactiveProperty<bool> IsOpen { get; } = new();

    public ReactiveProperty<bool> IsAllRowsChecked { get; } = new();

    public bool IsSingleRow => this.Rows.Count == 1;

    public short Number1 => this.Rows[0].Number1;

    public short Number2 => this.Rows[0].Number2;

    public short Number3 => this.Rows[0].Number3;

    public int Money => this.Rows[0].Money;

    public int MoneyMax => this.Rows[0].MoneyMax;

    public TicketType Type => this.Rows[0].Type;

    public ReactiveProperty<bool> IsChecked => this.Rows[0].IsChecked;

    public TicketItem(TicketData data)
    {
      this.Data = data;
      this.Count.Value = data.Count;
    }

    protected void SetRows(IReadOnlyList<TicketItemRow> rows)
    {
      foreach (var row in rows)
      {
        row.Type = this.Data.Type;
        row.IsSingleRow = rows.Count == 1;
        row.IsAllRowsChecked = this.IsAllRowsChecked;
        this.Rows.Add(row);
      }
    }

    public IReadOnlyList<TicketItem> Break()
    {
      if (this.Rows.Count == 1)
      {
        return new[] { this, };
      }

      var isOrder = this.Type == TicketType.Exacta || this.Type == TicketType.Trifecta;
      var rows = new List<TicketItem>();
      foreach (var row in this.Rows)
      {
        var nums = new[] { row.Number1, row.Number2, row.Number3, };
        if (!isOrder)
        {
          nums = nums
            .Select(n => n == default ? byte.MaxValue : n)
            .OrderBy(n => n)
            .Select(n => n == byte.MaxValue ? default : n)
            .ToArray();
        }

        var data = new TicketData
        {
          RaceKey = this.Data.RaceKey,
          FormType = TicketFormType.Single,
          Numbers1 = new[] { (byte)nums[0], },
          Count = (short)this.Count.Value,
        };
        if (nums[1] != default) data.Numbers2 = new byte[] { (byte)nums[1], };
        if (nums[2] != default) data.Numbers3 = new byte[] { (byte)nums[2], };

        var item = this.GenerateNewItem(data, row.Money, row.MoneyMax);
        item.Rows[0].IsChecked.Value = row.IsChecked.Value;
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
      foreach (var n1 in num1.OrderBy(n => n))
      {
        if (num2 != null)
        {
          foreach (var n2 in num2.OrderBy(n => n))
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
              foreach (var n3 in num3.OrderBy(n => n))
              {
                if (!canSameNumber)
                {
                  if (n2 == n3) continue;
                }
                if (!isOrder)
                {
                  if (list.Any(i => i.Item1 == n1 && i.Item2 == n2 && i.Item3 == n3)) continue;
                  if (list.Any(i => i.Item1 == n1 && i.Item2 == n3 && i.Item3 == n2)) continue;
                  if (list.Any(i => i.Item1 == n2 && i.Item2 == n1 && i.Item3 == n3)) continue;
                  if (list.Any(i => i.Item1 == n2 && i.Item2 == n3 && i.Item3 == n1)) continue;
                  if (list.Any(i => i.Item1 == n3 && i.Item2 == n1 && i.Item3 == n2)) continue;
                  if (list.Any(i => i.Item1 == n3 && i.Item2 == n2 && i.Item3 == n1)) continue;
                }

                if (!isOrder)
                {
                  var n = new[] { n1, n2, n3, };
                  n = n.OrderBy(nn => nn).ToArray();
                  list.Add((n[0], n[1], n[2]));
                }
                else
                {
                  list.Add((n1, n2, n3));
                }
              }
            }
            else
            {
              if (!isOrder && n1 > n2)
              {
                list.Add((n2, n1, default));
              }
              else
              {
                list.Add((n1, n2, default));
              }
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

    private int GetCompareValue() => (short)this.Type * 1000000 + this.Rows[0].Number1 * 10000 + this.Rows[0].Number2 * 100 + this.Rows[0].Number3;

    public int CompareTo(TicketItem? other)
    {
      return this.GetCompareValue() - (other?.GetCompareValue() ?? 0);
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

    public override string ToString()
    {
      if (this.IsSingleRow)
      {
        return this.Data.Numbers1[0].ToString();
      }
      return string.Join(',', this.Data.Numbers1);
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

    public override IReadOnlyList<TicketItemRow> GetHitRows(short num1, short num2, short num3, short frame1, short frame2, short frame3)
    {
      var rows = this.Rows.Where(r => r.Number1 == num1 || r.Number1 == num2 || r.Number1 == num3);
      return rows.ToArray();
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
              .Count(h => h.FrameNumber == n.Item1) <= 1)
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

    public override string ToString()
    {
      if (this.Data.FormType == TicketFormType.Formation)
      {
        return "フォメ " + string.Join(',', this.Data.Numbers1) + " - " + string.Join(',', this.Data.Numbers2);
      }
      else if (this.Data.FormType == TicketFormType.Box)
      {
        return "BOX " + string.Join(',', this.Data.Numbers1) + " - " + string.Join(',', this.Data.Numbers2);
      }
      return base.ToString()!;
    }
  }

  public class QuinellaPlaceTicketItem : TicketItem
  {
    private readonly bool _isUnder7;

    public QuinellaPlaceTicketItem(TicketData data, OddsInfo odds, int horsesCount) : base(data)
    {
      this._isUnder7 = horsesCount <= 7;

      var nums = GetFormationNumbers(data.Numbers1, data.Numbers2, null, false, false);
      var rows = new List<TicketItemRow>();

      if (odds.QuinellaPlaces.Value != null)
      {
        foreach (var n in nums)
        {
          var od = odds.QuinellaPlaces.Value.Columns.SelectMany(c => c.Odds).FirstOrDefault(o => o.Data.HorseNumber1 == n.Item1 && o.Data.HorseNumber2 == n.Item2);
          rows.Add(new TicketItemRow
          {
            Money = od.Data.PlaceOddsMin * 10,
            MoneyMax = od.Data.PlaceOddsMax * 10,
            Number1 = n.Item1,
            Number2 = n.Item2,
          });
        }
      }
      else
      {
        foreach (var n in nums)
        {
          rows.Add(new TicketItemRow
          {
            Number1 = n.Item1,
            Number2 = n.Item2,
          });
        }
      }

      this.SetRows(rows);
    }

    private QuinellaPlaceTicketItem(TicketData data, int money, int moneyMax, bool isUnder7) : base(data)
    {
      this._isUnder7 = isUnder7;
      this.SetRows(new[] { new TicketItemRow
      {
        Money = money,
        MoneyMax = moneyMax,
        Number1 = data.Numbers1[0],
        Number2 = data.Numbers2[0],
      }, });
    }

    protected override TicketItem GenerateNewItem(TicketData data, int money, int moneyMax)
    {
      data.Type = TicketType.QuinellaPlace;
      return new QuinellaPlaceTicketItem(data, money, moneyMax, this._isUnder7);
    }

    public override IReadOnlyList<TicketItemRow> GetHitRows(short num1, short num2, short num3, short frame1, short frame2, short frame3)
    {
      if (this._isUnder7)
      {
        var rows = this.Rows.Where(r => (r.Number1 == num1 && r.Number2 == num2) ||
                                        (r.Number1 == num2 && r.Number2 == num1));
        return rows.ToArray();
      }
      else
      {
        var rows = this.Rows.Where(r => (r.Number1 == num1 && r.Number2 == num2) ||
                                        (r.Number1 == num1 && r.Number2 == num3) ||
                                        (r.Number1 == num2 && r.Number2 == num1) ||
                                        (r.Number1 == num2 && r.Number2 == num3) ||
                                        (r.Number1 == num3 && r.Number2 == num1) ||
                                        (r.Number1 == num3 && r.Number2 == num2));
        return rows.ToArray();
      }
    }

    public override string ToString()
    {
      if (this.Data.FormType == TicketFormType.Formation)
      {
        return "フォメ " + string.Join(',', this.Data.Numbers1) + " - " + string.Join(',', this.Data.Numbers2);
      }
      else if (this.Data.FormType == TicketFormType.Box)
      {
        return "BOX " + string.Join(',', this.Data.Numbers1) + " - " + string.Join(',', this.Data.Numbers2);
      }
      return base.ToString()!;
    }
  }

  public class QuinellaTicketItem : TicketItem
  {
    public QuinellaTicketItem(TicketData data, OddsInfo odds) : base(data)
    {
      var nums = GetFormationNumbers(data.Numbers1, data.Numbers2, null, false, false);
      var rows = new List<TicketItemRow>();

      if (odds.Quinellas.Value != null)
      {
        foreach (var n in nums)
        {
          var od = odds.Quinellas.Value.Columns.SelectMany(c => c.Odds).FirstOrDefault(o => o.Data.HorseNumber1 == n.Item1 && o.Data.HorseNumber2 == n.Item2);
          rows.Add(new TicketItemRow
          {
            Money = (int)(od.Data.Odds * 10),
            Number1 = n.Item1,
            Number2 = n.Item2,
          });
        }
      }
      else
      {
        foreach (var n in nums)
        {
          rows.Add(new TicketItemRow
          {
            Number1 = n.Item1,
            Number2 = n.Item2,
          });
        }
      }

      this.SetRows(rows);
    }

    private QuinellaTicketItem(TicketData data, int money, int moneyMax) : base(data)
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
      data.Type = TicketType.Quinella;
      return new QuinellaTicketItem(data, money, moneyMax);
    }

    public override IReadOnlyList<TicketItemRow> GetHitRows(short num1, short num2, short num3, short frame1, short frame2, short frame3)
    {
      var rows = this.Rows.Where(r => (r.Number1 == num1 && r.Number2 == num2) ||
                                      (r.Number1 == num2 && r.Number2 == num1));
      return rows.ToArray();
    }

    public override string ToString()
    {
      if (this.Data.FormType == TicketFormType.Formation)
      {
        return "フォメ " + string.Join(',', this.Data.Numbers1) + " - " + string.Join(',', this.Data.Numbers2);
      }
      else if (this.Data.FormType == TicketFormType.Box)
      {
        return "BOX " + string.Join(',', this.Data.Numbers1) + " - " + string.Join(',', this.Data.Numbers2);
      }
      return base.ToString()!;
    }
  }

  public class ExactaTicketItem : TicketItem
  {
    public ExactaTicketItem(TicketData data, OddsInfo odds) : base(data)
    {
      var nums = GetFormationNumbers(data.Numbers1, data.Numbers2, null, true, false);
      var rows = new List<TicketItemRow>();

      if (odds.Exactas.Value != null)
      {
        foreach (var n in nums)
        {
          var od = odds.Exactas.Value.Columns.SelectMany(c => c.Odds).FirstOrDefault(o => o.Data.HorseNumber1 == n.Item1 && o.Data.HorseNumber2 == n.Item2);
          rows.Add(new TicketItemRow
          {
            Money = (int)(od.Data.Odds * 10),
            Number1 = n.Item1,
            Number2 = n.Item2,
          });
        }
      }
      else
      {
        foreach (var n in nums)
        {
          rows.Add(new TicketItemRow
          {
            Number1 = n.Item1,
            Number2 = n.Item2,
          });
        }
      }

      this.SetRows(rows);
    }

    private ExactaTicketItem(TicketData data, int money) : base(data)
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
      data.Type = TicketType.Quinella;
      return new ExactaTicketItem(data, money);
    }

    public override string ToString()
    {
      if (this.Data.FormType == TicketFormType.Formation)
      {
        return "フォメ " + string.Join(',', this.Data.Numbers1) + " - " + string.Join(',', this.Data.Numbers2);
      }
      else if (this.Data.FormType == TicketFormType.Box)
      {
        return "BOX " + string.Join(',', this.Data.Numbers1) + " - " + string.Join(',', this.Data.Numbers2);
      }
      return base.ToString()!;
    }
  }

  public class TicketItemRow
  {
    public TicketType Type { get; set; }

    public bool IsSingleRow { get; set; }

    public short Number1 { get; init; }

    public short Number2 { get; init; }

    public short Number3 { get; init; }

    public int Money { get; init; }

    public int MoneyMax { get; init; }

    public ReactiveProperty<int>? Count => null;  // xamlバインディング用

    public ReactiveProperty<bool> IsChecked { get; } = new();

    public ReactiveProperty<bool>? IsAllRowsChecked { get; set; }
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
