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
  public class TrainerData : DataBase<Trainer>
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

    [StringLength(32)]
    public string From { get; set; } = string.Empty;

    public override void SetEntity(Trainer trainer)
    {
      LastModified = trainer.LastModified;
      DataStatus = trainer.DataStatus;
      IsCentral = trainer.IsCentral;
      Code = trainer.Code;
      IsDeleted = trainer.IsDeleted;
      Belongs = trainer.Belongs;
      Sex = trainer.Sex;
      Issued = trainer.Issued;
      Deleted = trainer.Deleted;
      Born = trainer.Born;
      Name = trainer.Name;
      From = trainer.From;
    }

    public override bool IsEquals(DataBase<Trainer> b)
    {
      return ((TrainerData)b).Code == this.Code && ((TrainerData)b).IsCentral == this.IsCentral;
    }

    public override int GetHashCode()
    {
      return (this.Code + this.IsCentral).GetHashCode();
    }
  }
}
