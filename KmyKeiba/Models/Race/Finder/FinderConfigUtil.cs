using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.ObjectExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.Finder
{
  internal static class FinderConfigUtil
  {
    private static bool _isInitialized;

    public static ReactiveCollection<FinderConfigItem> Configs { get; } = new();

    public static async Task InitializeAsync(MyContext db)
    {
      if (!_isInitialized)
      {
        var configs = await db.FinderConfigs!.ToArrayAsync();
        foreach (var config in configs.OrderBy(c => c.Order))
        {
          Configs.Add(new(config));
        }
        _isInitialized = true;
      }
    }

    public static async Task<FinderConfigItem> AddAsync(MyContext db, FinderConfigData config)
    {
      config.Order = Configs.Count == 0 ? 1 : Configs.Max(c => c.Data.Order) + 1;

      await db.FinderConfigs!.AddAsync(config);
      await db.SaveChangesAsync();

      var item = new FinderConfigItem(config);
      Configs.Add(item);

      return item;
    }

    public static async Task RemoveAsync(MyContext db, FinderConfigItem configItem)
    {
      db.FinderConfigs!.Remove(configItem.Data);
      await db.SaveChangesAsync();

      Configs.Remove(configItem);
      configItem.Dispose();
    }

    public static async Task UpAsync(MyContext db, FinderConfigItem configItem)
    {
      var index = Configs.IndexOf(configItem);
      if (index <= 0) return;

      var targetItem = Configs[index - 1];

      Configs.Move(index, index - 1);

      db.FinderConfigs!.Attach(configItem.Data);
      db.FinderConfigs!.Attach(targetItem.Data);
      (configItem.Data.Order, targetItem.Data.Order) = (targetItem.Data.Order, configItem.Data.Order);

      await db.SaveChangesAsync();
    }

    public static async Task DownAsync(MyContext db, FinderConfigItem configItem)
    {
      var index = Configs.IndexOf(configItem);
      if (index >= Configs.Count - 1) return;

      var targetItem = Configs[index + 1];

      Configs.Move(index, index + 1);

      db.FinderConfigs!.Attach(configItem.Data);
      db.FinderConfigs!.Attach(targetItem.Data);
      (configItem.Data.Order, targetItem.Data.Order) = (targetItem.Data.Order, configItem.Data.Order);

      await db.SaveChangesAsync();
    }
  }

  public class FinderConfigItem : IDisposable
  {
    private readonly CompositeDisposable _disposables = new();

    public FinderConfigData Data { get; }

    public FinderModel FinderModelForConfig { get; }

    public ReactiveProperty<string> Name { get; } = new();

    public FinderConfigItem(FinderConfigData data)
    {
      this.Data = data;
      this.Name.Value = data.Name;

      this.FinderModelForConfig = new FinderModel(new RaceData(), RaceHorseAnalyzer.Empty, Enumerable.Empty<RaceHorseAnalyzer>());
      this.FinderModelForConfig.Input.Deserialize(data.Config, false);
      this.FinderModelForConfig.AddTo(this._disposables);

      this.FinderModelForConfig.Input.Query.Skip(1).Subscribe(async _ =>
      {
        // TODO try catch
        using var db = new MyContext();
        db.FinderConfigs!.Attach(this.Data);
        this.Data.Config = this.FinderModelForConfig.Input.Serialize(false);
        await db.SaveChangesAsync();
      }).AddTo(this._disposables);

      this.Name.Skip(1).Subscribe(async name =>
      {
        // TODO: try catch
        using var db = new MyContext();
        db.FinderConfigs!.Attach(this.Data);
        this.Data.Name = name;
        await db.SaveChangesAsync();
      }).AddTo(this._disposables);
    }

    public void Dispose() => this._disposables.Dispose();
  }
}
