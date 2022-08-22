using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  public class AnalysisTableWeightRowData : AppDataBase
  {
    public uint WeightId { get; set; }

    public int Order { get; set; }

    public string FinderConfig { get; set; } = string.Empty;

    public double Weight { get; set; }
    
    public WeightBehavior Behavior { get; set; }
  }

  public enum WeightBehavior : short
  {
    /// <summary>
    /// ウェイトを値に乗算する
    /// </summary>
    Multiply = 0,

    /// <summary>
    /// ウェイトを値に乗算した上で、次の値に進む
    /// </summary>
    MultiplyAndNext = 1,

    /// <summary>
    /// 値をウェイトに置き換える（TableRowのベースウェイトは適用される）
    /// </summary>
    Replace = 2,
  }
}
