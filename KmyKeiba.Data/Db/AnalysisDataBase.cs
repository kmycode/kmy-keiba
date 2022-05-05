using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  public class AnalysisDataBase
  {
    public uint Id { get; set; }

    public DateTime LastModified { get; set; }

    public ushort AnalysisVersion { get; set; }
  }
}
