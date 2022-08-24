using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  public class AnalysisTableRowData : AppDataBase
  {
    public string Name { get; set; } = string.Empty;

    public uint TableId { get; set; }

    public string FinderConfig { get; set; } = string.Empty;

    public AnalysisTableRowOutputType Output { get; set; }

    public double BaseWeight { get; set; }

    public uint WeightId { get; set; }

    public uint Weight2Id { get; set; }

    public uint Weight3Id { get; set; }

    public uint ParentRowId { get; set; }

    public uint ExternalNumberId { get; set; }

    public uint MemoConfigId { get; set; }

    public uint Order { get; set; }

    public short RequestedSize { get; set; }

    public double AlternativeValueIfEmpty { get; set; }
  }

  public enum AnalysisTableRowOutputType : short
  {
    Unknown = 0,

    Time = 1,
    A3HTime = 2,
    UA3HTime = 3,
    ShortestTime = 4,

    RecoveryRate = 11,

    PlaceBetsRate = 21,
    WinRate = 22,

    /// <summary>
    /// 現在のレースに条件に当てはまる馬がいるか？true or falseで判定する
    /// </summary>
    Binary = 101,

    /// <summary>
    /// 【現在のレースに対して】固定値に対して重みをかける。重みの値のみで判定することになる
    /// </summary>
    FixedValue = 201,

    /// <summary>
    /// 過去のレースそれぞれに対して重みをかける点を除き、FixedValueと同じ
    /// </summary>
    FixedValuePerPastRace = 202,

    /// <summary>
    /// 外部指数をそのままポイントとして出力する
    /// </summary>
    ExternalNumber = 301,

    ExpansionMemo = 302,
  }
}
