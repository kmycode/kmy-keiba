using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  public class LearningDataCache
  {
    public uint Id { get; set; }

    public string RaceKey { get; set; } = string.Empty;

    public string HorseName { get; set; } = string.Empty;

    public int CacheVersion { get; set; }

    public string Cache { get; set; } = string.Empty;
  }
}
