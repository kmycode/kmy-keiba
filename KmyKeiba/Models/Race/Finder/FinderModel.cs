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

    public ReactiveCollection<FinderRow> Rows { get; } = new();

    public ReactiveProperty<IEnumerable<IFinderColumnDefinition>> Columns { get; } = new();

    public ReactiveCollection<FinderColumnDefinition<FinderRaceItem>> RaceColumns { get; } = new();

    public ReactiveCollection<FinderColumnDefinition<FinderRaceHorseItem>> RaceHorseColumns { get; } = new();

    public ReactiveCollection<FinderColumnDefinition<FinderRaceHorseGroupItem>> RaceHorseGroupColumns { get; } = new();

    public ReactiveProperty<string> Keys { get; } = new(string.Empty);

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
            var data = await this._finder.FindRacesAsync(null, this.Keys.Value, 1000, withoutFutureRaces: false);
            this.UpdateRows(data.ToFinderRows(this.RaceColumns));
          }
          else if (this.IsSearchRaceHorses.Value)
          {
            this.Columns.Value = this.RaceHorseColumns;
            var data = await this._finder.FindRaceHorsesAsync(null, this.Keys.Value, 1000, withoutFutureRaces: false);
            this.UpdateRows(data.ToFinderRows(this.RaceHorseColumns));
          }
          else if (this.IsSearchRaceHorseGroups.Value)
          {
            this.Columns.Value = this.RaceHorseGroupColumns;
            var data = await this._finder.FindRaceHorsesAsync(null, this.Keys.Value, 1000, withoutFutureRaces: false);

            // TODO: グループキーは仮
            var grouped = data.GroupBy(i => i.Race.Key).Select(g => new FinderRaceHorseGroupItem(g));

            // TODO
            //this.UpdateRows(grouped.ToFinderRows(this.RaceHorseGroupColumns));
          }
        }
        finally
        {
          this.IsLoading.Value = false;
        }
      });
    }

    private void UpdateRows(IEnumerable<FinderRow> rows)
    {
      ThreadUtil.InvokeOnUiThread(() =>
      {
        this.Rows.Clear();
        foreach (var row in rows)
        {
          this.Rows.Add(row);
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
