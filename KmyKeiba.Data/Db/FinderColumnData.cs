using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  public class FinderColumnData : AppDataBase
  {
    public uint TabGroup { get; set; }

    public uint Order { get; set; }

    public FinderColumnProperty Property { get; set; }
  }
  
  public enum FinderColumnProperty : short
  {
    RaceSubject = 1,
    RaceName = 2,
    StartTime = 3,
    Course = 4,
    CourseInfo = 5,
    HorseName = 6,
    Popular = 7,
    ResultOrder = 8,
    ResultTime = 9,
    After3HalongTime = 10,
    RunningStyle = 11,
    CornerOrders = 12,
    RiderName = 13,
    Before3HalongTime = 14,
    Before3HalongTimeNormalized = 15,
    Before4HalongTime = 16,
    After4HalongTime = 17,
    Pci = 18,
    Rpci = 19,
    Pci3 = 20,
    SinglePayoff = 21,
    FramePayoff = 22,
    QuinellaPayoff = 23,
    ExactaPayoff = 24,
    TrioPayoff = 25,
    TrifectaPayoff = 26,
    LapTime1 = 27,
    LapTime2 = 28,
    LapTime3 = 29,
    LapTime4 = 30,
    LapTime5 = 31,
    LapTime6 = 32,
    LapTime7 = 33,
    LapTime8 = 34,
    LapTime9 = 35,
    LapTime10 = 36,
    LapTime11 = 37,
    LapTime12 = 38,
    LapTime13 = 39,
    LapTime14 = 40,
    LapTime15 = 41,
    LapTime16 = 42,
    LapTime17 = 43,
    LapTime18 = 44,
    Empty = 45,
    HorseMark = 46,
    Weight = 47,
    WeightDiff = 48,
    RiderWeight = 49,
    Age = 50,
    Sex = 51,
    HorsesCount = 52,
    Weather = 53,
    Condition = 54,
    GoalOrder = 55,
    SingleOdds = 56,
    PlaceOddsMin = 57,
    PlaceOddsMax = 58,
    RacePace = 59,
  }
}
