using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  public class SystemData
  {
    [Key]
    public uint Id { get; set; }

    public SettingKey Key { get; set; }

    public int IntValue { get; set; }

    public string StringValue { get; set; } = string.Empty;
  }

  public enum SettingKey : short
  {
    Unknown = 0,

    LastDownloadCentralDate = 1,
    LastDownloadLocalDate = 2,
    IsDownloadCentral = 3,
    IsDownloadLocal = 4,
    IsRTDownloadCentral = 5,
    IsRTDownloadLocal = 6,
    LastLaunchDate = 7,
    IsDownloadCentralOnThursdayAfterOnly = 8,
  }
}
