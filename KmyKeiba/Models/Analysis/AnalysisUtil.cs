using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Analysis
{
  internal static class AnalysisUtil
  {
    public static RunningStyleInfo GetRunningStyleInfo(RaceData race, RaceHorseData horse)
    {
      var cornerOrders = horse.GetCornerOrders();
      if (!cornerOrders.Any())
      {
        return default;
      }

      var result = horse.ResultOrder >= 1 && horse.ResultOrder <= 3 ? RunningStyleResult.Succeed :
        horse.ResultOrder >= 4 && horse.ResultOrder <= 5 ? RunningStyleResult.Partial : RunningStyleResult.Failed;

      var cornerGroups = cornerOrders.Select(o => GetHorseGroupNumber(o, race.HorsesCount)).ToArray();
      var groupOrderDiff = cornerGroups.Select((g, i) => i <= 0 ? 0 : cornerGroups[i - 1] - cornerGroups[i]).ToArray();

      var stalledIndex = 4;
      var diffMax = groupOrderDiff.Max();
      foreach (var diff in groupOrderDiff.Reverse())
      {
        if (diff == diffMax)
        {
          break;
        }
        stalledIndex--;
      }
      var stalledPosition = stalledIndex == 4 ? CoursePosition.Corner4 :
        stalledIndex == 3 ? CoursePosition.Corner3 :
        stalledIndex == 2 ? CoursePosition.Corner2 :
        stalledIndex == 1 ? CoursePosition.Corner1 : CoursePosition.Unknown;
      if (horse.ResultOrder > 4 && cornerOrders.Last() + 3 <= horse.ResultOrder)
      {
        stalledPosition = CoursePosition.LastLine;
      }
      if (horse.ResultOrder <= 3)
      {
        stalledPosition = CoursePosition.Unknown;
      }

      var style = cornerGroups[0] switch
      {
        0 => RunningStyle.FrontRunner,
        1 => RunningStyle.Stalker,
        2 => RunningStyle.Sotp,
        3 => RunningStyle.SaveRunner,
        _ => RunningStyle.Unknown,
      };
      if ((style == RunningStyle.Sotp || style == RunningStyle.SaveRunner) && horse.ResultOrder >= race.HorsesCount * 0.7f)
      {
        style = RunningStyle.NotClear;
      }

      return new RunningStyleInfo(style, result, stalledPosition);
    }

    private static int GetHorseGroupNumber(int order, int raceHorsesCount)
    {
      var split = 4.0f / raceHorsesCount;
      return (int)((order - 1) * split);
    }

    private static int[] GetCornerOrders(this RaceHorseData horse)
    {
      var cornerOrders =
        new int[] { horse.FirstCornerOrder, horse.SecondCornerOrder, horse.ThirdCornerOrder, horse.FourthCornerOrder, }
        .Where(v => v != 0)
        .ToArray();
      return cornerOrders;
    }

    internal record struct RunningStyleInfo(
      RunningStyle RunningStyle, RunningStyleResult RunningStyleResult, CoursePosition StallPosition);
  }
}
