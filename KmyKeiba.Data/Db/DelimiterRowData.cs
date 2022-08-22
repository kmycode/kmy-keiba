using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  public class DelimiterRowData : AppDataBase
  {
    public uint DelimiterId { get; set; }

    public int Order { get; set; }

    public string FinderConfig { get; set; } = string.Empty;
  }
}
