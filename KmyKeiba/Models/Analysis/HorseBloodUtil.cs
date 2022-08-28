using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis
{
  internal static class HorseBloodUtil
  {
    private static readonly Dictionary<string, Dictionary<BloodType, BloodItem>> _caches = new();
    private static readonly Dictionary<string, Dictionary<BloodType, BloodItem>> _codeCaches = new();
    private static readonly Dictionary<string, BloodItem> _bloodItems = new();

    public static async Task<string> GetNameAsync(MyContext db, string horseKey, BloodType type)
    {
      var item = await GetBloodItemAsync(db, horseKey, type);
      if (item != null)
      {
        return item.Name ?? string.Empty;
      }
      return string.Empty;
    }

    public static async Task<string> GetBloodCodeAsync(MyContext db, string horseKey, BloodType type)
    {
      var item = await GetBloodItemAsync(db, horseKey, type);
      if (item != null)
      {
        return item.BloodCode ?? string.Empty;
      }
      return string.Empty;
    }

    public static async Task<string> GetNameFromCodeAsync(MyContext db, string horseKey, BloodType type)
    {
      var item = await GetBloodItemFromCodeAsync(db, horseKey, type);
      if (item != null)
      {
        return item.Name ?? string.Empty;
      }
      return string.Empty;
    }

    public static async Task<string> GetBloodCodeFromCodeAsync(MyContext db, string horseKey, BloodType type)
    {
      var item = await GetBloodItemFromCodeAsync(db, horseKey, type);
      if (item != null)
      {
        return item.BloodCode ?? string.Empty;
      }
      return string.Empty;
    }

    private static async Task<BloodItem?> GetBloodItemFromCodeAsync(MyContext db, string horseKey, BloodType type)
    {
      if (!_caches.ContainsKey(horseKey))
      {
        async Task<(string?, string?, string?)> GetParentsAsync(string key)
        {
          var myHorse = await db.HorseBloods!.FirstOrDefaultAsync(h => h.Key == key);
          if (myHorse != null)
          {
            return (myHorse.FatherKey, myHorse.MotherKey, myHorse.Name);
          }

          return default;
        }

        var blood = await db.HorseBloods!.FirstOrDefaultAsync(h => h.Key == horseKey);
        if (blood != null)
        {
          var dic = new Dictionary<BloodType, BloodItem>();
          dic[BloodType.Father] = new BloodItem { BloodCode = blood.FatherKey, };
          dic[BloodType.Mother] = new BloodItem { BloodCode = blood.MotherKey, };
          if (blood.FatherKey != null)
          {
            var (ff, fm, n) = await GetParentsAsync(blood.FatherKey);
            if (ff != null) dic[BloodType.FatherFather] = new BloodItem { BloodCode = ff, };
            if (fm != null) dic[BloodType.FatherMother] = new BloodItem { BloodCode = fm, };
            if (n != null) dic[BloodType.Father].Name = n;
            if (ff != null)
            {
              var (fff, ffm, fn) = await GetParentsAsync(ff);
              if (fff != null) dic[BloodType.FatherFatherFather] = new BloodItem { BloodCode = fff, };
              if (ffm != null) dic[BloodType.FatherFatherMother] = new BloodItem { BloodCode = ffm, };
              if (fn != null) dic[BloodType.FatherFather].Name = fn;
              if (fff != null)
              {
                var (_, _, ffn) = await GetParentsAsync(fff);
                if (ffn != null) dic[BloodType.FatherFatherFather].Name = ffn;
              }
              if (ffm != null)
              {
                var (_, _, ffmn) = await GetParentsAsync(ffm);
                if (ffmn != null) dic[BloodType.FatherFatherMother].Name = ffmn;
              }
            }
            if (fm != null)
            {
              var (fmf, fmm, mn) = await GetParentsAsync(fm);
              if (fmf != null) dic[BloodType.FatherMotherFather] = new BloodItem { BloodCode = fmf, };
              if (fmm != null) dic[BloodType.FatherMotherMother] = new BloodItem { BloodCode = fmm, };
              if (mn != null) dic[BloodType.FatherMother].Name = mn;
              if (fmf != null)
              {
                var (_, _, fmn) = await GetParentsAsync(fmf);
                if (fmn != null) dic[BloodType.FatherMotherFather].Name = fmn;
              }
              if (fmm != null)
              {
                var (_, _, mmn) = await GetParentsAsync(fmm);
                if (mmn != null) dic[BloodType.FatherMotherMother].Name = mmn;
              }
            }
          }
          if (blood.MotherKey != null)
          {
            var (mf, mm, n) = await GetParentsAsync(blood.MotherKey);
            if (mf != null) dic[BloodType.MotherFather] = new BloodItem { BloodCode = mf, };
            if (mm != null) dic[BloodType.MotherMother] = new BloodItem { BloodCode = mm, };
            if (n != null) dic[BloodType.Mother].Name = n;
            if (mf != null)
            {
              var (mff, mfm, fn) = await GetParentsAsync(mf);
              if (mff != null) dic[BloodType.MotherFatherFather] = new BloodItem { BloodCode = mff, };
              if (mfm != null) dic[BloodType.MotherFatherMother] = new BloodItem { BloodCode = mfm, };
              if (fn != null) dic[BloodType.MotherFather].Name = fn;
              if (mff != null)
              {
                var (_, _, ffn) = await GetParentsAsync(mff);
                if (ffn != null) dic[BloodType.MotherFatherFather].Name = ffn;
              }
              if (mfm != null)
              {
                var (_, _, ffmn) = await GetParentsAsync(mfm);
                if (ffmn != null) dic[BloodType.MotherFatherMother].Name = ffmn;
              }
            }
            if (mm != null)
            {
              var (mmf, mmm, mn) = await GetParentsAsync(mm);
              if (mmf != null) dic[BloodType.MotherMotherFather] = new BloodItem { BloodCode = mmf, };
              if (mmm != null) dic[BloodType.MotherMotherMother] = new BloodItem { BloodCode = mmm, };
              if (mn != null) dic[BloodType.MotherMother].Name = mn;
              if (mmf != null)
              {
                var (_, _, fmn) = await GetParentsAsync(mmf);
                if (fmn != null) dic[BloodType.MotherMotherFather].Name = fmn;
              }
              if (mmm != null)
              {
                var (_, _, mmn) = await GetParentsAsync(mmm);
                if (mmn != null) dic[BloodType.MotherMotherMother].Name = mmn;
              }
            }
          }

          _codeCaches[horseKey] = dic;
        }
      }

      _codeCaches.TryGetValue(horseKey, out var item);
      if (item != null)
      {
        item.TryGetValue(type, out var value);
        return value;
      }
      return null;
    }

    public static async Task<string> KeyToBloodCodeAsync(MyContext db, string horseKey)
    {
      var born = await db.HorseBloods!.FirstOrDefaultAsync(b => b.Code == horseKey);
      return born?.Key ?? string.Empty;
    }

    private static async Task<BloodItem?> GetBloodItemAsync(MyContext db, string horseKey, BloodType type)
    {
      var r = _caches.TryGetValue(horseKey, out var item);
      if (!r || item == null)
      {
        var born = await db.BornHorses!.FirstOrDefaultAsync(b => b.Code == horseKey);
        if (born != null)
        {
          var dic = new Dictionary<BloodType, BloodItem>
          {
            [BloodType.Father] = new BloodItem { BloodCode = born.FatherBreedingCode, },
            [BloodType.FatherFather] = new BloodItem { BloodCode = born.FFBreedingCode, },
            [BloodType.FatherFatherFather] = new BloodItem { BloodCode = born.FFFBreedingCode, },
            [BloodType.FatherFatherMother] = new BloodItem { BloodCode = born.FFMBreedingCode, },
            [BloodType.FatherMother] = new BloodItem { BloodCode = born.FMBreedingCode, },
            [BloodType.FatherMotherFather] = new BloodItem { BloodCode = born.FMFBreedingCode, },
            [BloodType.FatherMotherMother] = new BloodItem { BloodCode = born.FMMBreedingCode, },
            [BloodType.Mother] = new BloodItem { BloodCode = born.MotherBreedingCode, },
            [BloodType.MotherFather] = new BloodItem { BloodCode = born.MFBreedingCode, },
            [BloodType.MotherFatherFather] = new BloodItem { BloodCode = born.MFFBreedingCode, },
            [BloodType.MotherFatherMother] = new BloodItem { BloodCode = born.MFMBreedingCode, },
            [BloodType.MotherMother] = new BloodItem { BloodCode = born.MMBreedingCode, },
            [BloodType.MotherMotherFather] = new BloodItem { BloodCode = born.MMFBreedingCode, },
            [BloodType.MotherMotherMother] = new BloodItem { BloodCode = born.MMMBreedingCode, },
          };
          _caches[horseKey] = dic;
        }
        else
        {
          var horse = await db.Horses!.FirstOrDefaultAsync(h => h.Code == horseKey);
          if (horse != null)
          {
            var dic = new Dictionary<BloodType, BloodItem>
            {
              [BloodType.Father] = new BloodItem { BloodCode = horse.FatherBreedingCode, },
              [BloodType.FatherFather] = new BloodItem { BloodCode = horse.FFBreedingCode, },
              [BloodType.FatherFatherFather] = new BloodItem { BloodCode = horse.FFFBreedingCode, },
              [BloodType.FatherFatherMother] = new BloodItem { BloodCode = horse.FFMBreedingCode, },
              [BloodType.FatherMother] = new BloodItem { BloodCode = horse.FMBreedingCode, },
              [BloodType.FatherMotherFather] = new BloodItem { BloodCode = horse.FMFBreedingCode, },
              [BloodType.FatherMotherMother] = new BloodItem { BloodCode = horse.FMMBreedingCode, },
              [BloodType.Mother] = new BloodItem { BloodCode = horse.MotherBreedingCode, },
              [BloodType.MotherFather] = new BloodItem { BloodCode = horse.MFBreedingCode, },
              [BloodType.MotherFatherFather] = new BloodItem { BloodCode = horse.MFFBreedingCode, },
              [BloodType.MotherFatherMother] = new BloodItem { BloodCode = horse.MFMBreedingCode, },
              [BloodType.MotherMother] = new BloodItem { BloodCode = horse.MMBreedingCode, },
              [BloodType.MotherMotherFather] = new BloodItem { BloodCode = horse.MMFBreedingCode, },
              [BloodType.MotherMotherMother] = new BloodItem { BloodCode = horse.MMMBreedingCode, },
            };
            _caches[horseKey] = dic;
          }
        }
      }

      if (!r)
      {
        r = _caches.TryGetValue(horseKey, out item);
      }
      if (!r || item == null)
      {
        return null;
      }

      if (item.TryGetValue(type, out var detail))
      {
        if (_bloodItems.TryGetValue(detail.BloodCode, out var existsItem))
        {
          detail.HorseKey = existsItem.HorseKey;
          detail.Name = existsItem.Name;
        }

        if (detail.HorseKey == null)
        {
          detail.HorseKey = string.Empty;
          var data = await db.HorseBloods!.FirstOrDefaultAsync(b => b.Key == detail.BloodCode);
          if (data != null)
          {
            detail.HorseKey = data.Code;
            detail.Name = data.Name;
          }
        }
        if (detail.HorseKey != null && detail.Name == null)
        {
          detail.Name = string.Empty;
          var data = await db.Horses!.FirstOrDefaultAsync(h => h.Code == detail.HorseKey);
          if (data != null)
          {
            detail.Name = data.Name;
          }
        }

        _bloodItems[detail.BloodCode] = detail;

        return detail;
      }

      return null;
    }

    public static string ToStringCode(this BloodType type)
    {
      return type switch
      {
        BloodType.Father => "f",
        BloodType.FatherFather => "ff",
        BloodType.FatherFatherFather => "fff",
        BloodType.FatherFatherMother => "ffm",
        BloodType.FatherMother => "fm",
        BloodType.FatherMotherFather => "fmf",
        BloodType.FatherMotherMother => "fmm",
        BloodType.Mother => "m",
        BloodType.MotherFather => "mf",
        BloodType.MotherFatherFather => "mff",
        BloodType.MotherFatherMother => "mfm",
        BloodType.MotherMother => "mm",
        BloodType.MotherMotherFather => "mmf",
        BloodType.MotherMotherMother => "mmm",
        _ => string.Empty,
      };
    }

    public static BloodType ToBloodType(string type)
    {
      return type switch
      {
        "f" => BloodType.Father,
        "ff" => BloodType.FatherFather,
        "fff" => BloodType.FatherFatherFather,
        "ffm" => BloodType.FatherFatherMother,
        "fm" => BloodType.FatherMother,
        "fmf" => BloodType.FatherMotherFather,
        "fmm" => BloodType.FatherMotherMother,
        "m" => BloodType.Mother,
        "mf" => BloodType.MotherFather,
        "mff" => BloodType.MotherFatherFather,
        "mfm" => BloodType.MotherFatherMother,
        "mm" => BloodType.MotherMother,
        "mmf" => BloodType.MotherMotherFather,
        "mmm" => BloodType.MotherMotherMother,
        _ => BloodType.Unknown,
      };
    }

    private class BloodItem
    {
      public string? HorseKey { get; set; }

      public string BloodCode { get; set; } = string.Empty;

      public string? Name { get; set; }
    }
  }

  public enum BloodType
  {
    Unknown,

    [Label("父")]
    Father,

    [Label("父父")]
    FatherFather,

    [Label("父父父")]
    FatherFatherFather,

    [Label("父父母")]
    FatherFatherMother,

    [Label("父母")]
    FatherMother,

    [Label("父母父")]
    FatherMotherFather,

    [Label("父母母")]
    FatherMotherMother,

    [Label("母")]
    Mother,

    [Label("母父")]
    MotherFather,

    [Label("母父父")]
    MotherFatherFather,

    [Label("母父母")]
    MotherFatherMother,

    [Label("母母")]
    MotherMother,

    [Label("母母父")]
    MotherMotherFather,

    [Label("母母母")]
    MotherMotherMother,
  }
}
