﻿using KmyKeiba.Data.Db;
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
    private readonly RaceInfo _race;

    [JsonPropertyName("name")]
    public string Name => this._race.Data.Name;

    [JsonPropertyName("displayName")]
    public string DisplayName => this._race.Subject.DisplayName;

    [JsonPropertyName("subjectName")]
    public string SubjectName => this._race.Data.SubjectName;

    [JsonPropertyName("subject")]
    public ScriptRaceSubject Subject => new(this._race.Subject);

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
    public string Horses => JsonSerializer.Serialize(this._race.Horses.Select(h => new ScriptRaceHorse(this._race.Data.Key, h)), ScriptManager.JsonOptions);

    public ScriptCurrentRace(RaceInfo race)
    {
      this._race = race;
    }

    [ScriptMember("getHorse")]
    public ScriptRaceHorse? GetHorse(short horseNum)
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

    [ScriptMember("getJson")]
    public string ToJson()
    {
      return JsonSerializer.Serialize(this, ScriptManager.JsonOptions);
    }
  }

  [NoDefaultScriptAccess]
  public class ScriptRace
  {
    private readonly RaceData _race;
    private readonly RaceSubjectInfo _subject;

    [JsonPropertyName("name")]
    public string Name => this._race.Name;

    [JsonPropertyName("displayName")]
    public string DisplayName => this._subject.DisplayName;

    [JsonPropertyName("subjectName")]
    public string SubjectName => this._race.SubjectName;

    [JsonPropertyName("subject")]
    public ScriptRaceSubject Subject => new(this._subject);

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

    public ScriptRace(RaceData race)
    {
      this._race = race;
      this._subject = new RaceSubjectInfo(race);
    }

    [ScriptMember("getJson")]
    public string ToJson()
    {
      return JsonSerializer.Serialize(this, ScriptManager.JsonOptions);
    }
  }
}
