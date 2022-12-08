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

    public int AnalysisTableSourceSize { get; init; } = 1000;

    public int AnalysisTableRaceHorseSourceSize { get; init; } = 4000;

    public int AnalysisTableSampleSize { get; init; } = 10;

    public int DownloadNormalDataIntervalMinutes { get; init; } = 120;

    public bool IsFirstMessageVisible { get; init; } = true;

    public int ExpansionMemoGroupSize { get; init; } = 8;
  }
}
