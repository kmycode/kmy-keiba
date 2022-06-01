using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis;
using Microsoft.ClearScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Script
{
  [NoDefaultScriptAccess]
  public class ScriptTraining
  {
    private readonly TrainingAnalyzer.TrainingRow _training;

    [JsonPropertyName("center")]
    public short Center => (short)this._training.Center;

    [JsonPropertyName("startTime")]
    public string StartTime => this._training.StartTime.ToString();

    [JsonPropertyName("course")]
    public short Course => (short)this._training.WoodtipCourse;

    [JsonPropertyName("direction")]
    public short Direction => (short)this._training.WoodtipDirection;

    [JsonPropertyName("lapTimes")]
    public short[] LapTimes => this._training.LapTimes.Select(t => t.LapTime).ToArray();

    public ScriptTraining(TrainingAnalyzer.TrainingRow row)
    {
      this._training = row;
    }
  }
}
