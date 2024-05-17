using KmyKeiba.Common;
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
  public class RaceHorseBloodModel : IDisposable
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

    public ReactiveCollection<GenerationBloodItem> GenerationInfos { get; } = new();

    public bool IsRequestedInitialization => this._bloodCode == null;

    public RaceHorseBloodModel(RaceData race, RaceHorseData horse)
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
            await CheckHorseUtil.CheckAsync(db, item.BloodKey, HorseCheckType.CheckBlood);
          }
          else
          {
            await CheckHorseUtil.UncheckAsync(db, item.BloodKey, HorseCheckType.CheckBlood);
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
    }

    public void CopyFrom(RaceHorseBloodModel old)
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
          var item = new MenuItem(oldItem.Name, oldItem.BloodKey)
          {
            Type = oldItem.Type,
            IsEnabled = oldItem.IsEnabled,
            IsChecked = { Value = oldItem.IsChecked.Value, },
          };
          items.Add(item);
        }
        this.SetMenu(items);

        ThreadUtil.InvokeOnUiThread(() =>
        {
          this.FourthGenerations.Clear();
          this.FifthGenerations.Clear();
          foreach (var item in old.FourthGenerations)
          {
            this.FourthGenerations.Add(item);
          }
          foreach (var item in old.FifthGenerations)
          {
            this.FifthGenerations.Add(item);
          }
        });
      }
      else
      {
      }
    }

    public void Dispose()
    {
      this._disposables.Dispose();
      this.MenuItems.Dispose();
      this.FourthGenerations.Dispose();
      this.FifthGenerations.Dispose();
      GC.SuppressFinalize(this);
    }

    public bool IsExistsHorseName(string name)
    {
      return this.MenuItems.Any(i => i.Name == name);
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
          items.Add(new MenuItem(d.Name, d.BloodKey)
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

      async Task<GeneralBloodItem> GenerateItem(string targetKey, BloodType type, bool isMale)
      {
        var name = await HorseBloodUtil.GetNameFromCodeAsync(db!, targetKey, type);
        if (string.IsNullOrEmpty(name)) return GeneralBloodItem.Empty;

        return new GeneralBloodItem
        {
          BloodKey = await HorseBloodUtil.GetBloodCodeFromCodeAsync(db!, targetKey, type),
          Name = name,
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
      }

      async Task SetTypeAsync(int index, BloodType type)
      {
        var item = this.MenuItems.FirstOrDefault(i => i.Type == type);
        if (!string.IsNullOrEmpty(item?.Name))
        {
          await SetGenerationsAsync(item.BloodKey, index);
        }
        else
        {
          arr4![index * 2] = GeneralBloodItem.Empty;
          arr4![index * 2 + 1] = GeneralBloodItem.Empty;
          arr5![index * 4] = GeneralBloodItem.Empty;
          arr5![index * 4 + 1] = GeneralBloodItem.Empty;
          arr5![index * 4 + 2] = GeneralBloodItem.Empty;
          arr5![index * 4 + 3] = GeneralBloodItem.Empty;
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
        foreach (var item in arr4)
        {
          this.FourthGenerations.Add(item);
        }
        foreach (var item in arr5)
        {
          this.FifthGenerations.Add(item);
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
          if (!CheckHorseUtil.IsChecked(name.BloodKey, HorseCheckType.CheckBlood))
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

    public interface IBloodCheckableItem : IMultipleCheckableItem
    {
      bool IsEmpty { get; }

      string Name { get; }

      string BloodKey { get; }

      bool IsMale { get; }
    }

    public class MenuItem : IBloodCheckableItem
    {
      public ReactiveProperty<bool> IsChecked { get; } = new();

      public bool IsEmpty { get; }

      public string? GroupName => null;

      public BloodType Type { get; init; }

      public bool IsEnabled { get; set; }

      public string Name { get; init; }

      public string BloodKey { get; init; }

      public bool IsMale => this.Type == BloodType.Father || this.Type == BloodType.FatherFather || this.Type == BloodType.FatherFatherFather ||
        this.Type == BloodType.FatherMotherFather || this.Type == BloodType.MotherFather || this.Type == BloodType.MotherFatherFather ||
        this.Type == BloodType.MotherMotherFather;

      public MenuItem(string name, string key)
      {
        this.Name = name;
        this.BloodKey = key;
      }
    }

    public class GeneralBloodItem : IBloodCheckableItem
    {
      public static GeneralBloodItem Empty => new() { IsEmpty = true, };

      public bool IsEmpty { get; private init; }

      public ReactiveProperty<bool> IsChecked { get; } = new();

      public string? GroupName => null;

      public string Name { get; init; } = string.Empty;

      public string BloodKey { get; init; } = string.Empty;

      public bool IsMale { get; init; }

      public bool IsDisabled => string.IsNullOrEmpty(this.BloodKey);
    }

    public class GenerationBloodItem
    {
      public string Name { get; init; } = string.Empty;

      public double Rate { get; set; }
    }
  }
}
