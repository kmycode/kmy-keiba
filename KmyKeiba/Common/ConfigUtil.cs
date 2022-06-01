using KmyKeiba.Data.Db;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Common
{
  internal static class ConfigUtil
  {
    public static async Task<int> GetIntValueAsync(SettingKey key)
    {
      using var db = new MyContext();
      return await GetIntValueAsync(db, key);
    }

    public static async Task<int> GetIntValueAsync(MyContext db, SettingKey key)
    {
      var data = await db.SystemData!.FirstOrDefaultAsync(s => s.Key == key);
      return data?.IntValue ?? default;
    }

    public static async Task<string> GetStringValueAsync(MyContext db, SettingKey key)
    {
      var data = await db.SystemData!.FirstOrDefaultAsync(s => s.Key == key);
      return data?.StringValue ?? string.Empty;
    }

    public static async Task SetIntValueAsync(SettingKey key, int value)
    {
      using var db = new MyContext();
      await SetIntValueAsync(db, key, value);
    }

    public static async Task SetStringValueAsync(SettingKey key, string value)
    {
      using var db = new MyContext();
      await SetStringValueAsync(db, key, value);
    }

    public static async Task SetIntValueAsync(MyContext db, SettingKey key, int value)
    {
      var data = await db.SystemData!.FirstOrDefaultAsync(s => s.Key == key);
      if (data != null)
      {
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
      }
      await db.SaveChangesAsync();
    }

    public static async Task SetStringValueAsync(MyContext db, SettingKey key, string value)
    {
      var data = await db.SystemData!.FirstOrDefaultAsync(s => s.Key == key);
      if (data != null)
      {
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
      }
      await db.SaveChangesAsync();
    }
  }
}
