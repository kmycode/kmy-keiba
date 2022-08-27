using KmyKeiba.JVLink.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Data.Db
{
  public class RiderData : DataBase<Rider>
  {
    [StringLength(8)]
    public string Code { get; set; } = string.Empty;

    [NotMapped]
    public bool IsCentral
    {
      get => this.CentralFlag != 0;
      set => this.CentralFlag = value ? (short)1 : (short)0;
    }

    public short CentralFlag { get; set; }

    public bool IsDeleted { get; set; }

    public HorseBelongs Belongs { get; set; }

    public HumanSex Sex { get; set; }

    public DateTime Issued { get; set; }

    public DateTime Deleted { get; set; }

    public DateTime Born { get; set; }

    [StringLength(56)]
    public string Name { get; set; } = string.Empty;

    [StringLength(8)]
    public string TrainerCode { get; set; } = string.Empty;

    [StringLength(20)]
    public string FirstRideRaceKey { get; set; } = string.Empty;

    [StringLength(20)]
    public string FirstRideRaceKeySteeplechase { get; set; } = string.Empty;

    [StringLength(20)]
    public string FirstWinRaceKey { get; set; } = string.Empty;

    [StringLength(20)]
    public string FirstWinRaceKeySteeplechase { get; set; } = string.Empty;

    [StringLength(32)]
    public string From { get; set; } = string.Empty;

    public override void SetEntity(Rider rider)
    {
      LastModified = rider.LastModified;
      DataStatus = rider.DataStatus;
      IsCentral = rider.IsCentral;
      Code = rider.Code;
      IsDeleted = rider.IsDeleted;
      Belongs = rider.Belongs;
      Sex = rider.Sex;
      Issued = rider.Issued;
      Deleted = rider.Deleted;
      Born = rider.Born;
      Name = rider.Name;
      TrainerCode = rider.TrainerCode;
      FirstRideRaceKey = rider.FirstRideRaceKey;
      FirstRideRaceKeySteeplechase = rider.FirstRideRaceKeySteeplechase;
      FirstWinRaceKey = rider.FirstWinRaceKey;
      FirstWinRaceKeySteeplechase = rider.FirstWinRaceKeySteeplechase;
      From = rider.From;
    }

    public override bool IsEquals(DataBase<Rider> b)
    {
      return ((RiderData)b).Code == this.Code && ((RiderData)b).IsCentral == this.IsCentral;
    }

    public override int GetHashCode()
    {
      return (this.Code + this.IsCentral).GetHashCode();
    }
  }
}
