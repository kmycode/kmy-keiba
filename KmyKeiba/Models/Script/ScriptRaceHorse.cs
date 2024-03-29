﻿using CefSharp.DevTools.Network;
using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Data;
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
    private readonly RaceHorseData _horse;
    private readonly RaceHorseAnalyzer? _analyzer;
    private readonly bool _isRaceGettable;

    [JsonPropertyName("isTargetRace")]
    public bool IsTargetRace => this._targetRaceKey == this._horse.RaceKey;

    [JsonPropertyName("name")]
    public string Name => this._horse.Name;

    [JsonPropertyName("age")]
    public short Age => this._horse.Age;

    [JsonPropertyName("sex")]
    public short Sex => (short)this._horse.Sex;

    [JsonPropertyName("number")]
    public short Number => this._horse.Number;

    [JsonPropertyName("frameNumber")]
    public short FrameNumber => this._horse.FrameNumber;

    [JsonPropertyName("type")]
    public short Type => (short)this._horse.Type;

    [JsonPropertyName("color")]
    public short Color => (short)this._horse.Color;

    [JsonPropertyName("place")]
    public short Place => this.IsTargetRace ? default : this._horse.ResultOrder;

    [JsonPropertyName("margin")]
    public short Margin => this.IsTargetRace ? default : this._horse.ResultLength1;

    [JsonPropertyName("abnormal")]
    public short Abnormal => this.IsTargetRace ? default : (short)this._horse.AbnormalResult;

    [JsonPropertyName("placeCorner1")]
    public short PlaceCorner1 => this.IsTargetRace ? default : this._horse.FirstCornerOrder;

    [JsonPropertyName("placeCorner2")]
    public short PlaceCorner2 => this.IsTargetRace ? default : this._horse.SecondCornerOrder;

    [JsonPropertyName("placeCorner3")]
    public short PlaceCorner3 => this.IsTargetRace ? default : this._horse.ThirdCornerOrder;

    [JsonPropertyName("placeCorner4")]
    public short PlaceCorner4 => this.IsTargetRace ? default : this._horse.FourthCornerOrder;

    [JsonPropertyName("popular")]
    public short Popular => this._horse.Popular;

    [JsonPropertyName("odds")]
    public short Odds => this._horse.Odds;

    [JsonPropertyName("placeOddsMax")]
    public short PlaceOddsMax => this._horse.PlaceOddsMax;

    [JsonPropertyName("placeOddsMin")]
    public short PlaceOddsMin => this._horse.PlaceOddsMin;

    [JsonPropertyName("weight")]
    public short Weight => this._horse.Weight;

    [JsonPropertyName("weightDiff")]
    public short WeightDiff => this._horse.WeightDiff;

    [JsonPropertyName("riderWeight")]
    public short RiderWeight => this._horse.RiderWeight;

    [JsonPropertyName("isBlinkers")]
    public bool IsBlinkers => this._horse.IsBlinkers;

    [JsonPropertyName("memo")]
    public string Memo => this._horse.Memo ?? string.Empty;

    [JsonPropertyName("riderPlaceBitsRate")]
    public float RiderPlaceBitsRate => this._analyzer?.RiderPlaceBitsRate ?? default;

    [JsonPropertyName("runningStyle")]
    public short RunningStyle => this.IsTargetRace ? default : (short)this._horse.RunningStyle;

    [JsonPropertyName("time")]
    public short Time => this.IsTargetRace ? default : (short)(this._horse.ResultTime.TotalSeconds * 10);

    [JsonPropertyName("a3hTime")]
    public short A3HTime => this.IsTargetRace ? default : (short)Math.Round(this._horse.AfterThirdHalongTime.TotalSeconds * 10);

    [JsonPropertyName("timeDeviationValue")]
    public double TimeDeviationValue => this.IsTargetRace ? default : this._analyzer?.ResultTimeDeviationValue ?? default;

    [JsonPropertyName("a3hTimeDeviationValue")]
    public double A3HTimeDeviationValue => this.IsTargetRace ? default : this._analyzer?.A3HResultTimeDeviationValue ?? default;

    [JsonPropertyName("ua3hTimeDeviationValue")]
    public double UntilA3HTimeDeviationValue => this.IsTargetRace ? default : this._analyzer?.UntilA3HResultTimeDeviationValue ?? default;

    [JsonPropertyName("pci")]
    public double Pci => this._analyzer?.Pci ?? default;

    [JsonPropertyName("race")]
    public ScriptRace? Race => this.IsTargetRace ? null : this._isRaceGettable ?
      new ScriptRace(this._analyzer!.Race, this._analyzer.CurrentRace?.TopHorses.ToArray()) : null;

    [JsonPropertyName("history")]
    public ScriptHistory? History => this._analyzer?.History != null ? new ScriptHistory(this._targetRaceKey, this._analyzer.History) : ScriptHistory.Default;

    [JsonPropertyName("extraData")]
    public ScriptExtraData? ExtraData => this._analyzer?.ExtraData != null ? new ScriptExtraData(this._analyzer.ExtraData) : null;

    public ScriptRaceHorse(string targetRaceKey, RaceHorseAnalyzer horse, bool isRaceGettable = true)
    {
      this._targetRaceKey = targetRaceKey;
      this._horse = horse.Data;
      this._analyzer = horse;
      this._isRaceGettable = isRaceGettable;
    }

    public ScriptRaceHorse(string targetRaceKey, RaceHorseData horse)
    {
      this._targetRaceKey = targetRaceKey;
      this._horse = horse;
    }

    [ScriptMember("getRiderSimilarRacesAsync")]
    public async Task<string> LoadRiderTrendRacesAsync(string keys, int count = 300, int offset = 0)
    {
      var analyzer = this._analyzer?.RiderTrendAnalyzers?.BeginLoadByScript(keys, count, offset, true);
      if (analyzer != null)
      {
        await analyzer.WaitAnalysisAsync();
        return JsonSerializer.Serialize(analyzer.Source.Select(s => new ScriptRaceHorse(string.Empty, s)).Take(count).ToArray(), ScriptManager.JsonOptions);
      }
      return "[]";
    }

    [ScriptMember("getTrainerSimilarRacesAsync")]
    public async Task<string> LoadTrainerTrendRacesAsync(string keys, int count = 300, int offset = 0)
    {
      var analyzer = this._analyzer?.TrainerTrendAnalyzers?.BeginLoadByScript(keys, count, offset);
      if (analyzer != null)
      {
        await analyzer.WaitAnalysisAsync();
        return JsonSerializer.Serialize(analyzer.Source.Select(s => new ScriptRaceHorse(string.Empty, s)).Take(count).ToArray(), ScriptManager.JsonOptions);
      }
      return "[]";
    }

    [ScriptMember("getBloodNamesAsync")]
    public async Task<string> GetBloodNamesAsync()
    {
      var menu = this._analyzer?.BloodSelectors;
      if (menu == null)
      {
        return "[]";
      }

      if (!menu.MenuItems.Any())
      {
        using MyContext db = new();
        await menu.InitializeBloodListAsync(db);
      }

      var keys = new[]
      {
        BloodType.Father,
        BloodType.FatherFather,
        BloodType.FatherFatherFather,
        BloodType.FatherFatherMother,
        BloodType.FatherMother,
        BloodType.FatherMotherFather,
        BloodType.FatherMotherMother,
        BloodType.Mother,
        BloodType.MotherFather,
        BloodType.MotherFatherFather,
        BloodType.MotherFatherMother,
        BloodType.MotherMother,
        BloodType.MotherMotherFather,
        BloodType.MotherMotherMother,
      };

      var names = new List<string>();
      foreach (var key in keys)
      {
        var selector = menu.GetSelector(key);
        names.Add(selector?.Name ?? string.Empty);
      }

      return JsonSerializer.Serialize(names, ScriptManager.JsonOptions);
    }

    [ScriptMember("getBloodHorseRacesAsync")]
    public async Task<string> LoadBloodRacesAsync(string typeCode)
    {
      var analyzer = this._analyzer?.BloodSelectors?.GetSelector(typeCode)?.BeginLoad(Enumerable.Empty<RaceHorseBloodTrendAnalysisSelector.Key>().Append(RaceHorseBloodTrendAnalysisSelector.Key.BloodHorseSelf).ToArray(), 300, 0);
      if (analyzer != null)
      {
        await analyzer.WaitAnalysisAsync();
        return JsonSerializer.Serialize(analyzer.Source.Select(s => new ScriptRaceHorse(string.Empty, s)).ToArray(), ScriptManager.JsonOptions);
      }
      return "[]";
    }

    [ScriptMember("getSameBloodHorseRacesAsync")]
    public async Task<string> LoadSameBloodRacesAsync(string typeCode, string keys, int count = 300, int offset = 0)
    {
      var analyzer = this._analyzer?.BloodSelectors?.GetSelector(typeCode)?.BeginLoadByScript(keys, count, offset);
      if (analyzer != null)
      {
        await analyzer.WaitAnalysisAsync();
        return JsonSerializer.Serialize(analyzer.Source.Select(s => new ScriptRaceHorse(string.Empty, s)).Take(count).ToArray(), ScriptManager.JsonOptions);
      }
      return "[]";
    }

    [ScriptMember("findRacesAsync")]
    public async Task<string> FindRacesAsync(string keys, int count, int offset = 0)
    {
      if (this._analyzer?.FinderModel.Value != null)
      {
        var result = await this._analyzer.FinderModel.Value.FindRacesAsync(keys, count, offset);
        return JsonSerializer.Serialize(
          result.Items.Select(s => new ScriptRace(s.Data)).Take(count).ToArray(), ScriptManager.JsonOptions);
      }
      return "[]";
    }

    [ScriptMember("findRaceHorsesAsync")]
    public async Task<string> FindRaceHorsesAsync(string keys, int count, int offset = 0)
    {
      if (this._analyzer?.FinderModel.Value != null)
      {
        var result = await this._analyzer.FinderModel.Value.FindRaceHorsesAsync(keys, count, offset);
        return JsonSerializer.Serialize(
          result.Items.Select(s => new ScriptRaceHorse(string.Empty, s.Data)).Take(count).ToArray(), ScriptManager.JsonOptions);
      }
      return "[]";
    }

    [ScriptMember("getTrainings")]
    public string GetTrainings()
    {
      var trainings = this._analyzer?.Training.Value?.Trainings.Select(t => new ScriptTraining(t)).ToArray();
      if (trainings != null)
      {
        return JsonSerializer.Serialize(trainings, ScriptManager.JsonOptions);
      }
      return "[]";
    }

    [ScriptMember("getOddsTimeline")]
    public string GetOddsTimeline()
    {
      var timeline = this._analyzer?.OddsTimeline.Select(o => new ScriptOddsTimelineItem(o)).ToArray();
      if (timeline != null)
      {
        return JsonSerializer.Serialize(timeline, ScriptManager.JsonOptions);
      }
      return "[]";
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

      [JsonPropertyName("ua3hTimeDeviationValue")]
      public double UntilA3HTimeDeviationValue => this._history.UntilA3HTimeDeviationValue;

      [JsonPropertyName("disturbanceRate")]
      public double DisturbanceRate => this._history.DisturbanceRate;

      [JsonPropertyName("beforeRaces")]
      public ScriptRaceHorse[] BeforeRaces => this._history.BeforeRaces.Select(r => new ScriptRaceHorse(this._targetRaceKey, r)).ToArray();

      public ScriptHistory(string targetRaceKey, RaceHorseAnalyzer.HistoryData history)
      {
        this._targetRaceKey = targetRaceKey;
        this._history = history;
      }

      public static ScriptHistory Default { get; } = new ScriptHistory(string.Empty, new RaceHorseAnalyzer.HistoryData(new RaceData(), new RaceHorseData(), Enumerable.Empty<RaceHorseAnalyzer>(), null));
    }

    public class ScriptExtraData
    {
      private readonly RaceHorseExtraData _data;

      [JsonPropertyName("rpci")]
      public double Rpci => this._data.Rpci;

      [JsonPropertyName("pci3")]
      public double Pci3 => this._data.Pci3;

      public ScriptExtraData(RaceHorseExtraData extra)
      {
        this._data = extra;
      }
    }
  }
}
