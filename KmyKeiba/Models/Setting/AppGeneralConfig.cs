using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race.Finder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Setting
{
  public class AppGeneralConfig
  {
    public static AppGeneralConfig Instance => _instance ??= new();
    private static AppGeneralConfig? _instance;

    private static bool _isInitialized;

    public FinderModel DefaultRaceSetting { get; } = new(new RaceData(), null, Enumerable.Empty<RaceHorseAnalyzer>());

    public async Task InitializeAsync(MyContext db)
    {
      if (_isInitialized) return;

      var raceSetting = ConfigUtil.GetStringValue(SettingKey.DefaultRaceSearchSetting);
      if (!string.IsNullOrEmpty(raceSetting))
      {
        this.DefaultRaceSetting.Input.Deserialize(raceSetting, false);
      }
      this.DefaultRaceSetting.Input.Query.Skip(1).Subscribe(async query =>
      {
        // TODO: catch error
        await ConfigUtil.SetStringValueAsync(SettingKey.DefaultRaceSearchSetting, this.DefaultRaceSetting.Input.Serialize(false));
      });

      _isInitialized = true;
    }
  }
}
