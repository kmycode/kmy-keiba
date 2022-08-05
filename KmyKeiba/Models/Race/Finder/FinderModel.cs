using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.Finder
{
  public class FinderModel
  {
    private readonly RaceFinder _finder;

    public ReactiveProperty<IEnumerable<IFinderColumnDefinition>> Columns { get; } = new();

    public ReactiveCollection<FinderColumnDefinition<FinderRaceItem>> RaceColumns { get; } = new();

    public ReactiveCollection<FinderColumnDefinition<FinderRaceHorseItem>> RaceHorseColumns { get; } = new();

    public ReactiveCollection<FinderColumnDefinition<FinderRaceHorseGroupItem>> RaceHorseGroupColumns { get; } = new();

    public ReactiveCollection<FinderRaceHorseGroupItem> HorseGroups { get; } = new();

    public ReactiveProperty<FinderRaceHorseGroupItem?> CurrentGroup { get; } = new();

    public ReactiveProperty<string> Keys { get; } = new("place>0");

    public ReactiveProperty<bool> IsSearchRaces { get; } = new();

    public ReactiveProperty<bool> IsSearchRaceHorses { get; } = new(true);

    public ReactiveProperty<bool> IsSearchRaceHorseGroups { get; } = new();

    public ReactiveProperty<bool> IsLoading { get; } = new();

    public FinderModel(RaceData? race, RaceHorseData? horse)
    {
      this._finder = new RaceFinder(race, horse);

      // TODO いずれカスタマイズできるように
      foreach (var preset in DatabasePresetModel.GetFinderRaceHorseColumns())
      {
        this.RaceHorseColumns.Add(preset.Clone());
      }

      this.CurrentGroup.Subscribe(g =>
      {
        if (g != null && !g.Rows.Any() && g.Items.Any())
        {
          this.UpdateRows(g.Items.Select(i => i.Analyzer).ToFinderRows(this.RaceHorseColumns));
        }
      });
    }

    public void BeginLoad()
    {
      Task.Run(async () =>
      {
        this.IsLoading.Value = true;

        try
        {
          if (this.IsSearchRaces.Value)
          {
            this.Columns.Value = this.RaceColumns;
            var data = await this._finder.FindRacesAsync(this.Keys.Value, 3000, withoutFutureRaces: false);
            this.UpdateRows(data.ToFinderRows(this.RaceColumns));
          }
          else if (this.IsSearchRaceHorses.Value)
          {
            this.Columns.Value = this.RaceHorseColumns;
            var data = await this._finder.FindRaceHorsesAsync(this.Keys.Value, 3000, withoutFutureRaces: false);

            if (!this.IsSearchRaceHorseGroups.Value)
            {
              var group = new FinderRaceHorseGroupItem(data);
              this.UpdateGroups(new[] { group, });
            }
            else
            {
              // TODO 結果のグループ分け処理を実装
              var groups = data.GroupBy(d => d.Data.RiderName).Select(g => new FinderRaceHorseGroupItem(g)).ToArray();
              this.UpdateGroups(groups);
            }
          }
        }
        catch (Exception ex)
        {

        }
        finally
        {
          this.IsLoading.Value = false;
        }
      });
    }

    private void UpdateGroups(IEnumerable<FinderRaceHorseGroupItem> groups)
    {
      ThreadUtil.InvokeOnUiThread(() =>
      {
        this.HorseGroups.Clear();
        foreach (var group in groups)
        {
          this.HorseGroups.Add(group);
        }
        if (groups.Any())
        {
          this.CurrentGroup.Value = groups.First();
        }
      });
    }

    private void UpdateRows(IEnumerable<FinderRow> rows)
    {
      var group = this.CurrentGroup.Value;
      if (group == null)
      {
        return;
      }

      ThreadUtil.InvokeOnUiThread(() =>
      {
        group.Rows.Clear();
        foreach (var row in rows)
        {
          group.Rows.Add(row);
        }
      });
    }
  }

  public class FinderRow
  {
    public IReadOnlyList<FinderCell> Cells { get; }

    public RaceAnalyzer? Race { get; }

    public RaceHorseAnalyzer? RaceHorse { get; }

    public FinderRow(IReadOnlyList<FinderCell> cells, RaceAnalyzer? race, RaceHorseAnalyzer? horse)
    {
      this.Cells = cells;
      this.Race = race;
      this.RaceHorse = horse;
    }
  }
}
