﻿using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Race.Expand
{
  public class RaceMemoModel : IDisposable
  {
    private readonly CompositeDisposable _disposables = new();
    private static List<ExpansionMemoConfig>? _configs;
    private static readonly List<RaceMemoItem> _memoCaches = new();
    private static bool _isRaceTab = true;
    private static bool _isRaceHorseTab;

    // 他のウィンドウのデータ
    private static readonly List<RaceMemoModel> _allModels = new();

    private static async Task UpdateConfigsAsync(MyContext db)
    {
      if (_configs == null)
      {
        _configs = await db.MemoConfigs!.ToListAsync();
      }
    }

    private static async Task ReloadAllModelsAsync(MyContext db, RaceMemoModel self)
    {
      foreach (var model in _allModels.Where(m => m != self))
      {
        await model.LoadAsync(db);
      }
    }

    public ReactiveCollection<RaceMemoItem> RaceMemos { get; } = new();

    public ReactiveCollection<RaceHorseMemoItem> RaceHorseMemos { get; } = new();

    public RaceMemoConfig Config { get; } = new();

    public RaceData Race { get; }

    public IReadOnlyList<RaceHorseData> RaceHorses { get; }

    public ReactiveProperty<bool> IsCreating { get; } = new();

    public ReactiveProperty<bool> IsEditing { get; } = new();

    public ReactiveProperty<bool> IsRaceView { get; } = new(_isRaceTab);

    public ReactiveProperty<bool> IsRaceHorseView { get; } = new(_isRaceHorseTab);

    public ReactiveCollection<RaceHorseMemoGroupInfo> Groups { get; } = new();

    private ExpansionMemoConfig? editingConfig;

    public RaceMemoModel(RaceData race, IReadOnlyList<RaceHorseData> raceHorses)
    {
      this.Race = race;
      if (raceHorses.All(rh => rh.Number == default))
      {
        this.RaceHorses = raceHorses.OrderBy(h => h.Name).ToArray();
      }
      else
      {
        this.RaceHorses = raceHorses.OrderBy(h => h.Number).ToArray();
      }
      _allModels.Add(this);

      this.IsRaceView.Subscribe(v => _isRaceTab = v).AddTo(this._disposables);
      this.IsRaceHorseView.Subscribe(v => _isRaceHorseTab = v).AddTo(this._disposables);

      this.IsCreating.Where(v => v).Subscribe(_ =>
      {
        this.IsEditing.Value = false;
        this.Config.CopyFromData(new ExpansionMemoConfig());
        this.editingConfig = null;
      }).AddTo(this._disposables);
      this.IsEditing.Where(v => v).Subscribe(_ => this.IsCreating.Value = false).AddTo(this._disposables);

      foreach (var num in Enumerable.Range(1, 8))
      {
        var group = new RaceHorseMemoGroupInfo(num);
        group.IsChecked.Where(c => c).Subscribe(c =>
        {
          this.ChangeGroup(group.GroupNumber, this.RaceHorseMemos);
        }).AddTo(this._disposables);
        this.Groups.Add(group);
      }
      this.Groups.First().IsChecked.Value = true;
    }

    private void ChangeGroup(int num, IEnumerable<RaceHorseMemoItem> horses)
    {
      foreach (var memo in horses.SelectMany(m => m.Memos))
      {
        memo.IsGroupVisible.Value = memo.Config.MemoGroup == num;
      }
    }

    private int GetCurrentGroup()
    {
      var group = this.Groups.FirstOrDefault(g => g.IsChecked.Value);
      return group?.GroupNumber ?? 1;
    }

    public async Task LoadAsync(MyContext db)
    {
      await UpdateConfigsAsync(db);
      try
      {
        await db.BeginTransactionAsync();
      }
      catch
      {
        // トランザクションは任意
      }

      var raceMemos = new List<RaceMemoItem>();
      var raceHorseMemos = new List<RaceHorseMemoItem>();

      // レースメモ
      foreach (var config in _configs!.Where(c => c.Type == MemoType.Race).OrderBy(c => c.Id).OrderBy(c => c.Order))
      {
        // 同じメモが同時に表示される可能性はないが、他のウィンドウで表示されているかもしれない
        var existsMemoQuery = _memoCaches.Concat(_allModels
          .Where(m => m != this)
          .SelectMany(m => m.RaceMemos));
        existsMemoQuery = await this.GetMemoQueryAsync(db, existsMemoQuery, config, null);
        var existsMemo = existsMemoQuery.FirstOrDefault();

        if (existsMemo != null)
        {
          raceMemos.Add(existsMemo);
        }
        else
        {
          RaceMemoItem newItem;
          var memo = await (await this.GetMemoQueryAsync(db, db.Memos!, config, null)).FirstOrDefaultAsync();
          if (memo != null)
          {
            newItem = new RaceMemoItem(memo, config).AddTo(this._disposables);
          }
          else
          {
            newItem = new RaceMemoItem(await this.GenerateMemoDataAsync(db, config, null), config);
          }
          newItem.IsGroupVisible.Value = true;
          raceMemos.Add(newItem);
          _memoCaches.Add(newItem);
        }
      }

      // 馬メモ
      foreach (var horse in this.RaceHorses)
      {
        var horseMemo = new RaceHorseMemoItem(Race, horse).AddTo(this._disposables);
        raceHorseMemos.Add(horseMemo);

        foreach (var config in _configs!.Where(c => c.Type == MemoType.RaceHorse).OrderBy(c => c.Id).OrderBy(c => c.Order))
        {
          var existsMemoQuery = _memoCaches.Concat(_allModels
            .SelectMany(m => m.RaceHorseMemos.SelectMany(h => h.Memos)))
            .Concat(raceHorseMemos.SelectMany(m => m.Memos));
          existsMemoQuery = await this.GetMemoQueryAsync(db, existsMemoQuery, config, horse);
          var existsMemo = existsMemoQuery.FirstOrDefault();

          if (existsMemo != null)
          {
            horseMemo.Memos.Add(existsMemo);
          }
          else
          {
            RaceMemoItem newItem;
            var memo = await (await this.GetMemoQueryAsync(db, db.Memos!, config, horse)).FirstOrDefaultAsync();
            if (memo != null)
            {
              newItem = new RaceMemoItem(memo, config);
            }
            else
            {
              newItem = new RaceMemoItem(await this.GenerateMemoDataAsync(db, config, horse), config);
            }
            horseMemo.Memos.Add(newItem);
            _memoCaches.Add(newItem);
          }
        }
      }

      this.ChangeGroup(this.GetCurrentGroup(), raceHorseMemos);

      ThreadUtil.InvokeOnUiThread(() =>
      {
        this.RaceMemos.Clear();
        this.RaceHorseMemos.Clear();
        foreach (var item in raceMemos)
        {
          this.RaceMemos.Add(item);
        }
        foreach (var item in raceHorseMemos)
        {
          this.RaceHorseMemos.Add(item);
        }
      });
    }

    public async Task AddConfigAsync()
    {
      var isNewMemoNumber = this.Config.MemoNumber.Value == "0";

      using var db = new MyContext();

      var config = new ExpansionMemoConfig();
      var isSucceed = await this.Config.CopyDataAsync(db, config, this.IsRaceView.Value);
      if (!isSucceed)
      {
        return;
      }

      if (_configs!.Any())
      {
        config.Order = (short)(_configs!.Max(c => c.Order) + 1);
      }
      config.MemoGroup = (short)this.GetCurrentGroup();

      await db.MemoConfigs!.AddAsync(config);
      await db.SaveChangesAsync();
      _configs!.Add(config);

      if (isNewMemoNumber)
      {
        // これでは同じ番号のつけられた既存のメモがロードできない
        var data = await this.GenerateMemoDataAsync(db, config, null);
        if (config.Type == MemoType.Race)
        {
          ThreadUtil.InvokeOnUiThread(() =>
          {
            var newItem = new RaceMemoItem(data, config);
            this.RaceMemos.Add(newItem);
          });
        }
        else
        {
          ThreadUtil.InvokeOnUiThread(() =>
          {
            foreach (var horse in this.RaceHorseMemos)
            {
              var newItem = new RaceMemoItem(data, config);
              newItem.IsGroupVisible.Value = true;
              horse.Memos.Add(newItem);
            }
          });
        }
      }
      else
      {
        await this.LoadAsync(db);
      }
      await ReloadAllModelsAsync(db, this);
      this.IsCreating.Value = false;
    }

    public void StartEditRaceMemoConfig(ExpansionMemoConfig config)
    {
      this.IsEditing.Value = true;
      this.Config.CopyFromData(config);
      this.editingConfig = config;
    }

    public async Task UpdateConfigAsync()
    {
      using var db = new MyContext();

      if (this.editingConfig != null)
      {
        db.MemoConfigs!.Attach(this.editingConfig);
        var isSucceed = await this.Config.CopyDataAsync(db, this.editingConfig, this.IsRaceView.Value);
        if (isSucceed)
        {
          await db.SaveChangesAsync();
          UpdateConfigs(this.editingConfig);
          this.IsEditing.Value = false;
        }
      }
    }

    private static void UpdateConfigs(ExpansionMemoConfig config)
    {
      var exists = _configs!.FirstOrDefault(c => c.Id == config.Id);
      if (exists != null)
      {
        _configs![_configs.IndexOf(exists)] = config;

        void SetValues(RaceMemoItem item)
        {
          // TODO 既存のモデルに更新を反映（ラベルとか）
          item.Header.Value = config.Header;
        }

        foreach (var model in _allModels)
        {
          var item = model.RaceMemos.FirstOrDefault(m => m.Config.Id == exists.Id);
          if (item != null)
          {
            SetValues(item);
          }

          foreach (var horse in model.RaceHorseMemos.SelectMany(h => h.Memos.Where(m => m.Config.Id == exists.Id)))
          {
            SetValues(horse);
          }
        }
      }
    }

    public async Task DeleteConfigAsync()
    {
      using var db = new MyContext();

      if (this.editingConfig != null)
      {
        db.MemoConfigs!.Remove(this.editingConfig);
        await db.SaveChangesAsync();

        var exists = _configs!.FirstOrDefault(c => c.Id == this.editingConfig.Id);
        if (exists != null)
        {
          _configs!.Remove(exists);
        }
      }

      this.IsEditing.Value = false;

      await this.LoadAsync(db);
      await ReloadAllModelsAsync(db, this);
    }

    public async Task UpConfigOrderAsync()
    {
      using var db = new MyContext();

      if (this.editingConfig != null)
      {
        var exists = _configs!.FirstOrDefault(c => c.Id == this.editingConfig.Id);
        if (exists != null)
        {
          var target = _configs!.Where(c => c.Order < exists.Order && c.Type == exists.Type).OrderByDescending(c => c.Order).FirstOrDefault();
          if (target != null)
          {
            db.MemoConfigs!.Attach(target);
            db.MemoConfigs!.Attach(exists);
            var order = target.Order;
            target.Order = this.editingConfig.Order;
            this.editingConfig.Order = exists.Order = order;
            await db.SaveChangesAsync();

            await this.LoadAsync(db);
            await ReloadAllModelsAsync(db, this);
          }
        }
      }
    }

    public async Task DownConfigOrderAsync()
    {
      using var db = new MyContext();

      if (this.editingConfig != null)
      {
        var exists = _configs!.FirstOrDefault(c => c.Id == this.editingConfig.Id);
        if (exists != null)
        {
          var target = _configs!.Where(c => c.Order > exists.Order && c.Type == exists.Type).OrderBy(c => c.Order).FirstOrDefault();
          if (target != null)
          {
            db.MemoConfigs!.Attach(target);
            db.MemoConfigs!.Attach(exists);
            var order = target.Order;
            target.Order = this.editingConfig.Order;
            this.editingConfig.Order = exists.Order = order;
            await db.SaveChangesAsync();

            await this.LoadAsync(db);
            await ReloadAllModelsAsync(db, this);
          }
        }
      }
    }

    private async Task<MemoData> GenerateMemoDataAsync(MyContext db, ExpansionMemoConfig config, RaceHorseData? horse)
    {
      var data = new MemoData
      {
        Target1 = config.Target1,
        Target2 = config.Target2,
        Target3 = config.Target3,
        Number = config.MemoNumber,
      };

      async Task<string> GetKeyAsync(MemoTarget target)
      {
        if (target == MemoTarget.Course)
        {
          data!.CourseKey = this.Race.Course;
          return string.Empty;
        }
        else if (target == MemoTarget.Race)
        {
          return this.Race.Key;
        }
        else if (target == MemoTarget.Day)
        {
          return this.Race.Key[..8];
        }
        else if (horse != null)
        {
          if (target == MemoTarget.Horse)
          {
            return horse.Key;
          }
          else if (target == MemoTarget.Rider)
          {
            return horse.RiderCode;
          }
          else if (target == MemoTarget.Trainer)
          {
            return horse.TrainerCode;
          }
          else if (target == MemoTarget.Owner)
          {
            return horse.OwnerCode;
          }
          else if (target == MemoTarget.Father)
          {
            return await HorseBloodUtil.GetBloodCodeAsync(db, horse.Key, BloodType.Father);
          }
        }

        return string.Empty;
      }

      data.Key1 = await GetKeyAsync(data.Target1);
      data.Key2 = await GetKeyAsync(data.Target2);
      data.Key3 = await GetKeyAsync(data.Target3);

      return data;
    }

    private async Task<IQueryable<MemoData>> GetMemoQueryAsync(MyContext db, IQueryable<MemoData> query, ExpansionMemoConfig config, RaceHorseData? horse)
    {
      query = query.Where(m => m.Target1 == config.Target1 && m.Target2 == config.Target2 && m.Target3 == config.Target3 && m.Number == config.MemoNumber);
      query = query.Where(await GetTargetComparationAsync(db, 1, config.Target1, horse));
      if (config.Target2 != MemoTarget.Unknown)
        query = query.Where(await GetTargetComparationAsync(db, 2, config.Target2, horse));
      if (config.Target3 != MemoTarget.Unknown)
        query = query.Where(await GetTargetComparationAsync(db, 3, config.Target3, horse));
      return query;
    }

    private async Task<IEnumerable<MemoData>> GetMemoQueryAsync(MyContext db, IEnumerable<MemoData> query, ExpansionMemoConfig config, RaceHorseData? horse)
    {
      query = query.Where(m => m.Target1 == config.Target1 && m.Target2 == config.Target2 && m.Target3 == config.Target3 && m.Number == config.MemoNumber);
      query = query.Where((await GetTargetComparationAsync(db, 1, config.Target1, horse)).Compile());
      if (config.Target2 != MemoTarget.Unknown)
        query = query.Where((await GetTargetComparationAsync(db, 2, config.Target2, horse)).Compile());
      if (config.Target3 != MemoTarget.Unknown)
        query = query.Where((await GetTargetComparationAsync(db, 3, config.Target3, horse)).Compile());
      return query;
    }

    private async Task<IEnumerable<RaceMemoItem>> GetMemoQueryAsync(MyContext db, IEnumerable<RaceMemoItem> query, ExpansionMemoConfig config, RaceHorseData? horse)
    {
      var hits = await this.GetMemoQueryAsync(db, query.Select(i => i.Data), config, horse);
      return query.Join(hits.ToArray(), i => (object)i.Data, m => (object)m, (i, m) => i);
    }

    private async Task<Expression<Func<MemoData, bool>>> GetTargetComparationAsync(MyContext db, int target, MemoTarget targetType, RaceHorseData? horse)
    {
      var memo = Expression.Parameter(typeof(MemoData), "x");
      var key = Expression.Property(memo, "Key" + target);

      if (targetType == MemoTarget.Race)
      {
        return Expression.Lambda<Func<MemoData, bool>>(Expression.Equal(key, Expression.Constant(this.Race.Key)), memo);
      }
      else if (targetType == MemoTarget.Course)
      {
        key = Expression.Property(memo, nameof(MemoData.CourseKey));
        return Expression.Lambda<Func<MemoData, bool>>(Expression.Equal(key, Expression.Constant(this.Race.Course)), memo);
      }
      else if (targetType == MemoTarget.Day)
      {
        return Expression.Lambda<Func<MemoData, bool>>(Expression.Equal(key, Expression.Constant(this.Race.Key[..8])), memo);
      }
      else if (targetType == MemoTarget.Unknown)
      {
        return Expression.Lambda<Func<MemoData, bool>>(Expression.Constant(true), memo);
      }
      else if (horse != null)
      {
        if (targetType == MemoTarget.Horse)
        {
          return Expression.Lambda<Func<MemoData, bool>>(Expression.Equal(key, Expression.Constant(horse.Key)), memo);
        }
        else if (targetType == MemoTarget.Rider)
        {
          return Expression.Lambda<Func<MemoData, bool>>(Expression.Equal(key, Expression.Constant(horse.RiderCode)), memo);
        }
        else if (targetType == MemoTarget.Trainer)
        {
          return Expression.Lambda<Func<MemoData, bool>>(Expression.Equal(key, Expression.Constant(horse.TrainerCode)), memo);
        }
        else if (targetType == MemoTarget.Owner)
        {
          return Expression.Lambda<Func<MemoData, bool>>(Expression.Equal(key, Expression.Constant(horse.OwnerCode)), memo);
        }
        else if (targetType == MemoTarget.Father)
        {
          var father = await HorseBloodUtil.GetBloodCodeAsync(db, horse.Key, BloodType.Father);
          if (!string.IsNullOrEmpty(father))
          {
            return Expression.Lambda<Func<MemoData, bool>>(Expression.Equal(key, Expression.Constant(father)), memo);
          }
        }
      }

      return Expression.Lambda<Func<MemoData, bool>>(Expression.Constant(false), memo);
    }

    public void Dispose()
    {
      this._disposables.Dispose();
      _allModels.Remove(this);
    }
  }

  public class RaceMemoConfig
  {
    public ReactiveProperty<string> Header { get; } = new();

    public ReactiveProperty<bool> IsFilterRace { get; } = new();

    public ReactiveProperty<bool> IsFilterDay { get; } = new();

    public ReactiveProperty<bool> IsFilterCourse { get; } = new();

    public ReactiveProperty<bool> IsFilterHorse { get; } = new();

    public ReactiveProperty<bool> IsFilterRider { get; } = new();

    public ReactiveProperty<bool> IsFilterTrainer { get; } = new();

    public ReactiveProperty<bool> IsFilterOwner { get; } = new();

    public ReactiveProperty<bool> IsFilterFather { get; } = new();

    public ReactiveProperty<bool> IsStylePoint { get; } = new();

    public ReactiveProperty<bool> IsStyleMemo { get; } = new();

    public ReactiveProperty<bool> IsStylePointAndMemo { get; } = new(true);

    public ReactiveProperty<bool> IsUseLabel { get; } = new();

    public ReactiveProperty<string> MemoNumber { get; } = new("0");

    public ReactiveProperty<string> ErrorMessage { get; } = new();

    private IReadOnlyList<MemoTarget> GetTargets(bool isRaceMemo)
    {
      var list = new List<MemoTarget>();
      if (this.IsFilterDay.Value) list.Add(MemoTarget.Day);
      if (this.IsFilterCourse.Value) list.Add(MemoTarget.Course);
      if (this.IsFilterRace.Value) list.Add(MemoTarget.Race);
      if (!isRaceMemo)
      {
        if (this.IsFilterOwner.Value) list.Add(MemoTarget.Owner);
        if (this.IsFilterHorse.Value) list.Add(MemoTarget.Horse);
        if (this.IsFilterRider.Value) list.Add(MemoTarget.Rider);
        if (this.IsFilterTrainer.Value) list.Add(MemoTarget.Trainer);
        if (this.IsFilterFather.Value) list.Add(MemoTarget.Father);
      }

      return list.OrderBy(i => i).Take(3).ToArray();
    }

    public async Task<string> GetErrorMessageAsync(MyContext db, bool isRaceMemo, ExpansionMemoConfig editingConfig)
    {
      if (string.IsNullOrEmpty(this.Header.Value))
      {
        return "名前が指定されていません";
      }

      var targets = this.GetTargets(isRaceMemo);
      if (!targets.Any())
      {
        return "絞り込み条件が指定されていません";
      }
      var target1 = targets[0];
      var target2 = targets.ElementAtOrDefault(1);
      var target3 = targets.ElementAtOrDefault(2);

      if (!isRaceMemo)
      {
        var horseConfigs = new[] { MemoTarget.Horse, MemoTarget.Rider, MemoTarget.Trainer, MemoTarget.Owner, MemoTarget.Father };
        if (!horseConfigs.Contains(target1) && !horseConfigs.Contains(target2) && !horseConfigs.Contains(target3))
        {
          return "馬の情報が絞り込み条件に含まれていません";
        }
      }

      var r = short.TryParse(this.MemoNumber.Value, out var number);
      if (!r)
      {
        return "識別番号に数値が指定されていません";
      }

      var exists = await db.MemoConfigs!.Where(c => c.Target1 == target1 && c.Target2 == target2 && c.Target3 == target3).ToArrayAsync();
      if (exists.Any(i => i.Id != editingConfig.Id && i.MemoNumber == number))
      {
        return "同一の絞り込み条件で同一の識別番号が設定されたメモがあります";
      }

      if (this.IsUseLabel.Value)
      {
        // TODO ラベル置換でその設定が存在するかのチェック
      }

      return string.Empty;
    }

    public void CopyFromData(ExpansionMemoConfig config)
    {
      this.IsFilterTrainer.Value = this.IsFilterOwner.Value = this.IsFilterDay.Value = this.IsFilterRider.Value =
        this.IsFilterRace.Value = this.IsFilterCourse.Value = this.IsFilterHorse.Value = false;
      var targets = new MemoTarget[] { config.Target1, config.Target2, config.Target3 };
      foreach (var target in targets)
      {
        switch (target)
        {
          case MemoTarget.Course:
            this.IsFilterCourse.Value = true;
            break;
          case MemoTarget.Race:
            this.IsFilterRace.Value = true;
            break;
          case MemoTarget.Day:
            this.IsFilterDay.Value = true;
            break;
          case MemoTarget.Owner:
            this.IsFilterOwner.Value = true;
            break;
          case MemoTarget.Horse:
            this.IsFilterHorse.Value = true;
            break;
          case MemoTarget.Rider:
            this.IsFilterRider.Value = true;
            break;
          case MemoTarget.Trainer:
            this.IsFilterTrainer.Value = true;
            break;
          case MemoTarget.Father:
            this.IsFilterFather.Value = true;
            break;
        }
      }

      this.IsUseLabel.Value = config.PointLabelId != default;
      switch (config.Style)
      {
        case MemoStyle.MemoAndPoint:
          this.IsStylePointAndMemo.Value = true;
          break;
        case MemoStyle.Point:
          this.IsStylePoint.Value = true;
          break;
        case MemoStyle.Memo:
          this.IsStyleMemo.Value = true;
          break;
      }

      this.MemoNumber.Value = config.MemoNumber.ToString();
      this.Header.Value = config.Header;
    }

    public async Task<bool> CopyDataAsync(MyContext db, ExpansionMemoConfig config, bool isRaceMemo)
    {
      // var isRaceMemo = !this.IsFilterHorse.Value && !this.IsFilterOwner.Value && !this.IsFilterRider.Value && !this.IsFilterTrainer.Value;

      var error = await this.GetErrorMessageAsync(db, isRaceMemo, config);
      if (!string.IsNullOrEmpty(error))
      {
        this.ErrorMessage.Value = error;
        return false;
      }
      this.ErrorMessage.Value = string.Empty;

      var targets = this.GetTargets(isRaceMemo);
      var target1 = targets[0];
      var target2 = targets.ElementAtOrDefault(1);
      var target3 = targets.ElementAtOrDefault(2);

      config.Header = this.Header.Value;
      config.Type = isRaceMemo ? MemoType.Race : MemoType.RaceHorse;
      config.Target1 = target1;
      config.Target2 = target2;
      config.Target3 = target3;

      var style = MemoStyle.None;
      if (this.IsStyleMemo.Value) style |= MemoStyle.Memo;
      if (this.IsStylePoint.Value) style |= MemoStyle.Point;
      if (this.IsStylePointAndMemo.Value) style |= MemoStyle.MemoAndPoint;
      if (style == MemoStyle.None)
      {
        style = MemoStyle.MemoAndPoint;
      }
      config.Style = style;

      short.TryParse(this.MemoNumber.Value, out var number);
      if (number == default)
      {
        var numberMax = await db.MemoConfigs!
          .OrderByDescending(c => c.MemoNumber)
          .FirstOrDefaultAsync(c => c.Target1 == target1 && c.Target2 == target2 && c.Target3 == target3);
        if (numberMax != null)
        {
          number = (short)(numberMax.MemoNumber + 1);
        }
        else
        {
          number = 1;
        }
      }
      config.MemoNumber = number;

      // TODO ラベル置換
      // config.PointLabelId =

      return true;
    }
  }

  public class RaceMemoItem : IDisposable
  {
    private readonly CompositeDisposable _disposables = new();

    public ReactiveProperty<bool> IsStopSaving { get; } = new();

    public MemoData Data { get; }

    public ExpansionMemoConfig Config { get; }

    public PointLabelConfig? Labels { get; }

    public ReactiveProperty<string> Header { get; } = new();

    public ReactiveProperty<string> Memo { get; } = new();

    public ReactiveProperty<string> Point { get; } = new();

    public ReactiveProperty<MemoStyle> Style { get; } = new();

    public ReactiveProperty<bool> IsPointVisible { get; } = new();

    public ReactiveProperty<bool> IsMemoVisible { get; } = new();

    public ReactiveProperty<bool> IsGroupVisible { get; } = new();

    public RaceMemoItem(MemoData data, ExpansionMemoConfig config)
    {
      this.Data = data;
      this.Config = config;
      this.Memo.Value = data.Memo;
      this.Point.Value = data.Point.ToString();
      this.Header.Value = config.Header;
      this.IsPointVisible.Value = config.Style.HasFlag(MemoStyle.Point);
      this.IsMemoVisible.Value = config.Style.HasFlag(MemoStyle.Memo);

      this.Memo.Skip(1).Where(_ => !this.IsStopSaving.Value).Subscribe(async (memo) =>
      {
        if (this.Data.Memo != memo)
        {
          await this.SaveMemoAsync(m => m.Memo = memo, memo);
        }
      }).AddTo(this._disposables);

      this.Point.Skip(1).Where(_ => !this.IsStopSaving.Value).Subscribe(async (point) =>
      {
        if (short.TryParse(point, out var p))
        {
          if (this.Data.Point != p)
          {
            await this.SaveMemoAsync(m => m.Point = p, $"(Point {p})");
          }
        }
      }).AddTo(this._disposables);
    }

    private async Task SaveMemoAsync(Action<MemoData> save, string memo)
    {
      try
      {
        using var db = new MyContext();

        if (this.Data.Id == default)
        {
          save(this.Data);
          await db.Memos!.AddAsync(this.Data);
        }
        else
        {
          db.Memos!.Attach(this.Data);
          save(this.Data);
        }
        await db.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        OpenErrorSavingMemoRequest.Default.Request(memo);
      }
    }

    public void SetMemoWithoutSave(string value)
    {
      this.IsStopSaving.Value = true;
      try
      {
        this.Data.Memo = this.Memo.Value = value;
      }
      finally
      {
        this.IsStopSaving.Value = false;
      }
    }

    public void SetPointWithoutSave(short value)
    {
      this.IsStopSaving.Value = true;
      try
      {
        this.Data.Point = value;
        this.Point.Value = value.ToString();
      }
      finally
      {
        this.IsStopSaving.Value = false;
      }
    }

    public void Dispose()
    {
      this._disposables.Dispose();
    }
  }

  public class RaceHorseMemoItem : IDisposable
  {
    public RaceData Race { get; }

    public RaceHorseData RaceHorse { get; }

    public ReactiveCollection<RaceMemoItem> Memos { get; } = new();

    public RaceHorseMemoItem(RaceData race, RaceHorseData raceHorse)
    {
      this.Race = race;
      this.RaceHorse = raceHorse;
    }

    public void Dispose()
    {
      foreach (var memo in this.Memos)
      {
        memo.Dispose();
      }
    }
  }

  public class RaceHorseMemoGroupInfo
  {
    public int GroupNumber { get; }

    public ReactiveProperty<bool> IsChecked { get; } = new();

    public RaceHorseMemoGroupInfo(int number)
    {
      this.GroupNumber = number;
    }
  }
}
