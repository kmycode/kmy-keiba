using KmyKeiba.Data.Db;
using KmyKeiba.Models.Analysis.Generic;
using KmyKeiba.Models.Data;
using KmyKeiba.Models.Race.Finder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis
{
  public class RaceHorseTrendAnalysisSelectorWrapper : TrendAnalysisSelector<RaceHorseTrendAnalysisSelectorWrapper.Key, RaceHorseTrendAnalyzer>
  {
    public override string Name { get; } = string.Empty;
    public override RaceData Race { get; } = new();
    private readonly RaceFinder _finder;

    public RaceHorseTrendAnalysisSelectorWrapper(RaceFinder finder)
    {
      this._finder = finder;
      base.OnFinishedInitialization();
    }

    protected override RaceHorseTrendAnalyzer GenerateAnalyzer(int sizeMax)
    {
      return new RaceHorseTrendAnalyzer(sizeMax, this.Race, new());
    }

    protected override Task InitializeAnalyzerAsync(MyContext db, IEnumerable<Key> keys, RaceHorseTrendAnalyzer analyzer, int sizeMax, int offset, bool isLoadSameHorses)
    {
      return Task.CompletedTask;
    }

    public RaceHorseTrendAnalyzer BeginLoad(string scriptKey, int sizeMax)
    {
      var analyzer = this.GenerateAnalyzer(sizeMax);
      Task.Run(async () =>
      {
        analyzer.SetSource((await this._finder.FindRaceHorsesAsync(scriptKey, sizeMax)).Items);
      });
      return analyzer;
    }

    public enum Key
    {
    }
  }
}
