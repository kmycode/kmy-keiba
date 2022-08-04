using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis.Generic;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.Memo
{
  public class PointLabelModel
  {
    private static bool _isLoaded;

    public static PointLabelModel Default { get; } = new();

    public static async Task InitializeAsync(MyContext db)
    {
      if (!_isLoaded)
      {
        await Default.LoadAsync(db);
        _isLoaded = true;
      }
    }

    public CheckableCollection<PointLabelConfig> Configs { get; } = new();

    public ReactiveProperty<PointLabelConfig?> ActiveConfig => this.Configs.ActiveItem;

    private PointLabelModel() { }

    private async Task LoadAsync(MyContext db)
    {
      var configRaws = await db.PointLabels!.ToArrayAsync();

      var configItems = new List<PointLabelConfig>();
      foreach (var config in configRaws)
      {
        var configItem = new PointLabelConfig(config);
        configItems.Add(configItem);
      }

      ThreadUtil.InvokeOnUiThread(() =>
      {
        foreach (var item in configItems)
        {
          this.Configs.Add(item);
        }
        if (this.Configs.Any())
        {
          this.Configs.First().IsChecked.Value = true;
        }
      });
    }

    public async Task AddConfigAsync()
    {
      using var db = new MyContext();

      var newItem = new PointLabelData();
      newItem.SetItems(new PointLabelItem[]
      {
        new PointLabelItem
        {
          Point = 0,
        },
      });
      await db.PointLabels!.AddAsync(newItem);
      await db.SaveChangesAsync();

      var config = new PointLabelConfig(newItem);
      this.Configs.Add(config);
      config.IsChecked.Value = true;
    }

    public async Task DeleteConfigAsync()
    {
      var config = this.ActiveConfig.Value;
      if (config == null)
      {
        return;
      }

      using var db = new MyContext();

      db.PointLabels!.Remove(config.Data);
      await db.SaveChangesAsync();

      this.Configs.Remove(config);
      config.Dispose();
      this.ActiveConfig.Value = null;

      RaceMemoModel.DeletePointLabelConfig(config.Data.Id);
    }
  }

  public class PointLabelConfig : IDisposable, ICheckableItem
  {
    private readonly CompositeDisposable _disposables = new();

    public PointLabelData Data { get; }

    public ReactiveProperty<string> Name { get; } = new();

    public ReactiveProperty<bool> IsChecked { get; } = new();

    public ReactiveCollection<PointLabelConfigItem> Items { get; } = new();

    public PointLabelConfig(PointLabelData data)
    {
      this.Data = data;
      this.Name.Value = data.Name;

      var raws = data.GetItems();
      foreach (var item in raws)
      {
        this.Items.Add(new PointLabelConfigItem(this, item));
      }

      this.Name.Subscribe(async n =>
      {
        if (this.Data.Name != n)
        {
          using var db = new MyContext();
          db.Attach(this.Data);
          this.Data.Name = n;
          await db.SaveChangesAsync();
        }
      }).AddTo(this._disposables);
    }

    public PointLabelConfigItem? GetLabelItem(short point)
    {
      return this.Items.Where(i => i.Data.Point == point).FirstOrDefault();
    }

    public string GetLabel(short point)
    {
      return this.GetLabelItem(point)?.Data.Label ?? point.ToString();
    }

    public MemoColor GetColor(short point)
    {
      return this.GetLabelItem(point)?.Data.Color ?? MemoColor.Default;
    }

    public async Task SaveItemsAsync(MyContext db)
    {
      db.PointLabels!.Attach(this.Data);
      this.Data.SetItems(this.Items.Select(i => i.Data));
      await db.SaveChangesAsync();
    }

    public async Task AddItemAsync()
    {
      var point = 0;
      if (this.Items.Any())
      {
        point = this.Items.Max(i => i.Data.Point);
      }
      point++;

      var item = new PointLabelConfigItem(this, new PointLabelItem
      {
        Point = (short)point,
      });
      this.Items.Add(item);

      using var db = new MyContext();
      await this.SaveItemsAsync(db);
    }

    public async Task RemoveItemAsync(PointLabelConfigItem item)
    {
      this.Items.Remove(item);
      using var db = new MyContext();
      await this.SaveItemsAsync(db);

      RaceMemoModel.UpdatePointLabel(item.Config.Data.Id, item.Data.Point);
    }

    public async Task UpItemAsync(PointLabelConfigItem item)
    {
      var target = this.Items.TakeWhile(i => i != item).LastOrDefault();
      if (target != null)
      {
        var index = this.Items.IndexOf(item);
        this.Items.Remove(target);
        this.Items.Insert(index, target);
        RaceMemoModel.UpdatePointLabel(target.Config.Data.Id, target.Data.Point);

        using var db = new MyContext();
        await this.SaveItemsAsync(db);
      }
    }

    public async Task DownItemAsync(PointLabelConfigItem item)
    {
      var target = this.Items.SkipWhile(i => i != item).ElementAtOrDefault(1);
      if (target != null)
      {
        var index = this.Items.IndexOf(target);
        this.Items.Remove(item);
        this.Items.Insert(index, item);
        RaceMemoModel.UpdatePointLabel(item.Config.Data.Id, item.Data.Point);

        using var db = new MyContext();
        await this.SaveItemsAsync(db);
      }
    }

    public void Dispose()
    {
      this._disposables.Dispose();
      foreach (var item in this.Items)
      {
        item.Dispose();
      }
    }
  }

  public class PointLabelConfigItem : IDisposable
  {
    private readonly CompositeDisposable _disposables = new();

    public PointLabelConfig Config { get; }

    public PointLabelItem Data { get; }

    public ReactiveProperty<string> Label { get; } = new();

    public ReactiveProperty<string> Point { get; } = new();

    public ReactiveProperty<bool> IsColorDefault { get; } = new();

    public ReactiveProperty<bool> IsColorGood { get; } = new();

    public ReactiveProperty<bool> IsColorBad { get; } = new();

    public ReactiveProperty<bool> IsColorWarning { get; } = new();

    public ReactiveProperty<bool> IsColorNegative { get; } = new();

    public ReactiveProperty<MemoColor> Color { get; } = new();

    public ReactiveProperty<bool> IsError { get; } = new();

    public ReactiveProperty<bool> IsSaving { get; } = new();

    private bool _isSaving;

    public PointLabelConfigItem(PointLabelConfig config, PointLabelItem data)
    {
      this.Config = config;
      this.Data = data;
      this.Label.Value = data.Label;
      this.Point.Value = data.Point.ToString();
      this.SetColorCheck(data.Color);

      this.Label.Subscribe(async l =>
      {
        var old = this.Data.Label;
        if (l != old)
        {
          var r = await this.SetValueAsync(() => this.Data.Label = l, () =>
          {
            this.Data.Label = old;
            this.Label.Value = old;
          }, db =>
          {
            RaceMemoModel.UpdatePointLabel(this.Config.Data.Id, this.Data.Point);
            return Task.CompletedTask;
          });
          if (r) this.IsError.Value = false;
        }
      }).AddTo(this._disposables);

      this.Point.Subscribe(async p =>
      {
        if (!short.TryParse(p, out var point))
        {
          this.IsError.Value = true;
          return;
        }

        var old = this.Data.Point;
        if (this.Config.Items.Any(i => i.Data.Point == point))
        {
          this.IsError.Value = true;
          this._isSaving = true;
          this.Data.Point = old;
          this.Point.Value = old.ToString();
          this._isSaving = false;
          return;
        }

        if (point != old)
        {
          var r = await this.SetValueAsync(() => this.Data.Point = point, () =>
          {
            this.Data.Point = old;
            this.Point.Value = old.ToString();
          }, db => this.UpdatePointAsync(db, old, point));
          if (r) this.IsError.Value = false;
        }
      }).AddTo(this._disposables);

      this.IsColorGood.Subscribe(async _ => await this.UpdateColorAsync()).AddTo(this._disposables);
      this.IsColorBad.Subscribe(async _ => await this.UpdateColorAsync()).AddTo(this._disposables);
      this.IsColorWarning.Subscribe(async _ => await this.UpdateColorAsync()).AddTo(this._disposables);
      this.IsColorNegative.Subscribe(async _ => await this.UpdateColorAsync()).AddTo(this._disposables);
      this.IsColorDefault.Subscribe(async _ => await this.UpdateColorAsync()).AddTo(this._disposables);
    }

    private void SetColorCheck(MemoColor color)
    {
      switch (color)
      {
        case MemoColor.Good:
          this.IsColorGood.Value = true;
          break;
        case MemoColor.Bad:
          this.IsColorBad.Value = true;
          break;
        case MemoColor.Warning:
          this.IsColorWarning.Value = true;
          break;
        case MemoColor.Negative:
          this.IsColorNegative.Value = true;
          break;
        default:
          this.IsColorDefault.Value = true;
          break;
      }
      this.Color.Value = color;
    }

    private MemoColor GetSelectedColor()
    {
      if (this.IsColorGood.Value)
      {
        return MemoColor.Good;
      }
      if (this.IsColorBad.Value)
      {
        return MemoColor.Bad;
      }
      if (this.IsColorWarning.Value)
      {
        return MemoColor.Warning;
      }
      if (this.IsColorNegative.Value)
      {
        return MemoColor.Negative;
      }
      return MemoColor.Default;
    }

    private async Task UpdatePointAsync(MyContext db, short old, short @new)
    {
      await RaceMemoModel.UpdateLabelPointNumbersAsync(db, this.Config.Data.Id, old, @new);
    }

    private async Task UpdateColorAsync()
    {
      var old = this.Data.Color;
      var value = this.GetSelectedColor();
      if (value != old)
      {
        var r = await this.SetValueAsync(() => this.Data.Color = value, () =>
        {
          this.Data.Color = old;
          this.SetColorCheck(old);
        }, db =>
        {
          RaceMemoModel.UpdatePointLabel(this.Config.Data.Id, this.Data.Point);
          return Task.CompletedTask;
        });
        if (r)
        {
          this.IsError.Value = false;
          this.Color.Value = this.GetSelectedColor();
        }
      }
    }

    private async Task<bool> SetValueAsync(Action setValue, Action rollbackValue, Func<MyContext, Task>? afterSaving)
    {
      if (this._isSaving)
      {
        return false;
      }
      while (this.IsSaving.Value)
      {
        await Task.Delay(100);
      }
      this.IsSaving.Value = true;
      this._isSaving = true;
      this.IsError.Value = false;

      try
      {
        using var db = new MyContext();
        setValue();
        await this.Config.SaveItemsAsync(db);

        if (afterSaving != null)
        {
          await afterSaving(db);
        }
      }
      catch
      {
        this.IsError.Value = true;
        rollbackValue();
      }
      finally
      {
        this.IsSaving.Value = false;
        this._isSaving = false;
      }

      return true;
    }

    public void Dispose()
    {
      this._disposables.Dispose();
    }
  }
}
