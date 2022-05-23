using KmyKeiba.Data.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Injection
{
  public interface ITimeDeviationValueCalculator
  {
    double GetTimeDeviationValue(RaceData race, RaceHorseData horse, RaceStandardTimeMasterData standardTime);

    double GetA3HTimeDeviationValue(RaceData race, RaceHorseData horse, RaceStandardTimeMasterData standardTime);
  }
}
