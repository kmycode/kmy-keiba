using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Data.Wrappers;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Connection;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race.AnalysisTable;
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

namespace KmyKeiba.Models.Race.Memo
{
  public class RaceMemoModel : IDisposable
  {
    private static bool _isInitialized;
    private readonly CompositeDisposable _disposables = new();
    private static bool _isRaceTab = true;
    private static bool _isRaceHorseTab;
    private static int _groupNumber = 1;
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);


    // 他のウィンドウのデータ
    private static readonly List<RaceMemoModel> _allModels = new();

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

    public PointLabelModel LabelConfig => throw new NotSupportedException("このプロパティはViewModelに移動します");

    public ReactiveProperty<HorseTeamModel?> TeamModel { get; } = new();

    public RaceData Race { get; }

    public IReadOnlyList<RaceHorseAnalyzer> RaceHorses { get; }

    public ReactiveProperty<bool> IsCreating { get; } = new();

    public ReactiveProperty<bool> IsEditing { get; } = new();

    public ReactiveProperty<bool> IsRaceView { get; } = new(_isRaceTab);

    public ReactiveProperty<bool> IsRaceHorseView { get; } = new(_isRaceHorseTab);

    public ReactiveCollection<RaceHorseMemoGroupInfo> Groups { get; } = new();

    public ReactiveCollection<RaceMemoItem> RaceMemoSelections { get; } = new();

    public ReactiveProperty<bool> CanSave => DownloaderModel.Instance.CanSaveOthers;

    private ExpansionMemoConfig? editingConfig;

