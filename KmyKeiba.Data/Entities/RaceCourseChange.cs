using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  public class RaceCourseChange : EntityBase
  {
    public string RaceKey { get; set; } = string.Empty;

    public short Distance { get; set; }

    public TrackType TrackType { get; set; }

    public TrackGround TrackGround { get; set; }

    public TrackCornerDirection TrackCornerDirection { get; set; }

    public TrackOption TrackOption { get; set; }

    internal RaceCourseChange()
    {
    }

    public static RaceCourseChange FromJV(JVData_Struct.JV_CC_INFO cc)
    {
      int.TryParse(cc.CCInfoAfter.Kyori, out var distance);
      int.TryParse(cc.CCInfoAfter.TruckCd, out int track);
      var (trackType, trackGround, trackCornerDirection, trackOption) = Race.GetTrackType(track);

      var obj = new RaceCourseChange()
      {
        LastModified = cc.head.MakeDate.ToDateTime(),
        DataStatus = cc.head.DataKubun.ToDataStatus(),
        RaceKey = cc.id.ToRaceKey(),
        Distance = (short)distance,
        TrackType = trackType,
        TrackGround = trackGround,
        TrackCornerDirection = trackCornerDirection,
        TrackOption = trackOption,
      };
      return obj;
    }

    public override int GetHashCode()
      => (this.RaceKey).GetHashCode();
  }
}
