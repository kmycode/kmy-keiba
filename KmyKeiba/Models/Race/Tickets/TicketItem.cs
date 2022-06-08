using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Analysis.Generic;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.Tickets
{
  public abstract class TicketItem : IComparable<TicketItem>, IDisposable
  {
    private readonly CompositeDisposable _disposables = new();

    public TicketData Data { get; }

    public MultipleCheckableCollection<TicketItemRow> Rows { get; private set; } = new();

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

    public ReactiveProperty<ValueComparation> Comparation => this.Rows[0].Comparation;

    public ReactiveProperty<bool> IsChecked => this.Rows[0].IsChecked;

    public TicketItem(TicketData data)
    {
      this.Data = data;
      this.Count.Value = data.Count;

      this.Count.Subscribe(c =>
      {
        foreach (var row in this.Rows)
        {
          row.DataCount = c;
        }
      }).AddTo(this._disposables);
    }

    protected void SetRows(IReadOnlyList<TicketItemRow> rows)
    {
      foreach (var row in rows)
      {
        row.Type = this.Data.Type;
        row.IsSingleRow = rows.Count == 1;
        row.IsAllRowsChecked = this.IsAllRowsChecked;
        row.DataCount = this.Data.Count;
        this.Rows.Add(row);
      }

      // フォーメーションで「２個ー１個」と「１個ー２個」などが違う馬券としてカウントされるのを修正する
      if (rows.Count == 1)
      {
        var row = rows[0];
        this.Data.Numbers1 = new[] { (byte)row.Number1, };
        if (row.Number2 != default) this.Data.Numbers2 = new[] { (byte)row.Number2, };
        if (row.Number3 != default) this.Data.Numbers3 = new[] { (byte)row.Number3, };
        this.Data.FormType = TicketFormType.Single;
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

    public virtual int CountAvailableRows(IReadOnlyList<RaceHorseData> horses)
    {
      var abnormalHorses = horses.Where(h => h.AbnormalResult != RaceAbnormality.Unknown).Select(h => h.Number);
      if (!abnormalHorses.Any())
      {
        return this.Rows.Count;
      }

      return this.Rows.Count(r => !abnormalHorses.Contains(r.Number1) && !abnormalHorses.Contains(r.Number2) && !abnormalHorses.Contains(r.Number3));
    }

    protected abstract TicketItem GenerateNewItem(TicketData data, int money, int moneyMax);

    public abstract IReadOnlyList<TicketItemRow> GetHitRows(short num1, short num2, short num3, short frame1, short frame2);

    protected static IReadOnlyList<(byte, byte, byte)> GetFormationNumbers(byte[] num1, byte[]? num2, byte[]? num3, bool isOrder, bool canSameNumber, bool isMulti)
    {
      if (isOrder && isMulti)
      {
        isOrder = false;
      }

      var list = new List<(byte, byte, byte)>();
      void AddMultiItem(byte a, byte b, byte c)
      {
        if (a == default || c != default && b == default)
        {
          return;
        }
        if (!list!.Any(i => i.Item1 == a && i.Item2 == b && i.Item3 == c))
        {
          list.Add((a, b, c));
        }
      }

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
                  if (n2 == n3 || n1 == n3) continue;
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

                  if (isMulti)
                  {
                    AddMultiItem(n[0], n[2], n[1]);
                    AddMultiItem(n[1], n[0], n[2]);
                    AddMultiItem(n[1], n[2], n[0]);
                    AddMultiItem(n[2], n[0], n[1]);
                    AddMultiItem(n[2], n[1], n[0]);
                  }
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
                if (isMulti)
                {
                  AddMultiItem(n1, n2, default);
                }
              }
              else
              {
                list.Add((n1, n2, default));
                if (isMulti)
                {
                  AddMultiItem(n2, n1, default);
                }
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

    public override string ToString()
    {
      return this.Data.ToSummaryString();
    }

    public void Dispose()
    {
      this.Rows.Dispose();
      this._disposables.Dispose();
    }

    public static TicketItem? FromData(TicketData ticket, IReadOnlyList<RaceHorseData> horses, OddsInfo? odds)
    {
      odds ??= new OddsInfo(horses, null, null, null, null, null, null);

      if (ticket.Type == TicketType.Single)
      {
        return new SingleTicketItem(ticket, odds);
      }
      else if (ticket.Type == TicketType.Place)
      {
        return new PlaceTicketItem(ticket, odds);
      }
      else if (ticket.Type == TicketType.FrameNumber)
      {
        return new FrameNumberTicketItem(ticket, odds, horses);
      }
      else if (ticket.Type == TicketType.QuinellaPlace)
      {
        return new QuinellaPlaceTicketItem(ticket, odds, horses.Count);
      }
      else if (ticket.Type == TicketType.Quinella)
      {
        return new QuinellaTicketItem(ticket, odds);
      }
      else if (ticket.Type == TicketType.Exacta)
      {
        return new ExactaTicketItem(ticket, odds);
      }
      else if (ticket.Type == TicketType.Trio)
      {
        return new TrioTicketItem(ticket, odds);
      }
      else if (ticket.Type == TicketType.Trifecta)
      {
        return new TrifectaTicketItem(ticket, odds);
      }

      return null;
    }
  }

  public class SingleTicketItem : TicketItem
  {
    public SingleTicketItem(TicketData data, OddsInfo odds) : base(data)
    {
      if (odds.Singles.Value != null)
      {
        this.SetRows(data.Numbers1
          .Join(odds.Singles.Value, n => n, o => o.Number1, (n, o) => new { Number = n, o.Odds, })
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

    public override IReadOnlyList<TicketItemRow> GetHitRows(short num1, short num2, short num3, short frame1, short frame2)
    {
      var rows = this.Rows.Where(r => r.Number1 == num1);
      return rows.ToArray();
    }
  }

  public class PlaceTicketItem : TicketItem
  {
    public PlaceTicketItem(TicketData data, OddsInfo odds) : base(data)
    {
      if (odds.Singles.Value != null && odds.Singles.Value.Any())
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

    public override IReadOnlyList<TicketItemRow> GetHitRows(short num1, short num2, short num3, short frame1, short frame2)
    {
      var rows = this.Rows.Where(r => r.Number1 == num1 || r.Number1 == num2 || r.Number1 == num3);
      return rows.ToArray();
    }
  }

  public class FrameNumberTicketItem : TicketItem
  {
    public FrameNumberTicketItem(TicketData data, OddsInfo odds, IEnumerable<RaceHorseData> horses) : base(data)
    {
      var nums = GetFormationNumbers(data.Numbers1, data.Numbers2, null, isOrder: false, canSameNumber: true, isMulti: false);
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

    public override IReadOnlyList<TicketItemRow> GetHitRows(short num1, short num2, short num3, short frame1, short frame2)
    {
      var rows = this.Rows.Where(r => r.Number1 == frame1 && (frame2 == default || r.Number2 == frame2));
      return rows.ToArray();
    }

    public override int CountAvailableRows(IReadOnlyList<RaceHorseData> horses)
    {
      var availableFrames = horses.Where(h => h.AbnormalResult == RaceAbnormality.Unknown).Select(h => h.FrameNumber);
      return this.Rows.Count(r => availableFrames.Contains(r.Number1) && availableFrames.Contains(r.Number2));
    }
  }

  public class QuinellaPlaceTicketItem : TicketItem
  {
    private readonly bool _isUnder7;

    public QuinellaPlaceTicketItem(TicketData data, OddsInfo odds, int horsesCount) : base(data)
    {
      this._isUnder7 = horsesCount <= 7;

      var nums = GetFormationNumbers(data.Numbers1, data.Numbers2, null, isOrder: false, canSameNumber: false, isMulti: false);
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

    public override IReadOnlyList<TicketItemRow> GetHitRows(short num1, short num2, short num3, short frame1, short frame2)
    {
      if (this._isUnder7)
      {
        var rows = this.Rows.Where(r => r.Number1 == num1 && r.Number2 == num2 ||
                                        r.Number1 == num2 && r.Number2 == num1);
        return rows.ToArray();
      }
      else
      {
        var rows = this.Rows.Where(r => r.Number1 == num1 && r.Number2 == num2 ||
                                        r.Number1 == num1 && r.Number2 == num3 ||
                                        r.Number1 == num2 && r.Number2 == num1 ||
                                        r.Number1 == num2 && r.Number2 == num3 ||
                                        r.Number1 == num3 && r.Number2 == num1 ||
                                        r.Number1 == num3 && r.Number2 == num2);
        return rows.ToArray();
      }
    }
  }

  public class QuinellaTicketItem : TicketItem
  {
    public QuinellaTicketItem(TicketData data, OddsInfo odds) : base(data)
    {
      var nums = GetFormationNumbers(data.Numbers1, data.Numbers2, null, isOrder: false, canSameNumber: false, isMulti: false);
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

    public override IReadOnlyList<TicketItemRow> GetHitRows(short num1, short num2, short num3, short frame1, short frame2)
    {
      var rows = this.Rows.Where(r => r.Number1 == num1 && r.Number2 == num2 ||
                                      r.Number1 == num2 && r.Number2 == num1);
      return rows.ToArray();
    }
  }

  public class ExactaTicketItem : TicketItem
  {
    public ExactaTicketItem(TicketData data, OddsInfo odds) : base(data)
    {
      var nums = GetFormationNumbers(data.Numbers1, data.Numbers2, null, isOrder: true, canSameNumber: false, isMulti: data.IsMulti);
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
      data.Type = TicketType.Exacta;
      return new ExactaTicketItem(data, money);
    }

    public override IReadOnlyList<TicketItemRow> GetHitRows(short num1, short num2, short num3, short frame1, short frame2)
    {
      var rows = this.Rows.Where(r => r.Number1 == num1 && r.Number2 == num2);
      return rows.ToArray();
    }
  }

  public class TrioTicketItem : TicketItem
  {
    public TrioTicketItem(TicketData data, OddsInfo odds) : base(data)
    {
      var nums = GetFormationNumbers(data.Numbers1, data.Numbers2, data.Numbers3, isOrder: false, canSameNumber: false, isMulti: false);
      var rows = new List<TicketItemRow>();

      if (odds.Trios.Value != null)
      {
        foreach (var n in nums)
        {
          var od = odds.Trios.Value
            .Blocks
            .Where(b => b.NumberInGroup == n.Item1)
            .SelectMany(b => b.Columns)
            .Where(c => c.Number == n.Item2)
            .SelectMany(c => c.Odds)
            .FirstOrDefault(o => o.Data.HorseNumber1 == n.Item1 && o.Data.HorseNumber2 == n.Item2 && o.Data.HorseNumber3 == n.Item3);
          rows.Add(new TicketItemRow
          {
            Money = (int)(od.Data.Odds * 10),
            Number1 = n.Item1,
            Number2 = n.Item2,
            Number3 = n.Item3,
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
            Number3 = n.Item3,
          });
        }
      }

      this.SetRows(rows);
    }

    private TrioTicketItem(TicketData data, int money) : base(data)
    {
      this.SetRows(new[] { new TicketItemRow
      {
        Money = money,
        Number1 = data.Numbers1[0],
        Number2 = data.Numbers2[0],
        Number3 = data.Numbers3[0],
      }, });
    }

    protected override TicketItem GenerateNewItem(TicketData data, int money, int moneyMax)
    {
      data.Type = TicketType.Trio;
      return new TrioTicketItem(data, money);
    }

    public override IReadOnlyList<TicketItemRow> GetHitRows(short num1, short num2, short num3, short frame1, short frame2)
    {
      var rows = this.Rows.Where(r => r.Number1 == num1 && r.Number2 == num2 && r.Number3 == num3 ||
                                      r.Number1 == num1 && r.Number2 == num3 && r.Number3 == num2 ||
                                      r.Number1 == num2 && r.Number2 == num1 && r.Number3 == num3 ||
                                      r.Number1 == num2 && r.Number2 == num3 && r.Number3 == num1 ||
                                      r.Number1 == num3 && r.Number2 == num1 && r.Number3 == num2 ||
                                      r.Number1 == num3 && r.Number2 == num2 && r.Number3 == num1);
      return rows.ToArray();
    }
  }

  public class TrifectaTicketItem : TicketItem
  {
    public TrifectaTicketItem(TicketData data, OddsInfo odds) : base(data)
    {
      var nums = GetFormationNumbers(data.Numbers1, data.Numbers2, data.Numbers3, isOrder: true, canSameNumber: false, isMulti: data.IsMulti);
      var rows = new List<TicketItemRow>();

      if (odds.Trifectas.Value != null)
      {
        foreach (var n in nums)
        {
          var od = odds.Trifectas.Value
            .Blocks
            .Where(b => b.NumberInGroup == n.Item1)
            .SelectMany(b => b.Columns)
            .Where(c => c.Number == n.Item2)
            .SelectMany(c => c.Odds)
            .FirstOrDefault(o => o.Data.HorseNumber1 == n.Item1 && o.Data.HorseNumber2 == n.Item2 && o.Data.HorseNumber3 == n.Item3);
          rows.Add(new TicketItemRow
          {
            Money = (int)(od.Data.Odds * 10),
            Number1 = n.Item1,
            Number2 = n.Item2,
            Number3 = n.Item3,
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
            Number3 = n.Item3,
          });
        }
      }

      this.SetRows(rows);
    }

    private TrifectaTicketItem(TicketData data, int money) : base(data)
    {
      this.SetRows(new[] { new TicketItemRow
      {
        Money = money,
        Number1 = data.Numbers1[0],
        Number2 = data.Numbers2[0],
        Number3 = data.Numbers3[0],
      }, });
    }

    protected override TicketItem GenerateNewItem(TicketData data, int money, int moneyMax)
    {
      data.Type = TicketType.Trifecta;
      return new TrifectaTicketItem(data, money);
    }

    public override IReadOnlyList<TicketItemRow> GetHitRows(short num1, short num2, short num3, short frame1, short frame2)
    {
      var rows = this.Rows.Where(r => r.Number1 == num1 && r.Number2 == num2 && r.Number3 == num3);
      return rows.ToArray();
    }
  }
}
