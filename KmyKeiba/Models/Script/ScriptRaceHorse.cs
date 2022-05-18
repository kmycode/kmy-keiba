using KmyKeiba.Models.Analysis;
using Microsoft.ClearScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Script
{
  [NoDefaultScriptAccess]
  public class ScriptRaceHorse
  {
    private readonly string _targetRaceKey;
    private readonly RaceHorseAnalyzer _horse;

    [JsonPropertyName("isTargetRace")]
    public bool IsTargetRace => this._targetRaceKey == this._horse.Data.RaceKey;

    [JsonPropertyName("name")]
    public string Name => this._horse.Data.Name;

    [JsonPropertyName("age")]
    public short Age => this._horse.Data.Age;

    [JsonPropertyName("sex")]
    public short Sex => (short)this._horse.Data.Sex;

    [JsonPropertyName("number")]
    public short Number => this._horse.Data.Number;

    [JsonPropertyName("frameNumber")]
    public short FrameNumber => this._horse.Data.FrameNumber;

    [JsonPropertyName("type")]
    public short Type => (short)this._horse.Data.Type;

    [JsonPropertyName("color")]
    public short Color => (short)this._horse.Data.Color;

    [JsonPropertyName("place")]
    public short Place => this.IsTargetRace ? default : this._horse.Data.ResultOrder;

    [JsonPropertyName("margin")]
    public short Margin => this.IsTargetRace ? default : this._horse.Data.ResultLength1;

    [JsonPropertyName("abnormal")]
    public short Abnormal => this.IsTargetRace ? default : (short)this._horse.Data.AbnormalResult;

    [JsonPropertyName("popular")]
    public short Popular => this._horse.Data.Popular;

    [JsonPropertyName("odds")]
    public short Odds => this._horse.Data.Odds;

    [JsonPropertyName("placeOddsMax")]
    public short PlaceOddsMax => this._horse.Data.PlaceOddsMax;

    [JsonPropertyName("placeOddsMin")]
    public short PlaceOddsMin => this._horse.Data.PlaceOddsMin;

    [JsonPropertyName("runningStyle")]
    public short RunningStyle => this.IsTargetRace ? default : (short)this._horse.Data.RunningStyle;

    [JsonPropertyName("time")]
    public short Time => this.IsTargetRace ? default : (short)(this._horse.Data.ResultTime.TotalSeconds * 10);

    [JsonPropertyName("a3hTime")]
    public short A3HTime => this.IsTargetRace ? default : (short)(this._horse.Data.AfterThirdHalongTime.TotalSeconds * 10);

    [JsonPropertyName("timeDeviationValue")]
    public double TimeDeviationValue => this.IsTargetRace ? default : this._horse.ResultTimeDeviationValue;

    [JsonPropertyName("a3hTimeDeviationValue")]
    public double A3HTimeDeviationValue => this.IsTargetRace ? default : this._horse.A3HResultTimeDeviationValue;

    [JsonPropertyName("race")]
    public ScriptRace? Race => this.IsTargetRace ? null : new ScriptRace(this._horse.Race);

    [JsonPropertyName("history")]
    public ScriptHistory? History => (this._horse.History != null && this.IsTargetRace) ? new ScriptHistory(this._targetRaceKey, this._horse.History) : null;

    public ScriptRaceHorse(string targetRaceKey, RaceHorseAnalyzer horse)
    {
      this._targetRaceKey = targetRaceKey;
      this._horse = horse;
    }

    [ScriptMember("getJson")]
    public string ToJson()
    {
      return JsonSerializer.Serialize(this, ScriptManager.JsonOptions);
    }

    public class ScriptHistory
    {
      private readonly string _targetRaceKey;
      private readonly RaceHorseAnalyzer.HistoryData _history;

      [JsonPropertyName("runningStyle")]
      public short RunningStyle => (short)this._history.RunningStyle;

      [JsonPropertyName("timeDeviationValue")]
      public double TimeDeviationValue => this._history.TimeDeviationValue;

      [JsonPropertyName("a3hTimeDeviationValue")]
      public double A3HTimeDeviationValue => this._history.A3HTimeDeviationValue;

      [JsonPropertyName("beforeRaces")]
      public ScriptRaceHorse[] BeforeRaces => this._history.BeforeRaces.Select(r => new ScriptRaceHorse(this._targetRaceKey, r)).ToArray();

      public ScriptHistory(string targetRaceKey, RaceHorseAnalyzer.HistoryData history)
      {
        this._targetRaceKey = targetRaceKey;
        this._history = history;
      }
    }
  }
}
