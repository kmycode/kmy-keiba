using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  public class HorseMarkConfigData : AppDataBase
  {
    public string Name { get; set; } = string.Empty;

    public uint Order { get; set; }
  }
}
