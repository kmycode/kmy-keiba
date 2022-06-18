using KmyKeiba.Common;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Analysis.Table;
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
    private List<ScriptAnalysisTableConfiguration> _tables = new();

    [ScriptMember("createAnalysisTable")]
    public ScriptAnalysisTableConfiguration CreateAnalysisTable(string name)
    {
      var table = new ScriptAnalysisTableConfiguration(name);
      this._tables.Add(table);
      return table;
    }

    public void SetConfiguration()
    {
      var configuration = new ApplicationConfiguration();
      foreach (var table in this._tables)
      {
        configuration.AnalysisTableGenerators.Add(table);
      }
      ApplicationConfiguration.Current.Value = configuration;
    }
  }

  [NoDefaultScriptAccess]
  public class ScriptAnalysisTableConfiguration : AnalysisTableGenerator
  {
    private string _name = string.Empty;
    private readonly List<Func<RaceInfo, Task<AnalysisTableRow>>> _rows = new();

    public ScriptAnalysisTableConfiguration(string name)
    {
      this._name = name;
    }

    [ScriptMember("useRaceHorseAnalyzer")]
    public void UseRaceHorseAnalyzer(string name, string keys, string value)
    {
      this._rows.Add(async race =>
      {
        return await new AnalysisTableRow(name, race, (race, horse) =>
          new LambdaAnalysisTableCell<RaceHorseTrendAnalysisSelector, RaceHorseTrendAnalyzer>(
            horse,
            h => h.TrendAnalyzers!,
            keys,
            (analyzer, cell) =>
            {
              if (value == "place")
              {
                cell.Value.Value = analyzer.AllGrade.Value.PlacingBetsRate.ToString("P0") + "\n" + $"({analyzer.AllGrade.Value.PlacingBetsCount} / {analyzer.AllGrade.Value.AllCount})";
                cell.Comparation.Value = analyzer.AllGrade.Value.PlacingBetsRateComparation;
              }
              if (value == "time")
              {
                cell.Value.Value = analyzer.TimeDeviationValue.Value.ToString("F1");
                cell.Comparation.Value = AnalysisUtil.CompareValue(analyzer.TimeDeviationValue.Value, 45, 20);
              }
              if (value == "a3htime")
              {
                cell.Value.Value = analyzer.A3HTimeDeviationValue.Value.ToString("F1");
                cell.Comparation.Value = AnalysisUtil.CompareValue(analyzer.A3HTimeDeviationValue.Value, 45, 20);
              }
            })).WithLoadAsync();
      });
    }

    public override async Task<AnalysisTable> GenerateAsync(RaceInfo race)
    {
      var table = new AnalysisTable(this._name, race.Horses);
      foreach (var row in this._rows)
      {
        table.Rows.Add(await row(race));
      }
      return table;
    }
  }
}
