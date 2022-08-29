using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  public class HorseMarkData : AppDataBase
  {
    public uint ConfigId { get; set; }

    public string RaceKey { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public RaceHorseMark Mark { get; set; }
  }
}
