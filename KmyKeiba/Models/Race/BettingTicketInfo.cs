using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Analysis.Generic;
using KmyKeiba.Models.Data;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

    public MultipleCheckableCollection<BettingHorseItem> Numbers1 { get; }

    public MultipleCheckableCollection<BettingHorseItem> Numbers2 { get; }

    public MultipleCheckableCollection<BettingHorseItem> Numbers3 { get; }

    public MultipleCheckableCollection<BettingFrameItem> FrameNumbers1 { get; }

    public MultipleCheckableCollection<BettingFrameItem> FrameNumbers2 { get; }

    public TicketItemCollection Tickets { get; } = new();

    public ReactiveProperty<string> Count { get; } = new("1");

    public ReactiveProperty<TicketType> Type { get; } = new(TicketType.Single);

    public ReactiveProperty<TicketFormType> FormType { get; } = new(TicketFormType.Formation);

    public ReactiveProperty<bool> IsMulti { get; } = new();

    public ReactiveProperty<bool> CanBuy { get; } = new();

    public ReactiveProperty<bool> IsSelectedItems { get; } = new();

    public ReactiveProperty<int> TotalMoney { get; } = new();

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
        }.AddTo(this._disposables));
      var frames = horses
        .GroupBy(h => h.Data.FrameNumber)
        .OrderBy(h => h.Key)
        .Select(h => new BettingFrameItem(h)
        {
          FrameNumber = h.Key,
        }.AddTo(this._disposables));
      // Linqの遅延評価を利用して、全く同じ内容で異なるインスタンスを持った配列を作成・格納する
      this.Numbers1 = new MultipleCheckableCollection<BettingHorseItem>(items.ToArray()).AddTo(this._disposables);
      this.Numbers2 = new MultipleCheckableCollection<BettingHorseItem>(items.ToArray()).AddTo(this._disposables);
      this.Numbers3 = new MultipleCheckableCollection<BettingHorseItem>(items.ToArray()).AddTo(this._disposables);
      this.FrameNumbers1 = new MultipleCheckableCollection<BettingFrameItem>(frames.ToArray()).AddTo(this._disposables);
      this.FrameNumbers2 = new MultipleCheckableCollection<BettingFrameItem>(frames.ToArray()).AddTo(this._disposables);

      // 購入可能か示す値を更新する
      this.Numbers1.ChangedItemObservable.Subscribe(_ => this.UpdateCanBuy()).AddTo(this._disposables);
      this.Numbers2.ChangedItemObservable.Subscribe(_ => this.UpdateCanBuy()).AddTo(this._disposables);
      this.Numbers3.ChangedItemObservable.Subscribe(_ => this.UpdateCanBuy()).AddTo(this._disposables);
      this.FrameNumbers1.ChangedItemObservable.Subscribe(_ => this.UpdateCanBuy()).AddTo(this._disposables);
      this.FrameNumbers2.ChangedItemObservable.Subscribe(_ => this.UpdateCanBuy()).AddTo(this._disposables);
      this.Type.Subscribe(_ => this.UpdateCanBuy()).AddTo(this._disposables);
      this.FormType.Subscribe(_ => this.UpdateCanBuy()).AddTo(this._disposables);
      this.Count.Subscribe(_ => this.UpdateCanBuy()).AddTo(this._disposables);

      this._horses = horses.Select(h => h.Data).OrderBy(h => h.Number).ToArray();

      foreach (var ticket in existTickets)
      {
        var item = TicketItem.FromData(ticket, this._horses, odds);
        if (item != null)
        {
          this.Tickets.Add(item);
        }
      }

      this.SortTickets();
      this.UpdateTotalMoney();

      Observable.FromEvent<EventHandler, EventArgs>(a => (s, e) => a(e), dele => this.Tickets.TicketCountChanged += dele, dele => this.Tickets.TicketCountChanged -= dele)
        .Subscribe(_ => this.UpdateTotalMoney())
        .AddTo(this._disposables);
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

    private void UpdateCanBuy()
    {
      if (short.TryParse(this.Count.Value, out short count))
      {
        this.CanBuy.Value = count > 0;
        if (!this.CanBuy.Value)
        {
          return;
        }
      }
      else
      {
        this.CanBuy.Value = false;
        return;
      }

      if (this.Type.Value == TicketType.FrameNumber)
      {
        var nums1 = this.FrameNumbers1.Where(f => f.IsChecked.Value).Select(f => f.FrameNumber).ToArray();
        var nums2 = this.FrameNumbers2.Where(f => f.IsChecked.Value).Select(f => f.FrameNumber).ToArray();
        this.CanBuy.Value = nums1.Any() && (this.FormType.Value == TicketFormType.Box || nums2.Any());

        if ((this.FormType.Value == TicketFormType.Formation && nums1.Length == 1 && nums2.Length == 1 && nums1[0] == nums2[0]) ||
          (this.FormType.Value == TicketFormType.Box && nums1.Length == 1))
        {
          var availableHorses = this._horses.Where(h => h.AbnormalResult != RaceAbnormality.Scratched && h.AbnormalResult != RaceAbnormality.ExcludedByStarters);
          this.CanBuy.Value &= availableHorses.Count(h => h.FrameNumber == nums1[0]) >= 2;
        }
      }
      else
      {
        var nums1 = this.Numbers1.Where(f => f.IsChecked.Value).Select(f => f.HorseNumber).ToArray();
        var nums2 = this.Numbers2.Where(f => f.IsChecked.Value).Select(f => f.HorseNumber).ToArray();
        var nums3 = this.Numbers3.Where(f => f.IsChecked.Value).Select(f => f.HorseNumber).ToArray();
        this.CanBuy.Value = nums1.Any();
        if (!this.CanBuy.Value)
        {
          return;
        }

        if (this.Type.Value == TicketType.QuinellaPlace || this.Type.Value == TicketType.Quinella || this.Type.Value == TicketType.Exacta)
        {
          this.CanBuy.Value &= this.FormType.Value == TicketFormType.Box || nums2.Any();

          // 同じ数字を選んでるか
          if (this.CanBuy.Value && this.FormType.Value == TicketFormType.Formation)
          {
            this.CanBuy.Value &= !(nums1.Length == 1 && nums2.Length == 1) || nums1[0] != nums2[0];
          }
          if (this.CanBuy.Value && this.FormType.Value == TicketFormType.Box)
          {
            this.CanBuy.Value &= nums1.Length >= 2;
          }
        }
        else if (this.Type.Value == TicketType.Trio || this.Type.Value == TicketType.Trifecta)
        {
          this.CanBuy.Value = this.FormType.Value == TicketFormType.Box || (nums2.Any() && (this.FormType.Value == TicketFormType.Nagashi || nums3.Any()));

          // 同じ数字を選んでるか
          if (this.CanBuy.Value && this.FormType.Value == TicketFormType.Formation)
          {
            this.CanBuy.Value &= !(nums1.Length == 1 && nums2.Length == 1) || nums1[0] != nums2[0];
            this.CanBuy.Value &= !(nums2.Length == 1 && nums3.Length == 1) || nums2[0] != nums3[0];
            this.CanBuy.Value &= !(nums1.Length == 1 && nums3.Length == 1) || nums1[0] != nums3[0];
          }
          if (this.CanBuy.Value && this.FormType.Value == TicketFormType.Box)
          {
            this.CanBuy.Value &= nums1.Length >= 3;
          }
          if (this.CanBuy.Value && this.FormType.Value == TicketFormType.Nagashi)
          {
            this.CanBuy.Value &= nums2.Length > 2 ||
              (nums2.Length == 2 && (
                  nums1.Length == 1 && !nums2.Contains(nums1[0]) ||
                  (nums1.Length == 2 && (!nums2.Contains(nums1[0]) || !nums2.Contains(nums1[1])))
                )
              );
          }
        }
      }
    }

    public void UpdateIsSelected()
    {
      this.IsSelectedItems.Value = this.Tickets.Any(t => t.IsAllRowsChecked.Value || t.Rows.Any(r => r.IsChecked.Value));
    }

    public void Dispose()
    {
      this._disposables.Dispose();
      this.Tickets.Dispose();
    }

    public void SetType(string str)
    {
      short.TryParse(str, out var num);
      this.Type.Value = (TicketType)num;

      if (this.FormType.Value == TicketFormType.Nagashi)
      {
        if (this.Type.Value != TicketType.Trio && this.Type.Value != TicketType.Trifecta)
        {
          this.FormType.Value = TicketFormType.Box;
        }
      }
      if (this.FormType.Value == TicketFormType.Box)
      {
        if (this.Type.Value == TicketType.Single || this.Type.Value == TicketType.Place)
        {
          this.FormType.Value = TicketFormType.Formation;
        }
      }
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
      await this.BuyAsync(db);
      await db.SaveChangesAsync();
    }

    private async Task BuyAsync(MyContext db)
    {
      short.TryParse(this.Count.Value, out var count);
      if (count <= 0)
      {
        return;
      }

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
      else if (this.Type.Value == TicketType.Trio)
      {
        tickets = await this.GenerateTrioTicketsAsync(db, count);
      }
      else if (this.Type.Value == TicketType.Trifecta)
      {
        tickets = await this.GenerateTrifectaTicketsAsync(db, count);
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
      this.UpdateTotalMoney();
    }

    private void UpdateTotalMoney()
    {
      if (this.Tickets.SelectMany(t => t.Rows).Any())
      {
        this.TotalMoney.Value = this.Tickets.SelectMany(t => t.Rows).Sum(r => r.DataCount) * 100;

        var sum = this.TotalMoney.Value;
        foreach (var row in this.Tickets.SelectMany(t => t.Rows))
        {
          var money = row.Money * row.DataCount;
          row.Comparation.Value = money < sum ? ValueComparation.Bad :
            money > sum ? ValueComparation.Good : ValueComparation.Standard;
        }
      }
      else
      {
        this.TotalMoney.Value = 0;
      }
    }

    public async Task BuyAsync(IEnumerable<TicketData> tickets)
    {
      // 力技になるが。。
      // モデルの情報を変更してから購入ボタンを押すのを繰り返す

      var nums1 = this.Numbers1.Where(n => n.IsChecked.Value).Select(n => (byte)n.HorseNumber).ToArray();
      var nums2 = this.Numbers2.Where(n => n.IsChecked.Value).Select(n => (byte)n.HorseNumber).ToArray();
      var nums3 = this.Numbers3.Where(n => n.IsChecked.Value).Select(n => (byte)n.HorseNumber).ToArray();
      var frames1 = this.FrameNumbers1.Where(n => n.IsChecked.Value).Select(n => (byte)n.FrameNumber).ToArray();
      var frames2 = this.FrameNumbers2.Where(n => n.IsChecked.Value).Select(n => (byte)n.FrameNumber).ToArray();
      var type = this.Type.Value;
      var formType = this.FormType.Value;
      var count = this.Count.Value;
      var isMulti = this.IsMulti.Value;

      void SetNumbers(IEnumerable<BettingHorseItem> horses, byte[]? nums)
      {
        nums ??= Array.Empty<byte>();
        foreach (var h in horses)
        {
          h.IsChecked.Value = nums.Contains((byte)h.HorseNumber);
        }
      }
      void SetFrames(IEnumerable<BettingFrameItem> frames, byte[]? nums)
      {
        nums ??= Array.Empty<byte>();
        foreach (var h in frames)
        {
          h.IsChecked.Value = nums.Contains((byte)h.FrameNumber);
        }
      }

      using var db = new MyContext();
      foreach (var ticket in tickets)
      {
        if (ticket.Type != TicketType.FrameNumber)
        {
          SetNumbers(this.Numbers1, ticket.Numbers1);
          SetNumbers(this.Numbers2, ticket.Numbers2);
          SetNumbers(this.Numbers3, ticket.Numbers3);
        }
        else
        {
          SetFrames(this.FrameNumbers1, ticket.Numbers1);
          SetFrames(this.FrameNumbers2, ticket.Numbers2);
        }
        this.Type.Value = ticket.Type;
        this.FormType.Value = ticket.FormType;
        this.Count.Value = ticket.Count.ToString();
        this.IsMulti.Value = ticket.IsMulti;

        await this.BuyAsync(db);
      }

      await db.SaveChangesAsync();

      // 情報を復元する
      SetNumbers(this.Numbers1, nums1);
      SetNumbers(this.Numbers2, nums2);
      SetNumbers(this.Numbers3, nums3);
      SetFrames(this.FrameNumbers1, frames1);
      SetFrames(this.FrameNumbers2, frames2);
      this.Type.Value = type;
      this.FormType.Value = formType;
      this.Count.Value = count;
      this.IsMulti.Value = isMulti;
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

    private async Task<IReadOnlyList<TicketItem>?> GenerateTrioTicketsAsync(MyContext db, int count)
      => await this.GenerateDribbleNumberTicketsAsync(db, count, TicketType.Trio, data => new TrioTicketItem(data, this.Odds));

    private async Task<IReadOnlyList<TicketItem>?> GenerateTrifectaTicketsAsync(MyContext db, int count)
      => await this.GenerateDribbleNumberTicketsAsync(db, count, TicketType.Trifecta, data => new TrifectaTicketItem(data, this.Odds));

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
      if (formType == TicketFormType.Formation && nums1.Length == 1 && nums2.Length == 1 && (!this.IsMulti.Value || !isOrder))
      {
        formType = TicketFormType.Single;
      }
      if (formType == TicketFormType.Box)
      {
        nums2 = nums1;
      }
      if (!nums2.Any())
      {
        return null;
      }

      // 馬券に実際に必要なデータを確定させるため、先にデータを作ってしまう
      // 指定したデータと実際に必要なデータが異なると、既存とは別のデータとして扱われることがある（Issue #102）
      var data = new TicketData
      {
        RaceKey = this._raceKey,
        Type = type,
        FormType = formType,
        Numbers1 = nums1,
        Numbers2 = nums2,
        Count = (short)count,
        IsMulti = this.IsMulti.Value,
      };
      var item = itemGenerator(data);
      nums1 = item.Data.Numbers1;
      nums2 = item.Data.Numbers2;
      formType = item.Data.FormType;

      foreach (var ticket in this.Tickets.Where(t => t.Type == type))
      {
        if (ticket.Data.FormType == formType && (!isOrder || ticket.Data.IsMulti == data.IsMulti))
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
        if (item.Rows.Any())
        {
          list.Add(item);
        }
      }

      if (isChanged) await db.SaveChangesAsync();

      if (list.Any()) return list;
      return null;
    }

    private async Task<IReadOnlyList<TicketItem>?> GenerateDribbleNumberTicketsAsync(MyContext db, int count, TicketType type, Func<TicketData, TicketItem> itemGenerator)
    {
      var list = new List<TicketItem>();
      var isChanged = false;
      var hit = false;
      var isOrder = type == TicketType.Trifecta;

      var nums1 = this.Numbers1.Where(n => n.IsChecked.Value).Select(n => (byte)n.HorseNumber).ToArray();
      var nums2 = this.Numbers2.Where(n => n.IsChecked.Value).Select(n => (byte)n.HorseNumber).ToArray();
      var nums3 = this.Numbers3.Where(n => n.IsChecked.Value).Select(n => (byte)n.HorseNumber).ToArray();

      if (!nums1.Any())
      {
        return null;
      }

      var formType = this.FormType.Value;
      if (formType == TicketFormType.Formation && nums1.Length == 1 && nums2.Length == 1 && nums3.Length == 1 && (!this.IsMulti.Value || !isOrder))
      {
        formType = TicketFormType.Single;
      }
      if (formType == TicketFormType.Box)
      {
        nums2 = nums1;
        nums3 = nums1;
      }
      else if (formType == TicketFormType.Nagashi)
      {
        nums3 = nums2;
      }
      if (!nums2.Any() || !nums3.Any())
      {
        return null;
      }

      // 馬券に実際に必要なデータを確定させるため、先にデータを作ってしまう
      // 指定したデータと実際に必要なデータが異なると、既存とは別のデータとして扱われることがある（Issue #102）
      var data = new TicketData
      {
        RaceKey = this._raceKey,
        Type = type,
        FormType = formType,
        Numbers1 = nums1,
        Numbers2 = nums2,
        Numbers3 = nums3,
        Count = (short)count,
        IsMulti = this.IsMulti.Value,
      };
      var item = itemGenerator(data);
      nums1 = item.Data.Numbers1;
      nums2 = item.Data.Numbers2;
      nums3 = item.Data.Numbers3;
      formType = item.Data.FormType;

      foreach (var ticket in this.Tickets.Where(t => t.Type == type))
      {
        if (ticket.Data.FormType == formType && ticket.Data.IsMulti == data.IsMulti)
        {
          if ((ticket.Data.Numbers1.SequenceEqual(nums1) && ticket.Data.Numbers2.SequenceEqual(nums2) && ticket.Data.Numbers3.SequenceEqual(nums3)) ||
            (!isOrder && (
              (ticket.Data.Numbers1.SequenceEqual(nums1) && ticket.Data.Numbers2.SequenceEqual(nums3) && ticket.Data.Numbers3.SequenceEqual(nums2)) ||
              (ticket.Data.Numbers1.SequenceEqual(nums2) && ticket.Data.Numbers2.SequenceEqual(nums1) && ticket.Data.Numbers3.SequenceEqual(nums3)) ||
              (ticket.Data.Numbers1.SequenceEqual(nums2) && ticket.Data.Numbers2.SequenceEqual(nums3) && ticket.Data.Numbers3.SequenceEqual(nums2)) ||
              (ticket.Data.Numbers1.SequenceEqual(nums3) && ticket.Data.Numbers2.SequenceEqual(nums1) && ticket.Data.Numbers3.SequenceEqual(nums2)) ||
              (ticket.Data.Numbers1.SequenceEqual(nums3) && ticket.Data.Numbers2.SequenceEqual(nums2) && ticket.Data.Numbers3.SequenceEqual(nums1))
            )))
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

    public async Task ClearTicketsAsync()
    {
      if (!this.Tickets.Any())
      {
        return;
      }

      using var db = new MyContext();
      db.Tickets!.RemoveRange(this.Tickets.Select(t => t.Data));
      await db.SaveChangesAsync();

      this.Tickets.Clear();
    }

    public async Task UpdateTicketCountAsync()
    {
      var canParse = short.TryParse(this.Count.Value, out var count);
      if (!canParse)
      {
        return;
      }
      if (count <= 0)
      {
        // await this.RemoveTicketAsync();
        return;
      }

      var targetTickets = this.Tickets
        .Where(t => t.IsAllRowsChecked.Value || t.Rows.Any(r => r.IsChecked.Value))
        .ToArray();
      if (!targetTickets.Any())
      {
        return;
      }

      using var db = new MyContext();

      foreach (var ticket in targetTickets)
      {
        db.Tickets!.Attach(ticket.Data);
        ticket.Data.Count = count;
        ticket.Count.Value = count;
      }

      await db.SaveChangesAsync();
    }
  }

  public class TicketItemCollection : ObservableItemCollection<TicketItem>, IDisposable
  {
    private readonly CompositeDisposable _disposables = new();
    private readonly Dictionary<TicketItem, List<IDisposable>> _disposableItems = new();

    public TicketItemCollection()
    {
      this.NewItemObservable.Subscribe(i =>
      {
        this._disposableItems.TryGetValue(i, out var list);
        if (list == null)
        {
          list = new();
          this._disposableItems.Add(i, list);
        }

        var d = i.Count.Subscribe(c =>
        {
          this.TicketCountChanged?.Invoke(this, EventArgs.Empty);
        });
        list.Add(d);
      }).AddTo(this._disposables);

      this.OldItemObservable.Subscribe(i =>
      {
        this._disposableItems.TryGetValue(i, out var list);
        if (list != null)
        {
          foreach (var disposable in list)
          {
            disposable.Dispose();
          }
          this._disposableItems.Remove(i);
        }
      });
    }

    public new void Dispose()
    {
      base.Dispose();
      this._disposables.Dispose();
      foreach (var disposable in this._disposableItems.SelectMany(i => i.Value))
      {
        disposable.Dispose();
      }
    }

    public event EventHandler? TicketCountChanged;
  }

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
        if (a == default || (c != default && b == default))
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
      var rows = this.Rows.Where(r => (r.Number1 == num1 && r.Number2 == num2) ||
                                      (r.Number1 == num2 && r.Number2 == num1));
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
      var rows = this.Rows.Where(r => (r.Number1 == num1 && r.Number2 == num2 && r.Number3 == num3) ||
                                      (r.Number1 == num1 && r.Number2 == num3 && r.Number3 == num2) ||
                                      (r.Number1 == num2 && r.Number2 == num1 && r.Number3 == num3) ||
                                      (r.Number1 == num2 && r.Number2 == num3 && r.Number3 == num1) ||
                                      (r.Number1 == num3 && r.Number2 == num1 && r.Number3 == num2) ||
                                      (r.Number1 == num3 && r.Number2 == num2 && r.Number3 == num1));
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
      var rows = this.Rows.Where(r => (r.Number1 == num1 && r.Number2 == num2 && r.Number3 == num3));
      return rows.ToArray();
    }
  }

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

  public class BettingHorseItem : IDisposable, IMultipleCheckableItem
  {
    public short HorseNumber { get; init; }

    public string Name { get; init; } = string.Empty;

    string IMultipleCheckableItem.GroupName => string.Empty;

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

  public class BettingFrameItem : IDisposable, IMultipleCheckableItem
  {
    private readonly CompositeDisposable _disposables = new();

    public short FrameNumber { get; init; }

    string IMultipleCheckableItem.GroupName => string.Empty;

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
          if (!this.IsEnabled.Value && this.IsChecked.Value)
          {
            this.IsChecked.Value = false;
          }
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
      }

      return ticket.ToString() ?? string.Empty;
    }
  }
}
