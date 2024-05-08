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
    // LastLaunchDate = 7,
    IsDownloadCentralOnThursdayAfterOnly = 8,
    LastUpdateStandardTimeYear = 9,
    // LastDownloadNormalDataHour = 10,
    DatabaseVersion = 11,
    IsDownloadJrdb = 12,
    IsRTDownloadJrdb = 13,
    LastDownloadJrdbDate = 14,
    JrdbId = 15,
    JrdbPassword = 16,
    IsNotDownloadHorseBloods = 17,
    IsNotDownloadTrainings = 18,
    IsNotBuildExtraData = 19,
    LastDownloadPlanOfRaceDate = 20,
    LastDownloadPreviousRaceDate = 21,
    LastLaunchDateEx = 22,
  }
}
