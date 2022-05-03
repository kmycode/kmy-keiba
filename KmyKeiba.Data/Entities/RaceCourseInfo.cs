using KmyKeiba.Data.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  public class RaceCourseInfo
  {
    public string Name
    {
      get
      {
        return this.Course.GetAttribute()?.Name ?? string.Empty;
      }
    }

    public DateTime? StartUsingDate { get; init; }

    public DateTime? EndUsingDate { get; init; }

    public RaceCourse Course { get; init; }

    public TrackGround Ground { get; init; }

    public string CourseName { get; init; } = string.Empty;

    public short Length { get; init; }

    public short LastStraightLineLength { get; init; }

    public TrackSlope Corner1Slope { get; init; }

    public TrackSlope Corner2Slope { get; init; }

    public TrackSlope Corner23LineSlope { get; init; }

    public TrackSlope Corner3Slope { get; init; }

    public TrackSlope Corner4Slope { get; init; }

    public TrackSlope FirstLineSlope { get; init; }

    public TrackSlope LastLineSlope { get; init; }

    public TrackOption Option { get; init; }

    public TrackCornerDirection Direction { get; init; }
  }

  public static class RaceCourses
  {
    public static RaceCourseInfo? TryGetCourse(Race race)
    {
      var list = Courses.Where(c => c.Course == race.Course);
      list = list.Where(c => c.Ground == race.TrackGround);
      list = list.Where(c => c.Option == TrackOption.Unknown || c.Option == race.TrackOption);
      list = list.Where(c => string.IsNullOrEmpty(c.CourseName) || c.CourseName == race.CourseName);
      list = list.Where(c => c.Direction == TrackCornerDirection.Unknown || c.Direction == race.TrackCornerDirection);
      list = list.Where(c => c.StartUsingDate == null || c.StartUsingDate <= race.StartTime);
      list = list.Where(c => c.EndUsingDate == null || c.EndUsingDate > race.StartTime);

      return list.FirstOrDefault();
    }

    public static IReadOnlyList<RaceCourseInfo> Courses { get; } = new List<RaceCourseInfo>
    {
      new()
      {
        Course = RaceCourse.Sapporo, Ground = TrackGround.Turf, CourseName = "A",
        Length = 1640, LastStraightLineLength = 266,
      },
      new()
      {
        Course = RaceCourse.Sapporo, Ground = TrackGround.Turf, CourseName = "B",
        Length = 1650, LastStraightLineLength = 267,
      },
      new()
      {
        Course = RaceCourse.Sapporo, Ground = TrackGround.Turf, CourseName = "C",
        Length = 1659, LastStraightLineLength = 269,
      },
      new()
      {
        Course = RaceCourse.Sapporo, Ground = TrackGround.Dirt,
        Length = 1487, LastStraightLineLength = 264,
      },
      new()
      {
        Course = RaceCourse.Hakodate, Ground = TrackGround.Turf, CourseName = "A",
        Length = 1626, LastStraightLineLength = 262,
        Corner1Slope = TrackSlope.Downhill, Corner23LineSlope = TrackSlope.Uphill,
        Corner3Slope = TrackSlope.Uphill, Corner4Slope = TrackSlope.Downhill,
      },
      new()
      {
        Course = RaceCourse.Hakodate, Ground = TrackGround.Turf, CourseName = "B",
        Length = 1651, LastStraightLineLength = 262,
        Corner1Slope = TrackSlope.Downhill, Corner23LineSlope = TrackSlope.Uphill,
        Corner3Slope = TrackSlope.Uphill, Corner4Slope = TrackSlope.Downhill,
      },
      new()
      {
        Course = RaceCourse.Hakodate, Ground = TrackGround.Turf, CourseName = "C",
        Length = 1675, LastStraightLineLength = 264,
        Corner1Slope = TrackSlope.Downhill, Corner23LineSlope = TrackSlope.Uphill,
        Corner3Slope = TrackSlope.Uphill, Corner4Slope = TrackSlope.Downhill,
      },
      new()
      {
        Course = RaceCourse.Hakodate, Ground = TrackGround.Dirt,
        Length = 1475, LastStraightLineLength = 260,
        Corner1Slope = TrackSlope.Downhill, Corner23LineSlope = TrackSlope.Uphill,
        Corner3Slope = TrackSlope.Uphill, Corner4Slope = TrackSlope.Downhill,
      },
      new()
      {
        Course = RaceCourse.Fukushima, Ground = TrackGround.Turf, CourseName = "A",
        Length = 1600, LastStraightLineLength = 292,
        Corner1Slope = TrackSlope.Downhill, Corner23LineSlope = TrackSlope.Uphill,
        LastLineSlope = TrackSlope.DownToUphill,
      },
      new()
      {
        Course = RaceCourse.Fukushima, Ground = TrackGround.Turf, CourseName = "B",
        Length = 1614, LastStraightLineLength = 297,
        Corner1Slope = TrackSlope.Downhill, Corner23LineSlope = TrackSlope.Uphill,
        LastLineSlope = TrackSlope.DownToUphill,
      },
      new()
      {
        Course = RaceCourse.Fukushima, Ground = TrackGround.Turf, CourseName = "C",
        Length = 1628, LastStraightLineLength = 299,
        Corner1Slope = TrackSlope.Downhill, Corner23LineSlope = TrackSlope.Uphill,
        LastLineSlope = TrackSlope.DownToUphill,
      },
      new()
      {
        Course = RaceCourse.Fukushima, Ground = TrackGround.Dirt,
        Length = 1444, LastStraightLineLength = 295,
        Corner1Slope = TrackSlope.Downhill, Corner23LineSlope = TrackSlope.Uphill,
        LastLineSlope = TrackSlope.DownToUphill,
      },
      new()
      {
        Course = RaceCourse.Niigata, Ground = TrackGround.Turf, CourseName = "A",
        Length = 1623, LastStraightLineLength = 358, Option = TrackOption.Inside,
        Corner3Slope = TrackSlope.Uphill, Corner2Slope = TrackSlope.Downhill,
      },
      new()
      {
        Course = RaceCourse.Niigata, Ground = TrackGround.Turf, CourseName = "A",
        Length = 2223, LastStraightLineLength = 658, Option = TrackOption.Outside,
        Corner4Slope = TrackSlope.Uphill, Corner3Slope = TrackSlope.Uphill,
        Corner23LineSlope = TrackSlope.Downhill, Corner2Slope = TrackSlope.Downhill,
      },
      new()
      {
        Course = RaceCourse.Niigata, Ground = TrackGround.Turf, CourseName = "B",
        Length = 1648, LastStraightLineLength = 358, Option = TrackOption.Inside,
        Corner3Slope = TrackSlope.Uphill, Corner2Slope = TrackSlope.Downhill,
      },
      new()
      {
        Course = RaceCourse.Niigata, Ground = TrackGround.Turf, CourseName = "B",
        Length = 2248, LastStraightLineLength = 658, Option = TrackOption.Outside,
        Corner4Slope = TrackSlope.Uphill, Corner3Slope = TrackSlope.Uphill,
        Corner23LineSlope = TrackSlope.Downhill, Corner2Slope = TrackSlope.Downhill,
      },
      new()
      {
        Course = RaceCourse.Niigata, Ground = TrackGround.Dirt,
        Length = 1472, LastStraightLineLength = 353,
        Corner3Slope = TrackSlope.Uphill, Corner2Slope = TrackSlope.Downhill,
      },
      new()
      {
        Course = RaceCourse.Tokyo, Ground = TrackGround.Turf, CourseName = "A",
        Length = 2083, LastStraightLineLength = 525,
        LastLineSlope = TrackSlope.Downhill, Corner4Slope = TrackSlope.Downhill,
        Corner3Slope = TrackSlope.DownToUphill, Corner23LineSlope = TrackSlope.UpToDownhill,
        Corner2Slope = TrackSlope.Uphill, Corner1Slope = TrackSlope.Uphill,
      },
      new()
      {
        Course = RaceCourse.Tokyo, Ground = TrackGround.Turf, CourseName = "B",
        Length = 2101, LastStraightLineLength = 525,
        LastLineSlope = TrackSlope.Downhill, Corner4Slope = TrackSlope.Downhill,
        Corner3Slope = TrackSlope.DownToUphill, Corner23LineSlope = TrackSlope.UpToDownhill,
        Corner2Slope = TrackSlope.Uphill, Corner1Slope = TrackSlope.Uphill,
      },
      new()
      {
        Course = RaceCourse.Tokyo, Ground = TrackGround.Turf, CourseName = "C",
        Length = 2120, LastStraightLineLength = 525,
        LastLineSlope = TrackSlope.Downhill, Corner4Slope = TrackSlope.Downhill,
        Corner3Slope = TrackSlope.DownToUphill, Corner23LineSlope = TrackSlope.UpToDownhill,
        Corner2Slope = TrackSlope.Uphill, Corner1Slope = TrackSlope.Uphill,
      },
      new()
      {
        Course = RaceCourse.Tokyo, Ground = TrackGround.Turf, CourseName = "D",
        Length = 2139, LastStraightLineLength = 525,
        LastLineSlope = TrackSlope.Downhill, Corner4Slope = TrackSlope.Downhill,
        Corner3Slope = TrackSlope.DownToUphill, Corner23LineSlope = TrackSlope.UpToDownhill,
        Corner2Slope = TrackSlope.Uphill, Corner1Slope = TrackSlope.Uphill,
      },
      new()
      {
        Course = RaceCourse.Tokyo, Ground = TrackGround.Dirt,
        Length = 1899, LastStraightLineLength = 501,
        LastLineSlope = TrackSlope.Downhill, Corner4Slope = TrackSlope.Downhill,
        Corner3Slope = TrackSlope.DownToUphill, Corner23LineSlope = TrackSlope.UpToDownhill,
        Corner2Slope = TrackSlope.Uphill, Corner1Slope = TrackSlope.Uphill,
      },
      new()
      {
        Course = RaceCourse.Nakayama, Ground = TrackGround.Turf, CourseName = "A",
        Length = 1667, LastStraightLineLength = 310, Option = TrackOption.Inside,
        FirstLineSlope = TrackSlope.Uphill, Corner1Slope = TrackSlope.Uphill,
        Corner2Slope = TrackSlope.Downhill, Corner23LineSlope = TrackSlope.Downhill,
        Corner4Slope = TrackSlope.Downhill, LastLineSlope = TrackSlope.LargeUphill,
      },
      new()
      {
        Course = RaceCourse.Nakayama, Ground = TrackGround.Turf, CourseName = "A",
        Length = 1839, LastStraightLineLength = 310, Option = TrackOption.Outside,
        FirstLineSlope = TrackSlope.Uphill, Corner1Slope = TrackSlope.Uphill,
        Corner2Slope = TrackSlope.Downhill, Corner23LineSlope = TrackSlope.Downhill,
        Corner4Slope = TrackSlope.Downhill, LastLineSlope = TrackSlope.LargeUphill, Corner3Slope = TrackSlope.Downhill,
      },
      new()
      {
        Course = RaceCourse.Nakayama, Ground = TrackGround.Turf, CourseName = "B",
        Length = 1686, LastStraightLineLength = 310, Option = TrackOption.Inside,
        FirstLineSlope = TrackSlope.Uphill, Corner1Slope = TrackSlope.Uphill,
        Corner2Slope = TrackSlope.Downhill, Corner23LineSlope = TrackSlope.Downhill,
        Corner4Slope = TrackSlope.Downhill, LastLineSlope = TrackSlope.LargeUphill,
      },
      new()
      {
        Course = RaceCourse.Nakayama, Ground = TrackGround.Turf, CourseName = "B",
        Length = 1858, LastStraightLineLength = 310, Option = TrackOption.Outside,
        FirstLineSlope = TrackSlope.Uphill, Corner1Slope = TrackSlope.Uphill,
        Corner2Slope = TrackSlope.Downhill, Corner23LineSlope = TrackSlope.Downhill,
        Corner4Slope = TrackSlope.Downhill, LastLineSlope = TrackSlope.LargeUphill, Corner3Slope = TrackSlope.Downhill,
      },
      new()
      {
        Course = RaceCourse.Nakayama, Ground = TrackGround.Turf, CourseName = "C",
        Length = 1704, LastStraightLineLength = 310, Option = TrackOption.Inside,
        FirstLineSlope = TrackSlope.Uphill, Corner1Slope = TrackSlope.Uphill,
        Corner2Slope = TrackSlope.Downhill, Corner23LineSlope = TrackSlope.Downhill,
        Corner4Slope = TrackSlope.Downhill, LastLineSlope = TrackSlope.LargeUphill,
      },
      new()
      {
        Course = RaceCourse.Nakayama, Ground = TrackGround.Turf, CourseName = "C",
        Length = 1877, LastStraightLineLength = 310, Option = TrackOption.Outside,
        FirstLineSlope = TrackSlope.Uphill, Corner1Slope = TrackSlope.Uphill,
        Corner2Slope = TrackSlope.Downhill, Corner23LineSlope = TrackSlope.Downhill,
        Corner4Slope = TrackSlope.Downhill, LastLineSlope = TrackSlope.LargeUphill, Corner3Slope = TrackSlope.Downhill,
      },
      new()
      {
        Course = RaceCourse.Nakayama, Ground = TrackGround.Dirt,
        Length = 1493, LastStraightLineLength = 308,
        FirstLineSlope = TrackSlope.Uphill, Corner1Slope = TrackSlope.Uphill,
        Corner2Slope = TrackSlope.Downhill, Corner23LineSlope = TrackSlope.Downhill,
        Corner4Slope = TrackSlope.Downhill, LastLineSlope = TrackSlope.LargeUphill,
      },
      new()
      {
        Course = RaceCourse.Chukyo, Ground = TrackGround.Turf, CourseName = "A",
        Length = 1705, LastStraightLineLength = 412,
        Corner2Slope = TrackSlope.Uphill, Corner23LineSlope = TrackSlope.UpToDownhill,
        Corner3Slope = TrackSlope.Downhill, Corner4Slope = TrackSlope.Downhill,
        LastLineSlope = TrackSlope.LargeUphill,
      },
      new()
      {
        Course = RaceCourse.Chukyo, Ground = TrackGround.Turf, CourseName = "B",
        Length = 1724, LastStraightLineLength = 412,
        Corner2Slope = TrackSlope.Uphill, Corner23LineSlope = TrackSlope.UpToDownhill,
        Corner3Slope = TrackSlope.Downhill, Corner4Slope = TrackSlope.Downhill,
        LastLineSlope = TrackSlope.LargeUphill,
      },
      new()
      {
        Course = RaceCourse.Chukyo, Ground = TrackGround.Dirt,
        Length = 1530, LastStraightLineLength = 410,
        Corner2Slope = TrackSlope.Uphill, Corner23LineSlope = TrackSlope.UpToDownhill,
        Corner3Slope = TrackSlope.Downhill, Corner4Slope = TrackSlope.Downhill,
        LastLineSlope = TrackSlope.LargeUphill,
      },
      new()
      {
        Course = RaceCourse.Kyoto, Ground = TrackGround.Turf, CourseName = "A",
        Length = 1782, LastStraightLineLength = 328, Option = TrackOption.Inside,
        Corner23LineSlope = TrackSlope.LargeUphill, Corner3Slope = TrackSlope.LargeDownhill
      },
      new()
      {
        Course = RaceCourse.Kyoto, Ground = TrackGround.Turf, CourseName = "A",
        Length = 1894, LastStraightLineLength = 403, Option = TrackOption.Outside,
        Corner23LineSlope = TrackSlope.LargeUphill, Corner3Slope = TrackSlope.LargeDownhill
      },
      new()
      {
        Course = RaceCourse.Kyoto, Ground = TrackGround.Turf, CourseName = "B",
        Length = 1802, LastStraightLineLength = 323, Option = TrackOption.Inside,
        Corner23LineSlope = TrackSlope.LargeUphill, Corner3Slope = TrackSlope.LargeDownhill
      },
      new()
      {
        Course = RaceCourse.Kyoto, Ground = TrackGround.Turf, CourseName = "B",
        Length = 1913, LastStraightLineLength = 398, Option = TrackOption.Outside,
        Corner23LineSlope = TrackSlope.LargeUphill, Corner3Slope = TrackSlope.LargeDownhill
      },
      new()
      {
        Course = RaceCourse.Kyoto, Ground = TrackGround.Turf, CourseName = "C",
        Length = 1821, LastStraightLineLength = 323, Option = TrackOption.Inside,
        Corner23LineSlope = TrackSlope.LargeUphill, Corner3Slope = TrackSlope.LargeDownhill
      },
      new()
      {
        Course = RaceCourse.Kyoto, Ground = TrackGround.Turf, CourseName = "C",
        Length = 1932, LastStraightLineLength = 398, Option = TrackOption.Outside,
        Corner23LineSlope = TrackSlope.LargeUphill, Corner3Slope = TrackSlope.LargeDownhill
      },
      new()
      {
        Course = RaceCourse.Kyoto, Ground = TrackGround.Turf, CourseName = "D",
        Length = 1839, LastStraightLineLength = 323, Option = TrackOption.Inside,
        Corner23LineSlope = TrackSlope.LargeUphill, Corner3Slope = TrackSlope.LargeDownhill
      },
      new()
      {
        Course = RaceCourse.Kyoto, Ground = TrackGround.Turf, CourseName = "D",
        Length = 1951, LastStraightLineLength = 398, Option = TrackOption.Outside,
        Corner23LineSlope = TrackSlope.LargeUphill, Corner3Slope = TrackSlope.LargeDownhill
      },
      new()
      {
        Course = RaceCourse.Kyoto, Ground = TrackGround.Dirt,
        Length = 1607, LastStraightLineLength = 329,
        Corner23LineSlope = TrackSlope.LargeUphill, Corner3Slope = TrackSlope.LargeDownhill
      },
      new()
      {
        Course = RaceCourse.Hanshin, Ground = TrackGround.Turf, CourseName = "A",
        Length = 1689, LastStraightLineLength = 356, Option = TrackOption.Inside,
        Corner3Slope = TrackSlope.Downhill,
        Corner4Slope = TrackSlope.Downhill, LastLineSlope = TrackSlope.DownToUphill,
      },
      new()
      {
        Course = RaceCourse.Hanshin, Ground = TrackGround.Turf, CourseName = "A",
        Length = 2089, LastStraightLineLength = 473, Option = TrackOption.Outside,
        Corner4Slope = TrackSlope.Downhill, LastLineSlope = TrackSlope.DownToUphill,
      },
      new()
      {
        Course = RaceCourse.Hanshin, Ground = TrackGround.Turf, CourseName = "B",
        Length = 1713, LastStraightLineLength = 359, Option = TrackOption.Inside,
        Corner3Slope = TrackSlope.Downhill,
        Corner4Slope = TrackSlope.Downhill, LastLineSlope = TrackSlope.DownToUphill,
      },
      new()
      {
        Course = RaceCourse.Hanshin, Ground = TrackGround.Turf, CourseName = "B",
        Length = 2113, LastStraightLineLength = 476, Option = TrackOption.Outside,
        Corner4Slope = TrackSlope.Downhill, LastLineSlope = TrackSlope.DownToUphill,
      },
      new()
      {
        Course = RaceCourse.Hanshin, Ground = TrackGround.Dirt,
        Length = 1517, LastStraightLineLength = 352,
        Corner23LineSlope = TrackSlope.Downhill, Corner3Slope = TrackSlope.Downhill,
        Corner4Slope = TrackSlope.Downhill, LastLineSlope = TrackSlope.Uphill,
      },
      new()
      {
        Course = RaceCourse.Kokura, Ground = TrackGround.Turf, CourseName = "A",
        Length = 1615, LastStraightLineLength = 293,
        FirstLineSlope = TrackSlope.Uphill, Corner1Slope = TrackSlope.Uphill,
        Corner2Slope = TrackSlope.Downhill, Corner3Slope = TrackSlope.Downhill,
      },
      new()
      {
        Course = RaceCourse.Kokura, Ground = TrackGround.Turf, CourseName = "B",
        Length = 1633, LastStraightLineLength = 293,
        FirstLineSlope = TrackSlope.Uphill, Corner1Slope = TrackSlope.Uphill,
        Corner2Slope = TrackSlope.Downhill, Corner3Slope = TrackSlope.Downhill,
      },
      new()
      {
        Course = RaceCourse.Kokura, Ground = TrackGround.Turf, CourseName = "C",
        Length = 1652, LastStraightLineLength = 293,
        FirstLineSlope = TrackSlope.Uphill, Corner1Slope = TrackSlope.Uphill,
        Corner2Slope = TrackSlope.Downhill, Corner3Slope = TrackSlope.Downhill,
      },
      new()
      {
        Course = RaceCourse.Kokura, Ground = TrackGround.Dirt,
        Length = 1445, LastStraightLineLength = 291,
        Corner1Slope = TrackSlope.Uphill,
        Corner2Slope = TrackSlope.Downhill, Corner3Slope = TrackSlope.Downhill,
      },
      new()
      {
        Course = RaceCourse.Mombetsu, Option = TrackOption.Inside,
        Length = 1376, LastStraightLineLength = 218,
      },
      new()
      {
        Course = RaceCourse.Mombetsu, Option = TrackOption.Outside,
        Length = 1600, LastStraightLineLength = 330,
      },
      new()
      {
        Course = RaceCourse.Morioka, Ground = TrackGround.Turf,
        Length = 1400, LastStraightLineLength = 300,
        Corner23LineSlope = TrackSlope.Uphill, Corner3Slope = TrackSlope.LargeDownhill,
        Corner4Slope = TrackSlope.LargeDownhill, LastLineSlope = TrackSlope.LargeUphill,
      },
      new()
      {
        Course = RaceCourse.Morioka, Ground = TrackGround.Dirt,
        Length = 1600, LastStraightLineLength = 300,
        Corner23LineSlope = TrackSlope.Uphill, Corner3Slope = TrackSlope.LargeDownhill,
        Corner4Slope = TrackSlope.LargeDownhill, LastLineSlope = TrackSlope.LargeUphill,
      },
      new()
      {
        Course = RaceCourse.Mizusawa,
        Length = 1200, LastStraightLineLength = 245,
      },
      new()
      {
        Course = RaceCourse.Urawa,
        Length = 1200, LastStraightLineLength = 220,
      },
      new()
      {
        Course = RaceCourse.Funabashi, Option = TrackOption.Inside,
        Length = 1250, LastStraightLineLength = 308,
      },
      new()
      {
        Course = RaceCourse.Funabashi, Option = TrackOption.Outside,
        Length = 1400, LastStraightLineLength = 308,
      },
      new()
      {
        Course = RaceCourse.Oi, Option = TrackOption.Inside, Direction = TrackCornerDirection.Right,
        Length = 1400, LastStraightLineLength = 286,
      },
      new()
      {
        Course = RaceCourse.Oi, Option = TrackOption.Outside, Direction = TrackCornerDirection.Left,
        Length = 1600, LastStraightLineLength = 300,
      },
      new()
      {
        Course = RaceCourse.Oi, Option = TrackOption.Outside, Direction = TrackCornerDirection.Right,
        Length = 1600, LastStraightLineLength = 386,
      },
      new()
      {
        Course = RaceCourse.Kawazaki,
        Length = 1200, LastStraightLineLength = 300,
      },
      new()
      {
        Course = RaceCourse.Kanazawa,
        Length = 1200, LastStraightLineLength = 236,
      },
      new()
      {
        Course = RaceCourse.Kasamatsu,
        Length = 1100, LastStraightLineLength = 201,
      },
      new()
      {
        Course = RaceCourse.Nagoya,
        Length = 1180, LastStraightLineLength = 240,
      },
      new()
      {
        Course = RaceCourse.Sonoda,
        Length = 1051, LastStraightLineLength = 213,
        Corner23LineSlope = TrackSlope.Uphill,
      },
      new()
      {
        Course = RaceCourse.Himeji,
        Length = 1200, LastStraightLineLength = 230,
      },
      new()
      {
        Course = RaceCourse.Kochi,
        Length = 1100, LastStraightLineLength = 200,
      },
      new()
      {
        Course = RaceCourse.Saga,
        Length = 1100, LastStraightLineLength = 200,
      },

      // 1990年以降に廃止された競馬場。確認してないけどNVLinkにデータが残ってるかもしれないので
      new()
      {
        EndUsingDate = new DateTime(2005, 3, 14).AddDays(1),
        Course = RaceCourse.Utsunomiya,
        Length = 1300, LastStraightLineLength = 200,
      },
      new()
      {
        EndUsingDate = new DateTime(2004, 12, 31).AddDays(1),
        Course = RaceCourse.Takasaki,
        Length = 1200, LastStraightLineLength = 300,
      },
      new()
      {
        EndUsingDate = new DateTime(2001, 8, 16).AddDays(1),
        Course = RaceCourse.Sanjo,
        Length = 1000, LastStraightLineLength = 195,
      },
      new()
      {
        EndUsingDate = new DateTime(2001, 6, 3).AddDays(1),
        Course = RaceCourse.Nakatsu,
        Length = 1000, LastStraightLineLength = 195,
      },
      new()
      {
        EndUsingDate = new DateTime(2002, 8, 31).AddDays(1),    // 日付は仮。月までしかわからなかった
        Course = RaceCourse.Masuda,
        Length = 1000, LastStraightLineLength = 200,
      },
      new()
      {
        EndUsingDate = new DateTime(2003, 3, 31).AddDays(1),    // 日付は仮。月までしかわからなかった
        Course = RaceCourse.Ashikaka,
        Length = 1100, LastStraightLineLength = 237,
      },
      new()
      {
        EndUsingDate = new DateTime(2003, 11, 30).AddDays(1),
        Course = RaceCourse.Kaminoyama,
        Length = 1050, LastStraightLineLength = 200,    // 直線の数字は仮。調べたが不明
      },
      new()
      {
        EndUsingDate = new DateTime(2006, 10, 2).AddDays(1),
        Course = RaceCourse.Iwamizawa,
        Length = 1200, LastStraightLineLength = 270,
      },
      new()
      {
        EndUsingDate = new DateTime(2008, 10, 16).AddDays(1),
        Course = RaceCourse.Asahikawa,
        Length = 1300, LastStraightLineLength = 262,
      },
      new()
      {
        EndUsingDate = new DateTime(2011, 12, 23).AddDays(1),
        Course = RaceCourse.Arao,
        Length = 1200, LastStraightLineLength = 220,
      },
      new()
      {
        EndUsingDate = new DateTime(2013, 3, 24).AddDays(1),
        Course = RaceCourse.Fukuyama,
        Length = 1000, LastStraightLineLength = 200,
      },
    };
  }

  public enum TrackSlope
  {
    Flat,
    Uphill,
    LargeUphill,
    Downhill,
    LargeDownhill,
    UpToDownhill,
    DownToUphill,
  }
}
