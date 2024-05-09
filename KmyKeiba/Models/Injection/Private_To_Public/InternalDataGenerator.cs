using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Data;
using KmyKeiba.Shared;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Injection.Private
{
  class InternalDataGenerator : IInternalDataGenerator
  {
#if DEBUG
    public static string BaseStandardTimeFileName { get; } = "basestandardtime.dat";
#else
    public static string BaseStandardTimeFileName { get; } = Path.Combine(Constrants.AppDir, "basestandardtime.dat");
#endif

    public async Task GenerateBaseStandardTimeDataAsync()
    {
      using var db = new MyContext();
      var lines = new StringBuilder();

      var standardTimes = await db.RaceStandardTimes!.Where(s => s.Course == RaceCourse.Nakayama).ToArrayAsync();
      foreach (var time in standardTimes)
      {
        lines.Append(time.SampleStartTime.Year).Append(',');
        lines.Append(time.SampleCount).Append(',');
        lines.Append(time.Condition).Append(',');
        lines.Append(time.TrackType).Append(',');
        lines.Append(time.TrackOption).Append(',');
        lines.Append(time.CornerDirection).Append(',');
        lines.Append(time.Average).Append(',');
        lines.Append(time.Median).Append(',');
        lines.Append(time.Deviation).Append(',');
        lines.Append(time.A3FAverage).Append(',');
        lines.Append(time.A3FMedian).Append(',');
        lines.Append(time.A3FDeviation).Append(',');
        lines.Append(time.UntilA3FAverage).Append(',');
        lines.Append(time.UntilA3FMedian).Append(',');
        lines.Append(time.UntilA3FDeviation).Append(',');
        lines.Append(time.Distance).Append(',');
        lines.Append(time.DistanceMax).Append(',');
        lines.Append(time.Ground).Append(',');
        lines.Append(time.Weather).Append(',');
        lines.Append("\r\n");
      }

      await File.WriteAllTextAsync(BaseStandardTimeFileName, lines.ToString());
    }
  }
}
