using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  [Index(nameof(RaceKey))]
  public class ExternalNumberData : AppDataBase
  {
    public uint ConfigId { get; set; }

    public string RaceKey { get; set; } = string.Empty;

    public short HorseNumber { get; set; }

    public int Value { get; set; }

    public short Order { get; set; }
  }
}
