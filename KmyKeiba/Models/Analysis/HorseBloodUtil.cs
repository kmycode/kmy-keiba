using KmyKeiba.Common;
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
