using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.RList
{
  public class RaceList : IDisposable
  {
    private readonly CompositeDisposable _disposables = new();

    public ReactiveProperty<DateTime> Date { get; } = new(DateTime.Now.Date);

    public ReactiveCollection<RaceCourseItem> Courses { get; } = new();

    public ReactiveProperty<RaceCourseItem?> CurrentCourse { get; } = new();

    public ReactiveProperty<string?> SelectedRaceKey { get; } = new();

    public RaceList()
    {
      this.Date.Skip(1).Subscribe(async _ =>
      {
        this.Courses.Clear();
        await this.UpdateListAsync();
      }).AddTo(this._disposables);
    }

    public async Task UpdateListAsync()
    {
      IEnumerable<RaceData> races;
      IEnumerable<RefundData> payoffs;
      IEnumerable<TicketData> tickets;
      IEnumerable<RaceHorseData> horses;

      using (var db = new MyContext())
      {
        var date = this.Date.Value;
        races = await db.Races!.Where(r => r.StartTime.Date == date).ToArrayAsync();

        var keys = races.Select(r => r.Key).ToArray();
        payoffs = await db.Refunds!.Where(r => keys.Contains(r.RaceKey)).ToArrayAsync();
        tickets = await db.Tickets!.Where(r => keys.Contains(r.RaceKey)).ToArrayAsync();
        
        var horseData = await db.RaceHorses!
          .Where(r => keys.Contains(r.RaceKey))
          .Select(r => new { r.Number, r.FrameNumber, r.RaceKey })
          .ToArrayAsync();
        horses = horseData.Select(h => new RaceHorseData { RaceKey = h.RaceKey, Number = h.Number, FrameNumber = h.FrameNumber, }).ToArray();
      }

      foreach (var group in races.OrderBy(r => r.StartTime).GroupBy(r => r.Course).OrderBy(g => g.Key))
      {
        var course = this.Courses.FirstOrDefault(c => c.Course == group.Key);
        if (course == null)
        {
          course = new RaceCourseItem(group.Key);
          this.Courses.AddOnScheduler(course);
        }

        course.Races.ClearOnScheduler();
        var items = group.ToArray();
        var newItems = new List<RaceListItem>();
        for (var i = 0; i < items.Length; i++)
        {
          var item = new RaceListItem(items[i]);
          item.Selected += Item_Selected;

          var nextItem = i < items.Length - 1 ? items[i + 1] : null;
          var prevItem = i > 0 ? items[i - 1] : null;
          item.NextRaceStartTime.Value = nextItem?.StartTime ?? DateTime.MinValue;
          item.PrevRaceStartTime.Value = prevItem?.StartTime ?? DateTime.MinValue;

          item.ViewTop.Value = ((prevItem?.StartTime.TimeOfDay.TotalMinutes ?? (items[i].StartTime.TimeOfDay.TotalMinutes - 40)) - Definitions.RaceTimelineStartHour * 60)
            * Definitions.RaceTimelineHeightPerMinutes;

          var refund = payoffs.FirstOrDefault(r => r.RaceKey == items[i].Key);
          if (refund != null)
          {
            var myHorses = horses.Where(h => h.RaceKey == items[i].Key).ToArray();
            var myTickets = tickets.Where(t => t.RaceKey == items[i].Key);
            this.UpdatePayoff(item, myHorses, myTickets, refund);
          }

          item.UpdateStatus();
          newItems.Add(item);
        }

        ThreadUtil.InvokeOnUiThread(() =>
        {
          course.Races.AddRangeOnScheduler(newItems);
        });
      }
    }

    public void UpdatePayoff(string raceKey, int income)
    {
      var item = this.Courses.SelectMany(c => c.Races).FirstOrDefault(r => r.Key == raceKey);
      if (item != null)
      {
        item.SetIncome(income);
      }
    }

    public async Task UpdatePayoffAsync(string raceKey)
    {
      var item = this.Courses.SelectMany(c => c.Races).FirstOrDefault(r => r.Key == raceKey);
      if (item == null)
      {
        return;
      }

      using var db = new MyContext();

      var horses = await db.RaceHorses!.Where(h => h.RaceKey == raceKey).ToArrayAsync();
      var tickets = await db.Tickets!.Where(t => t.RaceKey == raceKey).ToArrayAsync();
      var refund = await db.Refunds!.FirstOrDefaultAsync(r => r.RaceKey == raceKey);

      if (refund != null)
      {
        this.UpdatePayoff(item, horses, tickets, refund);
      }
    }

    private void UpdatePayoff(RaceListItem item, IReadOnlyList<RaceHorseData> horses, IEnumerable<TicketData> tickets, RefundData refund)
    {
      this.UpdatePayoff(item, tickets.Select(t => TicketItem.FromData(t, horses, null)).Where(t => t != null).Select(t => t!), refund);
    }

    private void UpdatePayoff(RaceListItem item, IEnumerable<TicketItem> tickets, RefundData refund)
    {
      var payoff = new PayoffInfo(refund);
      payoff.UpdateTicketsData(tickets.Where(t => t != null).OfType<TicketItem>().ToArray());

      var money = payoff.Income.Value;
      item.SetIncome(money);
      item.UpdateStatus();
    }

    private void Item_Selected(object? sender, EventArgs e)
    {
      foreach (var selected in this.Courses.SelectMany(c => c.Races).Where(r => r.Status.Value == RaceListItemStatus.Selected))
      {
        selected.UpdateStatus();
      }

      if (sender is RaceListItem item)
      {
        this.SelectedRaceKey.Value = item.Key;
        item.Status.Value = RaceListItemStatus.Selected;
      }
    }

    public void MoveToNextDay()
    {
      this.Date.Value = this.Date.Value.AddDays(1);
    }

    public void MoveToPrevDay()
    {
      this.Date.Value = this.Date.Value.AddDays(-1);
    }

    public void Dispose()
    {
      this._disposables.Dispose();
      foreach (var item in this.Courses.SelectMany(c => c.Races))
      {
        item.Selected -= this.Item_Selected;
      }
    }
  }

  public class RaceCourseItem
  {
    public RaceCourse Course { get; }

    public ReactiveCollection<RaceListItem> Races { get; } = new();

    public RaceCourseItem(RaceCourse course)
    {
      this.Course = course;
    }
  }

  public class RaceListItem
  {
    private readonly RaceData _race;
    private readonly RaceSubjectInfo _subject;

    public ReactiveProperty<RaceListItemStatus> Status { get; } = new();

    public RaceSubject Subject => this._subject.Subject;

    public string Key => this._race.Key;

    public string Name => this._subject.DisplayName;

    public DateTime StartTime => this._race.StartTime;

    public ReactiveProperty<DateTime> NextRaceStartTime { get; } = new();

    public ReactiveProperty<DateTime> PrevRaceStartTime { get; } = new();

    public ReactiveProperty<int> Money { get; } = new();

    public ReactiveProperty<ValueComparation> MoneyComparation { get; } = new();

    public ReactiveProperty<double> ViewTop { get; } = new();

    public RaceListItem(RaceData race)
    {
      this._race = race;
      this._subject = new RaceSubjectInfo(race);
    }

    public void OnSelected()
    {
      this.Selected?.Invoke(this, EventArgs.Empty);
    }

    public void UpdateStatus()
    {
      if (this._race.DataStatus == RaceDataStatus.Canceled)
      {
        this.Status.Value = RaceListItemStatus.Canceled;
      }
      else
      {
        this.Status.Value = this.StartTime > DateTime.Now ? RaceListItemStatus.NotStart : RaceListItemStatus.Finished;
      }
    }

    public void SetIncome(int money)
    {
      this.Money.Value = money;

      if (this.StartTime < DateTime.Now)
      {
        this.MoneyComparation.Value = money < 0 ? ValueComparation.Bad : money > 0 ? ValueComparation.Good : ValueComparation.Standard;
      }
      else
      {
        this.MoneyComparation.Value = ValueComparation.Standard;
      }
    }

    public event EventHandler? Selected;
  }

  public enum RaceListItemStatus
  {
    Unknown,
    Selected,
    NotStart,
    Finished,
    Canceled,
  }
}
