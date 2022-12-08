using KmyKeiba.Common;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Race;
using Microsoft.ClearScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Script
{
  [NoDefaultScriptAccess]
  public class ScriptConfiguration
  {
#pragma warning disable CS0618 // 型またはメンバーが旧型式です
    private List<ScriptAnalysisTableConfiguration> _tables = new();
#pragma warning restore CS0618 // 型またはメンバーが旧型式です

    [ScriptMember("raceCacheMax")]
    public int RaceInfoCacheMax { get; set; } = 48;

    [ScriptMember("distanceDiffCentral")]
    public int NearDistanceDiffCentral { get; init; } = 50;

    [ScriptMember("distanceDiffCentralInHorseGrade")]
    public int NearDistanceDiffCentralInHorseGrade { get; init; } = 50;

    [ScriptMember("distanceDiffLocal")]
    public int NearDistanceDiffLocal { get; init; } = 50;

    [ScriptMember("distanceDiffLocalInHorseGrade")]
    public int NearDistanceDiffLocalInHorseGrade { get; init; } = 50;

    [ScriptMember("analysisTableSourceSize")]
    public int AnalysisTableSourceSize { get; set; } = 1000;

    [ScriptMember("analysisTableRaceHorseSourceSize")]
    public int AnalysisTableRaceHorseSourceSize { get; set; } = 4000;

    [ScriptMember("analysisTableSampleSize")]
    public int AnalysisTableSampleSize { get; set; } = 10;

    [ScriptMember("downloadNormalDataIntervalMinutes")]
    public int DownloadNormalDataIntervalMinutes { get; set; } = 120;

    [ScriptMember("isFirstMessageVisible")]
    public bool IsFirstMessageVisible { get; set; } = true;

    [ScriptMember("expansionMemoGroupSize")]
    public int ExpansionMemoGroupSize { get; set; } = 8;

    [ScriptMember("createAnalysisTable")]
    [Obsolete("構成スクリプトからの分析画面カスタマイズは終了しました")]
    public ScriptAnalysisTableConfiguration CreateAnalysisTable(string name)
    {
      var table = new ScriptAnalysisTableConfiguration(name);
      this._tables.Add(table);
      return table;
    }

    public void SetConfiguration()
    {
      var configuration = new ApplicationConfiguration
      {
        RaceInfoCacheMax = this.RaceInfoCacheMax,
        NearDistanceDiffCentral = this.NearDistanceDiffCentral,
        NearDistanceDiffCentralInHorseGrade = this.NearDistanceDiffCentralInHorseGrade,
        NearDistanceDiffLocal = this.NearDistanceDiffLocal,
        NearDistanceDiffLocalInHorseGrade = this.NearDistanceDiffLocalInHorseGrade,
        AnalysisTableSourceSize = this.AnalysisTableSourceSize,
        AnalysisTableRaceHorseSourceSize = this.AnalysisTableRaceHorseSourceSize,
        AnalysisTableSampleSize = this.AnalysisTableSampleSize,
        DownloadNormalDataIntervalMinutes = this.DownloadNormalDataIntervalMinutes,
        IsFirstMessageVisible = this.IsFirstMessageVisible,
        ExpansionMemoGroupSize = this.ExpansionMemoGroupSize,
      };
      ApplicationConfiguration.Current.Value = configuration;
    }
  }

  [Obsolete("削除された機能")]
  [NoDefaultScriptAccess]
  public class ScriptAnalysisTableConfiguration
  {
    public ScriptAnalysisTableConfiguration(string name)
    {
    }

    [ScriptMember("useHorseAnalyzer")]
    public void UseHorseAnalyzer(string name, string keys, string value)
    {
    }

    [ScriptMember("useRiderAnalyzer")]
    public void UseRiderAnalyzer(string name, string keys, string value)
    {
    }

    [ScriptMember("useTrainerAnalyzer")]
    public void UseTrainerAnalyzer(string name, string keys, string value)
    {
    }

    [ScriptMember("useBloodAnalyzer")]
    public void UseRiderAnalyzer(string name, string type, string keys, string value)
    {
    }

    [ScriptMember("useFinder")]
    public void UseFinder(string name, string keys, string value)
    {
    }

    [ScriptMember("useRaceHorseAnalyzer")]
    public void UseRaceHorseAnalyzer(string name, string target, string keys, string value)
    {
    }
  }
}
