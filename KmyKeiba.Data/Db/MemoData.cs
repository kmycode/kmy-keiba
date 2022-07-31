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

  public enum MemoTarget : short
  {
    Unknown = 0,
    Race = 8,
    Course = 16,
    Distance = 24,        // 数値比較が必要になるため現在は実装しない。将来のための予約
    Horse = 32,
    Rider = 40,
    Trainer = 48,
    Owner = 56,
  }
}
