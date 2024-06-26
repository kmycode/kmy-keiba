﻿using KmyKeiba.Data.Db;
using KmyKeiba.Models.Data;
using log4net.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis
{
  internal static class CheckHorseUtil
  {
    private static readonly List<CheckHorseData> _checkedHorses = new();
    private static bool _isInitialized;

    public static async Task InitializeAsync(MyContext db)
    {
      if (!_isInitialized)
      {
        var horses = await db.CheckHorses!.ToArrayAsync();
        foreach (var h in horses)
        {
          _checkedHorses.Add(h);
        }
        _isInitialized = true;
      }
    }

    public static bool IsChecked(string key, HorseCheckType type)
    {
      return _checkedHorses.Any(h => h.Type == type && h.Key == key);
    }

    public static async Task CheckAsync(MyContext db, string key, HorseCheckType type)
    {
      if (!IsChecked(key, type))
      {
        var item = new CheckHorseData
        {
          Type = type,
          Key = key,
        };

        await db.CheckHorses!.AddAsync(item);
        await db.SaveChangesAsync();

        _checkedHorses.Add(item);
      }
    }

    public static async Task UncheckAsync(MyContext db, string key, HorseCheckType type)
    {
      var item = _checkedHorses.FirstOrDefault(h => h.Type == type && h.Key == key);
      if (item != null)
      {
        db.CheckHorses!.Remove(item);
        await db.SaveChangesAsync();

        _checkedHorses.Remove(item);
      }
    }
  }

  public class HorseCheckChangedEventArgs : EventArgs
  {
    public string RaceKey { get; }

    public bool IsChecked { get; }

    public HorseCheckType Type { get; }

    public HorseCheckChangedEventArgs(HorseCheckType type, string raceKey, bool isChecked)
    {
      this.Type = type;
      this.RaceKey = raceKey;
      this.IsChecked = isChecked;
    }
  }
}
