using KmyKeiba.JVLink.Wrappers.JVLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.JVLink.Entities
{
  public class Rider : EntityBase
  {
    public string Code { get; set; } = string.Empty;

    public bool IsCentral => this.Belongs != HorseBelongs.Local;

    public short CentralFlag => this.IsCentral ? (short)1 : (short)0;

    public bool IsDeleted { get; set; }

    public HorseBelongs Belongs { get; set; }

    public HumanSex Sex { get; set; }
    
    public DateTime Issued { get; set; }

    public DateTime Deleted { get; set; }

    public DateTime Born { get; set; }

    public string Name { get; set; } = string.Empty;

    public string TrainerCode { get; set; } = string.Empty;

    public string FirstRideRaceKey { get; set; } = string.Empty;

    public string FirstRideRaceKeySteeplechase { get; set; } = string.Empty;

    public string FirstWinRaceKey { get; set; } = string.Empty;

    public string FirstWinRaceKeySteeplechase { get; set; } = string.Empty;

    public string From { get; set; } = string.Empty;

    internal Rider()
    {
    }

    public static Rider FromJV(JVData_Struct.JV_KS_KISYU rider)
    {
      short.TryParse(rider.TozaiCD, out var belongs);
      short.TryParse(rider.SexCD, out var sex);
      short.TryParse(rider.DelKubun, out var deleted);

      return new Rider
      {
        LastModified = rider.head.MakeDate.ToDateTime(),
        DataStatus = rider.head.DataKubun.ToDataStatus(),
        Code = rider.KisyuCode.Trim(),
        IsDeleted = deleted != 0,
        Belongs = (HorseBelongs)belongs,
        Sex = (HumanSex)sex,
        Issued = rider.IssueDate.ToDateTime(),
        Deleted = rider.DelDate.ToDateTime(),
        Born = rider.BirthDate.ToDateTime(),
        Name = rider.KisyuName.Trim(),
        TrainerCode = rider.ChokyosiCode.Trim(),
        FirstRideRaceKey = rider.HatuKiJyo[0].Hatukijyoid.ToRaceKey(),
        FirstRideRaceKeySteeplechase = rider.HatuKiJyo[1].Hatukijyoid.ToRaceKey(),
        FirstWinRaceKey = rider.HatuSyori[0].Hatukijyoid.ToRaceKey(),
        FirstWinRaceKeySteeplechase = rider.HatuSyori[1].Hatukijyoid.ToRaceKey(),
        From = rider.Syotai.Trim(),
      };
    }
  }

  public enum HumanSex : short
  {
    Unknown = 0,
    Male = 1,
    Female = 2,
  }
}
