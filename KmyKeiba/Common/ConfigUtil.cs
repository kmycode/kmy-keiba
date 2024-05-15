using KmyKeiba.Data.Db;
using KmyKeiba.Models.Connection;
using KmyKeiba.Models.Data;
using KmyKeiba.Shared;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Common
{
  internal static class ConfigUtil
  {
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    private static bool _isInitialized;
    private static readonly Dictionary<SettingKey, SystemData> _caches = [];

    public static async Task InitializeAsync()
    {
      if (_isInitialized) return;
      _isInitialized = true;

      // この時点でDB初期化されていない可能性あり
      if (!DownloaderConnector.Instance.IsExistsDatabase) return;

      try
      {
        using var db = new MyContext();

        var configs = await db.SystemData!.ToArrayAsync();

        foreach (var config in configs)
        {
          _caches[config.Key] = config;
        }
      }
      catch (Exception ex)
      {
        logger.Fatal("初期化でエラーが発生", ex);

        // TODO: 画面に反映
      }
    }

    public static int GetIntValue(SettingKey key)
    {
      if (_caches.TryGetValue(key, out var data))
      {
        return data.IntValue;
      }
      return default;
    }

    public static bool GetBooleanValue(SettingKey key)
    {
      if (_caches.TryGetValue(key, out var data))
      {
        return data.IntValue != 0;
      }
      return default;
    }

    public static string GetStringValue(SettingKey key)
    {
      if (_caches.TryGetValue(key, out var data))
      {
        return data.StringValue;
      }
      return string.Empty;
    }

    public static async Task SetIntValueAsync(SettingKey key, int value)
    {
      using var db = new MyContext();
      await SetIntValueAsync(db, key, value);
    }

    public static async Task SetBooleanValueAsync(SettingKey key, bool value)
    {
      using var db = new MyContext();
      await SetBooleanValueAsync(db, key, value);
    }

    public static async Task SetStringValueAsync(SettingKey key, string value)
    {
      using var db = new MyContext();
      await SetStringValueAsync(db, key, value);
    }

    public static async Task SetIntValueAsync(MyContext db, SettingKey key, int value)
    {
      if (_caches.TryGetValue(key, out var data))
      {
        db.SystemData!.Attach(data);
        data.IntValue = value;
      }
      else
      {
        data = new SystemData
        {
          Key = key,
          IntValue = value,
        };
        await db.SystemData!.AddAsync(data);
        _caches[key] = data;
      }
      await db.SaveChangesAsync();
    }

    public static Task SetBooleanValueAsync(MyContext db, SettingKey key, bool value)
      => SetIntValueAsync(db, key, value ? 1 : 0);

    public static async Task SetStringValueAsync(MyContext db, SettingKey key, string value)
    {
      if (_caches.TryGetValue(key, out var data))
      {
        db.SystemData!.Attach(data);
        data.StringValue = value;
      }
      else
      {
        data = new SystemData
        {
          Key = key,
          StringValue = value,
        };
        await db.SystemData!.AddAsync(data);
        _caches[key] = data;
      }
      await db.SaveChangesAsync();
    }
  }
}
