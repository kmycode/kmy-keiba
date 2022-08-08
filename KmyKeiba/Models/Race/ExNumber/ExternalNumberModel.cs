using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Data;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.ExNumber
{
  public class ExternalNumberModel
  {
    private readonly string _raceKey;

    public ReactiveCollection<ExternalNumberItemSet> ItemSets { get; } = new();

    public ExternalNumberModel(string raceKey)
    {
      this._raceKey = raceKey;
    }

    public async Task LoadAsync(MyContext db)
    {
      await ExternalNumberUtil.InitializeAsync(db);

      var values = await ExternalNumberUtil.GetValuesAsync(db, this._raceKey);
      var items = values
        .Select(v => new { Config = ExternalNumberUtil.GetConfig(v.ConfigId), Data = v, })
        .Where(v => v.Config != null)
        .GroupBy(v => v.Config!.Id)
        .Select(g => new ExternalNumberItemSet(g.First().Config!, g.Select(d => d.Data)))
        .ToArray();
      ThreadUtil.InvokeOnUiThread(() =>
      {
        this.ItemSets.Clear();
        foreach (var value in items)
        {
          this.ItemSets.Add(value);
        }
      });
    }
  }

  public class ExternalNumberItemSet
  {
    public ExternalNumberConfig Config { get; }

    public ReactiveCollection<ExternalNumberData> Items { get; } = new();

    public ExternalNumberItemSet(ExternalNumberConfig config, IEnumerable<ExternalNumberData> items)
    {
      this.Config = config;
      foreach (var item in items)
      {
        this.Items.Add(item);
      }
    }
  }
}
