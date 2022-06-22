using KmyKeiba.Models.Analysis.Table;
using KmyKeiba.Models.Race;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Common
{
  internal class ApplicationConfiguration
  {
    public static ReactiveProperty<ApplicationConfiguration> Current { get; } = new(new ApplicationConfiguration());

    public int RaceInfoCacheMax { get; init; } = 48;

    public int NearDistanceDiffCentral { get; init; } = 50;

    public int NearDistanceDiffCentralInHorseGrade { get; init; } = 50;

    public int NearDistanceDiffLocal { get; init; } = 50;

    public int NearDistanceDiffLocalInHorseGrade { get; init; } = 50;

    public List<AnalysisTableGenerator> AnalysisTableGenerators { get; } = new();
  }

  public abstract class AnalysisTableGenerator
  {
    public abstract Task<AnalysisTable> GenerateAsync(RaceInfo race);
  }
}
