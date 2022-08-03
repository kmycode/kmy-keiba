using KmyKeiba.Data.Db;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Data
{
  static class DatabasePresetModel
  {
    public static async Task SetPresetsAsync()
    {
      using var db = new MyContext();
      await SetExpansionMemoPresets(db);
    }

    private static async Task SetExpansionMemoPresets(MyContext db)
    {
      if (await db.MemoConfigs!.AnyAsync() || await db.Memos!.AnyAsync())
      {
        return;
      }

      var presets = new ExpansionMemoConfig[]
      {
        new ExpansionMemoConfig
        {
          Header = "レースメモ",
          Target1 = MemoTarget.Race,
          MemoNumber = 1,
          Order = 1,
          Type = MemoType.Race,
          Style = MemoStyle.Memo,
        },
        new ExpansionMemoConfig
        {
          Header = "日記",
          Target1 = MemoTarget.Day,
          MemoNumber = 1,
          Order = 1,
          Type = MemoType.Race,
          Style = MemoStyle.Memo,
        },
        new ExpansionMemoConfig
        {
          Header = "馬メモ",
          Target1 = MemoTarget.Horse,
          MemoNumber = 1,
          Order = 1,
          MemoGroup = 1,
          Type = MemoType.RaceHorse,
          Style = MemoStyle.Memo,
        },
        new ExpansionMemoConfig
        {
          Header = "騎手メモ",
          Target1 = MemoTarget.Rider,
          MemoNumber = 1,
          Order = 1,
          MemoGroup = 1,
          Type = MemoType.RaceHorse,
          Style = MemoStyle.Memo,
        },
        new ExpansionMemoConfig
        {
          Header = "調教師メモ",
          Target1 = MemoTarget.Trainer,
          MemoNumber = 1,
          Order = 1,
          MemoGroup = 1,
          Type = MemoType.RaceHorse,
          Style = MemoStyle.Memo,
        },
      };
      await db.MemoConfigs!.AddRangeAsync(presets);
      await db.SaveChangesAsync();
    }
  }
}
