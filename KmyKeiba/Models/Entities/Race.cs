using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Entities
{
  class Race
  {
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// レースの名前
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// レースの副題
    /// </summary>
    public string SubName { get; init; } = string.Empty;

    /// <summary>
    /// 参加条件の名前
    /// </summary>
    public string SubjectName { get; init; } = string.Empty;

    /// <summary>
    /// 出走日時
    /// </summary>
    public DateTime StartTime { get; init; }

    public static Race FromJV(JVLib.JVData_Struct.JV_RA_RACE race)
    {
      var name = race.RaceInfo.Hondai.Trim();
      if (string.IsNullOrEmpty(name))
      {
        name = race.JyokenName.Trim();
      }

      var startTime = DateTime.ParseExact($"{race.id.Year}{race.id.MonthDay}{race.HassoTime}", "yyyyMMddHHmm", null);

      var obj = new Race
      {
        Id = race.id.ToRaceId(),
        Name = name,
        SubName = race.RaceInfo.Fukudai.Trim(),
        SubjectName = race.JyokenName.Trim(),
        StartTime = startTime,
      };
      return obj;
    }
  }
}
