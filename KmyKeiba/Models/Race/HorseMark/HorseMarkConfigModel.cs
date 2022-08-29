using KmyKeiba.Data.Db;
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

namespace KmyKeiba.Models.Race.HorseMark
{
  public class HorseMarkConfigModel : IDisposable
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public static HorseMarkConfigModel Instance => _instance ??= new();
    private static HorseMarkConfigModel? _instance;
    private bool _isInitialized;

    private readonly CompositeDisposable _disposables = new();

    public ReactiveCollection<HorseMarkConfig> Configs { get; } = new();

    private HorseMarkConfigModel()
    {
    }

    public void Initialize()
    {
      if (!this._isInitialized)
      {
        foreach (var config in HorseMarkUtil.Configs)
        {
          this.Configs.Add(new HorseMarkConfig(config));
        }

        this._isInitialized = true;
      }
    }

    public async Task AddConfigAsync()
    {
      try
      {
        using var db = new MyContext();

        var data = new HorseMarkConfigData();
        await db.HorseMarkConfigs!.AddAsync(data);
        await db.SaveChangesAsync();

        data.Order = data.Id;
        await db.SaveChangesAsync();

        var config = new HorseMarkConfig(data);
        this.Configs.Add(config);
      }
      catch (Exception ex)
      {
        logger.Error("印設定の追加でエラー", ex);
      }
    }

    public async Task RemoveConfigAsync()
    {
      var configs = this.Configs.Where(c => c.IsChecked.Value).ToArray();
      if (!configs.Any())
      {
        return;
      }

      try
      {
        using var db = new MyContext();

        db.HorseMarkConfigs!.RemoveRange(configs.Select(c => c.Data));
        await db.SaveChangesAsync();

        foreach (var config in configs)
        {
          this.Configs.Remove(config);
          config.Dispose();
        }
      }
      catch (Exception ex)
      {
        logger.Error("印設定の削除でエラー", ex);
      }
    }

    public async Task UpConfigAsync(HorseMarkConfig config)
    {
      try
      {
        var prev = this.Configs.TakeWhile(c => c != config).LastOrDefault();
        if (prev == null)
        {
          return;
        }

        using var db = new MyContext();
        db.HorseMarkConfigs!.Attach(prev.Data);
        db.HorseMarkConfigs!.Attach(config.Data);

        var tmp = prev.Data.Order;
        prev.Data.Order = config.Data.Order;
        config.Data.Order = tmp;

        await db.SaveChangesAsync();

        var index = this.Configs.IndexOf(config);
        this.Configs.Remove(prev);
        this.Configs.Insert(index, prev);
      }
      catch (Exception ex)
      {
        logger.Error("印設定の並べ替えでエラー", ex);
      }
    }

    public async Task DownConfigAsync(HorseMarkConfig config)
    {
      try
      {
        var next = this.Configs.SkipWhile(c => c != config).ElementAtOrDefault(1);
        if (next == null)
        {
          return;
        }

        using var db = new MyContext();
        db.HorseMarkConfigs!.Attach(next.Data);
        db.HorseMarkConfigs!.Attach(config.Data);

        var tmp = next.Data.Order;
        next.Data.Order = config.Data.Order;
        config.Data.Order = tmp;

        await db.SaveChangesAsync();

        var index = this.Configs.IndexOf(config);
        this.Configs.Remove(next);
        this.Configs.Insert(index, next);
      }
      catch (Exception ex)
      {
        logger.Error("印設定の並べ替えでエラー", ex);
      }
    }

    public void Dispose()
    {
      this._disposables.Dispose();
      foreach (var config in this.Configs)
      {
        config.Dispose();
      }
    }
  }

  public class HorseMarkConfig : IDisposable
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    private readonly CompositeDisposable _disposables = new();

    public HorseMarkConfigData Data { get; }

    public ReactiveProperty<string> Name { get; } = new();

    public ReactiveProperty<bool> IsChecked { get; } = new();

    public HorseMarkConfig(HorseMarkConfigData data)
    {
      this.Data = data;
      this.Name.Value = data.Name;

      this.Name.Skip(1).Subscribe(async name =>
      {
        try
        {
          using var db = new MyContext();
          db.HorseMarkConfigs!.Attach(this.Data);
          this.Data.Name = name;
          await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
          logger.Error("印設定の更新でエラー", ex);
        }
      }).AddTo(this._disposables);
    }

    public void Dispose()
    {
      this._disposables.Dispose();
    }
  }
}
