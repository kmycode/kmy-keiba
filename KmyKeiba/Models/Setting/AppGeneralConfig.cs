using KmyKeiba.Common;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race.Finder;
using Reactive.Bindings;
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

    public ReactiveProperty<string> NearDistanceDiffCentral { get; } = new();

    public ReactiveProperty<string> NearDistanceDiffLocal { get; } = new();

    public ReactiveProperty<string> ShortestTimeNearYearCentral { get; } = new();

    public ReactiveProperty<string> ShortestTimeNearYearLocal { get; } = new();

    public ReactiveProperty<bool> IsShowScriptBulkButton { get; } = new();

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

      this.NearDistanceDiffCentral.Value = ConfigUtil.GetIntValue(SettingKey.NearDistanceDiffCentral, 50).ToString();
      this.NearDistanceDiffLocal.Value = ConfigUtil.GetIntValue(SettingKey.NearDistanceDiffLocal, 50).ToString();
      this.ShortestTimeNearYearCentral.Value = ConfigUtil.GetIntValue(SettingKey.ShortestTimeNearYearCentral, 10).ToString();
      this.ShortestTimeNearYearLocal.Value = ConfigUtil.GetIntValue(SettingKey.ShortestTimeNearYearLocal, 10).ToString();
      this.IsShowScriptBulkButton.Value = ConfigUtil.GetBooleanValue(SettingKey.IsShowScriptBulkButton);

      this.NearDistanceDiffCentral.Skip(1).Subscribe(async v => await this.SaveIntValueAsync(SettingKey.NearDistanceDiffCentral, v));
      this.NearDistanceDiffLocal.Skip(1).Subscribe(async v => await this.SaveIntValueAsync(SettingKey.NearDistanceDiffLocal, v));
      this.ShortestTimeNearYearCentral.Skip(1).Subscribe(async v => await this.SaveIntValueAsync(SettingKey.ShortestTimeNearYearCentral, v));
      this.ShortestTimeNearYearLocal.Skip(1).Subscribe(async v => await this.SaveIntValueAsync(SettingKey.ShortestTimeNearYearLocal, v));
      this.IsShowScriptBulkButton.Skip(1).Subscribe(async v => await this.SaveBooleanValueAsync(SettingKey.IsShowScriptBulkButton, v));

      _isInitialized = true;
    }

    private async Task SaveIntValueAsync(SettingKey key, string value)
    {
      try
      {
        if (int.TryParse(value, out var v))
        {
          await ConfigUtil.SetIntValueAsync(key, v);
        }
      }
      catch (Exception ex)
      {
        // TODO: 保存失敗
      }
    }

    private async Task SaveBooleanValueAsync(SettingKey key, bool value)
    {
      try
      {
        await ConfigUtil.SetBooleanValueAsync(key, value);
      }
      catch (Exception ex)
      {
        // TODO: 保存失敗
      }
    }
  }
}
