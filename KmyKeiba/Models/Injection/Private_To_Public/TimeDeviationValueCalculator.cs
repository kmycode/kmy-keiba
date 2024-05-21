using KmyKeiba.Data.Db;
using KmyKeiba.JVLink.Entities;
using KmyKeiba.Models.Analysis;
using KmyKeiba.Models.Analysis.Math;
using KmyKeiba.Models.Data;
using KmyKeiba.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Injection.Private
{
  internal class TimeDeviationValueCalculator : ITimeDeviationValueCalculator
  {
    public async Task<double> GetA3HTimeDeviationValueAsync(RaceData race, RaceHorseData horse, RaceStandardTimeMasterData standardTime)
    {
      if (standardTime.SampleCount == 0 || horse.AfterThirdHalongTime == default)
      {
        return default;
      }

      var value = StatisticSingleArray.CalcDeviationValue(horse.AfterThirdHalongTime.TotalSeconds, standardTime.A3FAverage, standardTime.A3FDeviation);
      return 100 - value;
    }

    public async Task<double> GetUntilA3HTimeDeviationValueAsync(RaceData race, RaceHorseData horse, RaceStandardTimeMasterData standardTime)
    {
      if (standardTime.SampleCount == 0 || horse.AfterThirdHalongTime == default || race.Distance < 800)
      {
        return default;
      }

      var value = StatisticSingleArray.CalcDeviationValue((horse.ResultTime - horse.AfterThirdHalongTime).TotalSeconds / (race.Distance - 600), standardTime.UntilA3FAverage, standardTime.UntilA3FDeviation);
      return 100 - value;
    }

    public async Task<double> GetTimeDeviationValueAsync(RaceData race, RaceHorseData horse, RaceStandardTimeMasterData standardTime)
    {
      if (standardTime.SampleCount == 0 || horse.ResultTime == default || race.Distance == 0)
      {
        return default;
      }

      var resultTimePerMeter = (double)horse.ResultTime.TotalSeconds / race.Distance;

      var value = StatisticSingleArray.CalcDeviationValue(resultTimePerMeter, standardTime.Average, standardTime.Deviation);
      return 100 - value;
    }

    public double GetPciDeviationValue(RaceData race, double pci, RaceStandardTimeMasterData standardTime)
    {
      if (standardTime.SampleCount == 0 || pci == default || race.Distance == 0)
      {
        return default;
      }

      return StatisticSingleArray.CalcDeviationValue(pci, standardTime.PciAverage, standardTime.PciDeviation);
    }
  }
}
