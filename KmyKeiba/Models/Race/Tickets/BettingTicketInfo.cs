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

namespace KmyKeiba.Models.Race.Tickets
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
        .Select(h => new BettingHorseItem(h).AddTo(this._disposables));
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

        if (this.FormType.Value == TicketFormType.Formation && nums1.Length == 1 && nums2.Length == 1 && nums1[0] == nums2[0] ||
          this.FormType.Value == TicketFormType.Box && nums1.Length == 1)
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
          this.CanBuy.Value = this.FormType.Value == TicketFormType.Box || nums2.Any() && (this.FormType.Value == TicketFormType.Nagashi || nums3.Any());

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
              nums2.Length == 2 && (
                  nums1.Length == 1 && !nums2.Contains(nums1[0]) ||
                  nums1.Length == 2 && (!nums2.Contains(nums1[0]) || !nums2.Contains(nums1[1]))
                )
              ;
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
          if (ticket.Data.Numbers1.SequenceEqual(nums1) && ticket.Data.Numbers2.SequenceEqual(nums2) ||
            !isOrder && ticket.Data.Numbers1.SequenceEqual(nums2) && ticket.Data.Numbers2.SequenceEqual(nums1))
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
          if (ticket.Data.Numbers1.SequenceEqual(nums1) && ticket.Data.Numbers2.SequenceEqual(nums2) && ticket.Data.Numbers3.SequenceEqual(nums3) ||
            !isOrder && (
              ticket.Data.Numbers1.SequenceEqual(nums1) && ticket.Data.Numbers2.SequenceEqual(nums3) && ticket.Data.Numbers3.SequenceEqual(nums2) ||
              ticket.Data.Numbers1.SequenceEqual(nums2) && ticket.Data.Numbers2.SequenceEqual(nums1) && ticket.Data.Numbers3.SequenceEqual(nums3) ||
              ticket.Data.Numbers1.SequenceEqual(nums2) && ticket.Data.Numbers2.SequenceEqual(nums3) && ticket.Data.Numbers3.SequenceEqual(nums2) ||
              ticket.Data.Numbers1.SequenceEqual(nums3) && ticket.Data.Numbers2.SequenceEqual(nums1) && ticket.Data.Numbers3.SequenceEqual(nums2) ||
              ticket.Data.Numbers1.SequenceEqual(nums3) && ticket.Data.Numbers2.SequenceEqual(nums2) && ticket.Data.Numbers3.SequenceEqual(nums1)
            ))
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
      await db.SaveChangesAsync();

      if (this.Tickets.Any())
      {
        this.SortTickets();
      }
      else
      {
        // SortTickets内でAddが呼び出され、そこからUpdateTotalMoneyが呼び出される
        // しかしSortTicketsでソート対象がないとforeachが回らず呼び出されなくなる
        this.UpdateTotalMoney();
      }

      this.UpdateIsSelected();
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

      foreach (var ticket in targetTickets.Where(t => !t.IsAllRowsChecked.Value))
      {
        var broken = ticket.Break();

      }

      using var db = new MyContext();
      var isBroken = false;

      foreach (var ticket in targetTickets)
      {
        if (ticket.Rows.Count == 1 || ticket.IsAllRowsChecked.Value)
        {
          db.Tickets!.Attach(ticket.Data);
          ticket.Data.Count = count;
          ticket.Count.Value = count;
        }
        else
        {
          var broken = ticket.Break();
          foreach (var t in broken.Where(b => b.IsChecked.Value))
          {
            t.Data.Count = count;
            t.Count.Value = count;
          }

          this.Tickets.Remove(ticket);
          db.Tickets!.Remove(ticket.Data);

          foreach (var t in broken)
          {
            this.Tickets.Add(t);
          }
          await db.Tickets!.AddRangeAsync(broken.Select(b => b.Data));

          isBroken = true;
        }
      }

      await db.SaveChangesAsync();
      if (isBroken)
      {
        this.SortTickets();
      }
    }
  }
}