    public RaceMemoModel(RaceData race, IReadOnlyList<RaceHorseAnalyzer> raceHorses)
    {
      this.Race = race;
      if (raceHorses.All(rh => rh.Data.Number == default))
      {
        this.RaceHorses = raceHorses.OrderBy(h => h.Data.Name).ToArray();
      }
      else
      {
        this.RaceHorses = raceHorses.OrderBy(h => h.Data.Number).ToArray();
      }
      _allModels.Add(this);

      this.IsRaceView.Subscribe(v =>
      {
        this.IsEditing.Value = false;
        _isRaceTab = v;
        this.Config.IsRaceTab.Value = v;
      }).AddTo(this._disposables);
      this.IsRaceHorseView.Subscribe(v =>
      {
        this.IsEditing.Value = false;
        _isRaceHorseTab = v;
      }).AddTo(this._disposables);

      this.IsCreating.Where(v => v).Subscribe(_ =>
      {
        this.IsEditing.Value = false;
        this.Config.CopyFromData(new ExpansionMemoConfig());
        this.editingConfig = null;
      }).AddTo(this._disposables);
      this.IsEditing.Where(v => v).Subscribe(_ => this.IsCreating.Value = false).AddTo(this._disposables);

      foreach (var num in Enumerable.Range(1, Math.Max(1, ApplicationConfiguration.ExpansionMemoGroupSize)))
      {
        var group = new RaceHorseMemoGroupInfo(num);
        group.IsChecked.Where(c => c).Subscribe(c =>
        {
          this.ChangeGroup(group.GroupNumber, this.RaceHorseMemos);
          _groupNumber = group.GroupNumber;
        }).AddTo(this._disposables);
        this.Groups.Add(group);
      }
      var defaultGroup = this.Groups.FirstOrDefault(g => g.GroupNumber == _groupNumber);
      if (defaultGroup != null)
      {
        defaultGroup.IsChecked.Value = true;
      }
      else
      {
        this.Groups.First().IsChecked.Value = true;
      }
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
      try
      {
        if (DownloaderModel.Instance.CanSaveOthers.Value)
        {
          await db.BeginTransactionAsync();
        }
      }
      catch
      {
        // トランザクションは任意
      }

      try
      {
        var raceMemos = new List<RaceMemoItem>();
        var raceHorseMemos = new List<RaceHorseMemoItem>();

        // レースメモ
        foreach (var config in MemoUtil.Configs!.Where(c => c.Type == MemoType.Race).OrderBy(c => c.Id).OrderBy(c => c.Order))
        {
          // 同じメモが同時に表示される可能性はないが、他のウィンドウで表示されているかもしれない
          var existsMemoQuery = MemoUtil.MemoCaches.Concat(_allModels
            .Where(m => m != this)
            .SelectMany(m => m.RaceMemos));
          existsMemoQuery = await this.GetMemoQueryAsync(db, existsMemoQuery, config, null);
          var existsMemo = existsMemoQuery.FirstOrDefault();

          if (existsMemo != null)
          {
            raceMemos.Add(existsMemo);
            SetLabel(existsMemo, config);
          }
          else
          {
            RaceMemoItem newItem;
            var memo = await (await this.GetMemoQueryAsync(db, db.Memos!, config, null)).FirstOrDefaultAsync();
            if (memo != null)
            {
              newItem = new RaceMemoItem(memo, config);
            }
            else
            {
              newItem = new RaceMemoItem(await this.GenerateMemoDataAsync(db, config, null), config);
            }
            newItem.Name.Value = await this.GetItemNameAsync(db, newItem.Data, null);
            newItem.IsGroupVisible.Value = true;
            SetLabel(newItem, config);
            raceMemos.Add(newItem);
            MemoUtil.MemoCaches.Add(newItem);
          }
        }

        // 馬メモ
        foreach (var horse in this.RaceHorses)
        {
          var horseMemo = new RaceHorseMemoItem(Race, horse).AddTo(this._disposables);
          raceHorseMemos.Add(horseMemo);

          foreach (var config in MemoUtil.Configs!.Where(c => c.Type == MemoType.RaceHorse).OrderBy(c => c.Id).OrderBy(c => c.Order))
          {
            var existsMemoQuery = MemoUtil.MemoCaches.Concat(_allModels
              .SelectMany(m => m.RaceHorseMemos.SelectMany(h => h.Memos)))
              .Concat(raceHorseMemos.SelectMany(m => m.Memos));
            existsMemoQuery = await this.GetMemoQueryAsync(db, existsMemoQuery, config, horse);
            var existsMemo = existsMemoQuery.FirstOrDefault();

            if (existsMemo != null)
            {
              horseMemo.Memos.Add(existsMemo);
              SetLabel(existsMemo, config);
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
              try
              {
                newItem.Name.Value = await this.GetItemNameAsync(db, newItem.Data, horse);
              }
              catch (Exception ex)
              {

              }
              SetLabel(newItem, config);
              horseMemo.Memos.Add(newItem);
              MemoUtil.MemoCaches.Add(newItem);
            }
          }

          horse.MemoEx.Value = horseMemo;
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
          this.UpdateRaceMemoSelections();

          this.TeamModel.Value = new HorseTeamModel(this.RaceHorses, this.RaceHorseMemos);
        });
      }
      catch (Exception ex)
      {
        logger.Error("拡張メモのロードでエラー発生", ex);
      }
    }

    private static void SetLabel(RaceMemoItem memo, ExpansionMemoConfig config)
    {
      var label = PointLabelModel.Default.Configs.FirstOrDefault(c => c.Data.Id == (uint)config.PointLabelId);
      if (label != null)
      {
        memo.SetLabelConfig(label);
      }
      else
      {
        memo.RemoveLabelConfig();
      }
    }

    private void UpdateRaceMemoSelections()
    {
      var memos = this.RaceMemos.Where(r => r.Data.Target1 == MemoTarget.Race &&
        r.Data.Target2 == MemoTarget.Unknown && r.Data.Target3 == MemoTarget.Unknown &&
        r.Config.Style.HasFlag(MemoStyle.Point) && r.LabelConfig.Value != null)
        .Take(8).ToArray();

      ThreadUtil.InvokeOnUiThread(() =>
      {
        this.RaceMemoSelections.Clear();
        foreach (var memo in memos)
        {
          this.RaceMemoSelections.Add(memo);
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

      if (MemoUtil.Configs!.Any())
      {
        config.Order = (short)(MemoUtil.Configs!.Max(c => c.Order) + 1);
      }
      config.MemoGroup = (short)this.GetCurrentGroup();

      await db.MemoConfigs!.AddAsync(config);
      await db.SaveChangesAsync();
      MemoUtil.Configs!.Add(config);

      if (isNewMemoNumber)
      {
        // これでは同じ番号のつけられた既存のメモがロードできない

        if (config.Type == MemoType.Race)
        {
          var data = await this.GenerateMemoDataAsync(db, config, null);
          var newItem = new RaceMemoItem(data, config);
          newItem.Name.Value = await this.GetItemNameAsync(db, newItem.Data, null);
          newItem.IsGroupVisible.Value = true;
          SetLabel(newItem, config);
          ThreadUtil.InvokeOnUiThread(() =>
          {
            this.RaceMemos.Add(newItem);
            this.UpdateRaceMemoSelections();
          });
        }
        else
        {
          var items = new List<(RaceHorseMemoItem, RaceMemoItem)>();
          foreach (var horse in this.RaceHorseMemos)
          {
            var data = await this.GenerateMemoDataAsync(db, config, horse.RaceHorse);
            var newItem = new RaceMemoItem(data, config);
            newItem.Name.Value = await this.GetItemNameAsync(db, newItem.Data, horse.RaceHorse);
            newItem.IsGroupVisible.Value = true;
            SetLabel(newItem, config);
            items.Add((horse, newItem));
          }

          ThreadUtil.InvokeOnUiThread(() =>
          {
            foreach (var item in items)
            {
              item.Item1.Memos.Add(item.Item2);
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

      AnalysisTableConfigModel.Instance.OnMemoConfigChanged();
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
          if (this.editingConfig.Type == MemoType.RaceHorse)
          {
            this.editingConfig.MemoGroup = (short)this.GetCurrentGroup();
          }
          await db.SaveChangesAsync();
          UpdateConfigs(this.editingConfig);

          this.IsEditing.Value = false;

          AnalysisTableConfigModel.Instance.OnMemoConfigChanged();

          this.TeamModel.Value?.OnHorseMemoConfigUpdated(this.editingConfig.Id);
        }
      }
    }

    private static void UpdateConfigs(ExpansionMemoConfig config)
    {
      var exists = MemoUtil.Configs!.FirstOrDefault(c => c.Id == config.Id);
      if (exists != null)
      {
        MemoUtil.Configs![MemoUtil.Configs.IndexOf(exists)] = config;

        void SetValues(RaceMemoItem item)
        {
          item.UpdateConfig(config);
          SetLabel(item, config);
        }

        foreach (var memo in MemoUtil.MemoCaches.Where(m => m.Config.Id == exists.Id))
        {
          SetValues(memo);
        }

        foreach (var model in _allModels)
        {
          foreach (var nonCacheMemo in model.RaceMemos
            .Concat(model.RaceHorseMemos.SelectMany(m => m.Memos))
            .Where(m => m.Data.Id == default && m.Config.Id == exists.Id))
          {
            SetValues(nonCacheMemo);
          }

          if (config.Type == MemoType.RaceHorse)
          {
            model.ChangeGroup(model.GetCurrentGroup(), model.RaceHorseMemos);
          }
          else
          {
            model.UpdateRaceMemoSelections();
          }
        }

        AnalysisTableConfigModel.Instance.OnMemoConfigChanged();
      }
    }

    public async Task DeleteConfigAsync()
    {
      using var db = new MyContext();

      if (this.editingConfig != null)
      {
        try
        {
          db.MemoConfigs!.Remove(this.editingConfig);
          await db.SaveChangesAsync();

          var exists = MemoUtil.Configs!.FirstOrDefault(c => c.Id == this.editingConfig.Id);
          if (exists != null)
          {
            MemoUtil.Configs!.Remove(exists);
          }

          var existCaches = MemoUtil.MemoCaches.Where(c => c.Config.Id == this.editingConfig.Id).ToArray();
          foreach (var cache in existCaches)
          {
            cache.Dispose();
            MemoUtil.MemoCaches.Remove(cache);
          }

          AnalysisTableConfigModel.Instance.OnMemoConfigChanged();

          this.TeamModel.Value?.OnHorseMemoConfigUpdated(this.editingConfig.Id);
        }
        catch (Exception ex)
        {
          logger.Error("拡張メモ設定削除でエラー", ex);
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
        var exists = MemoUtil.Configs!.FirstOrDefault(c => c.Id == this.editingConfig.Id);
        if (exists != null)
        {
          var target = MemoUtil.Configs!.Where(c => c.Order < exists.Order && c.Type == exists.Type && c.MemoGroup == exists.MemoGroup).OrderByDescending(c => c.Order).FirstOrDefault();
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

            OnPointLabelOrderChangedForRaceList(exists, target);

            this.TeamModel.Value?.OnHorseMemoConfigUpdated(this.editingConfig.Id);
          }
        }
      }
    }

    public async Task DownConfigOrderAsync()
    {
      using var db = new MyContext();

      if (this.editingConfig != null)
      {
        var exists = MemoUtil.Configs!.FirstOrDefault(c => c.Id == this.editingConfig.Id);
        if (exists != null)
        {
          var target = MemoUtil.Configs!.Where(c => c.Order > exists.Order && c.Type == exists.Type && c.MemoGroup == exists.MemoGroup).OrderBy(c => c.Order).FirstOrDefault();
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

            OnPointLabelOrderChangedForRaceList(exists, target);

            this.TeamModel.Value?.OnHorseMemoConfigUpdated(this.editingConfig.Id);
          }
        }
      }
    }

    public static async Task UpdateLabelPointNumbersAsync(MyContext db, uint labelId, short old, short @new)
    {
      var targetConfigs = MemoUtil.Configs!.Where(c => c.PointLabelId == labelId).ToArray();
      var targetMemos = await db.Memos!
        .Join(db.MemoConfigs!, m => m.Target1 | m.Target2 | m.Target3, c => c.Target1 | c.Target2 | c.Target3, (m, c) => new { Memo = m, Config = c, })
        .Where(d => d.Memo.Number == d.Config.MemoNumber)
        .Select(d => d.Memo)
        .Where(m => m.Point == old)
        .ToArrayAsync();
      foreach (var memo in targetMemos)
      {
        memo.Point = @new;
      }
      await db.SaveChangesAsync();

      foreach (var item in MemoUtil.MemoCaches.Where(i => i.LabelConfig.Value?.Data.Id == labelId && i.Data.Point == old))
      {
        item.SetPointWithoutSave(@new);
      }
    }

    public static void UpdatePointLabel(uint labelId, short point)
    {
      var targetConfigs = MemoUtil.Configs!.Where(c => c.PointLabelId == labelId).ToArray();
      foreach (var item in MemoUtil.MemoCaches.Where(i => i.LabelConfig.Value?.Data.Id == labelId && i.Data.Point == point))
      {
        item.UpdateLabelConfig();
      }
    }

    public static void DeletePointLabelConfig(uint labelId)
    {
      var targetConfigs = MemoUtil.Configs!.Where(c => c.PointLabelId == labelId).ToArray();
      foreach (var item in MemoUtil.MemoCaches.Where(i => i.LabelConfig.Value?.Data.Id == labelId))
      {
        item.RemoveLabelConfig();
      }

      if (targetConfigs.Any(c => c.Target1 == MemoTarget.Race && c.Target2 == MemoTarget.Unknown && c.Target3 == MemoTarget.Unknown))
      {
        foreach (var model in _allModels)
        {
          model.UpdateRaceMemoSelections();
        }
      }
    }

    public static async Task DeleteLabelDataAsync(MyContext db, uint labelId)
    {
      var targetConfigs = MemoUtil.Configs!.Where(c => c.PointLabelId == labelId).ToArray();
      db.MemoConfigs!.AttachRange(targetConfigs);
      foreach (var config in targetConfigs)
      {
        config.PointLabelId = default;
      }
      await db.SaveChangesAsync();

      foreach (var item in MemoUtil.MemoCaches.Where(i => i.LabelConfig.Value?.Data.Id == labelId))
      {
        item.RemoveLabelConfig();
      }
    }

    private async Task<MemoData> GenerateMemoDataAsync(MyContext db, ExpansionMemoConfig config, RaceHorseAnalyzer? horse)
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
            return horse.Data.Key;
          }
          else if (target == MemoTarget.Rider)
          {
            return horse.Data.RiderCode;
          }
          else if (target == MemoTarget.Trainer)
          {
            return horse.Data.TrainerCode;
          }
          else if (target == MemoTarget.Owner)
          {
            return horse.Data.OwnerCode;
          }
          else if (target == MemoTarget.Father)
          {
            return await HorseBloodUtil.GetBloodCodeAsync(db, horse.Data.Key, BloodType.Father);
          }
          else if (target == MemoTarget.Mother)
          {
            return await HorseBloodUtil.GetBloodCodeAsync(db, horse.Data.Key, BloodType.Mother);
          }
          else if (target == MemoTarget.MotherFather)
          {
            return await HorseBloodUtil.GetBloodCodeAsync(db, horse.Data.Key, BloodType.MotherFather);
          }
        }

        return string.Empty;
      }

      data.Key1 = await GetKeyAsync(data.Target1);
      data.Key2 = await GetKeyAsync(data.Target2);
      data.Key3 = await GetKeyAsync(data.Target3);

      return data;
    }

    private async Task<IQueryable<MemoData>> GetMemoQueryAsync(MyContext db, IQueryable<MemoData> query, ExpansionMemoConfig config, RaceHorseAnalyzer? horse)
    {
      return await MemoUtil.GetMemoQueryAsync(db, this.Race, query, config, horse);
    }

    private async Task<IEnumerable<RaceMemoItem>> GetMemoQueryAsync(MyContext db, IEnumerable<RaceMemoItem> query, ExpansionMemoConfig config, RaceHorseAnalyzer? horse)
    {
      return await MemoUtil.GetMemoQueryAsync(db, this.Race, query, config, horse);
    }

    private async Task<string> GetItemNameAsync(MyContext db, MemoData memo, RaceHorseAnalyzer? horse)
    {
      var results = new string[]
      {
        await this.GetItemNameAsync(db, memo, memo.Target1, memo.Key1, horse),
        await this.GetItemNameAsync(db, memo, memo.Target2, memo.Key2, horse),
        await this.GetItemNameAsync(db, memo, memo.Target3, memo.Key3, horse),
      }.Where(t => !string.IsNullOrEmpty(t));
      return string.Join(" + ", results);
    }

    private async Task<string> GetItemNameAsync(MyContext db, MemoData memo, MemoTarget target, string key, RaceHorseAnalyzer? horse)
    {
      string SliceName(string? name, string defaultValue)
      {
        name = name?.Replace("　", string.Empty).Replace(" ", string.Empty) ?? string.Empty;
        if (string.IsNullOrEmpty(name))
        {
          return defaultValue;
        }
        return name.Length <= 6 ? name : name[..6];
      }

      switch (target)
      {
        case MemoTarget.Day:
          {
            if (short.TryParse(key.Substring(4, 2), out var month) && short.TryParse(key.Substring(6, 2), out var day))
            {
              return $"{month}/{day}";
            }
          }
          return string.Empty;
        case MemoTarget.Race:
          return "レース";
        case MemoTarget.Course:
          return memo.CourseKey.GetName() ?? "不明";
        case MemoTarget.Horse:
          return SliceName(horse?.Data.Name, "馬");
        case MemoTarget.Rider:
          return horse?.Data.RiderName ?? string.Empty;
        case MemoTarget.Trainer:
          return horse?.Data.TrainerName ?? string.Empty;
        case MemoTarget.Owner:
          return SliceName(horse?.Data.OwnerName, "馬主");
        case MemoTarget.Father:
          if (horse != null)
            return "(父)" + SliceName(await HorseBloodUtil.GetNameAsync(db, horse.Data.Key, BloodType.Father), string.Empty);
          return string.Empty;
        case MemoTarget.Mother:
          if (horse != null)
            return "(母)" + SliceName(await HorseBloodUtil.GetNameAsync(db, horse.Data.Key, BloodType.Mother), string.Empty);
          return string.Empty;
        case MemoTarget.MotherFather:
          if (horse != null)
            return "(母父)" + SliceName(await HorseBloodUtil.GetNameAsync(db, horse.Data.Key, BloodType.MotherFather), string.Empty);
          return string.Empty;
      }
      return string.Empty;
    }

    public void Dispose()
    {
      this._disposables.Dispose();
      this.TeamModel.Value?.Dispose();
      this.Config.Dispose();
      _allModels.Remove(this);
    }

    public static void OnPointLabelChangedForRaceList(MemoColor color, RaceMemoItem raceMemoItem, string label)
    {
      var config = GetRaceListMemoConfig();
      if (config?.Id == raceMemoItem.Config.Id)
      {
        foreach (var model in _allModels)
        {
          model.PointLabelChangedForRaceList?.Invoke(model, new PointLabelChangedEventArgs(color, raceMemoItem, !string.IsNullOrEmpty(label)));
        }
      }
    }

    public static void OnPointLabelOrderChangedForRaceList(ExpansionMemoConfig config, ExpansionMemoConfig target)
    {
      if (config.Target1 == MemoTarget.Race && config.Target2 == MemoTarget.Unknown && config.Target3 == MemoTarget.Unknown &&
        target.Target1 == MemoTarget.Race && target.Target2 == MemoTarget.Unknown && target.Target3 == MemoTarget.Unknown)
      {
        var configs = GetRaceHeaderConfigs().Take(2);
        if (configs.Any(c => c.Id == config.Id) && configs.Any(c => c.Id == target.Id))
        {
          foreach (var model in _allModels)
          {
            model.PointLabelOrderChangedForRaceList?.Invoke(model, EventArgs.Empty);
          }
        }
      }
    }

    private static ExpansionMemoConfig? GetRaceListMemoConfig()
    {
      var config = GetRaceHeaderConfigs().FirstOrDefault();
      return config;
    }

    private static IEnumerable<ExpansionMemoConfig> GetRaceHeaderConfigs()
    {
      var config = MemoUtil.Configs!
        .Where(m => m.Target1 == MemoTarget.Race && m.Target2 == MemoTarget.Unknown && m.Target3 == MemoTarget.Unknown && m.Type == MemoType.Race && m.PointLabelId != default)
        .Where(m => m.Style == MemoStyle.Point || m.Style == MemoStyle.MemoAndPoint)
        .Where(m => MemoUtil.MemoCaches.Any(c => c.Config.Id == m.Id && c.LabelConfig.Value != null))
        .OrderBy(m => m.Order);
      return config;
    }

    public event EventHandler<PointLabelChangedEventArgs>? PointLabelChangedForRaceList;

    public event EventHandler? PointLabelOrderChangedForRaceList;
  }

  public class PointLabelChangedEventArgs : EventArgs
  {
    public MemoColor Color { get; }

    public RaceMemoItem MemoItem { get; }

    public bool IsVisible { get; }

    public PointLabelChangedEventArgs(MemoColor color, RaceMemoItem item, bool isVisible)
    {
      this.Color = color;
      this.MemoItem = item;
      this.IsVisible = isVisible;
    }
  }

  public class RaceMemoConfig : IDisposable
  {
    private readonly CompositeDisposable _disposables = new();

    public ReactiveProperty<bool> IsRaceTab { get; } = new();

    public ReactiveProperty<string> Header { get; } = new();

    public ReactiveProperty<bool> IsFilterRace { get; } = new();

    public ReactiveProperty<bool> IsFilterDay { get; } = new();

    public ReactiveProperty<bool> IsFilterCourse { get; } = new();

    public ReactiveProperty<bool> IsFilterHorse { get; } = new();

    public ReactiveProperty<bool> IsFilterRider { get; } = new();

    public ReactiveProperty<bool> IsFilterTrainer { get; } = new();

    public ReactiveProperty<bool> IsFilterOwner { get; } = new();

    public ReactiveProperty<bool> IsFilterFather { get; } = new();

    public ReactiveProperty<bool> IsFilterMother { get; } = new();

    public ReactiveProperty<bool> IsFilterMotherFather { get; } = new();

    public ReactiveProperty<bool> IsStylePoint { get; } = new();

    public ReactiveProperty<bool> IsStyleMemo { get; } = new();

    public ReactiveProperty<bool> IsStylePointAndMemo { get; } = new(true);

    public ReactiveProperty<bool> IsUseLabel { get; } = new();

    public ReactiveProperty<PointLabelConfig?> SelectedLabel { get; } = new();

    public ReactiveProperty<string> MemoNumber { get; } = new("0");

    public ReactiveProperty<string> ErrorMessage { get; } = new();

    public ReactiveProperty<bool> IsRaceHeaderCombo { get; } = new();

    public ReactiveProperty<bool> IsHorseTeam { get; } = new();

    public RaceMemoConfig()
    {
      this.IsFilterRace
        .CombineLatest(this.IsFilterDay)
        .CombineLatest(this.IsFilterCourse)
        .CombineLatest(this.IsFilterFather)
        .CombineLatest(this.IsFilterHorse)
        .CombineLatest(this.IsFilterOwner)
        .CombineLatest(this.IsFilterRider)
        .CombineLatest(this.IsFilterTrainer)
        .CombineLatest(this.IsFilterMother)
        .CombineLatest(this.IsFilterMotherFather)
        .CombineLatest(this.IsStyleMemo)
        .CombineLatest(this.IsStylePoint)
        .CombineLatest(this.IsStylePointAndMemo)
        .CombineLatest(this.IsUseLabel)
        .CombineLatest(this.IsRaceTab)
        .CombineLatest(this.SelectedLabel.Select(_ => true))
        .Subscribe(_ =>
        {
          this.IsRaceHeaderCombo.Value = false;
          var targets = this.GetTargets(false, true);
          if (targets.Count == 1 && targets[0] == MemoTarget.Race && this.IsRaceTab.Value &&
              (this.IsStylePoint.Value || this.IsStylePointAndMemo.Value) &&
              this.IsUseLabel.Value && this.SelectedLabel.Value != null)
          {
            this.IsRaceHeaderCombo.Value = true;
          }

          this.IsHorseTeam.Value = false;
          if (targets.Count > 0 && !this.IsRaceTab.Value &&
              (this.IsStylePoint.Value || this.IsStylePointAndMemo.Value) &&
              this.IsUseLabel.Value && this.SelectedLabel.Value != null)
          {
            this.IsHorseTeam.Value = true;
          }
        })
        .AddTo(this._disposables);
    }

    public void Dispose()
    {
      this._disposables.Dispose();
    }

    private IReadOnlyList<MemoTarget> GetTargets(bool isRaceMemo, bool isAll = false)
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
        if (this.IsFilterMother.Value) list.Add(MemoTarget.Mother);
        if (this.IsFilterMotherFather.Value) list.Add(MemoTarget.MotherFather);
      }

      return list.OrderBy(i => i).Take(isAll ? 100 : 3).ToArray();
    }

    public async Task<string> GetErrorMessageAsync(MyContext db, bool isRaceMemo, ExpansionMemoConfig editingConfig)
    {
      if (string.IsNullOrEmpty(this.Header.Value))
      {
        return "名前が指定されていません";
      }

      var targets = this.GetTargets(isRaceMemo, true);
      if (!targets.Any())
      {
        return "絞り込み条件が指定されていません";
      }
      if (targets.Count() > 3)
      {
        return "絞り込み条件が３より多く指定されています";
      }
      var target1 = targets[0];
      var target2 = targets.ElementAtOrDefault(1);
      var target3 = targets.ElementAtOrDefault(2);

      if (!isRaceMemo)
      {
        var horseConfigs = new[] { MemoTarget.Horse, MemoTarget.Rider, MemoTarget.Trainer, MemoTarget.Owner, MemoTarget.Father, MemoTarget.MotherFather, MemoTarget.Mother, };
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
        if (this.SelectedLabel.Value == null)
        {
          return "ラベルが選択されていません";
        }
        if (!await db.PointLabels!.AnyAsync(l => l.Id == this.SelectedLabel.Value.Data.Id))
        {
          return "そのラベルは存在しません";
        }
      }

      return string.Empty;
    }

    public void CopyFromData(ExpansionMemoConfig config)
    {
      this.IsFilterTrainer.Value = this.IsFilterOwner.Value = this.IsFilterDay.Value = this.IsFilterRider.Value =
        this.IsFilterRace.Value = this.IsFilterCourse.Value = this.IsFilterHorse.Value = this.IsFilterFather.Value =
        this.IsFilterMother.Value = this.IsFilterMotherFather.Value = false;
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
          case MemoTarget.Mother:
            this.IsFilterMother.Value = true;
            break;
          case MemoTarget.MotherFather:
            this.IsFilterMotherFather.Value = true;
            break;
        }
      }

      this.IsUseLabel.Value = config.PointLabelId != default;
      if (this.IsUseLabel.Value)
      {
        var label = PointLabelModel.Default.Configs.FirstOrDefault(c => c.Data.Id == config.PointLabelId);
        if (label != null)
        {
          this.SelectedLabel.Value = label;
        }
        else
        {
          this.IsUseLabel.Value = false;
          this.SelectedLabel.Value = null;
        }
      }

      switch (config.Style)
      {
        case MemoStyle.MemoAndPoint:
          this.IsStylePointAndMemo.Value = true;
          break;
        case MemoStyle.Point:
          this.IsStylePoint.Value = true;
          break;
        case MemoStyle.Memo:
        default:
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

      if (this.IsUseLabel.Value && this.SelectedLabel.Value != null)
      {
        config.PointLabelId = (short)this.SelectedLabel.Value.Data.Id;
      }
      else
      {
        config.PointLabelId = 0;
      }

      return true;
    }
  }

  public class RaceMemoItem : IDisposable
  {
    private readonly CompositeDisposable _disposables = new();
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public ReactiveProperty<bool> IsStopSaving { get; } = new();

    public MemoData Data { get; }

    public ExpansionMemoConfig Config { get; private set; }

    public ReactiveProperty<PointLabelConfig?> LabelConfig { get; } = new();

    public ReactiveProperty<string> Header { get; } = new();

    public ReactiveProperty<string> Name { get; } = new();

    public ReactiveProperty<string> Memo { get; } = new();

    public ReactiveProperty<string> Point { get; } = new();

    public ReactiveProperty<string> Label { get; } = new();

    public ReactiveProperty<MemoStyle> Style { get; } = new();

    public ReactiveProperty<bool> IsPointVisible { get; } = new();

    public ReactiveProperty<bool> IsMemoVisible { get; } = new();

    public ReactiveProperty<bool> IsGroupVisible { get; } = new();

    public ReactiveProperty<bool> IsPopupVisible { get; } = new();

    public ReactiveProperty<bool> IsUseLabel { get; } = new();

    public ReactiveProperty<MemoColor> Color { get; } = new();

    public ReactiveProperty<PointLabelConfigItem?> SelectedLabel { get; } = new();

    public RaceMemoItem(MemoData data, ExpansionMemoConfig config)
    {
      this.Data = data;
      this.Config = config;
      this.Memo.Value = data.Memo;
      this.Point.Value = data.Point.ToString();
      this.UpdateConfig(config);

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
            this.UpdateLabel();
          }
        }
      }).AddTo(this._disposables);

      this.SelectedLabel.Subscribe(l =>
      {
        if (l != null)
        {
          this.Point.Value = l.Point.Value.ToString();
        }
      }).AddTo(this._disposables);

      this.LabelConfig.Subscribe(l => this.IsUseLabel.Value = l != null).AddTo(this._disposables);
    }

    public void UpdateConfig(ExpansionMemoConfig config)
    {
      this.Header.Value = config.Header;
      this.IsPointVisible.Value = config.Style.HasFlag(MemoStyle.Point);
      this.IsMemoVisible.Value = config.Style.HasFlag(MemoStyle.Memo);
      this.Style.Value = config.Style;
      this.Config = config;
    }

    public void SetLabelConfig(PointLabelConfig config)
    {
      this.LabelConfig.Value = config;
      this.UpdateLabel();
    }

    private void UpdateLabel()
    {
      if (this.LabelConfig.Value == null)
      {
        this.Label.Value = this.Data.Point.ToString();
        this.Color.Value = MemoColor.Default;
        this.SelectedLabel.Value = null;
      }
      else
      {
        this.Label.Value = this.LabelConfig.Value.GetLabel(this.Data.Point);
        this.Color.Value = this.LabelConfig.Value.GetColor(this.Data.Point);
        this.SelectedLabel.Value = this.LabelConfig.Value.GetLabelItem(this.Data.Point);

        if (this.Config.Target1 == MemoTarget.Race && this.Config.Target2 == MemoTarget.Unknown && this.Config.Target3 == MemoTarget.Unknown &&
          this.Config.Style.HasFlag(MemoStyle.Point))
        {
          RaceMemoModel.OnPointLabelChangedForRaceList(this.Color.Value, this, this.Label.Value);
        }
      }
    }

    public void UpdateLabelConfig()
    {
      this.UpdateLabel();
    }

    public void RemoveLabelConfig()
    {
      this.LabelConfig.Value = null;
      this.IsUseLabel.Value = false;
      this.UpdateLabel();
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
        logger.Error("メモの保存でエラー発生", ex);
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
        this.UpdateLabel();
      }
      finally
      {
        this.IsStopSaving.Value = false;
      }
    }

    public void Dispose()
    {
      // このアイテムはキャッシュされるので、キャッシュから情報を取り出して画面を表示する時にSubscribeが働かなくなる
      // 呼び出しは慎重に
      this._disposables.Dispose();
    }
  }

  public class RaceHorseMemoItem : IDisposable
  {
    public RaceData Race { get; }

    public RaceHorseAnalyzer RaceHorse { get; }

    public ReactiveCollection<RaceMemoItem> Memos { get; } = new();

    public RaceHorseMemoItem(RaceData race, RaceHorseAnalyzer raceHorse)
    {
      this.Race = race;
      this.RaceHorse = raceHorse;
    }

    public void Dispose()
    {
      /*
      foreach (var memo in this.Memos)
      {
        memo.Dispose();
      }
      */
    }
  }

  public class RaceHorseSingleMemoItem : IDisposable
  {
    public RaceData Race { get; }

    public RaceHorseAnalyzer RaceHorse { get; }

    public RaceMemoItem Memo { get; }

    public RaceHorseSingleMemoItem(RaceData race, RaceHorseAnalyzer raceHorse, RaceMemoItem memo)
    {
      this.Race = race;
      this.RaceHorse = raceHorse;
      this.Memo = memo;
    }

    public void Dispose()
    {
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
