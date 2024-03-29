﻿using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis.Generic;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis
{
  public class RaceHorseBloodTrendAnalysisSelectorMenu : IDisposable
  {
    private readonly CompositeDisposable _disposables = new();
    private Dictionary<BloodType, string>? _bloodCode;
    private Dictionary<BloodType, string>? _horseKey;

    public RaceData Race { get; }

    public RaceHorseData RaceHorse { get; }

    public MultipleCheckableCollection<MenuItem> MenuItems { get; } = new();

    public ReactiveProperty<MenuItem?> Father { get; } = new();
    public ReactiveProperty<MenuItem?> FatherFather { get; } = new();
    public ReactiveProperty<MenuItem?> FatherFatherFather { get; } = new();
    public ReactiveProperty<MenuItem?> FatherFatherMother { get; } = new();
    public ReactiveProperty<MenuItem?> FatherMother { get; } = new();
    public ReactiveProperty<MenuItem?> FatherMotherFather { get; } = new();
    public ReactiveProperty<MenuItem?> FatherMotherMother { get; } = new();
    public ReactiveProperty<MenuItem?> Mother { get; } = new();
    public ReactiveProperty<MenuItem?> MotherFather { get; } = new();
    public ReactiveProperty<MenuItem?> MotherFatherFather { get; } = new();
    public ReactiveProperty<MenuItem?> MotherFatherMother { get; } = new();
    public ReactiveProperty<MenuItem?> MotherMother { get; } = new();
    public ReactiveProperty<MenuItem?> MotherMotherFather { get; } = new();
    public ReactiveProperty<MenuItem?> MotherMotherMother { get; } = new();

    public MultipleCheckableCollection<GeneralBloodItem> FourthGenerations { get; } = new();
    public MultipleCheckableCollection<GeneralBloodItem> FifthGenerations { get; } = new();
    public MultipleCheckableCollection<GeneralBloodItem> SixthGenerations { get; } = new();

    public ReactiveCollection<GenerationBloodItem> GenerationInfos { get; } = new();

    public bool IsRequestedInitialization => this._bloodCode == null;

    public RaceHorseBloodTrendAnalysisSelectorMenu(RaceData race, RaceHorseData horse)
    {
      this.Race = race;
      this.RaceHorse = horse;

      async Task UpdateMenuItemAsync(IBloodCheckableItem item)
      {
        if (item == null)
        {
          return;
        }

        try
        {
          using var db = new MyContext();
          if (item.IsChecked.Value)
          {
            await CheckHorseUtil.CheckAsync(db, item.Key, HorseCheckType.CheckBlood);
          }
          else
          {
            await CheckHorseUtil.UncheckAsync(db, item.Key, HorseCheckType.CheckBlood);
          }

          this.UpdateGenerationRates();
        }
        catch (Exception ex)
        {
          // TODO
        }
      }

      this.MenuItems.ChangedItemObservable
        .Subscribe(async item =>
        {
          await UpdateMenuItemAsync(item);
        })
        .AddTo(this._disposables);
      this.FourthGenerations.ChangedItemObservable
        .Subscribe(async item =>
        {
          await UpdateMenuItemAsync(item);
        })
        .AddTo(this._disposables);
      this.FifthGenerations.ChangedItemObservable
        .Subscribe(async item =>
        {
          await UpdateMenuItemAsync(item);
        })
        .AddTo(this._disposables);
      this.SixthGenerations.ChangedItemObservable
        .Subscribe(async item =>
        {
          await UpdateMenuItemAsync(item);
        })
        .AddTo(this._disposables);
    }

    public void CopyFrom(RaceHorseBloodTrendAnalysisSelectorMenu old)
    {
      if (old.IsRequestedInitialization)
      {
        return;
      }

      if (this.IsRequestedInitialization)
      {
        this._bloodCode = old._bloodCode;
        var items = new List<MenuItem>();
        foreach (var oldItem in old.MenuItems)
        {
          var item = new MenuItem(new RaceHorseBloodTrendAnalysisSelector(this, this.Race, this.RaceHorse, oldItem.Selector.RelativeKey, oldItem.Selector.Name, oldItem.Type, oldItem.Selector.BloodKey))
          {
            Type = oldItem.Type,
            IsEnabled = oldItem.IsEnabled,
            IsChecked = { Value = oldItem.IsChecked.Value, },
          };
          item.Selector.CopyFrom(oldItem.Selector);
          items.Add(item);
        }
        this.SetMenu(items);

        ThreadUtil.InvokeOnUiThread(() =>
        {
          this.FourthGenerations.Clear();
          this.FifthGenerations.Clear();
          this.SixthGenerations.Clear();
          foreach (var item in old.FourthGenerations)
          {
            this.FourthGenerations.Add(item);
          }
          foreach (var item in old.FifthGenerations)
          {
            this.FifthGenerations.Add(item);
          }
          foreach (var item in old.SixthGenerations)
          {
            this.SixthGenerations.Add(item);
          }
        });
      }
      else
      {
        foreach (var item in old.MenuItems
          .Join(this.MenuItems, o => o.Type, n => n.Type, (o, n) => new { Old = o, New = n, }))
        {
          item.New.Selector.CopyFrom(item.Old.Selector);
        }
      }
    }

    public void Dispose()
    {
      this._disposables.Dispose();
      this.MenuItems.Dispose();
      GC.SuppressFinalize(this);
    }

    public bool IsExistsHorseName(string name)
    {
      return this.MenuItems.Any(i => i.Selector.Name == name);
    }

    public async Task InitializeBloodListAsync(MyContext db)
    {
      await CheckHorseUtil.InitializeAsync(db);

      // 血統リストを作成
      if (this._bloodCode == null)
      {
        async Task<(string?, string?)> GetParentsAsync(string key)
        {
          var myBlood = await db.Horses!.FirstOrDefaultAsync(h => h.Code == key);
          if (myBlood != null)
          {
            var father = await db.HorseBloods!.FirstOrDefaultAsync(h => h.Key == myBlood.FatherBreedingCode);
            var mother = await db.HorseBloods!.FirstOrDefaultAsync(h => h.Key == myBlood.MotherBreedingCode);
            return (father?.Code, mother?.Code);
          }

          var myHorse = await db.HorseBloods!.FirstOrDefaultAsync(h => h.Code == key);
          if (myHorse != null)
          {
            return (myHorse.FatherKey, myHorse.MotherKey);
          }

          return default;
        }

        // 繁殖登録番号
        this._bloodCode = new();

        // 血統登録番号
        this._horseKey = new();

        var born = await db.BornHorses!.FirstOrDefaultAsync(h => h.Code == this.RaceHorse.Key);
        if (born != null)
        {
          this._bloodCode[BloodType.Father] = born.FatherBreedingCode;
          this._bloodCode[BloodType.Mother] = born.MotherBreedingCode;
          this._bloodCode[BloodType.FatherFather] = born.FFBreedingCode;
          this._bloodCode[BloodType.FatherMother] = born.FMBreedingCode;
          this._bloodCode[BloodType.MotherFather] = born.MFBreedingCode;
          this._bloodCode[BloodType.MotherMother] = born.MMBreedingCode;
          this._bloodCode[BloodType.FatherFatherFather] = born.FFFBreedingCode;
          this._bloodCode[BloodType.FatherFatherMother] = born.FFMBreedingCode;
          this._bloodCode[BloodType.FatherMotherFather] = born.FMFBreedingCode;
          this._bloodCode[BloodType.FatherMotherMother] = born.FMMBreedingCode;
          this._bloodCode[BloodType.MotherFatherFather] = born.MFFBreedingCode;
          this._bloodCode[BloodType.MotherFatherMother] = born.MFMBreedingCode;
          this._bloodCode[BloodType.MotherMotherFather] = born.MMFBreedingCode;
          this._bloodCode[BloodType.MotherMotherMother] = born.MMMBreedingCode;
        }
        else
        {
          var horse = await db.Horses!.FirstOrDefaultAsync(h => h.Code == this.RaceHorse.Key);
          if (horse != null)
          {
            this._bloodCode[BloodType.Father] = horse.FatherBreedingCode;
            this._bloodCode[BloodType.Mother] = horse.MotherBreedingCode;
            this._bloodCode[BloodType.FatherFather] = horse.FFBreedingCode;
            this._bloodCode[BloodType.FatherMother] = horse.FMBreedingCode;
            this._bloodCode[BloodType.MotherFather] = horse.MFBreedingCode;
            this._bloodCode[BloodType.MotherMother] = horse.MMBreedingCode;
            this._bloodCode[BloodType.FatherFatherFather] = horse.FFFBreedingCode;
            this._bloodCode[BloodType.FatherFatherMother] = horse.FFMBreedingCode;
            this._bloodCode[BloodType.FatherMotherFather] = horse.FMFBreedingCode;
            this._bloodCode[BloodType.FatherMotherMother] = horse.FMMBreedingCode;
            this._bloodCode[BloodType.MotherFatherFather] = horse.MFFBreedingCode;
            this._bloodCode[BloodType.MotherFatherMother] = horse.MFMBreedingCode;
            this._bloodCode[BloodType.MotherMotherFather] = horse.MMFBreedingCode;
            this._bloodCode[BloodType.MotherMotherMother] = horse.MMMBreedingCode;
          }
          else
          {
            var blood = await db.HorseBloods!.FirstOrDefaultAsync(h => h.Code == this.RaceHorse.Key);
            if (blood != null)
            {
              this._bloodCode[BloodType.Father] = blood.FatherKey;
              this._bloodCode[BloodType.Mother] = blood.MotherKey;
              if (blood.FatherKey != null)
              {
                var (ff, fm) = await GetParentsAsync(blood.FatherKey);
                if (ff != null) this._bloodCode[BloodType.FatherFather] = ff;
                if (fm != null) this._bloodCode[BloodType.FatherMother] = fm;
                if (ff != null)
                {
                  var (fff, ffm) = await GetParentsAsync(ff);
                  if (fff != null) this._bloodCode[BloodType.FatherFatherFather] = fff;
                  if (ffm != null) this._bloodCode[BloodType.FatherFatherMother] = ffm;
                }
                if (fm != null)
                {
                  var (fmf, fmm) = await GetParentsAsync(fm);
                  if (fmf != null) this._bloodCode[BloodType.FatherMotherFather] = fmf;
                  if (fmm != null) this._bloodCode[BloodType.FatherMotherMother] = fmm;
                }
              }
              if (blood.MotherKey != null)
              {
                var (mf, mm) = await GetParentsAsync(blood.MotherKey);
                if (mf != null) this._bloodCode[BloodType.MotherFather] = mf;
                if (mm != null) this._bloodCode[BloodType.MotherMother] = mm;
                if (mf != null)
                {
                  var (mff, mfm) = await GetParentsAsync(mf);
                  if (mff != null) this._bloodCode[BloodType.MotherFatherFather] = mff;
                  if (mfm != null) this._bloodCode[BloodType.MotherFatherMother] = mfm;
                }
                if (mm != null)
                {
                  var (mmf, mmm) = await GetParentsAsync(mm);
                  if (mmf != null) this._bloodCode[BloodType.MotherMotherFather] = mmf;
                  if (mmm != null) this._bloodCode[BloodType.MotherMotherMother] = mmm;
                }
              }
            }
          }
        }

        var bloodKeys = this._bloodCode.Select(p => p.Value).ToArray();

        // 血統番号と繁殖番号が結びついてるのはこのテーブルだけ
        var horses = await db.HorseBloods!.Where(h => bloodKeys.Contains(h.Key)).Select(h => new { h.Code, h.Key, h.Name }).ToArrayAsync();
        var items = new List<MenuItem>();
        foreach (var d in this._bloodCode
          .Join(horses, b => b.Value, h => h.Key, (b, h) => new { Type = b.Key, Key = h.Code, h.Name, BloodKey = h.Key, })
          .Where(h => !string.IsNullOrEmpty(h.Name)))
        {
          this._horseKey.Add(d.Type, d.Key);
          items.Add(new MenuItem(new RaceHorseBloodTrendAnalysisSelector(this, this.Race, this.RaceHorse, d.Key, d.Name, d.Type, d.BloodKey))
          {
            Type = d.Type,
            IsEnabled = !d.Key.All(k => k == '0'),
          });
        }

        this.SetMenu(items);
        _ = this.UpdateGenerationsAsync();
      }
    }

    private void SetMenu(IEnumerable<MenuItem> items)
    {
      ThreadUtil.InvokeOnUiThread(() =>
      {
        this.MenuItems.AddRangeOnScheduler(items.OrderBy(i => i.Type));
      });

      foreach (var item in items)
      {
        switch (item.Type)
        {
          case BloodType.Father:
            this.Father.Value = item;
            break;
          case BloodType.FatherFather:
            this.FatherFather.Value = item;
            break;
          case BloodType.FatherFatherFather:
            this.FatherFatherFather.Value = item;
            break;
          case BloodType.FatherFatherMother:
            this.FatherFatherMother.Value = item;
            break;
          case BloodType.FatherMother:
            this.FatherMother.Value = item;
            break;
          case BloodType.FatherMotherFather:
            this.FatherMotherFather.Value = item;
            break;
          case BloodType.FatherMotherMother:
            this.FatherMotherMother.Value = item;
            break;
          case BloodType.Mother:
            this.Mother.Value = item;
            break;
          case BloodType.MotherFather:
            this.MotherFather.Value = item;
            break;
          case BloodType.MotherFatherFather:
            this.MotherFatherFather.Value = item;
            break;
          case BloodType.MotherFatherMother:
            this.MotherFatherMother.Value = item;
            break;
          case BloodType.MotherMother:
            this.MotherMother.Value = item;
            break;
          case BloodType.MotherMotherFather:
            this.MotherMotherFather.Value = item;
            break;
          case BloodType.MotherMotherMother:
            this.MotherMotherMother.Value = item;
            break;
        }
      }
    }

    private async Task UpdateGenerationsAsync()
    {
      using var db = new MyContext();
      var arr4 = new GeneralBloodItem[16];
      var arr5 = new GeneralBloodItem[32];
      var arr6 = new GeneralBloodItem[64];

      async Task<GeneralBloodItem> GenerateItem(string targetKey, BloodType type, bool isMale)
      {
        return new GeneralBloodItem
        {
          Key = await HorseBloodUtil.GetBloodCodeFromCodeAsync(db!, targetKey, type),
          Name = await HorseBloodUtil.GetNameFromCodeAsync(db!, targetKey, type),
          IsMale = isMale,
        };
      }

      async Task SetGenerationsAsync(string targetKey, int index)
      {
        arr4![index * 2] = await GenerateItem(targetKey, BloodType.Father, true);
        arr4![index * 2 + 1] = await GenerateItem(targetKey, BloodType.Mother, false);
        arr5![index * 4] = await GenerateItem(targetKey, BloodType.FatherFather, true);
        arr5![index * 4 + 1] = await GenerateItem(targetKey, BloodType.FatherMother, false);
        arr5![index * 4 + 2] = await GenerateItem(targetKey, BloodType.MotherFather, true);
        arr5![index * 4 + 3] = await GenerateItem(targetKey, BloodType.MotherMother, false);
        // arr6![index * 8] = await GenerateItem(targetKey, BloodType.FatherFatherFather, true);
        // arr6![index * 8 + 1] = await GenerateItem(targetKey, BloodType.FatherFatherMother, false);
        // arr6![index * 8 + 2] = await GenerateItem(targetKey, BloodType.FatherMotherFather, true);
        // arr6![index * 8 + 3] = await GenerateItem(targetKey, BloodType.FatherMotherMother, false);
        // arr6![index * 8 + 4] = await GenerateItem(targetKey, BloodType.MotherFatherFather, true);
        // arr6![index * 8 + 5] = await GenerateItem(targetKey, BloodType.MotherFatherMother, false);
        // arr6![index * 8 + 6] = await GenerateItem(targetKey, BloodType.MotherMotherFather, true);
        // arr6![index * 8 + 7] = await GenerateItem(targetKey, BloodType.MotherMotherMother, false);
      }

      async Task SetTypeAsync(int index, BloodType type)
      {
        var item = this.MenuItems.FirstOrDefault(i => i.Type == type);
        if (!string.IsNullOrEmpty(item?.Selector.Name))
        {
          await SetGenerationsAsync(item.Selector.BloodKey, index);
        }
        else
        {
          arr4![index * 2] = new GeneralBloodItem();
          arr4![index * 2 + 1] = new GeneralBloodItem();
          arr5![index * 4] = new GeneralBloodItem();
          arr5![index * 4 + 1] = new GeneralBloodItem();
          arr5![index * 4 + 2] = new GeneralBloodItem();
          arr5![index * 4 + 3] = new GeneralBloodItem();
        }
      }

      await SetTypeAsync(0, BloodType.FatherFatherFather);
      await SetTypeAsync(1, BloodType.FatherFatherMother);
      await SetTypeAsync(2, BloodType.FatherMotherFather);
      await SetTypeAsync(3, BloodType.FatherMotherMother);
      await SetTypeAsync(4, BloodType.MotherFatherFather);
      await SetTypeAsync(5, BloodType.MotherFatherMother);
      await SetTypeAsync(6, BloodType.MotherMotherFather);
      await SetTypeAsync(7, BloodType.MotherMotherMother);

      ThreadUtil.InvokeOnUiThread(() =>
      {
        this.FourthGenerations.Clear();
        this.FifthGenerations.Clear();
        this.SixthGenerations.Clear();
        foreach (var item in arr4)
        {
          this.FourthGenerations.Add(item);
        }
        foreach (var item in arr5)
        {
          this.FifthGenerations.Add(item);
        }
        foreach (var item in arr6)
        {
          this.SixthGenerations.Add(item);
        }

        this.UpdateGenerationRates();
      });
    }

    private bool _isCheckingGenerationRates;
    public void UpdateGenerationRates()
    {
      if (this._isCheckingGenerationRates)
      {
        return;
      }
      this._isCheckingGenerationRates = true;

      var g1 = this.MenuItems.Where(i => i.Type == BloodType.Father || i.Type == BloodType.Mother).Cast<IBloodCheckableItem>();
      var g2 = this.MenuItems.Where(i => i.Type == BloodType.FatherFather || i.Type == BloodType.FatherMother ||
                                         i.Type == BloodType.MotherFather || i.Type == BloodType.MotherMother).Cast<IBloodCheckableItem>();
      var g3 = this.MenuItems.Where(i => i.Type == BloodType.FatherFatherFather || i.Type == BloodType.FatherFatherMother ||
                                         i.Type == BloodType.FatherMotherFather || i.Type == BloodType.FatherMotherMother ||
                                         i.Type == BloodType.MotherFatherFather || i.Type == BloodType.MotherFatherMother ||
                                         i.Type == BloodType.MotherMotherFather || i.Type == BloodType.MotherMotherMother).Cast<IBloodCheckableItem>();
      var g4 = this.FourthGenerations.Cast<IBloodCheckableItem>();
      var g5 = this.FifthGenerations.Cast<IBloodCheckableItem>();

      var bloods = new Dictionary<string, GenerationBloodItem>();

      GenerationBloodItem GetItem(string name)
      {
        if (bloods!.TryGetValue(name, out var item))
        {
          return item;
        }

        item = new GenerationBloodItem
        {
          Name = name,
        };
        bloods![name] = item;
        return item;
      }

      void ProcessGeneration(IEnumerable<IBloodCheckableItem> names, double point)
      {
        foreach (var name in names)
        {
          if (!CheckHorseUtil.IsChecked(name.Key, HorseCheckType.CheckBlood))
          {
            if (name.IsChecked.Value)
            {
              name.IsChecked.Value = false;
            }
            continue;
          }

          var item = GetItem(name.Name);
          item.Rate += point;
          if (!name.IsChecked.Value)
          {
            name.IsChecked.Value = true;
          }
        }
      }

      ProcessGeneration(g1, 1.0 / 2);
      ProcessGeneration(g2, 1.0 / 4);
      ProcessGeneration(g3, 1.0 / 8);
      ProcessGeneration(g4, 1.0 / 16);
      ProcessGeneration(g5, 1.0 / 32);

      ThreadUtil.InvokeOnUiThread(() =>
      {
        this.GenerationInfos.Clear();
        foreach (var b in bloods.OrderByDescending(bb => bb.Value.Rate))
        {
          this.GenerationInfos.Add(b.Value);
        }

        this._isCheckingGenerationRates = false;
      });
    }

    public RaceHorseBloodTrendAnalysisSelector? GetSelector(BloodType type)
    {
      return this.MenuItems.FirstOrDefault(i => i.Type == type)?.Selector;
    }

    public RaceHorseBloodTrendAnalysisSelector? GetSelector(string scriptType)
    {
      var type = HorseBloodUtil.ToBloodType(scriptType);
      return this.GetSelector(type);
    }

    public async Task<RaceHorseBloodTrendAnalysisSelector> GetSelectorForceAsync(string scriptType)
    {
      using var db = new MyContext();
      await this.InitializeBloodListAsync(db);
      return this.GetSelector(scriptType) ?? new RaceHorseBloodTrendAnalysisSelector(this, this.Race, this.RaceHorse, string.Empty, string.Empty, BloodType.Unknown, string.Empty);
    }

    public interface IBloodCheckableItem : IMultipleCheckableItem
    {
      string Name { get; }

      string Key { get; }

      bool IsMale { get; }
    }

    public class MenuItem : IBloodCheckableItem
    {
      public ReactiveProperty<bool> IsChecked { get; } = new();

      public string? GroupName => null;

      public BloodType Type { get; init; }

      public bool IsEnabled { get; set; }

      public string Name => this.Selector.Name;

      public string Key => this.Selector.BloodKey;

      public bool IsMale => this.Type == BloodType.Father || this.Type == BloodType.FatherFather || this.Type == BloodType.FatherFatherFather ||
        this.Type == BloodType.FatherMotherFather || this.Type == BloodType.MotherFather || this.Type == BloodType.MotherFatherFather ||
        this.Type == BloodType.MotherMotherFather;

      public RaceHorseBloodTrendAnalysisSelector Selector { get; }

      public MenuItem(RaceHorseBloodTrendAnalysisSelector selector)
      {
        this.Selector = selector;
      }
    }

    public class GeneralBloodItem : IBloodCheckableItem
    {
      public ReactiveProperty<bool> IsChecked { get; } = new();

      public string? GroupName => null;

      public string Name { get; init; } = string.Empty;

      public string Key { get; init; } = string.Empty;

      public bool IsMale { get; init; }

      public bool IsDisabled => string.IsNullOrEmpty(this.Key);
    }

    public class GenerationBloodItem
    {
      public string Name { get; init; } = string.Empty;

      public double Rate { get; set; }
    }
  }

  public class RaceHorseBloodTrendAnalysisSelector : TrendAnalysisSelector<RaceHorseBloodTrendAnalysisSelector.Key, RaceHorseBloodTrendAnalyzer>
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    public enum Key
    {
      [IgnoreKey]
      [ScriptParameterKey("self")]
      BloodHorseSelf,      // 血統馬自身を指定するために内部でAnalyzerを管理する便宜上のキー

      [Label("コース")]
      [ScriptParameterKey("course")]
      SameCourse,

      [Label("地面")]
      [ScriptParameterKey("ground")]
      SameGround,

      [Label("馬場状態")]
      [ScriptParameterKey("condition")]
      SameCondition,

      [Label("天気")]
      [ScriptParameterKey("weather")]
      SameWeather,

      [Label("条件")]
      [ScriptParameterKey("subject")]
      SameSubject,

      [Label("格")]
      [ScriptParameterKey("grade")]
      SameGrade,

      [Label("季節")]
      [ScriptParameterKey("season")]
      [IgnoreKey]
      SameSeason,

      [Label("距離")]
      [ScriptParameterKey("distance")]
      NearDistance,

      [Label("向き")]
      [ScriptParameterKey("direction")]
      SameDirection,

      [Label("複勝")]
      [ScriptParameterKey("placebits")]
      [GroupName("ResultOrder")]
      PlaceBets,

      [Label("着外")]
      [ScriptParameterKey("losed")]
      [GroupName("ResultOrder")]
      Losed,

      [Label("間隔")]
      [ScriptParameterKey("interval")]
      NearInterval,

      [Label("年齢")]
      [ScriptParameterKey("age")]
      SameAge,

      [Label("運営")]
      [ScriptParameterKey("region")]
      SameRegion,

      [Label("重賞")]
      [ScriptParameterKey("grades")]
      Grades,

      [Label("枠")]
      [ScriptParameterKey("frame")]
      Frame,

      [Label("オッズ")]
      [ScriptParameterKey("odds")]
      [NotCacheKeyUntilRace]
      Odds,
    }

    private IReadOnlyList<RaceHorseAnalyzer>? _allRaces;
    private readonly CompositeDisposable _disposables = new();

    public override string Name { get; }

    public string BloodKey { get; }

    public string RelativeKey { get; }

    public override RaceData Race { get; }

    public RaceHorseData RaceHorse { get; }

    public RaceHorseBloodTrendAnalysisSelectorMenu Menu { get; }

    public BloodType Type { get; }

    public ReactiveProperty<bool> IsSameChildren { get; } = new(true);

    protected override bool IsAutoLoad => false;

    public RaceHorseBloodTrendAnalysisSelector(RaceHorseBloodTrendAnalysisSelectorMenu menu, RaceData race, RaceHorseData horse, string relativeKey, string relativeName, BloodType type, string bloodKey) : base(typeof(Key))
    {
      this.Menu = menu;
      this.Race = race;
      this.RaceHorse = horse;
      this.RelativeKey = relativeKey;
      this.Name = relativeName;
      this.Type = type;
      this.BloodKey = bloodKey;

      this.IsSameChildren.Subscribe(isChecked =>
      {
        var item = this.IgnoreKeys.FirstOrDefault(k => k.Key == Key.BloodHorseSelf);
        if (item != null)
        {
          item.IsChecked.Value = !isChecked;
        }
      }).AddTo(this._disposables);

      base.OnFinishedInitialization();
    }

    protected override RaceHorseBloodTrendAnalyzer GenerateAnalyzer(int sizeMax)
    {
      return new RaceHorseBloodTrendAnalyzer(sizeMax, this.Race, this.RaceHorse);
    }

    protected override async Task InitializeAnalyzerAsync(MyContext db, IEnumerable<Key> keys, RaceHorseBloodTrendAnalyzer analyzer, int sizeMax, int offset, bool isLoadSameHorses)
    {
      if (keys.Contains(Key.BloodHorseSelf))
      {
        await this.InitializeBloodAnalyzerAsync(db, keys, analyzer, sizeMax, offset, isLoadSameHorses);
        return;
      }

      /*
      var query = db.Races!
        .Where(r => r.StartTime < this.Race.StartTime && r.DataStatus != RaceDataStatus.Canceled && r.TrackType == this.Race.TrackType)
        .Join(db.RaceHorses!, r => r.Key, rh => rh.RaceKey, (r, rh) => new { Race = r, RaceHorse = rh, })
        .Where(d => d.RaceHorse.RiderCode == this.RaceHorse.RiderCode);
      */
      var query = db.RaceHorses!
        .Join(db.Races!.Where(r => r.StartTime < this.Race.StartTime && r.DataStatus != RaceDataStatus.Canceled && r.TrackType == this.Race.TrackType),
          rh => rh.RaceKey, r => r.Key, (rh, r) => new { RaceHorse = rh, Race = r, });

      IQueryable<HorseData> q1 = db.Horses!.Where(h => h.Id == 0);
      IQueryable<BornHorseData> q2 = db.BornHorses!.Where(h => h.Id == 0);
      
      if (this.Type == BloodType.Father)
      {
        q1 = db.Horses!.Where(h => h.FatherBreedingCode == this.BloodKey);
        q2 = db.BornHorses!.Where(h => h.FatherBreedingCode == this.BloodKey);
      }
      else if (this.Type == BloodType.FatherFather)
      {
        q1 = db.Horses!.Where(h => h.FFBreedingCode == this.BloodKey);
        q2 = db.BornHorses!.Where(h => h.FFBreedingCode == this.BloodKey);
      }
      else if (this.Type == BloodType.FatherFatherFather)
      {
        q1 = db.Horses!.Where(h => h.FFFBreedingCode == this.BloodKey);
        q2 = db.BornHorses!.Where(h => h.FFFBreedingCode == this.BloodKey);
      }
      else if (this.Type == BloodType.FatherFatherMother)
      {
        q1 = db.Horses!.Where(h => h.FFMBreedingCode == this.BloodKey);
        q2 = db.BornHorses!.Where(h => h.FFMBreedingCode == this.BloodKey);
      }
      else if (this.Type == BloodType.FatherMother)
      {
        q1 = db.Horses!.Where(h => h.FMBreedingCode == this.BloodKey);
        q2 = db.BornHorses!.Where(h => h.FMBreedingCode == this.BloodKey);
      }
      else if (this.Type == BloodType.FatherMotherFather)
      {
        q1 = db.Horses!.Where(h => h.FMFBreedingCode == this.BloodKey);
        q2 = db.BornHorses!.Where(h => h.FMFBreedingCode == this.BloodKey);
      }
      else if (this.Type == BloodType.FatherMotherMother)
      {
        q1 = db.Horses!.Where(h => h.FMMBreedingCode == this.BloodKey);
        q2 = db.BornHorses!.Where(h => h.FMMBreedingCode == this.BloodKey);
      }
      else if (this.Type == BloodType.Mother)
      {
        q1 = db.Horses!.Where(h => h.MotherBreedingCode == this.BloodKey);
        q2 = db.BornHorses!.Where(h => h.MotherBreedingCode == this.BloodKey);
      }
      else if (this.Type == BloodType.MotherFather)
      {
        q1 = db.Horses!.Where(h => h.MFBreedingCode == this.BloodKey);
        q2 = db.BornHorses!.Where(h => h.MFBreedingCode == this.BloodKey);
      }
      else if (this.Type == BloodType.MotherFatherFather)
      {
        q1 = db.Horses!.Where(h => h.MFFBreedingCode == this.BloodKey);
        q2 = db.BornHorses!.Where(h => h.MFFBreedingCode == this.BloodKey);
      }
      else if (this.Type == BloodType.MotherFatherMother)
      {
        q1 = db.Horses!.Where(h => h.MFMBreedingCode == this.BloodKey);
        q2 = db.BornHorses!.Where(h => h.MFMBreedingCode == this.BloodKey);
      }
      else if (this.Type == BloodType.MotherMother)
      {
        q1 = db.Horses!.Where(h => h.MMBreedingCode == this.BloodKey);
        q2 = db.BornHorses!.Where(h => h.MMBreedingCode == this.BloodKey);
      }
      else if (this.Type == BloodType.MotherMotherFather)
      {
        q1 = db.Horses!.Where(h => h.MMFBreedingCode == this.BloodKey);
        q2 = db.BornHorses!.Where(h => h.MMFBreedingCode == this.BloodKey);
      }
      else if (this.Type == BloodType.MotherMotherMother)
      {
        q1 = db.Horses!.Where(h => h.MMMBreedingCode == this.BloodKey);
        q2 = db.BornHorses!.Where(h => h.MMMBreedingCode == this.BloodKey);
      }
      else
      {

      }

      if (keys.Contains(Key.SameCourse))
      {
        query = query.Where(r => r.Race.Course == this.Race.Course);
      }
      if (keys.Contains(Key.SameRegion))
      {
        if (this.Race.Course <= RaceCourse.CentralMaxValue)
        {
          query = query.Where(r => r.Race.Course <= RaceCourse.CentralMaxValue);
        }
        else
        {
          query = query.Where(r => r.Race.Course >= RaceCourse.LocalMinValue);
        }
      }
      if (keys.Contains(Key.SameGround))
      {
        query = query.Where(r => r.Race.TrackGround == this.Race.TrackGround);
      }
      if (keys.Contains(Key.NearDistance))
      {
        var diff = this.Race.Course <= RaceCourse.CentralMaxValue ?
          ApplicationConfiguration.Current.Value.NearDistanceDiffCentral :
          ApplicationConfiguration.Current.Value.NearDistanceDiffLocal;
        query = query.Where(r => r.Race.Distance >= this.Race.Distance - diff && r.Race.Distance <= this.Race.Distance + diff);
      }
      if (keys.Contains(Key.SameDirection))
      {
        query = query.Where(r => r.Race.TrackCornerDirection == this.Race.TrackCornerDirection);
      }
      if (keys.Contains(Key.SameSeason))
      {
        query = query.Where(r => r.Race.StartTime.Month % 12 / 3 == this.Race.StartTime.Month % 12 / 3);
      }
      if (keys.Contains(Key.SameCondition))
      {
        query = query.Where(r => r.Race.TrackCondition == this.Race.TrackCondition);
      }
      if (keys.Contains(Key.SameSubject))
      {
        query = query.Where(r => r.Race.SubjectName == this.Race.SubjectName &&
                                 r.Race.SubjectAge2 == this.Race.SubjectAge2 &&
                                 r.Race.SubjectAge3 == this.Race.SubjectAge3 &&
                                 r.Race.SubjectAge4 == this.Race.SubjectAge4 &&
                                 r.Race.SubjectAge5 == this.Race.SubjectAge5 &&
                                 r.Race.SubjectAgeYounger == this.Race.SubjectAgeYounger);
      }
      if (keys.Contains(Key.SameGrade))
      {
        query = query.Where(r => r.Race.Grade == this.Race.Grade);
      }
      if (keys.Contains(Key.SameWeather))
      {
        query = query.Where(r => r.Race.TrackWeather == this.Race.TrackWeather);
      }
      if (keys.Contains(Key.PlaceBets))
      {
        query = query.Where(r => r.RaceHorse.ResultOrder >= 1 && r.RaceHorse.ResultOrder <= 3);
      }
      if (keys.Contains(Key.Losed))
      {
        query = query.Where(r => r.RaceHorse.ResultOrder > 5);
      }
      if (keys.Contains(Key.SameAge))
      {
        query = query.Where(r => r.RaceHorse.Age == this.RaceHorse.Age);
      }
      if (keys.Contains(Key.Grades))
      {
        query = query.Where(r => r.Race.Grade == RaceGrade.Grade1 || r.Race.Grade == RaceGrade.Grade2 || r.Race.Grade == RaceGrade.Grade3 ||
                                 r.Race.Grade == RaceGrade.LocalGrade1 || r.Race.Grade == RaceGrade.LocalGrade2 || r.Race.Grade == RaceGrade.LocalGrade3);
      }
      if (keys.Contains(Key.NearInterval))
      {
        var (min, max) = AnalysisUtil.GetIntervalRange(this.RaceHorse.PreviousRaceDays);
        query = query.Where(r => r.RaceHorse.PreviousRaceDays >= min && r.RaceHorse.PreviousRaceDays <= max);
      }
      if (keys.Contains(Key.Frame))
      {
        query = query.Where(r => r.RaceHorse.FrameNumber == this.RaceHorse.FrameNumber);
      }
      if (keys.Contains(Key.Odds))
      {
        var (min, max) = AnalysisUtil.GetOddsRange(this.RaceHorse.Odds);
        query = query.Where(r => r.RaceHorse.Odds >= min && r.RaceHorse.Odds < max);
      }

      //var r0 = query
      //  .Join(db.HorseBloods!, h => h.RaceHorse.Key, h => h.Code, (d, h) => new { d.Race, d.RaceHorse, BloodKey = h.Code, });
      //var r1 = r0.Join(q1, d => d.BloodKey, q => q.Code, (d, q) => new { d.Race, d.RaceHorse, });
      //var r2 = r0.Join(q2, d => d.BloodKey, q => q.Code, (d, q) => new { d.Race, d.RaceHorse, });
      var r3 = query.Join(q2, d => d.RaceHorse.Key, q => q.Code, (d, q) => new { d.Race, d.RaceHorse, });

      try
      {
        var races = await r3 //r2.Concat(r1)
          .OrderByDescending(r => r.Race.StartTime)
          .Skip(offset)
          .Take(sizeMax)
          .ToArrayAsync();
        races = races.DistinctBy(r => r.RaceHorse.Key + r.RaceHorse.RaceKey).ToArray();
        var raceHorses = Array.Empty<RaceHorseData>();
        if (isLoadSameHorses)
        {
          var raceKeys = races.Select(r => r.Race.Key).ToArray();
          raceHorses = await db.RaceHorses!
            .Where(rh => rh.ResultOrder >= 1 && rh.ResultOrder <= 5 && raceKeys.Contains(rh.RaceKey))
            .ToArrayAsync();
        }

        var list = new List<RaceHorseAnalyzer>();
        foreach (var race in races)
        {
          list.Add(
            new RaceHorseAnalyzer(
              race.Race,
              race.RaceHorse,
              raceHorses.Where(rh => rh.RaceKey == race.Race.Key).ToArray(),
              await AnalysisUtil.GetRaceStandardTimeAsync(db, race.Race)));
        }
        analyzer.SetSource(list);
      }
      catch (Exception ex)
      {
        logger.Error($"血統馬 {this.BloodKey}/{this.Type} のレース取得でエラー", ex);
        this.IsError.Value = true;

        throw new Exception("血統馬の解析中にエラーが発生しました", ex);
      }
    }

    private async Task InitializeBloodAnalyzerAsync(MyContext db, IEnumerable<Key> keys, RaceHorseBloodTrendAnalyzer analyzer, int count, int offset, bool isLoadSameHorses)
    {
      // WARNING: 全体の総数が多くないと予想されるのでここでDBからすべて取得し、配分している
      //          間違ってもこれをこのまま他のSelectorクラスにコピペしないように
      if (this._allRaces == null)
      {
        var allRaces = await db.RaceHorses!
          .Where(h => h.Key == this.RelativeKey)
          .Join(db.Races!, rh => rh.RaceKey, r => r.Key, (rh, r) => new { RaceHorse = rh, Race = r, })
          .Where(d => d.Race.StartTime < this.Race.StartTime)
          .OrderByDescending(d => d.Race.StartTime)
          .ToArrayAsync();
        var list = new List<RaceHorseAnalyzer>();
        foreach (var race in allRaces)
        {
          var standardTime = await AnalysisUtil.GetRaceStandardTimeAsync(db, race.Race);
          list.Add(new RaceHorseAnalyzer(race.Race, race.RaceHorse, standardTime));
        }
        this._allRaces = list;
      }

      var query = this._allRaces.Where(r => r.Data.Key == this.RelativeKey);

      if (keys.Contains(Key.SameCourse))
      {
        query = query.Where(r => r.Race.Course == this.Race.Course);
      }
      if (keys.Contains(Key.SameGround))
      {
        query = query.Where(r => r.Race.TrackGround == this.Race.TrackGround);
      }
      if (keys.Contains(Key.NearDistance))
      {
        var diff = this.Race.Course <= RaceCourse.CentralMaxValue ?
          ApplicationConfiguration.Current.Value.NearDistanceDiffCentralInHorseGrade :
          ApplicationConfiguration.Current.Value.NearDistanceDiffLocalInHorseGrade;
        query = query.Where(r => r.Race.Distance >= this.Race.Distance - diff && r.Race.Distance <= this.Race.Distance + diff);
      }
      if (keys.Contains(Key.SameDirection))
      {
        query = query.Where(r => r.Race.TrackCornerDirection == this.Race.TrackCornerDirection);
      }
      if (keys.Contains(Key.SameSeason))
      {
        query = query.Where(r => r.Race.StartTime.Month % 12 / 3 == this.Race.StartTime.Month % 12 / 3);
      }
      if (keys.Contains(Key.SameCondition))
      {
        query = query.Where(r => r.Race.TrackCondition == this.Race.TrackCondition);
      }
      if (keys.Contains(Key.SameSubject))
      {
        query = query.Where(r => r.Race.SubjectName == this.Race.SubjectName &&
                                 r.Race.SubjectAge2 == this.Race.SubjectAge2 &&
                                 r.Race.SubjectAge3 == this.Race.SubjectAge3 &&
                                 r.Race.SubjectAge4 == this.Race.SubjectAge4 &&
                                 r.Race.SubjectAge5 == this.Race.SubjectAge5 &&
                                 r.Race.SubjectAgeYounger == this.Race.SubjectAgeYounger);
      }
      if (keys.Contains(Key.SameGrade))
      {
        query = query.Where(r => r.Race.Grade == this.Race.Grade);
      }
      if (keys.Contains(Key.SameWeather))
      {
        query = query.Where(r => r.Race.TrackWeather == this.Race.TrackWeather);
      }
      if (keys.Contains(Key.PlaceBets))
      {
        query = query.Where(r => r.Data.ResultOrder >= 1 && r.Data.ResultOrder <= 3);
      }
      if (keys.Contains(Key.Losed))
      {
        query = query.Where(r => r.Data.ResultOrder > 5);
      }
      if (keys.Contains(Key.NearInterval))
      {
        var (min, max) = AnalysisUtil.GetIntervalRange(this.RaceHorse.PreviousRaceDays);
        query = query.Where(r => r.Data.PreviousRaceDays >= min && r.Data.PreviousRaceDays <= max);
      }

      analyzer.SetSource(query);
    }

    public override void Dispose()
    {
      base.Dispose();
      this._disposables.Dispose();
    }
  }
}
