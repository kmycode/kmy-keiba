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
          Order = 2,
          Type = MemoType.Race,
          Style = MemoStyle.Memo,
        },
        new ExpansionMemoConfig
        {
          Header = "馬メモ",
          Target1 = MemoTarget.Horse,
          MemoNumber = 1,
          Order = 3,
          MemoGroup = 1,
          Type = MemoType.RaceHorse,
          Style = MemoStyle.Memo,
        },
        new ExpansionMemoConfig
        {
          Header = "騎手メモ",
          Target1 = MemoTarget.Rider,
          MemoNumber = 1,
          Order = 4,
          MemoGroup = 1,
          Type = MemoType.RaceHorse,
          Style = MemoStyle.Memo,
        },
        new ExpansionMemoConfig
        {
          Header = "調教師メモ",
          Target1 = MemoTarget.Trainer,
          MemoNumber = 1,
          Order = 5,
          MemoGroup = 1,
          Type = MemoType.RaceHorse,
          Style = MemoStyle.Memo,
        },
      };
      var presetsWithLabel = new (ExpansionMemoConfig, PointLabelData)[]
      {
        (
          new ExpansionMemoConfig
          {
            Header = "レースフラグ",
            Target1 = MemoTarget.Race,
            MemoNumber = 2,
            Order = 6,
            Type = MemoType.Race,
            Style = MemoStyle.Point,
          },
          new PointLabelData
          {
            Name = "レースフラグ",
          }
        ),
        (
          new ExpansionMemoConfig
          {
            Header = "レース結果",
            Target1 = MemoTarget.Race,
            MemoNumber = 3,
            Order = 7,
            Type = MemoType.Race,
            Style = MemoStyle.Point,
          },
          new PointLabelData
          {
            Name = "レース結果",
          }
        ),
        (
          new ExpansionMemoConfig
          {
            Header = "馬券結果",
            Target1 = MemoTarget.Race,
            MemoNumber = 4,
            Order = 8,
            Type = MemoType.Race,
            Style = MemoStyle.Point,
          },
          new PointLabelData
          {
            Name = "馬券結果",
          }
        ),
      };
      presetsWithLabel[0].Item2.SetItems(new PointLabelItem[]
      {
        new PointLabelItem
        {
          Label = string.Empty,
          Point = 0,
          Color = MemoColor.Default,
        },
        new PointLabelItem
        {
          Label = "勝負",
          Point = 1,
          Color = MemoColor.Warning,
        },
        new PointLabelItem
        {
          Label = "見送",
          Point = 2,
          Color = MemoColor.Bad,
        },
        new PointLabelItem
        {
          Label = "買い",
          Point = 3,
          Color = MemoColor.Good,
        },
        new PointLabelItem
        {
          Label = "重要",
          Point = 4,
          Color = MemoColor.Warning,
        },
        new PointLabelItem
        {
          Label = "練習",
          Point = 5,
          Color = MemoColor.Negative,
        },
      });
      presetsWithLabel[1].Item2.SetItems(new PointLabelItem[]
      {
        new PointLabelItem
        {
          Label = string.Empty,
          Point = 0,
          Color = MemoColor.Default,
        },
        new PointLabelItem
        {
          Label = "万券",
          Point = 1,
          Color = MemoColor.Good,
        },
        new PointLabelItem
        {
          Label = "堅い",
          Point = 2,
          Color = MemoColor.Bad,
        },
        new PointLabelItem
        {
          Label = "普通",
          Point = 3,
          Color = MemoColor.Default,
        },
        new PointLabelItem
        {
          Label = "波乱",
          Point = 4,
          Color = MemoColor.Warning,
        },
        new PointLabelItem
        {
          Label = "大荒",
          Point = 5,
          Color = MemoColor.Negative,
        },
      });
      presetsWithLabel[2].Item2.SetItems(new PointLabelItem[]
      {
        new PointLabelItem
        {
          Label = string.Empty,
          Point = 0,
          Color = MemoColor.Default,
        },
        new PointLabelItem
        {
          Label = "的中",
          Point = 1,
          Color = MemoColor.Good,
        },
        new PointLabelItem
        {
          Label = "ガミ",
          Point = 2,
          Color = MemoColor.Warning,
        },
        new PointLabelItem
        {
          Label = "外し",
          Point = 3,
          Color = MemoColor.Bad,
        },
      });
      await db.MemoConfigs!.AddRangeAsync(presets);
      await db.PointLabels!.AddRangeAsync(presetsWithLabel.Select(l => l.Item2));
      await db.SaveChangesAsync();

      foreach (var label in presetsWithLabel)
      {
        label.Item1.PointLabelId = (short)label.Item2.Id;
      }
      await db.MemoConfigs!.AddRangeAsync(presetsWithLabel.Select(l => l.Item1));
      await db.SaveChangesAsync();
    }
  }
}
