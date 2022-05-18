using KmyKeiba.JVLink.Entities;
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
  public class ScriptRaceSubject
  {
    private readonly RaceSubjectInfo _subject;

    [JsonPropertyName("allClasses")]
    public short[] AllClasses => this._subject.Subject.AllClasses.Select(c => (short)c).ToArray();

    [JsonPropertyName("maxClass")]
    public short MaxClass => (short)this._subject.Subject.MaxClass;

    [JsonPropertyName("money")]
    public int Money => this._subject.Subject.Money;

    [JsonPropertyName("moneySubjectType")]
    public short MoneySubjectType => (short)this._subject.Subject.MoneySubject;

    [JsonPropertyName("isNewHorses")]
    public bool IsNewHorses => this._subject.Subject.IsNewHorses;

    [JsonPropertyName("isNotWon")]
    public bool IsNotWon => this._subject.Subject.IsNotWon;

    [JsonPropertyName("isLocal")]
    public bool IsLocal => this._subject.Subject.IsLocal;

    [JsonPropertyName("items")]
    public JsonObject[] Items => this._subject.Subject.Items.Select(i =>
    {
      return new JsonObject
      {
        { "cls", (short)i.Class },
        { "level", (short)i.Level },
        { "group", (short)i.Group },
        { "age", (short)i.Age },
        { "classSubject", (short)i.ClassSubject },
      };
    }).ToArray();

    public ScriptRaceSubject(RaceSubjectInfo subject)
    {
      this._subject = subject;
    }

    [ScriptMember("getJson")]
    public string ToJson()
    {
      return JsonSerializer.Serialize(this, ScriptManager.JsonOptions);
    }
  }
}
