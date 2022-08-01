using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  public class ExpansionMemoConfig : AppDataBase
  {
    public string Header { get; set; } = string.Empty;

    public MemoType Type { get; set; }

    public short Order { get; set; }

    public MemoStyle Style { get; set; }

    public short MemoNumber { get; set; }

    public MemoTarget Target1 { get; set; }

    public MemoTarget Target2 { get; set; }

    public MemoTarget Target3 { get; set; }

    public short PointLabelId { get; set; }
  }

  [Flags]
  public enum MemoStyle : short
  {
    None = 0,
    Memo = 0b_1,
    Point = 0b_10,
    MemoAndPoint = Memo | Point,
  }

  public enum MemoType : short
  {
    Unknown = 0,
    Race = 1,
    RaceHorse = 2,
  }
}
