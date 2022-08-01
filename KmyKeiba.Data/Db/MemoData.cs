using KmyKeiba.JVLink.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  [Index(nameof(Point), nameof(Target1), nameof(Key1))]
  public class MemoData : AppDataBase
  {
    public MemoTarget Target1 { get; set; }

    [StringLength(20)]
    public string Key1 { get; set; } = string.Empty;

    public MemoTarget Target2 { get; set; }

    [StringLength(20)]
    public string Key2 { get; set; } = string.Empty;

    public MemoTarget Target3 { get; set; }

    [StringLength(20)]
    public string Key3 { get; set; } = string.Empty;

    public RaceCourse CourseKey { get; set; }

    /// <summary>
    /// Targetが同じメモを複数作るときの識別子
    /// </summary>
    public int Number { get; set; }

    public string Memo { get; set; } = string.Empty;

    /// <summary>
    ///  ユーザーの指定する得点
    /// </summary>
    public short Point { get; set; }
  }

  // 拡張キー検索（finder）、拡張メモ画面、両方での対応が必要
  [Flags]
  public enum MemoTarget : int
  {
    Unknown = 0,
    Race = 0b_0001,
    Day = 0b_0010,
    Course = 0b_0100,
    Direction = 0b_1000,       // 予約
    Distance = 0b_0001_0000,        // 数値比較が必要になるため現在は実装しない。将来のための予約
    Grades = 0b_0010_0000,          // 予約
    Condition = 0b_0100_0000,       // 予約
    Weather = 0b_1000_0000,         // 予約
    Horse = 0b_0001_0000_0000_0000_0000,
    Rider = 0b_0010_0000_0000_0000_0000,
    Trainer = 0b_0100_0000_0000_0000_0000,
    Owner = 0b_1000_0000_0000_0000_0000,
  }
}
