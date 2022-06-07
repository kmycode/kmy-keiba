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
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.Tickets
{
  public class BettingHorseItem : IDisposable, IMultipleCheckableItem
  {
    public short HorseNumber { get; }

    public short FrameNumber { get; }

    public string Name { get; }

    public ReactiveProperty<RaceHorseMark> Mark { get; }

    string IMultipleCheckableItem.GroupName => string.Empty;

    public ReactiveProperty<bool> IsChecked { get; } = new();

    public ReactiveProperty<bool> IsEnabled { get; } = new();

    public BettingHorseItem(RaceHorseAnalyzer horse)
    {
      this.Name = horse.Data.Name;
      this.HorseNumber = horse.Data.Number;
      this.FrameNumber = horse.Data.FrameNumber;
      this.Mark = horse.Mark;

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
}
