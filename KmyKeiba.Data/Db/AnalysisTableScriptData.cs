using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  public class AnalysisTableScriptData : AppDataBase
  {
    public string Name { get; set; } = string.Empty;

    public string Script { get; set; } = string.Empty;
  }
}
