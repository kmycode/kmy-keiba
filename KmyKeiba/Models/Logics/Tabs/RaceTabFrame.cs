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

namespace KmyKeiba.Models.Logics.Tabs
{
  class RaceTabFrame : TabFrame, IDisposable
  {
    private readonly CompositeDisposable disposables = new();

    public ReactiveProperty<RaceDataObject> Race { get; } = new();

    public ReactiveProperty<bool> IsDataMode { get; } = new();

    public ReactiveProperty<bool> IsDataUpdating { get; } = new();

    public List<IHorseAnalyticsGroup> AnalyticsGroups { get; } = new()
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
    }

    private async Task UpdateAnalyticsAsync()
    {
      if (this.Race.Value == null)
      {
        return;
      }

      var dbConfig = DatabaseConfigManager.GetCurrentConfigFile();

      this.IsDataUpdating.Value = true;
      foreach (var horse in this.Race.Value.Horses)
      {
        await horse.AnalyticsAsync(dbConfig.GetConnectionString(), dbConfig.Database, this.AnalyticsGroups, this.AnalyticsFilters);
      }
      this.IsDataUpdating.Value = false;
    }

    public void Dispose()
    {
      this.disposables.Dispose();
    }
  }
}
