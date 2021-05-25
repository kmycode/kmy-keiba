using KmyKeiba.Data.DataObjects;
using KmyKeiba.Models.Analytics;
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
using System.Windows.Threading;

namespace KmyKeiba.Models.Logics.Tabs
{
  class RaceTabFrame : TabFrame, IDisposable
  {
    private readonly CompositeDisposable disposables = new();
    private readonly DispatcherTimer timer = new();
    private bool isAnalytics;

    public ReactiveProperty<RaceDataObject> Race { get; } = new();

    public ReactiveProperty<bool> IsDataMode { get; } = new();

    public ReactiveProperty<bool> IsDataUpdating { get; } = new();

    private Func<IEnumerable<IHorseAnalyticsGroup>> AnalyticsGroups { get; } = () => new IHorseAnalyticsGroup[]
    {
      new SelfHorseAnalyticsGroup(),
      new RiderHorseAnalyticsGroup(),
      new CourseHorseAnalyticsGroup(),
    };

    public List<IHorseAnalyticsFilter> AnalyticsFilters { get; } = new()
    {
      new SameWeatherHorseAnalyticsFilter(),
      new NearCourseDistanceHorseAnalyticsFilter(),
      new SameRunningStyleHorseAnalyticsFilter(),
      new SameCourseHorseAnalyticsFilter(),
      new SameCourseConditionHorseAnalyticsFilter(),
    };

    public RaceTabFrame()
    {
      foreach (var filter in this.AnalyticsFilters)
      {
        filter.IsEnabled.Subscribe(async (b) =>
        {
          await this.UpdateAnalyticsAsync();
        }).AddTo(this.disposables);
      }

      this.IsDataMode.Where((d) => d).Subscribe(async (d) =>
      {
        if (!this.Race.Value.Horses.Any((h) => h.AnalyticsResults.Any()))
        {
          await this.UpdateAnalyticsAsync();
        }
      }).AddTo(this.disposables);

      this.Race.Subscribe(async (r) =>
      {
        await this.UpdateAnalyticsAsync();
      }).AddTo(this.disposables);

      this.timer.Interval = TimeSpan.FromSeconds(30);
      this.timer.Tick += (sender, e) =>
      {
        if (this.isAnalytics)
        {
          this.UpdateAnalyticsAsync();
        }
      };
      this.disposables.Add(Disposable.Create(() => this.timer.Stop()));
      this.timer.Start();
    }

    private async Task UpdateAnalyticsAsync()
    {
      if (this.Race.Value == null || this.IsDataUpdating.Value)
      {
        return;
      }

      this.IsDataUpdating.Value = true;
      this.isAnalytics = true;

      try
      {
        foreach (var horse in this.Race.Value.Horses)
        {
          horse.Analytics(CacheDataManager.Cache, this.AnalyticsGroups, this.AnalyticsFilters);
        }
      }
      catch
      {
      }

      this.IsDataUpdating.Value = false;
    }

    public void Dispose()
    {
      this.disposables.Dispose();
    }
  }
}
