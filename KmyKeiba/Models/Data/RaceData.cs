using KmyKeiba.JVLink.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Data
{
  public class RaceData
  {
    public uint Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string SubName { get; set; } = string.Empty;

    public RaceCourse Course { get; set; }

    public int CourseRaceNumber { get; set; }

    public string SubjectName { get; set; } = string.Empty;

    public int HorsesCount { get; set; }

    public DateTime StartTime { get; set; }
  }
}
