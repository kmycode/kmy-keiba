using KmyKeiba.Data.Db;
using KmyKeiba.Data.Wrappers;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Race;
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
  public class ScriptCurrentRace
  {
    protected readonly RaceInfo _race;

    [JsonPropertyName("name")]
    public string Name => this._race.Data.Name;

    [JsonPropertyName("displayName")]
    public string DisplayName => this._race.Subject.DisplayName;

    [JsonPropertyName("subjectName")]
    public string SubjectName => this._race.Data.SubjectName;

    [JsonPropertyName("subject")]
    public ScriptRaceSubject? Subject => this._race.Subject.Subject.IsLocal ? new(this._race.Subject) : null;

    [JsonPropertyName("grade")]
    public short Grade => (short)this._race.Data.Grade;

    [JsonPropertyName("subjectAge2")]
    public short SubjectAge2 => (short)this._race.Data.SubjectAge2;

    [JsonPropertyName("subjectAge3")]
    public short SubjectAge3 => (short)this._race.Data.SubjectAge3;

    [JsonPropertyName("subjectAge4")]
    public short SubjectAge4 => (short)this._race.Data.SubjectAge4;

    [JsonPropertyName("subjectAge5")]
    public short SubjectAge5 => (short)this._race.Data.SubjectAge5;

    [JsonPropertyName("subjectAgeYoungest")]
    public short SubjectAgeYoungest => (short)this._race.Data.SubjectAgeYounger;

    [JsonPropertyName("course")]
    public short Course => (short)this._race.Data.Course;

    [JsonPropertyName("courseName")]
    public string CourseName => this._race.Data.Course.GetName();

    [JsonPropertyName("courseType")]
    public string CourseType => this._race.Data.CourseType;

    [JsonPropertyName("raceNumber")]
    public short RaceNumber => this._race.Data.CourseRaceNumber;

    [JsonPropertyName("weather")]
    public short Weather => (short)this._race.Data.TrackWeather;

    [JsonPropertyName("condition")]
    public short Condition => (short)this._race.Data.TrackCondition;

    [JsonPropertyName("ground")]
    public short Ground => (short)this._race.Data.TrackGround;

    [JsonPropertyName("type")]
    public short Type => (short)this._race.Data.TrackType;

    [JsonPropertyName("direction")]
    public short Direction => (short)this._race.Data.TrackCornerDirection;

    [JsonPropertyName("distance")]
    public short Distance => this._race.Data.Distance;

    [JsonPropertyName("horsesCount")]
    public short HorsesCount => this._race.Data.HorsesCount;

    [JsonPropertyName("startTime")]
    public string StartTime => this._race.Data.StartTime.ToString();

    [JsonIgnore]
    [ScriptMember("horses")]
    public virtual string Horses => JsonSerializer.Serialize(this._race.Horses.Select(h => new ScriptRaceHorse(this._race.Data.Key, h)), ScriptManager.JsonOptions);

    public ScriptCurrentRace(RaceInfo race)
    {
      this._race = race;
    }

    [ScriptMember("getHorse")]
    public virtual ScriptRaceHorse? GetHorse(short horseNum)
    {
      var horse = this._race.Horses.FirstOrDefault(h => h.Data.Number == horseNum);
      if (horse != null)
      {
        return new ScriptRaceHorse(this._race.Data.Key, horse);
      }
      return null;
    }

    [ScriptMember("getAllHorses")]
    public ScriptRaceHorse[] GetAllHorses()
    {
      return this._race.Horses.Select(h => new ScriptRaceHorse(this._race.Data.Key, h)).ToArray();
    }

    [ScriptMember("getSimilarRacesAsync")]
    public async Task<string> LoadTrendRacesAsync(string keys, int count = 300, int offset = 0)
    {
      var analyzer = this._race.TrendAnalyzers.BeginLoadByScript(keys, count, offset);
      await analyzer.WaitAnalysisAsync();
      return JsonSerializer.Serialize(analyzer.Source.Select(s => new ScriptRace(s.Data, s.TopHorses)).Take(count).ToArray(), ScriptManager.JsonOptions);
    }

    [ScriptMember("getSimilarRaceHorsesAsync")]
    public async Task<string> LoadHorseTrendRacesAsync(string keys, int count = 500, int offset = 0)
    {
      var analyzer = this._race.WinnerTrendAnalyzers.BeginLoadByScript(keys, count, offset, true);
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
      var result = await this._race.Finder.FindRacesAsync(null, keys, count, offset);
      return JsonSerializer.Serialize(
        result.Select(s => new ScriptRace(s.Data)).Take(count).ToArray(), ScriptManager.JsonOptions);
    }

    [ScriptMember("findRaceHorsesAsync")]
    public async Task<string> FindRaceHorsesAsync(string keys, int count, int offset = 0)
    {
      var result = await this._race.Finder.FindRaceHorsesAsync(null, keys, count, offset);
      return JsonSerializer.Serialize(
        result.Select(s => new ScriptRaceHorse(string.Empty, s.Data)).Take(count).ToArray(), ScriptManager.JsonOptions);
    }

    [ScriptMember("getFrameNumberOdds")]
    public string GetFrameNumberOdds()
    {
      var odds = this._race.Odds.Value?.Frame?.RestoreOdds()
        .Where(o => o.Odds != default)
        .Select(o => new JsonObject { { "frame1", o.Frame1 }, { "frame2", o.Frame2 }, { "odds", o.Odds }, });
      return JsonSerializer.Serialize(odds);
    }

    [ScriptMember("getQuinellaPlaceOdds")]
    public string GetQuinellaPlaceOdds()
    {
      var odds = this._race.Odds.Value?.QuinellaPlace?.RestoreOdds()
        .Where(o => o.PlaceOddsMax != default)
        .Select(o => new JsonObject { { "number1", o.HorseNumber1 }, { "number2", o.HorseNumber2 }, { "oddsMax", o.PlaceOddsMax }, { "oddsMin", o.PlaceOddsMin }, });
      return JsonSerializer.Serialize(odds);
    }

    [ScriptMember("getQuinellaOdds")]
    public string GetQuinellaOdds()
    {
      var odds = this._race.Odds.Value?.Quinella?.RestoreOdds()
        .Where(o => o.Odds != default)
        .Select(o => new JsonObject { { "number1", o.HorseNumber1 }, { "number2", o.HorseNumber2 }, { "odds", o.Odds }, });
      return JsonSerializer.Serialize(odds);
    }

    [ScriptMember("getExactaOdds")]
    public string GetExactaOdds()
    {
      var odds = this._race.Odds.Value?.Exacta?.RestoreOdds()
        .Where(o => o.Odds != default)
        .Select(o => new JsonObject { { "number1", o.HorseNumber1 }, { "number2", o.HorseNumber2 }, { "odds", o.Odds }, });
      return JsonSerializer.Serialize(odds);
    }

    [ScriptMember("getTrioOdds")]
    public string GetTrioOdds()
    {
      var odds = this._race.Odds.Value?.Trio?.RestoreOdds()
        .Where(o => o.Odds != default)
        .Select(o => new JsonObject { { "number1", o.HorseNumber1 }, { "number2", o.HorseNumber2 }, { "number3", o.HorseNumber3 }, { "odds", o.Odds }, });
      return JsonSerializer.Serialize(odds);
    }

    [ScriptMember("getTrifectaOdds")]
    public string GetTrifectaOdds()
    {
      var odds = this._race.Odds.Value?.Trifecta?.RestoreOdds()
        .Where(o => o.Odds != default)
        .Select(o => new JsonObject { { "number1", o.HorseNumber1 }, { "number2", o.HorseNumber2 }, { "number3", o.HorseNumber3 }, { "odds", o.Odds }, });
      return JsonSerializer.Serialize(odds);
    }

    [ScriptMember("getJson")]
    public string ToJson()
    {
      return JsonSerializer.Serialize(this, ScriptManager.JsonOptions);
    }
  }

  public class ScriptCurrentRaceWithResults : ScriptCurrentRace
  {
    [JsonPropertyName("cornerRanking1")]
    public string CornerRanking1 => this._race.Data.Corner1Result;

    [JsonPropertyName("cornerLapTime1")]
    public short CornerLapTime1 => (short)(this._race.Data.Corner1LapTime.TotalSeconds * 10);

    [JsonPropertyName("cornerRanking2")]
    public string CornerRanking2 => this._race.Data.Corner2Result;

    [JsonPropertyName("cornerLapTime2")]
    public short CornerLapTime2 => (short)(this._race.Data.Corner2LapTime.TotalSeconds * 10);

    [JsonPropertyName("cornerRanking3")]
    public string CornerRanking3 => this._race.Data.Corner3Result;

    [JsonPropertyName("cornerLapTime3")]
    public short CornerLapTime3 => (short)(this._race.Data.Corner3LapTime.TotalSeconds * 10);

    [JsonPropertyName("cornerRanking4")]
    public string CornerRanking4 => this._race.Data.Corner4Result;

    [JsonPropertyName("cornerLapTime4")]
    public short CornerLapTime4 => (short)(this._race.Data.Corner4LapTime.TotalSeconds * 10);

    [JsonIgnore]
    [ScriptMember("horses")]
    public override string Horses => JsonSerializer.Serialize(this._race.Horses.Select(h => new ScriptRaceHorse(string.Empty, h)), ScriptManager.JsonOptions);

    public ScriptCurrentRaceWithResults(RaceInfo race) : base(race)
    {
    }

    [ScriptMember("getHorse")]
    public override ScriptRaceHorse? GetHorse(short horseNum)
    {
      var horse = this._race.Horses.FirstOrDefault(h => h.Data.Number == horseNum);
      if (horse != null)
      {
        return new ScriptRaceHorse(string.Empty, horse);
      }
      return null;
    }
  }

  [NoDefaultScriptAccess]
  public class ScriptRace
  {
    private readonly RaceData _race;
    private readonly RaceSubjectInfo _subject;
    private readonly IReadOnlyList<RaceHorseAnalyzer>? _topHorses;
    private readonly IReadOnlyList<RaceHorseData>? _topHorses2;

    [JsonPropertyName("name")]
    public string Name => this._race.Name;

    [JsonPropertyName("displayName")]
    public string DisplayName => this._subject.DisplayName;

    [JsonPropertyName("subjectName")]
    public string SubjectName => this._race.SubjectName;

    [JsonPropertyName("subject")]
    public ScriptRaceSubject? Subject => this._subject.Subject.IsLocal ? new(this._subject) : null;

    [JsonPropertyName("grade")]
    public short Grade => (short)this._race.Grade;

    [JsonPropertyName("subjectAge2")]
    public short SubjectAge2 => (short)this._race.SubjectAge2;

    [JsonPropertyName("subjectAge3")]
    public short SubjectAge3 => (short)this._race.SubjectAge3;

    [JsonPropertyName("subjectAge4")]
    public short SubjectAge4 => (short)this._race.SubjectAge4;

    [JsonPropertyName("subjectAge5")]
    public short SubjectAge5 => (short)this._race.SubjectAge5;

    [JsonPropertyName("subjectAgeYoungest")]
    public short SubjectAgeYoungest => (short)this._race.SubjectAgeYounger;

    [JsonPropertyName("course")]
    public short Course => (short)this._race.Course;

    [JsonPropertyName("courseName")]
    public string CourseName => this._race.Course.GetName();

    [JsonPropertyName("courseType")]
    public string CourseType => this._race.CourseType;

    [JsonPropertyName("raceNumber")]
    public short RaceNumber => this._race.CourseRaceNumber;

    [JsonPropertyName("weather")]
    public short Weather => (short)this._race.TrackWeather;

    [JsonPropertyName("condition")]
    public short Condition => (short)this._race.TrackCondition;

    [JsonPropertyName("ground")]
    public short Ground => (short)this._race.TrackGround;

    [JsonPropertyName("type")]
    public short Type => (short)this._race.TrackType;

    [JsonPropertyName("direction")]
    public short Direction => (short)this._race.TrackCornerDirection;

    [JsonPropertyName("distance")]
    public short Distance => this._race.Distance;

    [JsonPropertyName("horsesCount")]
    public short HorsesCount => this._race.HorsesCount;

    [JsonPropertyName("startTime")]
    public string StartTime => this._race.StartTime.ToString();

    [JsonPropertyName("cornerRanking1")]
    public string CornerRanking1 => this._race.Corner1Result;

    [JsonPropertyName("cornerLapTime1")]
    public short CornerLapTime1 => (short)(this._race.Corner1LapTime.TotalSeconds * 10);

    [JsonPropertyName("cornerRanking2")]
    public string CornerRanking2 => this._race.Corner2Result;

    [JsonPropertyName("cornerLapTime2")]
    public short CornerLapTime2 => (short)(this._race.Corner2LapTime.TotalSeconds * 10);

    [JsonPropertyName("cornerRanking3")]
    public string CornerRanking3 => this._race.Corner3Result;

    [JsonPropertyName("cornerLapTime3")]
    public short CornerLapTime3 => (short)(this._race.Corner3LapTime.TotalSeconds * 10);

    [JsonPropertyName("cornerRanking4")]
    public string CornerRanking4 => this._race.Corner4Result;

    [JsonPropertyName("cornerLapTime4")]
    public short CornerLapTime4 => (short)(this._race.Corner4LapTime.TotalSeconds * 10);

    [JsonPropertyName("topHorses")]
    public ScriptRaceHorse[]? TopHorses =>
      this._topHorses != null ? this._topHorses.Select(h => new ScriptRaceHorse(string.Empty, h, false)).ToArray() :
      this._topHorses2 != null ? this._topHorses2.Select(h => new ScriptRaceHorse(string.Empty, h)).ToArray() : null;

    public ScriptRace(RaceData race, IReadOnlyList<RaceHorseAnalyzer>? topHorses = null)
    {
      this._race = race;
      this._subject = new RaceSubjectInfo(race);
      this._topHorses = topHorses;
    }

    public ScriptRace(RaceData race, IReadOnlyList<RaceHorseData>? topHorses)
    {
      this._race = race;
      this._subject = new RaceSubjectInfo(race);
      this._topHorses2 = topHorses;
    }

    [ScriptMember("getJson")]
    public string ToJson()
    {
      return JsonSerializer.Serialize(this, ScriptManager.JsonOptions);
    }
  }
}
